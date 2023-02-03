/// <summary>
/// Copyright 2022, Loki Alexander Button Hornsby (Loki Hornsby), All rights reserved.
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Loki.Signal.Analysis.Utilities;

namespace Loki.Signal.Analysis.Demo {
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

        /// <summary>
        /// Generic setup
        /// </summary>
        void Start(){
            // Set our start to "playing" to begin with
            state = GameStates.playing;
        }
        
        /// <summary>
        /// Update game state
        /// </summary>
        void Update(){
            switch (state){
                case GameStates.playing:
                    // if our player isn't grounded and their height is less than 0
                    if (!player.grounded && player.transform.position.y < 0f){
                        // Set game state to lost
                        state = GameStates.lost;
                    
                    // If our tracker is near the end
                    } else if (tracker.t >= 0.99f) {
                        // Set game state to won
                        //state = GameStates.won;
                    }

                    break;
                case GameStates.lost:
                    // Destroy the player
                    Destroy(player.gameObject);

                    // Destroy the game object this script is attached to
                    Destroy(this.gameObject);

                    Debug.Log("BOOOOOOOOO!");

                    break;
                case GameStates.won:
                    //Debug.Log("WOOHOO!");

                    break;
            }
        }
    }
}
