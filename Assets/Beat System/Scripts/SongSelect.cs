/// <summary>
/// Copyright 2022, Loki Alexander Button Hornsby (Loki Hornsby), All rights reserved.
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongSelect : MonoBehaviour {
    public AudioClip clip;
    public Loki.Signal.Analysis.Analyser analyser;

    void Awake(){
        DontDestroyOnLoad(this.gameObject);
        analyser.Analyse(clip);
    }
}
