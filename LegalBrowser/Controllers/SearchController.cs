using LegalBrowser.Data.Repositories.Lucene;
using LegalBrowser.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Semantika.Marcell.Data;
using Semantika.Marcell.LuceneStore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using Semantika.Marcell.LuceneStore.Util;
using Semantika.Marcell.LuceneStore.Query.Impl;
using LegalBrowser.Models;

namespace LegalBrowser.Controllers
{
    public enum SegmentType
    {
        Document = 0,
        Section = 1,
        Paragraph = 2,
        Sentence = 3
    }

    public class SearchController : Controller
    {
        private MarcellCorpus marcellCorpus;
        private OptimizedParametrizedSearch searchProvider;

        public SearchController(MarcellCorpus marcellCorpus)
        {
            this.marcellCorpus = marcellCorpus;
        }

        private SearchResultViewModel GetResultFromDocument(Document sourceDoc, bool typeIsParagraph)
        {
            var tmpResult = new SearchResultViewModel
            {
                Id = sourceDoc.InternalId.ToString("N"),
                Description = sourceDoc.Sections.FirstOrDefault()?.Text.Truncate(280),
                ResultType = typeIsParagraph ? SearchResultType.Paragraph : SearchResultType.Document
            };
            var matchingParagraph = sourceDoc.Sections.Where(s => s.Paragraphs.Any(p => p.IsMatch)).FirstOrDefault()?.Paragraphs.Where(p => p.IsMatch).FirstOrDefault();

            if (matchingParagraph != null)
            {
                tmpResult.Title = matchingParagraph.Text.Truncate(60);
            }
            else
            {
                tmpResult.Title = sourceDoc.Sections.FirstOrDefault()?.Text.Truncate(60);
            }

            return tmpResult;
        }

        public IActionResult Index(string query, string scope, string lang, string eid = null, string did = null, string sln = null)
        {
            Result<Document> result;
            bool isPara = false;
            if (scope == "para")
            {
                isPara = true;
            }

            result = marcellCorpus.FindDocument(lang, query, isPara);

            SearchViewModel results = new SearchViewModel
            {
                ResultCount = result.TotalResults,
                Results = new List<SearchResultViewModel>(),
                EId = eid,
                DId = did,
                SourceLangauge = sln
            };

            foreach (var r in result.ResultList)
            {
                results.Results.Add(GetResultFromDocument(r, isPara));
            }

            return View(results);
        }

        public IActionResult Improve(string id = null, string lang = null)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            return View("Index");
        }

        public IActionResult Details(string id = null, string lang = null)
        {
            Guid parsedId;
            if (id == null || lang == null || !Guid.TryParse(id, out parsedId))
            {
                return RedirectToAction("Index");
            }

            var sourceDoc = marcellCorpus.GetDocument(lang, parsedId);

            TranslationViewModel result = new TranslationViewModel
            {
                Result = GetResultFromDocument(sourceDoc, false),
                Translations = new List<ParagraphLanguagePair>()
            };

            foreach (var section in sourceDoc.Sections)
            {
                foreach (var paragraph in section.Paragraphs)
                {
                    result.Translations.Add(new ParagraphLanguagePair
                    {
                        SourceParagraph = paragraph.Text,
                        SourceParagraphId = paragraph.InternalId,
                        SourceLanguage = lang,
                    });
                }
            }

            return View(result);
        }

        [HttpPost]
        public IActionResult SegmentTranslation([FromBody] SegmentModel model)
        {
            if (model == null)
            {
                return Json("");
            }
            Guid parsedId;
            if (model.Id == null || model.SourceLang == null || model.TargetLang == null || !Guid.TryParse(model.Id, out parsedId))
            {
                return Json("");
            }
            IMarcellEntity result;

            if (model.SegType == SegmentType.Sentence)
            {
                result = marcellCorpus.FindTranslation<Sentence>(parsedId, model.SourceLang, model.TargetLang, model.Parameters);
            }
            else
            {
                result = marcellCorpus.FindTranslation<Paragraph>(parsedId, model.SourceLang, model.TargetLang, model.Parameters);
            }

            var targetText = result?.Text ?? "";
            string resultHtml = "";
            if (result != null)
            {
                resultHtml = $@"<a href=""/Search/Index?query={Uri.EscapeUriString(targetText.Truncate(160))}&scope=para&lang={result.Language}&did={((Paragraph)result).ParentId}&eid={result.InternalId}&sln={model.SourceLang}"">{targetText}</a>";
            }

            return Json(resultHtml);
        }
    }
}