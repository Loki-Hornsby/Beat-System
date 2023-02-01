/// <summary>
/// Copyright 2022, Loki Alexander Button Hornsby (Loki Hornsby), All rights reserved.
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

using Loki.Signal.Analysis;

public class Generator : MonoBehaviour {
    public enum Modes {
        Frequency,
        Volume,
        Pitch
    }

    [Header("Configuration")]
    public Modes mode;
    public int DivScale;
    public GameObject prefab;

    /// <summary>
    /// Generate our items using our settings
    /// </summary>
    public void Generate(Analyser.Data[] data){
        // Loop through each data piece
        foreach (var data_piece in data){
            // Initialize our array
            float[] arr = new float[data_piece.O_Notes.Length];

            // Select our array
            switch (mode){
                case Modes.Frequency:
                    arr = data_piece.frequencies;
                    break;
                case Modes.Volume:
                    arr = data_piece.volumes;
                    break;
                case Modes.Pitch:
                    arr = data_piece.pitches;
                    break;
            }

            // Loop through every item from our filters result
            for (int i = 0; i < data_piece.frequencies.Length; i++){
                // Create an object {x: index, y: value}
                GameObject Obj = Instantiate(prefab, new Vector3(i, arr[i], 0f), Quaternion.identity);

                // Parent the object to this object
                Obj.transform.parent = this.transform;
            }
        }
        
        /*// Divisible scale divided by the largest data point in our array
        float divisor = (DivScale / arr.Max());
        
        // Loop through our selected array
        for (int i = 0; i < arr.Length; i++){
            // Calculate our percentage (Multiplying by DivScale once more is optional)
            float percentage = ((divisor * arr[i]) * DivScale);
            
            // Create an object with it's x defined by i and its height defined by our percentage
            GameObject obj = Instantiate(prefab, new Vector3(i, transform.position.y + percentage, 0f), Quaternion.identity);
        }*/
    }
}
