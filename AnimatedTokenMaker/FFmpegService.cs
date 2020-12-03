using AnimatedTokenMaker.Source;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AnimatedTokenMaker
{
    public class FFmpegService : IFFmpegService
    {
        private ISourceSetting _sourceSetting;

        public FFmpegService(ISourceSetting sourceSetting)
        {
            _sourceSetting = sourceSetting;
        }

        public delegate void FFmpegMessageWritten(string message);

        public event FFmpegMessageWritten OnFFmpegMessageWritten;

        public void EncodeFolderAsWebm(string outputFile, string sourceFolder)
        {
            InvokeFFmpeg($"-framerate {_sourceSetting.GetFrameRate()} -f image2 -i \"{sourceFolder}\" -c:v libvpx-vp9 -pix_fmt yuva420p \"{outputFile}\"");
        }

        public IEnumerable<string> GetFramesFromFile(string inputFile, string outputFolder)
        {
            Directory.CreateDirectory(outputFolder);
            InvokeFFmpeg($"-i \"{inputFile}\" -t {_sourceSetting.GetMaxTime()} -r {_sourceSetting.GetFrameRate()} \"{outputFolder}\\w%04d.bmp\"");

            return Directory.EnumerateFiles(outputFolder, "*.bmp");
        }

        private void InvokeFFmpeg(string args)
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
                process.Start();

                ParseOutput(process);

                if (process.ExitCode != 0)
                {
                    throw new System.Exception("FFmpeg error!");
                }
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
            }
        }
    }
}