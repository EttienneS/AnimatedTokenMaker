using System.Diagnostics;
using System.IO;

namespace AnimatedTokenMaker
{
    public class VideoExporter : IVideoExporter
    {
        public string GenerateVideoFromFolder(string output, string pattern = "t%04d.png")
        {
            var outputFile = Path.Combine(output, "output.webm");
            var info = new ProcessStartInfo("ffmpeg")
            {
                Arguments = $"-framerate 25 -f image2 -i \"{output}\\{pattern}\" -c:v libvpx-vp9 -pix_fmt yuva420p \"{outputFile}\""
            };
            var proc = new Process()
            {
                StartInfo = info
            };
            proc.Start();
            proc.WaitForExit();

            
            return outputFile;
        }
    }
}