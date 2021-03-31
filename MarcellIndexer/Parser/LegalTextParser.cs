using CoNLLUP;
using Semantika.Marcell.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TBXFormat;
using CoNLLDocument = CoNLLUP.text;
using MarcellDocument = Semantika.Marcell.Data.Document;

namespace Semantika.Marcell.Processor.Parser
{
    public abstract class LegalTextParser
    {
        private static object m_tbxLock = new object();
        private static Tbx m_tbxSource;
        private static ConcurrentDictionary<string, List<string>> m_tbxCache = new ConcurrentDictionary<string, List<string>>();

        protected bool IsNewSectionByRegex(Regex regex, string sentenceText, Section currentSection, out Section nextSection)
        {
            var headingMatch = regex.Match(sentenceText);
            if (headingMatch.Success)
            {
                string artNum;
                var artNumGroup = headingMatch.Groups["ArtNum"];
                if (artNumGroup.Success)
                {
                    artNum = artNumGroup.Value;
                    //We have located the specific heading pattern
                    nextSection = new Section()
                    {
                        InternalId = Guid.NewGuid(),
                        Language = currentSection.Language,
                        ArticleNumber = artNum
                    };
                    return true;
                }
            }

            nextSection = currentSection;
            return false;
        }

        protected abstract bool IsNewSection(string sentenceText, Section currentSection, out Section nextSection);

        public LegalTextParser(Tbx tbxData)
        {
            lock (m_tbxLock)
            {
                if (m_tbxSource == null)
                {
                    m_tbxSource = tbxData;

                    var topics = m_tbxSource.Text.body.Where(b => b.Description.Type == "subjectField").Select(b => new { Key = b.Id.ToString(), Value = b.Description.Value }).Where(b => !String.IsNullOrWhiteSpace(b.Value));
                    foreach (var topic in topics)
                    {
                        m_tbxCache[topic.Key] = topic.Value.Split(';').ToList(); ;
                    }
                }
            }
        }

        private void AddSectionToDocument(MarcellDocument document, Section section)
        {
            section.SectionSimilarityData.ConsolidatedTokens = section.SectionSimilarityData.ConsolidatedTokens.Distinct().ToList();
            section.SectionSimilarityData.ConsolidatedTopics = section.SectionSimilarityData.ConsolidatedTopics.Distinct().ToList();

            section.RecognitionQuality = section.Paragraphs.Average(s => s.RecognitionQuality);

            document.TokenCount += section.TokenCount;
            document.DocumentSimilarityData.ConsolidatedTokens.AddRange(section.SectionSimilarityData.ConsolidatedTokens);
            document.DocumentSimilarityData.ConsolidatedTopics.AddRange(section.SectionSimilarityData.ConsolidatedTopics);
            document.Sections.Add(section);
        }

        private List<string> ParseMarcellList(string source, out List<string> domains, string prefix = "")
        {
            domains = new List<string>();
            if (string.IsNullOrEmpty(source) || source == "_")
            {
                return new List<string>();
            }

            var tokens = source.Split(';').Where(t => t.Contains(":")).Select(t => t.Split(':')[1]).ToArray();
            var results = new List<string>(tokens.Length);
            foreach (var t in tokens)
            {
                if (t.Contains("-"))
                {
                    var termDomain = t.Split('-');
                    var curDomains = termDomain[1].Split(',');
                    results.Add(prefix + termDomain[0]);
                    domains.AddRange(curDomains);
                }
                else
                {
                    results.Add(prefix + t);
                }
            }

            return results;
        }

        private Dictionary<string, string> ParseMarcellFeatures(string source)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(source) || source == "_")
            {
                return result;
            }

