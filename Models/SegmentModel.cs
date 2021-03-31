using LegalBrowser.Controllers;
using LegalBrowser.Data.Repositories.Lucene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegalBrowser.Models
{
    public class SegmentModel
    {
        public string Id { get; set; }
        public string SourceLang { get; set; }
        public string TargetLang { get; set; }
        public SegmentType SegType { get; set; }
        public ParametrizedSearchParameters Parameters { get; set; }
    }
}
