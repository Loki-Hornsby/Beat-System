/// <summary>
/// Made by Loki Alexander Button Hornsby
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//// http://www.zytrax.com/tech/audio/

namespace Loki.Signal.Analysis {
    [RequireComponent(typeof(AudioSource))]
    public class Audio : MonoBehaviour {
        // State of our audio
        public enum States {
            Analysing,
            Playing,
            Stopped
        }

        Audio.States state;

        // Our audio source
        [System.NonSerialized] public AudioSource source;

        // List of data
        Queue<Analyser.Data> data;

        // Current data
        [System.NonSerialized] public Analyser.Data current;

        /// <summary>
        /// Initialization
        /// </summary>
        void Start(){
            data = new Queue<Analyser.Data>();
            source = GetComponent<AudioSource>();
        }

        /// <summary>
        /// Pass data to our
        /// </summary>
        public void PassData(Analyser.Data _data){
            // Add our data to the queue
            data.Enqueue(_data);
        }

        /// <summary>
        /// Play a song
        /// </summary>
        public void PlaySong(){
            if (data.Count > 0){
                // Our current data
                current = data.Dequeue();

                // Assign our clip
                source.clip = current.Clip;

                // Play the clip
                source.Play();
            } else {
                Debug.LogError("No Songs Queued!");
            }
        }

        /// <summary>
        /// Get our current data
        /// </summary>
        public bool hasSongs(){
            return (data.Count > 0);
        }
    }
}