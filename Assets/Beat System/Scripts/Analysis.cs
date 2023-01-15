/// <summary>
/// Copyright 2022, Loki Alexander Button Hornsby (Loki Hornsby), All rights reserved.
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Song {
    public static class Analysis {
        static void FinishedAnalysing(){
            Debug.Log("Analysis Finished");
            Data.SongAnalyzed = true;
        }

        // Queues all tasks needed and then executes them
        public static void Perform(){
            ThreadHandling.Finished += FinishedAnalysing;

            ThreadHandling.QueueTask(Analysis.General.CreateArray());
            ThreadHandling.QueueTask(Analysis.Wave.CreateArray());
            ThreadHandling.QueueTask(Analysis.Frequency.CreateArray());
            ThreadHandling.QueueTask(Analysis.Volume.CreateArray());

            ThreadHandling.ExecuteTasks();
        }

        public static class Average {
            // Bug: ~1623 index out of range
            // Take an average of the data given an index
            public static float[] Take(float[] input){
                float[] result = new float[input.Length];

                Unitilities.Counter offset = new Unitilities.Counter();
                Unitilities.Counter counter = new Unitilities.Counter();
                Unitilities.Counter sum = new Unitilities.Counter();

                for (int i = 0; i < input.Length; i++){
                    sum.Update(input[i]);
                    counter.Update(1);

                    if (counter.t<int>() == Settings.Advanced.AverageSplit + offset.t<int>()){
                        for (int j = 0; j < Settings.Advanced.AverageSplit; j++){
                            result[counter.t<int>() - (j)] = (sum.t<float>() / Settings.Advanced.AverageSplit);
                        }
                        
                        sum.Set(0.0f);
                        offset.Set(counter.t<int>());
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
                            result[i] = result[i] + input[i - Settings.Advanced.SampleDepth];
                        }
                    }

                    return result;
                }
            }
        }
    
        public static class General {
            public static float[][][] Array;

            // Get level of detail
            public static float GetLOD(){
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
            } 

            // Scans through the array and splits the raw data into arrays with size of Settings.Advanced.SampleDepth
            public static Action CreateArray(){
                return () => {

                // Counters
                Unitilities.Counter WaveCounter = new Unitilities.Counter();
                Unitilities.Counter SampleCounter = new Unitilities.Counter();
                Unitilities.Counter DataCounter = new Unitilities.Counter();

                // End of loop
                bool end = false;

                // Setup Array
                float[][][] ScannedData = new float[
                    Mathf.FloorToInt(Data.SongSamples / (Settings.Advanced.LowestHeardFrequency * Settings.Advanced.SampleDepth))][][];
                
                if (WaveCounter.t<int>() < ScannedData.Length){
                    ScannedData[WaveCounter.t<int>()] = new float[Settings.Advanced.LowestHeardFrequency][];
                    ScannedData[WaveCounter.t<int>()][SampleCounter.t<int>()] = new float[Settings.Advanced.SampleDepth];

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
                    for (int i = 0; i < Data.SongSamples; i++){
                        // Store an item every loop
                        if (!end) ScannedData[WaveCounter.t<int>()][SampleCounter.t<int>()][DataCounter.t<int>()] = Mathf.Abs(Data.SongData[i]); // Because audio ranges from -1 to 1

                        // Move to next data slot
                        DataCounter.Update(1);

                        // If the data slot position is over the sample depth limit
                        if (DataCounter.t<int>() + 1 > Settings.Advanced.SampleDepth){ // (+1 to account for arrays starting at index 0)
                            // Move to new sample array
                            SampleCounter.Update(1);
                            
                            // If the sample slot position is over the wave limit
                            if (SampleCounter.t<int>() + 1 > Settings.Advanced.LowestHeardFrequency){ // (+1 to account for arrays starting at index 0)
                                // Move to new wave array
                                WaveCounter.Update(1);
        
                                if (WaveCounter.t<int>() + 1 > Data.SongSamples / (Settings.Advanced.LowestHeardFrequency * Settings.Advanced.SampleDepth)){
                                    end = true;
                                }
                                
                                // Create new wave array
                                if (!end) ScannedData[WaveCounter.t<int>()] = new float[Settings.Advanced.LowestHeardFrequency][];
                                
                                // Reset Sample Counter
                                SampleCounter.Set(0);
                            } 
                            
                            // Create new sample array
                            if (!end) ScannedData[WaveCounter.t<int>()][SampleCounter.t<int>()] = new float[Settings.Advanced.SampleDepth];

                            // Reset Data Counter
                            DataCounter.Set(0);
                        }
                    }
                }

                Array = ScannedData;

                };
            }
        }

        public static class Wave {
            public static float[][] Array;

            // Get a Wave
            public static float[] Get(float[][] input){
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
            public static Action CreateArray(){
                return () => {

                float[][] waves = new float[Analysis.General.Array.Length][];

                for (int i = 0; i < Analysis.General.Array.Length; i++){
                    waves[i] = Get(Analysis.General.Array[i]); // Convert to 1d array
                }

                Array = waves;

                };
            }
        }

        public static class Frequency { // Also known as Hz
            public static float[] Array;
            public static List<Analysis.Frequency.Note> Notes;
            public static float ArrayAverage;

            // Get frequency from a raw wave
            public static float Get(float[] wave){
                Unitilities.Counter Waves = new Unitilities.Counter();
                Unitilities.Counter QuarterWavesFound = new Unitilities.Counter();
                
                for (int i = 0; i < wave.Length; i++){
                    if (i < wave.Length - 1){
                        // Use mathf.abs to make sure wave is always > 0 so instead of a wave it's 2 hills
                        float Signal = Mathf.Abs(wave[i]);
                        float NextSignal = Mathf.Abs(wave[i + 1]);

                        // Top of hill 1
                        if (Signal > NextSignal && QuarterWavesFound.t<int>() == 0){
                            QuarterWavesFound.Update(1);
                        }

                        // Bottom of hill 1 
                        if (Signal < NextSignal && QuarterWavesFound.t<int>() == 1){
                            QuarterWavesFound.Update(1);
                        }
                        
                        // Top of hill 2
                        if (Signal > NextSignal && QuarterWavesFound.t<int>() == 2){
                            QuarterWavesFound.Update(1);
                        }

                        // Bottom of hill 2
                        if (Signal < NextSignal && QuarterWavesFound.t<int>() == 3){
                            Waves.Update(1);
                            QuarterWavesFound.Set(0);
                        }
                    }
                }

                // Calculate final frequency F = T/1 (Inverse frequency seems to be F = 1/T and it converts silence to the highest possible frequency?)
                float frequency = (
                    (
                        Waves.t<float>() + (
                            (
                                QuarterWavesFound.t<float>() + 1f
                            ) / 4f
                        )
                    ) / (float) Settings.Advanced.LowestHeardFrequency
                ) * 1000f;
                
                return frequency;
            }

            // Create an array of frequencies
            public static Action CreateArray(){
                return () => {
                
                // Frequency
                float[] frequencies = new float[Analysis.Wave.Array.Length];
                Notes = new List<Analysis.Frequency.Note>();

                for (int i = 0; i < Analysis.Wave.Array.Length; i++){
                    frequencies[i] = Get(Analysis.Wave.Array[i]);
                }

                Array = frequencies;
                ArrayAverage = Array.Sum() / Array.Length;

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
                }

                };
            }

            // Todo: calculate separate notes - attack, duration and onset
            // https://dsp.stackexchange.com/questions/36822/method-for-calculating-musical-note-based-magnitude-spectrum
            // https://wikimedia.org/api/rest_v1/media/math/render/svg/52f27939c12917765ace98cf3db6c0bdd8c806ed
            // https://dsp.stackexchange.com/questions/84212/performing-onset-detection-in-audio-without-the-use-of-an-fft?noredirect=1#comment177715_84212
            // https://www.nti-audio.com/en/support/know-how/fast-fourier-transform-fft
           
            public class Note {
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
            }
        }

        public static class Volume { // Also known as magnitude
            public static float[] Array;
            public static float ArrayAverage;

            // Convert input data to simulated volume
            public static float Get(float Sum){
                // Grab an average of the mean of the sum and sqrt the sum to get an RMS value
                float Avg = Mathf.Sqrt(Sum) / Settings.Advanced.SampleDepth;
                // use the song's peak to calculate an amplitude
                float amplitude = Avg / Data.SongPeak;
                // Multiply to get percentage
                float percentage = (amplitude * Settings.Advanced.SampleDepth) * 100f;
                // Get it into a DB - divide by 20f
                float result = percentage / 20f;

                return result;
            }

            // Create an array of volumes
            public static Action CreateArray(){
                return () => {
                
                // Set Data
                float[] volumeArr = new float[Analysis.Wave.Array.Length];

                for (int i = 0; i < volumeArr.Length; i++){
                    // Get Data
                    volumeArr[i] = Get(Analysis.Wave.Array[i].Sum());
                }

                Array = volumeArr;
                ArrayAverage = Array.Sum() / Array.Length;

                };
            }
        }
    }
}