using Semantika.Marcell.Data;
using Semantika.Marcell.LuceneStore.Index;
using Semantika.Marcell.LuceneStore.Query.Impl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WeightOptimizer.Genetics.Candidates
{
    public class ParametrizedAlignerCandidate<TEntityType> : Candidate<ParametrizedSearchQuery, TEntityType> where TEntityType : class, IMarcellEntity
    {
        protected double m_documentTopicWeight;
        protected double m_sectionTopicWeight;
        protected double m_paragraphTopicWeight;
        protected double m_sentenceTopicWeight;

        protected double m_documentTokenWeight;
        protected double m_sectionTokenWeight;
        protected double m_paragraphTokenWeight;
        protected double m_sentenceIATETokenWeight;
        protected double m_sentenceEVTokenWeight;

        protected double m_documentTopicLimit;
        protected double m_sectionTopicLimit;
        protected double m_paragraphTopicLimit;
        protected double m_sentenceTopicLimit;

        protected double m_documentTokenLimit;
        protected double m_sectionTokenLimit;
        protected double m_paragraphTokenLimit;
        protected double m_sentenceIATETokenLimit;
        protected double m_sentenceEVTokenLimit;

        protected bool m_useDocumentData;
        protected bool m_useSectionData;

        protected bool m_useDocumentTokens;
        protected bool m_useSectionTokens;
        protected bool m_useParagraphTokens;
        protected bool m_useSentenceTokens;

        protected bool m_useDocumentTopic;
        protected bool m_useSectionTopic;
        protected bool m_useParagraphTopic;
        protected bool m_useSentenceTopic;

        private const double m_failedFalseMatchPenalty = 10000;
        private const double m_failedNoMatchPenalty = 20 * m_failedFalseMatchPenalty;

        public ParametrizedAlignerCandidate(IndexManager indexManager,
                                            double documentTopicWeight,
                                            double sectionTopicWeight,
                                            double paragraphTopicWeight,
                                            double sentenceTopicWeight,

                                            double documentTokenWeight,
                                            double sectionTokenWeight,
                                            double paragraphTokenWeight,
                                            double sentenceIATETokenWeight,
                                            double sentenceEVTokenWeight,

                                            double documentTopicLimit,
                                            double sectionTopicLimit,
                                            double paragraphTopicLimit,
                                            double sentenceTopicLimit,

                                            double documentTokenLimit,
                                            double sectionTokenLimit,
                                            double paragraphTokenLimit,
                                            double sentenceIATETokenLimit,
                                            double sentenceEVTokenLimit,

                                            double useDocumentData,
                                            double useSectionData,

                                            double useDocumentTokens,
                                            double useSectionTokens,
                                            double useParagraphTokens,
                                            double useSentenceTokens,

                                            double useDocumentTopics,
                                            double useSectionTopics,
                                            double useParagraphTopics,
                                            double useSentenceTopics
            ) : base(indexManager)
        {
            m_documentTopicWeight = documentTopicWeight;
            m_sectionTopicWeight = sectionTopicWeight;
            m_paragraphTopicWeight = paragraphTopicWeight;
            m_sentenceTopicWeight = sentenceTopicWeight;

            m_documentTokenWeight = documentTokenWeight;
            m_sectionTokenWeight = sectionTokenWeight;
            m_paragraphTokenWeight = paragraphTokenWeight;
            m_sentenceIATETokenWeight = sentenceIATETokenWeight;
            m_sentenceEVTokenWeight = sentenceEVTokenWeight;

            m_documentTopicLimit = documentTopicLimit;
            m_sectionTopicLimit = sectionTopicLimit;
            m_paragraphTopicLimit = paragraphTopicLimit;
            m_sentenceTopicLimit = sentenceTopicLimit;

            m_documentTokenLimit = documentTokenLimit;
            m_sectionTokenLimit = sectionTokenLimit;
            m_paragraphTokenLimit = paragraphTokenLimit;
            m_sentenceIATETokenLimit = sentenceIATETokenLimit;
            m_sentenceEVTokenLimit = sentenceEVTokenLimit;

            m_useDocumentData = (useDocumentData >= 0.5);
            m_useSectionData = (useSectionData >= 0.5);

            m_useDocumentTokens = (useDocumentTokens >= 0.5);
            m_useSectionTokens = (useSectionTokens >= 0.5);
            m_useParagraphTokens = (useParagraphTokens >= 0.5);
            m_useSentenceTokens = (useSentenceTokens >= 0.5);

            m_useDocumentTopic = (useDocumentTopics >= 0.5);
            m_useSectionTopic = (useSectionTopics >= 0.5);
            m_useParagraphTopic = (useParagraphTopics >= 0.5);
            m_useSentenceTopic = (useSentenceTopics >= 0.5);

            InitSearcher();
        }

        public ParametrizedAlignerCandidate(IndexManager indexManager)
            : base(indexManager)
        {
            if (typeof(TEntityType) == typeof(Sentence))
            {
                if (!m_useSentenceTokens && !m_useSentenceTopic)
                {
                    m_useSentenceTokens = true;
                }
            }
            else if (typeof(TEntityType) == typeof(Paragraph))
            {
                if (!m_useParagraphTokens && !m_useParagraphTopic)
                {
                    m_useParagraphTokens = true;
                }
            }

            InitSearcher();
        }

        public ParametrizedAlignerCandidate(ParametrizedAlignerCandidate<TEntityType> parent1, ParametrizedAlignerCandidate<TEntityType> parent2)
            : base(parent1, parent2)
        {
            InitSearcher();
        }

        protected virtual void InitSearcher()
        {
            if (m_searcher != null)
            {
                m_searcher.Dispose();
            }

            m_searcher = new SimpleParametrizedSearch(
                    m_indexManager,

                    m_documentTopicWeight,
                    m_sectionTopicWeight,
                    m_paragraphTopicWeight,
                    m_sentenceTopicWeight,

                    m_documentTokenWeight,
                    m_sectionTokenWeight,
                    m_paragraphTokenWeight,
                    m_sentenceIATETokenWeight,
                    m_sentenceEVTokenWeight,

                    m_documentTopicLimit,
                    m_sectionTopicLimit,
                    m_paragraphTopicLimit,
                    m_sentenceTopicLimit,

                    m_documentTokenLimit,
                    m_sectionTokenLimit,
                    m_paragraphTokenLimit,
                    m_sentenceIATETokenLimit,
                    m_sentenceEVTokenLimit,

                    m_useDocumentData,
                    m_useSectionData,

                    m_useDocumentTokens,
                    m_useSectionTokens,
                    m_useParagraphTokens,
                    m_useSentenceTokens,

                    m_useDocumentTopic,
                    m_useSectionTopic,
                    m_useParagraphTopic,
                    m_useSentenceTopic
                    );
        }

        private ParametrizedSearchQuery GetQueryFromEntity(TEntityType sourceEntity, string language)
        {
            ParametrizedSearchQuery alignQuery = new ParametrizedSearchQuery
            {
                Language = language,
            };

            if (typeof(TEntityType) == typeof(Document))
            {
                var sourceDocument = sourceEntity as Document;
                alignQuery.DocumentTokens = sourceDocument.DocumentSimilarityData.ConsolidatedTokens.ToArray();
                alignQuery.DocumentTopics = sourceDocument.DocumentSimilarityData.ConsolidatedTopics.ToArray();
                alignQuery.SearchIn = IndexObjectType.DocumentIndex;
            }
            else if (typeof(TEntityType) == typeof(Section))
            {
                var sourceSection = sourceEntity as Section;
                alignQuery.DocumentTokens = sourceSection.DocumentSimilarityData.ConsolidatedTokens.ToArray();
                alignQuery.DocumentTopics = sourceSection.DocumentSimilarityData.ConsolidatedTopics.ToArray();
                alignQuery.SectionTokens = sourceSection.SectionSimilarityData.ConsolidatedTokens.ToArray();
                alignQuery.SectionTopics = sourceSection.SectionSimilarityData.ConsolidatedTopics.ToArray();
                alignQuery.SearchIn = IndexObjectType.DocumentIndex | IndexObjectType.SectionIndex;
            }
            else if (typeof(TEntityType) == typeof(Paragraph))
            {
                var sourceParagraph = sourceEntity as Paragraph;
                alignQuery.DocumentTokens = sourceParagraph.DocumentSimilarityData.ConsolidatedTokens.ToArray();
                alignQuery.DocumentTopics = sourceParagraph.DocumentSimilarityData.ConsolidatedTopics.ToArray();
                alignQuery.SectionTokens = sourceParagraph.SectionSimilarityData.ConsolidatedTokens.ToArray();
                alignQuery.SectionTopics = sourceParagraph.SectionSimilarityData.ConsolidatedTopics.ToArray();
                alignQuery.ParagraphTokens = sourceParagraph.ParagraphSimilarityData.ConsolidatedTokens.ToArray();
                alignQuery.ParagraphTopics = sourceParagraph.ParagraphSimilarityData.ConsolidatedTopics.ToArray();
                alignQuery.SearchIn = IndexObjectType.DocumentIndex | IndexObjectType.SectionIndex | IndexObjectType.ParagraphIndex;
            }
            else if (typeof(TEntityType) == typeof(Sentence))
            {
                var sourceSentence = sourceEntity as Sentence;
                alignQuery.DocumentTokens = sourceSentence.DocumentSimilarityData.ConsolidatedTokens.ToArray();
                alignQuery.DocumentTopics = sourceSentence.DocumentSimilarityData.ConsolidatedTopics.ToArray();
                alignQuery.SectionTokens = sourceSentence.SectionSimilarityData.ConsolidatedTokens.ToArray();
                alignQuery.SectionTopics = sourceSentence.SectionSimilarityData.ConsolidatedTopics.ToArray();
                alignQuery.ParagraphTokens = sourceSentence.ParagraphSimilarityData.ConsolidatedTokens.ToArray();
                alignQuery.ParagraphTopics = sourceSentence.ParagraphSimilarityData.ConsolidatedTopics.ToArray();
                alignQuery.SentenceTokens = sourceSentence.SentenceSimilarityData.ConsolidatedTokens.ToArray();
                alignQuery.SentenceTopics = sourceSentence.SentenceSimilarityData.ConsolidatedTopics.ToArray();
                alignQuery.SearchIn = IndexObjectType.DocumentIndex | IndexObjectType.SectionIndex | IndexObjectType.ParagraphIndex | IndexObjectType.SentenceIndex;
            }

            return alignQuery;
        }

        private double CalculateTokenSimilarity(TEntityType sourceEntity, TEntityType finalEntity)
        {
            double missedTokens = 0, allTokens = 0, penalty = 0;

            IEnumerable<string> sourceTokens, finalTokens, sourceTopics, finalTopics;

            if (typeof(TEntityType) == typeof(Document))
            {
                var sourceDocument = sourceEntity as Document;
                var finalDocument = sourceEntity as Document;
                sourceTokens = sourceDocument.DocumentSimilarityData.ConsolidatedTokens.ToArray();
                sourceTopics = sourceDocument.DocumentSimilarityData.ConsolidatedTopics.ToArray();
                finalTokens = finalDocument.DocumentSimilarityData.ConsolidatedTokens.ToArray();
                finalTopics = finalDocument.DocumentSimilarityData.ConsolidatedTopics.ToArray();
            }
            else if (typeof(TEntityType) == typeof(Section))
            {
                var sourceSection = sourceEntity as Section;
                var finalSection = sourceEntity as Section;
                sourceTokens = sourceSection.SectionSimilarityData.ConsolidatedTokens.ToArray();
                sourceTopics = sourceSection.SectionSimilarityData.ConsolidatedTopics.ToArray();
                finalTokens = finalSection.SectionSimilarityData.ConsolidatedTokens.ToArray();
                finalTopics = finalSection.SectionSimilarityData.ConsolidatedTopics.ToArray();
            }
            else if (typeof(TEntityType) == typeof(Paragraph))
            {
                var sourceParagraph = sourceEntity as Paragraph;
                var finalParagraph = sourceEntity as Paragraph;
                sourceTokens = sourceParagraph.ParagraphSimilarityData.ConsolidatedTokens.ToArray();
                sourceTopics = sourceParagraph.ParagraphSimilarityData.ConsolidatedTopics.ToArray();
                finalTokens = finalParagraph.ParagraphSimilarityData.ConsolidatedTokens.ToArray();
                finalTopics = finalParagraph.ParagraphSimilarityData.ConsolidatedTopics.ToArray();
            }
            else if (typeof(TEntityType) == typeof(Sentence))
            {
                var sourceSentence = sourceEntity as Sentence;
                var finalSentence = sourceEntity as Sentence;
                sourceTokens = sourceSentence.SentenceSimilarityData.ConsolidatedTokens.ToArray();
                sourceTopics = sourceSentence.SentenceSimilarityData.ConsolidatedTopics.ToArray();
                finalTokens = finalSentence.SentenceSimilarityData.ConsolidatedTokens.ToArray();
                finalTopics = finalSentence.SentenceSimilarityData.ConsolidatedTopics.ToArray();
            }
            else
            {
                throw new InvalidOperationException("Unsupported entity type");
            }

            foreach (var token in sourceTokens)
            {
                if (!finalTokens.Contains(token))
                {
                    missedTokens++;
                }
                allTokens++;
            }

            if (allTokens > 0)
            {
                penalty += (missedTokens / allTokens) * m_failedFalseMatchPenalty * 0.75;
            }
            else
            {
                penalty += m_failedFalseMatchPenalty * 0.75;
            }

            allTokens = 0;
            missedTokens = 0;
            foreach (var token in finalTokens)
            {
                if (!sourceTokens.Contains(token))
                {
                    missedTokens++;
                }
                allTokens++;
            }

            if (allTokens > 0)
            {
                penalty += (missedTokens / allTokens) * m_failedFalseMatchPenalty * 0.25;
            }
            else
            {
                penalty += m_failedFalseMatchPenalty * 0.25;
            }

            int allTopics = 0;
            int missedTopics = 0;
            foreach (var token in sourceTopics)
            {
                if (!finalTopics.Contains(token))
                {
                    missedTopics++;
                }
                allTopics++;
            }

            if (allTopics > 0)
            {
                penalty += (missedTopics / allTopics) * m_failedFalseMatchPenalty * 0.75;
            }
            else
            {
                penalty += m_failedFalseMatchPenalty * 0.75;
            }

            allTopics = 0;
            missedTopics = 0;
            foreach (var token in finalTopics)
            {
                if (!sourceTopics.Contains(token))
                {
                    missedTopics++;
                }
                allTopics++;
            }

            if (allTopics > 0)
            {
                penalty += (missedTopics / allTopics) * m_failedFalseMatchPenalty * 0.25;
            }
            else
            {
                penalty += m_failedFalseMatchPenalty * 0.25;
            }

            return penalty;
        }

        private IEnumerable<IMarcellEntity> PerformSearch(TEntityType sourceEntity, ParametrizedSearchQuery query, int resultCount)
        {
            if (typeof(TEntityType) == typeof(Paragraph))
            {
                return m_searcher.PerformSearch<Paragraph>(query, resultCount, 1).ResultList;
            }
            else if (typeof(TEntityType) == typeof(Sentence))
            {
                return m_searcher.PerformSearch<Sentence>(query, resultCount, 1).ResultList;
            }
            throw new ArgumentException("Unsupported type provided!");
        }

        protected virtual double PerformRoundTrip(TEntityType genericSourceEntity, string language)
        {
            /* The logic is as follows:
             *  - the initial quality metric is whether the parameters are set in a way that allow finding the original paragraph in the original language; this is abasic sanity check to make sure we are losing as little information in the translation as possible
             *  - we then perform n language 1 -> lanugage 2 alignements based on tokens & parameters
             *  - we go through each of them and perform n translations back from language 2 -> language 1
             *  - if we detertmine that one of the lang 1 -> lang 2 -> lang 1 translations is the original paragraph, the penalty is calculated based on the position of the result (higher the better) and the similarity between the orginal, translated and back-translated paragraphs
             *  - if not, the penalty is determined based on the similarity between translated and final paragraphs
             */

            const int scannedDocuments = 10;
            //try
            {
                //First, try to locate the paragraph by using the Marcell metadata within the same language. This should always return the correct paragraph in cases,where the recognition qualit is high enough
                var sourceEntity = genericSourceEntity as TEntityType;

                ParametrizedSearchQuery selfAlignQuery = GetQueryFromEntity(sourceEntity, sourceEntity.Language);

                var selfTranslatedParagraphs = PerformSearch(sourceEntity, selfAlignQuery, 1);
                if (selfTranslatedParagraphs.Count() == 0)
                {
                    //When searching for the randomly selected paragraph by using its own set of keywords in the original language, no match was found -> maximum penalty should be applied
                    return 10 * m_failedNoMatchPenalty;
                }
                else
                {
                    if (selfTranslatedParagraphs.First().InternalId != sourceEntity.InternalId)
                    {
                        //We located a different paragraphthan the one we were searching for in the original language -> apply a high penalty for that
                        return 5 * m_failedFalseMatchPenalty;
                    }
                }

                ParametrizedSearchQuery alignQuery = GetQueryFromEntity(sourceEntity, language);

                var translatedParagraphs = PerformSearch(sourceEntity, alignQuery, scannedDocuments);

                if (translatedParagraphs.Count() == 0)
                {
                    return 2 * m_failedNoMatchPenalty;
                }

                TEntityType firstMatchFinal = null;
                for (int i = 0; i < translatedParagraphs.Count(); i++)
                {
                    var translatedParagraph = translatedParagraphs.ElementAt(i) as TEntityType;
                    ParametrizedSearchQuery alignBackQuery = GetQueryFromEntity(translatedParagraph, sourceEntity.Language);
                    var finalParagraph = PerformSearch(translatedParagraph, alignBackQuery, scannedDocuments);

                    if (finalParagraph.Count() == 0)
                    {
                        return m_failedNoMatchPenalty;
                    }

                    if (i == 0)
                    {
                        //We are within the first match for the translated paragraph, we select it
                        firstMatchFinal = finalParagraph.First() as TEntityType;
                    }

                    //Check if one of the top n matches is the one we want
                    for (int j = 0; j < finalParagraph.Count(); j++)
                    {
                        if (finalParagraph.ElementAt(j).InternalId == sourceEntity.InternalId)
                        {
                            //It is - the penalty is based on the search result position;
                            //the higher the result, the lower the penalty
                            double penaltyFirst = (Math.Log(i + 1) / Math.Log(translatedParagraphs.Count()));
                            double penaltySecond = (Math.Log(j + 1) / Math.Log(finalParagraph.Count()));

                            if (i == 0 && j == 0)
                            {
                                //for the correct match just short-circuit and return 0 to avoid lengthy unnecessary calculation
                                return 0;
                            }
                            else
                            {
                                return (penaltyFirst * 5 + penaltySecond) * m_failedFalseMatchPenalty / 2 //Initial penalty calculated based on the position of the correct result after round trip
                                    + CalculateTokenSimilarity(sourceEntity, translatedParagraphs.First() as TEntityType) + //Additional penalty calculated based on the similarity of the first match in translated and round-trip. THe more similar to the original they are, the better
                                    CalculateTokenSimilarity(sourceEntity, firstMatchFinal) / 2;
                            }
                        }
                    }
                }

                //It isn't. We now calculate the penalty, based on the base of the no match penalty with the addition of the number of paragraph tokens not matching after round trip
                //We add the maximum possible penalty in case a direct match is found, which is 4x the false match penalty
                return 7 * m_failedFalseMatchPenalty + CalculateTokenSimilarity(sourceEntity, translatedParagraphs.First() as TEntityType) + CalculateTokenSimilarity(sourceEntity, firstMatchFinal) / 2;
            }
        }

        public override int WeightCount => m_weightNameArray.Length;

        protected override void PerformEvaluation(ref double finalScore, LearningSet set)
        {
            double tmpScore = PerformRoundTrip(set.Entity, set.Language);
            finalScore += tmpScore;
        }

        public override void Mutate()
        {
            int mutationIndex = (int)(GetRandomWeight() * 28);
            SetWeight(mutationIndex, GetRandomWeight());

            InitSearcher();
        }

        public override double GetWeight(int weightIndex)
        {
            switch (weightIndex)
            {
                case 0:
                    return m_documentTopicWeight;

                case 1:
                    return m_sectionTopicWeight;

                case 2:
                    return m_paragraphTopicWeight;

                case 3:
                    return m_sentenceTopicWeight;

                case 4:
                    return m_documentTokenWeight;

                case 5:
                    return m_sectionTokenWeight;

                case 6:
                    return m_paragraphTokenWeight;

                case 7:
                    return m_sentenceIATETokenWeight;

                case 8:
                    return m_sentenceEVTokenWeight;

                case 9:
                    return m_documentTopicLimit;

                case 10:
                    return m_sectionTopicLimit;

                case 11:
                    return m_paragraphTopicLimit;

                case 12:
                    return m_sentenceTopicLimit;

                case 13:
                    return m_documentTokenLimit;

                case 14:
                    return m_sectionTokenLimit;

                case 15:
                    return m_paragraphTokenLimit;

                case 16:
                    return m_sentenceIATETokenLimit;

                case 17:
                    return m_sentenceEVTokenLimit;

                case 18:
                    return m_useDocumentData ? 1 : 0;

                case 19:
                    return m_useSectionData ? 1 : 0;

                case 20:
                    return m_useDocumentTokens ? 1 : 0;

                case 21:
                    return m_useSectionTokens ? 1 : 0;

                case 22:
                    return m_useParagraphTokens ? 1 : 0;

                case 23:
                    return m_useSentenceTokens ? 1 : 0;

                case 24:
                    return m_useDocumentTopic ? 1 : 0;

                case 25:
                    return m_useSectionTopic ? 1 : 0;

                case 26:
                    return m_useParagraphTopic ? 1 : 0;

                case 27:
                    return m_useSentenceTopic ? 1 : 0;
            }

            return -1;
        }

        private static readonly string[] m_weightNameArray = new string[] {
                    "DocumentTopicWeight",
                    "SectionTopicWeight",
                    "ParagraphTopicWeight",
                    "SentenceTopicWeight",
                    "DocumentTokenWeight",
                    "SectionTokenWeight",
                    "ParagraphTokenWeight",
                    "SentenceIATETokenWeight",
                    "SentenceEVTokenWeight",
                    "DocumentTopicLimit",
                    "SectionTopicLimit",
                    "ParagraphTopicLimit",
                    "SentenceTopicLimit",
                    "DocumentTokenLimit",
                    "SectionTokenLimit",
                    "ParagraphTokenLimit",
                    "SentenceIATETokenLimit",
                    "SentenceEVTokenLimit",
                    "UseDocumentData",
                    "UseSectionData",
                    "UseDocumentToken",
                    "UseSectionToken",
                    "UseParagraphToken",
                    "UseSentenceToken",
                    "UseDocumentTopic",
                    "UseSectionTopic",
                    "UseParagraphTopic",
                    "UseSentenceTopic"
            };

        public override string GetWeightName(int weightIndex)
        {
            return m_weightNameArray[weightIndex];
        }

        public override void SetWeight(int weightIndex, double weightValue)
        {
            switch (weightIndex)
            {
                case 0:
                    m_documentTopicWeight = weightValue; break;
                case 1:
                    m_sectionTopicWeight = weightValue; break;
                case 2:
                    m_paragraphTopicWeight = weightValue; break;
                case 3:
                    m_sentenceTopicWeight = weightValue; break;
                case 4:
                    m_documentTokenWeight = weightValue; break;
                case 5:
                    m_sectionTokenWeight = weightValue; break;
                case 6:
                    m_paragraphTokenWeight = weightValue; break;
                case 7:
                    m_sentenceIATETokenWeight = weightValue; break;
                case 8:
                    m_sentenceEVTokenWeight = weightValue; break;
                case 9:
                    m_documentTopicLimit = weightValue; break;
                case 10:
                    m_sectionTopicLimit = weightValue; break;
                case 11:
                    m_paragraphTopicLimit = weightValue; break;
                case 12:
                    m_sentenceTopicLimit = weightValue; break;
                case 13:
                    m_documentTokenLimit = weightValue; break;
                case 14:
                    m_sectionTokenLimit = weightValue; break;
                case 15:
                    m_paragraphTokenLimit = weightValue; break;
                case 16:
                    m_sentenceIATETokenLimit = weightValue; break;
                case 17:
                    m_sentenceEVTokenLimit = weightValue; break;
                case 18:
                    m_useDocumentData = (weightValue >= 0.5); break;
                case 19:
                    m_useSectionData = (weightValue >= 0.5); break;
                case 20:
                    m_useDocumentTokens = (weightValue >= 0.5); break;
                case 21:
                    m_useSectionTokens = (weightValue >= 0.5); break;
                case 22:
                    m_useParagraphTokens = (weightValue >= 0.5); break;
                case 23:
                    m_useSentenceTokens = (weightValue >= 0.5); break;
                case 24:
                    m_useDocumentTopic = (weightValue >= 0.5); break;
                case 25:
                    m_useSectionTopic = (weightValue >= 0.5); break;
                case 26:
                    m_useParagraphTopic = (weightValue >= 0.5); break;
                case 27:
                    m_useSentenceTopic = (weightValue >= 0.5); break;
                default:
                    throw new ArgumentException("Invalid index!");
            }
        }
    }
}