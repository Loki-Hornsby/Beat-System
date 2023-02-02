/// <summary>
/// Copyright 2022, Loki Alexander Button Hornsby (Loki Hornsby), All rights reserved.
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Loki.Signal.Analysis;

namespace Loki.Signal.Analysis.Demo {
    public class Map : MonoBehaviour {
        [Header("Tracker")]
        public Loki.Signal.Analysis.Utilities.SongTracker tracker;

        [Header("Obstacles")]
        public float mult;
        public GameObject jump;

        // Stored maps
        List<GameObject> maps = new List<GameObject>();

        // Map length
        [System.NonSerialized] public float length;

        /// <summary>
        /// Create jumps on the floor
        /// </summary>
        void CreateJumps(ref GameObject map, float length, bool[] onsets){
            // Determine our position (length of song divided by amount of pitches)
            float div = length / onsets.Length;

            // Loop through each pitch
            for (int i = 0; i < onsets.Length; i++){
                // Assign empty game object
                GameObject obj = null;

                // If the current item is an onset
                if (onsets[i]) {
                    // Create our object
                    obj = Instantiate(jump, new Vector3(div * i, 0f, 0f), Quaternion.identity);

                    // Parent it to our map
                    obj.transform.parent = map.transform;
                }
            }
        }

        /// <summary>
        /// Generate the parts of our map
        /// </summary>
        public void Generate(Analyser.Data data){
            // Create our map game object and disable it
            GameObject map = new GameObject(data.Clip.name);
            //map.SetActive(false);

            // Add it to our list
            maps.Add(map);
            
            // Calculate length of song using multiplier
            length = data.Length * mult;

            // Set our trackers target to be the end of the map
            tracker.EndPos = new Vector3(length, 0f, 0f);

            // Create our "jumps" (Hurdles)
            CreateJumps(
                ref map,
                length, 
                data.onsets
            );
        }
    }
}