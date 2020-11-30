namespace AnimatedTokenMaker.Source
{
    public interface ISourceSetting
    {
        int GetFrameRate();

        int GetMaxTime();

        string GetWorkingDirectory();
    }
}