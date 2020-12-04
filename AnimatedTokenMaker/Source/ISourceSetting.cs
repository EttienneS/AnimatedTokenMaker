namespace AnimatedTokenMaker.Source
{
    public interface ISourceSetting
    {
        int GetFrameRate();

        int GetClipLenght();

        int GetStartTime();

        string GetWorkingDirectory();
    }
}