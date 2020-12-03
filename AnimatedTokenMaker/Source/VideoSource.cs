using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace AnimatedTokenMaker.Source
{
    public class VideoSource : SourceFileBase
    {
        private readonly IFFmpegService _ffmpegService;
        private readonly string _inputFile;
        private readonly string _workingDirectory;
        private Dictionary<int, string> _frames;

        public VideoSource(string inputFile, IFFmpegService ffmpegService)
        {
            _ffmpegService = ffmpegService;
            _workingDirectory = "temp\\" + Guid.NewGuid().ToString();
            _inputFile = inputFile;
            _frames = GetFrames();
        }

        public override void Dispose()
        {
            if (Directory.Exists(_workingDirectory))
            {
                Directory.Delete(_workingDirectory, true);
            }
        }

        public override Bitmap GetFrame(int frame, Size size)
        {
            return GetScaledOffsetFrame(size, GetImageAtFrame(frame));
        }

        public override int GetFrameCount()
        {
            return _frames.Count;
        }

        private Dictionary<int, string> GetFrames()
        {
            var exportedFrames = _ffmpegService.GetFramesFromFile(_inputFile, _workingDirectory);

            var i = 0;
            var frames = new Dictionary<int, string>();
            foreach (var file in exportedFrames)
            {
                frames.Add(i, file);
                i++;
            }
            return frames;
        }

        private Bitmap GetImageAtFrame(int frame)
        {
            using (var image = Image.FromFile(_frames[frame]))
            {
                return new Bitmap(image);
            }
        }
    }
}