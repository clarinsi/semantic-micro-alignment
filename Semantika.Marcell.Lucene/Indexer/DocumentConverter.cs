using Lucene.Net.Documents;
using Lucene.Net.Documents.Extensions;
using Semantika.Marcell.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using LuceneDocument = Lucene.Net.Documents.Document;
using MarcellDocument = Semantika.Marcell.Data.Document;

namespace Semantika.Marcell.LuceneStore.Indexer
{
    public static class DocumentConverter
    {
        #region Utility functions to convert from .NET Types to Lucene.NET fields

        private static Field AddDateField(this LuceneDocument doc, string name, DateTime? datedata)
        {
            if (datedata == null)
            {
                return null;
            }
            return doc.AddInt64Field(name, datedata.Value.Ticks, Field.Store.YES);
        }

        private static Field AddStringList(this LuceneDocument doc, string name, IEnumerable<string> stringList, Field.Store store = Field.Store.YES)
        {
            if (stringList == null)
            {
                return null;
            }

            return doc.AddTextField(name, string.Join(" ", stringList), store);
        }

        private static Field AddBoolField(this LuceneDocument doc, string name, bool data)
        {
            int convertedValue = (data ? 0x01 : 0x00);
            return doc.AddInt32Field(name, convertedValue, Field.Store.YES);
        }

        private static Field[] AddScoredDoubleField(this LuceneDocument doc, string name, double data)
        {
            return new Field[2]
            {
                doc.AddDoubleDocValuesField($"{name}$Scoring", data),
                doc.AddStoredField(name, data)
            };
        }

        #endregion Utility functions to convert from .NET Types to Lucene.NET fields

        #region Utility functions to convert from Lucene.NET fields to .NET types

        private static DateTime GetDate(this LuceneDocument doc, string name)
        {
            var ticks = doc.GetField(name)?.GetInt64ValueOrDefault();
            return ticks.HasValue ? new DateTime(ticks.Value) : DateTime.MinValue;
        }

        private static List<string> GetStringList(this LuceneDocument doc, string name)
        {
            return doc.GetValues(name)?.FirstOrDefault()?.Split(' ').ToList();
        }

        private static bool GetBool(this LuceneDocument doc, string name)
        {
            var value = doc.GetField(name)?.GetInt32ValueOrDefault();
            return (value == 1);
        }

        private static int GetInt(this LuceneDocument doc, string name)
        {
            var value = doc.GetField(name)?.GetInt32ValueOrDefault();
            return value ?? 0;
        }

        private static double GetDouble(this LuceneDocument doc, string name)
        {
            var value = doc.GetField(name)?.GetDoubleValueOrDefault();
            return value ?? 0.0;
        }

        #endregion Utility functions to convert from Lucene.NET fields to .NET types

        #region Conversion from .NET to Lucene documents

        public static LuceneDocument ToLucene(this MarcellDocument sourceDocument)
        {
            LuceneDocument result = new LuceneDocument();

            //Add basic document data
            result.AddStringField("Id", sourceDocument.Id ?? sourceDocument.InternalId.ToString("N"), Field.Store.YES);
            result.AddStringField("InternalId", sourceDocument.InternalId.ToString("N"), Field.Store.YES);
            result.AddDateField("ApprovalDate", sourceDocument.ApprovalDate);
            result.AddDateField("DocumentDate", sourceDocument.DocumentDate);
            result.AddDateField("EffectiveDate", sourceDocument.EffectiveDate);
            result.AddStringList("DocumentToken", sourceDocument.DocumentSimilarityData.ConsolidatedTokens);
            result.AddStringList("DocumentTopic", sourceDocument.DocumentSimilarityData.ConsolidatedTopics);
            result.AddInt32Field("TokenCount", sourceDocument.TokenCount, Field.Store.YES);
            result.AddTextField("DocumentType", sourceDocument.DocumentType ?? "", Field.Store.YES);
            result.AddTextField("OriginalType", sourceDocument.OriginalType ?? "", Field.Store.YES);
            result.AddTextField("Issuer", sourceDocument.Issuer ?? "", Field.Store.YES);
            result.AddStringField("Language", sourceDocument.Language, Field.Store.YES);
            result.AddStringField("Url", sourceDocument.Url ?? "", Field.Store.YES);
            result.AddScoredDoubleField("RecognitionQuality", sourceDocument.RecognitionQuality);
            result.AddBoolField("IsStructured", sourceDocument.IsStructured);

            if (sourceDocument.FileName != null)
            {
                result.AddStringField("FileName", sourceDocument.FileName, Field.Store.YES);
            }

            return result;
        }

