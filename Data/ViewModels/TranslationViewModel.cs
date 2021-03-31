using System;
using System.Collections.Generic;

namespace LegalBrowser.Data.ViewModels
{
    public class ParagraphLanguagePair
    {
        public string SourceParagraph { get; set; }
        public string TargetParagraph { get; set; }
        public int Rating { get; set; }
        public bool IsManuallyAligned { get; set; }
        public Guid SourceParagraphId { get; set; }
        public Guid TargetParagraphId { get; set; }
        public int Order { get; set; }
        public bool IsMatch { get; set; }
        public string SourceLanguage { get; set; }
        public string TargetLanguage { get; set; }
    }

    public class TranslationViewModel
    {
        public SearchResultViewModel Result { get; set; }
        public string SourceLanguage { get; set; }
        public string TargetLanguage { get; set; }
        public int ResultCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public List<ParagraphLanguagePair> Translations { get; set; }
    }
}