using System;
using System.Collections.Generic;

enum EventType { Arrival, Finish }

class Event : IComparable<Event>
{
    public int Time;
    public EventType Type;
    public int Cashier;
    public int Articles;
    public int CompareTo(Event o)
    {
        int r = Time.CompareTo(o.Time);
        if (r != 0) return r;
        r = Type.CompareTo(o.Type);
        if (r != 0) return r;
        return Cashier.CompareTo(o.Cashier);
    }
}

class MinHeap<T> where T : IComparable<T>
{
    List<T> a = new List<T>();
    public int Count => a.Count;
    public void Add(T v) { a.Add(v); Up(a.Count - 1); }
    public T Pop() { var v = a[0]; int i = a.Count - 1; a[0] = a[i]; a.RemoveAt(i); if (a.Count > 0) Down(0); return v; }
    void Up(int i) { while (i > 0) { int p = (i - 1) / 2; if (a[i].CompareTo(a[p]) >= 0) break; var t = a[i]; a[i] = a[p]; a[p] = t; i = p; } }
    void Down(int i) { int c = a.Count; while (true) { int l = i * 2 + 1; int r = i * 2 + 2; if (l >= c) break; int m = r < c && a[r].CompareTo(a[l]) < 0 ? r : l; if (a[i].CompareTo(a[m]) <= 0) break; var t = a[i]; a[i] = a[m]; a[m] = t; i = m; } }
}

class Customer { public int Articles; }

class Program
{
    static void Main()
    {
        var first = Console.ReadLine().Split();
        int k = int.Parse(first[0]);
        int n = int.Parse(first[1]);
        bool full = first[2] == "F";
        int[] speed = new int[k];
        for (int i = 0; i < k; i++) speed[i] = int.Parse(Console.ReadLine());
        MinHeap<Event> events = new MinHeap<Event>();
        for (int i = 0; i < n; i++) { var s = Console.ReadLine().Split(); int t = int.Parse(s[0]); int a = int.Parse(s[1]); events.Add(new Event { Time = t, Type = EventType.Arrival, Articles = a }); }
        Queue<Customer>[] q = new Queue<Customer>[k];
        for (int i = 0; i < k; i++) q[i] = new Queue<Customer>();
        int lastTime = 0; int lastCashier = 0;
        while (events.Count > 0)
        {
            var e = events.Pop();
            int time = e.Time;
            if (e.Type == EventType.Arrival)
            {
                int best = 0; int len = q[0].Count;
                for (int i = 1; i < k; i++) { int l = q[i].Count; if (l < len) { len = l; best = i; } }
                q[best].Enqueue(new Customer { Articles = e.Articles });
                if (full) Console.WriteLine(time + ": enqueue " + best);
                if (len == 0) { var c = q[best].Peek(); int fin = time + c.Articles * speed[best]; events.Add(new Event { Time = fin, Type = EventType.Finish, Cashier = best }); if (full) Console.WriteLine(time + ": start " + best); }
            }
            else
            {
                lastTime = time; lastCashier = e.Cashier; q[e.Cashier].Dequeue();
                if (full) Console.WriteLine(time + ": finish " + e.Cashier);
                if (q[e.Cashier].Count > 0) { var c = q[e.Cashier].Peek(); int fin = time + c.Articles * speed[e.Cashier]; events.Add(new Event { Time = fin, Type = EventType.Finish, Cashier = e.Cashier }); if (full) Console.WriteLine(time + ": start " + e.Cashier); }
            }
        }
        Console.WriteLine((lastTime + 1) + ": close " + lastCashier);
    }
}
