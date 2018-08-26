using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using System;

namespace Henzai
{
    public class FrameTimer : IDisposable
    {
        private System.Timers.Timer _frameTimer;
        private float _ticksInMilliseconds = 0;
        private float _prevFrameTicksInMilliseconds;
        public float prevFrameTicksInMilliseconds => _prevFrameTicksInMilliseconds;
        public float prevFrameTicksInSeconds => _prevFrameTicksInMilliseconds/1000.0f;
        public CancellationTokenSource FrameTimerCancellationTokenSource;

        public FrameTimer(double interval){
            FrameTimerCancellationTokenSource = new CancellationTokenSource();
            _frameTimer = new System.Timers.Timer(interval);
            
            // https://cpratt.co/async-tips-tricks/
            _frameTimer.Elapsed += Increment;
        }

        private async void Increment(object sender, ElapsedEventArgs e){
            if(FrameTimerCancellationTokenSource.IsCancellationRequested)
                return;
            await Task.Run(() => _ticksInMilliseconds++,FrameTimerCancellationTokenSource.Token).ConfigureAwait(false);
        }

        public void Start(){
            if(FrameTimerCancellationTokenSource.IsCancellationRequested)
                return;
            _ticksInMilliseconds = 0f;
            _frameTimer.Start();
        }

        public void Stop(){
            if(FrameTimerCancellationTokenSource.IsCancellationRequested)
                return;
            _frameTimer.Stop();
            _prevFrameTicksInMilliseconds = _ticksInMilliseconds;
        }

        public void Cancel(){
            FrameTimerCancellationTokenSource.Cancel();
            _frameTimer.Close();
        }

        public float Query(){
            return _ticksInMilliseconds;
        }

        public void Dispose(){
            _frameTimer.Elapsed -= Increment;
            _frameTimer.Dispose();
            FrameTimerCancellationTokenSource.Dispose();
        }
    }
} 