        public static LuceneDocument ToLucene(this Section sourceSection, MarcellDocument parentDocument)
        {
            LuceneDocument result = new LuceneDocument();

            //Add basic document data
            result.AddStringField("Id", sourceSection.Id ?? sourceSection.InternalId.ToString("N"), Field.Store.YES);
            result.AddStringField("InternalId", sourceSection.InternalId.ToString("N"), Field.Store.YES);
            result.AddStringList("DocumentToken", sourceSection.DocumentSimilarityData.ConsolidatedTokens);
            result.AddStringList("DocumentTopic", sourceSection.DocumentSimilarityData.ConsolidatedTopics);
            result.AddStringList("SectionToken", sourceSection.SectionSimilarityData.ConsolidatedTokens);
            result.AddStringList("SectionTopic", sourceSection.SectionSimilarityData.ConsolidatedTopics);
            result.AddInt32Field("TokenCount", sourceSection.TokenCount, Field.Store.YES);
            result.AddStringField("Language", sourceSection.Language, Field.Store.YES);
            result.AddScoredDoubleField("RecognitionQuality", sourceSection.RecognitionQuality);

            result.AddStringField("Type", sourceSection.Type.ToString(), Field.Store.YES);

            string sectionText = sourceSection.Text ?? "";
            result.AddTextField("Text", sectionText, Field.Store.YES);

            //Add reference to parent document
            if (parentDocument != null)
            {
                result.AddStringField("ParentDocumentId", parentDocument.InternalId.ToString("N"), Field.Store.YES);
            }

            return result;
        }

        public static LuceneDocument ToLucene(this Paragraph sourceParagraph, MarcellDocument parentDocument, Section parentSection)
        {
            LuceneDocument result = new LuceneDocument();

            //Add basic document data
            result.AddStringField("Id", sourceParagraph.Id ?? sourceParagraph.InternalId.ToString("N"), Field.Store.YES);
            result.AddStringField("InternalId", sourceParagraph.InternalId.ToString("N"), Field.Store.YES);
            result.AddStringList("DocumentToken", sourceParagraph.DocumentSimilarityData.ConsolidatedTokens);
            result.AddStringList("DocumentTopic", sourceParagraph.DocumentSimilarityData.ConsolidatedTopics);
            result.AddStringList("SectionToken", sourceParagraph.SectionSimilarityData.ConsolidatedTokens);
            result.AddStringList("SectionTopic", sourceParagraph.SectionSimilarityData.ConsolidatedTopics);
            result.AddStringList("ParagraphToken", sourceParagraph.ParagraphSimilarityData.ConsolidatedTokens);
            result.AddStringList("ParagraphTopic", sourceParagraph.ParagraphSimilarityData.ConsolidatedTopics);
            result.AddInt32Field("TokenCount", sourceParagraph.TokenCount, Field.Store.YES);
            result.AddStringField("Language", sourceParagraph.Language, Field.Store.YES);
            result.AddScoredDoubleField("RecognitionQuality", sourceParagraph.RecognitionQuality);
            result.AddInt32Field("Order", sourceParagraph.Order, Field.Store.YES);

            result.AddStringField("ParagraphType", sourceParagraph.ParagraphType.ToString(), Field.Store.YES);
            result.AddStringField("ParagraphNumber", sourceParagraph.ParagraphNumber ?? "", Field.Store.YES);
            result.AddStringField("PointNumber", sourceParagraph.PointNumber ?? "", Field.Store.YES);

            string paragraphText = sourceParagraph.Text ?? "";
            result.AddTextField("Text", paragraphText, Field.Store.YES);

            //Add reference to parent document
            if (parentDocument != null)
            {
                result.AddStringField("ParentDocumentId", parentDocument.InternalId.ToString("N"), Field.Store.YES);
            }
            //Add reference to parent section
            if (parentSection != null)
            {
                result.AddStringField("ParentSectionId", parentSection.InternalId.ToString("N"), Field.Store.YES);
            }

            return result;
        }

