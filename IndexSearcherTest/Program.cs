using Semantika.Marcell.Data;
using Semantika.Marcell.LuceneStore.Index;
using Semantika.Marcell.LuceneStore.Query;
using Semantika.Marcell.LuceneStore.Query.Impl;

namespace IndexSearcherTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var indexManager = new IndexManager(IndexingMode.SingleIndex, @"D:\Code\Marcel\Index\637357511578389192");

            using (SimpleTextSearch searchText = new SimpleTextSearch(indexManager))
            {
                var result = searchText.PerformSearch<Paragraph>(new SimpleQuery
                {
                    Language = "sl",
                    QueryString = "zakon",
                    SearchIn = IndexObjectType.ParagraphIndex
                });
            }
        }
    }
}