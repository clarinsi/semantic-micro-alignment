using System.Collections.Generic;

namespace Semantika.Marcell.Data
{
    public class SimilarityData
    {
        public List<string> ConsolidatedTopics { get; set; } = new List<string>();
        public List<string> ConsolidatedTokens { get; set; } = new List<string>();
    }
}