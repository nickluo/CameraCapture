using System;
using MediaFoundation;

namespace VideoModule
{
    class CPreview : CProcess
    {
        public CPreview(IntPtr hVideo, IntPtr hEvent)
            :base(hVideo,hEvent)
        { }

        protected override int OnFrame(IMFSample pSample, IMFMediaBuffer pBuffer, long llTimestamp, string ssFormat)
            => Draw.DrawFrame(pBuffer, !string.IsNullOrEmpty(ssFormat), ssFormat);
    }

}