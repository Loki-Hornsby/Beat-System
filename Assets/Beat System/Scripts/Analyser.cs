/// <summary>
/// Copyright 2022, Loki Alexander Button Hornsby (Loki Hornsby), All rights reserved.
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Loki.Signal.Analysis {
    public class Analyser : MonoBehaviour {
        // Finished analysis event
        public delegate void Analysis();
        public event Analysis Finished;

        /// <summary>
        /// The song has finished being analysed for data
        /// </summary>
        void FinishedAnalysing(){
            Debug.Log("Finished Analysis!");
            Finished?.Invoke();
        }

        /// <summary>
        /// Holds the settings for this analyser
        /// </summary>
        [Serializable]
        public class Settings {
            // ------------------- Sample Depth ------------------- \\
            public enum PossibleSampleDepths {
                A = 2,        // Don't even
                B = 4,        // This will leave you waiting for an hour (and potentially crash unity)
                C = 8,        // This will show you the sine wave
                D = 16,       // Still Way too small
                E = 32,       // Way too small
                F = 64,       // Still miniscule
                G = 128, 
                H = 256, 
                I = 512, 
                J = 1024, 
                K = 2048,     // Records 1 X every second on 44.1hz
                L = 4096      // Too high
            };

            /*
                A small Sample Depth results in fast measurement repetitions with a coarse frequency resolution.
                A large Sample Depth results in slower measuring repetitions with fine frequency resolution.
            */
            public PossibleSampleDepths SampleDepth;

            // ------------------- Hearing ------------------- \\
            public enum PossibleLowestHeardFrequencies {
                A = 20,
                B = 25,
                C = 30,
                D = 35,
                E = 40,
                F = 45,
                G = 50,
                H = 55,
                I = 60,
                J = 65,
                K = 70,
                L = 75,
                M = 80
            };

            public PossibleLowestHeardFrequencies LowestHeardFrequency;

            // ------------------- FFT Window ------------------- \\
            public static List<FFTWindow> PossibleFFTWindows = new List<FFTWindow> {
                FFTWindow.Rectangular, 
                FFTWindow.Triangle, 
                FFTWindow.Hamming, 
                FFTWindow.Hanning,
                FFTWindow.Blackman, 
                FFTWindow.BlackmanHarris
            };

            public FFTWindow fftWindow;

            // ------------------- Outliers ------------------- \\
            public bool RemoveOutliers;  

            // ------------------- Average Splits ------------------- \\
            public enum PossibleAverageSplits {
                A = 0,
                B = 2,
                C = 4,
                D = 6,
                E = 8,
                F = 10,
                G = 12,
                H = 14,
                I = 16
            };

            public PossibleAverageSplits AverageSplit;
            public bool TakeAverage = false;  

            /*
            // Auto Calculate Advanced Settings
            public void AutoCalculate(){
                // 44.1hz, 10 seconds, 2048 
                    // results in 10 data points // Bug: 9 data points atm

                //Advanced.SampleDepth = 1024;

                //Debug.Log(Analysis.General.GetEXP());
            }*/
        }

        public Analyser.Settings settings;

        /// <summary>
        /// Hold the data for this analyser
        /// </summary>
        [Serializable]
        public class Data {
            /// <summary>
            /// Predefined generic data
            /// </summary>
            public AudioClip Clip;          // The Song (AudioClip)
            public float Length;            // Length of the song in seconds
            public float Frequency;         // Sample rate of the song; How many samples were taken per second when recording the song
            public int Channels;            // Channels of the song (normally 2)
            public float Samples;           // Total amount of samples building up the song
            public float Peak;              // Largest item of data
            public float[] RawData;         // Raw PCM data of the song

            /// <summary>
            /// Output data
            /// </summary>
            public volatile float[][][] O_General;
            public volatile float[][] O_Wave;
            public volatile float[] O_Volume;
            public volatile float[] O_Frequency;

            /// <summary>
            /// Setup our data
            /// </summary>
            public Data (AudioClip _song) {
                // Pre Defined
                Clip = _song;   
                Channels = Clip.channels;          
                Frequency = Clip.frequency;         
                Samples = Clip.samples;   
                Length = Clip.length;

                // Gets
                RawData = new float[Mathf.FloorToInt(Samples * Channels)]; 
                Clip.GetData(RawData, 0);

                Peak = RawData.Max();
            }
        }

        public Analyser.Data data;

        /// <summary>
        /// Startup
        /// </summary>
        void Awake(){
            DontDestroyOnLoad(this.gameObject);
        }
        
        /// <summary>
        /// Analyse a song
        /// </summary>
        public void Analyse(AudioClip song){
            // Creates the data for our analyser
            data = new Analyser.Data(song);

            // Subscribe the finished event to the function of FinishedAnalysing
            ThreadHandling.Finished += FinishedAnalysing;

            // Queue tasks to our Thread Handling class

            // General
            ThreadHandling.QueueTask(
                Implementation.General.CreateArray(
                    data.O_General,
                    data.RawData,
                    (int) data.Samples, 
                    (int) settings.LowestHeardFrequency, 
                    (int) settings.SampleDepth 
                )
            );

            // Wave
            ThreadHandling.QueueTask(
                Implementation.Wave.CreateArray(
                    data.O_Wave, 
                    data.O_General
                )
            );

            // Volume
            ThreadHandling.QueueTask(
                Implementation.Volume.CreateArray(
                    data.O_Volume, 
                    data.O_Wave,
                    (int) settings.SampleDepth,
                    data.Peak
                )
            );

            // Frequency
            ThreadHandling.QueueTask(
                Implementation.Frequency.CreateArray(
                    data.O_Frequency, 
                    data.O_Wave,
                    (int) settings.LowestHeardFrequency
                )
            );

            // Execute all currently queued tasks
            ThreadHandling.ExecuteTasks();
        }

        /// <summary>
        /// The implementation of our audio analysis system
        /// </summary>
        public static class Implementation {
            /*public static class Average {
                // Bug: ~1623 index out of range
                // Take an average of the data given an index
                public static float[] Take(float[] input){
                    float[] result = new float[input.Length];

                    Unitilities.Counter offset = new Unitilities.Counter();
                    Unitilities.Counter counter = new Unitilities.Counter();
                    Unitilities.Counter sum = new Unitilities.Counter();

                    for (int i = 0; i < input.Length; i++){
                        sum.Update(input[i]);
                        counter++;

                        /// <summary>
                        /// 
                        /// </summary>
                        /// <param name="="></param>
                        /// <typeparam name="int"></typeparam>
                        /// <returns></returns>
                        if (counter == settings.AverageSplit + offset){
                            for (int j = 0; j < settings.AverageSplit; j++){
                                result[counter - (j)] = (sum.t<float>() / settings.AverageSplit);
                            }
                            
                            sum = 0f;
                            offset.Set(counter);
                        }
                    }

                    return result;
                }
            }

            public static class Filters {
                public static class Comb {
                    public static float[] Perform(float[] input, float ms){
                        float interval = (Data.SongFrequency / 1000f) * ms;

                        float[] result = new float[input.Length];

                        for (int i = 0; i < input.Length; i++){
                            result[i] = input[i];

                            if (i + 1 > Mathf.FloorToInt(interval)){
                                //result[i] = result[i] + input[i - Settings.Advanced.SampleDepth];
                            }
                        }

                        return result;
                    }
                }
            }*/

            /// <summary>
            /// Requires Array of <PCM>
            /// </summary>
            public static class General {
                // Get level of detail
                /*public static float GetLOD(){
                    return Analysis.Frequency.Array.Length / 
                        (
                            Data.SongSamples / (
                                Settings.Advanced.PossibleLowestHeardFrequencies.Min() * Settings.Advanced.PossibleSampleDepths.Min()
                            )
                        );
                }

                // Get expected data points
                public static float GetEXP(){
                    const float x = 2048f;
                    const float y = 44100;

                    float x2 = Settings.Advanced.SampleDepth / x;
                    float y2 = Data.SongFrequency / y;

                    Debug.Log(x2);
                    Debug.Log(y2);
                    Debug.Log(Data.SongLength);

                    // (1 / 0.5) * 10
                    // (_ / _) * Data.SongLength = 20

                    return (y2 / x2) * Data.SongLength;
                }*/

                /// <summary>
                /// Scans through the array and splits the raw data into arrays with size of SampleDepth
                /// </summary>
                public static Action CreateArray(float[][][] Output, float[] Input, int Samples, int SampleDepth, int LowestHeardFrequency){
                    return () => {

                    // Counters
                    int WaveCounter = 0;
                    int SampleCounter = 0; 
                    int DataCounter = 0;

                    // End of loop
                    bool end = false;

                    // Setup Array structure
                    Output = new float[Mathf.FloorToInt(Samples / (LowestHeardFrequency * SampleDepth))][][];
                    Output[WaveCounter] = new float[LowestHeardFrequency][];
                    Output[WaveCounter][SampleCounter] = new float[SampleDepth];

                    // 1 Wave = (HearingRange) samples
                    // 1 sample = (SampleDepth) data pieces

                    // (ScannedData)  ScannedData = [Wave, Wave, Wave, Wave....]
                    // (ScannedData[0])  Wave = [Sample, Sample, Sample, Sample, Sample]
                    // (ScannedData[0][0])  Sample = [1, 45, 2, 2, 21, 21, 21, 32, 4, 543, 43, 4, 23, 23, 2, 3....]

                    // ScannedData = [
                    //      Wave [
                    //          Sample [
                    //              1, 4, 2, 5
                    //          ]
                    //      ]
                    //  ]

                    // Loop through all of song data
                    for (int i = 0; i < Samples; i++){
                        // Store an item every loop
                        if (!end) Output[WaveCounter][SampleCounter][DataCounter] = Mathf.Abs(Input[i]); // Because audio ranges from -1 to 1

                        // Move to next data slot
                        DataCounter++;

                        // If the data slot position is over the sample depth limit
                        if (DataCounter + 1 > SampleDepth){ // (+1 to account for arrays starting at index 0)
                            // Move to new sample array
                            SampleCounter++;
                            
                            // If the sample slot position is over the wave limit
                            if (SampleCounter + 1 > LowestHeardFrequency){ // (+1 to account for arrays starting at index 0)
                                // Move to new wave array
                                WaveCounter++;

                                // End condition
                                if (WaveCounter + 1 > Samples / (LowestHeardFrequency * SampleDepth)){
                                    end = true;
                                }
                                
                                // Create new wave array
                                if (!end) Output[WaveCounter] = new float[LowestHeardFrequency][];
                                
                                // Reset Sample Counter
                                SampleCounter = 0;
                            } 
                            
                            // Create new sample array
                            if (!end) Output[WaveCounter][SampleCounter] = new float[SampleDepth];

                            // Reset Data Counter
                            DataCounter = 0;
                        }
                    }

                    };
                }
            }

            /// <summary>
            /// Requires Array of <General>
            /// </summary>
            public static class Wave {
                // Get a Wave
                static float[] Get(float[][] input){
                    float[] result = new float[input.Length * input[0].Length];
                    int height = input.Length;
                    int width = input[0].Length;

                    for (int j = 0; j < height; j++) {
                        for (int i = 0; i < width; i++) {
                            result[j * width + i] = input[j][i];
                        }
                    }

                    return result;
                }

                // Create an array of waves
                public static Action CreateArray(float[][] Output, float[][][] Input){
                    return () => {

                    Output = new float[Input.Length][];

                    for (int i = 0; i < Input.Length; i++){
                        Output[i] = Get(Input[i]); // Convert to 1d array
                    }

                    };
                }
            }

            /// <summary>
            /// Requires Array of <Wave>
            /// </summary>
            public static class Volume { // Also known as magnitude
                /// <summary>
                /// Convert input data to simulated volume
                /// </summary>
                static float Get(float Sum, float SampleDepth, float Peak){
                    // Grab an average of the mean of the sum and sqrt the sum to get an RMS value
                    float Avg = Mathf.Sqrt(Sum) / SampleDepth;
                    // use the song's peak to calculate an amplitude
                    float amplitude = Avg / Peak;
                    // Multiply to get percentage
                    float percentage = (amplitude * SampleDepth) * 100f;
                    // Get it into a DB - divide by 20f
                    float result = percentage / 20f;

                    return result;
                }

                /// <summary>
                /// Create an array of volumes
                /// </summary>
                public static Action CreateArray(float[] Output, float[][] Input, float SampleDepth, float Peak){
                    return () => {
                    
                    // Set Data
                    Output = new float[Input.Length];

                    for (int i = 0; i < Output.Length; i++){
                        // Get Data
                        Output[i] = Get(Input[i].Sum(), SampleDepth, Peak);
                    }

                    };
                }
            }

            /// <summary>
            /// Requires Array of <Wave>
            /// </summary>
            public static class Frequency { // Also known as Hz
                // Get frequency from a raw wave
                static float Get(float[] wave, float LowestHeardFrequency){
                    int Waves = 0;
                    int QuarterWavesFound = 0;
                    
                    for (int i = 0; i < wave.Length; i++){
                        if (i < wave.Length - 1){
                            // Use mathf.abs to make sure wave is always > 0 so instead of a wave it's 2 hills
                            float Signal = Mathf.Abs(wave[i]);
                            float NextSignal = Mathf.Abs(wave[i + 1]);

                            // Top of hill 1
                            if (Signal > NextSignal && QuarterWavesFound == 0){
                                QuarterWavesFound++;
                            }

                            // Bottom of hill 1 
                            if (Signal < NextSignal && QuarterWavesFound == 1){
                                QuarterWavesFound++;
                            }
                            
                            // Top of hill 2
                            if (Signal > NextSignal && QuarterWavesFound == 2){
                                QuarterWavesFound++;
                            }

                            // Bottom of hill 2
                            if (Signal < NextSignal && QuarterWavesFound == 3){
                                Waves++;
                                QuarterWavesFound = 0;
                            }
                        }
                    }

                    // Calculate final frequency F = T/1 (Inverse frequency seems to be F = 1/T and it converts silence to the highest possible frequency?)
                    float frequency = (
                        (
                            Waves + (
                                (
                                    QuarterWavesFound + 1f
                                ) / 4f
                            )
                        ) / (float) LowestHeardFrequency
                    ) * 1000f;
                    
                    return frequency;
                }

                // Create an array of frequencies
                public static Action CreateArray(float[] Output, float[][] Input, float LowestHeardFrequency){      
                    return () => {
                    
                    // Frequency
                    Output = new float[Input.Length];
                    //Notes = new List<Analysis.Frequency.Note>();

                    for (int i = 0; i < Input.Length; i++){
                        Output[i] = Get(Input[i], LowestHeardFrequency);
                    }

                    /*
                    // Averaging
                    float[] arr;

                    if (Settings.Advanced.AverageSplit > 0){
                        arr = Average.Take(Array);
                    } else {
                        arr = Array;
                    }

                    // Notes
                    for (int i = 0; i < arr.Length; i++){
                        Notes.Add(new Analysis.Frequency.Note().Initialize(arr[i]));
                    }

                    // Combing
                    float[] ToComb = new float[Notes.Count];

                    for (int i = 0; i < ToComb.Length; i++){
                        ToComb[i] = Notes[i].pitch;
                    }
                    
                    float[] Combed = Analysis.Filters.Comb.Perform(ToComb, 10f);

                    for (int i = 0; i < ToComb.Length; i++){
                        Notes[i].pitch = Combed[i];
                    }*/

                    };
                }

                // Todo: calculate separate notes - attack, duration and onset
                // https://dsp.stackexchange.com/questions/36822/method-for-calculating-musical-note-based-magnitude-spectrum
                // https://wikimedia.org/api/rest_v1/media/math/render/svg/52f27939c12917765ace98cf3db6c0bdd8c806ed
                // https://dsp.stackexchange.com/questions/84212/performing-onset-detection-in-audio-without-the-use-of-an-fft?noredirect=1#comment177715_84212
                // https://www.nti-audio.com/en/support/know-how/fast-fourier-transform-fft
                
                /*public class Note {
                    public float pitch;
                    public float duration;
                    public int attackPos;
                    public int onsetPos;

                    static float GetPitch(float frequency){
                        float p = 9f + 12f * Mathf.Log(frequency / 440f, 2);
                        return p;
                    }

                    // https://stackoverflow.com/questions/16875751/musical-note-duration
                    static float GetDuration(int start){ // ?
                        return -1f;
                    }

                    static float GetAttackPosition(float frequency){ // ?
                        return -1f;
                    }

                    static float GetOnsetPosition(float frequency){ // ?
                        return -1f;
                    }

                    public Analysis.Frequency.Note Initialize(float frequency){
                        pitch = GetPitch(frequency);

                        return this;
                    }
                }*/
            }
        }
    }
}
