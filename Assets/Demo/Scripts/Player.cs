/// <summary>
/// Copyright 2022, Loki Alexander Button Hornsby (Loki Hornsby), All rights reserved.
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loki.Signal.Analysis.Demo {
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Player : MonoBehaviour {
        // Jump Height
        public const float jump = 5f;

        // Grounded
        public LayerMask select;
        public const float offset = 1f;
        [System.NonSerialized] public bool grounded;

        // Components
        Collider col;
        Rigidbody rb;

        /// <summary>
        /// Generic Setup
        /// </summary>
        void Start(){
            col = GetComponent<Collider>();
            rb = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Update our player
        /// </summary>
        void Update(){
            // Grounded check
            grounded = Physics.Linecast(transform.position, transform.position - new Vector3(0f, offset, 0f), select);

            // if the players x velocity is less than 0
            if (rb.velocity.x >= 0f){
                // If the player presses space
                if (Input.GetKeyDown(KeyCode.Space) && grounded){
                    // Add force upwards - 
                    // we multiply against half of gravity and also mass then divide this operation by our constant float of jump
                    rb.AddForce(new Vector3(0f, jump, 0f), ForceMode.Impulse);
                }
            } else {
                // Un parent this game object
                if (this.transform.parent != null) this.transform.parent = null;
            }
        }
    }
}