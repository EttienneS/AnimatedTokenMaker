using AnimatedTokenMaker.Source;
using System.IO;

namespace AnimatedTokenMaker
{
    public class SourceSetting : ISourceSetting
    {
        private readonly int _framerate;
        private readonly int _maxtime;
        private readonly string _workingDirectory;

        public SourceSetting(int framerate, int maxtime, string workingDirectory = "temp")
        {
            _framerate = framerate;
            _maxtime = maxtime;

            _workingDirectory = workingDirectory;
            Directory.CreateDirectory(_workingDirectory);
        }

        public int GetFrameRate()
        {
            return _framerate;
        }

        public int GetMaxTime()
        {
            return _maxtime;
        }

        public string GetWorkingDirectory()
        {
            return _workingDirectory;
        }
    }
}