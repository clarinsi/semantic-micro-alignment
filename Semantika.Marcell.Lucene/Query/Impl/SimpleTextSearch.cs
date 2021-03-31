using Lucene.Net.Queries;
using Lucene.Net.QueryParsers.Classic;
using Semantika.Marcell.LuceneStore.Index;
using Semantika.Marcell.LuceneStore.Indexer;
using System;
using System.Collections.Generic;
using LuceneNet = Lucene.Net;

namespace Semantika.Marcell.LuceneStore.Query.Impl
{
    public class SimpleTextSearch : Search<SimpleQuery>
    {
        private static readonly QueryParser qpSimple;

        static SimpleTextSearch()
        {
            qpSimple = new QueryParser(LuceneNet.Util.LuceneVersion.LUCENE_48, "Text", m_basicAnalyzer);
            qpSimple.DefaultOperator = Operator.AND;
        }

        public SimpleTextSearch(IndexManager indexmanager) : base(indexmanager)
        {
        }

        public override LuceneNet.Search.Query CalculateLuceneQuery(SimpleQuery query, ref IndexObjectType searchType)
        {
            searchType = query.SearchIn;
            return qpSimple.Parse(query.QueryString);
        }

        public override Result<T> PerformSearch<T>(SimpleQuery query, int pageSize = 10, int pageNumber = 1)
        {

            LuceneNet.Search.Query parsedQuery;

            if (string.IsNullOrEmpty(query.QueryString))
            {
                qpSimple.AllowLeadingWildcard = true;
                parsedQuery = qpSimple.Parse("*");
            }
            else
            {
                parsedQuery = qpSimple.Parse(query.QueryString);
            }

            var searcher = GetAppropriateSearcher(query.Language, query.SearchIn);
            var results = searcher.Search(parsedQuery, new TermFilter(new LuceneNet.Index.Term("Language", query.Language)), pageNumber * pageSize);
            Result<T> result = new Result<T>
            {
                TotalResults = results.TotalHits,
                ResultList = new List<T>(pageSize)
            };

            for (int i = (pageNumber - 1) * pageSize; i < Math.Min(pageNumber * pageSize, results.TotalHits); i++)
            {
                result.ResultList.Add(GetEntityHierarchy<T>(searcher.Doc(results.ScoreDocs[i].Doc).ToEntity(GetTypeFromObjectType(query.SearchIn))));
            }

            return result;
        }
    }
}