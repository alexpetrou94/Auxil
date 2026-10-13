using System.Runtime.CompilerServices;

namespace Auxil.Pathfinding.Engine;

// ---------------------------------------------------------------------------------------------
// Binary min-heap, lazy duplicates.
// ---------------------------------------------------------------------------------------------
internal struct BinaryHeapLazy
{
    private int[] _key;
    private int[] _id;
    private int _count;

    public BinaryHeapLazy(int capacity)
    {
        if (capacity < 4) capacity = 4;
        _key = new int[capacity];
        _id = new int[capacity];
        _count = 0;
    }

    public int Count => _count;

    public void Init(Grid grid)
    {
        // Growable: a modest preallocation is enough; warmup grows it to the steady-state size.
        int cap = Math.Min(grid.Size, 1 << 18);
        if (_key == null || _key.Length < cap)
        {
            _key = new int[cap];
            _id = new int[cap];
        }
        _count = 0;
    }

    public void Clear() => _count = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(int key, int id)
    {
        if (_count == _key.Length) Grow();
        int[] keys = _key, ids = _id;
        int i = _count++;
        while (i > 0)
        {
            int p = (i - 1) >> 1;
            if (keys[p] <= key) break;
            keys[i] = keys[p];
            ids[i] = ids[p];
            i = p;
        }
        keys[i] = key;
        ids[i] = id;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Pop()
    {
        int[] keys = _key, ids = _id;
        int id = ids[0];
        int n = --_count;
        if (n > 0)
        {
            int k = keys[n], v = ids[n];
            int i = 0;
            int half = n >> 1;
            while (i < half)
            {
                int c = 2 * i + 1;
                if (c + 1 < n && keys[c + 1] < keys[c]) c++;
                if (keys[c] >= k) break;
                keys[i] = keys[c];
                ids[i] = ids[c];
                i = c;
            }
            keys[i] = k;
            ids[i] = v;
        }
        return id;
    }

    private void Grow()
    {
        Array.Resize(ref _key, _key.Length * 2);
        Array.Resize(ref _id, _id.Length * 2);
    }
}


// ---------------------------------------------------------------------------------------------
// Dial's algorithm with lazy duplicates and plain per-bucket arrays. The cheapest possible
// bucket queue: push appends to the bucket array (no linking, no decrease-key), pop takes the
// newest entry and callers discard stale entries via their closed-set stamp. Keys must be
// non-decreasing (true for Dijkstra / A* with a consistent heuristic).
// ---------------------------------------------------------------------------------------------
internal struct DialBucketsLazy
{
    private int[]?[] _data;
    private int[] _size;
    private int _bucketCount;
    private int _cur;
    private int _max;
    private int _count;

    public DialBucketsLazy(int initialBuckets)
    {
        if (initialBuckets < 4) initialBuckets = 4;
        _data = new int[]?[initialBuckets];
        _size = new int[initialBuckets];
        _bucketCount = initialBuckets;
        _cur = 0;
        _max = -1;
        _count = 0;
    }

    public int Count => _count;

    public void Init(Grid grid)
    {
        int bc = DialHeapIndexed.DialBucketCount(grid);
        if (_data == null || _data.Length < bc)
        {
            _data = new int[]?[bc];
            _size = new int[bc];
        }
        else
        {
            Array.Clear(_size);
        }
        _bucketCount = bc;
        _cur = 0;
        _max = -1;
        _count = 0;
    }

