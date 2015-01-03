using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using SDL2;
using DarkNetwork;
using MessageStream2;

namespace SyncAudioPlayer
{
    public class MainClass
    {
        private static SyncObject syncObject;

        public static void Main()
        {
            if (Stopwatch.Frequency != 10000000)
            {
                Console.WriteLine("High resolution timer unavailable");
                return;
            }
            if (!SDLAudio.Initialize())
            {
                Console.WriteLine("Failed to initialize SDL");
                return;
            }
            SetupNetwork();
            WaitForTimeLock();
            WaitForPlayCommand();
            while (SDLAudio.DataAvailable())
            {
                Thread.Sleep(1000);
                int sampleError = (int)SDLAudio.GetSampleError();
                Console.WriteLine("Sample error: " + sampleError);
                if (Math.Abs(sampleError) > 30)
                {
                    SDLAudio.AdjustSamples(sampleError);
                }
            }
            Console.WriteLine("Everything is ok");
        }

        private static void SetupNetwork()
        {
            syncObject = new SyncObject();
            NetworkHandler<SyncObject> networkHandler = new NetworkHandler<SyncObject>();
            networkHandler.RegisterCallback(0, NetworkMessages.HandleSyncTime);
            networkHandler.RegisterCallback(1, NetworkMessages.HandlePlay);
            networkHandler.RegisterCallback(2, NetworkMessages.HandleStop);
            NetworkClient<SyncObject> darkNetworkClient = NetworkClient<SyncObject>.ConnectNewTCPClient("192.168.3.2", 6700, networkHandler, syncObject);
            syncObject.networkClient = darkNetworkClient;
            NetworkMessages.SendSyncTime(syncObject);
        }

        private static void WaitForTimeLock()
        {
            Console.WriteLine("Waiting for sync lock");
            while (!syncObject.isSynced)
            {
                Thread.Sleep(100);
            }
            double secondsDifference = syncObject.offsetTick / 100000000d;
            Console.WriteLine("Synced with " + secondsDifference + " seconds offset!");
        }

        private static void WaitForPlayCommand()
        {
            Console.WriteLine("Waiting for play command");
            while (!syncObject.isPlaying)
            {
                Thread.Sleep(100);
            }
            Console.WriteLine("Playing!");
        }

        /*
        private static void PlaySDLAudio()
        {
            Console.WriteLine("Playing with SDL Audio");

            SDLAudio.PlaySound();
            TimeLock syncLock = new TimeLock(0);
            SDLAudio.SetSyncLock(syncLock);
            while (SDLAudio.DataAvailable())
            {
                SDL.SDL_Delay(1000);
                int sampleError = (int)SDLAudio.GetSampleError();
                Console.WriteLine("Sample error: " + sampleError);
                if (Math.Abs(sampleError) > 30)
                {
                    SDLAudio.AdjustSamples(sampleError);
                }
            }
        }
        */
        /*
        private static void PlaySDLMusic()
        {
            Console.WriteLine("Playing with SDL_mixer");
            Console.CancelKeyPress += ControlCPressed;
            if (!SDLMusic.OpenSound())
            {
                return;
            }
            if (!SDLMusic.LoadSound())
            {
                return;
            }
            TimeLock tl = new TimeLock(0);
            SDLMusic.PlaySound(tl);
            while (SDL_mixer.Mix_PlayingMusic() == 1)
            {
                double sampleErrorMs = SDLMusic.sampleError / (double)(Stopwatch.Frequency / 1000);
                Console.WriteLine("Sample error: " + Math.Round(sampleErrorMs, 2) + " ms.");
                SDL.SDL_Delay(100);
            }
            SDLMusic.UnloadSound();
            SDLMusic.CloseSound();
        }

        private static void ControlCPressed(object sender, ConsoleCancelEventArgs args)
        {
            SDLMusic.PauseSound();
            Thread.Sleep(100);
            SDLMusic.UnpauseSound();
            args.Cancel = true;
        }
        */
    }
}

