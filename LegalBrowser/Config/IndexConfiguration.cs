using Microsoft.Extensions.Configuration;
using Semantika.Marcell.LuceneStore.Index;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegalBrowser.Config
{
    /// <summary>
    /// Basic configuration of the index, specifying the mode used for indexing and the location of the index.
    /// </summary>
    public class IndexConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexConfiguration"/> class.
        /// </summary>
        /// <param name="configurationProvider">The configuration provider to use for reading the settings.</param>
        public IndexConfiguration(IConfiguration configurationProvider)
        {
            var indexingMode = configurationProvider.GetSection("IndexConfig").GetValue<int>("IndexMode");
            var indexPath = configurationProvider.GetSection("IndexConfig").GetValue<string>("IndexPath");

            IndexingMode = (IndexingMode)indexingMode;
            IndexPath = indexPath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexConfiguration"/> class.
        /// </summary>
        /// <param name="indexingMode">The indexing mode used by Lucene index.</param>
        /// <param name="indexPath">The index path, where the Lucene index is located.</param>
        public IndexConfiguration(IndexingMode indexingMode, string indexPath)
        {
            IndexingMode = indexingMode;
            IndexPath = indexPath;
        }

        /// <summary>
        /// Gets the indexing mode that is used for indexing.
        /// Two modes are currently supported: single index per entity and index per langauge.
        /// </summary>
        public IndexingMode IndexingMode { get; private set; }

        /// <summary>
        /// Gets the path to the Lucene index location.
        /// </summary>
        public string IndexPath { get; private set; }
    }
}
