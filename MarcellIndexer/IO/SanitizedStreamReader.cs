using System;
using System.IO;

namespace Semantika.Marcell.Processor.IO
{
    public class SanitizedStreamReader : StreamReader
    {
        public SanitizedStreamReader(string filename) : base(filename)
        {
        }

        /* other ctors as needed */

        // this is the only one that XmlTextReader appears to use but
        // it is unclear from the documentation which methods call each other
        // so best bet is to override all of the Read* methods and Peek
        public override string ReadLine()
        {
            return Sanitize(base.ReadLine());
        }

        public override int Read()
        {
            int temp = base.Read();
            while (temp == 0x01 || temp == 0x14)
                temp = base.Read();
            return temp;
        }

        public override int Peek()
        {
            int temp = base.Peek();
            while (temp == 0x01 || temp == 0x14)
            {
                temp = base.Read();
                temp = base.Peek();
            }
            return temp;
        }

        public override int Read(char[] buffer, int index, int count)
        {
            int temp = base.Read(buffer, index, count);
            for (int x = index; x < buffer.Length; x++)
            {
                if (buffer[x] == 0x01 || buffer[x] == 0x14)
                {
                    for (int a = x; a < buffer.Length - 1; a++)
                        buffer[a] = buffer[a + 1];
                    temp--; //decrement the number of characters read
                }
            }
            return temp;
        }

        private static string Sanitize(string unclean)
        {
            if (unclean == null)
                return null;
            if (String.IsNullOrEmpty(unclean))
                return "";
            return unclean.Replace((0x01).ToString(), "").Replace(((char)0x14).ToString(), "");
        }
    }
}