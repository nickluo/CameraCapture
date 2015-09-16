using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using MediaFoundation;
using MediaFoundation.Alt;
using MediaFoundation.Misc;
using MediaFoundation.ReadWrite;

namespace VideoModule
{
    internal abstract class CProcess : COMBase, IMFSourceReaderCallback, IDisposable
    {
        #region Definitions

        [DllImport("user32")]
        protected extern static int PostMessage(
            IntPtr handle,
            int msg,
            IntPtr wParam,
            IntPtr lParam
            );

        // ReSharper disable once InconsistentNaming
        protected const int WM_APP = 0x8000;
        // ReSharper disable once InconsistentNaming
        protected const int WM_APP_PREVIEW_ERROR = WM_APP + 2;

        #endregion

        #region Member Variables

        protected IMFSourceReaderAsync PReader;
        protected readonly IntPtr HwndEvent;       // Application window to receive events.
        protected DrawDevice Draw;             // Manages the Direct3D device.
        protected string PwszSymbolicLink;
        protected object LockSync = new object();
        protected IntPtr hVideo;

        #endregion

        // Constructor
        protected CProcess(IntPtr hVideo, IntPtr hEvent)
        {
            this.hVideo = hVideo;
            PReader = null;
            HwndEvent = hEvent;
            PwszSymbolicLink = null;

            var hr = MFExtern.MFStartup(0x20070, MFStartup.Lite);
            MFError.ThrowExceptionForHR(hr);

            Draw = new DrawDevice();
            hr = Draw.CreateDevice(hVideo);

            MFError.ThrowExceptionForHR(hr);
        }

#if DEBUG
        ~CProcess()
        {
            // Was Dispose called?
            Debug.Assert(Draw == null);
        }
#endif

        #region Public Methods

        public static void CloseMediaSession()
        {
            // Shutdown the Media Foundation platform
            var hr = MFExtern.MFShutdown();
            MFError.ThrowExceptionForHR(hr);
        }

        private int OpenMediaSource(IMFMediaSource pSource, ref IMFSourceReaderAsync pReaderAsync)
        {
            // Create an attribute store to hold initialization settings.
            IMFAttributes pAttributes;

            var hr = MFExtern.MFCreateAttributes(out pAttributes, 2);

            //if (Succeeded(hr))
            //{
            //    hr = pAttributes.SetUINT32(MFAttributesClsid.MF_READWRITE_DISABLE_CONVERTERS, 1);
            //}

            if (Succeeded(hr))
            {
                hr = pAttributes.SetUnknown(MFAttributesClsid.MF_SOURCE_READER_ASYNC_CALLBACK, this);
            }

            if (Succeeded(hr))
            {
                IMFSourceReader pReader;
                hr = MFExtern.MFCreateSourceReaderFromMediaSource(pSource, pAttributes, out pReader);
                // ReSharper disable once SuspiciousTypeConversion.Global
                pReaderAsync = (IMFSourceReaderAsync)pReader;
            }

            SafeRelease(pAttributes);

            return hr;
        }

        private int GetOptimizedFormatIndex(ref string format)
        {
            if (PReader == null)
                return -1;
            int index = 0, wMax = 0, rMax = 0;
            for (var i = 0;; i++)
            {
                IMFMediaType pType;
                var hr = PReader.GetNativeMediaType((int) MF_SOURCE_READER.FirstVideoStream, i, out pType);
                if (Failed(hr))
                    break;
                try
                {
                    //hr = TryMediaType(pType);
                    if (Succeeded(hr))
                    {
                        // Found an output type.
                        int rate, den, width, height;
                        MfGetAttributeRatio(pType, MFAttributesClsid.MF_MT_FRAME_RATE, out rate, out den);
                        rate /= den;
                        MfGetAttributeSize(pType, out width, out height);
                        if (width >= wMax && rate >= rMax)
                        {
                            format = $"{width}X{height} @ {rate/den}fps";
                            wMax = width;
                            rMax = rate;
                            index = i;
                        }
                        //formatTable.Add(i, $"{width}X{height}, {rate/den}fps");
                    }
                }
                finally
                {
                    SafeRelease(pType);
                }
            }
            return index;
        }

