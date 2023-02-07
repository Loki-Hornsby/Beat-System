/// <summary>
/// Made by Loki Alexander Button Hornsby
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Loki.Signal.Analysis;

namespace Loki.Signal.Analysis.Demo {
    public class Maps : MonoBehaviour {
        public class Map {
            public GameObject Obj; // Map object
            public float Length; // Length of the map

            /// <summary>
            /// Constructor
            /// </summary>
            public Map(GameObject _Obj, float _Length){
                // Define and disable our object
                Obj = _Obj;
                Obj.SetActive(false);

                // Define our map length
                Length = _Length;
            }
        }

        [Header("Tracker")]
        public Loki.Signal.Analysis.Utilities.SongTracker tracker;

        [Header("Obstacles")]
        public float mult;
        public GameObject jump;

        // Stored maps
        List<Maps.Map> maps = new List<Maps.Map>();

        // Selected map
        int selected;

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
                    obj = Instantiate(jump, new Vector3(div * i, jump.transform.position.y, jump.transform.position.z), Quaternion.identity);

                    // Parent it to our map
                    obj.transform.parent = map.transform;
                }
            }
        }

        /// <summary>
        /// Generate the parts of our map
        /// </summary>
        public void Generate(Analyser.Data data){
            // Define our map parent
            Maps.Map map = new Maps.Map(
                new GameObject(data.Clip.name), 
                data.Length * mult
            );
            
            // Add it to our list
            maps.Add(map);

            // Create our "jumps" (Hurdles)
            CreateJumps(
                ref map.Obj,
                map.Length, 
                data.onsets
            );
        }

        /// <summary>
        /// Goto the next map
        /// </summary>
        public void Next(){
            // Disable our current map
            maps[selected].Obj.SetActive(false);

            // Increment counter
            selected++;

            // Reset counter if needed
            if (selected > maps.Count - 1){
                selected = 0;
            }

            // Select and enable our new map
            maps[selected].Obj.SetActive(true);

            // Set our trackers end position
            tracker.EndPos = new Vector3(maps[selected].Length, 0f, 0f);
        }
    }
}