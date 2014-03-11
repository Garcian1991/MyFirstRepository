using System;
using CompressionFiles;

namespace GZipTest
{
    class Program
    {
        static int Main(string[] args)
        {
            if ((args.Length == 0) || (args.Length < 3))
            {
                Console.WriteLine("Please enter a correct form.");
                Console.WriteLine("Usage: [compress | decompress] [<sourse file name> | <arhive file name>] [<arhive file name> | <sourse file name>]");
                return 1;
            }

            else if ((args[0] != "compress") && (args[0] != "decompress"))
            {
                Console.WriteLine("Please enter a correct form.");
                Console.WriteLine("Usage: [compress | decompress] [<sourse file name> | <arhive file name>] [<arhive file name> | <sourse file name>]");
                return 1;
            }

            string sourceFile = "";
            string arhiveFile = "";
            int s = 4194304;
            CompressionFilesMultyThread cFiles;

            if (args[0] == "compress")
            {
                sourceFile = args[1];
                arhiveFile = args[2];
                cFiles = new CompressionFilesMultyThread(sourceFile, arhiveFile, s);
                cFiles.Compress();
                Console.WriteLine(cFiles.OutputMessage);
                return cFiles.Success;
            }
            else
            {
                sourceFile = args[2];
                arhiveFile = args[1];
                cFiles = new CompressionFilesMultyThread(sourceFile, arhiveFile, s);
                cFiles.Decompress();
                Console.WriteLine(cFiles.OutputMessage);
                Console.WriteLine();
                return 1;
            }
        }
    }
}
