using System;
using DarkNetwork;

namespace SyncAudioPlayer
{
    public class SyncObject
    {
        public TimeLock timeLock = new TimeLock(0);
        public NetworkClient<SyncObject> networkClient;
        public bool isSynced = false;
        public bool isPlaying = false;
        public long[] offsetTime = new long[10];
        public long offsetTick;
        public int offsetCount = 0;
    }
}

