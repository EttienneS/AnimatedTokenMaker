using AnimatedTokenMaker.Source;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AnimatedTokenMaker.Services
{
    public sealed class WebmuxService : IWebpmuxService
    {
        private string _capturedOutput;

        public string Message
        {
            get
            {
                return "To use .webp sources you need webpmux.exe and dwebp.exe.\n\nPlease download the exe's from http://bit.ly/webp-dl and put it in the same folder as this application.";
            }
        }

        public bool IsReady()
        {
            return File.Exists("webpmux.exe") && File.Exists("dwebp.exe");
        }

        public IEnumerable<string> GetFramesFromFile(string inputFile, string outputFolder, ISourceSetting sourceSetting)
        {
            Directory.CreateDirectory(outputFolder);

            var frames = GetFrameTimings(inputFile);
            for (int i = 0; i < frames.Count; i++)
            {
                var tempWebp = $"{outputFolder}\\w{i}.webp";
                InvokeWebpmux($"-get frame {i} \"{inputFile}\" -o \"{tempWebp}\"");
                InvokeDwebp($"{tempWebp} -o \"{outputFolder}\\w{i}.bmp\"");
                File.Delete(tempWebp);
            }

            return Directory.EnumerateFiles(outputFolder, "*.bmp");
        }

        public int GetVideoDurationInSeconds(string inputFile)
        {
            var totalInMiliseconds = 0;

            foreach (var timing in GetFrameTimings(inputFile))
            {
                totalInMiliseconds += timing.Value;
            }

            if (totalInMiliseconds >= 0)
            {
                return totalInMiliseconds / 1000;
            }

            throw new KeyNotFoundException("Unable to determine time!");
        }

        internal Dictionary<int, int> GetFrameTimings(string inputFile)
        {
            var timings = new Dictionary<int, int>();
            var output = InvokeWebpmux($"-info \"{inputFile}\"");

            var read = false;
            var i = 0;
            foreach (var line in output.Split('\n'))
            {
                if (line.Contains("No.:"))
                {
                    read = true;
                }
                else if (read)
                {
                    // parse output in this format
                    // No.: width height alpha x_offset y_offset duration   dispose blend image_size
                    // 1:   640   360    no        0        0      100       none no        486
                    var duration = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (duration.Length > 5)
                    {
                        timings.Add(i, int.Parse(duration[6]));
                        i++;
                    }
                }
            }

            return timings;
        }

        private string InvokeDwebp(string args)
        {
            return InvokeTool("dwebp", args);
        }

        private string InvokeTool(string tool, string args)
        {
            var info = new ProcessStartInfo(tool)
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
                    throw new Exception($"{tool} error!");
                }

                return _capturedOutput;
            };
        }

        private string InvokeWebpmux(string args)
        {
            return InvokeTool("webpmux", args);
        }

        private void ParseOutput(Process process)
        {
            string outputLine;
            while ((outputLine = process.StandardOutput.ReadLine()) != null)
            {
                Debug.WriteLine(outputLine);
                _capturedOutput += outputLine + "\n";
            }
        }
    }
}