        private IMFActivate pActivate;

        //-------------------------------------------------------------------
        // SetDevice
        //
        // Set up preview for a specified video capture device.
        //-------------------------------------------------------------------

        public int SetDevice(MfDevice pDevice, ref string format)
        {
            int hr;

            IMFMediaSource pSource = null;
            lock (LockSync)
            {
                try
                {
                    // Release the current device, if any.
                    hr = CloseDevice();
                    pActivate = pDevice.Activator;
                    object o = null;
                    if (Succeeded(hr))
                    {
                        // Create the media source for the device.
                        hr = pActivate.ActivateObject(typeof(IMFMediaSource).GUID, out o);
                    }

                    if (Succeeded(hr))
                    {
                        pSource = (IMFMediaSource)o;
                    }

                    // Get Symbolic device link
                    PwszSymbolicLink = pDevice.SymbolicName;

                    // Create the source reader.
                    if (Succeeded(hr))
                    {
                        hr = OpenMediaSource(pSource, ref PReader);
                    }

                    if (Succeeded(hr))
                    {
                        var index = GetOptimizedFormatIndex(ref format);
                        if (index>=0)
                            hr = ConfigureSourceReader(index);
                    }

                    if (Failed(hr))
                    {
                        
                        pSource?.Shutdown();
                        //pActivate.ShutdownObject();
                        // NOTE: The source reader shuts down the media source
                        // by default, but we might not have gotten that far.
                        CloseDevice();
                    }
                }
                finally
                {
                    SafeRelease(pSource);
                }
            }

            return hr;
        }

