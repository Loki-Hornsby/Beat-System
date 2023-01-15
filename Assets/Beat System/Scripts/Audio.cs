/// <summary>
/// Copyright 2022, Loki Alexander Button Hornsby (Loki Hornsby), All rights reserved.
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Song;

//// http://www.zytrax.com/tech/audio/

[RequireComponent(typeof(AudioListener))]

public class Audio : MonoBehaviour {
    // Enums
    public enum States {
        Stopped             = 1,
        Playing             = 2,
        PlayingSoundEffect  = 3,
        Rewinding           = 4,
        Paused              = 5
    }

    // Audio Source
    public class Source {
        public int ind;
        public AudioSource source;
        public Audio.States state;

        public Queue<AudioClip> songs;

        public float LastPitch = 1f;
        public float LastFrequency = 1f;

        public void DequeueItems(){
            songs.Dequeue();
        }

        public void EnqueueItems(AudioClip s){
            songs.Enqueue(s);
        }
        
        public void Initialize(int SourceIndex, AudioSource audioSource, Audio.States selectedState){
            // Variable Setup
            ind = SourceIndex;
            source = audioSource;
            state = selectedState;

            // Songs
            songs = new Queue<AudioClip>();

            // General Setup
            source.outputAudioMixerGroup = Audio.Instance.Mixer;
            source.priority = ind;
            source.dopplerLevel = 0f;
            source.spatialBlend = 0f;

            // State Setup
            if (state == Audio.States.Playing){
                Audio.Instance.PlaySong(SourceIndex, null);
            }
        }
    }

    public static Audio Instance { get; private set; }

    // Audio
    [System.NonSerialized] public UnityEngine.Audio.AudioMixerGroup Mixer;
    [System.NonSerialized] public List<Audio.Source> sources;

    void Awake(){
        if (Instance == null) {
            Instance = this;
        } else {
            Debug.LogError(this.GetType().Name + " " + "Cannot have multiple instances");
            Destroy (gameObject);  
        }

        DontDestroyOnLoad(this.gameObject);
    }

    // Utilities
    public Audio.States GetState(int SourceIndex){
        AutoAddAudioSource(SourceIndex, Audio.States.Stopped);

        return sources[SourceIndex].state;
    } 

    public AudioSource GetAudioSource(int SourceIndex){
        AutoAddAudioSource(SourceIndex, Audio.States.Stopped);

        return sources[SourceIndex].source;
    } 

    public Audio.Source GetSource(int SourceIndex){
        AutoAddAudioSource(SourceIndex, Audio.States.Stopped);

        return sources[SourceIndex];
    } 

    public float GetAudioSourceTime(int SourceIndex){
        AutoAddAudioSource(SourceIndex, Audio.States.Stopped);

        return ((sources[SourceIndex].source.timeSamples) / (Data.SongSamples)) * Data.SongLength;
    }

    // Effects
    public IEnumerator LerpSongVolume(int SourceIndex, bool Stop, float toVol = 0f, float fadeTime = 1f){
        AutoAddAudioSource(SourceIndex, Audio.States.Playing);

        Unitilities.Counter elapsedTime = new Unitilities.Counter();
        float fromVol = GetAudioSource(SourceIndex).volume;

        while (elapsedTime.t<float>() < fadeTime){
            elapsedTime.Update(Time.deltaTime);
            GetAudioSource(SourceIndex).volume = Mathf.Lerp(fromVol, toVol, elapsedTime.t<float>() / fadeTime);

            yield return null;
        }
    }

    public IEnumerator RewindToStart(int SourceIndex){
        GetSource(SourceIndex).state = Audio.States.Rewinding;

        AudioSource source = GetAudioSource(SourceIndex);
        Unitilities.Counter elapsedTime = new Unitilities.Counter();
        float time = GetAudioSourceTime(SourceIndex);

        StartCoroutine(LerpSongVolume(SourceIndex, false, 0.1f, 1f));  

        while (elapsedTime.t<float>() < time && GetAudioSourceTime(SourceIndex) > 1f){
            elapsedTime.Update(Time.deltaTime);
            source.pitch = Mathf.Lerp(1f, -time, elapsedTime.t<float>() / (GetAudioSourceTime(SourceIndex) / 4f));
            
            yield return null;
        }  

        GetSource(SourceIndex).state = Audio.States.Playing;

        source.volume = 1f;

        source.pitch = 1f;
    }

    public float GetFrequency(){
        float result = 0f;
        Audio.Instance.Mixer.audioMixer.GetFloat("PitchBend", out result);
        return result;
    }

    public IEnumerator ChangeFrequency(int SourceIndex, float start, float end){
        Audio.Instance.Mixer.audioMixer.SetFloat("PitchBend", Mathf.Lerp(start, end, Time.deltaTime));
        yield return null;
    }

    public float GetPitch(){
        float result = 0f;
        Audio.Instance.Mixer.audioMixer.GetFloat("MasterPitch", out result);
        return result;
    }

