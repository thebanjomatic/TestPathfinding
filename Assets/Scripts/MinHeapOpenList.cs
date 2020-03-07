using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct MinHeapOpenList : IOpenList {
    private struct KVP {
        public int fCost;
        public int Index;
    }

    private NativeList<KVP> _data;
    private NativeArray<int> _indexMap;
    private int maxSize;
    private int existingHitCount;

    public void Initialize(int initialCapacity, int infimum, int supremum) {
        _data = new NativeList<KVP>(initialCapacity, Allocator.Temp);
        _indexMap = new NativeArray<int>(initialCapacity, Allocator.Temp);
        for (int i = 0; i < initialCapacity; i++) {
            _indexMap[i] = -1;
        }
        maxSize = 0;
    }

    public int PeekMin() {
        return _data[0].Index;
    }

    public int DequeueMin(NativeArray<PathNode> pathNodes) {
        var min = _data[0].Index;
        _data.RemoveAtSwapBack(0);
        if (Length > 0) {
            _indexMap[_data[0].Index] = 0;
        }
        _indexMap[min] = -1;
        HeapifyDown(0);
        pathNodes[min].RemoveFromOpenList(pathNodes);
        return min;
    }

    public void Enqueue(NativeArray<PathNode> pathNodes, int index) {
        var node = pathNodes[index];
        var fCost = node.fCost;
        node.SetInOpenList(pathNodes);

        var existingIndex = _indexMap[index];
        if (_indexMap[index] != -1) {
            var existingData = _data[existingIndex];
            if (fCost == existingData.fCost) {
                return;
            } else if (fCost > existingData.fCost) {
                _data[existingIndex] = new KVP() { fCost = fCost, Index = index };
                HeapifyDown(existingIndex);                
            } else {
                _data[existingIndex] = new KVP() { fCost = fCost, Index = index };
                HeapifyUp(existingIndex);
            }
        } else {
            _data.Add(new KVP { fCost = fCost, Index = index });
            _indexMap[index] = _data.Length - 1;
            HeapifyUp(_data.Length - 1);
        }
        maxSize = math.max(maxSize, _data.Length);
    }

    public bool Contains(int index) {
        return _indexMap[index] != -1;
    }

    public int Length => _data.Length;

    private void HeapifyUp(int i) {
        while (true) {
            if (i == 0) {
                return;
            }

            int parentIndex = (i - 1) / 2;
            if (isFCostGT(parentIndex, i)) {
                SwapData(parentIndex, i);
                i = parentIndex;
            } else {
                return;
            }
        }
    }

    private void HeapifyDown(int index) {
        int size = _data.Length;

        while (index < size) {
            int left = 2 * index + 1;
            int right = 2 * index + 2;

            if (left >= size) {
                break;
            }
            var minIndex = index;

            if (isFCostGT(index, left)) {
                minIndex = left;
            }

            if (right < size && isFCostGT(minIndex, right)) {
                minIndex = right;
            }

            if (minIndex == index) {
                break;
            }

            SwapData(index, minIndex);
            index = minIndex;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool isFCostGT(int l_index, int r_index) {
        return _data[l_index].fCost > _data[r_index].fCost;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SwapData(int l_index, int r_index) {
        var temp = _data[l_index];
        _data[l_index] = _data[r_index];
        _data[r_index] = temp;
        _indexMap[_data[r_index].Index] = r_index;
        _indexMap[_data[l_index].Index] = l_index;
    }

    public void Dispose() {
        _data.Dispose();
        _indexMap.Dispose();
        // Debug.Log("Existing Item Hits: " + existingHitCount);
    }
}
