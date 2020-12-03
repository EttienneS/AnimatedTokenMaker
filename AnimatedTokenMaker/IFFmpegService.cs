using System.Collections.Generic;

namespace AnimatedTokenMaker
{
    public interface IFFmpegService
    {
        void EncodeFolderAsWebm(string outputFile, string sourceFolder);
        IEnumerable<string> GetFramesFromFile(string inputFile, string outputFolder);
    }
}