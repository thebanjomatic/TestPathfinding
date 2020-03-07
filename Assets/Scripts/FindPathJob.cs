using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
struct FindPathJob<OPENLIST> : IJob where OPENLIST : IOpenList, new() {
    public int2 startPosition;
    public int2 endPosition;
    public int2 gridSize;

    public void Execute() {
        var rand = new Unity.Mathematics.Random(42);

        NativeArray<int2> neighborOffsets = new NativeArray<int2>(9, Allocator.Temp);
        neighborOffsets[0] = new int2 { x = -1, y = -1 };
        neighborOffsets[1] = new int2 { x = +0, y = -1 };
        neighborOffsets[2] = new int2 { x = +1, y = -1 };
        neighborOffsets[3] = new int2 { x = -1, y = +0 };
        neighborOffsets[4] = new int2 { x = +0, y = +0 };
        neighborOffsets[5] = new int2 { x = +1, y = +0 };
        neighborOffsets[6] = new int2 { x = -1, y = +1 };
        neighborOffsets[7] = new int2 { x = +0, y = +1 };
        neighborOffsets[8] = new int2 { x = +1, y = +1 };

        var pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        for (int y = 0; y < gridSize.y; y++) {
            for (int x = 0; x < gridSize.x; x++) {
                var pathNode = new PathNode() {
                    position = new int2(x, y),
                    index = Index(x, y),

                    gCost = int.MaxValue,
                    hCost = CalculateDistanceCost(new int2(x, y), endPosition),

                    flags = PathNodeFlags.Empty, //rand.NextFloat() > 0.75 ? PathNodeFlags.IsWall : PathNodeFlags.Empty,
                    cameFromNodeIndex = -1,
                };
                pathNode.CalculateFCost();
                pathNodeArray[pathNode.index] = pathNode;
            }
        }

        for (int y = 0; y < gridSize.y - 1; y++) {
            pathNodeArray[Index(gridSize.x / 2, y)].SetIsWall(pathNodeArray);
        }

        var endNodeIndex = Index(endPosition.x, endPosition.y);
        var startNode = pathNodeArray[Index(startPosition.x, startPosition.y)];
        startNode.gCost = 0;
        startNode.CalculateFCost();
        pathNodeArray[startNode.index] = startNode;

        // var openList = new MinHeapOpenList(pathNodeArray.Length);
        var openList = new OPENLIST();
        openList.Initialize(pathNodeArray.Length);

        openList.Enqueue(pathNodeArray, startNode.index);
        pathNodeArray[startNode.index] = startNode;

        while (openList.Length > 0) {
            int currentNodeIndex = openList.DequeueMin(pathNodeArray);

            if (currentNodeIndex == endNodeIndex) {
                break;
            } else {
                var currentNode = pathNodeArray[currentNodeIndex];
                currentNode.SetInClosedList(pathNodeArray);

                for (int i = 0; i < neighborOffsets.Length; i++) {
                    int2 neighborOffset = neighborOffsets[i];
                    int2 neighborPosition = currentNode.position + neighborOffset;

                    if (!IsPositionInsideGrid(neighborPosition)) {
                        continue;
                    }

                    int neighbourNodeIndex = Index(neighborPosition.x, neighborPosition.y);
                    var neighbourNode = pathNodeArray[neighbourNodeIndex];

                    if (neighbourNode.IsBlockedOrClosed) {
                        continue;
                    }

                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode.position, neighborPosition);
                    if (tentativeGCost < neighbourNode.gCost) {
                        neighbourNode.cameFromNodeIndex = currentNodeIndex;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.CalculateFCost();
                        pathNodeArray[neighbourNodeIndex] = neighbourNode;

                        openList.Enqueue(pathNodeArray, neighbourNode.index);
                    }
                }
            }
        }

        var endNode = pathNodeArray[endNodeIndex];
        var path = BuildPath(pathNodeArray, endNode);
        //foreach (int2 pathPosition in path) {
        //    Debug.Log(pathPosition);
        //}
        path.Dispose();

        openList.Dispose();
        pathNodeArray.Dispose();
        neighborOffsets.Dispose();
    }

    private static NativeList<int2> BuildPath(NativeArray<PathNode> pathNodeArray, PathNode endNode) {
        if (endNode.cameFromNodeIndex == -1) {
            return new NativeList<int2>(Allocator.Temp);
        } else {
            NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
            path.Add(endNode.position);

            var currentNode = endNode;
            while (currentNode.cameFromNodeIndex != -1) {
                var cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                path.Add(cameFromNode.position);

                currentNode = cameFromNode;
            }
            return path;
        }
    }

    private bool IsPositionInsideGrid(int2 gridPosition) {
        return
            gridPosition.x >= 0 && gridPosition.y >= 0 &&
            gridPosition.x < gridSize.x && gridPosition.y < gridSize.y;
    }


    private int Index(int x, int y) {
        return x + y * gridSize.x;
    }

    private static int CalculateDistanceCost(int2 aPosition, int2 bPosition) {
        int xDistance = math.abs(aPosition.x - bPosition.x);
        int yDistance = math.abs(aPosition.y - bPosition.y);
        int remaining = math.abs(xDistance - yDistance);

        const int MOVE_STRAIGHT_COST = 10;
        const int MOVE_DIAGONAL_COST = 14;

        return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }
}


interface IOpenList : IDisposable {
    void Initialize(int size);
    void Enqueue(NativeArray<PathNode> pathNodes, int index);

    int DequeueMin(NativeArray<PathNode> pathNodes);

    int Length { get; }
}