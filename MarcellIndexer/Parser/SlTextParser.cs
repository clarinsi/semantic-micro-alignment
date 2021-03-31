using Semantika.Marcell.Data;
using System.Text.RegularExpressions;
using TBXFormat;

namespace Semantika.Marcell.Processor.Parser
{
    public class SlLegalTextParser : LegalTextParser
    {
        public SlLegalTextParser(Tbx tbxSource) : base(tbxSource)
        {
        }

        private static readonly Regex m_sectionHeading = new Regex(@"^(?<ArtNum>([0-9]*))\.\s?člen$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        protected override bool IsNewSection(string sentenceText, Section currentSection, out Section nextSection)
        {
            return IsNewSectionByRegex(m_sectionHeading, sentenceText, currentSection, out nextSection);
        }
    }
}