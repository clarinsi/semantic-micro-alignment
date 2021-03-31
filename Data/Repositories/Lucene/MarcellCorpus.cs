using LegalBrowser.Config;
using Semantika.Marcell.Data;
using Semantika.Marcell.LuceneStore.Index;
using Semantika.Marcell.LuceneStore.Query;
using Semantika.Marcell.LuceneStore.Query.Impl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LegalBrowser.Data.Repositories.Lucene
{
    public class MarcellCorpus
    {
        public readonly IndexManager indexManager;
        private static readonly object indexInitLock = new object();

        private static readonly Dictionary<ParametrizedSearchParameters, OptimizedParametrizedSearch> m_searcherCache = new Dictionary<ParametrizedSearchParameters, OptimizedParametrizedSearch>();
        private static readonly ParametrizedSearchParameters defaultSearchParameters = new ParametrizedSearchParameters
        {
            DocumentTopicWeight = 0,
            SectionTopicWeight = 0.338,
            ParagraphTopicWeight = 0,
            SentenceTopicWeight = 0,
            DocumentSingleTermWeight = 0,
            SectionSingleTermWeight = 0,
            ParagraphSingleTermWeight = 1,
            SentenceIATEWeight = 0,
            SentenceEVWeight = 0,
            DocumentTopicLimit = 0,
            SectionTopicLimit = 0.351,
            ParagraphTopicLimit = 0,
            SentenceTopicLimit = 0,
            DocumentTokenLimit = 0,
            SectionTokenLimit = 0,
            ParagraphTokenLimit = 0.273,
            SentenceEVLimit = 0,
            SentenceIATELimit = 0,
            UseDocumentSimilarity = 0,
            UseSectionSimilarity = 1,
            UseDocumentTokens = false,
            UseSectionTokens = false,
            UseParagraphTokens = true,
            UseSentenceTokens = false,
            UseDocumentTopics = false,
            UseSectionTopics = true,
            UseParagraphTopics = false,
            UseSentenceTopics = false,

        };

        public MarcellCorpus(IndexConfiguration indexConfiguration) :
            this(indexConfiguration.IndexingMode, indexConfiguration.IndexPath)
        {

        }

        private OptimizedParametrizedSearch GetSearcher(ParametrizedSearchParameters parameters)
        {
            return new OptimizedParametrizedSearch(indexManager,
                parameters.DocumentTopicWeight,
                parameters.SectionTopicWeight,
                parameters.ParagraphTopicWeight,
                parameters.SectionTopicWeight,
                parameters.DocumentSingleTermWeight,
                parameters.SectionSingleTermWeight,
                parameters.ParagraphSingleTermWeight,
                parameters.SentenceIATEWeight,
                parameters.SentenceEVWeight,
                parameters.DocumentTopicLimit,
                parameters.SectionTopicLimit,
                parameters.ParagraphTopicLimit,
                parameters.SentenceTopicLimit,
                parameters.DocumentTokenLimit,
                parameters.SectionTokenLimit,
                parameters.ParagraphTokenLimit,
                parameters.SentenceEVLimit,
                parameters.SentenceIATELimit,
                parameters.UseDocumentSimilarity,
                parameters.UseSectionSimilarity,
                parameters.UseDocumentTokens,
                parameters.UseSectionTokens,
                parameters.UseParagraphTokens,
                parameters.UseSentenceTokens,
                parameters.UseDocumentTopics,
                parameters.UseSectionTopics,
                parameters.UseParagraphTopics,
                parameters.UseSentenceTopics
                );
        }

        public MarcellCorpus(IndexingMode indexingMode, string indexPath)
        {
            lock (indexInitLock)
            {
                if (indexManager == null)
                {
                    //Configure the index manager
                    indexManager = new IndexManager(indexingMode, indexPath, true);

                    //Prepare the default implementation of 
                    m_searcherCache[defaultSearchParameters] = GetSearcher(defaultSearchParameters);
                    WarmUp();
                }
            }
        }

        public void WarmUp()
        {
            using (SimpleTextSearch searchText = new SimpleTextSearch(indexManager))
            {
                foreach (var lang in IndexManager.SupportedLanguages)
                {
                    _ = searchText.PerformSearch<Paragraph>(new SimpleQuery
                    {
                        Language = lang,
                        SearchIn = IndexObjectType.ParagraphIndex,
                        QueryString = "a"
                    });

                    _ = searchText.PerformSearch<Sentence>(new SimpleQuery
                    {
                        Language = lang,
                        SearchIn = IndexObjectType.SentenceIndex,
                        QueryString = "a"
                    });

                    _ = searchText.PerformSearch<Document>(new SimpleQuery
                    {
                        Language = lang,
                        SearchIn = IndexObjectType.SectionIndex,
                        QueryString = "a"
                    });
                }
            }
        }

        public Result<Document> FindDocument(string language, string query, bool searchParagraphs)
        {
            using (SimpleTextSearch searchText = new SimpleTextSearch(indexManager))
            {
                if (searchParagraphs)
                {
                    return searchText.PerformSearch<Document>(new SimpleQuery
                    {
                        Language = language,
                        SearchIn = IndexObjectType.ParagraphIndex,
                        QueryString = query
                    });
                }
                else
                {
                    return searchText.PerformSearch<Document>(new SimpleQuery
                    {
                        Language = language,
                        SearchIn = IndexObjectType.SectionIndex,
                        QueryString = query
                    });
                }
            }
        }

        public T FindTranslation<T>(Guid sourceId, string sourceLanguage, string targetLanguage, ParametrizedSearchParameters searchParameters) where T : class, IMarcellEntity
        {
            //TODO: Update to sent parameters
            OptimizedParametrizedSearch searchProvider;

            if (searchParameters == null || !searchParameters.IsSet())
            {
                searchProvider = m_searcherCache[defaultSearchParameters];
            }
            else
            {
                searchProvider = GetSearcher(searchParameters);
            }

            ParametrizedSearchQuery alignQuery;
            if (typeof(T) == typeof(Sentence))
            {
                var sourceSentence = searchProvider.GetSentence(sourceLanguage, sourceId);
                if (sourceSentence == null)
                {
                    return null;
                }

                alignQuery = new ParametrizedSearchQuery
                {
                    Language = targetLanguage,
                    DocumentTokens = sourceSentence.DocumentSimilarityData.ConsolidatedTokens.ToArray(),
                    DocumentTopics = sourceSentence.DocumentSimilarityData.ConsolidatedTopics.ToArray(),
                    SectionTokens = sourceSentence.SectionSimilarityData.ConsolidatedTokens.ToArray(),
                    SectionTopics = sourceSentence.SectionSimilarityData.ConsolidatedTopics.ToArray(),
                    ParagraphTokens = sourceSentence.ParagraphSimilarityData.ConsolidatedTokens.ToArray(),
                    ParagraphTopics = sourceSentence.ParagraphSimilarityData.ConsolidatedTopics.ToArray(),
                    SentenceTokens = sourceSentence.SentenceSimilarityData.ConsolidatedTokens.ToArray(),
                    SentenceTopics = sourceSentence.SentenceSimilarityData.ConsolidatedTopics.ToArray(),
                    SearchIn = IndexObjectType.SentenceIndex
                };
            }
            else
            {
                var sourceParagraph = searchProvider.GetParagraph(sourceLanguage, sourceId);
                if (sourceParagraph == null)
                {
                    return null;
                }

                alignQuery = new ParametrizedSearchQuery
                {
                    Language = targetLanguage,
                    DocumentTokens = sourceParagraph.DocumentSimilarityData.ConsolidatedTokens.ToArray(),
                    DocumentTopics = sourceParagraph.DocumentSimilarityData.ConsolidatedTopics.ToArray(),
                    SectionTokens = sourceParagraph.SectionSimilarityData.ConsolidatedTokens.ToArray(),
                    SectionTopics = sourceParagraph.SectionSimilarityData.ConsolidatedTopics.ToArray(),
                    ParagraphTokens = sourceParagraph.ParagraphSimilarityData.ConsolidatedTokens.ToArray(),
                    ParagraphTopics = sourceParagraph.ParagraphSimilarityData.ConsolidatedTopics.ToArray(),
                    SearchIn = IndexObjectType.ParagraphIndex
                };
            }

            return searchProvider.PerformSearch<T>(alignQuery, 1, 1).ResultList.FirstOrDefault();
        }

        public Document GetDocument(string language, Guid id)
        {
            using (Search<SimpleQuery> searchText = new SimpleTextSearch(indexManager))
            {
                Document result = null;
                try
                {
                    result = searchText.GetDocument(language, id, true);
                }
                catch (Exception) { } //IGNORE: we will try other types first}

                if (result == null)
                {
                    var tmpResult = searchText.GetSection(language, id);
                    if (tmpResult == null)
                    {
                        var tmpParagraph = searchText.GetParagraph(language, id);
                        if (tmpParagraph == null)
                        {
                            return null;
                        }
                        else
                        {
                            result = searchText.DocFromParagraph(language, tmpParagraph, true);
                            return result;
                        }
                    }
                    else
                    {
                        result = searchText.DocFromSection(language, tmpResult, true);
                        return result;
                    }
                }
                else
                {
                    return result;
                }
            }
        }
    }
}