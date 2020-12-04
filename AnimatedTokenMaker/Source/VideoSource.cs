using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace AnimatedTokenMaker.Source
{
    public class VideoSource : SourceFileBase
    {
        private readonly int _durationInSeconds;
        private readonly IFFmpegService _ffmpegService;
        private readonly string _inputFile;
        private readonly string _workingDirectory;
        private Dictionary<int, string> _frames;
        private ISourceSetting _sourceSetting;

        public VideoSource(string inputFile, IFFmpegService ffmpegService, ISourceSetting defaultSetting)
        {
            _ffmpegService = ffmpegService;
            _workingDirectory = "temp\\" + Guid.NewGuid().ToString();
            _inputFile = inputFile;
            _durationInSeconds = _ffmpegService.GetVideoDurationInSeconds(_inputFile);
            _sourceSetting = defaultSetting;
            UpdateFrames();
        }

        public override void Dispose()
        {
            ClearWorkingDir();
        }

        private void ClearWorkingDir()
        {
            if (Directory.Exists(_workingDirectory))
            {
                Directory.Delete(_workingDirectory, true);
            }
        }

        public int GetDurationInSeconds()
        {
            return _durationInSeconds;
        }

        public override Bitmap GetFrame(int frame, Size size)
        {
            return GetScaledOffsetFrame(size, GetImageAtFrame(frame));
        }

        public override int GetFrameCount()
        {
            return _frames.Count;
        }

        internal int GetClipLenght()
        {
            return _sourceSetting.GetClipLenght();
        }

        internal void UpdateFrames()
        {
            ClearWorkingDir();

            var exportedFrames = _ffmpegService.GetFramesFromFile(_inputFile, _workingDirectory, _sourceSetting);

            var i = 0;
            var frames = new Dictionary<int, string>();
            foreach (var file in exportedFrames)
            {
                frames.Add(i, file);
                i++;
            }

            _frames = frames;
        }

        internal void UpdateSetting(SourceSetting sourceSetting)
        {
            _sourceSetting = sourceSetting;
        }

        private Bitmap GetImageAtFrame(int frame)
        {
            using (var image = Image.FromFile(_frames[frame]))
            {
                return new Bitmap(image);
            }
        }

        internal ISourceSetting GetSetting()
        {
            return _sourceSetting;
        }
    }
}