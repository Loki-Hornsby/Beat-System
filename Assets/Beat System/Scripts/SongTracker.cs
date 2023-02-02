/// <summary>
/// Copyright 2022, Loki Alexander Button Hornsby (Loki Hornsby), All rights reserved.
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loki.Signal.Analysis.Utilities {
    public class SongTracker : MonoBehaviour{
        // End position
        public Vector3 EndPos;

        // Is our tracker allowed to move
        bool active;

        // Timer
        [System.NonSerialized] public float t;

        // Our data
        Analyser.Data data;

        /// <summary>
        /// initializse our tracker
        /// </summary>
        public void Awake(){
            active = false;
            t = 0f;
        }

        /// <summary>
        /// Activate our tracker
        /// </summary>
        public void Begin(Analyser.Data _data){
            // Activate our song tracker
            active = true;

            // Apply our data
            data = _data;
        }   

        /// <summary>
        /// Move our tracker in time to the song
        /// </summary>
        void Update() {
            if (active){
                // Add our time divided by the length of our song
                t += Time.deltaTime / data.Length;

                // Lerp our song tracker to our end pos in time to the length of our songs
                this.transform.position = Vector3.Lerp(Vector3.zero, EndPos, t);
            }
        }
    }
}

