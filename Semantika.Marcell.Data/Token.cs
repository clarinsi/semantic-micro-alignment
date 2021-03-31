using System;
using System.Collections.Generic;

namespace Semantika.Marcell.Data
{
    public class Token
    {
        public Guid InternalId { get; set; }
        public string Id { get; set; }
        public int Order { get; set; }
        public SimilarityData SimilarityData { get; set; } = new SimilarityData();
        public string Form { get; set; }
        public string Lemma { get; set; }
        public string GeneralPos { get; set; }
        public string LanguagePos { get; set; }
        public Dictionary<string, string> Features { get; set; } = new Dictionary<string, string>();
        public string HeadId { get; set; }
        public string Deprel { get; set; }
        public string Deps { get; set; }
        public string Misc { get; set; }
        public string NE { get; set; }
        public string NP { get; set; }
        public string Language { get; set; }
        public List<string> IateEntities { get; set; } = new List<string>();
        public List<string> EuroVocEntities { get; set; } = new List<string>();
        public List<string> IateDomains { get; set; } = new List<string>();
    }
}