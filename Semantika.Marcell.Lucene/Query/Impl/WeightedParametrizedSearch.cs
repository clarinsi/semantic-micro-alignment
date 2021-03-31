using Semantika.Marcell.LuceneStore.Index;
using System;
using LuceneNet = Lucene.Net;

namespace Semantika.Marcell.LuceneStore.Query.Impl
{
    public struct ParametrizedSearchQuery : ILanguageQuery
    {
        public string[] DocumentTokens { get; set; }
        public string[] SectionTokens { get; set; }
        public string[] ParagraphTokens { get; set; }
        public string[] SentenceTokens { get; set; }
        public string[] DocumentTopics { get; set; }
        public string[] SectionTopics { get; set; }
        public string[] ParagraphTopics { get; set; }
        public string[] SentenceTopics { get; set; }
        public string Language { get; set; }
        public IndexObjectType SearchIn { get; set; }
        public int ShiftStartingPosition { get; set; }
    }

    public abstract class WeightedParametrizedSearch<TQueryType> : Search<TQueryType> where TQueryType : ILanguageQuery
    {
        #region Weigths definition

        protected double m_documentTopicWeight;
        protected double m_sectionTopicWeight;
        protected double m_paragraphTopicWeight;
        protected double m_sentenceTopicWeight;

        protected double m_documentSingleTermWeight;
        protected double m_sectionSingleTermWeight;
        protected double m_paragraphSingleTermWeight;

        protected double m_sentenceIATEWeight;
        protected double m_sentenceEuroVocWeight;

        #endregion Weigths definition

        public double DocumentTopicWeight { get { return m_documentTopicWeight; } }
        public double SectionTopicWeight { get { return m_sectionTopicWeight; } }
        public double ParagraphTopicWeight { get { return m_paragraphTopicWeight; } }
        public double SentenceTopicWeight { get { return m_sentenceTopicWeight; } }

        public double DocumentSingleTermWeight { get { return m_documentSingleTermWeight; } }
        public double SectionSingleTermWeight { get { return m_sectionSingleTermWeight; } }
        public double ParagraphSingleTermWeight { get { return m_paragraphSingleTermWeight; } }

        public double SentenceIATEWeight { get { return m_sentenceIATEWeight; } }
        public double SentenceEuroVocWeight { get { return m_sentenceEuroVocWeight; } }

        protected LuceneNet.Search.Query GetWeightedTermQuery(string name, string value, double boost)
        {
            var query = queryParser.CreatePhraseQuery(name, value);
            if (query == null)
            {
                return null;
            }

            query.Boost = Convert.ToSingle(boost);

            return query;
        }

        public WeightedParametrizedSearch(IndexManager indexManager, double documentTopicWeight, double sectionTopicWeight, double paragraphTopicWeight, double sentenceTopicWeight,
            double documentSingleTermWeight, double sectionSingleTermWeight, double paragraphSingleTermWeight, double sentenceSingleTermWeight,
            double sentenceIATEWeight, double sentenceEuroVocWeight)
            : base(indexManager)
        {
            m_documentTopicWeight = documentTopicWeight;
            m_sectionTopicWeight = sectionTopicWeight;
            m_paragraphTopicWeight = paragraphTopicWeight;
            m_sentenceTopicWeight = sentenceTopicWeight;

            m_documentSingleTermWeight = documentSingleTermWeight;
            m_sectionSingleTermWeight = sectionSingleTermWeight;
            m_paragraphSingleTermWeight = paragraphSingleTermWeight;

            if (sentenceIATEWeight == 0)
            {
                m_sentenceIATEWeight = sentenceSingleTermWeight;
            }
            else
            {
                m_sentenceIATEWeight = sentenceIATEWeight;
            }

            if (sentenceEuroVocWeight == 0)
            {
                m_sentenceEuroVocWeight = sentenceSingleTermWeight;
            }
            else
            {
                m_sentenceEuroVocWeight = sentenceEuroVocWeight;
            }
        }
    }
}