using Semantika.Marcell.Data;
using Semantika.Marcell.LuceneStore.Index;
using Semantika.Marcell.LuceneStore.Query.Impl;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeightOptimizer.Genetics;
using WeightOptimizer.Genetics.Candidates;

namespace WeightOptimizer
{
    public class Program
    {
        private static void Main(string[] args)
        {
            if (!(args.Length == 3) || !int.TryParse(args[0], out _))
            {
                Console.WriteLine("Usage: WeigthOptimizer <indexMode> <indexPath> <outputPath>");
                Console.WriteLine("Source path must contain one folder per language (sl, hr, ...) with the Xml CoNLL-UP files.");
                Console.WriteLine("Index mode can be: 0=single index file per type, 1=index file per language");

                return;
            }

            var langArray = GeneticOptimizer<ParametrizedEnsembleCandidate<Paragraph>, ParametrizedSearchQuery, Paragraph>.SupportedLanguages;

            List<Task> tasks = new List<Task>();
            for (int srcloop = 0; srcloop < langArray.Count; srcloop++)
            {
                IndexManager indexManager = new IndexManager((IndexingMode)int.Parse(args[0]), args[1], true);
                for (int tgtloop = 0; tgtloop < langArray.Count; tgtloop++)
                {
                    if (srcloop != tgtloop)
                    {
                        int src = srcloop;
                        int tgt = tgtloop;
                        IndexManager manager = indexManager;
                        tasks.Add(Task.Factory.StartNew(() =>
                        {
                            Console.WriteLine("[{0}]: Startingh alignement for language pair: {1}, {2}.", DateTime.Now, src, tgt);
                            string logPathSentence = @$"{args[2]}\_{langArray[src]}_{langArray[tgt]}";
                            string logPathParagraph = @$"{args[2]}\{langArray[src]}_{langArray[tgt]}";
                            System.IO.Directory.CreateDirectory(logPathSentence);
                            System.IO.Directory.CreateDirectory(logPathParagraph);
                            var go1 = new GeneticOptimizer<ParametrizedOptimizingAlignerCandidate<Paragraph>, ParametrizedSearchQuery, Paragraph>(manager, trainingSetReseedFrequency: 1, sourceLanguageIndex: src, targetLanguageIndex: tgt);
                            var go2 = new GeneticOptimizer<ParametrizedOptimizingAlignerCandidate<Sentence>, ParametrizedSearchQuery, Sentence>(manager, trainingSetReseedFrequency: 1, sourceLanguageIndex: src, targetLanguageIndex: tgt);
                            for (int i = 0; i < 8; i++)
                            {
                                Console.WriteLine("[{0}]: Starting iteration {1} for paragraph alignement for language pair: {2}, {3}.", DateTime.Now, i, langArray[src], langArray[tgt]);

                                go1.AdvanceGeneration();
                                go1.SaveState(logPathParagraph);

                                Console.WriteLine("[{0}]: Starting iteration {1} for sentence alignement for language pair: {2}, {3}.", DateTime.Now, i, langArray[src], langArray[tgt]);

                                go2.AdvanceGeneration();
                                go2.SaveState(logPathSentence);

                                Console.WriteLine("[{0}]: Completed iteration {1} for for language pair: {2}, {3}.", DateTime.Now, i, langArray[src], langArray[tgt]);
                            }
                        }, TaskCreationOptions.LongRunning));
                    }
                }
            }

            foreach (var task in tasks)
            {
                task.Wait();
            }
        }
    }
}