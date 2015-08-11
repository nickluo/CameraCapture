using System;
using System.Linq;
using System.Threading.Tasks;
using MediaFoundation;
using MediaFoundation.Alt;
using MediaFoundation.Misc;
using MediaFoundation.ReadWrite;
using MediaFoundation.Transform;

namespace VideoModule
{
    internal class CCapture : CProcess, ICapture
    {
        protected IMFSinkWriter PWriter;
        protected bool BFirstSample;
        protected long LlBaseTime;

        public CCapture(IntPtr hVideo, IntPtr hEvent)
            : base(hVideo, hEvent)
        {
            PWriter = null;
            BFirstSample = false;
            LlBaseTime = 0;
        }

        public int StartCapture(string pwszFileName, Guid subType)
        {
            var hr = 0;

            lock (LockSync)
            {
                if (PReader == null)
                    return hr;
                // Create the sink writer 
                if (Succeeded(hr))
                {
                    hr = MFExtern.MFCreateSinkWriterFromURL(pwszFileName, null, null, out PWriter);
                }

                // Set up the encoding parameters.
                if (Succeeded(hr))
                {
                    var param = new EncodingParameters {Subtype = subType};
                    hr = ConfigureCapture(param);
                }

                if (Succeeded(hr))
                {
                    BFirstSample = true;
                    LlBaseTime = 0;
                }
            }

            return hr;
        }

        public int StopCapture()
        {
            var hr = S_Ok;

            lock (LockSync)
            {
                if (PWriter != null)
                {
                    hr = PWriter.Finalize_();
                    SafeRelease(PWriter);
                    PWriter = null;
                }
            }

            return hr;
        }

        public bool IsCapturing()
        {
            bool bIsCapturing;

            lock (LockSync)
            {
                bIsCapturing = (PWriter != null);
            }

            return bIsCapturing;
        }

        private static int ConfigureEncoder(EncodingParameters eparams, IMFMediaType pType, IMFSinkWriter pWriter,
            out int pdwStreamIndex)
        {
            IMFMediaType pType2;

            var hr = MFExtern.MFCreateMediaType(out pType2);

            if (Succeeded(hr))
            {
                hr = pType2.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video);
            }

            if (Succeeded(hr))
            {
                hr = pType2.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, eparams.Subtype);
            }

            if (Succeeded(hr))
            {
                hr = pType2.SetUINT32(MFAttributesClsid.MF_MT_AVG_BITRATE, eparams.Bitrate);
            }

            if (Succeeded(hr))
            {
                hr = CopyAttribute(pType, pType2, MFAttributesClsid.MF_MT_FRAME_SIZE);
            }

            if (Succeeded(hr))
            {
                hr = CopyAttribute(pType, pType2, MFAttributesClsid.MF_MT_FRAME_RATE);
            }

            if (Succeeded(hr))
            {
                hr = CopyAttribute(pType, pType2, MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO);
            }

            if (Succeeded(hr))
            {
                hr = CopyAttribute(pType, pType2, MFAttributesClsid.MF_MT_INTERLACE_MODE);
            }

            pdwStreamIndex = 0;
            if (Succeeded(hr))
            {
                hr = pWriter.AddStream(pType2, out pdwStreamIndex);
            }

            SafeRelease(pType2);

            return hr;
        }

        private int ConfigureCapture(EncodingParameters eparam)
        {
            IMFMediaType pType = null;

            //var hr = ConfigureSourceReader(PReader);

            var hr = PReader.GetCurrentMediaType((int)MF_SOURCE_READER.FirstVideoStream, out pType);
            int w, h;
            MfGetAttributeSize(pType, out w, out h);
            eparam.Bitrate = w*h*20;

            var sinkStream = 0;
            if (Succeeded(hr))
            {
                hr = ConfigureEncoder(eparam, pType, PWriter, out sinkStream);
            }

            if (Succeeded(hr))
            {
                // Register the color converter DSP for this process, in the video 
                // processor category. This will enable the sink writer to enumerate
                // the color converter when the sink writer attempts to match the
                // media types.

                hr = MFExtern.MFTRegisterLocalByCLSID(
                    typeof(CColorConvertDMO).GUID,
                    MFTransformCategory.MFT_CATEGORY_VIDEO_PROCESSOR,
                    "",
                    MFT_EnumFlag.SyncMFT,
                    0,
                    null,
                    0,
                    null
                    );
            }

            if (Succeeded(hr))
            {
                hr = PWriter.SetInputMediaType(sinkStream, pType, null);
            }

            if (Succeeded(hr))
            {
                hr = PWriter.BeginWriting();
            }

            SafeRelease(pType);

            return hr;
        }

        private static int CopyAttribute(IMFAttributes pSrc, IMFAttributes pDest, Guid key)
        {
            var variant = new PropVariant();

            var hr = pSrc.GetItem(key, variant);
            if (Succeeded(hr))
                hr = pDest.SetItem(key, variant);
            return hr;
        }

        public override int CloseDevice()
        {
            StopCapture();
            return base.CloseDevice();
        }

        protected override int OnFrame(IMFSample pSample, IMFMediaBuffer pBuffer, long llTimestamp, string ssFormat)
        {
            int hr;
            if (IsCapturing())
            {
                if (BFirstSample)
                {
                    LlBaseTime = llTimestamp;
                    BFirstSample = false;
                }

                // re-base the time stamp
                llTimestamp -= LlBaseTime;

                hr = pSample.SetSampleTime(llTimestamp);

                //if (Succeeded(hr))
                //{
                //    var displayTask = Task<int>.Factory.StartNew(() => Draw.DrawFrame(pBuffer));
                //    var recordTask = Task<int>.Factory.StartNew(() => PWriter.WriteSample(0, pSample));
                //    Task.WaitAll(displayTask, recordTask);
                //    hr = displayTask.Result;
                //    if (Succeeded(hr))
                //        hr = recordTask.Result;
                //}
                //Parallel.Invoke(()=>Draw.DrawFrame(pBuffer),()=> PWriter.WriteSample(0, pSample));
                if (Succeeded(hr))
                    hr = Draw.DrawFrame(pBuffer, !string.IsNullOrEmpty(ssFormat), ssFormat);
                if (Succeeded(hr))
                    hr = PWriter.WriteSample(0, pSample);
            }
            else
                hr = Draw.DrawFrame(pBuffer, !string.IsNullOrEmpty(ssFormat), ssFormat);
            return hr;
        }
    }
}
