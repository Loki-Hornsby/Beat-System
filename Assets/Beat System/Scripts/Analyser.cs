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
        }

        public Analyser.Settings settings;

        /// <summary>
        /// A single filter
        /// </summary>
        [Serializable] public class Filter {
            // Averaging
            public enum PossibleAverageSplits {
                _0 = 0,
                _2 = 2,
                _4 = 4,
                _6 = 6,
                _8 = 8,
                _10 = 10,
                _12 = 12,
                _14 = 14,
                _16 = 16
            };

            public PossibleAverageSplits AverageSplit; 

            // Range
            public float range;

            /// <summary>
            /// Applies a "Comb" filter to our data
            /// </summary>
            void Comb(ref float[] input, float ms, float Frequency, int SampleDepth){
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
            void TakeAverage(ref float[] input, float AverageSplit){
                // Counters
                int offset = 0;
                int counter = 0;
                float sum = 0f; 

                // Loop through our input array
                for (int i = 0; i < input.Length; i++){
                    // Add the current item to our sum float
                    sum += input[i];

                    // Increment our counter
                    counter++;

                    // If our counter is equal to our average split + our offset
                    if (counter == AverageSplit + offset){
                        // Create a for loop the size of our average split variable
                        for (int j = 0; j < AverageSplit; j++){
                            // Change the input (located at our counters current position - our j index) 
                                // to our sum divided by our average split
                            input[counter - (j)] = (sum / AverageSplit);
                        }
                        
                        // Reset the sum
                        sum = 0f;

                        // Apply our offset
                        offset = counter;
                    }
                }
            }

            /// <summary>
            /// Squish our input into a range
            /// </summary>
            void FitRange(ref float[] input, float range){
                // Get the largest value in the data
                float max = input.Max();

                // Loop through each input
                for (int i = 0; i < input.Length; i++){
                    // Change the currently indexed input to itself divided by the largest input multiplied by range
                    input[i] = (input[i] / max) * range;
                }
            }

            /// <summary>
            /// Apply our filter
            /// </summary>
            public void Apply(out float[] arr, float[] input){
                // Initialize our array
                arr = input;

                /*
                // Apply the comb filter to them using input settings
                if (comb) Comb (
                    ref arr,
                    10f,
                    data.Frequency,
                    (int) settings.SampleDepth
                );
                */

                // Take an average of our generated frequency array
                if ((int) AverageSplit > 0) TakeAverage(
                    ref arr, 
                    (int) AverageSplit
                );

                // Put our data into a specific range
                if (range > 0f) FitRange(
                    ref arr, 
                    range
                );
            }
        }

        /// <summary>
        /// Contains all our data
        /// </summary>
        public class Data {
            // Output data
            public float[][][] O_General;
            public float[][] O_Wave;
            public Notes.Note[] O_Notes;

            // Generic data
            public float[] frequencies;
            public float[] volumes;
            public float[] pitches;

            // Predefined generic data
            public AudioClip Clip;          // The Song (AudioClip)
            public float Length;            // Length of the song in seconds
            public float Frequency;         // Sample rate of the song; How many samples were taken per second when recording the song
            public int Channels;            // Channels of the song (normally 2)
            public float Samples;           // Total amount of samples building up the song
            public float Peak;              // Largest item of data
            public float[] RawData;         // Raw PCM data of the song

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

                // Assign our PCM data
                RawData = new float[Mathf.FloorToInt(Samples * Channels)]; 
                Clip.GetData(RawData, 0);

                // Get the peak from our PCM data
                Peak = RawData.Max();
            }
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
        /// Analyse a song
        /// </summary>
        public async Task<Data> Analyse(AudioClip song, Filter frequency_filter, Filter volume_filter, Filter pitch_filter){
            // Creates the data for our analyser
            Data data = new Data(
                song
            );

            // Awaits a new task
            return await Task.Factory.StartNew(() => 
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

                    // Apply filters
                    frequency_filter.Apply(out data.frequencies, data.O_Notes.Select(x => x.frequency).ToArray());
                    volume_filter.Apply(out data.volumes, data.O_Notes.Select(x => x.volume).ToArray());
                    pitch_filter.Apply(out data.pitches, data.O_Notes.Select(x => x.pitch).ToArray());

                    // Return our data
                    return data;
                }
            );
        }
    }
}
