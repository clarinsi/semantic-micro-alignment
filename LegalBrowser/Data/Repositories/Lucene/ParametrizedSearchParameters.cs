namespace LegalBrowser.Data.Repositories.Lucene
{
    public class ParametrizedSearchParameters
    {
        /// <summary>
        /// Gets or sets the document topic weight.
        /// </summary>
        public double DocumentTopicWeight { get; set; } = 0;
        /// <summary>
        /// Gets or sets the section topic weight.
        /// </summary>
        public double SectionTopicWeight { get; set; } = 0;
        /// <summary>
        /// Gets or sets the paragraph topic weight.
        /// </summary>
        public double ParagraphTopicWeight { get; set; } = 0;
        /// <summary>
        /// Gets or sets the sentence topic weight.
        /// </summary>
        public double SentenceTopicWeight { get; set; } = 0;
        /// <summary>
        /// Gets or sets the document single term weight.
        /// </summary>
        public double DocumentSingleTermWeight { get; set; } = 0;
        /// <summary>
        /// Gets or sets the section single term weight.
        /// </summary>
        public double SectionSingleTermWeight { get; set; } = 0;
        /// <summary>
        /// Gets or sets the paragraph single term weight.
        /// </summary>
        public double ParagraphSingleTermWeight { get; set; } = 0;
        /// <summary>
        /// Gets or sets the sentence IATE token weight.
        /// </summary>
        public double SentenceIATEWeight { get; set; } = 0;
        /// <summary>
        /// Gets or sets the sentence EV token weight.
        /// </summary>
        public double SentenceEVWeight { get; set; } = 0;
        /// <summary>
        /// Gets or sets the document topic limit.
        /// </summary>
        public double DocumentTopicLimit { get; set; } = 0;
        /// <summary>
        /// Gets or sets the section topic limit.
        /// </summary>
        public double SectionTopicLimit { get; set; } = 0;
        /// <summary>
        /// Gets or sets the paragraph topic limit.
        /// </summary>
        public double ParagraphTopicLimit { get; set; } = 0;
        /// <summary>
        /// Gets or sets the sentence topic limit.
        /// </summary>
        public double SentenceTopicLimit { get; set; } = 0;
        /// <summary>
        /// Gets or sets the document token limit.
        /// </summary>
        public double DocumentTokenLimit { get; set; } = 0;
        /// <summary>
        /// Gets or sets the section token limit.
        /// </summary>
        public double SectionTokenLimit { get; set; } = 0;
        /// <summary>
        /// Gets or sets the paragraph token limit.
        /// </summary>
        public double ParagraphTokenLimit { get; set; } = 0;
        /// <summary>
        /// Gets or sets the sentence i a t e limit.
        /// </summary>
        public double SentenceIATELimit { get; set; } = 0;
        /// <summary>
        /// Gets or sets the sentence e v limit.
        /// </summary>
        public double SentenceEVLimit { get; set; } = 0;
        /// <summary>
        /// Gets or sets the use document similarity.
        /// </summary>
        public double UseDocumentSimilarity { get; set; } = 0;
        /// <summary>
        /// Gets or sets the use section similarity.
        /// </summary>
        public double UseSectionSimilarity { get; set; } = 0;
        /// <summary>
        /// Gets or sets a value indicating whether use document tokens.
        /// </summary>
        public bool UseDocumentTokens { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether use section tokens.
        /// </summary>
        public bool UseSectionTokens { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether use paragraph tokens.
        /// </summary>
        public bool UseParagraphTokens { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether use sentence tokens.
        /// </summary>
        public bool UseSentenceTokens { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether use document topics.
        /// </summary>
        public bool UseDocumentTopics { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether use section topics.
        /// </summary>
        public bool UseSectionTopics { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether use paragraph topics.
        /// </summary>
        public bool UseParagraphTopics { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether use sentence topics.
        /// </summary>
        public bool UseSentenceTopics { get; set; } = false;

        public bool IsSet()
        {
            if (!(UseDocumentTokens || UseDocumentTopics || UseParagraphTokens || UseParagraphTokens || UseParagraphTopics
                || UseSectionTokens || UseSectionTopics || UseSentenceTokens || UseSentenceTopics)) { return false; }

            if (DocumentTopicWeight > 0 && DocumentTopicWeight <= 1) { return true; }
            if (SectionTopicWeight > 0 && SectionTopicWeight <= 1) { return true; }
            if (ParagraphTopicWeight > 0 && ParagraphTopicWeight <= 1) { return true; }
            if (SentenceTopicWeight > 0 && SentenceTopicWeight <= 1) { return true; }
            if (DocumentSingleTermWeight > 0 && DocumentSingleTermWeight <= 1) { return true; }
            if (SectionSingleTermWeight > 0 && SectionSingleTermWeight <= 1) { return true; }
            if (ParagraphSingleTermWeight > 0 && ParagraphSingleTermWeight <= 1) { return true; }
            if (SentenceIATEWeight > 0 && SentenceIATEWeight <= 1) { return true; }
            if (SentenceEVWeight > 0 && SentenceEVWeight <= 1) { return true; }
            if (DocumentTopicLimit > 0 && DocumentTopicLimit <= 1) { return true; }
            if (SectionTopicLimit > 0 && SectionTopicLimit <= 1) { return true; }
            if (ParagraphTopicLimit > 0 && ParagraphTopicLimit <= 1) { return true; }
            if (SentenceTopicLimit > 0 && SentenceTopicLimit <= 1) { return true; }
            if (DocumentTokenLimit > 0 && DocumentTokenLimit <= 1) { return true; }
            if (SectionTokenLimit > 0 && SectionTokenLimit <= 1) { return true; }
            if (ParagraphTokenLimit > 0 && ParagraphTokenLimit <= 1) { return true; }
            if (SentenceIATELimit > 0 && SentenceIATELimit <= 1) { return true; }
            if (SentenceEVLimit > 0 && SentenceEVLimit <= 1) { return true; }
            if (UseDocumentSimilarity > 0 && UseDocumentSimilarity <= 1) { return true; }
            if (UseSectionSimilarity > 0 && UseSectionSimilarity <= 1) { return true; }

            return false;
        }

    }
}