        public static LuceneDocument ToLucene(this Sentence sourceSentence, MarcellDocument parentDocument, Section parentSection, Paragraph parentParagraph)
        {
            LuceneDocument result = new LuceneDocument();

            //Add basic document data
            result.AddStringField("Id", sourceSentence.Id ?? sourceSentence.InternalId.ToString("N"), Field.Store.YES);
            result.AddStringField("InternalId", sourceSentence.InternalId.ToString("N"), Field.Store.YES);
            result.AddStringList("DocumentToken", sourceSentence.DocumentSimilarityData.ConsolidatedTokens);
            result.AddStringList("DocumentTopic", sourceSentence.DocumentSimilarityData.ConsolidatedTopics);
            result.AddStringList("SectionToken", sourceSentence.SectionSimilarityData.ConsolidatedTokens);
            result.AddStringList("SectionTopic", sourceSentence.SectionSimilarityData.ConsolidatedTopics);
            result.AddStringList("ParagraphToken", sourceSentence.ParagraphSimilarityData.ConsolidatedTokens);
            result.AddStringList("ParagraphTopic", sourceSentence.ParagraphSimilarityData.ConsolidatedTopics);
            result.AddStringList("SentenceToken", sourceSentence.SentenceSimilarityData.ConsolidatedTokens);
            result.AddStringList("SentenceTopic", sourceSentence.SentenceSimilarityData.ConsolidatedTopics);
            result.AddInt32Field("TokenCount", sourceSentence.TokenCount, Field.Store.YES);
            result.AddStringField("Language", sourceSentence.Language, Field.Store.YES);
            result.AddScoredDoubleField("RecognitionQuality", sourceSentence.RecognitionQuality);
            result.AddInt32Field("Order", sourceSentence.Order, Field.Store.YES);

            string sentencehText = sourceSentence.Text ?? "";
            result.AddTextField("Text", sentencehText, Field.Store.YES);

            //Add reference to parent document
            if (parentDocument != null)
            {
                result.AddStringField("ParentDocumentId", parentDocument.InternalId.ToString("N"), Field.Store.YES);
            }
            //Add reference to parent section
            if (parentSection != null)
            {
                result.AddStringField("ParentSectionId", parentSection.InternalId.ToString("N"), Field.Store.YES);
            }
            //Add reference to parent section
            if (parentParagraph != null)
            {
                result.AddStringField("ParentParagraphId", parentParagraph.InternalId.ToString("N"), Field.Store.YES);
            }

            result.AddStringList("ContainedTokenEV", sourceSentence.Tokens.SelectMany(t => t.EuroVocEntities), Field.Store.NO);
            result.AddStringList("ContainedTokenIATE", sourceSentence.Tokens.SelectMany(t => t.IateEntities), Field.Store.NO);

            return result;
        }

        #endregion Conversion from .NET to Lucene documents

        #region Conversion from Lucene documents to .NET types

        public static MarcellDocument ToDocument(this LuceneDocument source)
        {
            MarcellDocument result = new MarcellDocument
            {
                Id = source.GetValues("Id")?.FirstOrDefault(),
                InternalId = Guid.Parse(source.GetValues("InternalId").First()),
                ApprovalDate = source.GetDate("ApprovalDate"),
                DocumentDate = source.GetDate("DocumentDate"),
                EffectiveDate = source.GetDate("EffectiveDate"),
                DocumentSimilarityData = new SimilarityData
                {
                    ConsolidatedTokens = source.GetStringList("DocumentToken"),
                    ConsolidatedTopics = source.GetStringList("DocumentTopic"),
                },
                TokenCount = source.GetInt("TokenCount"),
                DocumentType = source.GetValues("DocumentType")?.FirstOrDefault(),
                OriginalType = source.GetValues("OriginalType")?.FirstOrDefault(),
                Issuer = source.GetValues("Issuer")?.FirstOrDefault(),
                Language = source.GetValues("Language")?.FirstOrDefault(),
                Url = source.GetValues("Url")?.FirstOrDefault(),
                RecognitionQuality = source.GetDouble("RecognitionQuality"),
                IsStructured = source.GetBool("IsStructured"),
                FileName = source.GetValues("FileName")?.FirstOrDefault(),
            };

            return result;
        }

        public static IMarcellEntity ToEntity(this LuceneDocument source, Type targetType)
        {
            if (targetType == typeof(MarcellDocument))
            {
                return source.ToDocument();
            }
            else if (targetType == typeof(Section))
            {
                return source.ToSection();
            }
            else if (targetType == typeof(Paragraph))
            {
                return source.ToParagraph();
            }
            else if (targetType == typeof(Sentence))
            {
                return source.ToSentence();
            }
            else
            {
                throw new InvalidOperationException("The requested type is unsupported!");
            }
        }

        public static T ToEntity<T>(this LuceneDocument source) where T : class, IMarcellEntity
        {
            return ToEntity(source, typeof(T)) as T;
        }

