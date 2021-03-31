using Semantika.Marcell.Data;
using System.Collections.Generic;

namespace Semantika.Marcell.LuceneStore.Query
{
    public class Result<T> where T : IMarcellEntity
    {
        public int TotalResults { get; set; }
        public List<T> ResultList { get; set; }
    }
}