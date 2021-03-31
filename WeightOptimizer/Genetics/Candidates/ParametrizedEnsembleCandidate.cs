using Semantika.Marcell.Data;
using Semantika.Marcell.LuceneStore.Index;
using Semantika.Marcell.LuceneStore.Query.Impl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WeightOptimizer.Genetics.Candidates
{
    public class ParametrizedEnsembleCandidate<TEntityType> : Candidate<ParametrizedSearchQuery, TEntityType> where TEntityType : class, IMarcellEntity
    {
        private const double m_failedFalseMatchPenalty = 10000;
        private const double m_failedNoMatchPenalty = 20 * m_failedFalseMatchPenalty;

        private const int m_ensembleSize = 5;
        private readonly EnsembleMemberParameters[] m_ensembleParameters = new EnsembleMemberParameters[m_ensembleSize];

        public ParametrizedEnsembleCandidate(IndexManager indexManager) : base(indexManager)
        {
            InitSearcher();
        }

        public ParametrizedEnsembleCandidate(ParametrizedEnsembleCandidate<TEntityType> parent1, ParametrizedEnsembleCandidate<TEntityType> parent2) : base(parent1, parent2)
        {
            InitSearcher();
        }

        public ParametrizedEnsembleCandidate(IndexManager indexManager, params double[] weights) : base(indexManager)
        {
            if (weights.Length != m_ensembleSize * m_weightNameArray.Length)
            {
                throw new ArgumentException("Incorrect number of weights provided!");
            }

            for (int i = 0; i < weights.Length; i++)
            {
                SetWeight(i, weights[i]);
            }

            InitSearcher();
        }

        protected virtual void InitSearcher()
        {
            if (m_searcher != null)
            {
                m_searcher.Dispose();
            }

            m_searcher = new OptimizedEnsembleSearch(m_indexManager, m_ensembleParameters);
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
             *  - the initial quality metric is whether the parameters are set in a way that allow finding the original paragraph in the original language; this is abasic sanity check to make sure we are loosing as little information in the translation as possible
             *  - we then perform n language 1 -> lanugage 2 alignements based on tokens & parameters
             *  - we go through each of them and perform n translations back from language 2 -> language 1
             *  - if we detertmine that one of the lang 1 -> lang 2 -> lang 1 translations is the original paragraph, the penalty is calculated based on the position of the result (higher the better) and the similarity between the orginal, translated and back-translated paragraphs
             *  - if not, the penalty is determined based on the similarity between translated and final paragraphs
             */

            //Number of top documents to scan for the exact match between the
            const int scannedDocuments = 10;
            const double firstMatchWeight = 5;
            const double notFoundIntTopNPenalty = 3;
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
                        //We located a different paragraph than the one we were searching for in the original language -> apply a high penalty for that
                        return 5 * m_failedNoMatchPenalty;
                    }
                }

                ParametrizedSearchQuery alignQuery = GetQueryFromEntity(sourceEntity, language);

                var translatedParagraphs = PerformSearch(sourceEntity, alignQuery, scannedDocuments);

                if (translatedParagraphs.Count() == 0)
                {
                    return 2 * m_failedNoMatchPenalty;
                }

                TEntityType firstMatchFinal = null;
                double firstMatchFinalSimilarity = double.MaxValue;
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
                        firstMatchFinalSimilarity = firstMatchWeight * CalculateTokenSimilarity(sourceEntity, firstMatchFinal);
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
                                    firstMatchFinalSimilarity;
                            }
                        }
                    }
                }

                //It isn't. We now calculate the penalty, based on the base of the no match penalty with the addition of the number of paragraph tokens not matching after round trip
                //We add the maximum possible penalty in case a direct match is found, which is 7x the false match penalty
                return (7 + notFoundIntTopNPenalty) * m_failedFalseMatchPenalty + CalculateTokenSimilarity(sourceEntity, translatedParagraphs.First() as TEntityType) + firstMatchFinalSimilarity;
            }
        }

        protected override void PerformEvaluation(ref double finalScore, LearningSet set)
        {
            double tmpScore = PerformRoundTrip(set.Entity, set.Language);
            finalScore += tmpScore;
        }

        public override void Mutate()
        {
            int mutationIndex = (int)(GetRandomWeight() * 30);
            SetWeight(mutationIndex, GetRandomWeight());

            InitSearcher();
        }

        public override int WeightCount => m_ensembleSize * m_weightNameArray.Length;

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
                    "UseSentenceTopic",
                    "ScoringStrategy",
                    "StartPosShift"
            };

        public override double GetWeight(int weightIndex)
        {
            int mainIndex = weightIndex / m_weightNameArray.Length;
            int innerIndex = weightIndex % m_weightNameArray.Length;
            switch (innerIndex)
            {
                case 0:
                    return m_ensembleParameters[mainIndex].DocumentTopicWeight;

                case 1:
                    return m_ensembleParameters[mainIndex].SectionTopicWeight;

                case 2:
                    return m_ensembleParameters[mainIndex].ParagraphTopicWeight;

                case 3:
                    return m_ensembleParameters[mainIndex].SentenceTopicWeight;

                case 4:
                    return m_ensembleParameters[mainIndex].DocumentSingleTermWeight;

                case 5:
                    return m_ensembleParameters[mainIndex].SectionSingleTermWeight;

                case 6:
                    return m_ensembleParameters[mainIndex].ParagraphSingleTermWeight;

                case 7:
                    return m_ensembleParameters[mainIndex].SentenceIATEWeight;

                case 8:
                    return m_ensembleParameters[mainIndex].SentenceEVWeight;

                case 9:
                    return m_ensembleParameters[mainIndex].DocumentTopicLimit;

                case 10:
                    return m_ensembleParameters[mainIndex].SectionTopicLimit;

                case 11:
                    return m_ensembleParameters[mainIndex].ParagraphTopicLimit;

                case 12:
                    return m_ensembleParameters[mainIndex].SentenceTopicLimit;

                case 13:
                    return m_ensembleParameters[mainIndex].DocumentTokenLimit;

                case 14:
                    return m_ensembleParameters[mainIndex].SectionTokenLimit;

                case 15:
                    return m_ensembleParameters[mainIndex].ParagraphTokenLimit;

                case 16:
                    return m_ensembleParameters[mainIndex].SentenceIATELimit;

                case 17:
                    return m_ensembleParameters[mainIndex].SentenceEVLimit;

                case 18:
                    return m_ensembleParameters[mainIndex].UseDocumentSimilarity;

                case 19:
                    return m_ensembleParameters[mainIndex].UseSectionSimilarity;

                case 20:
                    return m_ensembleParameters[mainIndex].UseDocumentTokens ? 1 : 0;

                case 21:
                    return m_ensembleParameters[mainIndex].UseSectionTokens ? 1 : 0;

                case 22:
                    return m_ensembleParameters[mainIndex].UseParagraphTokens ? 1 : 0;

                case 23:
                    return m_ensembleParameters[mainIndex].UseSentenceTokens ? 1 : 0;

                case 24:
                    return m_ensembleParameters[mainIndex].UseDocumentTopics ? 1 : 0;

                case 25:
                    return m_ensembleParameters[mainIndex].UseSectionTopics ? 1 : 0;

                case 26:
                    return m_ensembleParameters[mainIndex].UseParagraphTopics ? 1 : 0;

                case 27:
                    return m_ensembleParameters[mainIndex].UseSentenceTopics ? 1 : 0;

                case 28:
                    return ((double)(int)(m_ensembleParameters[mainIndex].ScoringStrategy)) / 4;

                case 29:
                    return ((double)(m_ensembleParameters[mainIndex].StartPositionShift)) / 3;

                default:
                    throw new ArgumentException("Invalid index!");
            }
        }

        public override string GetWeightName(int weightIndex)
        {
            int mainIndex = weightIndex / m_weightNameArray.Length;
            int innerIndex = weightIndex % m_weightNameArray.Length;
            return $"{m_weightNameArray[innerIndex]}{mainIndex}";
        }

        public override void SetWeight(int weightIndex, double weightValue)
        {
            int mainIndex = weightIndex / m_weightNameArray.Length;
            int innerIndex = weightIndex % m_weightNameArray.Length;
            switch (innerIndex)
            {
                case 0:
                    m_ensembleParameters[mainIndex].DocumentTopicWeight = weightValue; break;
                case 1:
                    m_ensembleParameters[mainIndex].SectionTopicWeight = weightValue; break;
                case 2:
                    m_ensembleParameters[mainIndex].ParagraphTopicWeight = weightValue; break;
                case 3:
                    m_ensembleParameters[mainIndex].SentenceTopicWeight = weightValue; break;
                case 4:
                    m_ensembleParameters[mainIndex].DocumentSingleTermWeight = weightValue; break;
                case 5:
                    m_ensembleParameters[mainIndex].SectionSingleTermWeight = weightValue; break;
                case 6:
                    m_ensembleParameters[mainIndex].ParagraphSingleTermWeight = weightValue; break;
                case 7:
                    m_ensembleParameters[mainIndex].SentenceIATEWeight = weightValue; break;
                case 8:
                    m_ensembleParameters[mainIndex].SentenceEVWeight = weightValue; break;
                case 9:
                    m_ensembleParameters[mainIndex].DocumentTopicLimit = weightValue; break;
                case 10:
                    m_ensembleParameters[mainIndex].SectionTopicLimit = weightValue; break;
                case 11:
                    m_ensembleParameters[mainIndex].ParagraphTopicLimit = weightValue; break;
                case 12:
                    m_ensembleParameters[mainIndex].SentenceTopicLimit = weightValue; break;
                case 13:
                    m_ensembleParameters[mainIndex].DocumentTokenLimit = weightValue; break;
                case 14:
                    m_ensembleParameters[mainIndex].SectionTokenLimit = weightValue; break;
                case 15:
                    m_ensembleParameters[mainIndex].ParagraphTokenLimit = weightValue; break;
                case 16:
                    m_ensembleParameters[mainIndex].SentenceIATELimit = weightValue; break;
                case 17:
                    m_ensembleParameters[mainIndex].SentenceEVLimit = weightValue; break;
                case 18:
                    m_ensembleParameters[mainIndex].UseDocumentSimilarity = weightValue; break;
                case 19:
                    m_ensembleParameters[mainIndex].UseSectionSimilarity = weightValue; break;
                case 20:
                    m_ensembleParameters[mainIndex].UseDocumentTokens = (weightValue >= 0.5); break;
                case 21:
                    m_ensembleParameters[mainIndex].UseSectionTokens = (weightValue >= 0.5); break;
                case 22:
                    m_ensembleParameters[mainIndex].UseParagraphTokens = (weightValue >= 0.5); break;
                case 23:
                    m_ensembleParameters[mainIndex].UseSentenceTokens = (weightValue >= 0.5); break;
                case 24:
                    m_ensembleParameters[mainIndex].UseDocumentTopics = (weightValue >= 0.5); break;
                case 25:
                    m_ensembleParameters[mainIndex].UseSectionTopics = (weightValue >= 0.5); break;
                case 26:
                    m_ensembleParameters[mainIndex].UseParagraphTopics = (weightValue >= 0.5); break;
                case 27:
                    m_ensembleParameters[mainIndex].UseSentenceTopics = (weightValue >= 0.5); break;
                case 28:
                    m_ensembleParameters[mainIndex].ScoringStrategy = (EnsembleMemberScoring)(int)(weightValue * 4); break;
                case 29:
                    m_ensembleParameters[mainIndex].StartPositionShift = (int)(weightValue * 3); break;
                default:
                    throw new ArgumentException("Invalid index!");
            }
        }
    }
}