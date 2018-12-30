﻿using System.IO;
using NAudio.Wave;
using UnityEngine;

namespace GameUtils
{
    // From http://answers.unity3d.com/questions/737002/wav-byte-to-audioclip.html
    // and https://gamedev.stackexchange.com/a/114886
    // Thanks to https://github.com/nobbele for pointers on this
    public class AudioLoader
    {
        public static AudioClip GetAudio(string path)
        {
            var extension = Path.GetExtension(path);
            Debug.Log($"Path @ GetAudio: {path}");
            var data = File.ReadAllBytes(path);

            switch (extension)
            {
                case ".mp3":
                    return GetMp3Audio(Path.GetFileName(path), data);
                default:
                {
                    Debug.Log($"{extension} files not yet supported");
                    return null;
                }
            }
        }
        
        private static MemoryStream AudioMemStream(WaveStream waveStream)
        {
            var outputStream = new MemoryStream();
            using (var waveWriter = new WaveFileWriter(outputStream, waveStream.WaveFormat))
            {
                var bytes = new byte[waveStream.Length];
                waveStream.Position = 0;
                waveStream.Read(bytes, 0, (int) waveStream.Length);
                waveWriter.Write(bytes, 0, bytes.Length);
                waveWriter.Flush();
            }

            return outputStream;
        }

        private static AudioClip GetMp3Audio(string name, byte[] data)
        {
            // Load data into a stream
            var mp3Stream = new MemoryStream(data);
            // Convert data to WAV format
            var mp3Audio = new Mp3FileReader(mp3Stream);
            // Convert to WAV data
            var wav = new Wav(AudioMemStream(mp3Audio).ToArray());
            Debug.Log(wav);
            var audioClip = AudioClip.Create(name, wav.SampleCount, 1, wav.Frequency, false);
            audioClip.SetData(wav.LeftChannel, 0);
            return audioClip;
        }
        
        public class Wav
        {
            public float[] LeftChannel { get; internal set; }
            public float[] RightChannel { get; internal set; }
            public int ChannelCount { get; internal set; }
            public int SampleCount { get; internal set; }
            public int Frequency { get; internal set; }

            public Wav(byte[] wav)
            {
                // Determine whether mono or stereo audio
                ChannelCount = wav[22];

                Frequency = BytesToInt(wav, 24);

                // Get to first data sub-chunk
                var pos = 12;
                
                // Keep iterating until we find the data chunk
                while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97))
                {
                    pos += 4;
                    var chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
                    pos += 4 + chunkSize;
                }

                pos += 8;
                
                // Pos is now at the start of the sound data
                SampleCount = (wav.Length - pos) / 2;    // 2 bytes per sample (16 bit mono)
                if (ChannelCount == 2) SampleCount /= 2;    // 4 bytes per sample (16 bit stereo)
                
                // Allocate memory (right will be null if only mono sound)
                LeftChannel = new float[SampleCount];
                RightChannel = ChannelCount == 2 ? new float[SampleCount] : null; 
                
                // Write to double array(s)
                var i = 0;
                while (pos < wav.Length)
                {
                    LeftChannel[i] = BytesToFloat(wav[pos], wav[pos + 1]);
                    pos += 2;
                    if (ChannelCount == 2)
                    {
                        RightChannel[i] = BytesToFloat(wav[pos], wav[pos + 1]);
                        pos += 2;
                    }

                    i++;
                }
            }

            public override string ToString()
            {
                return
                    $"[Wav: LeftChannel={LeftChannel}, RightChannel={RightChannel}, ChannelCount={ChannelCount}, SampleCount={SampleCount}, Frequency={Frequency}";
            }

            // Convert two bytes to one float in the range -1 to 1
            private static float BytesToFloat(byte first, byte second)
            {
                // Convert to one short (little endian)
                var s = (short) ((second << 8) | first);
                // Convert to -1 to (below) 1 range
                return s / 32768.0f;
            }

            private static int BytesToInt(byte[] bytes, int offset = 0)
            {
                var value = 0;
                for (var i = 0; i < 4; i++)
                    value |= bytes[offset + i] << (i * 8);

                return value;
            }
        }
    }

}
