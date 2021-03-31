using Semantika.Marcell.Data;
using Semantika.Marcell.LuceneStore.Index;
using Semantika.Marcell.LuceneStore.Query;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace WeightOptimizer.Genetics
{
    public abstract class Candidate<TQueryType, TEntityType> : IDisposable where TQueryType : ILanguageQuery
                                                                           where TEntityType : IMarcellEntity
    {
        protected IndexManager m_indexManager;
        protected Search<TQueryType> m_searcher;

        private static readonly RandomNumberGenerator m_randomGenerator = RandomNumberGenerator.Create();

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

        public Candidate(IndexManager indexManager)
        {
            m_indexManager = indexManager;

            for (int i = 0; i < WeightCount; i++)
            {
                SetWeight(i, GetRandomWeight());
            }
        }

        public Candidate(IndexManager indexManager, double[] weights) : this(indexManager)
        {
            if (weights.Length != WeightCount)
            {
                throw new InvalidOperationException("Invalid number of weights provided!");
            }

            for (int i = 0; i < WeightCount; i++)
            {
                SetWeight(i, weights[i]);
            }
        }

        public Candidate(Candidate<TQueryType, TEntityType> parent1, Candidate<TQueryType, TEntityType> parent2) : this(parent1.m_indexManager)
        {
            for (int i = 0; i < parent1.WeightCount; i++)
            {
                SetWeight(i, CombineWeights(parent1.GetWeight(i), parent2.GetWeight(i), parent1.Fitnes, parent2.Fitnes));
            }
        }

        public struct LearningSet
        {
            public TEntityType Entity { get; set; }
            public string Language { get; set; }
        }

        protected virtual double CombineWeights(double weight1, double weight2, double fitnes1, double fitnes2)
        {
            int combineUsing = (int)(GetRandomWeight() * 4);
            switch (combineUsing)
            {
                case 0:
                    return (fitnes1 < fitnes2) ? weight1 : weight2;

                case 1:
                    double w1 = fitnes2 / (fitnes1 + fitnes2);
                    double w2 = fitnes1 / (fitnes1 + fitnes2);
                    return w1 * weight1 + w2 * weight2;

                case 2:
                    return (weight1 + weight2) / 2;

                case 3:
                    return (GetRandomWeight() < 0.5) ? weight1 : weight2;

                default:
                    throw new ArgumentException("A bug in the cross-over method!");
            }
        }

        private int m_fitnes = 0;

        public int Fitnes
        {
            get
            {
                return m_fitnes;
            }

            protected set
            {
                m_fitnes = value;
            }
        }

        public double TestResult { get; protected set; } = 0.0;

        public void Reset()
        {
            Fitnes = 0;
        }

        public double Evaluate(List<LearningSet> trainingSet)
        {
            double finalScore = 0;

            foreach (var set in trainingSet)
            {
                PerformEvaluation(ref finalScore, set);
            }

            TestResult = finalScore / trainingSet.Count;
            return TestResult;
        }

        public void Train(List<LearningSet> trainingSet)
        {
            int valFitnes = (int)Evaluate(trainingSet);
            Interlocked.Add(ref m_fitnes, valFitnes);
        }

        protected abstract void PerformEvaluation(ref double finalScore, LearningSet set);

        public abstract void Mutate();

        public abstract double GetWeight(int weightIndex);

        public abstract string GetWeightName(int weightIndex);

        public abstract void SetWeight(int weightIndex, double weightValue);

        public abstract int WeightCount { get; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (TestResult > 0)
            {
                sb.Append("TestResult=").Append(TestResult).Append(";");
            }

            for (int i = 0; i < WeightCount; i++)
            {
                sb.Append(GetWeightName(i)).Append("=").Append(GetWeight(i)).Append(";");
            }
            return sb.ToString();
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (m_searcher != null)
                    {
                        m_searcher.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        //~Candidate()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}