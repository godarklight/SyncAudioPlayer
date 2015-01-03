using System;
using DarkNetwork;
using MessageStream2;

namespace SyncAudioPlayer
{
    public class NetworkMessages
    {
        //ID 0
        public static void HandleSyncTime(SyncObject stateObject, byte[] messageData)
        {
            if (stateObject.offsetCount == 10)
            {
                return;
            }
            using (MessageReader mr = new MessageReader(messageData))
            {
                long ourTime = mr.Read<long>();
                long serverTime = mr.Read<long>();
                long currentTime = DateTime.UtcNow.Ticks;
                //NTP algorithm with 3 params instead of 4.
                long latency = currentTime - ourTime;
                long halfLatency = latency / 2;
                long serverTimeAdjusted = serverTime + halfLatency;
                long offset = serverTimeAdjusted - currentTime;
                stateObject.offsetTime[stateObject.offsetCount] = offset;
                stateObject.offsetCount++;
                if (stateObject.offsetCount == 10)
                {
                    long totalTime = 0;
                    foreach (long offsetTime in stateObject.offsetTime)
                    {
                        totalTime += offsetTime;
                    }
                    stateObject.offsetTick = totalTime / stateObject.offsetTime.Length;
                    stateObject.isSynced = true;
                }
                else
                {
                    SendSyncTime(stateObject);
                }
            }
            Console.WriteLine("Syncing!");
        }

        //ID 1
        public static void HandlePlay(SyncObject stateObject, byte[] messageData)
        {
            if (stateObject.isSynced)
            {
                using (MessageReader mr = new MessageReader(messageData))
                {
                    long startTime = mr.Read<long>();
                    long startTimeAdjusted = startTime + stateObject.offsetTick;
                    long timeNow = DateTime.UtcNow.Ticks;
                    long offsetTicks = startTimeAdjusted - timeNow;
                    TimeLock newLock = new TimeLock(offsetTicks);
                    Console.WriteLine("Playing (" + offsetTicks + " late)!");
                    SDLAudio.SetSyncLock(newLock);
                    SDLAudio.PlaySound();
                    stateObject.isPlaying = true;
                }
            }
        }

        //ID 2
        public static void HandleStop(SyncObject stateObject, byte[] messageData)
        {
            Console.WriteLine("Stopping!");
        }

        public static void SendSyncTime(SyncObject stateObject)
        {
            NetworkMessage newMessage = new NetworkMessage(0);
            using (MessageWriter mw = new MessageWriter())
            {
                mw.Write<long>(DateTime.UtcNow.Ticks);
                newMessage.messageData = mw.GetMessageBytes();
            }
            stateObject.networkClient.QueueMessage(newMessage);
        }
    }
}

