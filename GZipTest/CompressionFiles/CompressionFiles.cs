using System;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace CompressionFiles
{
    /// <summary>
    /// Простой класс для архивации/разархивации файлов
    /// </summary>
    public class CompressionFiles
    {
        string _fileName; // имя исходного файла
        string _archiveName; // имя архива
        int _bufSize; // размер буфера
        byte[] _buffer; // буфер

        public CompressionFiles()
        {
            _fileName = "";
            _archiveName = "";
            OutputMessage = "";
            _bufSize = 0;
            _buffer = null;
            Success = 1;
        }

        /// <summary>
        /// Архивация / разархивация файла
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <param name="archiveName">Имя архива</param>
        /// <param name="bufSize">Размер буффера для архивации/разархивации данных</param>
        public CompressionFiles(string fileName, string archiveName, int bufSize)
        {
            _fileName = fileName;
            _archiveName = archiveName;
            OutputMessage = "";
            Success = 1;
            _bufSize = bufSize;
            _buffer = new byte[_bufSize];
        }
        
        /// <summary>
        /// Архивация исходного файла
        /// </summary>
        public void Compress()
        {
            try
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                using (FileStream sourceFile = File.OpenRead(_fileName))
                {
                    using (FileStream destinationFile = File.Create(_archiveName + ".gz"))
                    {
                        using (GZipStream output = new GZipStream(destinationFile, CompressionMode.Compress))
                        {
                            Console.WriteLine("Start compressing...");
                            int n;
                            while ((n = sourceFile.Read(_buffer, 0, _buffer.Length)) > 0)
                            {
                                output.Write(_buffer, 0, n);
                            }
                            Console.WriteLine("Compressed {0} from {1} to {2} bytes.",
                            sourceFile, sourceFile.Length.ToString(), destinationFile.Length.ToString());
                            OutputMessage = String.Format("Compressed {0} to {1}.", sourceFile.Name, destinationFile.Name);
                            Success = 0;
                        }
                    }
                }
                watch.Stop();
                TimeSpan ts = watch.Elapsed;
                string elapsedTime = String.Format("\nВремя компрессии: {0:00}:{1:00}.{2:00}",
                                                    ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                OutputMessage += elapsedTime;
            }
            #region Исключения
            catch (InvalidDataException)
            {
                OutputMessage = "Error: The file being read contains invalid data.";
                Success = 1;
            }
            catch (FileNotFoundException)
            {
                OutputMessage = "Error:The file specified was not found.";
                Success = 1;
            }
            catch (ArgumentException)
            {
                OutputMessage = "Error: path is a zero-length string, contains only white space, or contains one or more invalid characters";
                Success = 1;
            }
            catch (PathTooLongException)
            {
                OutputMessage = "Error: The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.";
                Success = 1;
            }
            catch (DirectoryNotFoundException)
            {
                OutputMessage = "Error: The specified path is invalid, such as being on an unmapped drive.";
                Success = 1;
            }
            catch (IOException)
            {
                OutputMessage = "Error: An I/O error occurred while opening the file.";
                Success = 1;
            }
            catch (UnauthorizedAccessException)
            {
                OutputMessage = "Error: path specified a file that is read-only, the path is a directory, or caller does not have the required permissions.";
                Success = 1;
            }
            catch (OutOfMemoryException)
            {
                OutputMessage = "Error: Не хватает оперативной памяти.";
                Success = 1;
            }
            catch (IndexOutOfRangeException)
            {
                OutputMessage = "Error: You must provide parameters for MyGZIP.";
                Success = 1;
            }
            catch (Exception)
            {
                OutputMessage = "Error: Undefined error.";
                Success = 1;
            }
            #endregion
        }

        /// <summary>
        /// Разархивация архива
        /// </summary>
        public void Decompress()
        {
            try
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                using (FileStream sourceFile = File.OpenRead(_archiveName))
                {
                    using (FileStream destinationFile = File.Create(_fileName))
                    {
                        using (GZipStream input = new GZipStream(sourceFile, CompressionMode.Decompress, false))
                        {
                            Console.WriteLine("Start decompressing...");
                            int n;
                            while ((n = input.Read(_buffer, 0, _buffer.Length)) > 0)
                            {
                                destinationFile.Write(_buffer, 0, n);
                            }
                            OutputMessage = String.Format("Decompressed {0} to {1}.", sourceFile.Name, destinationFile.Name);
                            Success = 0;
                        }
                    }
                }
                watch.Stop();
                TimeSpan ts = watch.Elapsed;
                string elapsedTime = String.Format("\nВремя компрессии: {0:00}:{1:00}.{2:00}",
                                                    ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                OutputMessage += elapsedTime;
            }
            #region Исключения
            catch (InvalidDataException)
            {
                OutputMessage = "Error: The file being read contains invalid data.";
                Success = 1;
            }
            catch (FileNotFoundException)
            {
                OutputMessage = "Error:The file specified was not found.";
                Success = 1;
            }
            catch (ArgumentException)
            {
                OutputMessage = "Error: path is a zero-length string, contains only white space, or contains one or more invalid characters";
                Success = 1;
            }
            catch (PathTooLongException)
            {
                OutputMessage = "Error: The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.";
                Success = 1;
            }
            catch (DirectoryNotFoundException)
            {
                OutputMessage = "Error: The specified path is invalid, such as being on an unmapped drive.";
                Success = 1;
            }
            catch (IOException)
            {
                OutputMessage = "Error: An I/O error occurred while opening the file.";
                Success = 1;
            }
            catch (OutOfMemoryException)
            {
                OutputMessage = "Error: Не хватает оперативной памяти.";
                Success = 1;
            }
            catch (UnauthorizedAccessException)
            {
                OutputMessage = "Error: path specified a file that is read-only, the path is a directory, or caller does not have the required permissions.";
                Success = 1;
            }
            catch (IndexOutOfRangeException)
            {
                OutputMessage = "Error: You must provide parameters for MyGZIP.";
                Success = 1;
            }
            catch (Exception)
            {
                OutputMessage = "Error: Undefined error.";
                Success = 1;
            }
            #endregion

        }

        /// <summary>
        /// Консольное сообщение
        /// </summary>
        public string OutputMessage { get; private set; }

        /// <summary>
        /// В случае успешной работы возвращает 0, в противном случае - 1
        /// </summary>
        public short Success { get; private set; }
    }
}
