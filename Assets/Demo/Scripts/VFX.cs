/// <summary>
/// Copyright 2022, Loki Alexander Button Hornsby (Loki Hornsby), All rights reserved.
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Loki.Signal.Analysis.Demo {
    public class VFX : MonoBehaviour {
        // Camera reference
        public Camera camera;

        // Target FOV
        float targetFOV;

        /// <summary>
        /// Generic setup
        /// </summary>
        void Awake(){
            camera.orthographic = false;
        }

        /// <summary>
        /// Update map
        /// </summary>
        public void IndexChange(Analyser.Data data, int index){
            // If our current index is an onset
            if (data.onsets[index]){
                // "Pulse" the camera
                targetFOV = 200f;
            } else {
                // Assign a new FOV target using our currently indexed magnitude as a percentage
                targetFOV = (data.pitches[index] / data.pitches.Max()) * 60f;
            }
        }

        void Update(){
            // Lerp our camera's field of view to our target FOV
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, targetFOV, Time.deltaTime);
        }
    }
}
