using System.Threading.Tasks;
using System.Timers;

namespace Henzai
{
    public class FrameTimer
    {
        private Timer _frameTimer;
        private float _ticksInMilliseconds;
        private float _prevFrameTicksInMilliseconds;
        public float prevFrameTicksInMilliseconds => _prevFrameTicksInMilliseconds;
        public float prevFrameTicksInSeconds => _prevFrameTicksInMilliseconds/1000.0f;

        public FrameTimer(double interval){
            _frameTimer = new Timer(interval);
            // https://cpratt.co/async-tips-tricks/
            _frameTimer.Elapsed += async (sender,e) => await Task.Run(() => _ticksInMilliseconds++).ConfigureAwait(false);
        }

        public void Start(){
            _ticksInMilliseconds = 0f;
            _frameTimer.Start();
        }

        public void Stop(){
            _frameTimer.Stop();
            _prevFrameTicksInMilliseconds = _ticksInMilliseconds;
        }

        public float Query(){
            return _ticksInMilliseconds;
        }
    }
} 