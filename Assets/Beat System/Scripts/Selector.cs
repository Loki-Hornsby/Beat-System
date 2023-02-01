/// <summary>
/// Copyright 2022, Loki Alexander Button Hornsby (Loki Hornsby), All rights reserved.
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using System.Threading.Tasks;

namespace Loki.Signal.Analysis {
    public class Selector : MonoBehaviour {
        public static Selector Instance;

        [Serializable]
        public class AnalysisItem {
            // Our selected analyser
            public Analyser analyser;
            // Audio clips to analyse
            [Space(10)]
            public AudioClip[] clips;
            // Events we want to trigger after analysis has finished
            [Space(10)]
            public UnityEvent<Analyser.Data[]> use;
            // Returned data from analyser
            [System.NonSerialized] public Analyser.Data[] data;

            /// <summary>
            /// Constructor
            /// </summary>
            public async void PerformAnalysis(){
                // Initialize unity event
                if (use == null)
                    use = new UnityEvent<Analyser.Data[]>();

                // Initialize our data array
                data = new Analyser.Data[clips.Length];

                // Loop through each clip
                for (int i = 0; i < clips.Length; i++){
                    // Assign a task to analyse each clip with the selected analyser
                    data[i] = await analyser.Analyse(clips[i]);
                }

                // Invoke our method after analysis is complete
                use.Invoke(data);
            }
        }

        // Items to analyse
        public AnalysisItem[] items;

        /// <summary>
        /// Setup
        /// </summary>
        void Awake(){
            Debug.LogWarning("Music by QubeSounds from Pixabay");

            // Only allow one instance of this object
            if (Instance == null){
                Instance = this;
            } else {
                Debug.LogError("Only one selector can be available at a time!");
                Destroy(this);
            }

            // Trigger each AnalysisItems to perform analysis
            foreach (var item in items){
                item.PerformAnalysis();
            }
        }
    }
}