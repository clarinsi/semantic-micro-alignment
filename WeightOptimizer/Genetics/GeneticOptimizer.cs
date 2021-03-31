using Semantika.Marcell.Data;
using Semantika.Marcell.LuceneStore.Index;
using Semantika.Marcell.LuceneStore.Query;
using Semantika.Marcell.LuceneStore.Query.Impl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WeightOptimizer.Genetics
{
    public delegate Candidate<TQueryType, TEntityType>.LearningSet[] DataSetGenerator<TQueryType, TEntityType>(int membersToGenerate) where TEntityType : class, IMarcellEntity
                                                                           where TQueryType : ILanguageQuery;

    public class GeneticOptimizer<TCandidateType, TQueryType, TEntityType> where TCandidateType : Candidate<TQueryType, TEntityType>
                                                                           where TEntityType : class, IMarcellEntity
                                                                           where TQueryType : ILanguageQuery
    {
        private List<TCandidateType> m_currentGeneration;
        private List<Candidate<TQueryType, TEntityType>.LearningSet> m_trainingSet;
        private List<Candidate<TQueryType, TEntityType>.LearningSet> m_testingSet;

        private readonly int m_generationSize;
        private readonly double m_mutationRate;
        private readonly double m_survivalRate;
        private readonly int m_trainingSetSize;
        private readonly int m_trainingSetReseedFrequency;
        private readonly IndexManager m_indexManager;
        private int m_currentGenerationCount = 0;
        private readonly Guid m_runGuid = Guid.NewGuid();
        private readonly int? m_sourceLanguage = null;
        private readonly int? m_targetLangauge = null;

        private static readonly List<string> m_supportedLanguage = new List<string> { "bg", "hr", "hu", "pl", "ro", "sk", "sl" };
        private static readonly RandomNumberGenerator m_randomGenerator = RandomNumberGenerator.Create();

        public int GenerationSize { get => m_generationSize; }
        public double MutationRate { get => m_mutationRate; }
        public double SurvivalRate { get => m_survivalRate; }
        public int TrainingSetSize { get => m_trainingSetSize; }
        public int TrainingSetReseedFrequency { get => m_trainingSetReseedFrequency; }
        public int CurrentGenerationCount { get => m_currentGenerationCount; }

        public static List<string> SupportedLanguages
        {
            get
            {
                return m_supportedLanguage;
            }
        }

        protected static double GetRandomWeight()
        {
            double factor;
            do
            {
                byte[] rndBytes = new byte[4];
                m_randomGenerator.GetBytes(rndBytes);
                double dblValue = BitConverter.ToUInt32(rndBytes, 0);
                factor = dblValue / uint.MaxValue;
            } while (factor == 1);

            return factor;
        }

        private Candidate<TQueryType, TEntityType>.LearningSet[] GenerateSample(int count)
        {
            using (SimpleTextSearch searcher = new SimpleTextSearch(m_indexManager))
            {
                Candidate<TQueryType, TEntityType>.LearningSet[] setArray = new Candidate<TQueryType, TEntityType>.LearningSet[count];
                Parallel.For(0, count, i =>
                 {
                     int sourceLangIdx;
                     if (m_sourceLanguage == null)
                     {
                         sourceLangIdx = (int)(GetRandomWeight() * 7);
                     }
                     else
                     {
                         sourceLangIdx = m_sourceLanguage.Value;
                     }

                     int targetLangIdx;
                     if (m_targetLangauge == null)
                     {
                         do
                         {
                             targetLangIdx = (int)(GetRandomWeight() * 7);
                         } while (targetLangIdx == sourceLangIdx);
                     }
                     else
                     {
                         targetLangIdx = m_targetLangauge.Value;
                     }

                     setArray[i] = new Candidate<TQueryType, TEntityType>.LearningSet
                     {
                         Language = m_supportedLanguage[targetLangIdx],
                         Entity = searcher.GetRandomEntity<TEntityType>(m_supportedLanguage[sourceLangIdx])
                     };
                 });

                return setArray;
            }
        }

        private void InitTrainingSet()
        {
            Console.WriteLine($"[{DateTime.Now}]: Starting training set initialization.");

            m_trainingSet = GenerateSample(m_trainingSetSize).ToList();

            if (m_testingSet == null)
            {
                Console.WriteLine($"[{DateTime.Now}]: Creating a test set.");

                m_testingSet = GenerateSample(100).ToList();
            }

            Console.WriteLine($"[{DateTime.Now}]: Finished training set initialization.");
        }

        private void InitGeneration()
        {
            TCandidateType[] setCandidates = new TCandidateType[m_generationSize];
            Parallel.For(0, m_generationSize, i =>
             {
                 setCandidates[i] = Activator.CreateInstance(typeof(TCandidateType), new object[] { m_indexManager }) as TCandidateType;
             });

            m_currentGeneration = setCandidates.ToList();
        }

        public GeneticOptimizer(IndexManager indexManager, int generationSize = 50, double mutationRate = 0.05, double survivalRate = 0.2, int trainingSetSize = 50, int trainingSetReseedFrequency = 2, int? sourceLanguageIndex = null, int? targetLanguageIndex = null)
        {
            m_indexManager = indexManager;
            m_generationSize = generationSize;
            m_mutationRate = mutationRate;
            m_trainingSetSize = trainingSetSize;
            m_survivalRate = survivalRate;
            m_trainingSetReseedFrequency = trainingSetReseedFrequency;
            m_sourceLanguage = sourceLanguageIndex;
            m_targetLangauge = targetLanguageIndex;

            InitTrainingSet();
            InitGeneration();
        }

        private void Evaluate()
        {
            Console.WriteLine($"[{DateTime.Now}]: Starting evaluation.");
            int counter = 0;
            Parallel.ForEach(m_currentGeneration, cand =>
          {
              cand.Reset();
              cand.Train(m_trainingSet);
              Interlocked.Increment(ref counter);
              Console.WriteLine($"[{DateTime.Now}] Finished evaluating {counter} examples from generation. Last member score: {cand.Fitnes}");
          });
        }

        private TCandidateType GetOffspring(TCandidateType parent1, TCandidateType parent2)
        {
            return Activator.CreateInstance(typeof(TCandidateType), new object[] { parent1, parent2 }) as TCandidateType;
        }

        private void Cross()
        {
            TCandidateType[] m_nextGeneration = new TCandidateType[m_generationSize];
            int maxParentIndex = (int)(Math.Round(m_survivalRate * m_generationSize)) + 1;
            var parents = m_currentGeneration.OrderBy(cand => cand.Fitnes).Take(maxParentIndex + 1).ToList();
            Parallel.For(0, maxParentIndex + 1, i =>
              {
                  m_nextGeneration[i] = parents[i];
              });

            Parallel.For(maxParentIndex + 1, m_generationSize, i =>
            {
                m_currentGeneration[i].Dispose();
                m_nextGeneration[i] = GetOffspring(parents[(int)(GetRandomWeight() * maxParentIndex)], parents[(int)(GetRandomWeight() * maxParentIndex)]);
            });

            m_currentGeneration = m_nextGeneration.ToList();
        }

        private void Mutate()
        {
            Parallel.ForEach(m_currentGeneration, p =>
            {
                if (GetRandomWeight() < m_mutationRate)
                {
                    p.Mutate();
                }
            });
        }

        private void TestTop()
        {
            Console.WriteLine($"[{DateTime.Now}] Starting testing.");
            int counter = 0;
            Parallel.For(0, 5, i =>
            {
                var cand = m_currentGeneration[i];
                cand.Reset();
                cand.Evaluate(m_testingSet);
                Interlocked.Increment(ref counter);
            });
        }

        public void AdvanceGeneration()
        {
            m_currentGenerationCount++;
            Evaluate();
            Console.WriteLine("[{0}]: Done evaluation.", DateTime.Now);
            Cross();
            Console.WriteLine("[{0}]: Done cross-over.", DateTime.Now);
            Mutate();
            Console.WriteLine("[{0}]: Done mutation.", DateTime.Now);
            TestTop();
            Console.WriteLine("[{0}]: Done testing.", DateTime.Now);

            if (m_currentGenerationCount % m_trainingSetReseedFrequency == 0)
            {
                InitTrainingSet();
            }
        }

        public void SaveState(string directory)
        {
            string path = Path.Combine(directory, m_runGuid.ToString());
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filePath = Path.Combine(path, $"Run-{m_currentGenerationCount}.txt");
            StringBuilder fileContent = new StringBuilder();

            foreach (var member in m_currentGeneration)
            {
                fileContent.AppendLine($"Fitnes={member.Fitnes}: {member}");
            }
            fileContent.AppendLine("======");
            fileContent.AppendLine($"Average fitnes: {m_currentGeneration.Where(g => g.Fitnes != 0).Select(g => g.Fitnes).Average()}");
            fileContent.AppendLine($"Average test fitnes: {m_currentGeneration.Where(g => g.TestResult != 0).Select(g => g.TestResult).Average()}");
            File.WriteAllText(filePath, fileContent.ToString());
        }
    }
}