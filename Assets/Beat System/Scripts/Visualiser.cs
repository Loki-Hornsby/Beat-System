/// <summary>
/// Copyright 2022, Loki Alexander Button Hornsby (Loki Hornsby), All rights reserved.
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Loki.Signal.Analysis.Utilities {
    public class Visualiser : MonoBehaviour {
        public enum Modes {
            Frequency,
            Volume,
            Pitch
        }

        [Header("Configuration")]
        public Modes mode;
        public GameObject prefab;

        /// <summary>
        /// Generate our items using our settings
        /// </summary>
        public void Generate(Analyser.Data data){
            // Initialize our array
            float[] arr = new float[data.O_Notes.Length];

            // Select our array
            switch (mode){
                case Modes.Frequency:
                    arr = data.frequencies;
                    break;
                case Modes.Volume:
                    arr = data.volumes;
                    break;
                case Modes.Pitch:
                    arr = data.pitches;
                    break;
            }

            // Loop through every item from our filters result
            for (int i = 0; i < data.frequencies.Length; i++){
                // Create an object {x: index, y: value}
                GameObject Obj = Instantiate(prefab, new Vector3(i, arr[i], 0f), Quaternion.identity);

                // Parent the object to this object
                Obj.transform.parent = this.transform;
            }
        }
    }
}