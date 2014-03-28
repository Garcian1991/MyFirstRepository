using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CompressionFiles
{
    /// <summary>
    /// Представляет набор методов и средств для реализации потокобезопасной очереди
    /// </summary>
    /// <typeparam name="E"></typeparam>
    public class BoundedBuffer<E>
    {
        private readonly Semaphore availableItems;
        private readonly Semaphore availableSpaces;
        private readonly E[] items;
        private int putPosition = 0;
        private int takePosition = 0;
 
        /// <summary>
        /// Инициализирует новый экземпляр класса BoundedBuffer c очередью ограниченной емкостью.
        /// </summary>
        /// <param name="capacity">Емкость очереди</param>
        public BoundedBuffer(int capacity) {
            availableSpaces = new Semaphore(capacity, capacity);
            availableItems = new Semaphore(0, capacity);
            items = new E[capacity];
        }
 
        /// <summary>
        /// Добавление элемента в очередь
        /// </summary>
        /// <param name="item">Элемент на добавление</param>
        public void Add(E item)
        {
            availableSpaces.WaitOne();
            lock (items)
            {
                int i = putPosition;
                items[i] = item;
                putPosition = (++i == items.Length) ? 0 : i;
            }
            availableItems.Release();
        }
 
        /// <summary>
        /// Изъятие элемента из очереди
        /// </summary>
        /// <returns>Изымаемый элемент</returns>
        public E Take()
        {
            availableItems.WaitOne();
            E item;
            lock (items)
            {
                int i = takePosition;
                item = items[i];
                items[i] = default(E);
                takePosition = (++i == items.Length) ? 0 : i;
            }
            availableSpaces.Release();
            return item;
        }

    }
}
