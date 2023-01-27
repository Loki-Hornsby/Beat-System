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
    // Threading delegate and event
    public delegate void Threading();
    public static event Threading Finished;

    // Queued tasks
    static Queue<Action> queuedTasks = new Queue<Action>();

    // Numbers
    static int completed;
    static int tasks;

    /// <summary>
    /// Queue a task (Action)
    /// </summary>
    public static void QueueTask(Action action){
        if (completed == tasks) tasks = 0;
        tasks++;
        queuedTasks.Enqueue(action);
    }

    /// <summary>
    /// Execute all queued tasks
    /// </summary>
    public static void ExecuteTasks(){
        completed = 0;
        LoopTaskExec();
    }

    /// <summary>
    /// Run next queued task
    /// </summary>
    public async static void LoopTaskExec(){
        // Get the first action in our tasks list by use of the Dequeue function
        Action action = queuedTasks.Dequeue();

        // Run the task and await its completion
        await Task.Run(action).ContinueWith(
            t => {
                // Increment completed tasks counter
                completed++;

                // Log a message
                Debug.Log("Task completed! (" + completed + " / " + tasks + ")");
                
                // Run next task
                if (completed != tasks){
                    LoopTaskExec();

                // Invoke finish
                } else {
                    Finished?.Invoke();
                }
            }
        );
    }
}

