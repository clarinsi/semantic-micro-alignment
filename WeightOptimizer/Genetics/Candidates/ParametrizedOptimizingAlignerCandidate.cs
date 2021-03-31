using Semantika.Marcell.Data;
using Semantika.Marcell.LuceneStore.Index;
using Semantika.Marcell.LuceneStore.Query.Impl;

namespace WeightOptimizer.Genetics.Candidates
{
    public class ParametrizedOptimizingAlignerCandidate<TEntityType> : ParametrizedAlignerCandidate<TEntityType> where TEntityType : class, IMarcellEntity
    {
        private double m_useDocumentDataPercentage;
        private double m_useSectionDataPercentage;

        public ParametrizedOptimizingAlignerCandidate(IndexManager indexManager,
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

            m_useDocumentDataPercentage = useDocumentData;
            m_useSectionDataPercentage = useSectionData;

            m_useDocumentTokens = (useDocumentTokens > 0.5);
            m_useSectionTokens = (useSectionTokens > 0.5);
            m_useParagraphTokens = (useParagraphTokens > 0.5);
            m_useSentenceTokens = (useSentenceTokens > 0.5);

            m_useDocumentTopic = (useDocumentTopics > 0.5);
            m_useSectionTopic = (useSectionTopics > 0.5);
            m_useParagraphTopic = (useParagraphTopics > 0.5);
            m_useSentenceTopic = (useSentenceTopics > 0.5);

            InitSearcher();
        }

        public ParametrizedOptimizingAlignerCandidate(IndexManager indexManager)
            : base(indexManager)
        {
            m_useDocumentDataPercentage = GetRandomWeight();
            m_useSectionDataPercentage = GetRandomWeight();

            InitSearcher();
        }

        public ParametrizedOptimizingAlignerCandidate(ParametrizedAlignerCandidate<TEntityType> parent1, ParametrizedAlignerCandidate<TEntityType> parent2)
            : base(parent1, parent2)
        {
            InitSearcher();
        }

        protected override void InitSearcher()
        {
            if (m_searcher != null)
            {
                m_searcher.Dispose();
            }

            m_searcher = new OptimizedParametrizedSearch(
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

                    m_useDocumentDataPercentage,
                    m_useSectionDataPercentage,

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

        public override double GetWeight(int weightIndex)
        {
            switch (weightIndex)
            {
                case 18:
                    return m_useDocumentDataPercentage;

                case 19:
                    return m_useSectionDataPercentage;

                default:
                    return base.GetWeight(weightIndex);
            }
        }

        public override void SetWeight(int weightIndex, double weightValue)
        {
            switch (weightIndex)
            {
                case 18:
                    m_useDocumentDataPercentage = weightValue; break;
                case 19:
                    m_useSectionDataPercentage = weightValue; break;
                default:
                    base.SetWeight(weightIndex, weightValue); break;
            }
        }
    }
}