        public static Section ToSection(this LuceneDocument source)
        {
            SectionType secType;
            Enum.TryParse(source.GetValues("Type").FirstOrDefault(), true, out secType);

            Section result = new Section
            {
                Id = source.GetValues("Id").FirstOrDefault(),
                InternalId = Guid.Parse(source.GetValues("InternalId").First()),
                DocumentSimilarityData = new SimilarityData
                {
                    ConsolidatedTokens = source.GetStringList("DocumentToken"),
                    ConsolidatedTopics = source.GetStringList("DocumentTopic"),
                },
                SectionSimilarityData = new SimilarityData
                {
                    ConsolidatedTokens = source.GetStringList("SectionToken"),
                    ConsolidatedTopics = source.GetStringList("SectionTopic"),
                },
                TokenCount = source.GetInt("TokenCount"),
                Language = source.GetValues("Language")?.FirstOrDefault(),
                RecognitionQuality = source.GetDouble("RecognitionQuality"),
                Type = secType,
                Text = source.GetValues("Text")?.FirstOrDefault(),
                ParentId = Guid.Parse(source.GetValues("ParentDocumentId").First())
            };

            return result;
        }

        public static Paragraph ToParagraph(this LuceneDocument source)
        {
            ParagraphType pType;
            Enum.TryParse(source.GetValues("Type")?.FirstOrDefault(), true, out pType);

            Paragraph result = new Paragraph
            {
                Id = source.GetValues("Id").FirstOrDefault(),
                InternalId = Guid.Parse(source.GetValues("InternalId").First()),
                DocumentSimilarityData = new SimilarityData
                {
                    ConsolidatedTokens = source.GetStringList("DocumentToken"),
                    ConsolidatedTopics = source.GetStringList("DocumentTopic"),
                },
                SectionSimilarityData = new SimilarityData
                {
                    ConsolidatedTokens = source.GetStringList("SectionToken"),
                    ConsolidatedTopics = source.GetStringList("SectionTopic"),
                },
                ParagraphSimilarityData = new SimilarityData
                {
                    ConsolidatedTokens = source.GetStringList("ParagraphToken"),
                    ConsolidatedTopics = source.GetStringList("ParagraphTopic"),
                },
                TokenCount = source.GetInt("TokenCount"),
                Language = source.GetValues("Language")?.FirstOrDefault(),
                RecognitionQuality = source.GetDouble("RecognitionQuality"),
                Order = source.GetInt("Order"),
                Text = source.GetValues("Text")?.FirstOrDefault(),
                ParagraphNumber = source.GetValues("ParagraphNumber")?.FirstOrDefault(),
                PointNumber = source.GetValues("PointNumber")?.FirstOrDefault(),
                ParentId = Guid.Parse(source.GetValues("ParentSectionId").First())
            };

            return result;
        }

        public static Sentence ToSentence(this LuceneDocument source)
        {
            Sentence result = new Sentence
            {
                Id = source.GetValues("Id").FirstOrDefault(),
                InternalId = Guid.Parse(source.GetValues("InternalId").First()),
                DocumentSimilarityData = new SimilarityData
                {
                    ConsolidatedTokens = source.GetStringList("DocumentToken"),
                    ConsolidatedTopics = source.GetStringList("DocumentTopic"),
                },
                SectionSimilarityData = new SimilarityData
                {
                    ConsolidatedTokens = source.GetStringList("SectionToken"),
                    ConsolidatedTopics = source.GetStringList("SectionTopic"),
                },
                ParagraphSimilarityData = new SimilarityData
                {
                    ConsolidatedTokens = source.GetStringList("ParagraphToken"),
                    ConsolidatedTopics = source.GetStringList("ParagraphTopic"),
                },
                SentenceSimilarityData = new SimilarityData
                {
                    ConsolidatedTokens = source.GetStringList("SentenceToken"),
                    ConsolidatedTopics = source.GetStringList("SentenceTopic"),
                },
                TokenCount = source.GetInt("TokenCount"),
                Language = source.GetValues("Language")?.FirstOrDefault(),
                RecognitionQuality = source.GetDouble("RecognitionQuality"),
                Order = source.GetInt("Order"),
                Text = source.GetValues("Text")?.FirstOrDefault(),
                ParentId = Guid.Parse(source.GetValues("ParentParagraphId").First())
            };

            return result;
        }

        #endregion Conversion from Lucene documents to .NET types
    }
}