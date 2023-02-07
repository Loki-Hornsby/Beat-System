/// <summary>
/// Made by Loki Alexander Button Hornsby
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Loki.Signal.Analysis.Demo {
    [RequireComponent(typeof(Camera))]
    public class VFX : MonoBehaviour {
        // Camera reference
        Camera camera;

        // Target FOV
        float targetFOV;

        /// <summary>
        /// Generic setup
        /// </summary>
        void Awake(){
            camera = GetComponent<Camera>();
            camera.orthographic = false;
        }

        /// <summary>
        /// Update map
        /// </summary>
        public void IndexChange(Analyser.Data data, int index){
            // If our current index is an onset
            if (data.onsets[index]){
                // "Pulse" the camera
                targetFOV = 100f;
            } else {
                // Assign a new FOV target using our currently indexed magnitude as a percentage
                targetFOV = 60f + (data.pitches[index] / data.pitches.Max());
            }
        }

        void Update(){
            // Lerp our camera's field of view to our target FOV
            //camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, targetFOV, Time.deltaTime);
        }
    }
}
