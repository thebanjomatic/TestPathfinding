using System;
using Unity.Collections;
using Unity.Mathematics;

public struct PathNode {
    public int2 position;

    public int index;

    public int gCost;
    public int hCost;
    public int fCost;

    public PathNodeFlags flags;

    public void CalculateFCost() {
        fCost = gCost + hCost;
    }

    public bool IsWalkable => (flags & PathNodeFlags.IsWall) == 0;

    public bool IsInOpenList => (flags & PathNodeFlags.IsInOpenList) != 0;

    public bool IsInClosedList => (flags & PathNodeFlags.IsInClosedList) != 0;

    public bool IsBlockedOrClosed => (flags & (PathNodeFlags.IsInClosedList | PathNodeFlags.IsWall)) != 0;

    public void SetIsWall(NativeArray<PathNode> pathNodes) {
        flags |= PathNodeFlags.IsWall;
        pathNodes[index] = this;
    }

    public void SetInClosedList(NativeArray<PathNode> pathNodes) {
        flags |= PathNodeFlags.IsInClosedList;
        pathNodes[index] = this;
    }

    public void SetInOpenList(NativeArray<PathNode> pathNodes) {
        flags |= PathNodeFlags.IsInOpenList;
        pathNodes[index] = this;
    }

    public void RemoveFromOpenList(NativeArray<PathNode> pathNodes) {
        flags &= ~PathNodeFlags.IsInOpenList;
        pathNodes[index] = this;
    }

    public int cameFromNodeIndex;
}

[Flags]
public enum PathNodeFlags : byte {
    Empty = 0,
    IsWall = 1,
    IsInClosedList = 2,
    IsInOpenList = 4,
}