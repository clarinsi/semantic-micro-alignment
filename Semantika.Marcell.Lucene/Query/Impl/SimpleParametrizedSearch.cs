using Lucene.Net.Queries;
using Lucene.Net.Search;
using Semantika.Marcell.LuceneStore.Index;
using Semantika.Marcell.LuceneStore.Indexer;
using System;
using System.Collections.Generic;
using LuceneNet = Lucene.Net;

namespace Semantika.Marcell.LuceneStore.Query.Impl
{
    public class SimpleParametrizedSearch : WeightedParametrizedSearch<ParametrizedSearchQuery>
    {
        protected double m_documentTopicLimit;
        protected double m_sectionTopicLimit;
        protected double m_paragraphTopicLimit;
        protected double m_sentenceTopicLimit;

        protected double m_documentTokenLimit;
        protected double m_sectionTokenLimit;
        protected double m_paragraphTokenLimit;

        protected double m_sentenceIATELimit;
        protected double m_sentenceEVLimit;

        protected bool m_useDocumentSimilarity;
        protected bool m_useSectionSimilarity;

        protected bool m_useDocumentTokens;
        protected bool m_useSectionTokens;
        protected bool m_useParagraphTokens;
        protected bool m_useSentenceTokens;

        protected bool m_useDocumentTopics;
        protected bool m_useSectionTopics;
        protected bool m_useParagraphTopics;
        protected bool m_useSentenceTopics;

        public SimpleParametrizedSearch(IndexManager indexManager,
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
            bool useDocumentSimilarity,
            bool useSectionSimilarity,
            bool useDocumentTokens,
            bool useSectionTokens,
            bool useParagraphTokens,
            bool useSentenceTokens,
            bool useDocumentTopics,
            bool useSectionTopics,
            bool useParagraphTopics,
            bool useSentenceTopics
            ) :
            base(indexManager, documentTopicWeight, sectionTopicWeight, paragraphTopicWeight, sentenceTopicWeight, documentSingleTermWeight, sectionSingleTermWeight, paragraphSingleTermWeight, 0, sentenceIATEWeight, sentenceEVWeight)
        {
            m_documentTopicLimit = documentTopicLimit;
            m_sectionTopicLimit = sectionTopicLimit;
            m_paragraphTopicLimit = paragraphTopicLimit;
            m_sentenceTopicLimit = sentenceTopicLimit;

            m_documentTokenLimit = documentTokenLimit;
            m_sectionTokenLimit = sectionTokenLimit;
            m_paragraphTokenLimit = paragraphTokenLimit;
            m_sentenceIATELimit = sentenceIATELimit;
            m_sentenceEVLimit = sentenceEVLimit;

            m_useDocumentSimilarity = useDocumentSimilarity;
            m_useSectionSimilarity = useSectionSimilarity;

            m_useDocumentTokens = useDocumentTokens;
            m_useSectionTokens = useSectionTokens;
            m_useParagraphTokens = useParagraphTokens;
            m_useSentenceTokens = useSentenceTokens;

            m_useDocumentTopics = useDocumentTopics;
            m_useSectionTopics = useSectionTopics;
            m_useParagraphTopics = useParagraphTopics;
            m_useSentenceTopics = useSentenceTopics;
        }

