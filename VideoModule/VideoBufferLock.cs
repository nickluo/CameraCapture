using System;
using System.Diagnostics;
using MediaFoundation;
using MediaFoundation.Misc;

namespace VideoModule
{
    //-------------------------------------------------------------------
    //  VideoBufferLock class
    //
    //  Locks a video buffer that might or might not support IMF2DBuffer.
    //
    //-------------------------------------------------------------------
    class VideoBufferLock : COMBase, IDisposable
    {
        private IMFMediaBuffer pBuffer;
        private IMF2DBuffer p2DBuffer;

        private bool bLocked;

        // Constructor
        public VideoBufferLock(IMFMediaBuffer pBuffer)
        {
            p2DBuffer = null;
            bLocked = false;
            this.pBuffer = pBuffer;

            // Query for the 2-D buffer interface. OK if this fails.
            // ReSharper disable once SuspiciousTypeConversion.Global
            p2DBuffer = pBuffer as IMF2DBuffer;
        }

#if DEBUG
        ~VideoBufferLock()
        {
            // Was Dispose called?
            Debug.Assert(pBuffer == null && p2DBuffer == null);
        }
#endif

        //-------------------------------------------------------------------
        // LockBuffer
        //
        // Locks the buffer. Returns a pointer to scan line 0 and returns the stride.
        //
        // The caller must provide the default stride as an input parameter, in case
        // the buffer does not expose IMF2DBuffer. You can calculate the default stride
        // from the media type.
        //-------------------------------------------------------------------
        public int LockBuffer(
            int lDefaultStride,    // Minimum stride (with no padding).
            int dwHeightInPixels,  // Height of the image, in pixels.
            out IntPtr ppbScanLine0,    // Receives a pointer to the start of scan line 0.
            out int plStride          // Receives the actual stride.
            )
        {
            int hr;
            ppbScanLine0 = IntPtr.Zero;
            plStride = 0;

            // Use the 2-D version if available.
            if (p2DBuffer != null)
            {
                hr = p2DBuffer.Lock2D(out ppbScanLine0, out plStride);
            }
            else
            {
                // Use non-2D version.
                IntPtr pData;
                int pcbMaxLength;
                int pcbCurrentLength;


                hr = pBuffer.Lock(out pData, out pcbMaxLength, out pcbCurrentLength);
                if (Succeeded(hr))
                {
                    plStride = lDefaultStride;
                    if (lDefaultStride < 0)
                    {
                        // Bottom-up orientation. Return a pointer to the start of the
                        // last row *in memory* which is the top row of the image.
                        ppbScanLine0 += lDefaultStride * (dwHeightInPixels - 1);
                    }
                    else
                    {
                        // Top-down orientation. Return a pointer to the start of the
                        // buffer.
                        ppbScanLine0 = pData;
                    }
                }
            }

            bLocked = (Succeeded(hr));

            return hr;
        }

        //-------------------------------------------------------------------
        // UnlockBuffer
        //
        // Unlocks the buffer. Called automatically by the destructor.
        //-------------------------------------------------------------------

        public void UnlockBuffer()
        {
            if (bLocked)
            {
                if (p2DBuffer != null)
                {
                    p2DBuffer.Unlock2D();
                }
                else
                {
                    pBuffer.Unlock();
                }

                bLocked = false;
            }
        }

        public void Dispose()
        {
            UnlockBuffer();
            SafeRelease(pBuffer);
            SafeRelease(p2DBuffer);

            pBuffer = null;
            p2DBuffer = null;

            GC.SuppressFinalize(this);
        }
    }
}
