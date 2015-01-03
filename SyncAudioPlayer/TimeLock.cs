using System;
using System.Diagnostics;

namespace SyncAudioPlayer
{
    public class TimeLock
    {
        //Master clock
        private static Stopwatch masterClock = Stopwatch.StartNew();

        //Epoch
        public long startTime
        {
            get;
            private set;
        }

        //Constructor
        public TimeLock (long offset)
        {
            this.startTime = masterClock.ElapsedTicks + offset;
        }

        //Interface
        public void Adjust(long tickOffset)
        {
            this.startTime += tickOffset;
        }

        public long GetCurrentTick()
        {
            return (masterClock.ElapsedTicks - startTime);
        }
    }
}

