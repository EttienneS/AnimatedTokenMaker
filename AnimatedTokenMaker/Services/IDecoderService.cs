using AnimatedTokenMaker.Source;
using System.Collections.Generic;

namespace AnimatedTokenMaker.Services
{
    public interface IDecoderService
    {
        IEnumerable<string> GetFramesFromFile(string inputFile, string outputFolder, ISourceSetting _sourceSetting);

        int GetVideoDurationInSeconds(string inputFile);
    }
}
