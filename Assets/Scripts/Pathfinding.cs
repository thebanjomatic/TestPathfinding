using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System.Linq;
using System;

public class Pathfinding : MonoBehaviour {
    private const int SIZE_DIM = 100;
    private const int JOB_COUNT = 20;

    public TMPro.TMP_Text text1;
    public TMPro.TMP_Text text2;

    private Queue<float> times1 = new Queue<float>();
    private Queue<float> times2 = new Queue<float>();
    private bool spunUp = false;

    void Update() {
        times1.Enqueue(Test<ArrayOpenList>());
        times2.Enqueue(Test<MinHeapOpenList>());

        if (times1.Count > 0 && (times1.Count % 200) == 0) {
            if (!spunUp) {
                times1.Clear();
                times2.Clear();
                spunUp = true;
            } else if (text1 && text2 && spunUp) {
                text1.text = "ArrayOpenList Time: " + times1.Average();
                text2.text = "MinHeapOpenList Time: " + times2.Average();
            }
        }
    }

    private float Test<T>() where T: IOpenList, new() {
        float startTime = Time.realtimeSinceStartup;
        var jobHandleArray = new NativeArray<JobHandle>(JOB_COUNT, Allocator.TempJob);
        for (int i = 0; i < JOB_COUNT; i++) {
            var job = new FindPathJob<T> {
                startPosition = new int2(0, 0),
                endPosition = new int2(SIZE_DIM - 1, 0),
                gridSize = new int2(SIZE_DIM, SIZE_DIM)
            };
            jobHandleArray[i] = job.Schedule();
        }
        JobHandle.CompleteAll(jobHandleArray);
        jobHandleArray.Dispose();

        float timing = (Time.realtimeSinceStartup - startTime) * 1000f;
        return timing;
    }
}