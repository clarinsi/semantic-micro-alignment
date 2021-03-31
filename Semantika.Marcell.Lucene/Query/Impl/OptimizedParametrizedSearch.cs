using Semantika.Marcell.LuceneStore.Index;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Semantika.Marcell.LuceneStore.Query.Impl
{
    public class OptimizedParametrizedSearch : SimpleParametrizedSearch
    {
        private double m_useDocumentSimilarityPercentage;
        private double m_useSectionSimilarityPercentage;

        private const int m_maxUseTerms = 1000;
        private const int m_minUseTerms = 25;

        public OptimizedParametrizedSearch(IndexManager indexManager,
            double documentTopicWeight,
            double sectionTopicWeight,
            double paragraphTopicWeight,
            double sentenceTopicWeight,
            double documentSingleTermWeight,
            double sectionSingleTermWeight,
            double paragraphSingleTermWeight,
            double sentenceIATEWeight,
            double sentenceEVWeight,
            double documentTopicLimit,
            double sectionTopicLimit,
            double paragraphTopicLimit,
            double sentenceTopicLimit,
            double documentTokenLimit,
            double sectionTokenLimit,
            double paragraphTokenLimit,
            double sentenceIATELimit,
            double sentenceEVLimit,
            double useDocumentSimilarity,
            double useSectionSimilarity,
            bool useDocumentTokens,
            bool useSectionTokens,
            bool useParagraphTokens,
            bool useSentenceTokens,
            bool useDocumentTopics,
            bool useSectionTopics,
            bool useParagraphTopics,
            bool useSentenceTopics
            ) :
            base(indexManager, documentTopicWeight, sectionTopicWeight, paragraphTopicWeight,
                sentenceTopicWeight, documentSingleTermWeight, sectionSingleTermWeight,
                paragraphSingleTermWeight, sentenceIATEWeight, sentenceEVWeight,
                documentTopicLimit, sectionTopicLimit, paragraphTopicLimit,
                sentenceTopicLimit, documentTokenLimit, sectionTokenLimit,
                paragraphTokenLimit, sentenceIATELimit, sentenceEVLimit,
                true, true,
                useDocumentTokens, useSectionTokens, useParagraphTokens, useSentenceTokens,
                useDocumentTopics, useSectionTopics, useParagraphTopics, useSentenceTopics)
        {
            m_useDocumentSimilarityPercentage = useDocumentSimilarity;
            m_useSectionSimilarityPercentage = useSectionSimilarity;
        }

        private int GetTermCount(double percentage, int termCount)
        {
            int result = (int)Math.Min(m_maxUseTerms, percentage * termCount);
            result = Math.Max(result, m_minUseTerms);
            return Math.Min(result, termCount);
        }

        private static string[] emptyStringList = new string[0];

        private string[] GetFilteredList(string[] source, int requestedItems, int shiftStartingPosition)
        {
            if (requestedItems <= 0)
            {
                return emptyStringList;
            }

            List<string> result = new List<string>(requestedItems);
            int step = (int)Math.Max(1, source.Length / requestedItems);

            for (int i = shiftStartingPosition; i < source.Length; i += step)
            {
                result.Add(source[i]);
            }

            return result.ToArray();
        }

        public override Result<T> PerformSearch<T>(ParametrizedSearchQuery query, int pageSize = 10, int pageNumber = 1)
        {
            //First remove tokens already contained in lower entities from higher entities
            ParametrizedSearchQuery processedQuery = new ParametrizedSearchQuery()
            {
                DocumentTokens = query.DocumentTokens.Where(t => !query.ParagraphTokens.Contains(t) && !query.SectionTokens.Contains(t)).ToArray(),
                DocumentTopics = query.DocumentTopics.Where(t => !query.ParagraphTopics.Contains(t) && !query.SectionTopics.Contains(t)).ToArray(),
                Language = query.Language,
                SectionTokens = query.SectionTokens.Where(t => !query.ParagraphTokens.Contains(t)).ToArray(),
                SectionTopics = query.SectionTopics.Where(t => !query.ParagraphTopics.Contains(t)).ToArray(),
                ParagraphTokens = query.ParagraphTokens,
                ParagraphTopics = query.ParagraphTopics,
                SentenceTokens = query.SentenceTokens,
                SentenceTopics = query.SentenceTopics,
                SearchIn = query.SearchIn
            };

            //Optimize the query by only taking a number of section & document tokens
            int docTerms = GetTermCount(m_useDocumentSimilarityPercentage, processedQuery.DocumentTokens.Length);
            int sectionTerms = GetTermCount(m_useSectionSimilarityPercentage, processedQuery.SectionTokens.Length);
            int docTopics = GetTermCount(m_useDocumentSimilarityPercentage, processedQuery.DocumentTopics.Length);
            int sectionTopics = GetTermCount(m_useSectionSimilarityPercentage, processedQuery.SectionTopics.Length);

            processedQuery.DocumentTokens = GetFilteredList(processedQuery.DocumentTokens, docTerms, query.ShiftStartingPosition);
            processedQuery.SectionTokens = GetFilteredList(processedQuery.SectionTokens, sectionTerms, query.ShiftStartingPosition);
            processedQuery.DocumentTopics = GetFilteredList(processedQuery.DocumentTopics, docTopics, query.ShiftStartingPosition);
            processedQuery.SectionTopics = GetFilteredList(processedQuery.SectionTopics, sectionTopics, query.ShiftStartingPosition);

            //Search using the updated query
            return base.PerformSearch<T>(processedQuery, pageSize, pageNumber);
        }
    }
}