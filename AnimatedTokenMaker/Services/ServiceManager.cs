using System.Windows;

namespace AnimatedTokenMaker.Services
{
    public class ServiceManager
    {
        private ServiceManager()
        {
            WebmuxService = new WebmuxService();
            FFmpegService = new FFmpegService();
        }

        private static ServiceManager _instance;

        public static ServiceManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServiceManager();
                }

                return _instance;
            }
        }

        public WebmuxService WebmuxService { get; }
        public FFmpegService FFmpegService { get; }

        internal bool Ready()
        {
            if (!FFmpegService.IsReady())
            {
                MessageBox.Show(FFmpegService.Message, "FFmpeg not found!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }
    }
}