using Lucene.Net.Documents;
using Lucene.Net.Index;
using Semantika.Marcell.LuceneStore.Index;
using Semantika.Marcell.LuceneStore.Indexer;
using Semantika.Marcell.Processor.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using TBXFormat;
using MarcellDocument = Semantika.Marcell.Data.Document;

namespace Semantika.Marcell.Processor.Indexer
{
    public class CorpusIndexManager
    {
        private readonly string m_rootSourceDirectory;
        private readonly string m_rootIndexDirectory;
        private readonly string m_logLocation;
        private readonly IndexingMode m_indexingMode;

        private readonly LegalTextParserFactory m_parserFactory;
        private Semantika.Marcell.LuceneStore.Index.IndexManager m_luceneIndexManager;
        private Dictionary<string, IndexWriter> m_documentWriter = new Dictionary<string, IndexWriter>();
        private Dictionary<string, IndexWriter> m_sectionWriter = new Dictionary<string, IndexWriter>();
        private Dictionary<string, IndexWriter> m_paragraphWriter = new Dictionary<string, IndexWriter>();
        private Dictionary<string, IndexWriter> m_sentenceWriter = new Dictionary<string, IndexWriter>();

        private readonly bool m_readOnly;

        private void SetupWriters()
        {
            m_luceneIndexManager = new LuceneStore.Index.IndexManager(m_indexingMode, m_rootIndexDirectory);

            foreach (var lang in LuceneStore.Index.IndexManager.SupportedLanguages)
            {
                m_documentWriter[lang] = m_luceneIndexManager.GetIndexWriter(new LuceneStore.Index.IndexType(lang, LuceneStore.Index.IndexObjectType.DocumentIndex));
                m_sectionWriter[lang] = m_luceneIndexManager.GetIndexWriter(new LuceneStore.Index.IndexType(lang, LuceneStore.Index.IndexObjectType.SectionIndex));
                m_paragraphWriter[lang] = m_luceneIndexManager.GetIndexWriter(new LuceneStore.Index.IndexType(lang, LuceneStore.Index.IndexObjectType.ParagraphIndex));
                m_sentenceWriter[lang] = m_luceneIndexManager.GetIndexWriter(new LuceneStore.Index.IndexType(lang, LuceneStore.Index.IndexObjectType.SentenceIndex));
            }
        }

        public CorpusIndexManager(IndexingMode indexingMode, string rootDirectory, string indexDirectory, string tbxIateLocation = null, string logLocation = null, int? parallelIndexingLimit = null, bool readOnly = true)
        {
            m_readOnly = readOnly;
            m_indexingMode = indexingMode;

            if (!Directory.Exists(rootDirectory))
            {
                throw new ArgumentException("The source directory must exist!");
            }

            /*if (Directory.Exists(indexDirectory) && !Directory.EnumerateFileSystemEntries(indexDirectory).GetEnumerator().MoveNext())
            {
                throw new ArgumentException("The indexing directory must be empty!");
            }*/

            m_rootSourceDirectory = rootDirectory;

            if (logLocation != null)
            {
                m_logLocation = logLocation;
                if (!m_logLocation.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    m_logLocation += Path.DirectorySeparatorChar;
                }
                if (!Directory.Exists(m_logLocation))
                {
                    Directory.CreateDirectory(m_logLocation);
                }
            }

            m_rootIndexDirectory = indexDirectory;
            if (!m_rootIndexDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                m_rootIndexDirectory += Path.DirectorySeparatorChar;
            }
            if (!Directory.Exists(m_rootIndexDirectory))
            {
                Directory.CreateDirectory(m_rootIndexDirectory);
            }

            if (tbxIateLocation != null)
            {
                Tbx sourceTbx = null;
                var serializerTbx = new System.Xml.Serialization.XmlSerializer(typeof(Tbx));
                using (var xmlReader = XmlReader.Create(tbxIateLocation))
                {
                    sourceTbx = (Tbx)serializerTbx.Deserialize(xmlReader);
                }

                m_parserFactory = new LegalTextParserFactory(sourceTbx);
            }

            SetupWriters();
        }

        private void AddDocument(MarcellDocument document, bool isParallel = false)
        {
            List<Document> sectionDocument = new List<Document>();
            List<Document> paragraphDocument = new List<Document>();
            List<Document> sentenceDocument = new List<Document>();

            foreach (var section in document.Sections)
            {
                sectionDocument.Add(section.ToLucene(document));
                foreach (var paragraph in section.Paragraphs)
                {
                    paragraphDocument.Add(paragraph.ToLucene(document, section));
                    foreach (var sentence in paragraph.Sentences)
                    {
                        sentenceDocument.Add(sentence.ToLucene(document, section, paragraph));
                    }
                }
            }

            m_documentWriter[document.Language].AddDocument(document.ToLucene());

            var t1 = Task.Factory.StartNew(() => { m_sectionWriter[document.Language].AddDocuments(sectionDocument); }, TaskCreationOptions.LongRunning);
            var t2 = Task.Factory.StartNew(() => { m_paragraphWriter[document.Language].AddDocuments(paragraphDocument); }, TaskCreationOptions.LongRunning);
            var t3 = Task.Factory.StartNew(() => { m_sentenceWriter[document.Language].AddDocuments(sentenceDocument); }, TaskCreationOptions.LongRunning);

            Task.WaitAll(t1, t2, t3);
        }

