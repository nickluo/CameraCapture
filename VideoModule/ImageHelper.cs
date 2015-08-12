using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VideoModule
{
    internal class ImageHelper
    {
        private static readonly Assembly PresentationCore = Assembly.GetAssembly(typeof(BitmapEncoder));

        public static void SnapShot(IntPtr sourcePtr, int pitch, int width, int height, string format)
        {
            var sync = new AutoResetEvent(false);
            Task.Factory.StartNew(() =>
            {
                var source = BitmapSource.Create(width, height, 72, 72, PixelFormats.Bgr32, null,
                    sourcePtr, pitch * height, pitch);
                sync.Set();
                BitmapEncoder encoder;
                var encoderType = PresentationCore.GetType(
                    $"System.Windows.Media.Imaging.{format}BitmapEncoder"
                    , false, true);
                if (encoderType == null)
                    encoder = new JpegBitmapEncoder();
                else
                {
                    encoder = Activator.CreateInstance(encoderType) as BitmapEncoder;
                    if (encoder == null)
                        return;
                }
                var frame = BitmapFrame.Create(source);
                encoder.Frames.Add(frame);
                using (
                    var file =
                        File.Create(FileHelper.SavePath + "Snapshot " + DateTime.Now.ToString("yyyyMMddTHHmmss.") +
                                    format?.ToLower()))
                {
                    encoder.Save(file);
                    file.Close();
                }
            });
            sync.WaitOne();
        }
    }
}
