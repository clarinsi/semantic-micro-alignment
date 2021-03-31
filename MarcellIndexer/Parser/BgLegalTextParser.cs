using Semantika.Marcell.Data;
using System;
using System.Text.RegularExpressions;
using TBXFormat;

namespace Semantika.Marcell.Processor.Parser
{
    public class BgLegalTextParser : LegalTextParser
    {
        private static readonly Regex m_sectionHeading = new Regex(@"^(Член\s?(?<ArtNum1>([0-9]*))\s?-)|(.*Чл\.\s(?<ArtNum2>([0-9]*))\.?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        public BgLegalTextParser(Tbx tbxSource) : base(tbxSource)
        {
        }

        protected override bool IsNewSection(string sentenceText, Section currentSection, out Section nextSection)
        {
            var headingMatch = m_sectionHeading.Match(sentenceText);
            if (headingMatch.Success)
            {
                string artNum;
                var artNum1Group = headingMatch.Groups["ArtNum1"];
                if (artNum1Group.Success)
                {
                    artNum = artNum1Group.Value;
                }
                else
                {
                    var artNum2Group = headingMatch.Groups["ArtNum2"];
                    artNum = artNum2Group.Value;
                }
                //We have located the specific heading pattern
                nextSection = new Section()
                {
                    InternalId = Guid.NewGuid(),
                    Language = currentSection.Language,
                    ArticleNumber = artNum
                };

                return true;
            }

            nextSection = currentSection;
            return false;
        }
    }
}