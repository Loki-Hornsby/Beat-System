/// <summary>
/// Copyright 2022, Loki Alexander Button Hornsby (Loki Hornsby), All rights reserved.
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Visualiser : MonoBehaviour {
    public Loki.Signal.Analysis.Analyser analyser;
    
    /// <summary>
    /// Display an array
    /// </summary>
    /*Action<dynamic> Display(float[] arr){
        return (dynamic arr) => {

        /*for (int i = 0; i < arr.Length; i++){
            Debug.DrawLine(
                new Vector3(i, arr[i], 0f), 
                new Vector3(i, arr[i-1], 0f),
                Color.blue,
                10000f
            );

            System.Console.WriteLine(arr[i]);

            new GameObject(i.ToString());
        }*

        };
    }*/

    /// <summary>
    /// Visualise our data
    /// </summary>
    void Visualise(){
        Debug.Log("VIS");

        float[] arr = analyser.data.O_Frequency;
        Debug.Log(arr.Length);

        for (int i = 0; i < arr.Length; i++){
            Debug.DrawLine(
                new Vector3(i, arr[i], 0f), 
                new Vector3(i, arr[i-1], 0f),
                Color.blue,
                10000f
            );
        }

        // Queue our visualisation tasks
        //ThreadHandling.QueueTask(Display(analyser.data.O_Frequency));
        //ThreadHandling.QueueTask(Display(analyser.data.O_Volume));

        // Execute all currently queued tasks
        //ThreadHandling.ExecuteTasks();
    }

    /// <summary>
    /// Setup
    /// </summary>
    void Start(){
        // Wait for analysis to finish
        analyser.Finished += Visualise;
    }

    void Update(){
        if (analyser.data.O_Frequency != null) Debug.Log(analyser.data.O_Frequency.Length);
    }
}
