using System;

namespace MarcellFormatIndexer.Format
{
    public sealed class Vocabulary
    {
        private readonly uint[] iateVocabulary;
        private readonly uint[] evVocabulary;
        private readonly UInt16 evVocabularyOffset;
        public readonly uint vocabularySize;

        public Vocabulary(uint[] iateVocabulary, uint[] evVocabulary)
        {
            this.iateVocabulary = iateVocabulary;
            this.evVocabulary = evVocabulary;

            Array.Sort(this.iateVocabulary);
            Array.Sort(this.evVocabulary);

            evVocabularyOffset = Convert.ToUInt16(iateVocabulary.Length);
            vocabularySize = (uint)(iateVocabulary.Length + evVocabulary.Length);
        }

        public UInt16 GetPosition(bool isEuroVoc, uint termId)
        {
            return 0;
        }
    }
}