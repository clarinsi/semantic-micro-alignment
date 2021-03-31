using Semantika.Marcell.LuceneStore.Index;
using Semantika.Marcell.Processor.Indexer;
using System;

namespace FileReaderTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (!(args.Length == 5) || !int.TryParse(args[0], out _))
            {
                Console.WriteLine("Usage: CorpusIndexer <indexMode> <sourcePath> <indexPath> <topicTbxFileUri> <LogPath>");
                Console.WriteLine("Source path must contain one folder per language (sl, hr, ...) with the Xml CoNLL-UP files.");
                Console.WriteLine("Index mode can be: 0=single index file per type, 1=index file per language");

                return;
            }

            CorpusIndexManager corpusIndexer = new CorpusIndexManager((IndexingMode)(int.Parse(args[0])), args[1], args[2], args[3], args[4], 0, false);
            //CorpusIndexer corpusIndexer = new CorpusIndexer(@"C:\Users\marko\source\repos\Marcell\Data\MarcellTesting\Source", $@"C:\Users\marko\source\repos\Marcell\Data\MarcellTesting\{DateTime.Now.Ticks}", "file:///C:/Users/marko/source/repos/Marcell/Data/MarcellTesting/Source/IATE_export.tbx", $@"C:\Users\marko\source\repos\Marcell\Data\MarcellTesting\{DateTime.Now.Ticks}-");

            corpusIndexer.PerformParallelIndex();
        }
    }
}