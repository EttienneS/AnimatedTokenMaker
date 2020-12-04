using AnimatedTokenMaker.Source;
using System.Collections.Generic;

namespace AnimatedTokenMaker
{
    public interface IFFmpegService
    {
        void EncodeFolderAsWebm(string outputFile, string sourceFolder, ISourceSetting _sourceSetting);

        IEnumerable<string> GetFramesFromFile(string inputFile, string outputFolder, ISourceSetting _sourceSetting);

        int GetVideoDurationInSeconds(string inputFile);
    }
}