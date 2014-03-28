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
            CompressionFilesWithConveyor cFiles;
            if (args[0] == "compress")
            {
                sourceFile = args[1];
                arhiveFile = args[2];
                cFiles = new CompressionFilesWithConveyor(sourceFile, arhiveFile);
                cFiles.Compress();
                Console.WriteLine(cFiles.OutputMessage);
                return cFiles.Success;
            }
            else
            {
                sourceFile = args[1];
                arhiveFile = args[2];
                cFiles = new CompressionFilesWithConveyor(sourceFile, arhiveFile);
                cFiles.Decompress();
                Console.WriteLine(cFiles.OutputMessage);
                Console.WriteLine();
                return cFiles.Success;
            }
        }
    }
}
