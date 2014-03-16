using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace CompressionFiles
{
    /// <summary>
    /// Клаcc реализующий принцип конвейера
    /// </summary>
    public class CompressionFilesСonveyor
    {
        string _sourceFile;
        string _destinationFile;
        byte[] _buffer;
        byte[] _compressedBuffer;
        int _bufferSize;
        int _lastPart;
        Thread _thrdReading;
        Thread _thrdCompressing;
        Thread _thrdWriting;
        Storage _storageOne;
        Storage _storageTwo;

        public CompressionFilesСonveyor(string sourceFile, string destinationFile, int size)
        {
            _sourceFile = sourceFile;
            _destinationFile = destinationFile;
            _bufferSize = size;
        }

         public void Compress()
        {
            try
            {
                Console.WriteLine("Start compressing...");
                _thrdReading = new Thread(ReadingC);
                _thrdCompressing = new Thread(Compressing);
                _thrdWriting = new Thread(WritingC);
                _thrdReading.Start();
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
        /// Метод, предозачающийся для выполения в потоке(чтение исходного файла для компрессии). 
        /// Обязательно должен идти в паре
        /// с поточным методом, который забирает элементы очереди
        /// </summary>
        private void ReadingC()
        {
            using (FileStream source = File.OpenRead(_sourceFile))
            {
                int patrs = (int)source.Length / _bufferSize + 1;                
                _lastPart = (int)source.Length - _bufferSize * (patrs - 1);
                _storageOne = new Storage(patrs, 10);

                _thrdCompressing.Start(patrs);
                for (int i = 0; i < patrs; )
                {
                    if (_storageOne.CanEnqeue == true)
                    {
                        _buffer = new byte[_bufferSize];
                        source.Read(_buffer, 0, _buffer.Length);
                        _storageOne.Enqueue(_buffer);
                        i++;
                    }
                }
            }
        }

        /// <summary>
        /// Метод, предозачающийся для выполения в потоке(компрессия файла). Обязательно должен идти в паре
        /// с поточным методом, который забирает элементы очереди
        /// </summary>
        /// <param name="parts"></param>
        private void Compressing(object parts)
        {
            int part = _bufferSize;
            _storageTwo = new Storage((int)parts, 10);

            _thrdWriting.Start(parts);
            for (int i = 0; i < (int)parts; )
            {
                if (_storageOne.CanDequeu == true && _storageTwo.CanEnqeue)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (GZipStream cs = new GZipStream(ms, CompressionMode.Compress))
                        {
                            _compressedBuffer = _storageOne.Dequeue();
                            if (i == (int)parts - 1)
                                part = _lastPart;
                            cs.Write(_compressedBuffer, 0, part);
                        }
                        _compressedBuffer = ms.ToArray();
                        _storageTwo.Enqueue(_compressedBuffer);
                        i++;
                    }
                }
            }
        }

        /// <summary>
        /// Метод, предозачающийся для выполения в потоке (Запись архива). Обязательно должен идти в паре
        /// с поточным методом, который предоставляет элементы очереди
        /// </summary>
        /// <param name="parts"></param>
        private void WritingC(object parts)
        {
            int part = _bufferSize;
            byte[] res;
            using (FileStream dest = new FileStream(_destinationFile + ".gz", FileMode.Create, FileAccess.Write))
            {
                for (int i = 0; i < (int)parts; )
                {
                    if (_storageTwo.CanDequeu == true)
                    {
                        res = _storageTwo.Dequeue();
                        BitConverter.GetBytes(res.Length + 1).CopyTo(res, 4);
                        dest.Write(res, 0, res.Length);
                        i++;
                    }
                }
            }
            OutputMessage = "Compression finished";
            Success = 0;
        }

        public void Decompress()
        {
            try
            {
                Console.WriteLine("Start decompressing...");
                _thrdReading = new Thread(ReadingD);
                _thrdCompressing = new Thread(Decompressing);
                _thrdWriting = new Thread(WritingD);
                _thrdReading.Start();
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
        /// Метод, предозачающийся для выполения в потоке(чтение исходного файла при декомпрессии). 
        /// Обязательно должен идти в паре
        /// с поточным методом, который забирает элементы очереди
        /// </summary>
        private void ReadingD()
        {
            using (FileStream source = File.OpenRead(_sourceFile))
            {
                int ComprLen = 0;
                Pair pair;
                byte[] buf = new byte[8];
                _storageOne = new Storage(3);
                _thrdCompressing.Start();
                while (source.Position < source.Length)
                {
                    if (_storageOne.CanEnqeuePair == true)
                    {
                        source.Read(buf, 0, 8);
                        ComprLen = BitConverter.ToInt32(buf, 4);
                        pair.array = new byte[ComprLen + 1];
                        buf.CopyTo(pair.array, 0);
                        source.Read(pair.array, 8, ComprLen - 9);
                        pair.Length = BitConverter.ToInt32(pair.array, ComprLen - 5);
                        _storageOne.Enqueue(pair);
                    }
                }
                _storageOne.EndStream = true;
            }
            
        }

        private void Decompressing()
        {
            _storageTwo = new Storage(3);
            _thrdWriting.Start();
            while (!_storageOne.EndStream || _storageOne.PairCount != 0)
            {
                if (_storageOne.CanDequeuPair == true && _storageTwo.CanEnqeuePair)
                {
                    Pair p = _storageOne.DequeueDecompress();
                    _buffer = new byte[p.Length];
                    using (MemoryStream ms = new MemoryStream(p.array))
                    {
                        using (GZipStream cs = new GZipStream(ms, CompressionMode.Decompress))
                        {
                            cs.Read(_buffer, 0, _buffer.Length);
                            _storageTwo.Enqueue(_buffer);
                        }
                    }
                }
            }
            _storageTwo.EndStream = true;         
        }

        private void WritingD()
        {
            using (FileStream dest = new FileStream(_destinationFile, FileMode.Create, FileAccess.Write))
            {
                while (!_storageTwo.EndStream || _storageTwo.BoxCount != 0)
                {
                    if (_storageTwo.CanDequeu == true)
                    {
                        byte[] array = _storageTwo.Dequeue();
                        dest.Write(array, 0, array.Length);
                    }
                }
            }
            Success = 0;
            OutputMessage = "Decompression finished";
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

    /// <summary>
    /// Класс, хранящий очередь, которая представляет собой хранилище для 
    /// конвейера
    /// </summary>
    class Storage
    {
        Queue<byte[]> _Box; // сама очередь
        Queue<Pair> _pairBox; // очередь из пар значений
        int _partsCount; // количество частей, которые мы будем обрабатывать, не обязательно
        int _capacity; // допустимая длина очереди

        /// <summary>
        /// Указыаем, что помещаем в очередь
        /// </summary>
        /// <param name="PartsCount">Общее количество которое пойдет в Storage</param>
        /// <param name="Capacity">Допустимая длина очереди (слишком длинная очередь может привести
        /// к переполению оперативной памяти)</param>
        public Storage(int PartsCount, int Capacity)
        {
            _pairBox = new Queue<Pair>();
            _Box = new Queue<byte[]>();
            _partsCount = PartsCount;
            _capacity = Capacity;
            EndStream = false;
        }

        /// <summary>
        /// Указыаем, что помещаем в очередь
        /// </summary>
        /// <param name="Capacity">Допустимая длина очереди (слишком длинная очередь может привести
        /// к переполению оперативной памяти)</param>
        public Storage(int Capacity)
        {
            _Box = new Queue<byte[]>();
            _pairBox = new Queue<Pair>();
            _partsCount = 0;
            _capacity = Capacity;
            EndStream = false;
        }

        /// <summary>
        /// Становление в очередь пары значений
        /// </summary>
        /// <param name="pair">Пара значений</param>
        public void Enqueue(Pair pair)
        {
            _pairBox.Enqueue(pair);
        }

        /// <summary>
        /// Становление в очередь простого массива
        /// </summary>
        /// <param name="array">Массив байт</param>
        public void Enqueue(byte[] array)
        {
            _Box.Enqueue(array);
            _partsCount--;
        }

        /// <summary>
        /// Вывод из очереди
        /// </summary>
        /// <returns>Массив байт</returns>
        public byte[] Dequeue()
        {
            try
            {
                byte[] array = _Box.Dequeue();
                return array;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        /// <summary>
        /// Вывод из очереди
        /// </summary>
        /// <returns>Пара значений</returns>
        public Pair DequeueDecompress()
        {
            try
            {
                Pair pair = _pairBox.Dequeue();
                return pair;
            }
            catch (InvalidOperationException)
            {
                return new Pair(null, 0);
            }
        }

        /// <summary>
        /// Количество частей, ожидающих поставку
        /// </summary>
        public int PartrsLeft
        {
            get
            {
                return _partsCount;
            }
        }


        /// <summary>
        /// Текущее количество элементов в очереди
        /// </summary>
        public int BoxCount
        {
            get
            {
                return _Box.Count;
            }
        }

        /// <summary>
        /// Текущее количество элементов в очереди
        /// </summary>
        public int PairCount
        {
            get
            {
                return _pairBox.Count;
            }
        }

        /// <summary>
        /// Есть ли что брать из очереди?
        /// </summary>
        public bool CanDequeu
        {
            get
            {
                if (BoxCount == 0)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// Можно ли поставить в очередь. Ограничение на 10 и более элементов в очереди.
        /// </summary>
        public bool CanEnqeue
        {
            get
            {
                if (BoxCount >= _capacity)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// Есть ли что брать из очереди?
        /// </summary>
        public bool CanDequeuPair
        {
            get
            {
                if (PairCount == 0)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// Можно ли поставить в очередь. Ограничение на 10 и более элементов в очереди.
        /// </summary>
        public bool CanEnqeuePair
        {
            get
            {
                if (PairCount >= _capacity)
                    return false;
                else
                    return true;
            }
        }


        public bool EndStream { get; set; }
    }

    /// <summary>
    /// Пара, созданная для успешной декомпрессии
    /// </summary>
    struct Pair
    {
        public byte[] array;
        public int Length;

        public Pair(byte[] _array, int _Length)
        {
            array = _array;
            Length = _Length;
        }


    }


}
