using Lucene.Net.Index;
using Lucene.Net.Queries;
using Lucene.Net.Search;
using Semantika.Marcell.LuceneStore.Index;
using Semantika.Marcell.LuceneStore.Indexer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Semantika.Marcell.LuceneStore.Query.Impl
{
    public enum EnsembleMemberScoring
    {
        Linear,
        Asymptotic,
        ScoreRelative,
        Constant
    }

    public struct EnsembleMemberParameters
    {
        public double DocumentTopicWeight { get; set; }
        public double SectionTopicWeight { get; set; }
        public double ParagraphTopicWeight { get; set; }
        public double SentenceTopicWeight { get; set; }
        public double DocumentSingleTermWeight { get; set; }
        public double SectionSingleTermWeight { get; set; }
        public double ParagraphSingleTermWeight { get; set; }
        public double SentenceIATEWeight { get; set; }
        public double SentenceEVWeight { get; set; }
        public double DocumentTopicLimit { get; set; }
        public double SectionTopicLimit { get; set; }
        public double ParagraphTopicLimit { get; set; }
        public double SentenceTopicLimit { get; set; }
        public double DocumentTokenLimit { get; set; }
        public double SectionTokenLimit { get; set; }
        public double ParagraphTokenLimit { get; set; }
        public double SentenceIATELimit { get; set; }
        public double SentenceEVLimit { get; set; }
        public double UseDocumentSimilarity { get; set; }
        public double UseSectionSimilarity { get; set; }
        public bool UseDocumentTokens { get; set; }
        public bool UseSectionTokens { get; set; }
        public bool UseParagraphTokens { get; set; }
        public bool UseSentenceTokens { get; set; }
        public bool UseDocumentTopics { get; set; }
        public bool UseSectionTopics { get; set; }
        public bool UseParagraphTopics { get; set; }
        public bool UseSentenceTopics { get; set; }
        public EnsembleMemberScoring ScoringStrategy { get; set; }
        public int StartPositionShift { get; set; }
    }

    public class OptimizedEnsembleSearch : Search<ParametrizedSearchQuery>
    {
        private struct EnsembleMember
        {
            public OptimizedParametrizedSearch Searcher { get; set; }
            public EnsembleMemberScoring ScoringStrategy { get; set; }
            public int StartPositionShift { get; set; }
        }

        private readonly EnsembleMember[] m_searchMembers;

        public OptimizedEnsembleSearch(IndexManager indexManager, IEnumerable<EnsembleMemberParameters> memberParameters) : base(indexManager)
        {
            if (memberParameters == null)
            {
                throw new ArgumentException("No parameters provided for ensemble members. Please provide at least one set of parameters.");
            }

            //Create ensemble members based on provided parameters for individual members
            m_searchMembers = new EnsembleMember[memberParameters.Count()];
            int i = 0;
            foreach (var currentParameters in memberParameters)
            {
                m_searchMembers[i] = new EnsembleMember
                {
                    Searcher = new OptimizedParametrizedSearch(indexManager,
                    currentParameters.DocumentTopicWeight,
                    currentParameters.SectionTopicWeight,
                    currentParameters.ParagraphTopicWeight,
                    currentParameters.SentenceTopicWeight,
                    currentParameters.DocumentSingleTermWeight,
                    currentParameters.SectionSingleTermWeight,
                    currentParameters.ParagraphSingleTermWeight,
                    currentParameters.SentenceIATEWeight,
                    currentParameters.SentenceEVWeight,
                    currentParameters.DocumentTopicLimit,
                    currentParameters.SectionTopicLimit,
                    currentParameters.ParagraphTopicLimit,
                    currentParameters.SentenceTopicLimit,
                    currentParameters.DocumentTokenLimit,
                    currentParameters.SectionTokenLimit,
                    currentParameters.ParagraphTokenLimit,
                    currentParameters.SentenceIATELimit,
                    currentParameters.SentenceEVLimit,
                    currentParameters.UseDocumentSimilarity,
                    currentParameters.UseSectionSimilarity,
                    currentParameters.UseDocumentTokens,
                    currentParameters.UseSectionTokens,
                    currentParameters.UseParagraphTokens,
                    currentParameters.UseSentenceTokens,
                    currentParameters.UseDocumentTopics,
                    currentParameters.UseSectionTopics,
                    currentParameters.UseParagraphTopics,
                    currentParameters.UseSentenceTopics
                    ),
                    ScoringStrategy = currentParameters.ScoringStrategy,
                    StartPositionShift = currentParameters.StartPositionShift
                };
                i++;
            }
        }

        protected virtual float CalculateResultScore(EnsembleMemberScoring scoringType, float score, float topScore, int resultIndex, int totalResults)
        {
            switch (scoringType)
            {
                case EnsembleMemberScoring.Constant:
                    return 1;

                case EnsembleMemberScoring.Linear:
                    return 1 - ((float)resultIndex) / totalResults;

                case EnsembleMemberScoring.Asymptotic:
                    return (4.0f) / (resultIndex + 4);

                case EnsembleMemberScoring.ScoreRelative:
                    return score / topScore;

                default:
                    throw new InvalidOperationException("Invalid scoring type selected.");
            }
        }

        public override Result<T> PerformSearch<T>(ParametrizedSearchQuery query, int pageSize = 10, int pageNumber = 1)
        {
            var searchType = GetObjectType(typeof(T));
            object lockMonitor = new object();
            IDictionary<int, float> resultTable = new ConcurrentDictionary<int, float>();
            //Get the results requested from each individual member, then combine them into a final score by using individual member strategies

            IndexSearcher searcher = null;
            Parallel.ForEach(m_searchMembers, ensembleMember =>
            {
                query.ShiftStartingPosition = ensembleMember.StartPositionShift;
                var mainQuery = ensembleMember.Searcher.CalculateLuceneQuery(query, ref searchType);
                lock (lockMonitor)
                {
                    if (searcher == null)
                    {
                        //We don't have the searcher yet - get one.
                        searcher = GetAppropriateSearcher(query.Language, searchType);
                    }
                }

                var memberResults = searcher.Search(mainQuery, new TermFilter(new Term("Language", query.Language)), (pageNumber + 1) * pageSize);

                Parallel.For(0, memberResults.ScoreDocs.Length, i =>
                {
                    var currentResult = memberResults.ScoreDocs[i];
                    if (resultTable.ContainsKey(currentResult.Doc))
                    {
                        resultTable[currentResult.Doc] = resultTable[currentResult.Doc] + CalculateResultScore(ensembleMember.ScoringStrategy, currentResult.Score, memberResults.MaxScore, i, memberResults.ScoreDocs.Length);
                    }
                    else
                    {
                        resultTable.Add(currentResult.Doc, CalculateResultScore(ensembleMember.ScoringStrategy, currentResult.Score, memberResults.MaxScore, i, memberResults.ScoreDocs.Length));
                    }
                });
            });

            var results = resultTable.OrderBy(t => t.Value).ThenBy(t => t.Key).Select(t => t.Key).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToArray();

            Result<T> result = new Result<T>
            {
                TotalResults = pageSize,
                ResultList = new List<T>(pageSize)
            };

            object addListMonitor = new object();
            Parallel.For(0, results.Length, i =>
            {
                T entity;
                lock (addListMonitor)
                {
                    entity = GetEntityHierarchy<T>(searcher.Doc(results[i]).ToEntity(GetTypeFromObjectType(searchType)));
                }
                result.ResultList.Add(entity);
            });

            return result;
        }

        public override Lucene.Net.Search.Query CalculateLuceneQuery(ParametrizedSearchQuery query, ref IndexObjectType searchType)
        {
            throw new InvalidOperationException("Unable to calculate a single query!");
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var member in m_searchMembers)
            {
                member.Searcher.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}