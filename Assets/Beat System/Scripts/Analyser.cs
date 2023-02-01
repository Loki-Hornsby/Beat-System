/// <summary>
/// Copyright 2022, Loki Alexander Button Hornsby (Loki Hornsby), All rights reserved.
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

using Loki.Signal.Analysis;

namespace Loki.Signal.Analysis {
    public class Analyser : MonoBehaviour {
        /// <summary>
        /// Holds the settings for this analyser
        /// </summary>
        [Serializable]
        public class Settings {
            // ------------------- Sampling ------------------- \\
            public enum PossibleSampleDepths {
                // Warn_2 = 2,        // Don't even
                // Warn_4 = 4,        // This will leave you waiting for an hour (and potentially crash unity)
                // Warn_8 = 8,        // This will show you the sine wave
                // Warn_16 = 16,       // Still Way too small
                // Warn_32 = 32,       // Way too small
                // Warn_64 = 64,       // Still miniscule
                _128 = 128, 
                _256 = 256, 
                _512 = 512, 
                _1024 = 1024, 
                _2048 = 2048,     // Records 1 X every second on 44.1hz
                //_4096 = 4096      // Too high
            };

            /*A small Sample Depth results in fast measurement repetitions with a coarse frequency resolution.
            A large Sample Depth results in slower measuring repetitions with fine frequency resolution.*/
            [Header("Sampling")]
            public PossibleSampleDepths SampleDepth;

            // ------------------- Hearing ------------------- \\
            public enum PossibleLowestHeardFrequencies {
                _20 = 20,
                _25 = 25,
                _30 = 30,
                _35 = 35,
                _40 = 40,
                _45 = 45,
                _50 = 50,
                _55 = 55,
                _60 = 60,
                _65 = 65,
                _70 = 70,
                _75 = 75,
                _80 = 80
            };

            [Header("Hearing")]
            public PossibleLowestHeardFrequencies LowestHeardFrequency;

            // ------------------- FFT Window ------------------- \\
            [Header("FFT")]
            public FFTWindow fftWindow;

            // ------------------- Averaging ------------------- \\
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

            [Header("Averaging")]
            public PossibleAverageSplits AverageSplit;
            public bool TakeAverage;  

            // ------------------- Combing ------------------- \\
            [Header("Combing")]
            public bool Comb;  
        }

        public Analyser.Settings settings;

        /// <summary>
        /// Calculated data
        /// </summary>
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
            public float[][][] O_General;
            public float[][] O_Wave;
            public Notes.Note[] O_Notes;

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

        /// <summary>
        /// Filters for the data
        /// </summary>
        public class Filters {
            /// <summary>
            /// Applies a "Comb" filter to our data
            /// </summary>
            public void Comb(ref float[] input, float ms, float Frequency, int SampleDepth){
                // Define our interval ((Sample Rate (usually 44100) / 1000) * ms)
                float interval = (Frequency / 1000f) * ms;

                // Loop through our array
                for (int i = 0; i < input.Length; i++){
                    // If our index + 1 is more than our interval floored
                    if (i + 1 > Mathf.FloorToInt(interval)){
                        // Set our result to be itself + the item in input at index i - SampleDepth
                        input[i] = input[i] + input[i - SampleDepth];
                    }
                }
            }

            /// <summary>
            /// Gets an average of our data
            /// </summary>
            /*public float[] TakeAverage(float[] input){
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
            }*/
        }

