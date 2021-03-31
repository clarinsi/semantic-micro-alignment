using Semantika.Marcell.LuceneStore.Index;

namespace Semantika.Marcell.LuceneStore.Query
{
    public class SimpleQuery : ILanguageQuery
    {
        public string QueryString { get; set; }
        public string Language { get; set; }
        public IndexObjectType SearchIn { get; set; }
    }
}