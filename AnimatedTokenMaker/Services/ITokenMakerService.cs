namespace AnimatedTokenMaker.Services
{
    public interface ITokenMakerService
    {
        bool IsReady();

        string Message { get; }
    }
}
