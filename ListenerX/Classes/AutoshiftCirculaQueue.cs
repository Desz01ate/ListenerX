﻿using System;
using System.Collections.Generic;
using System.Timers;

namespace ListenerX.Classes
{
    class AutoshiftCirculaQueue<T> : CircularQueue<T>, IDisposable
    {
        private readonly static Lazy<AutoshiftCirculaQueue<T>> _emptySource = new Lazy<AutoshiftCirculaQueue<T>>(() => new AutoshiftCirculaQueue<T>());
        public static AutoshiftCirculaQueue<T> Empty => _emptySource.Value;

        private readonly Timer shiftTimer;
        private bool disposed;

        private AutoshiftCirculaQueue() : base(0)
        {

        }

        public AutoshiftCirculaQueue(int limitSize, int shiftInMs) : base(limitSize)
        {
            this.shiftTimer = new Timer();
            this.shiftTimer.Interval = shiftInMs;
            this.shiftTimer.Elapsed += ShiftTimer_Elapsed;
            this.shiftTimer.Start();
        }

        public AutoshiftCirculaQueue(IEnumerable<T> source, int shiftInMs) : base(source)
        {
            this.shiftTimer = new Timer();
            this.shiftTimer.Interval = shiftInMs;
            this.shiftTimer.Elapsed += ShiftTimer_Elapsed;
            this.shiftTimer.Start();
        }

        private void ShiftTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.ShiftLeft();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !this.disposed)
            {
                this.shiftTimer?.Dispose();
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
