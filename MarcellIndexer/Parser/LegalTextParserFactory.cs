using System;
using TBXFormat;

namespace Semantika.Marcell.Processor.Parser
{
    public sealed class LegalTextParserFactory
    {
        private LegalTextParser bgParser;
        private LegalTextParser hrParser;
        private LegalTextParser huParser;
        private LegalTextParser plParser;
        private LegalTextParser roParser;
        private LegalTextParser skParser;
        private LegalTextParser slParser;

        public LegalTextParserFactory(Tbx tbxSource)
        {
            bgParser = new BgLegalTextParser(tbxSource);
            hrParser = new HrLegalTextParser(tbxSource);
            huParser = new HuLegalTextParser(tbxSource);
            plParser = new PlLegalTextParser(tbxSource);
            roParser = new RoLegalTextParser(tbxSource);
            skParser = new SkLegalTextParser(tbxSource);
            slParser = new SlLegalTextParser(tbxSource);
        }

        public LegalTextParser CreateParser(string lang)
        {
            switch (lang.ToLowerInvariant())
            {
                case "bg":
                    return bgParser;

                case "hr":
                    return hrParser;

                case "hu":
                    return huParser;

                case "pl":
                    return plParser;

                case "ro":
                    return roParser;

                case "sk":
                    return skParser;

                case "sl":
                    return slParser;

                default:
                    throw new ArgumentException("Unsupported language!");
            }
        }
    }
}