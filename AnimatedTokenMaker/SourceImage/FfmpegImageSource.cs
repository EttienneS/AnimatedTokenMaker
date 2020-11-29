using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace AnimatedTokenMaker
{
    public class FfmpegImageSource : ISourceFile
    {
        private readonly IISourceSetting _setting;
        private readonly string _inputFile;
        private readonly string _workingDirectory;
        private Dictionary<int, string> _frames;

        public FfmpegImageSource(string inputFile, IISourceSetting setting)
        {
            _workingDirectory = "temp\\" + Guid.NewGuid().ToString();
            _setting = setting;
            _inputFile = inputFile;
            _frames = GetFrames();
        }

        private Dictionary<int, string> GetFrames()
        {
            Directory.CreateDirectory(_workingDirectory);
            var info = new ProcessStartInfo("ffmpeg")
            {
                Arguments = $"-i \"{_inputFile}\" -t {_setting.GetMaxTime()} -r {_setting.GetFrameRate()} \"{ _workingDirectory}\\w%04d.bmp\""
            };
            var proc = new Process()
            {
                StartInfo = info
            };
            proc.Start();
            proc.WaitForExit();

            var i = 0;
            var frames = new Dictionary<int, string>();
            foreach (var file in Directory.EnumerateFiles(_workingDirectory, "*.bmp"))
            {
                frames.Add(i, file);
                i++;
            }
            return frames;
        }

        public Bitmap GetScaledFrame(int frame, float scale)
        {
            var sourceImage = GetImageAtFrame(frame);
            return new Bitmap(sourceImage, Math.Max(24, (int)(sourceImage.Width * scale)), Math.Max(24, (int)(sourceImage.Height * scale)));
        }

        public int GetFrameCount()
        {
            return _frames.Count;
        }

        private Bitmap GetImageAtFrame(int frame)
        {
            return (Bitmap)Image.FromFile(_frames[frame]);
        }

        public void Dispose()
        {
            Directory.Delete(_workingDirectory, true);
        }
    }
}