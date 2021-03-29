using AnimatedTokenMaker.Services;

namespace AnimatedTokenMaker.Services
{
    public interface IFFmpegService : IDecoderService, IEncoderService, ITokenMakerService
    {
    }
}