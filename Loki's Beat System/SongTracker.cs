using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Map;
using Song;

// Todo: #9 disable until game has started

public class SongTracker : MonoBehaviour{
    public static SongTracker Instance { get; private set; }

    // Movement Time Handling
    Unitilities.Counter delta = new Unitilities.Counter();

    // Single grid item size
    Vector3 SingleGridItemSize;

    void Awake () {
        if (Instance == null) {
            Instance = this;
        } else {
            Debug.LogError(this.GetType().Name + " " + "Cannot have multiple instances");
            Destroy (gameObject);  
        }
    }

    void Update() {
        delta.Update(Time.deltaTime);

        if (Audio.Instance.GetState(0) == Audio.States.Playing){
            this.transform.position = new Vector3(
                Mathf.Lerp(0f, Data.SongLength * Generation.Instance.MapDetail, (Audio.Instance.GetAudioSourceTime(0) / Data.SongLength)),
                0f,
                0f
            );
        }
    }
}