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
    public enum Channels {
        Frequency,
        Volume,
        Pitch
    }
    
    [Header("Configuration")]
    public Channels channel;
    public int DivScale;
    public GameObject prefab;

    /// <summary>
    /// Generate our items using our settings
    /// </summary>
    public void Generate(Analyser.Data[] data){
        // 
        foreach (var data_piece in data){
            for (int i = 0; i < data_piece.O_Notes.Length; i++){
                Analyser.Notes.Note note = data_piece.O_Notes[i];

                float val = 0f;

                switch (channel){
                    case Channels.Frequency:
                        val = note.frequency;
                        break;
                    case Channels.Volume:
                        val = note.volume;
                        break;
                    case Channels.Pitch:
                        val = note.pitch;
                        break;
                }

                Instantiate(prefab, new Vector3(i, val, 0f), Quaternion.identity);
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