    public IEnumerator ChangePitch(int SourceIndex, float start, float end){
        Audio.Instance.Mixer.audioMixer.SetFloat("MasterPitch", Mathf.Lerp(start, end, Time.deltaTime));
        yield return null;
    }

    // Queue
    public void DequeueSong(int SourceIndex){
        AutoAddAudioSource(SourceIndex, Audio.States.Stopped);

        sources[SourceIndex].DequeueItems();
    }

    public void EnqueueSong(int SourceIndex, AudioClip s){
        AutoAddAudioSource(SourceIndex, Audio.States.Stopped);

        sources[SourceIndex].EnqueueItems(s);
    }

    // Song Functions
    public void PauseSong(int SourceIndex){
        AutoAddAudioSource(SourceIndex, Audio.States.Playing);

        GetAudioSource(SourceIndex).Pause();
        sources[SourceIndex].state = Audio.States.Paused;
    }  

    public void UnPauseSong(int SourceIndex){
        AutoAddAudioSource(SourceIndex, Audio.States.Playing);

        GetAudioSource(SourceIndex).UnPause();
        sources[SourceIndex].state = Audio.States.Playing;
    }  

    public void StopSong(int SourceIndex){
        AutoAddAudioSource(SourceIndex, Audio.States.Stopped);

        GetAudioSource(SourceIndex).Stop();
        GetAudioSource(SourceIndex).clip = null;

        if (GetState(SourceIndex) != Audio.States.PlayingSoundEffect) DequeueSong(SourceIndex);

        sources[SourceIndex].state = Audio.States.Stopped;
    }

    public void PlaySong(
            int SourceIndex, 
            AudioClip s
        ){

        AutoAddAudioSource(SourceIndex, Audio.States.Playing);
        
        GetAudioSource(SourceIndex).clip = (s == null) ? sources[SourceIndex].songs.Peek() : s;
        GetAudioSource(SourceIndex).Play();
        sources[SourceIndex].state = (s == null) ? Audio.States.Playing : Audio.States.PlayingSoundEffect;
    }

    public IEnumerator PassSoundEffectToCoroutine(
            int SourceIndex, 
            AudioClip s, 
            float delay
        ){
        
        yield return new WaitForSeconds(delay + 0.1f);

        Audio.Instance.PlaySong(SourceIndex, s);
    }

    public void PlaySoundEffect(
            int SourceIndex, 
            AudioClip s, 
            float delay
        ){

        AutoAddAudioSource(SourceIndex, Audio.States.Stopped);

        StartCoroutine(PassSoundEffectToCoroutine(SourceIndex, s, delay));
    }

    // Source Functions
    public void AddAudioSource(Audio.States state){
        this.transform.gameObject.AddComponent<AudioSource>();
        AudioSource[] audioSources = this.transform.gameObject.GetComponents<AudioSource>();

        Audio.Source source = new Audio.Source();
        source.Initialize(audioSources.Length - 1, audioSources[audioSources.Length - 1], state);
        sources.Add(source);
    }

    public void AutoAddAudioSource(int SourceIndex, Audio.States state){
        // If SourceIndex is more than the amount of sources
        if (SourceIndex > sources.Count - 1){

            // If the next Source Index doesn't equal the amount of sources
            if (SourceIndex + 1 != sources.Count - 1){
                // Make sure the newly created source is stopped
                AddAudioSource(Audio.States.Stopped);

                // Try recursively adding another source
                AutoAddAudioSource(SourceIndex, state);
            } else {
                // Add a source
                AddAudioSource(state);
            }
        }
    }

    // Analysis Functions
    public float[] GetSpectrumDataFromSource(int SourceIndex){
        float[] SpectrumData = new float[Mathf.Clamp(Settings.Advanced.SampleDepth, 1024, 8192)];
        
        Audio.Instance.GetAudioSource(SourceIndex).GetSpectrumData(SpectrumData, 0, Settings.Advanced.fftWindow); 

        return SpectrumData;
    }

    // Simulate Audio
    AudioClip GenerateAudioClip(string name, float[] input){
        AudioClip ac = AudioClip.Create(name, input.Length, Data.SongChannels, (int) Data.SongFrequency, false);
        ac.SetData(input, 0);

        return ac;
    }

    /*float[] GenerateTone(float frequency, int duration){
        float[] result = new float[duration];
        int vol = 1f;

        for (int i = 0; i < result.Length; i++){
            vol -= (1f/result.Length);
            //result[i] = Mathf.Sin(w * i) * Mathf.Exp(-2 * i);
            result[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / Data.SongFrequency);
        }

        return result;
    }*/

    // Update
    void Update(){
        for (int i = 0; i < sources.Count; i++){
            if (!sources[i].source.isPlaying && (GetState(i) == Audio.States.Playing)){
                //StopSong(i); //Bug: ~4565 this is executing at wrong times
            }
        }
        
    }

    // Initialization
    void Start(){
        Mixer = Resources.Load<UnityEngine.Audio.AudioMixerGroup>("Audio/Handling/Mixer");
        sources = new List<Audio.Source>();
    }
}
