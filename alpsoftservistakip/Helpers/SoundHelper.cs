using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;

namespace alpsoftservistakip.Helpers
{
    public static class SoundHelper
    {
        private static byte[] _cachedChimeWav;

        public static void PlayOnayChime()
        {
            Task.Run(() =>
            {
                try
                {
                    if (_cachedChimeWav == null)
                    {
                        _cachedChimeWav = GenerateChimeWav();
                    }

                    using (var ms = new MemoryStream(_cachedChimeWav))
                    using (var player = new SoundPlayer(ms))
                    {
                        player.PlaySync();
                    }
                }
                catch
                {
                    try
                    {
                        SystemSounds.Asterisk.Play();
                    }
                    catch
                    {
                        try { Console.Beep(1200, 300); } catch { }
                    }
                }
            });
        }

        private static byte[] GenerateChimeWav()
        {
            int sampleRate = 44100;
            double duration1 = 0.22;
            double duration2 = 0.45;
            int samples1 = (int)(sampleRate * duration1);
            int samples2 = (int)(sampleRate * duration2);
            int totalSamples = samples1 + samples2;

            short[] samples = new short[totalSamples];

            // 1. Ton: 659.25 Hz (E5)
            double freq1 = 659.25;
            for (int i = 0; i < samples1; i++)
            {
                double t = (double)i / sampleRate;
                double env = Math.Exp(-4.0 * t / duration1);
                double s = Math.Sin(2.0 * Math.PI * freq1 * t) * env;
                samples[i] = (short)(s * 22000);
            }

            // 2. Ton: 1046.50 Hz (C6)
            double freq2 = 1046.50;
            for (int i = 0; i < samples2; i++)
            {
                double t = (double)i / sampleRate;
                double env = Math.Exp(-3.5 * t / duration2);
                double s = Math.Sin(2.0 * Math.PI * freq2 * t) * env;
                samples[samples1 + i] = (short)(s * 25000);
            }

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                // RIFF header
                bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                bw.Write(36 + totalSamples * 2);
                bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

                // fmt chunk
                bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                bw.Write(16); // subchunk size
                bw.Write((short)1); // PCM
                bw.Write((short)1); // mono
                bw.Write(sampleRate);
                bw.Write(sampleRate * 2); // byte rate
                bw.Write((short)2); // block align
                bw.Write((short)16); // bits per sample

                // data chunk
                bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                bw.Write(totalSamples * 2);
                for (int i = 0; i < totalSamples; i++)
                {
                    bw.Write(samples[i]);
                }

                return ms.ToArray();
            }
        }
    }
}