            var featurePairs = source.Split('|').ToList();
            foreach (var featurePair in featurePairs)
            {
                if (featurePair != "_")
                {
                    var keyValuePair = featurePair.Split('=');
                    result[keyValuePair[0]] = keyValuePair[1];
                }
            }
            return result;
        }

        private List<string> m_emptyStringList = new List<string>();

        private List<string> GetTopicsTbx(List<string> iateTerms)
        {
            List<string> result = new List<string>();
            foreach (var term in iateTerms)
            {
                if (m_tbxCache.TryGetValue(term, out List<string> cachedResults))
                {
                    result.AddRange(cachedResults);
                }
                else
                {
                    return m_emptyStringList;
                }
            }

            return result;
        }

        private MarcellDocument GetBasicDocument(CoNLLDocument sourceDocument)
        {
            return new MarcellDocument
            {
                ApprovalDate = sourceDocument.doc.date_approved,
                DocumentDate = sourceDocument.doc.date,
                DocumentType = sourceDocument.doc.entype,
                EffectiveDate = sourceDocument.doc.date_effect,
                InternalId = Guid.NewGuid(),
                IsStructured = false,
                Id = sourceDocument.doc.id,
                Issuer = sourceDocument.doc.issuer,
                Language = sourceDocument.doc.language,
                OriginalType = sourceDocument.doc.type,
                Url = sourceDocument.doc.url
            };
        }

        private Token GetParsedToken(textDocPSToken token, MarcellDocument document, int curPos)
        {
            var iateEntries = ParseMarcellList(token.marcell_iate, out List<string> iateDomains, "IATE");
            return new Token
            {
                Deprel = token.deprel,
                Deps = token.deps,
                EuroVocEntities = ParseMarcellList(token.marcell_eurovoc, out _, "EV"),
                Features = ParseMarcellFeatures(token.feats),
                Form = token.form,
                GeneralPos = token.upos,
                HeadId = token.head,
                IateEntities = iateEntries,
                IateDomains = iateDomains,
                Id = token.id.ToString(),
                InternalId = Guid.NewGuid(),
                Language = document.Language,
                LanguagePos = token.xpos,
                Lemma = token.lemma,
                Misc = token.misc,
                NE = token.marcell_ne,
                NP = token.marcell_np,
                Order = curPos
            };
        }

        public MarcellDocument ParsePass1(CoNLLDocument sourceDocument)
        {
            //Initialize the document with basic information, new Guid as the internal Id and as an unstructured document
            MarcellDocument document = GetBasicDocument(sourceDocument);

            Section currentSection = new Section()
            {
                InternalId = Guid.NewGuid(),
                Language = document.Language
            };
            Section nextSection = null;
            bool sectionCommited = true;
            int curPar = 0;
            //Go through the document and combine all sentences into paragraphs, propagating keywords upwards from children to parents
            foreach (var paragraph in sourceDocument.doc.p)
            {
                Paragraph currentParagraph = new Paragraph()
                {
                    Id = paragraph.id,
                    InternalId = Guid.NewGuid(),
                    Language = document.Language,
                    ParagraphNumber = curPar.ToString(),
                    Order = curPar
                };
                curPar++;

                StringBuilder paragraphText = new StringBuilder();
                int curSent = 0;
                foreach (var sentence in paragraph.s)
                {
                    if (sentence != null && !string.IsNullOrEmpty(sentence.text) && IsNewSection(sentence.text, currentSection, out nextSection))
                    {
                        if (!sectionCommited)
                        {
                            AddSectionToDocument(document, currentSection);
                            sectionCommited = true;
                        }
                        currentSection = nextSection;
                    }

                    Sentence currentSentence = new Sentence
                    {
                        Id = sentence.id,
                        InternalId = Guid.NewGuid(),
                        Language = document.Language,
                        Text = sentence.text,
                        Order = curSent,
                    };
                    curSent++;
                    int curPos = 0;
                    int recognizedTokens = 0;
                    int totalTokens = 0;
                    foreach (var token in sentence.token)
                    {
                        Token currentToken = GetParsedToken(token, document, curPos);

                        currentToken.SimilarityData.ConsolidatedTokens.AddRange(currentToken.EuroVocEntities);
                        currentToken.SimilarityData.ConsolidatedTokens.AddRange(currentToken.IateEntities);
                        currentToken.SimilarityData.ConsolidatedTopics.AddRange(currentToken.IateDomains);

                        if (currentToken.GeneralPos != "PUNCT")
                        {
                            if (currentToken.EuroVocEntities.Count > 0)
                            {
                                recognizedTokens++;
                            }
                            if (currentToken.IateEntities.Count > 0)
                            {
                                recognizedTokens++;
                            }
                            totalTokens += 2;
                        }

                        currentSentence.SentenceSimilarityData.ConsolidatedTokens.AddRange(currentToken.SimilarityData.ConsolidatedTokens);
                        currentSentence.SentenceSimilarityData.ConsolidatedTopics.AddRange(currentToken.SimilarityData.ConsolidatedTopics);

                        currentSentence.Tokens.Add(currentToken);
                        currentSentence.TokenCount++;

                        curPos++;
                    }

                    currentSentence.SentenceSimilarityData.ConsolidatedTokens = currentSentence.SentenceSimilarityData.ConsolidatedTokens.Distinct().ToList();
                    currentSentence.SentenceSimilarityData.ConsolidatedTopics = currentSentence.SentenceSimilarityData.ConsolidatedTopics.Distinct().ToList();

                    if (totalTokens > 0)
                    {
                        double sentenceQuality = (double)recognizedTokens / (double)totalTokens;
                        currentSentence.RecognitionQuality = sentenceQuality;
                    }
                    else
                    {
                        currentSentence.RecognitionQuality = 1;
                    }

                    paragraphText.Append(currentSentence.Text).Append(" ");
                    currentParagraph.Sentences.Add(currentSentence);
                    currentParagraph.ParagraphSimilarityData.ConsolidatedTokens.AddRange(currentSentence.SentenceSimilarityData.ConsolidatedTokens);
                    currentParagraph.ParagraphSimilarityData.ConsolidatedTopics.AddRange(currentSentence.SentenceSimilarityData.ConsolidatedTopics);
                    currentParagraph.TokenCount += currentSentence.TokenCount;
                }
                currentParagraph.Text = paragraphText.ToString();
                currentSection.TextStringBuilder.AppendLine(currentParagraph.Text);
                currentSection.TokenCount += currentParagraph.TokenCount;
                currentParagraph.RecognitionQuality = currentParagraph.Sentences.Average(s => s.RecognitionQuality);

                currentParagraph.ParagraphSimilarityData.ConsolidatedTokens = currentParagraph.ParagraphSimilarityData.ConsolidatedTokens.Distinct().ToList();
                currentParagraph.ParagraphSimilarityData.ConsolidatedTopics = currentParagraph.ParagraphSimilarityData.ConsolidatedTopics.Distinct().ToList();

                currentSection.SectionSimilarityData.ConsolidatedTokens.AddRange(currentParagraph.ParagraphSimilarityData.ConsolidatedTokens);
                currentSection.SectionSimilarityData.ConsolidatedTopics.AddRange(currentParagraph.ParagraphSimilarityData.ConsolidatedTopics);

                currentSection.Paragraphs.Add(currentParagraph);
                sectionCommited = false;
            }

            //If the last section has been modified after last commit, add it to the document
            if (!sectionCommited)
            {
                AddSectionToDocument(document, currentSection);
            }

            document.RecognitionQuality = document.Sections.Average(s => s.RecognitionQuality);
            if (document.Sections.Count > 1)
            {
                document.IsStructured = true;
            }
            return document;
        }

        public void ParsePass2(MarcellDocument document)
        {
            //We now move all data back from parents to children for easier searching in Phase 2
            //We add additional metadata for indexing, such as token numbers, recognition quality and similar - this is then used in the similarity function to determine the similar paragraphs
            foreach (var section in document.Sections)
            {
                section.DocumentSimilarityData = document.DocumentSimilarityData;
                foreach (var paragraph in section.Paragraphs)
                {
                    paragraph.DocumentSimilarityData = document.DocumentSimilarityData;
                    paragraph.SectionSimilarityData = section.SectionSimilarityData;
                    foreach (var sentence in paragraph.Sentences)
                    {
                        sentence.DocumentSimilarityData = document.DocumentSimilarityData;
                        sentence.SectionSimilarityData = section.SectionSimilarityData;
                        sentence.ParagraphSimilarityData = paragraph.ParagraphSimilarityData;
                    }
                }
            }
        }
    }
}