using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace BouwNouGouwSimulation
{
    internal class Program
    {
        private static void Main()
        {
            var header = Console.ReadLine()!
                .Trim()
                .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            int k = int.Parse(header[0], CultureInfo.InvariantCulture);
            int n = int.Parse(header[1], CultureInfo.InvariantCulture);
            char mode = char.Parse(header[2]);

            var cashiers = new Cashier[k];
            var cashierNodes = new CashierNode[k];

            for (int i = 0; i < k; i++)
            {
                BigInteger speed = BigInteger.Parse(Console.ReadLine()!.Trim(), CultureInfo.InvariantCulture);
                cashiers[i] = new Cashier(speed);
                cashierNodes[i] = new CashierNode(i);
            }

            var eventQueue = new MinHeap<Event>();

            for (int i = 0; i < n; i++)
            {
                var parts = Console.ReadLine()!
                    .Trim()
                    .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                BigInteger arrivalTime = BigInteger.Parse(parts[0], CultureInfo.InvariantCulture);
                BigInteger items = BigInteger.Parse(parts[1], CultureInfo.InvariantCulture);

                eventQueue.Add(new Event(arrivalTime, new Customer(items)));
            }

            var cashierHeap = new MinHeap<CashierNode>();
            foreach (var node in cashierNodes) cashierHeap.Add(node);

            BigInteger lastFinishTime = -1;
            int lastFinishCashier = -1;

            while (eventQueue.Count > 0)
            {
                Event ev = eventQueue.ExtractMin();
                BigInteger t = ev.Time;

                switch (ev.Type)
                {
                    case EventType.Arrival:
                        {
                            CashierNode node = cashierHeap.Peek();
                            int cid = node.Id;
                            Cashier cshr = cashiers[cid];

                            if (mode == 'F')
                                Console.WriteLine($"{t}: enqueue {cid}");

                            if (!cshr.Busy)
                            {
                                BigInteger finish = t + ev.Customer.Items * cshr.Speed;
                                cshr.Busy = true;

                                if (mode == 'F')
                                    Console.WriteLine($"{t}: start {cid}");

                                eventQueue.Add(new Event(finish, cid));
                            }
                            else
                            {
                                cshr.Queue.Enqueue(ev.Customer);
                                node.Waiting++;
                                cashierHeap.UpdateItem(node);
                            }
                            break;
                        }

                    case EventType.Finish:
                        {
                            int cid = ev.CashierId;
                            Cashier cshr = cashiers[cid];

                            if (mode == 'F' || mode == 'D')
                                Console.WriteLine($"{t}: finish {cid}");

                            if (t > lastFinishTime || (t == lastFinishTime && cid > lastFinishCashier))
                            {
                                lastFinishTime = t;
                                lastFinishCashier = cid;
                            }

                            if (cshr.Queue.Count > 0)
                            {
                                Customer next = cshr.Queue.Dequeue();

                                CashierNode node = cashierNodes[cid];
                                node.Waiting--;
                                cashierHeap.UpdateItem(node);

                                if (mode == 'F')
                                    Console.WriteLine($"{t}: start {cid}");

                                BigInteger finish = t + next.Items * cshr.Speed;
                                eventQueue.Add(new Event(finish, cid));
                            }
                            else
                            {
                                cshr.Busy = false;
                            }
                            break;
                        }
                }
            }

            BigInteger closeTime = lastFinishTime + 1;
            Console.WriteLine($"{closeTime}: close {lastFinishCashier}");
        }
    }

    internal record struct Customer(BigInteger Items);

    internal enum EventType { Arrival, Finish }

    internal sealed class Event : IComparable<Event>, IHeapItem
    {
        public BigInteger Time { get; }
        public EventType Type { get; }
        public int CashierId { get; }
        public Customer Customer { get; }

        private int _heapIndex;

        public Event(BigInteger time, Customer customer)
        {
            Time = time;
            Type = EventType.Arrival;
            CashierId = -1;
            Customer = customer;
        }

        public Event(BigInteger time, int cashierId)
        {
            Time = time;
            Type = EventType.Finish;
            CashierId = cashierId;
            Customer = default;
        }

        public int CompareTo(Event other)
        {
            int cmp = Time.CompareTo(other.Time);
            if (cmp != 0) return cmp;

            if (Type != other.Type)
                return Type == EventType.Arrival ? -1 : 1;

            return CashierId.CompareTo(other.CashierId);
        }

        public void SetHeapIndex(int index) => _heapIndex = index;
        public int GetHeapIndex() => _heapIndex;
    }

    internal sealed class Cashier
    {
        public readonly BigInteger Speed;
        public readonly Queue<Customer> Queue = new();
        public bool Busy;

        public Cashier(BigInteger speed) => Speed = speed;
    }

    internal sealed class CashierNode : IComparable<CashierNode>, IHeapItem
    {
        public readonly int Id;
        public int Waiting;

        private int _heapIndex;

        public CashierNode(int id) => Id = id;

        public int CompareTo(CashierNode other)
        {
            int cmp = Waiting.CompareTo(other.Waiting);
            return cmp != 0 ? cmp : Id.CompareTo(other.Id);
        }

        public void SetHeapIndex(int index) => _heapIndex = index;
        public int GetHeapIndex() => _heapIndex;
    }

    internal sealed class MinHeap<T> where T : IComparable<T>, IHeapItem
    {
        private readonly List<T> _data = new();
        public int Count => _data.Count;

        public void Add(T item)
        {
            _data.Add(item);
            int idx = _data.Count - 1;
            item.SetHeapIndex(idx);
            BubbleUp(idx);
        }

        public T Peek() => _data[0];

        public T ExtractMin()
        {
            T root = _data[0];
            int lastIx = _data.Count - 1;
            Swap(0, lastIx);
            _data.RemoveAt(lastIx);
            if (_data.Count > 0) BubbleDown(0);

            root.SetHeapIndex(-1);
            return root;
        }

        public void UpdateItem(T item)
        {
            int idx = item.GetHeapIndex();
            if (idx < 0 || idx >= _data.Count) return;
            BubbleUp(idx);
            BubbleDown(idx);
        }

        private void BubbleUp(int idx)
        {
            while (idx > 0)
            {
                int parent = (idx - 1) >> 1;
                if (_data[idx].CompareTo(_data[parent]) >= 0) break;
                Swap(idx, parent);
                idx = parent;
            }
        }

        private void BubbleDown(int idx)
        {
            int count = _data.Count;
            while (true)
            {
                int left = (idx << 1) + 1;
                if (left >= count) break;
                int right = left + 1;

                int smallest = (right < count && _data[right].CompareTo(_data[left]) < 0) ? right : left;
                if (_data[idx].CompareTo(_data[smallest]) <= 0) break;

                Swap(idx, smallest);
                idx = smallest;
            }
        }

        private void Swap(int i, int j)
        {
            if (i == j) return;
            (_data[i], _data[j]) = (_data[j], _data[i]);
            _data[i].SetHeapIndex(i);
            _data[j].SetHeapIndex(j);
        }
    }

    internal interface IHeapItem
    {
        void SetHeapIndex(int index);
        int GetHeapIndex();
    }
}