        public int ConfigureSourceReader(int mediaIndex)
        {
            if (PReader == null)
                return S_False;

            IMFMediaType pType;

            var hr = PReader.GetNativeMediaType((int)MF_SOURCE_READER.FirstVideoStream, mediaIndex, out pType);
            try
            {
                if (Succeeded(hr))
                    hr = TryMediaType(pType);
            }
            finally
            {
                SafeRelease(pType);
            }
            if (Succeeded(hr))
            {
                // Ask for the first sample.
                if (PReader != null)
                    hr = PReader.ReadSample((int)MF_SOURCE_READER.FirstVideoStream, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            }
            return hr;
        }

        //-------------------------------------------------------------------
        //  CloseDevice
        //
        //  Releases all resources held by this object.
        //-------------------------------------------------------------------

        public virtual int CloseDevice()
        {
            lock (LockSync)
            {
                SafeRelease(PReader);
                PReader = null;
                pActivate?.ShutdownObject();
                pActivate = null;
                PwszSymbolicLink = null;
            }
            return S_Ok;
        }

        //-------------------------------------------------------------------
        //  ResizeVideo
        //  Resizes the video rectangle.
        //
        //  The application should call this method if the size of the video
        //  window changes; e.g., when the application receives WM_SIZE.
        //-------------------------------------------------------------------

        public int ResizeVideo()
        {
            var hr=0;

            lock (LockSync)
            {
                if (Draw!=null)
                    hr = Draw.ResetDevice();
            }

            return hr;
        }

        public bool CheckDeviceLost(string sName)
        {
            return string.Compare(sName, PwszSymbolicLink, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public void Dispose()
        {
            CloseDevice();

            Draw.DestroyDevice();
            Draw = null;

            GC.SuppressFinalize(this);
        }

        private string snapFormat = string.Empty;

        public void SnapShot(string format)
        {
            lock (LockSync)
            {
                snapFormat = format;
            }
        }

        #endregion

        #region Protected Methods

        // NotifyState: Notifies the application when an error occurs.
        protected void NotifyError(int hr)
        {
            TRACE("NotifyError: 0x" + hr.ToString("X"));
            PostMessage(HwndEvent, WM_APP_PREVIEW_ERROR, new IntPtr(hr), IntPtr.Zero);
        }

        //-------------------------------------------------------------------
        // TryMediaType
        //
        // Test a proposed video format.
        //-------------------------------------------------------------------
        protected int TryMediaType(IMFMediaType pType)
        {
            var bFound = false;
            Guid subtype;

            var hr = pType.GetGUID(MFAttributesClsid.MF_MT_SUBTYPE, out subtype);
            if (Failed(hr))
            {
                return hr;
            }

            // Do we support this type directly?
            if (Draw.IsFormatSupported(subtype))
            {
                bFound = true;
            }
            else
            {
                // Can we decode this media type to one of our supported
                // output formats?

                for (var i = 0; ; i++)
                {
                    // Get the i th format.
                    hr = Draw.GetFormat(i, out subtype);
                    if (Failed(hr))
                        break;

                    hr = pType.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, subtype);
                    if (Failed(hr))
                        break;

                    // Try to set this type on the source reader.
                    hr = PReader.SetCurrentMediaType((int)MF_SOURCE_READER.FirstVideoStream, null, pType);
                    if (Succeeded(hr))
                    {
                        bFound = true;
                        break;
                    }
                }
            }

            if (bFound)
            {
                hr = Draw.SetVideoType(pType);
            }

            return hr;
        }

        #endregion

        

        protected abstract int OnFrame(IMFSample pSample, IMFMediaBuffer pBuffer, long llTimestamp, string ssFormat);

        #region IMFSourceReaderCallback Members
        // IMFSourceReaderCallback methods

        //-------------------------------------------------------------------
        // OnReadSample
        //
        // Called when the IMFMediaSource::ReadSample method completes.
        //-------------------------------------------------------------------
        public int OnReadSample(int hrStatus, int dwStreamIndex, MF_SOURCE_READER_FLAG dwStreamFlags, long llTimestamp, IMFSample pSample)
        {
            var hr = hrStatus;
            IMFMediaBuffer pBuffer = null;
            lock (LockSync)
            {
                try
                {
                    if (pSample != null)
                    {
                        // Get the video frame buffer from the sample.
                        if (Succeeded(hr))
                            hr = pSample.GetBufferByIndex(0, out pBuffer);

                        if (Succeeded(hr))
                        {
                            hr = OnFrame(pSample, pBuffer, llTimestamp, snapFormat);
                            snapFormat = string.Empty;
                        }
                    }

                    // Request the next frame.
                    if (Succeeded(hr))
                    {
                        // Read next sample.
                        hr = PReader.ReadSample(
                            (int) MF_SOURCE_READER.FirstVideoStream,
                            0,
                            IntPtr.Zero, // actual
                            IntPtr.Zero, // flags
                            IntPtr.Zero, // time stamp
                            IntPtr.Zero // sample
                            );
                    }

                    if (Failed(hr))
                    {
                        NotifyError(hr);
                    }
                }
                finally
                {
                    SafeRelease(pBuffer);
                    SafeRelease(pSample);
                }
            }

            return hr;
        }

        public int OnEvent(int dwStreamIndex, IMFMediaEvent pEvent)
        {
            return S_Ok;
        }

        public int OnFlush(int dwStreamIndex)
        {
            return S_Ok;
        }

        #endregion

        public static int MfGetAttributeSize(IMFMediaType pType, out int width, out int height)
        {
            return MfGetAttribute2Uint32AsUint64(pType, MFAttributesClsid.MF_MT_FRAME_SIZE, out width, out height);
        }

        public static int MfGetAttributeRatio(IMFMediaType pType, out int numerator, out int denominator)
        {
            return MfGetAttribute2Uint32AsUint64(pType, MFAttributesClsid.MF_MT_PIXEL_ASPECT_RATIO, out numerator, out denominator);
        }

        public static int MfGetAttributeRatio(IMFMediaType pType, Guid guidKey, out int numerator, out int denominator)
        {
            return MfGetAttribute2Uint32AsUint64(pType, guidKey, out numerator, out denominator);
        }

        private static int MfGetAttribute2Uint32AsUint64(IMFMediaType pType, Guid guidKey, out int punHigh32, out int punLow32)
        {
            long attrValue;
            var hr = pType.GetUINT64(guidKey, out attrValue);

            if (Succeeded(hr))
            {
                punLow32 = (int)attrValue;
                punHigh32 = (int)(attrValue >> 32);
            }
            else
            {
                punLow32 = 0;
                punHigh32 = 0;
            }

            return hr;
        }
    }
}
