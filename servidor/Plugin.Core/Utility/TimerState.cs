// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Utility.TimerState
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;
using System.Threading;

namespace Plugin.Core.Utility
{
    public class TimerState
    {
        protected Timer Timer;
        protected DateTime EndDate;
        protected object Sync = new object();

        public bool IsTimer() => this.Timer != null;

        public void StartJob(int Period, TimerCallback Callback)
        {
            lock (this.Sync)
            {
                this.Timer = new Timer(Callback, (object)this, Period, -1);
                this.EndDate = DateTimeUtil.Now().AddMilliseconds((double)Period);
            }
        }

        public void StopJob()
        {
            lock (this.Sync)
            {
                if (this.Timer == null)
                    return;
                this.Timer.Dispose();
                this.Timer = (Timer)null;
            }
        }

        public void StartTimer(TimeSpan Period, TimerCallback Callback)
        {
            lock (this.Sync)
                this.Timer = new Timer(Callback, (object)this, Period, TimeSpan.Zero);
        }

        public int GetTimeLeft()
        {
            if (this.Timer == null)
                return 0;
            int duration = (int)ComDiv.GetDuration(this.EndDate);
            return duration >= 0 ? duration : 0;
        }
    }
}