/// <summary>
/// Copyright 2022, Loki Alexander Button Hornsby (Loki Hornsby), All rights reserved.
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Song;

// Probes current audio for data

[RequireComponent(typeof(Audio))]
public class Probe : MonoBehaviour {
    public static Probe Instance { get; private set; }

    void Awake(){
        if (Instance == null) {
            Instance = this;
        } else {
            Debug.LogError(this.GetType().Name + " " + "Cannot have multiple instances");
            Destroy (gameObject);  
        }
    }

    /// <summary>
    /// Gather data from audio
    /// </summary>
    public void probe(AudioClip song){
        Data.GenerateData(song);
    }
}
