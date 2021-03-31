using System.Collections.Generic;

namespace LegalBrowser.Data.ViewModels
{
    public enum SearchResultType
    {
        Document,
        Paragraph
    }

    public class ParagraphData
    {
        public string Text { get; set; }
        public int Rating { get; set; }
        public string Id { get; set; }
    }

    public class SearchResultViewModel
    {
        public SearchResultType ResultType { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<ParagraphData> Paragraphs { get; set; }
    }

    public class SearchViewModel
    {
        public int ResultCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public string Language { get; set; }
        public string Query { get; set; }
        public string EId { get; set; }
        public string DId { get; set; }
        public int Scope { get; set; }
        public string SourceLangauge { get; set; }
        public List<SearchResultViewModel> Results { get; set; }
        public string TranslatingText { get; set; }
    }
}