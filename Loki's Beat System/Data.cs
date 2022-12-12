/// <summary>
/// Copyright 2022, Loki Alexander Button Hornsby (Loki Hornsby), All rights reserved.
/// Licensed under the BSD 3-Clause "New" or "Revised" License
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

// 1f = 1 second

// Stores data of probed song
namespace Song {
    public static class Data {
        // Analysed 
        public static bool SongAnalyzed;           // Boolean value for wether the song data has finished analysing

        // General
        public static AudioClip Song;              // The Song (AudioClip)

        public static float SongLength;            // Length of the song in seconds
        public static float SongFrequency;         // Sample rate of the song; How many samples were taken per second when recording the song
        public static int SongChannels;            // Channels of the song (normally 2) // Bug: Something is wrong with the channels
        public static float SongSamples;           // Total amount of samples building up the song
        public static float SongPeak;              // Largest item of data

        // Data
        public static float[] SongData;            // Raw data of the song

        static bool IsPowerOfTwo(int x){
            return (x != 0) && ((x & (x - 1)) == 0);
        }
        
        /// Generate data for a selected audio clip
        public static void GenerateData(AudioClip _song){
            SongAnalyzed = false;

            Song = _song;            
            
            SongChannels = Song.channels;          
            SongFrequency = Song.frequency;         
            SongSamples = Song.samples;   
            SongLength = Song.length;

            SongData = new float[Mathf.FloorToInt(SongSamples * SongChannels)]; 
            Song.GetData(SongData, 0);

            SongPeak = SongData.Max();

            // Perform Analysis
            Analysis.Perform();
        }
    }
}
