using AnimatedTokenMaker.Source;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AnimatedTokenMaker.Services
{
    public sealed class FFmpegService : IFFmpegService
    {
        private string _capturedOutput;

        public string Message
        {
            get
            {
                return "This application needs FFmpeg.exe to work.\n\nPlease download the exe from http://bit.ly/dl-ffmpeg and put it in the same folder as this application.";
            }
        }

        public delegate void FFmpegMessageWritten(string message);

        public event FFmpegMessageWritten OnFFmpegMessageWritten;

        public void EncodeFolderAsWebm(string outputFile, string sourceFolder, ISourceSetting sourceSetting)
        {
            InvokeFFmpeg($"-framerate {sourceSetting.GetFrameRate()} -f image2 -i \"{sourceFolder}\" -c:v libvpx-vp9 -pix_fmt yuva420p \"{outputFile}\"");
        }

        public IEnumerable<string> GetFramesFromFile(string inputFile, string outputFolder, ISourceSetting sourceSetting)
        {
            Directory.CreateDirectory(outputFolder);

            var lenght = EnsureLenghtIsValid(inputFile, sourceSetting);

            InvokeFFmpeg($"-i \"{inputFile}\" -ss {TimeSpan.FromSeconds(sourceSetting.GetStartTime())} -t {lenght} -r {sourceSetting.GetFrameRate()} \"{outputFolder}\\w%04d.bmp\"");

            return Directory.EnumerateFiles(outputFolder, "*.bmp");
        }

        public int GetVideoDurationInSeconds(string inputFile)
        {
            var output = InvokeFFmpeg($"-i \"{inputFile}\" -f null -");

            var totalTime = -1;
            foreach (var line in output.Split('\n'))
            {
                if (line.Contains(" time="))
                {
                    var time = line.Split(' ').FirstOrDefault(p => p.StartsWith("time="));

                    if (TimeSpan.TryParse(time.Split('=').Last(), out TimeSpan span))
                    {
                        totalTime = (int)span.TotalSeconds;
                    }
                }
            }

            if (totalTime >= 0)
            {
                return totalTime;
            }

            throw new KeyNotFoundException("Unable to determine time!");
        }

        private int EnsureLenghtIsValid(string inputFile, ISourceSetting sourceSetting)
        {
            var lenght = GetVideoDurationInSeconds(inputFile);
            if (sourceSetting.GetClipLenght() <= lenght)
            {
                lenght = sourceSetting.GetClipLenght();
            }

            return lenght;
        }

        private string InvokeFFmpeg(string args)
        {
            var info = new ProcessStartInfo("ffmpeg")
            {
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            using (var process = new Process { StartInfo = info })
            {
                _capturedOutput = "";
                process.Start();

                ParseOutput(process);

                if (process.ExitCode != 0)
                {
                    throw new System.Exception("FFmpeg error!");
                }

                return _capturedOutput;
            };
        }

        private void ParseOutput(Process process)
        {
            // ffmpeg seems to write to the StandardError stream rather than StandardOutput for some reason
            // https://stackoverflow.com/questions/51300900/how-to-get-output-from-ffmpeg-process-in-c-sharp/51302403
            string outputLine;
            while ((outputLine = process.StandardError.ReadLine()) != null)
            {
                OnFFmpegMessageWritten?.Invoke(outputLine);
                Debug.WriteLine(outputLine);
                _capturedOutput += outputLine + "\n";
            }
        }

        public bool IsReady()
        {
            return File.Exists("ffmpeg.exe");
        }
    }
}