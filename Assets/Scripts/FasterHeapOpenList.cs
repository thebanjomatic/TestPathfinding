using Unity.Collections;
/// <summary>
/// A Priority Queue implemented as a Binary Heap. Enqueue and dequeue both have a complexity of O(log(n)).
/// 
/// The code is intended to maximise performance. It intentionally does not include safety checks,
/// and only offers the minimal API required to use it as the basis for Dijkstra or A* implementations.
/// </summary>
/// <typeparam name="K">The key (priority) type.</typeparam>
/// <typeparam name="V">The value type.</typeparam>
public struct FasterHeapOpenList : IOpenList {

    private struct Element {
        public int key;
        public int value;
    }

    private NativeArray<Element> data;
    private int capacity;
    private int supremum;
    private int size;

    public int Length => size;

    /// <summary>
    /// Create a new <c>BinaryHeap</c>.
    /// </summary>
    /// <param name="capacity">The maximum number of elements that can be inserted.</param>
    /// <param name="infimum">A key guaranteed to be smaller than any key that might be inserted.
    /// Inserting keys which compare as less or equal to the infimum will breaint the heap.</param>
    /// <param name="supremum">A key guaranteed to be greater than any key that might be inserted.
    /// Inserting keys which compare as greater or equal to the supremum will breaint the heap.</param>
    public void Initialize(int capacity, int infimum, int supremum) {
        this.data = new NativeArray<Element>(capacity + 2, Allocator.Temp);
        this.capacity = capacity;
        this.supremum = supremum;
        this.size = 0;

        SetKey(0, infimum);
        SetKey(capacity + 1, supremum);
        Clear();
    }

    private void SetKey(int index, int key) {
        var item = data[index];
        item.key = key;
        data[index] = item;
    }

    private void SetKeyValue(int index, int key, int value) {
        data[index] = new Element() { key = key, value = value };
    }

    /// <summary>
    /// Removes all keys and values from the <c>BinaryHeap</c>.
    /// </summary>
    private void Clear() {
        size = 0;
        int cap = capacity;
        for (int i = 1; i <= cap; ++i) {
            SetKeyValue(i, supremum, default(int));
        }
    }

    public void Enqueue(NativeArray<PathNode> pathNodes, int index) {
        var node = pathNodes[index];
        var fCost = node.fCost;
        Enqueue_Impl(index, fCost);
        node.SetInOpenList(pathNodes);
    }

    /// <summary>
    /// Enqueues an element.
    /// 
    /// Throws if the <c>BinaryHeap</c> is already full.
    /// </summary>
    /// <param name="value">The value to insert.</param>
    /// <param name="key">The key (priority) to insert.</param>
    private void Enqueue_Impl(int value, int key) {
        ++size;
        int hole = size;
        int pred = hole >> 1;
        int predKey = data[pred].key;
        while (predKey.CompareTo(key) > 0) {
            SetKeyValue(hole, predKey, data[pred].value);
            hole = pred;
            pred >>= 1;
            predKey = data[pred].key;
        }

        SetKeyValue(hole, key, value);
    }

    /// <summary>
    /// Removes the element with the smallest key (priority), and returns its value.
    /// 
    /// Use <c>Count()</c> to determine the current size of the queue. If the queue is empty this method will return a nonsensical value.
    /// </summary>
    /// <returns>The value that was dequeued.</returns>
    private int Dequeue_Impl() {
        int min = data[1].value;

        int hole = 1;
        int succ = 2;
        int sz = size;

        while (succ < sz) {
            int key1 = data[succ].key;
            int key2 = data[succ + 1].key;
            if (key1.CompareTo(key2) > 0) {
                succ++;
                SetKeyValue(hole, key2, data[succ].value);
            } else {
                SetKeyValue(hole, key1, data[succ].value);
            }
            hole = succ;
            succ <<= 1;
        }

        int bubble = data[sz].key;
        int pred = hole >> 1;
        while (data[pred].key.CompareTo(bubble) > 0) {
            data[hole] = data[pred];
            hole = pred;
            pred >>= 1;
        }

        SetKeyValue(hole, bubble, data[sz].value);

        SetKey(size, supremum);
        size = sz - 1;

        return min;
    }


    public int DequeueMin(NativeArray<PathNode> pathNodes) {
        PathNode node;
        do {
            node = pathNodes[Dequeue_Impl()];
            node.RemoveFromOpenList(pathNodes);
        } while (node.IsInClosedList);
        return node.index;
    }

    public void Dispose() {
        data.Dispose();
    }
}