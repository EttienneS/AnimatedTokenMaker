using AnimatedTokenMaker.Source;
using System.IO;

namespace AnimatedTokenMaker
{
    public class SourceSetting : ISourceSetting
    {
        private readonly int _startTime;
        private readonly int _framerate;
        private readonly int _clipLenght;
        private readonly string _workingDirectory;

        public SourceSetting(int startTime, int framerate, int clipLenght, string workingDirectory = "temp")
        {
            _startTime = startTime;
            _framerate = framerate;
            _clipLenght = clipLenght;

            _workingDirectory = workingDirectory;
            Directory.CreateDirectory(_workingDirectory);
        }

        public int GetFrameRate()
        {
            return _framerate;
        }

        public int GetClipLenght()
        {
            return _clipLenght;
        }

        public string GetWorkingDirectory()
        {
            return _workingDirectory;
        }

        public int GetStartTime()
        {
            return _startTime;
        }
    }
}