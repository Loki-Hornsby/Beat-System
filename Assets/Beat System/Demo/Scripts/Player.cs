/// <summary>
/// Made by Loki Alexander Button Hornsby
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
        public const float jump = 10f;

        // Grounded
        public LayerMask select;
        public const float offset = 1f;
        [System.NonSerialized] public bool grounded;

        // Tracker
        public Transform tracker;

        // Components
        Collider col;
        Rigidbody rb;

        /// <summary>
        /// Generic Setup
        /// </summary>
        void Start(){
            // Define Components
            col = GetComponent<Collider>();
            rb = GetComponent<Rigidbody>();

            // Apply drag so our player slows down
            rb.drag = 5f;
        }

        /// <summary>
        /// Update our player
        /// </summary>
        void Update(){
            // Grounded check
            grounded = Physics.Linecast(transform.position, transform.position - new Vector3(0f, offset, 0f), select);

            // If the player presses space
            if (Input.GetKeyDown(KeyCode.Space) && grounded){
                // Add force upwards - 
                // we multiply against half of gravity and also mass then divide this operation by our constant float of jump
                rb.AddForce(new Vector3(0f, jump, 0f), ForceMode.Impulse);
            }

            // If our players velocity is negative
            if (rb.velocity.y < 0f) {
                // If our player isn't grounded
                if (!grounded){
                    // Apply a downwards boost
                    rb.AddForce(new Vector3(0f, -Mathf.Pow(jump, 2), 0f), ForceMode.Force);
                } else {
                    // Reset velocity
                    rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                }
            }
        }
    }
}