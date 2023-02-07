/// <summary>
/// Made by Loki Alexander Button Hornsby
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loki.Signal.Analysis.Demo {
    public class Narrator : MonoBehaviour {
        // Reference
        public Loki.Signal.Analysis.Audio audio;
        public Loki.Signal.Analysis.Utilities.SongTracker tracker;

        // Clips
        public AudioClip clip_countdown;

        /// <summary>
        /// Make the narrator perform a countdown
        /// </summary>
        IEnumerator Countdown(){
            // Disable our tracker
            tracker.active = false;

            // Play our audio source
            audio.source.PlayOneShot(clip_countdown, 1f);

            // Wait a few seconds
            yield return new WaitForSeconds(clip_countdown.length);

            // Play a song
            audio.PlaySong();

            // Enable our tracker
            tracker.active = true;
        }

        /// <summary>
        /// Trigger our countdown
        /// </summary>
        public bool Trigger(){
            // if our audio script still contains queued songs
            if (audio.hasSongs()){
                StartCoroutine(Countdown());

                return true;
            } else {
                return false;
            }
        }
    }
}