using System;
using System.Runtime.InteropServices;
using System.Threading;
using SDL2;

namespace SyncAudioPlayer
{
    public class SDLAudio
    {
        private static long[] syncErrors = new long[100];
        private static int syncPointer;
        private static TimeLock syncLock;
        private static uint audioDevice;
        private static byte[] loadedMusic;
        private static int bytesLeft;
        private static SDL.SDL_AudioSpec audioSpec;
        private static SDL.SDL_AudioSpec fileSpec;

        public static bool DataAvailable()
        {
            return bytesLeft > 0;
        }

        internal static bool Initialize()
        {
            if (!SDLAudio.OpenSound())
            {
                return false;
            }
            if (!SDLAudio.LoadSound())
            {
                return false;
            }
            return true;
        }

        internal static bool OpenSound()
        {
            SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);
            SDL.SDL_AudioSpec desiredSpec = new SDL.SDL_AudioSpec();
            desiredSpec.channels = 2;
            desiredSpec.format = SDL.AUDIO_S16;
            desiredSpec.freq = 44100;
            desiredSpec.samples = 128;
            desiredSpec.callback = FillAudioBuffer;
            uint retValue = SDL.SDL_OpenAudioDevice(SDL.SDL_GetAudioDeviceName(0,0), 0, ref desiredSpec, out audioSpec, 0);
            if (retValue == 0)
            {
                Console.WriteLine("Couldn't open audio device: " + SDL.SDL_GetError());
                return false;
            }
            audioDevice = retValue;
            return true;
        }

        internal static bool LoadSound()
        {
            IntPtr audioPtr;
            uint audioLength;
            fileSpec = SDL.SDL_LoadWAV("stb.wav", ref audioSpec, out audioPtr, out audioLength);
            if (audioPtr == IntPtr.Zero)
            {
                Console.WriteLine("Couldn't open audio file: " + SDL.SDL_GetError());
                return false;
            }
            loadedMusic = new byte[audioLength];
            bytesLeft = (int)audioLength;
            Marshal.Copy(audioPtr, loadedMusic, 0, (int)audioLength);
            SDL.SDL_FreeWAV(audioPtr);
            int samples = (int)(audioLength / (audioSpec.channels * SizeOfAudioFormat(audioSpec.format)));
            double seconds = samples / (double)audioSpec.freq;
            Console.WriteLine("Loaded " + audioLength + " bytes, " + samples + " samples, " + seconds + " seconds.");
            return true;
        }

        internal static void PlaySound()
        {
            SDL.SDL_PauseAudioDevice(audioDevice, 0);
        }

        private static void FillAudioBuffer(IntPtr userData, IntPtr stream, int length)
        {
            //Console.WriteLine("Filling audio buffer: " + length + ", bytes left: " + bytesLeft);
            if (bytesLeft == 0)
            {
                return;
            }
            byte[] src = new byte[length];
            int fillBytes = length > bytesLeft ? bytesLeft : length;
            Array.Copy(loadedMusic, loadedMusic.Length - bytesLeft, src, 0, fillBytes);
            Marshal.Copy(src, 0, stream, src.Length);
            bytesLeft -= fillBytes;
            if (syncLock != null)
            {
                SetSampleError();
            }
        }

        private static int SizeOfAudioFormat(ushort audioFormat)
        {
            switch (audioFormat)
            {
                case SDL.AUDIO_U8:
                case SDL.AUDIO_S8:
                    return 1;
                case SDL.AUDIO_U16LSB:
                case SDL.AUDIO_U16MSB:
                case SDL.AUDIO_S16LSB:
                case SDL.AUDIO_S16MSB:
                    return 2;
                case SDL.AUDIO_F32LSB:
                case SDL.AUDIO_F32MSB:
                    return 4;
            }
            return 0;
        }

        public static void AdjustSamples(int adjustValue)
        {
            int sizeOfSample = SizeOfAudioFormat(audioSpec.format) * audioSpec.channels;
            double seconds = adjustValue / (double)audioSpec.freq;
            Console.WriteLine("Adjusting " + sizeOfSample * adjustValue + " bytes, " + adjustValue + " samples, "  + seconds + " seconds.");
            bytesLeft -= sizeOfSample * adjustValue;
            if (bytesLeft < 0)
            {
                bytesLeft = 0;
            }
            if (bytesLeft > loadedMusic.Length)
            {
                bytesLeft = loadedMusic.Length;
            }
        }

        public static void AdjustSeconds(double adjustValue)
        {
            AdjustSamples((int)(audioSpec.freq * adjustValue));
        }

        public static void SetSyncLock(TimeLock syncLock)
        {
            SDLAudio.syncLock = syncLock;
            syncPointer = 0;
            for (int i = 0; i < syncErrors.Length; i++)
            {
                syncErrors[i] = 0;
            }
        }

        private static void SetSampleError()
        {
            //Get current audio sample
            int positionBytes = loadedMusic.Length - bytesLeft;
            int bytesPerSample = audioSpec.channels * SizeOfAudioFormat(audioSpec.format);
            int currentAudioSample = positionBytes / bytesPerSample;
            //Get target audio sample
            long currentTick = syncLock.GetCurrentTick();
            long targetAudioSample = (currentTick * audioSpec.freq) / System.Diagnostics.Stopwatch.Frequency;
            syncErrors[syncPointer] = targetAudioSample - currentAudioSample;
            syncPointer++;
            if (syncPointer == syncErrors.Length)
            {
                syncPointer = 0;
            }
        }

        public static long GetSampleError()
        {
            long totalError = 0;
            foreach (long currentError in syncErrors)
            {
                totalError += currentError;
            }
            long averageError = totalError / syncErrors.Length;
            return averageError;
        }
    }
}

