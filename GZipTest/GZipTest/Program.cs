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

            // для отладки
            //string sourceFile = @"E:\Documents\Мои документы\GitHub\MyFirstRepository\GZipTest\GZipTest\bin\Debug\hh.txt";
            //string arhiveFile = @"E:\Documents\Мои документы\GitHub\MyFirstRepository\GZipTest\GZipTest\bin\Debug\hh.txt.gz";
            //int s = 100;//4194304;
            ////CompressionFiles.CompressionFiles cFiles = new CompressionFiles.CompressionFiles(sourceFile, arhiveFile, s);
            //CompressionFilesMultyThread cFiles = new CompressionFilesMultyThread(sourceFile, arhiveFile, s);
            ////CompressionFilesСonveyor cFiles = new CompressionFilesСonveyor(sourceFile, arhiveFile, s);
            //cFiles.Decompress();
            ////Console.WriteLine(cFiles.OutputMessage);
            //Console.ReadKey();
            //return 1;
            // конец отладки

            string sourceFile = "";
            string arhiveFile = "";
            int s = 4194304;
            CompressionFilesСonveyor cFiles;
            if (args[0] == "compress")
            {
                sourceFile = args[1];
                arhiveFile = args[2];
                cFiles = new CompressionFilesСonveyor(sourceFile, arhiveFile, s);
                cFiles.Compress();
                Console.WriteLine(cFiles.OutputMessage);
                return cFiles.Success;
            }
            else
            {
                sourceFile = args[2];
                arhiveFile = args[1];
                cFiles = new CompressionFilesСonveyor(sourceFile, arhiveFile, s);
                cFiles.Decompress();
                Console.WriteLine(cFiles.OutputMessage);
                Console.WriteLine();
                return 1;
            }
        }
    }
}
