using System;

namespace VideoModule
{
    public class EncodingParameters
    {
        public Guid Subtype { get; set; }
        public int Bitrate { get; set; }
    }

    internal interface ICapture
    {
        int StartCapture(string pwszFileName, Guid subType);
        int StopCapture();
        bool IsCapturing();
    }
}
