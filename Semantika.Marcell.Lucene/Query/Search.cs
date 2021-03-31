using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Semantika.Marcell.Data;
using Semantika.Marcell.LuceneStore.Index;
using Semantika.Marcell.LuceneStore.Indexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LuceneNet = Lucene.Net;

namespace Semantika.Marcell.LuceneStore.Query
{
    public abstract class Search<TQueryType> : IDisposable where TQueryType : ILanguageQuery
    {
        protected static readonly Analyzer m_basicAnalyzer = new StandardAnalyzer(LuceneNet.Util.LuceneVersion.LUCENE_48);
        protected static readonly QueryParser queryParser = new QueryParser(LuceneNet.Util.LuceneVersion.LUCENE_48, "Text", new StandardAnalyzer(LuceneNet.Util.LuceneVersion.LUCENE_48));

        protected static readonly RandomNumberGenerator m_randomGenerator = RandomNumberGenerator.Create();

        private IndexManager m_indexManager;
        private readonly Dictionary<string, IndexReader> m_documentReader = new Dictionary<string, IndexReader>();
        private readonly Dictionary<string, IndexReader> m_sectionReader = new Dictionary<string, IndexReader>();
        private readonly Dictionary<string, IndexReader> m_paragraphReader = new Dictionary<string, IndexReader>();
        private readonly Dictionary<string, IndexReader> m_sentenceReader = new Dictionary<string, IndexReader>();

        protected readonly Dictionary<string, IndexSearcher> m_documentSearcher = new Dictionary<string, IndexSearcher>();
        protected readonly Dictionary<string, IndexSearcher> m_sectionSearcher = new Dictionary<string, IndexSearcher>();
        protected readonly Dictionary<string, IndexSearcher> m_paragraphSearcher = new Dictionary<string, IndexSearcher>();
        protected readonly Dictionary<string, IndexSearcher> m_sentenceSearcher = new Dictionary<string, IndexSearcher>();

        public bool Disposed
        {
            get
            {
                return disposedValue;
            }
        }

        public Search(IndexManager indexManager)
        {
            if (indexManager == null)
            {
                throw new ArgumentException("You need to provide an instance to the index manager in order to use the searcher!");
            }

            m_indexManager = indexManager;

            foreach (var lang in IndexManager.SupportedLanguages)
            {
                m_documentReader[lang] = m_indexManager.GetIndexReader(new IndexType(lang, IndexObjectType.DocumentIndex));
                m_sectionReader[lang] = m_indexManager.GetIndexReader(new IndexType(lang, IndexObjectType.SectionIndex));
                m_paragraphReader[lang] = m_indexManager.GetIndexReader(new IndexType(lang, IndexObjectType.ParagraphIndex));
                m_sentenceReader[lang] = m_indexManager.GetIndexReader(new IndexType(lang, IndexObjectType.SentenceIndex));

                m_documentSearcher[lang] = m_indexManager.GetSearcher(m_documentReader[lang]);
                m_sectionSearcher[lang] = m_indexManager.GetSearcher(m_sectionReader[lang]);
                m_paragraphSearcher[lang] = m_indexManager.GetSearcher(m_paragraphReader[lang]);
                m_sentenceSearcher[lang] = m_indexManager.GetSearcher(m_sentenceReader[lang]);
            }
        }

        public abstract LuceneNet.Search.Query CalculateLuceneQuery(TQueryType query, ref IndexObjectType searchType);

        public abstract Result<T> PerformSearch<T>(TQueryType query, int pageSize = 10, int pageNumber = 1) where T : class, IMarcellEntity;

        private TermQuery GetInternalIdQuery(Guid internalId)
        {
            return new TermQuery(new Term("InternalId", internalId.ToString("N")));
        }

        private TermQuery GetFieldIdQuery(string fieldName, Guid parentId)
        {
            return new TermQuery(new Term(fieldName, parentId.ToString("N")));
        }

        protected Type GetTypeFromObjectType(IndexObjectType objectType)
        {
            switch (objectType)
            {
                case IndexObjectType.DocumentIndex:
                    return typeof(Document);

                case IndexObjectType.SectionIndex:
                    return typeof(Section);

                case IndexObjectType.ParagraphIndex:
                    return typeof(Paragraph);

                case IndexObjectType.SentenceIndex:
                    return typeof(Sentence);

                default:
                    throw new ArgumentException("Invalid object type requested!");
            }
        }