    public void Clear()
    {
        for (int b = _cur; b <= _max; b++) _size[b] = 0;
        _cur = 0;
        _max = -1;
        _count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(int key, int id)
    {
        int b = key;
        if (b < _cur) b = _cur;
        if (b >= _bucketCount) GrowBuckets(b);
        int[]? data = _data[b];
        if (data == null)
        {
            data = new int[8];
            _data[b] = data;
        }
        int size = _size[b];
        if (size == data.Length)
        {
            Array.Resize(ref data, size * 2);
            _data[b] = data;
        }
        data[size] = id;
        _size[b] = size + 1;
        if (b > _max) _max = b;
        _count++;
    }

    private void GrowBuckets(int key)
    {
        int newCount = Math.Max(key + 1, _bucketCount * 2);
        Array.Resize(ref _data, newCount);
        Array.Resize(ref _size, newCount);
        _bucketCount = newCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Pop(out int key)
    {
        while (_size[_cur] == 0) _cur++;
        int size = --_size[_cur];
        int id = _data[_cur]![size];
        _count--;
        key = _cur;
        return id;
    }
}

// ---------------------------------------------------------------------------------------------
// Dial's algorithm (bucket queue keyed by integer f), indexed. Near-O(1) per operation when
// the key range is bounded. On grids f <= 2 * (W + H + layers) * 10 + constant.
// ---------------------------------------------------------------------------------------------
internal struct DialHeapIndexed
{
    private int[] _head;   // bucket -> first node in doubly-linked list
    private int[] _next;   // node -> next in bucket
    private int[] _prev;   // node -> previous in bucket
    private int[] _key;    // node -> f value (== bucket index)
    private int _cur;      // lowest non-empty bucket candidate
    private int _max;      // highest bucket ever used in this query
    private int _count;
    private int _bucketCount;

    public DialHeapIndexed(int capacity, int bucketCount)
    {
        if (capacity < 4) capacity = 4;
        _head = new int[bucketCount];
        Array.Fill(_head, -1);
        _next = new int[capacity];
        _prev = new int[capacity];
        Array.Fill(_prev, -1);
        _key = new int[capacity];
        _bucketCount = bucketCount;
        _cur = 0;
        _max = -1;
        _count = 0;
    }

    public int Count => _count;

    public void Init(Grid grid)
    {
        int cap = grid.Size;
        if (_next == null || _next.Length < cap)
        {
            _next = new int[cap];
            _prev = new int[cap];
            _key = new int[cap];
        }
        int bc = DialBucketCount(grid);
        if (_head == null || _head.Length < bc)
        {
            _head = new int[bc];
            Array.Fill(_head, -1);
        }
        _bucketCount = bc;
        _cur = 0;
        _max = -1;
        _count = 0;
    }

    public static int DialBucketCount(Grid grid)
        => 20 * (grid.Width + grid.Height + grid.Layers) + 64;

    public void Clear()
    {
        for (int b = _cur; b <= _max; b++) _head[b] = -1;
        _cur = 0;
        _max = -1;
        _count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Insert(int id, int key)
    {
        int b = key;
        if (b < _cur) b = _cur;               // monotone keys make this a no-op in practice
        if (b >= _bucketCount) GrowBuckets(b);
        _key[id] = b;
        int h = _head[b];
        _next[id] = h;
        _prev[id] = -1;
        if (h != -1) _prev[h] = id;
        _head[b] = id;
        if (b > _max) _max = b;
        _count++;
    }

    private void GrowBuckets(int key)
    {
        // Keys can legitimately exceed the W+H estimate on maze-like maps whose optimal costs are
        // winding and large; grow the bucket array (amortized during warmup, zero-cost afterwards).
        int newCount = Math.Max(key + 1, _bucketCount * 2);
        int oldCount = _bucketCount;
        Array.Resize(ref _head, newCount);
        Array.Fill(_head, -1, oldCount, newCount - oldCount);
        _bucketCount = newCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Decrease(int id, int key)
    {
        int b = key;
        if (b < _cur) b = _cur;
        if (b >= _bucketCount) GrowBuckets(b);
        int old = _key[id];
        if (old == b) { _key[id] = b; return; }

        // Unlink from the old bucket.
        int p = _prev[id], n = _next[id];
        if (p != -1) _next[p] = n; else _head[old] = n;
        if (n != -1) _prev[n] = p;

        // Push to the front of the new bucket.
        _key[id] = b;
        int h = _head[b];
        _next[id] = h;
        _prev[id] = -1;
        if (h != -1) _prev[h] = id;
        _head[b] = id;
        if (b > _max) _max = b;
    }

    public int Pop(out int key)
    {
        while (_cur <= _max && _head[_cur] == -1) _cur++;
        int id = _head[_cur];
        int n = _next[id];
        _head[_cur] = n;
        if (n != -1) _prev[n] = -1;
        _count--;
        key = _cur;
        return id;
    }
}
