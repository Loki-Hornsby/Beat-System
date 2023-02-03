/// <summary>
/// Copyright 2022, Loki Alexander Button Hornsby (Loki Hornsby), All rights reserved.
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Loki.Signal.Analysis.Utilities {
    public class SongTracker : MonoBehaviour{
        // Is our tracker allowed to move
        bool active;

        // End position
        [System.NonSerialized] public Vector3 EndPos;

        // Timer
        [System.NonSerialized] public float t;

        // Current index
        [System.NonSerialized] public int index;

        // Unity event
        public UnityEvent<Analyser.Data, int> use;

        // Our data
        Analyser.Data data;

        /// <summary>
        /// initialize our tracker
        /// </summary>
        public void Awake(){
            // Initialize variables
            active = false;

            // Initialize unity event
            if (use == null)
                use = new UnityEvent<Analyser.Data, int>();
        }

        /// <summary>
        /// Activate our tracker
        /// </summary>
        public void Begin(Analyser.Data _data){
            // Activate our song tracker
            active = true;

            // Apply our data
            data = _data;

            // Reset timer
            t = 0f;

            // Reset index
            index = 0;
        }   

        /// <summary>
        /// Move our tracker in time to the song
        /// </summary>
        void Update() {
            if (active){
                // Add our time divided by the length of our song
                t += Time.deltaTime;

                // Lerp our song tracker to our end pos in time to the length of our songs
                this.transform.position = Vector3.Lerp(Vector3.zero, EndPos, t / data.Length);

                // if our timer is more than our current time (and if we are in range of our array)
                if (index + 1 < data.times.Length && t > data.times[index]) {
                    // Increase our index 
                    index++;

                    // Trigger our unity event
                    use.Invoke(data, index);
                }
            }
        }
    }
}

