/// <summary>
/// Made by Loki Alexander Button Hornsby
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Loki.Signal.Analysis.Utilities {
    public class SongTracker : MonoBehaviour{
        // Is our tracker allowed to move?
        private bool _active;
        public bool active {
            get {
                // Return our variable
                return _active;
            }

            set {
                // Reset timer
                t = 0f;

                // Reset index
                index = 0;

                // Assign variable
                _active = value; 
            }
        }

        // End position
        [System.NonSerialized] public Vector3 EndPos;

        // Timer
        [System.NonSerialized] public float t;

        // Current index
        [System.NonSerialized] public int index;

        // Audio source to monitor
        public Audio audio;

        // Unity event
        public UnityEvent<Analyser.Data, int> use;

        /// <summary>
        /// initialize our tracker
        /// </summary>
        void Awake(){
            // Initialize variables
            active = false;

            // Initialize unity event
            if (use == null)
                use = new UnityEvent<Analyser.Data, int>();
        }

        /// <summary>
        /// Move our tracker in time to the song
        /// </summary>
        void Update() {
            // If our tracker is active and our data is valid
            if (active && audio.current != null){
                // Add our time
                t += Time.deltaTime;

                // Lerp our song tracker to our end pos in time to the length of our songs
                this.transform.position = Vector3.Lerp(Vector3.zero, EndPos, t / audio.current.Length);

                // if our timer is more than our current time (and if we are in range of our array)
                if (index + 1 < audio.current.times.Length && t > audio.current.times[index]) {
                    // Increase our index 
                    index++;

                    // Trigger our unity event
                    use.Invoke(audio.current, index);
                }
            }
        }
    }
}