        protected IndexObjectType GetObjectType(Type resultType)
        {
            IndexObjectType objectType;
            if (typeof(Document).IsAssignableFrom(resultType))
            {
                objectType = IndexObjectType.DocumentIndex;
            }
            else if (typeof(Section).IsAssignableFrom(resultType))
            {
                objectType = IndexObjectType.SectionIndex;
            }
            else if (typeof(Paragraph).IsAssignableFrom(resultType))
            {
                objectType = IndexObjectType.ParagraphIndex;
            }
            else if (typeof(Sentence).IsAssignableFrom(resultType))
            {
                objectType = IndexObjectType.SentenceIndex;
            }
            else
            {
                throw new ArgumentException("Unsupported result type has been requested!");
            }

            return objectType;
        }

        protected IndexSearcher GetAppropriateSearcher(string language, IndexObjectType type)
        {
            var normalizedlang = language?.ToLower();
            if (!IndexManager.SupportedLanguages.Contains(normalizedlang))
            {
                throw new ArgumentException("Unsupported language requested!");
            }

            switch (type)
            {
                case IndexObjectType.DocumentIndex:
                    return m_documentSearcher[language];

                case IndexObjectType.SectionIndex:
                    return m_sectionSearcher[language];

                case IndexObjectType.ParagraphIndex:
                    return m_paragraphSearcher[language];

                case IndexObjectType.SentenceIndex:
                    return m_sentenceSearcher[language];

                default:
                    throw new ArgumentException("Unsupported index object type requested!");
            }
        }

        protected IndexSearcher GetAppropriateSearcher(string language, Type resultType)
        {
            return GetAppropriateSearcher(language, GetObjectType(resultType));
        }

        protected LuceneNet.Documents.Document GetSingleDocumentByQuery(IndexSearcher searcher, LuceneNet.Search.Query query)
        {
            var result = searcher.Search(query, 1);
            if (result.TotalHits > 0)
            {
                return searcher.Doc(result.ScoreDocs[0].Doc);
            }
            else
            {
                return null;
            }
        }

        private const int m_MaxDocs = 50000;

        protected IEnumerable<LuceneNet.Documents.Document> GetDocumentsByQuery(IndexSearcher searcher, LuceneNet.Search.Query query)
        {
            var result = searcher.Search(query, m_MaxDocs);
            if (result.TotalHits > m_MaxDocs)
            {
                throw new Exception("Too many documents returned by query!");
            }
            if (result.TotalHits > 0)
            {
                return result.ScoreDocs.Select(d => searcher.Doc(d.Doc));
            }
            else
            {
                return null;
            }
        }

        protected T GetEntityHierarchy<T>(IMarcellEntity sourceEntity, bool fullLoad = false) where T : class, IMarcellEntity
        {
            if (typeof(T) == typeof(Document))
            {
                if (sourceEntity.GetType() == typeof(Document))
                {
                    return sourceEntity as T;
                }
                else if (sourceEntity.GetType() == typeof(Section))
                {
                    return DocFromSection(sourceEntity.Language, sourceEntity as Section, fullLoad) as T;
                }
                else if (sourceEntity.GetType() == typeof(Paragraph))
                {
                    return DocFromParagraph(sourceEntity.Language, sourceEntity as Paragraph, fullLoad) as T;
                }
                else if (sourceEntity.GetType() == typeof(Sentence))
                {
                    return DocFromSentence(sourceEntity.Language, sourceEntity as Sentence, fullLoad) as T;
                }
                else
                {
                    throw new ArgumentException("Cannot convert from requested type to the requested type.");
                }
            }
            else if (typeof(T) == typeof(Section))
            {
                if (sourceEntity.GetType() == typeof(Section))
                {
                    return sourceEntity as T;
                }
                else if (sourceEntity.GetType() == typeof(Paragraph))
                {
                    return SectionFromParagraph(sourceEntity.Language, sourceEntity as Paragraph) as T;
                }
                else if (sourceEntity.GetType() == typeof(Sentence))
                {
                    return SectionFromSentence(sourceEntity.Language, sourceEntity as Sentence) as T;
                }
                else
                {
                    throw new ArgumentException("Cannot convert from requested type to the requested type.");
                }
            }
            else if (typeof(T) == typeof(Paragraph))
            {
                if (sourceEntity.GetType() == typeof(Paragraph))
                {
                    return sourceEntity as T;
                }
                else if (sourceEntity.GetType() == typeof(Sentence))
                {
                    return ParagraphFromSentence(sourceEntity.Language, sourceEntity as Sentence) as T;
                }
                else
                {
                    throw new ArgumentException("Cannot convert from requested type to the requested type.");
                }
            }
            else if (typeof(T) == typeof(Sentence))
            {
                if (sourceEntity.GetType() == typeof(Sentence))
                {
                    return sourceEntity as T;
                }
                else
                {
                    throw new ArgumentException("Cannot convert from requested type to the requested type.");
                }
            }
            else
            {
                throw new ArgumentException("Requested conversion to unknown type");
            }
        }

