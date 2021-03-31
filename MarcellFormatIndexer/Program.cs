using System;

namespace MarcellFormatIndexer
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Random rnd = new Random();
            uint[] testa, testb;
            testa = new uint[5000];
            testb = new uint[5000];
            for (int i = 0; i < 5000; i++)
            {
                testa[i] = (uint)rnd.Next(0, 10000);
                testb[i] = (uint)rnd.Next(0, 10000);
            }

            uint maxcountst = 0, maxcountp = 0, maxcountse = 0, maxcountd = 0;
            uint idx = 0;

            int a = rnd.Next(10000);
            int b = rnd.Next(10000);

            for (int i = 0; i < 40000000; i++)
            {
                if (i % 100000 == 0)
                {
                    Console.WriteLine(DateTime.Now + ": " + i);
                }
                int posa = 0, posb = 0, posc = 0;
                uint countst = 0, countp = 0, countse = 0, countd = 0;

                for (int j = 0; j < 10000; j++)
                {
                    if (a < b)
                    {
                        posa++;
                    }
                    else
                    {
                        posb++;
                    }
                    if (posa >= 5000)
                    {
                        posa = 4999;
                        posb++;
                    }
                    if (posb >= 5000)
                    {
                        posb = 4999;
                        posa++;
                    }

                    if (posa < 5000)
                    {
                        uint value = testa[posa] & testa[posb];
                        countd += System.Runtime.Intrinsics.X86.Popcnt.PopCount(value & 0x0000FFFF);
                        countst += System.Runtime.Intrinsics.X86.Popcnt.PopCount(value & 0x0000F0F0);
                        countp += System.Runtime.Intrinsics.X86.Popcnt.PopCount(value & 0x00000F0F);
                        countse += System.Runtime.Intrinsics.X86.Popcnt.PopCount(value & 0x0000F00F);
                    }
                }

                if (countd > maxcountd)
                {
                    maxcountd = countd;
                    idx = 2;
                }
                if (countst > maxcountst)
                {
                    maxcountst = countst;
                    idx = 3;
                }
                if (countp > maxcountp)
                {
                    maxcountp = countp;
                    idx = 4;
                }
                if (countse > maxcountse)
                {
                    maxcountse = countse;
                    idx = 5;
                }
            }
        }
    }
}