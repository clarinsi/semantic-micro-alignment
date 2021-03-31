using System;
using System.Collections.Generic;

namespace Semantika.Marcell.Data
{
    public class Paragraph : IMarcellNestedEntity
    {
        public Guid InternalId { get; set; }
        public Guid ParentId { get; set; }
        public string Id { get; set; }
        public SimilarityData DocumentSimilarityData { get; set; } = new SimilarityData();
        public SimilarityData SectionSimilarityData { get; set; } = new SimilarityData();
        public SimilarityData ParagraphSimilarityData { get; set; } = new SimilarityData();
        public double RecognitionQuality { get; set; }
        public int TokenCount { get; set; }
        public List<Section> RelatedSections { get; set; } = new List<Section>();
        public List<Paragraph> RelatedParagraphs { get; set; } = new List<Paragraph>();
        public List<Sentence> Sentences { get; set; } = new List<Sentence>();
        public ParagraphType ParagraphType { get; set; }
        public string Language { get; set; }
        public string ParagraphNumber { get; set; }
        public string PointNumber { get; set; }
        public string Text { get; set; }
        public int Order { get; set; }
        public bool IsMatch { get; set; }
    }
}