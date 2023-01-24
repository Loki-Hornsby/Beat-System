/// <summary>
/// Copyright 2022, Loki Alexander Button Hornsby (Loki Hornsby), All rights reserved.
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Threading.Tasks;

public static class ThreadHandling {
    public delegate void Threading();
    public static event Threading Finished;

    static Queue<Action> tasks = new Queue<Action>();

    public static void QueueTask(Action action){
        tasks.Enqueue(action);
    }

    public static void ExecuteTasks(){
        RunNextTask();
    }

    // Runs the next task queued in [tasks]
    async static void RunNextTask(int timeout = 10){
        // Timeout in ms
        timeout = timeout * 1000;

        // Task creation
        var task = Task.Run(tasks.Dequeue());
        await task.ContinueWith(t => Debug.Log("TASK DONE"));

        // Timeout
        if (await Task.WhenAny(task, Task.Delay(timeout)) == task) {
            // Task completed without timing out
            if (tasks.Count > 0){
                RunNextTask();
            } else {
                // Invoke our finished condition
                Finished?.Invoke(); 
            }
        } else { 
            // Task timed out
            Debug.LogError("A timeout occurred in Analysis.cs");
        }
    }
}