        public Document GetDocument(string language, Guid internalId, bool loadFull = false)
        {
            var result = GetSingleDocumentByQuery(m_documentSearcher[language], GetInternalIdQuery(internalId))?.ToDocument();
            if (result != null && loadFull)
            {
                LoadChildren(language, result);
            }
            return result;
        }

        public Section GetSection(string language, Guid internalId)
        {
            return GetSingleDocumentByQuery(m_sectionSearcher[language], GetInternalIdQuery(internalId))?.ToSection();
        }

        public Paragraph GetParagraph(string language, Guid internalId)
        {
            return GetSingleDocumentByQuery(m_paragraphSearcher[language], GetInternalIdQuery(internalId))?.ToParagraph();
        }

        public Sentence GetSentence(string language, Guid internalId)
        {
            return GetSingleDocumentByQuery(m_sentenceSearcher[language], GetInternalIdQuery(internalId))?.ToSentence();
        }

        public List<Section> GetSections(string language, Guid parentId)
        {
            return GetDocumentsByQuery(m_sectionSearcher[language], GetFieldIdQuery("ParentDocumentId", parentId))?.Select(d => d.ToSection()).ToList();
        }

        public List<Paragraph> GetParagraphsBySection(string language, Guid parentId)
        {
            return GetDocumentsByQuery(m_paragraphSearcher[language], GetFieldIdQuery("ParentSectionId", parentId))?.Select(d => d.ToParagraph()).ToList();
        }

        public List<Paragraph> GetParagraphsByDocument(string language, Guid parentId)
        {
            return GetDocumentsByQuery(m_paragraphSearcher[language], GetFieldIdQuery("ParentDocumentId", parentId))?.Select(d => d.ToParagraph()).ToList();
        }

        public List<Sentence> GetSentences(string language, Guid parentId)
        {
            return GetDocumentsByQuery(m_sentenceSearcher[language], GetFieldIdQuery("ParentParagraphId", parentId))?.Select(d => d.ToSentence()).ToList();
        }

        public T GetRandomEntity<T>(string language) where T : class, IMarcellEntity
        {
            T result;
            do
            {
                byte[] rndBytes = new byte[4];
                m_randomGenerator.GetBytes(rndBytes);
                double dblValue = BitConverter.ToUInt32(rndBytes, 0);
                double factor = dblValue / uint.MaxValue;

                IndexReader selectedReader;
                if (typeof(T) == typeof(Document))
                {
                    selectedReader = m_documentReader[language];
                }
                else if (typeof(T) == typeof(Section))
                {
                    selectedReader = m_sectionReader[language];
                }
                else if (typeof(T) == typeof(Paragraph))
                {
                    selectedReader = m_paragraphReader[language];
                }
                else if (typeof(T) == typeof(Sentence))
                {
                    selectedReader = m_sentenceReader[language];
                }
                else
                {
                    throw new InvalidOperationException("Unsupported request received!");
                }

                int docNo = (int)(factor * (selectedReader.MaxDoc - 1));
                LuceneNet.Documents.Document outputDoc = selectedReader.Document(docNo);

                if (typeof(T) == typeof(Document))
                {
                    result = outputDoc.ToDocument() as T;
                }
                else if (typeof(T) == typeof(Section))
                {
                    result = outputDoc.ToSection() as T;
                }
                else if (typeof(T) == typeof(Paragraph))
                {
                    result = outputDoc.ToParagraph() as T;
                }
                else if (typeof(T) == typeof(Sentence))
                {
                    result = outputDoc.ToSentence() as T;
                }
                else
                {
                    throw new InvalidOperationException("Unsupported entity type requested!");
                }
            } while (result.TokenCount < 5 || result.RecognitionQuality < 0.10); //Only use sufficiently large text chunks with enough recognized tokens for training

            return result;
        }

