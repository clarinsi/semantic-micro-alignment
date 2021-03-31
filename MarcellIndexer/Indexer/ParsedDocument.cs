using Semantika.Marcell.Processor.IO;
using Semantika.Marcell.Processor.Parser;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CoNLLDocument = CoNLLUP.text;
using MarcellDocument = Semantika.Marcell.Data.Document;

namespace Semantika.Marcell.Processor.Indexer
{
    public class ParsedDocument
    {
        private static volatile System.Xml.Serialization.XmlSerializer m_serializerCoNLLUP;
        private static readonly object syncRoot = new object();

        private static System.Xml.Serialization.XmlSerializer SerializerCoNLLUP
        {
            get
            {
                // only create a new instance if one doesn't already exist.
                if (m_serializerCoNLLUP == null)
                {
                    // use this lock to ensure that only one thread is access
                    // this block of code at once.
                    lock (syncRoot)
                    {
                        if (m_serializerCoNLLUP == null)
                        {
                            XmlSerializerFactory factory = new XmlSerializerFactory();
                            m_serializerCoNLLUP = factory.CreateSerializer(typeof(CoNLLDocument));
                        }
                    }
                }
                // return instance where it was just created or already existed.
                return m_serializerCoNLLUP;
            }
        }

        private MarcellDocument m_marcellDocument;

        public MarcellDocument Document
        {
            get
            {
                return m_marcellDocument;
            }
        }

        private void Preparse(CoNLLDocument sourceDoc, string fileName)
        {
            //Ensure we have a language
            if (string.IsNullOrEmpty(sourceDoc.doc.language))
            {
                sourceDoc.doc.language = Path.GetFileNameWithoutExtension(fileName).Substring(0, 2);
            }

            //Ensure we have paragraphs (some Xml files are missing the "p" elements) by moving the sentences directly in the doc to a new automatically generated paragraph
            if (sourceDoc.doc.p == null)
            {
                sourceDoc.doc.p = new CoNLLUP.textDocP[1]
                {
                    new CoNLLUP.textDocP
                    {
                        id = "autoparagraph_fix",
                        s = sourceDoc.doc.s
                    }
                };
            }
            else
            {
                if (sourceDoc.doc.s != null)
                {
                    //We seem to have sentences out of paragraphs, create a new paragraph and put them there
                    var tempBuffer = sourceDoc.doc.p;
                    sourceDoc.doc.p = new CoNLLUP.textDocP[sourceDoc.doc.p.Length + 1];
                    Array.Copy(tempBuffer, sourceDoc.doc.p, tempBuffer.Length);
                    sourceDoc.doc.p[tempBuffer.Length] = new CoNLLUP.textDocP
                    {
                        id = "autoparagraph_fix",
                        s = sourceDoc.doc.s
                    };
                }
            }
            sourceDoc.doc.s = null;
        }

        private void ParseDocumentXml(string fileName, LegalTextParserFactory parserFactory)
        {
            CoNLLDocument sourceDoc = null;
            using (var fileReader = new SanitizedStreamReader(fileName))
            {
                using (var xmlReader = XmlReader.Create(fileReader))
                {
                    sourceDoc = (CoNLLDocument)SerializerCoNLLUP.Deserialize(xmlReader);
                }
            }

            if (sourceDoc == null || sourceDoc.doc == null)
            {
                throw new InvalidOperationException("Unsupported document found in corpus!");
            }

            //Preprocess the document to fix common problems in xml files
            Preparse(sourceDoc, fileName);

            ParseDocumentXml(sourceDoc, parserFactory);
        }

        private void ParseDocumentXml(CoNLLDocument sourceDoc, LegalTextParserFactory parserFactory)
        {
            if (sourceDoc == null || sourceDoc.doc == null)
            {
                throw new InvalidOperationException("Unsupported document found in corpus!");
            }

            var parser = parserFactory.CreateParser(sourceDoc.doc.language);

            var pass1 = parser.ParsePass1(sourceDoc);
            parser.ParsePass2(pass1);

            m_marcellDocument = pass1;
        }

        private void ParseDocumentCoNLLUP(string fileName, LegalTextParserFactory parserFactory)
        {
            //Convert to an XML representation
            CoNLLDocument sourceDoc = null;

            //Parse the same way as XML
            ParseDocumentXml(sourceDoc, parserFactory);

        }

        public ParsedDocument(string fileName, LegalTextParserFactory parserFactory, bool isXml = true)
        {
            if (isXml)
            {
                ParseDocumentXml(fileName, parserFactory);
            }
            else
            {
                ParseDocumentCoNLLUP(fileName, parserFactory);
            }
        }

        public ParsedDocument(MarcellDocument document)
        {
            m_marcellDocument = document;
        }
    }
}