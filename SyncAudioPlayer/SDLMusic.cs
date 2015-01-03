/*
using System;
using SDL2;

namespace SyncAudioPlayer
{
    public class SDLMusic
    {
        private static bool playing;
        private static long[] sampleErrors = new long[SAMPLE_RATE/BUFFER_LENGTH];
        private static int sampleErrorPointer = 0;
        public static long sampleError
        {
            get
            {

                long totalError = 0;
                foreach (long currentError in sampleErrors)
                {
                    totalError += currentError;
                }
                return totalError / sampleErrors.Length;
            }
        }
        private static IntPtr loadedMusic;
        private static TimeLock playLock;
        private static long currentPos;
        private const int SAMPLE_RATE = 44100;
        private const int BUFFER_LENGTH = 512;

        internal static bool OpenSound()
        {
            SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);
            int retValue = SDL_mixer.Mix_OpenAudio(SAMPLE_RATE, SDL_mixer.MIX_DEFAULT_FORMAT, SDL_mixer.MIX_DEFAULT_CHANNELS, BUFFER_LENGTH);
            if (retValue == -1)
            {
                Console.WriteLine("Couldn't open audio device: " + SDL.SDL_GetError());
                return false;
            }
            return true;
        }

        internal static bool LoadSound()
        {
            loadedMusic = SDL_mixer.Mix_LoadMUS("stb.wav");
            if (loadedMusic == IntPtr.Zero)
            {
                Console.WriteLine("Couldn't open audio file: " + SDL.SDL_GetError());
                return false;
            }
            return true;
        }

        internal static void PlaySound(TimeLock playLock)
        {
            SDLMusic.playLock = playLock;
            SDL_mixer.Mix_SetPostMix(TrackSamples, IntPtr.Zero);
            SDL_mixer.Mix_PlayMusic(loadedMusic, 0);
            playing = true;
        }

        internal static void PauseSound()
        {
            SDL_mixer.Mix_PauseMusic();
            playing = false;
        }

        internal static void UnpauseSound()
        {
            SDL_mixer.Mix_ResumeMusic();
            playing = true;
        }

        internal static void TrackSamples(IntPtr uData, IntPtr stream, int length)
        {
            if (playing)
            {
                currentPos += length;
                double tickPerSecond = System.Diagnostics.Stopwatch.Frequency / (double)(44100 * 2 * 2);
                long sync = (long)(currentPos * tickPerSecond);
                sampleErrors[sampleErrorPointer] = playLock.GetError(sync);
                sampleErrorPointer++;
                if (sampleErrorPointer == sampleErrors.Length)
                {
                    sampleErrorPointer = 0;
                }
            }
        }

        internal static void UnloadSound()
        {
            SDL_mixer.Mix_FreeMusic(loadedMusic);
        }

        internal static void CloseSound()
        {
            SDL_mixer.Mix_CloseAudio();
        }
    }
}
*/

