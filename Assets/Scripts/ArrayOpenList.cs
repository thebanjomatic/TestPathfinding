using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct ArrayOpenList : IOpenList {
    private NativeList<int> _data;
    private int maxSize;

    public void Initialize(int size) {
        _data = new NativeList<int>(size, Allocator.Temp);
        maxSize = 0;
    }

    public void Enqueue(NativeArray<PathNode> pathNodes, int index) {
        var node = pathNodes[index];
        if (!node.IsInOpenList) {
            _data.Add(node.index);
            node.SetInOpenList(pathNodes);
            maxSize = math.max(maxSize, _data.Length);
        }
    }

    public int DequeueMin(NativeArray<PathNode> pathNodes) {
        var minFCost = pathNodes[_data[0]].fCost;
        var minDataIndex = 0;
        for (int i = 1; i < _data.Length; i++) {
            int currIndex = _data[i];
            int fCost = pathNodes[currIndex].fCost;
            if (fCost < minFCost) {
                minDataIndex = i;
                minFCost = fCost;
            }
        }
        var minIndex = _data[minDataIndex];
        pathNodes[minIndex].RemoveFromOpenList(pathNodes);
        _data.RemoveAtSwapBack(minDataIndex);
        return minIndex;
    }

    public int Length => _data.Length;

    public void Dispose() {
        _data.Dispose();
        // Debug.Log("Max OpenList Size: " + maxSize);
    }
}
