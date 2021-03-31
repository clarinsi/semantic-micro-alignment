////////////////////////////////////////////////////////////////////////////////////////////////////
// file:	Index\IndexManager.cs
//
// summary:	Implements the Lucene index manager class and helper classes
////////////////////////////////////////////////////////////////////////////////////////////////////
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Index.Extensions;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Semantika.Marcell.LuceneStore.Index
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Values that represent different supported index types within the project. </summary>
    ///
    /// <remarks>   Semantika d.o.o.,. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    [Flags]
    public enum IndexObjectType
    {
        DocumentIndex = 1,
        SectionIndex = 2,
        ParagraphIndex = 4,
        SentenceIndex = 8
    }

    public struct IndexType
    {
        public IndexType(string language, IndexObjectType objectType)
        {
            Language = language;
            ObjectType = objectType;
        }

        public string Language { get; set; }
        public IndexObjectType ObjectType { get; set; }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Language.GetHashCode();
                hash = hash * 23 + ObjectType.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is IndexType))
            {
                return false;
            }

            var castObj = (IndexType)obj;
            return Language == castObj.Language && ObjectType == castObj.ObjectType;
        }

        public override string ToString()
        {
            return $"{ObjectType}_{Language}";
        }

        public static bool operator ==(IndexType left, IndexType right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IndexType left, IndexType right)
        {
            return !(left == right);
        }
    }

    public enum IndexingMode
    {
        SingleIndex = 0,
        IndexPerLanguage = 1
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   A structure for storing Luene objects for working with a s ingle index. </summary>
    ///
    /// <remarks>   Semantika d.o.o.,. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public class IndexManagementObjects
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Reference to the directory containing the index file. </summary>
        ///
        /// <value> The pathname of the directory. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public FSDirectory Directory { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The Lucene index writer instance for the index. Since we are using NRTS, it is also
        /// responsible for creating reader instances.
        /// </summary>
        ///
        /// <value> The Lucene index writer. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public IndexWriter Writer { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The Lucene index reader instance for the main index. Created from the writer instance by
        /// helper methods to support NRTS.
        /// </summary>
        ///
        /// <value> The Lucene index reader. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public IndexReader Reader { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Dictionary containing all of the index reader instances and keeps track of any reader that
        /// need to be kept open because of active transactions.
        /// </summary>
        ///
        /// <value> The old readers that were still being used at the time of a writer closing. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public IDictionary<IndexReader, int> OldReaders { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the number of the users currently using the active index reader. </summary>
        ///
        /// <value> The current number of users. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public int CurrentReaderUsers { get; set; }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The index manager takes care of low level index operations, such as:
    ///  - opening and closing the index
    ///  - ensuring that index readers are refreshed after changes to the data
    ///  - ensuring that index readers are not closed while they are still being used
    ///  - ensuring that commits and index closing happens in the right order
    ///
    /// There should be one index manager per index during the whole lifecycle of the application. It
    /// is the responsibility of the application to ensure that this is observed.
    /// </summary>
    ///
    /// <remarks>   Semantika d.o.o.,. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public class IndexManager
    {
        private static readonly string[] m_supportedLanguages = { "bg", "hr", "hu", "pl", "ro", "sk", "sl" };

        public static string[] SupportedLanguages
        {
            get
            {
                return m_supportedLanguages;
            }
        }

        private readonly TaskScheduler m_taskScheduler;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Instances of the Lucene objects for each of the supported indexes. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private IDictionary<IndexType, IndexManagementObjects> m_managementObjects = new ConcurrentDictionary<IndexType, IndexManagementObjects>();

        /// <summary>
        /// Stores index searchers that have already been accessed for readers.
        /// </summary>
        private IDictionary<IndexReader, IndexSearcher> m_searcherCache = new ConcurrentDictionary<IndexReader, IndexSearcher>();

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Cache of the list of types of the indexes supported by the application. </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private readonly IndexType[] m_indexTypes;

        private readonly IndexingMode m_indexingMode;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The event hook for handling an index refresh after an NRTS addition / update / deletion.
        /// </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public event EventHandler<EventArgs> IndexRefreshed;

        public bool IsReadOnly { get; private set; }

        private static readonly string m_singleIndexLangConstant = "all";

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Create a new instance of the index manager. There should always be only one instance of the
        /// index manager.
        /// </summary>
        ///
        /// <remarks>   Semantika d.o.o.,. </remarks>
        ///
        /// <param name="indexLocation">  The configuration of the index. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public IndexManager(IndexingMode indexingMode, string indexLocation, bool isReadOnly = false)
        {
            //Go through all index types and create the management structure for individual indexes
            var indexObjectTypes = Enum.GetValues(typeof(IndexObjectType)).Cast<IndexObjectType>().ToArray();
            m_indexingMode = indexingMode;

            if (indexingMode == IndexingMode.IndexPerLanguage)
            {
                m_indexTypes = new IndexType[indexObjectTypes.Length * m_supportedLanguages.Length];
                for (int i = 0; i < m_supportedLanguages.Length; i++)
                {
                    for (int j = 0; j < indexObjectTypes.Length; j++)
                    {
                        m_indexTypes[i * indexObjectTypes.Length + j] = new IndexType
                        {
                            Language = m_supportedLanguages[i],
                            ObjectType = indexObjectTypes[j]
                        };
                    }
                }
            }
            else
            {
                m_indexTypes = new IndexType[indexObjectTypes.Length];
                for (int j = 0; j < indexObjectTypes.Length; j++)
                {
                    m_indexTypes[j] = new IndexType
                    {
                        Language = m_singleIndexLangConstant,
                        ObjectType = indexObjectTypes[j]
                    };
                }
            }

            foreach (IndexType curType in m_indexTypes)
            {
                var directoryPath = indexLocation + "\\FTS_" + curType.ToString();
                IndexManagementObjects indexManagementObjects = new IndexManagementObjects
                {
                    CurrentReaderUsers = 0,
                    Directory = FSDirectory.Open(directoryPath),
                    OldReaders = new ConcurrentDictionary<IndexReader, int>()
                };

                m_managementObjects[curType] = indexManagementObjects;
            }
            IsReadOnly = isReadOnly;
            m_taskScheduler = TaskScheduler.Default;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The monitor object for call synchronization of all methods that operate over same objects on
        /// the file I/O level.
        /// </summary>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        //private readonly object m_indexIOLock = new object();

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the index types arrays in this collection. </summary>
        ///
        /// <remarks>   Semantika d.o.o.,. </remarks>
        ///
        /// <param name="indexType">  (Optional) Type of the index. </param>
        ///
        /// <returns>
        /// An enumerator that allows foreach to be used to process the index types arrays in this
        /// collection.
        /// </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private IEnumerable<IndexType> GetIndexTypesArray(IndexType? indexType = null)
        {
            IEnumerable<IndexType> typesToOpen;
            if (indexType == null)
            {
                typesToOpen = m_indexTypes;
            }
            else
            {
                var tmpIndexType = indexType.Value;
                if (m_indexingMode == IndexingMode.SingleIndex)
                {
                    tmpIndexType.Language = m_singleIndexLangConstant;
                }
                typesToOpen = new IndexType[] { tmpIndexType };
            }

            return typesToOpen;
        }

        private IndexType ApplyIndexingMode(IndexType indexType)
        {
            var tmpIndexType = indexType;
            if (m_indexingMode == IndexingMode.SingleIndex)
            {
                tmpIndexType.Language = m_singleIndexLangConstant;
            }
            return tmpIndexType;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Ensures that open writer. </summary>
        ///
        /// <remarks>   Semantika d.o.o.,. </remarks>
        ///
        /// <param name="recreateIndex">  (Optional) True to recreate index. </param>
        /// <param name="indexType">      (Optional) Type of the index. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void EnsureOpenWriter(bool recreateIndex = false, IndexType? indexType = null)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("The Index has been open as read only!");
            }

            //If we get the type of index we need, we open just that type, otherwise we open all supported types
            IEnumerable<IndexType> typesToOpen = GetIndexTypesArray(indexType);

            //lock (m_indexIOLock)
            {
                //Go through al lrequested index types
                foreach (var currentIndexType in typesToOpen)
                {
                    var currentManagementObjects = m_managementObjects[currentIndexType];

                    //Create writer instances
                    if (currentManagementObjects.Writer == null || currentManagementObjects.Writer.IsClosed)
                    {
                        //Configure the basic index configuration to use the main analyzer, and commit everything on close
                        IndexWriterConfig iwic = new IndexWriterConfig(LuceneVersion.LUCENE_48, new StandardAnalyzer(LuceneVersion.LUCENE_48));
                        if (recreateIndex)
                        {
                            iwic.SetOpenMode(OpenMode.CREATE);
                        }
                        else
                        {
                            iwic.SetOpenMode(OpenMode.CREATE_OR_APPEND);
                        }
                        iwic.SetMaxThreadStates(12);
                        currentManagementObjects.Writer = new IndexWriter(currentManagementObjects.Directory, iwic);
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Returns the index writer for the main index. </summary>
        ///
        /// <remarks>   Semantika d.o.o.,. </remarks>
        ///
        /// <param name="indexType">  Type of the index. </param>
        ///
        /// <returns>   The index writer for the main index. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public IndexWriter GetIndexWriter(IndexType indexType)
        {
            var tmpIndexType = ApplyIndexingMode(indexType);
            EnsureOpenWriter(false, tmpIndexType);

            return m_managementObjects[tmpIndexType].Writer;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Returns an instance of the index reader based on the writer supporting NRTS for the main
        /// index. Please note that this method will also open a taxonomy reader at the same time, if it
        /// is not yet open.
        /// </summary>
        ///
        /// <remarks>   Semantika d.o.o.,. </remarks>
        ///
        /// <param name="indexType">  Type of the index. </param>
        ///
        /// <returns>
        /// Main index reader instance containing the latest NRTS changes and a coresponding taxonomy
        /// reader.
        /// </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public IndexReader GetIndexReader(IndexType indexType)
        {
            var tmpIndexType = ApplyIndexingMode(indexType);
            var currentManagementObjects = m_managementObjects[tmpIndexType];

            //Make sure that only one thread at a time is manipulating reader instances
            //lock (m_indexIOLock)
            {
                //Check if we have an instance of the main index reader
                if (currentManagementObjects.Reader == null)
                {
                    if (IsReadOnly)
                    {
                        currentManagementObjects.Reader = DirectoryReader.Open(currentManagementObjects.Directory);
                    }
                    else
                    {
                        //If we do not yet have an instance of the reader, we need to make sure we have a functional writer first
                        if (currentManagementObjects.Writer == null || currentManagementObjects.Writer.IsClosed)
                        {
                            //If we did not yet have an open writer, we need to open one.
                            EnsureOpenWriter();
                        }

                        //Since we had no open reader, we can now open a new one which will include all of the NRTS changes
                        currentManagementObjects.Reader = DirectoryReader.Open(currentManagementObjects.Writer, true);
                    }
                }
                else
                {
                    if (!IsReadOnly)
                    {
                        //We already have a previous instance of the reader - make sure that includes all of the changes done to the index in the meantime
                        DirectoryReader newReader;
                        try
                        {
                            newReader = DirectoryReader.OpenIfChanged((DirectoryReader)currentManagementObjects.Reader, currentManagementObjects.Writer, true);
                        }
                        catch (IOException)
                        {
                            if (currentManagementObjects.Writer == null || currentManagementObjects.Writer.IsClosed)
                            {
                                //If we did not yet have an open writer, we need to open one.
                                EnsureOpenWriter();
                            }
                            newReader = DirectoryReader.Open(currentManagementObjects.Writer, true);
                        }
                        if (newReader != null)
                        {
                            //There was a change in the index since the last time reader was accessed - create a new instance and close the old one
                            if (currentManagementObjects.CurrentReaderUsers == 0)
                            {
                                //No one is using the reader at the moment - close it
                                currentManagementObjects.Reader.Dispose();
                            }
                            else
                            {
                                //We have searchers using the reader - add it to cleanup queue
                                currentManagementObjects.OldReaders[currentManagementObjects.Reader] = currentManagementObjects.CurrentReaderUsers;
                            }

                            currentManagementObjects.CurrentReaderUsers = 0;
                            currentManagementObjects.Reader = newReader;

                            //Inform listeners that the index was refreshed
                            IndexRefreshed?.Invoke(this, new EventArgs());
                        }
                    }
                }

                currentManagementObjects.CurrentReaderUsers++;

                //Return the created / refreshed instance of the reader to the caller and exit critical section
                return currentManagementObjects.Reader;
            }
        }

        public IndexSearcher GetSearcher(IndexReader indexReader)
        {
            if (m_searcherCache.TryGetValue(indexReader, out IndexSearcher cachedSearcher))
            {
                return cachedSearcher;
            }
            else
            {
                //lock (m_indexIOLock)
                {
                    var newSearcher = new IndexSearcher(indexReader);
                    m_searcherCache[indexReader] = newSearcher;
                    return newSearcher;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Closes and pending index readers marked for closing and disposes the objects.
        /// </summary>
        ///
        /// <remarks>   Semantika d.o.o.,. </remarks>
        ///
        /// <param name="indexType">          Type of the index. </param>
        /// <param name="readerToRelease">    The reader instance to be released. </param>
        ///
        /// ### <param name="taxonomyReaderToRelease">  The taxonomy reader instance to be released. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public void ReleaseReaderInstance(IndexType indexType, IndexReader readerToRelease)
        {
            var tmpIndexType = ApplyIndexingMode(indexType);
            var currentManagementObjects = m_managementObjects[tmpIndexType];

            //We can only release
            //lock (m_indexIOLock)
            {
                if (readerToRelease == currentManagementObjects.Reader)
                {
                    //We are releasing the current reader. Simply decrease the usage counter
                    currentManagementObjects.CurrentReaderUsers--;
                }
                else
                {
                    //We are releasing a reader on the cleanup queue.
                    int readerUsers = 0;
                    if (currentManagementObjects.OldReaders.TryGetValue(readerToRelease, out readerUsers))
                    {
                        readerUsers--;
                        if (readerUsers <= 0)
                        {
                            currentManagementObjects.OldReaders.Remove(readerToRelease);
                            m_searcherCache.Remove(readerToRelease);

                            readerToRelease.Dispose();
                        }
                        else
                        {
                            currentManagementObjects.OldReaders[readerToRelease] = readerUsers;
                        }
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Commit any uncommited data for all index writers and flush data to underlying storage
        /// subsystem.
        /// </summary>
        ///
        /// <remarks>   Semantika d.o.o.,. </remarks>
        ///
        /// <param name="indexType">  (Optional) Type of the index. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public void Commit(IndexType? indexType = null)
        {
            if (IsReadOnly)
            {
                return;
            }

            IEnumerable<IndexType> typesToProcess = GetIndexTypesArray(indexType);

            Parallel.ForEach(typesToProcess, curType =>
            {
                var curManagementObjects = m_managementObjects[curType];
                if (curManagementObjects.Writer != null && !curManagementObjects.Writer.IsClosed)
                {
                    curManagementObjects.Writer.Commit();
                    curManagementObjects.Writer.Flush(true, true);
                }
            });
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Optimize the indexes by merging segments. </summary>
        ///
        /// <remarks>   Semantika d.o.o.,. </remarks>
        ///
        /// <param name="indexType">  (Optional) Type of the index. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public void FullOptimize(IndexType? indexType = null, int maxSegments = 1)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Index is read only!");
            }
            IEnumerable<IndexType> typesToProcess = GetIndexTypesArray(indexType);

            Parallel.ForEach(typesToProcess, curType =>
            {
                var curManagementObjects = m_managementObjects[curType];
                if (curManagementObjects.Writer != null && !curManagementObjects.Writer.IsClosed)
                {
                    curManagementObjects.Writer.ForceMerge(maxSegments);
                    curManagementObjects.Writer.Commit();
                }
            });
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Optimize the indexes by merging segments. </summary>
        ///
        /// <remarks>   Semantika d.o.o.,. </remarks>
        ///
        /// <param name="indexType">  (Optional) Type of the index. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public void TryOptimize(IndexType? indexType = null)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Index is read only!");
            }
            IEnumerable<IndexType> typesToProcess = GetIndexTypesArray(indexType);
            Parallel.ForEach(typesToProcess, curType =>
            {
                var curManagementObjects = m_managementObjects[curType];
                if (curManagementObjects.Writer != null && !curManagementObjects.Writer.IsClosed)
                {
                    curManagementObjects.Writer.MaybeMerge();
                    curManagementObjects.Writer.Commit();
                }
            });
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Force close old readers. </summary>
        ///
        /// <remarks>   Semantika d.o.o.,. </remarks>
        ///
        /// <param name="indexType">  (Optional) Type of the index. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public void ForceCloseOldReaders(IndexType? indexType = null)
        {
            IEnumerable<IndexType> typesToProcess = GetIndexTypesArray(indexType);

            m_searcherCache.Clear();

            foreach (var curType in typesToProcess)
            {
                var curManagementObjects = m_managementObjects[curType];
                //Perform cleanup of all potentialy open old readers / writers
                foreach (var readerToClose in curManagementObjects.OldReaders.Keys)
                {
                    try
                    {
                        readerToClose.Dispose();
                    }
                    catch (Exception)
                    {
                        //Nothing to do if we cannot close the reader
                    }
                }

                //Clear all of the old readers
                curManagementObjects.OldReaders.Clear();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Close all index readers and writers. After calling this method, a new index will need to be
        /// opened in case any indexing or querying operations are required.
        /// </summary>
        ///
        /// <remarks>   Semantika d.o.o.,. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public void CloseIndex()
        {
            //Close any lingering old readers
            try
            {
                ForceCloseOldReaders();
            }
            catch (Exception)
            {
            }

            foreach (var curType in m_indexTypes)
            {
                var curManagementObjects = m_managementObjects[curType];

                //Close the main index and sidecar index readers and writers
                //lock (m_indexIOLock)
                {
                    if (curManagementObjects.Reader != null)
                    {
                        try
                        {
                            if (curManagementObjects.Reader != null)
                            {
                                curManagementObjects.Reader.Dispose();
                            }
                        }
                        catch (Exception) { }
                        curManagementObjects.Reader = null;
                    }

                    if (!IsReadOnly && curManagementObjects.Writer != null)
                    {
                        try
                        {
                            curManagementObjects.Writer.ForceMergeDeletes();
                            curManagementObjects.Writer.Commit();
                            curManagementObjects.Writer.Flush(true, true);
                            curManagementObjects.Writer.Dispose();
                        }
                        catch (Exception)
                        {
                            //Not much we can do at this point, also not expected to cause any problems with the index as we are performing regular commits anyway
                            //In case of data loss, the data will be rebuilt by the reindexing job detecting missing data in the index
                        }
                        curManagementObjects.Writer = null;
                    }

                    if (curManagementObjects.Directory != null)
                    {
                        try
                        {
                            curManagementObjects.Directory.Dispose();
                        }
                        catch (Exception)
                        {
                            //Not much we can do at this point, also not expected to cause any problems with the index as we are performing regular commits anyway
                            //In case of data loss, the data will be rebuilt by the reindexing job detecting missing data in the index
                        }
                        curManagementObjects.Directory = null;
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// A helper method to delete an index files that are no longer in use due to index merges.
        /// </summary>
        ///
        /// <remarks>   Semantika d.o.o.,. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public void CleanOldFiles()
        {
            foreach (var curType in m_indexTypes)
            {
                m_managementObjects[curType].Writer.DeleteUnusedFiles();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Clean up and close resources if the index manager is destroyed. </summary>
        ///
        /// <remarks>   Semantika d.o.o.,. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        ~IndexManager()
        {
            //Close any readers that may remain open after object finalization
            try
            {
                ForceCloseOldReaders();
            }
            catch (Exception)
            {
                //Nothing we can handle - just finalize the object
            }
        }
    }
}