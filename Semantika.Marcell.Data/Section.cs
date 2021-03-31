using System;
using System.Collections.Generic;
using System.Text;

namespace Semantika.Marcell.Data
{
    public class Section : IMarcellNestedEntity
    {
        public Guid InternalId { get; set; }
        public Guid ParentId { get; set; }
        public string Id { get; set; }
        public SimilarityData DocumentSimilarityData { get; set; } = new SimilarityData();
        public SimilarityData SectionSimilarityData { get; set; } = new SimilarityData();
        public List<Section> RelatedSections { get; set; } = new List<Section>();
        public double RecognitionQuality { get; set; }
        public int TokenCount { get; set; }
        public SectionType Type { get; set; }
        public string Language { get; set; }
        public StringBuilder TextStringBuilder { get; set; } = new StringBuilder();

        public string Text
        {
            get
            {
                return TextStringBuilder.ToString();
            }

            set
            {
                TextStringBuilder = new StringBuilder(value);
            }
        }

        public List<Paragraph> Paragraphs { get; set; } = new List<Paragraph>();
        public string ArticleNumber { get; set; }
    }
}