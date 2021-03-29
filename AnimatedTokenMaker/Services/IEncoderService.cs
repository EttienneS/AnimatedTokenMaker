using AnimatedTokenMaker.Source;

namespace AnimatedTokenMaker.Services
{
    public interface IEncoderService
    {
        void EncodeFolderAsWebm(string outputFile, string sourceFolder, ISourceSetting _sourceSetting);
    }
}