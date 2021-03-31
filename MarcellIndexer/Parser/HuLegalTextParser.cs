using Semantika.Marcell.Data;
using TBXFormat;

namespace Semantika.Marcell.Processor.Parser
{
    public class HuLegalTextParser : LegalTextParser
    {
        public HuLegalTextParser(Tbx tbxSource) : base(tbxSource)
        {
        }

        protected override bool IsNewSection(string sentenceText, Section currentSection, out Section nextSection)
        {
            nextSection = currentSection;
            return false;
        }
    }
}