        private void IndexDocument(string file, bool isParallel = false)
        {
            try
            {
                var parsedDoc = new ParsedDocument(file, m_parserFactory);
                parsedDoc.Document.FileName = file;
                AddDocument(parsedDoc.Document, isParallel);
            }
            catch (Exception ex)
            {
                string logFileName = Path.GetFileNameWithoutExtension(file);

                File.WriteAllText($"{m_logLocation}{logFileName}-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}{DateTime.Now.Hour}{DateTime.Now.Minute}.txt",
                    $@"Error occured while parsing: {file}
                       Error: {ex.Message}
                       Stack: {ex.StackTrace}
                    "
                    );
            }
        }

        public void PerformSequentialIndex()
        {
            if (m_readOnly || m_parserFactory == null || m_logLocation == null)
            {
                throw new Exception("Index open in read-only mode");
            }

            string[] allfiles = Directory.GetFiles(m_rootSourceDirectory, "*.xml", SearchOption.AllDirectories);
            for (int i = 0; i < allfiles.Length; i++)
            {
                IndexDocument(allfiles[i]);
                if (i % 1000 == 0)
                {
                    Console.WriteLine("{0}: Processed {1} files out of {2}.", DateTime.Now, i, allfiles.Length);
                    m_luceneIndexManager.Commit();
                }

                if (i % 10000 == 0)
                {
                    m_luceneIndexManager.FullOptimize();
                }

                if (i % 25000 == 0)
                {
                    m_luceneIndexManager.Commit();
                    m_luceneIndexManager.FullOptimize();
                    m_luceneIndexManager.CloseIndex();
                }
            }
        }

        public void PerformParallelIndex()
        {
            if (m_readOnly || m_parserFactory == null || m_logLocation == null)
            {
                throw new Exception("Index open in read-only mode");
            }
            int stepSize = 800;
            int threadCount = 8;
            Console.WriteLine("{0}: Started indexing.", DateTime.Now);

            string[] dirs = Directory.GetDirectories(m_rootSourceDirectory);
            string[][] allfiles = new string[dirs.Length][];
            List<Task> allThreads = new List<Task>();

            //Get the maximum number of files in directory
            int maxNum = 0;
            for (int i = 0; i < dirs.Length; i++)
            {
                allfiles[i] = Directory.GetFiles(dirs[i], "*.xml", SearchOption.AllDirectories);
                maxNum = (int)Math.Max(allfiles[i].Length, maxNum);
            }

            for (int iStart = 0; iStart < maxNum; iStart = iStart + stepSize)
            {
                for (int d = 0; d < dirs.Length; d++)
                {
                    int currentDir = d;

                    if (iStart > allfiles[d].Length)
                    {
                        continue;
                    }

                    int batchSize = stepSize / threadCount;
                    //Chunk up the array into smaller segments for parallel processing

                    for (int th = 0; th < threadCount; th++)
                    {
                        int lowerLimit = iStart + th * batchSize;
                        int upperLimit = Math.Min(allfiles[currentDir].Length, iStart + (th + 1) * batchSize);
                        if (th == threadCount - 1)
                        {
                            upperLimit = Math.Min(allfiles[currentDir].Length, iStart + stepSize);
                        }

                        var task = Task.Factory.StartNew(() =>
                         {
                             Console.WriteLine("{0}: Processing files {1} - {2} in directory: {3}.", DateTime.Now, lowerLimit, upperLimit, dirs[currentDir]);

                             for (int i = lowerLimit; i < upperLimit; i++)
                             {
                                 IndexDocument(allfiles[currentDir][i]);
                             }

                             Console.WriteLine("{0}: Processed {1} files out of {2} for {3}.", DateTime.Now, lowerLimit, upperLimit, dirs[currentDir]);
                         }, TaskCreationOptions.LongRunning);
                        allThreads.Add(task);
                    }
                }

                //Spin off all threads and perform work
                Console.WriteLine("{0}: Waiting for {1} threads to complete.", DateTime.Now, allThreads.Count);
                //Wait for all threads to finish
                Task.WaitAll(allThreads.ToArray());
                allThreads.Clear();

                m_luceneIndexManager.Commit();
                m_luceneIndexManager.FullOptimize(maxSegments: 12);
                m_luceneIndexManager.CloseIndex();
                SetupWriters();
            }

            Console.WriteLine("{0}: Performing final commit and full index optimization.", DateTime.Now);
            m_luceneIndexManager.Commit();
            m_luceneIndexManager.FullOptimize(maxSegments: 8);
            m_luceneIndexManager.CloseIndex();

            Console.WriteLine("{0}: Finished.", DateTime.Now);
        }
    }
}