        /// <summary>
        /// Requires Array of <PCM>
        /// </summary>
        public class General {
            // Get level of detail
            /*public float GetLOD(){
                return Analysis.Frequency.Array.Length / 
                    (
                        Data.SongSamples / (
                            Settings.Advanced.PossibleLowestHeardFrequencies.Min() * Settings.Advanced.PossibleSampleDepths.Min()
                        )
                    );
            }

            // Get expected data points
            public float GetEXP(){
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
            public General(out float[][][] Output, float[] Input, int Samples, int SampleDepth, int LowestHeardFrequency){
                // Initialize
                Output = new float[Mathf.FloorToInt(Samples / (LowestHeardFrequency * SampleDepth))][][];
                Output[0] = new float[LowestHeardFrequency][];
                Output[0][0] = new float[SampleDepth];

                // Counters
                int WaveCounter = 0;
                int SampleCounter = 0; 
                int DataCounter = 0;

                // End of loop
                bool end = false;

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
            }
        }

        /// <summary>
        /// Requires Array of <General>
        /// </summary>
        public class Wave {
            // Get a Wave
            float[] Get(float[][] input){
                // Create a result array using the inputs length multiplied by the sub arrays length
                int height = input.Length; // Array length
                int width = input[0].Length; // Sub array length

                // Define our result array
                float[] result = new float[height * width];

                // Loop through the height
                for (int j = 0; j < height; j++) {

                    // Loop through the width
                    for (int i = 0; i < width; i++) {
                        // height (index) * width + width (index) = input[height (index)][width (index)]
                        result[j * width + i] = input[j][i];
                    }
                }

                return result;
            }

            // Create an array of waves
            public Wave(out float[][] Output, float[][][] Input){
                // Initialize our array
                Output = new float[Input.Length][];

                // Loop through each input item
                for (int i = 0; i < Input.Length; i++){
                    // Convert our input to a 1d array
                    Output[i] = Get(Input[i]); 
                }
            }
        }

        /// <summary>
        /// Requires Array of <Wave>
        /// </summary>
        public class Notes {
            // Todo: calculate separate notes - attack, duration and onset
            // https://dsp.stackexchange.com/questions/36822/method-for-calculating-musical-note-based-magnitude-spectrum
            // https://wikimedia.org/api/rest_v1/media/math/render/svg/52f27939c12917765ace98cf3db6c0bdd8c806ed
            // https://dsp.stackexchange.com/questions/84212/performing-onset-detection-in-audio-without-the-use-of-an-fft?noredirect=1#comment177715_84212
            // https://www.nti-audio.com/en/support/know-how/fast-fourier-transform-fft
            // https://stackoverflow.com/questions/16875751/musical-note-duration

            public class Note { 
                public float pitch;
                public float frequency;
                public float volume; // Also known as magnitude
                // public float duration;
                // public int attackPos;
                // public int onsetPos;

                /// <summary>
                /// Get the pitch from a frequency
                /// </summary>
                float GetPitch(float frequency){
                    float p = 9f + 12f * Mathf.Log(frequency / 440f, 2);
                    return p;
                }

                /// <summary>
                /// Get frequency from a raw wave
                /// </summary>
                float GetFrequency(float[] wave, float LowestHeardFrequency){
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

                    // Calculate final frequency F = T/1 
                    // (Inverse frequency seems to be F = 1/T and it converts silence to the highest possible frequency?)
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

                /// <summary>
                /// Get volume from input sum
                /// </summary>
                float GetVolume(float Sum, float SampleDepth, float Peak){
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
                /// Constructor
                /// </summary>
                public Note(float[] Input, float SampleDepth, float Peak, float LowestHeardFrequency){
                    // Assign local values
                    volume = GetVolume(Input.Sum(), SampleDepth, Peak);
                    frequency = GetFrequency(Input, LowestHeardFrequency);
                    pitch = GetPitch(frequency);
                }
            }

            /// <summary>
            /// Create an array of volumes
            /// </summary>
            public Notes(out Notes.Note[] Output, float[][] Input, float SampleDepth, float Peak, float LowestHeardFrequency){
                // Initialize our array
                Output = new Notes.Note[Input.Length];

                // Loop through each input item
                for (int i = 0; i < Output.Length; i++){
                    // Get our data using input settings
                    Output[i] = new Notes.Note(Input[i], SampleDepth, Peak, LowestHeardFrequency);
                }

                /*
                // Averaging
                float[] arr;

                if (Settings.Advanced.AverageSplit > 0){
                    arr = Average.Take(Array);
                } else {
                    arr = Array;
                }*/
            }
        
        }
        
        /// <summary>
        /// Startup
        /// </summary>
        void Awake(){
            //DontDestroyOnLoad(this.gameObject);
        }
        
        /// <summary>
        /// Analyse a song
        /// </summary>
        public async Task<Data> Analyse(AudioClip song){
            // Creates the data for our analyser
            Data data = new Data(
                song
            );

            // Awaits a new task
            return await Task<Data>.Factory.StartNew(() => 
                {
                    Debug.LogWarning("Analyser.cs: Copyright 2022, Loki Alexander Button Hornsby (Loki Hornsby), All rights reserved. Licensed under the BSD 3-Clause 'New' or 'Revised' License");

                    // Gets generic data
                    General general = new General(
                        out data.O_General,
                        data.RawData,
                        (int) data.Samples, 
                        (int) settings.LowestHeardFrequency, 
                        (int) settings.SampleDepth 
                    );

                    // Generates our array of waves
                    Wave wave = new Wave(
                        out data.O_Wave, 
                        data.O_General
                    );

                    // Generates our "notes" (these contain pitch, frequency, volume (amplitude))
                    Notes notes = new Notes(
                        out data.O_Notes, 
                        data.O_Wave,
                        (int) settings.SampleDepth,
                        data.Peak,
                        (int) settings.LowestHeardFrequency
                    );

                    /*
                    // Combing
                    Filters filters = new Filters();

                    if (settings.Comb) solution.QueueTask(
                        () => {
                            Debug.Log("Comb!");

                            // Select all of our pitches
                            IEnumerable<float> pitches = data.O_Notes.Select(x => x.pitch);

                            // Convert them to an array
                            float[] arr = pitches.ToArray();

                            // Apply the comb filter to them using input settings
                            filters.Comb (
                                ref arr,
                                10f,
                                data.Frequency,
                                (int) settings.SampleDepth
                            );

                            // Reassign our pitches (Will this result in anything?)
                            pitches = arr;
                        }
                    );

                    // Averaging
                    if (settings.TakeAverage) solution.QueueTask(
                        () => {
                            Debug.Log("Average!");
                        }
                    );
                    */

                    // Execute all currently queued tasks
                    //solution.ExecuteTasks();

                    // Return our data once this task is finished
                    return data;
                }
            );
        }
    }
}
