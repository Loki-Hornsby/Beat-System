/// <summary>
/// Made by Loki Alexander Button Hornsby
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

            // Our input data
            [Serializable] public class InputData {
                // Audio clips to analyse
                public AudioClip clip;

                // Filters to apply
                public Analyser.Filter FrequencyFilter;
                public Analyser.Filter VolumeFilter;
                public Analyser.Filter PitchFilter;
            }

            public InputData[] input;

            // Events we want to trigger after analysis has finished
            public UnityEvent<Analyser.Data> use;

            /// <summary>
            /// Constructor
            /// </summary>
            public async void PerformAnalysis(){
                // Initialize unity event
                if (use == null)
                    use = new UnityEvent<Analyser.Data>();

                // Loop through each clip
                for (int i = 0; i < input.Length; i++){
                    // Assign a task to analyse each clip with the selected analyser
                    Analyser.Data data = await analyser.Analyse(
                        input[i].clip, 
                        input[i].FrequencyFilter,
                        input[i].VolumeFilter,
                        input[i].PitchFilter
                    );

                    // Invoke our method after analysis is complete
                    use.Invoke(data);
                }
            }
        }

        // Items to analyse
        public AnalysisItem[] items;

        /// <summary>
        /// Setup
        /// </summary>
        void Awake(){
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