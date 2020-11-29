namespace AnimatedTokenMaker
{
    public class SourceSetting : IISourceSetting
    {
        private int _framerate;
        private int _maxtime;

        public SourceSetting(int framerate, int maxtime)
        {
            _framerate = framerate;
            _maxtime = maxtime;
        }

        public int GetFrameRate()
        {
            return _framerate;
        }

        public int GetMaxTime()
        {
            return _maxtime;
        }
    }
}