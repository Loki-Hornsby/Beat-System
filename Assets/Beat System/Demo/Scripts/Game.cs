/// <summary>
/// Made by Loki Alexander Button Hornsby
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Loki.Signal.Analysis.Utilities;

namespace Loki.Signal.Analysis.Demo {
    [RequireComponent(typeof(Credits))]
    [RequireComponent(typeof(Narrator))]
    public class Game : MonoBehaviour {
        public enum GameStates {
            playing,
            lost,
            won
        }

        // Current game state
        [SerializeField] private GameStates state;

        // Refs
        public Player player;
        public SongTracker tracker;
        public Audio audio;
        public Narrator narrator;
        public Maps map;

        /// <summary>
        /// Generic setup
        /// </summary>
        void Start(){
            // Set our start to "won" to begin with
            state = GameStates.won;
        }
        
        /// <summary>
        /// Update game state
        /// </summary>
        void Update(){
            // Update things dependent on our current state
            switch (state){
                case GameStates.playing:
                    // if our player isn't grounded and their height is less than 0
                    if (!player.grounded && player.transform.position.y < 0f){
                        // Set game state to lost
                        state = GameStates.lost;
                    
                    // If our tracker is near the end
                    } else if (tracker.t >= 0.99f) {
                        // Set game state to won
                        state = GameStates.won;
                    }

                    break;
                case GameStates.lost:
                    // Destroy the player
                    Destroy(player.gameObject);

                    // Destroy the game object this script is attached to
                    Destroy(this.gameObject);

                    //Debug.Log("BOOOOOOOOO!");

                    break;
                case GameStates.won:
                    // If our audio source isn't playing
                    if (!audio.source.isPlaying) {
                        // If our narrator was able to trigger
                        if (narrator.Trigger()){
                            // Goto our next map
                            map.Next();

                            // Revert back to the playing state
                            state = GameStates.playing;
                        }
                    }

                    break;
            }
        }
    }
}