        public override LuceneNet.Search.Query CalculateLuceneQuery(ParametrizedSearchQuery query, ref IndexObjectType searchType)
        {
            BooleanQuery.MaxClauseCount = 50000;
            BooleanQuery mainQuery = new BooleanQuery();

            if ((query.SearchIn & IndexObjectType.DocumentIndex) == IndexObjectType.DocumentIndex && m_useDocumentSimilarity)
            {
                if (m_useDocumentTokens)
                {
                    BooleanQuery documentTokens = new BooleanQuery();
                    foreach (var term in query.DocumentTokens)
                    {
                        if (!string.IsNullOrWhiteSpace(term))
                        {
                            var docQuery = GetWeightedTermQuery("DocumentToken", term, 1);
                            //Add topic to compare
                            if (docQuery != null)
                            {
                                documentTokens.Add(docQuery, LuceneNet.Search.Occur.SHOULD);
                            }
                        }
                    }
                    documentTokens.MinimumNumberShouldMatch = (int)(Math.Max(documentTokens.Clauses.Count * m_documentTokenLimit, 1));
                    documentTokens.Boost = (float)m_documentSingleTermWeight;
                    if (documentTokens.Clauses.Count > 0)
                    {
                        mainQuery.Add(documentTokens, Occur.MUST);
                    }
                }

                if (m_useDocumentTopics)
                {
                    BooleanQuery documentTopics = new BooleanQuery();
                    foreach (var term in query.DocumentTopics)
                    {
                        if (!string.IsNullOrWhiteSpace(term))
                        {
                            var docQuery = GetWeightedTermQuery("DocumentTopic", term, 1);
                            //Add topic to compare
                            if (docQuery != null)
                            {
                                documentTopics.Add(docQuery, LuceneNet.Search.Occur.SHOULD);
                            }
                        }
                    }
                    documentTopics.MinimumNumberShouldMatch = (int)(Math.Max(documentTopics.Clauses.Count * m_documentTopicLimit, 1));
                    documentTopics.Boost = (float)m_documentTopicWeight;

                    if (documentTopics.Clauses.Count > 0)
                    {
                        mainQuery.Add(documentTopics, Occur.MUST);
                    }
                }
            }

            if ((query.SearchIn & IndexObjectType.SectionIndex) == IndexObjectType.SectionIndex && m_useSectionSimilarity)
            {
                if (searchType < IndexObjectType.SectionIndex)
                {
                    searchType = IndexObjectType.SectionIndex;
                }
                if (m_useSectionTokens)
                {
                    BooleanQuery pSectionTokens = new BooleanQuery();
                    foreach (var term in query.SectionTokens)
                    {
                        if (!string.IsNullOrWhiteSpace(term))
                        {
                            var docQuery = GetWeightedTermQuery("SectionToken", term, 1);
                            //Add topic to compare
                            if (docQuery != null)
                            {
                                pSectionTokens.Add(docQuery, LuceneNet.Search.Occur.SHOULD);
                            }
                        }
                    }
                    pSectionTokens.MinimumNumberShouldMatch = (int)(Math.Max(pSectionTokens.Clauses.Count * m_sectionTokenLimit, 1));
                    pSectionTokens.Boost = (float)m_sectionSingleTermWeight;

                    if (pSectionTokens.Clauses.Count > 0)
                    {
                        mainQuery.Add(pSectionTokens, Occur.MUST);
                    }
                }

                if (m_useSectionTopics)
                {
                    BooleanQuery pSectionTopic = new BooleanQuery();
                    foreach (var term in query.SectionTopics)
                    {
                        if (!string.IsNullOrWhiteSpace(term))
                        {
                            var docQuery = GetWeightedTermQuery("SectionTopic", term, 1);
                            //Add topic to compare
                            if (docQuery != null)
                            {
                                pSectionTopic.Add(docQuery, LuceneNet.Search.Occur.SHOULD);
                            }
                        }
                    }
                    pSectionTopic.MinimumNumberShouldMatch = (int)(Math.Max(pSectionTopic.Clauses.Count * m_sectionTopicLimit, 1));
                    pSectionTopic.Boost = (float)m_sectionTopicWeight;
                    if (pSectionTopic.Clauses.Count > 0)
                    {
                        mainQuery.Add(pSectionTopic, Occur.MUST);
                    }
                }
            }

            if ((query.SearchIn & IndexObjectType.ParagraphIndex) == IndexObjectType.ParagraphIndex)
            {
                if (searchType < IndexObjectType.ParagraphIndex)
                {
                    searchType = IndexObjectType.ParagraphIndex;
                }
                if (m_useParagraphTokens)
                {
                    BooleanQuery pTokenQuery = new BooleanQuery();
                    foreach (var term in query.ParagraphTokens)
                    {
                        if (!string.IsNullOrWhiteSpace(term))
                        {
                            pTokenQuery.Add(GetWeightedTermQuery("ParagraphToken", term, 1), Occur.SHOULD);
                        }
                    }
                    pTokenQuery.MinimumNumberShouldMatch = (int)(Math.Max(pTokenQuery.Clauses.Count * m_paragraphTokenLimit, 1));
                    pTokenQuery.Boost = (float)m_paragraphSingleTermWeight;
                    mainQuery.Add(pTokenQuery, Occur.MUST);
                }

                if (m_useParagraphTopics)
                {
                    BooleanQuery pParagraphTopic = new BooleanQuery();
                    foreach (var term in query.ParagraphTopics)
                    {
                        if (!string.IsNullOrWhiteSpace(term))
                        {
                            var docQuery = GetWeightedTermQuery("ParagraphTopic", term, 1);
                            //Add topic to compare
                            if (docQuery != null)
                            {
                                pParagraphTopic.Add(docQuery, LuceneNet.Search.Occur.SHOULD);
                            }
                        }
                    }
                    pParagraphTopic.MinimumNumberShouldMatch = (int)(Math.Max(pParagraphTopic.Clauses.Count * m_paragraphTopicLimit, 1));
                    pParagraphTopic.Boost = (float)m_paragraphTopicWeight;
                    mainQuery.Add(pParagraphTopic, Occur.MUST);
                }
            }

            if ((query.SearchIn & IndexObjectType.SentenceIndex) == IndexObjectType.SentenceIndex)
            {
                if (searchType < IndexObjectType.SentenceIndex)
                {
                    searchType = IndexObjectType.SentenceIndex;
                }

                if (m_useSentenceTokens)
                {
                    //If we are searching within the sentence index, use the uncosolidated IATE and EuroVoc tokens & consolidated Sentence Topics
                    BooleanQuery pIateTokenQuery = new BooleanQuery();
                    BooleanQuery pEVTokenQuery = new BooleanQuery();
                    foreach (var term in query.SentenceTokens)
                    {
                        if (!string.IsNullOrWhiteSpace(term))
                        {
                            if (term.StartsWith("IATE"))
                            {
                                pIateTokenQuery.Add(GetWeightedTermQuery("ContainedTokenIATE", term, 1), Occur.SHOULD);
                            }
                            else
                            {
                                pEVTokenQuery.Add(GetWeightedTermQuery("ContainedTokenEV", term, 1), Occur.SHOULD);
                            }
                        }
                    }

                    BooleanQuery pSentenceTokenQuery = new BooleanQuery();
                    if (pIateTokenQuery.Clauses.Count > 0)
                    {
                        pIateTokenQuery.MinimumNumberShouldMatch = (int)(Math.Max(pIateTokenQuery.Clauses.Count * m_sentenceIATELimit, 0));
                        pIateTokenQuery.Boost = (float)m_sentenceIATEWeight;
                        pSentenceTokenQuery.Add(pIateTokenQuery, Occur.SHOULD);
                    }

                    if (pEVTokenQuery.Clauses.Count > 0)
                    {
                        pEVTokenQuery.MinimumNumberShouldMatch = (int)(Math.Max(pEVTokenQuery.Clauses.Count * m_sentenceEVLimit, 0));
                        pEVTokenQuery.Boost = (float)m_sentenceEuroVocWeight;
                        pSentenceTokenQuery.Add(pEVTokenQuery, Occur.SHOULD);
                    }

                    if (pSentenceTokenQuery.Clauses.Count > 0)
                    {
                        mainQuery.Add(pSentenceTokenQuery, Occur.MUST);
                    }
                }

                if (m_useSentenceTopics)
                {
                    BooleanQuery pSentenceTopics = new BooleanQuery();
                    foreach (var term in query.SentenceTopics)
                    {
                        var senTopic = GetWeightedTermQuery("SentenceTopic", term, 1);
                        if (senTopic != null)
                        {
                            pSentenceTopics.Add(senTopic, LuceneNet.Search.Occur.SHOULD);
                        }
                    }
                    if (pSentenceTopics.Clauses.Count > 0)
                    {
                        pSentenceTopics.MinimumNumberShouldMatch = (int)(Math.Max(pSentenceTopics.Clauses.Count * m_sentenceTopicLimit, 1));
                        pSentenceTopics.Boost = (float)m_sentenceTopicWeight;
                        mainQuery.Add(pSentenceTopics, Occur.MUST);
                    }
                }
            }

            return mainQuery;
        }

        public override Result<T> PerformSearch<T>(ParametrizedSearchQuery query, int pageSize = 10, int pageNumber = 1)
        {
            var searchType = GetObjectType(typeof(T));
            var mainQuery = CalculateLuceneQuery(query, ref searchType);

            var searcher = GetAppropriateSearcher(query.Language, searchType);
            var results = searcher.Search(mainQuery, new TermFilter(new LuceneNet.Index.Term("Language", query.Language)), pageNumber * pageSize);
            Result<T> result = new Result<T>
            {
                TotalResults = results.TotalHits,
                ResultList = new List<T>(pageSize)
            };

            for (int i = (pageNumber - 1) * pageSize; i < Math.Min(pageNumber * pageSize, results.TotalHits); i++)
            {
                result.ResultList.Add(GetEntityHierarchy<T>(searcher.Doc(results.ScoreDocs[i].Doc).ToEntity(GetTypeFromObjectType(searchType))));
            }

            return result;
        }
    }
}