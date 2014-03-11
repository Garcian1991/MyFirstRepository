using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace CompressionFiles
{
    /// <summary>
    /// Kласс, реализующий многопоточную архивацию/разархивацию файлов.
    /// </summary>
    public class CompressionFilesMultyThread
    {
        int _threadNum; // номер потока
        string _fileName; // имя исходного файла
        string _archiveName; // имя архива
        int _bufSize; // размер буфера
        byte[][] _buffer; // буфер исходного файла
        byte[][] _cmprssBuf; // буфер для сжатия
        Thread[] _Threads; // потоки

        public CompressionFilesMultyThread()
        {
            _threadNum = 0;
            _fileName = "";
            _archiveName = "";
            _bufSize = 0;
            _buffer = null;
            _Threads = null;
            _cmprssBuf = null;
            Success = 1;
            OutputMessage = "";
        }

        /// <summary>
        /// Многопоточная архивация / разархивация файла
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <param name="archiveName">Имя архива</param>
        /// <param name="bufSize">Размер буффера для архивации/разархивации данных</param>
        public CompressionFilesMultyThread(string fileName, string archiveName, int bufSize)
        {
            _threadNum = Environment.ProcessorCount;
            _fileName = fileName;
            _archiveName = archiveName;
            OutputMessage = "";
            Success = 1;
            _bufSize = bufSize;
            _buffer = new byte[_threadNum][];
            _cmprssBuf = new byte[_threadNum][];
        }

        /// <summary>
        /// Многопоточная архивация файла
        /// </summary>
        public void Compress()
        {
            try
            {
                using (FileStream sourceFile = File.OpenRead(_fileName))
                {
                    using (FileStream destinationFile = File.Create(_archiveName + ".gz"))
                    {
                        int bufPart = 0; // часть буфера
                        Console.WriteLine("Start compressing...");
                        // пока мы не прошлись по всему исходному файлу
                        while (sourceFile.Position < sourceFile.Length)
                        {
                            _Threads = new Thread[_threadNum];
                            // делим буфер на количество потоков
                            for (int thrd = 0; thrd < _threadNum && sourceFile.Position < sourceFile.Length; thrd++)
                            {
                                // если размер буфера превышает размер остатка файла, обрезаем размер буфера
                                if (sourceFile.Length - sourceFile.Position <= _bufSize)
                                {
                                    bufPart = (int)(sourceFile.Length - sourceFile.Position);
                                }
                                else { bufPart = _bufSize; }

                                _buffer[thrd] = new byte[bufPart]; // определяем часть файла потоку
                                sourceFile.Read(_buffer[thrd], 0, bufPart); // читаем его

                                // создаем новый поток для сжатия и запускаем его
                                _Threads[thrd] = new Thread(
                                    delegate(object i)
                                    {
                                        // компрессия выполняется в MemoryStream, результат записывается в буфер сжатия
                                        // таким образом выполнение не зависит от скорости диска
                                        using (MemoryStream ms = new MemoryStream(_buffer[(int)i].Length))
                                        {
                                            using (GZipStream gz= new GZipStream(ms, CompressionMode.Compress))
                                            {
                                                gz.Write(_buffer[(int)i], 0, _buffer[(int)i].Length);
                                            }
                                            _cmprssBuf[(int)i] = ms.ToArray();
                                        }
                                    }
                                );
                                _Threads[thrd].Start(thrd);
                            }

                            // по завершению работы потоков, записываем результат из буфера сжатия в файл
                            for (int i = 0; i < _threadNum && _Threads[i] != null; i++) // проверка на null служит для случая, если поток оказался неиспользуемым
                            {
                                _Threads[i].Join(); // ожидаем завершение потока
                                BitConverter.GetBytes(_cmprssBuf[i].Length + 1).CopyTo(_cmprssBuf[i], 4);
                                destinationFile.Write(_cmprssBuf[i], 0, _cmprssBuf[i].Length);
                            }
                        }
                        OutputMessage = String.Format("Compressed {0} to {1}.", sourceFile.Name, destinationFile.Name);
                        Success = 0;
                    }
                }
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
        /// Многопоточная разархивация файла
        /// </summary>
        public void Decompress()
        {
            try
            {
                using (FileStream sourceFile = File.OpenRead(_archiveName))
                {
                    using (FileStream destinationFile = File.Create(_fileName))
                    {
                        int bufPart = 0;
                        int ComprLen = 0;
                        byte[] buf = new byte[8];
                        Console.WriteLine("Start decompressing...");

                        // пока мы не прошлись по всему исходному файлу
                        while (sourceFile.Position < sourceFile.Length)
                        {
                            _Threads = new Thread[_threadNum];
                            // делим буфер на количество потоков

                            for (int thrd = 0; thrd < _threadNum && sourceFile.Position < sourceFile.Length; thrd++)
                            {
                                sourceFile.Read(buf, 0, 8);
                                ComprLen = BitConverter.ToInt32(buf, 4);
                                _cmprssBuf[thrd] = new byte[ComprLen + 1];
                                buf.CopyTo(_cmprssBuf[thrd], 0);

                                sourceFile.Read(_cmprssBuf[thrd], 8, ComprLen - 9);
                                bufPart = BitConverter.ToInt32(_cmprssBuf[thrd], ComprLen - 5);
                                _buffer[thrd] = new byte[bufPart];
                                // создаем новый поток для архивирования и запускаем его
                                _Threads[thrd] = new Thread(
                                    delegate(object i)
                                    {
                                        // декомпрессия выполняется в MemoryStream, результат записывается в буфер 
                                        // таким образом выполнение не зависит от скорости диска
                                        using (MemoryStream ms = new MemoryStream(_cmprssBuf[(int)i]))
                                        {
                                            using (GZipStream gz = new GZipStream(ms, CompressionMode.Decompress))
                                            {
                                                gz.Read(_buffer[(int)i], 0, _buffer[(int)i].Length);
                                            }
                                        }
                                    }
                                );
                                _Threads[thrd].Start(thrd);
                            }

                            // по завершению работы потоков, записываем результат из буфера сжатия в файл
                            for (int i = 0; i < _threadNum && _Threads[i] != null; i++) // проверка на null служит для случая, если поток оказался неиспользуемым
                            {
                                _Threads[i].Join(); // ожидаем завершение потока
                                destinationFile.Write(_buffer[i], 0, _buffer[i].Length);
                            }
                        }
                        OutputMessage = String.Format("Decompressed {0} to {1}.", sourceFile.Name, destinationFile.Name);
                        Success = 0;
                    }
                }
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
        /// Консольное сообщение
        /// </summary>
        public string OutputMessage { get; private set; }

        /// <summary>
        /// В случае успешной работы возвращает 0, в противном случае - 1
        /// </summary>
        public short Success { get; private set; }
    }
}
