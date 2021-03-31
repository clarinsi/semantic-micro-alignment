using System;
using System.Collections.Generic;

namespace Semantika.Marcell.Data
{
    public class Document : IMarcellEntity
    {
        public Guid InternalId { get; set; }
        public Guid ParentId { get; set; }
        public string Id { get; set; }
        public string FileName { get; set; }
        public SimilarityData DocumentSimilarityData { get; set; } = new SimilarityData();
        public List<Document> RelatedDocuments { get; set; } = new List<Document>();
        public double RecognitionQuality { get; set; }
        public int TokenCount { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime ApprovalDate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string DocumentType { get; set; }
        public string OriginalType { get; set; }
        public string Issuer { get; set; }
        public string Language { get; set; }
        public string Url { get; set; }
        public bool IsStructured { get; set; }
        public string Text { get; set; }
        public List<Section> Sections { get; set; } = new List<Section>();

        public string InferredTitle
        {
            get; set;
        }
    }
}