        public Document DocFromSection(string language, Section Section, bool getFullDocument = false, bool getSentences = false)
        {
            Document currentDocument = GetDocument(language, Section.ParentId);

            if (getFullDocument)
            {
                var sections = GetSections(language, currentDocument.InternalId);
                var paragraphs = GetParagraphsByDocument(language, currentDocument.InternalId);
                foreach (var section in sections)
                {
                    section.Paragraphs.AddRange(paragraphs.Where(p => p.ParentId == section.InternalId).OrderBy(p => p.Order).ToList());
                    currentDocument.Sections.Add(section);
                }
            }
            else
            {
                var paragraphs = GetParagraphsBySection(language, Section.InternalId);
                Section.Paragraphs = paragraphs.OrderBy(p => p.Order).ToList();
                currentDocument.Sections.Add(Section);
            }

            return currentDocument;
        }

        public Document DocFromParagraph(string language, Paragraph Paragraph, bool getFullDocument = false, bool getSentences = false)
        {
            Section currentSection = GetSection(language, Paragraph.ParentId);
            Document currentDocument = GetDocument(language, currentSection.ParentId);

            if (getFullDocument)
            {
                var sections = GetSections(language, currentDocument.InternalId);
                var paragraphs = GetParagraphsByDocument(language, currentDocument.InternalId);
                foreach (var section in sections)
                {
                    section.Paragraphs = paragraphs.Where(p => p.ParentId == section.InternalId).OrderBy(p => p.Order).ToList();
                    Parallel.ForEach(section.Paragraphs, paragraph =>
                    {
                        if (paragraph.InternalId == Paragraph.InternalId) { paragraph.IsMatch = true; }
                    });
                    currentDocument.Sections.Add(section);
                }
            }
            else
            {
                currentSection.Paragraphs.Add(Paragraph);
                currentDocument.Sections.Add(currentSection);
            }
            return currentDocument;
        }

        public void LoadChildren(string language, Document Document, bool getSentences = false)
        {
            var sections = GetSections(language, Document.InternalId);
            var paragraphs = GetParagraphsByDocument(language, Document.InternalId);
            foreach (var section in sections)
            {
                section.Paragraphs = paragraphs.Where(p => p.ParentId == section.InternalId).OrderBy(p => p.Order).ToList();
                Document.Sections.Add(section);
            }
        }

        public Document DocFromSentence(string language, Sentence Sentence, bool getFullDocument = false, bool getSentences = false)
        {
            Paragraph currentParagraph = GetParagraph(language, Sentence.ParentId);
            Section currentSection = GetSection(language, currentParagraph.ParentId);
            Document currentDocument = GetDocument(language, currentSection.ParentId);

            if (getFullDocument)
            {
                var sections = GetSections(language, currentDocument.InternalId);
                foreach (var section in sections)
                {
                    var paragraphs = GetParagraphsBySection(language, section.InternalId);
                    section.Paragraphs.AddRange(paragraphs);
                    currentDocument.Sections.Add(section);
                }
            }
            else
            {
                currentParagraph.Sentences.Add(Sentence);
                currentSection.Paragraphs.Add(currentParagraph);
                currentDocument.Sections.Add(currentSection);
            }

            return currentDocument;
        }

        public Section SectionFromParagraph(string language, Paragraph Paragraph)
        {
            Section currentSection = GetSection(language, Paragraph.ParentId);
            currentSection.Paragraphs.Add(Paragraph);
            return currentSection;
        }

        public Section SectionFromSentence(string language, Sentence Sentence)
        {
            Paragraph currentParagraph = GetParagraph(language, Sentence.ParentId);
            Section currentSection = GetSection(language, currentParagraph.ParentId);

            currentParagraph.Sentences.Add(Sentence);
            currentSection.Paragraphs.Add(currentParagraph);

            return currentSection;
        }

        public Paragraph ParagraphFromSentence(string language, Sentence Sentence)
        {
            Paragraph currentParagraph = GetParagraph(language, Sentence.ParentId);

            currentParagraph.Sentences.Add(Sentence);

            return currentParagraph;
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        private bool TryRelease(IndexType indexType, IndexReader indexReader)
        {
            try
            {
                m_indexManager.ReleaseReaderInstance(indexType, indexReader);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var lang in IndexManager.SupportedLanguages)
                    {
                        TryRelease(new IndexType(lang, IndexObjectType.DocumentIndex), m_documentReader[lang]);
                        TryRelease(new IndexType(lang, IndexObjectType.SectionIndex), m_sectionReader[lang]);
                        TryRelease(new IndexType(lang, IndexObjectType.ParagraphIndex), m_paragraphReader[lang]);
                        TryRelease(new IndexType(lang, IndexObjectType.SentenceIndex), m_sentenceReader[lang]);

                        m_documentReader[lang] = null;
                        m_sectionReader[lang] = null;
                        m_paragraphReader[lang] = null;
                        m_sentenceReader[lang] = null;
                    }
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable Support
    }
}