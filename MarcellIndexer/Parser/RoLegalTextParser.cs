﻿using Semantika.Marcell.Data;
using System.Text.RegularExpressions;
using TBXFormat;

namespace Semantika.Marcell.Processor.Parser
{
    public class RoLegalTextParser : LegalTextParser
    {
        public RoLegalTextParser(Tbx tbxSource) : base(tbxSource)
        {
        }

        private static readonly Regex m_sectionHeading = new Regex(@"^(Articolul\s?(?<ArtNum>([0-9]*))\.?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        protected override bool IsNewSection(string sentenceText, Section currentSection, out Section nextSection)
        {
            return IsNewSectionByRegex(m_sectionHeading, sentenceText, currentSection, out nextSection);
        }
    }
}