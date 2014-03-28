using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace CompressionFiles
{
    /// <summary>
    /// Предоставляет набор методов с средств для компрессии и декомпрессии файлов
    /// в многопоточной среде и использованием принципа конвейера
    /// </summary>
    public class CompressionFilesWithConveyor
    {
        string _sourceFilePath;
        string _destinatinFilePath;
        const int _bufferSize = 4194304;
        Thread _readingThread;
        Thread[] _gzipThread;
        Thread _writingThread;
        BoundedBuffer<byte[]> _readingBufferCompressing; 
        BoundedBuffer<byte[]> _writingBuffer;
        BoundedBuffer<CompressingData> _readingBufferDecompressing;
        static ManualResetEvent _mre = new ManualResetEvent(false);
        int _length;
        int _parts;
        byte _success;
        string _out;

        /// <summary>
        /// Инициализирует новый экземпляр класса CompressionFilesWithConveyor
        /// </summary>
        /// <param name="sourceFilePath">Имя исходного файла</param>
        /// <param name="destinationFilePath">Имя конечного файла</param>
        public CompressionFilesWithConveyor(string sourceFilePath, string destinationFilePath)
        {
            _sourceFilePath = sourceFilePath;
            _destinatinFilePath = destinationFilePath;
            _readingBufferCompressing = new BoundedBuffer<byte[]>(4);
            _writingBuffer = new BoundedBuffer<byte[]>(4);
            _readingBufferDecompressing = new BoundedBuffer<CompressingData>(4); 
            _gzipThread = new Thread[2];
            _length = 0;
            _parts = 0;
            _success = 1;
            _out = "Ошибка компрессии/декомпрессии";
        }

        /// <summary>
        /// Выполняет компрессию файла в многопоточной среде
        /// </summary>
        public void Compress()
        {
            try
            {
                Console.WriteLine("Start compressing...");
                _readingThread = new Thread(ReadingFile);
                _gzipThread[0] = new Thread(CompressingFile);
                _writingThread = new Thread(WritingCompressedFile);
                // Запуск потока на чтение, сжатие и запись
                _readingThread.Start();
                _gzipThread[0].Start();
                _writingThread.Start();
                _readingThread.Join();
                _gzipThread[0].Join();
                _writingThread.Join();
                _success = 0;
            }
            catch (Exception ex)
            {
                _success = 1;
                Console.WriteLine(ex.Message);
            }
        }

        #region Приватные методы для работы компрессии
        /// <summary>
        /// Приватный метод для выполнения в потоке, осуществляет чтение исходного файла
        /// </summary>
        private void ReadingFile()
        {
            try
            {
                using (FileStream sourceFile = File.OpenRead(_sourceFilePath))
                {
                    int n;
                    _parts = (int)(sourceFile.Length / _bufferSize) + 1; // как только установили количество частей
                    _mre.Set(); // разрешаем работать остальным потокам
                    byte[] buffer;
                    int i = 1;
                    while ((n = sourceFile.Read(buffer = new byte[_bufferSize], 0, buffer.Length)) > 0) // пока не прочитан весь файл
                    {
                        _length = n;
                        _readingBufferCompressing.Add(buffer);
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _gzipThread[0].Abort();
                _writingThread.Abort();
                _readingThread.Abort();
            }
            
        }

        /// <summary>
        /// Приватный метод для выполнения в потоке, осуществляет компрессию элемента
        /// </summary>
        private void CompressingFile()
        {
            int part = _bufferSize;
            byte[] buffer;
            _mre.WaitOne(); // ждем разрешеня на компрессию
            try
            {
                for (int i = 0; i < _parts; )
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (GZipStream cs = new GZipStream(ms, CompressionMode.Compress))
                        {
                            buffer = _readingBufferCompressing.Take(); // извлекаем элемент из очереди
                            if (i == _parts - 1) // если это последний элемент, его длина будет иной
                                part = _length;
                            cs.Write(buffer, 0, part); // сжимаем элемент в поток
                        }
                        buffer = ms.ToArray(); // копируем поток в массив
                        _writingBuffer.Add(buffer); // заносим элемент на второй склад
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _readingThread.Abort();
                _writingThread.Abort();
                _gzipThread[0].Abort();
            }
        }

        /// <summary>
        /// Приватный метод для выполнения в потоке, осуществляет запись сжатого элемента в файл
        /// </summary>
        private void WritingCompressedFile()
        {
            byte[] buffer;
            _mre.WaitOne();
            try
            {
                using (FileStream destinationFile = File.Create(_destinatinFilePath + ".gz"))
                {
                    for (int i = 0; i < _parts; )
                    {
                        buffer = _writingBuffer.Take(); // извлекаем элемент из очереди
                        BitConverter.GetBytes(buffer.Length + 1).CopyTo(buffer, 4);
                        destinationFile.Write(buffer, 0, buffer.Length); // пишем элемент в архив
                        i++;
                    }
                    _out = String.Format("Компрессия выполнена успешно в {0}. Размер архива {1}", _destinatinFilePath, destinationFile.Length);
                    _success = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _readingThread.Abort();
                _gzipThread[0].Abort();
                _writingThread.Abort();
            }
        }
        #endregion

        /// <summary>
        /// Выполняет декомпрессию файла в многопоточной среде
        /// </summary>
        public void Decompress()
        {
            try
            {
                Console.WriteLine("Start decompressing...");
                _readingThread = new Thread(ReadingCompressedFile);
                _gzipThread[0] = new Thread(DecompressingFile);
                _writingThread = new Thread(WritingFile);
                _readingThread.Start();
                _gzipThread[0].Start();
                _writingThread.Start();
                _readingThread.Join();
                _gzipThread[0].Join();
                _writingThread.Join();
                _success = 0;
                _out = "Decompression finished";
            }
            catch (Exception ex)
            {
                _success = 1;
                Console.WriteLine(ex.Message);
            }
        }

        #region Приватные методы для работы декомпрессии
        /// <summary>
        /// Приватный метод для выполнения в потоке, осуществляет чтение сжатого файла
        /// </summary>
        private void ReadingCompressedFile()
        {
            try
            {
                using (FileStream sourceFile = File.OpenRead(_sourceFilePath))
                {
                    int compressedBufferLength = 0;
                    CompressingData compressingData;
                    byte[] buf = new byte[8];
                    while (sourceFile.Position < sourceFile.Length)
                    {
                        sourceFile.Read(buf, 0, 8);
                        compressedBufferLength = BitConverter.ToInt32(buf, 4);
                        compressingData.Buffer = new byte[compressedBufferLength + 1];
                        buf.CopyTo(compressingData.Buffer, 0);
                        sourceFile.Read(compressingData.Buffer, 8, compressedBufferLength - 9);
                        compressingData.Length = BitConverter.ToInt32(compressingData.Buffer, compressedBufferLength - 5);
                        _readingBufferDecompressing.Add(compressingData);
                    }
                    Thread.Sleep(1000);
                    _readingThread.Abort();
                }
            }
            catch (Exception)
            {
                _gzipThread[0].Abort();
                _writingThread.Abort();
                _readingThread.Abort();
            }
        }

        /// <summary>
        /// Приватный метод для выполнения в потоке, осуществляет декомпрессию элемента
        /// </summary>
        private void DecompressingFile()
        {
            try
            {
                while (true)
                {
                    CompressingData cd = _readingBufferDecompressing.Take();
                    byte[] buffer = new byte[cd.Length];
                    using (MemoryStream ms = new MemoryStream(cd.Buffer))
                    {
                        using (GZipStream cs = new GZipStream(ms, CompressionMode.Decompress))
                        {
                            cs.Read(buffer, 0, buffer.Length);
                            _writingBuffer.Add(buffer);
                        }
                    }
                }
            }
            catch (Exception)
            {
                _readingThread.Abort();
                _writingThread.Abort();
                _gzipThread[0].Abort();
            }
        }

        /// <summary>
        /// Приватный метод для выполнения в потоке, осуществляет запись элемента в файл
        /// </summary>
        private void WritingFile()
        {
            try
            {
                using (FileStream destinationFile = File.Create(_destinatinFilePath))
                {
                    while (true)
                    {
                        byte[] buffer = _writingBuffer.Take();
                        destinationFile.Write(buffer, 0, buffer.Length);
                    }
                }
            }
            catch (Exception)
            {
                _readingThread.Abort();
                _gzipThread[0].Abort();
                _writingThread.Abort();
            }
        }
        #endregion

    
        /// <summary>
        /// Консольное сообщение
        /// </summary>
        public string OutputMessage { get { return _out; }  }

        /// <summary>
        /// В случае успешной работы возвращает 0, в противном случае - 1
        /// </summary>
        public byte Success { get { return _success; }  }
    }

    /// <summary>
    /// Данные, необходимые для декомпрессии файла
    /// </summary>
    struct CompressingData
    {
        public byte[] Buffer; // сжатый буффер
        public int Length; // длина после декомпрессии
    }
}
