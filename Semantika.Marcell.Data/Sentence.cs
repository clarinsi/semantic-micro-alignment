using System;
using System.Collections.Generic;

namespace Semantika.Marcell.Data
{
    public class Sentence : IMarcellNestedEntity
    {
        public Guid InternalId { get; set; }
        public Guid ParentId { get; set; }
        public string Id { get; set; }
        public SimilarityData DocumentSimilarityData { get; set; } = new SimilarityData();
        public SimilarityData SectionSimilarityData { get; set; } = new SimilarityData();
        public SimilarityData ParagraphSimilarityData { get; set; } = new SimilarityData();
        public SimilarityData SentenceSimilarityData { get; set; } = new SimilarityData();
        public double RecognitionQuality { get; set; }
        public int TokenCount { get; set; }
        public List<Sentence> RelatedSentences { get; set; } = new List<Sentence>();
        public string Text { get; set; }
        public string Language { get; set; }
        public List<Token> Tokens { get; set; } = new List<Token>();
        public int Order { get; set; }
    }
}