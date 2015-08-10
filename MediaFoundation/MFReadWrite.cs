/* license

MFReadWrite.cs - Part of MediaFoundationLib, which provide access to MediaFoundation interfaces via .NET

Copyright (C) 2015, by the Administrators of the Media Foundation .NET SourceForge Project
http://mfnet.sourceforge.net

This is free software; you can redistribute it and/or modify it under the terms of either:

a) The Lesser General Public License version 2.1 (see license.txt)
b) The BSD License (see BSDL.txt)

*/

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;

using MediaFoundation.Misc;
using System.Drawing;

using MediaFoundation.EVR;
using MediaFoundation.Transform;

namespace MediaFoundation.ReadWrite
{
    #region COM Class Objects

    [UnmanagedName("CLSID_MFReadWriteClassFactory"),
    ComImport,
    Guid("48e2ed0f-98c2-4a37-bed5-166312ddd83f")]
    public class MFReadWriteClassFactory
    {
    }

    #endregion

    #region Declarations

    [Flags, UnmanagedName("MF_SOURCE_READER_FLAG")]
    public enum MF_SOURCE_READER_FLAG
    {
        None = 0,
        Error = 0x00000001,
        EndOfStream = 0x00000002,
        NewStream = 0x00000004,
        NativeMediaTypeChanged = 0x00000010,
        CurrentMediaTypeChanged = 0x00000020,
        AllEffectsRemoved = 0x00000200,
        StreamTick = 0x00000100
    }

    [UnmanagedName("Unnamed enum")]
    public enum MF_SOURCE_READER
    {
        InvalidStreamIndex = unchecked((int)0xFFFFFFFF),
        AllStreams = unchecked((int)0xFFFFFFFE),
        AnyStream = unchecked((int)0xFFFFFFFE),
        FirstAudioStream = unchecked((int)0xFFFFFFFD),
        FirstVideoStream = unchecked((int)0xFFFFFFFC),
        FirstSourcePhotoStream = unchecked((int)0xFFFFFFFB),
        PreferredSourceVideoStreamForPreview = unchecked((int)0xFFFFFFFA),
        PreferredSourceVideoStreamForRecord = unchecked((int)0xFFFFFFF9),
        FirstSourceIndependentPhotoStream = unchecked((int)0xFFFFFFF8),
        PreferredSourceStreamForVideoRecord = unchecked((int)0xFFFFFFF9),
        PreferredSourceStreamForPhoto = unchecked((int)0xFFFFFFF8),
        PreferredSourceStreamForAudio = unchecked((int)0xFFFFFFF7),
        MediaSource = unchecked((int)0xFFFFFFFF),
    }

    [UnmanagedName("MF_SOURCE_READER_CONTROL_FLAG")]
    public enum MF_SOURCE_READER_CONTROL_FLAG
    {
        None = 0,
        Drain = 0x00000001
    }

    [UnmanagedName("Unnamed enum")]
    public enum MF_SINK_WRITER
    {
        InvalidStreamIndex = unchecked((int)0xFFFFFFFF),
        AllStreams = unchecked((int)0xFFFFFFFE),
        MediaSink = unchecked((int)0xFFFFFFFF)
    }

    [StructLayout(LayoutKind.Sequential), UnmanagedName("MF_SINK_WRITER_STATISTICS")]
    public struct MF_SINK_WRITER_STATISTICS
    {
        public int cb;

        public long llLastTimestampReceived;
        public long llLastTimestampEncoded;
        public long llLastTimestampProcessed;
        public long llLastStreamTickReceived;
        public long llLastSinkSampleRequest;

        public long qwNumSamplesReceived;
        public long qwNumSamplesEncoded;
        public long qwNumSamplesProcessed;
        public long qwNumStreamTicksReceived;

        public int dwByteCountQueued;
        public long qwByteCountProcessed;

        public int dwNumOutstandingSinkSampleRequests;

        public int dwAverageSampleRateReceived;
        public int dwAverageSampleRateEncoded;
        public int dwAverageSampleRateProcessed;
    }

    #endregion

    #region Interfaces

#if ALLOW_UNTESTED_INTERFACES

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("17C3779E-3CDE-4EDE-8C60-3899F5F53AD6")]
    public interface IMFSinkWriterEncoderConfig
    {
        [PreserveSig]
        int SetTargetMediaType(
            int dwStreamIndex,
            IMFMediaType pTargetMediaType,
            IMFAttributes pEncodingParameters
            );

        [PreserveSig]
        int PlaceEncodingParameters(
            int dwStreamIndex,
            IMFAttributes pEncodingParameters
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("CF839FE6-8C2A-4DD2-B6EA-C22D6961AF05")]
    public interface IMFSourceReaderCallback2 : IMFSourceReaderCallback
    {
        #region IMFSourceReaderCallback

        [PreserveSig]
        new int OnReadSample(
            int hrStatus,
            int dwStreamIndex,
            MF_SOURCE_READER_FLAG dwStreamFlags,
            long llTimestamp,
            IMFSample pSample
        );

        [PreserveSig]
        new int OnFlush(
            int dwStreamIndex
        );

        [PreserveSig]
        new int OnEvent(
            int dwStreamIndex,
            IMFMediaEvent pEvent
        );

        #endregion

        [PreserveSig]
        int OnTransformChange();

        [PreserveSig]
        int OnStreamError(
            int dwStreamIndex,
            int hrStatus);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("2456BD58-C067-4513-84FE-8D0C88FFDC61")]
    public interface IMFSinkWriterCallback2 : IMFSinkWriterCallback
    {

        #region IMFSinkWriterCallback

        [PreserveSig]
        new int OnFinalize(
            int hrStatus
        );

        [PreserveSig]
        new int OnMarker(
            int dwStreamIndex,
            IntPtr pvContext
        );

        #endregion

        [PreserveSig]
        int OnTransformChange();

        [PreserveSig]
        int OnStreamError(
            int dwStreamIndex,
            int hrStatus);

    }

#endif

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("3137f1cd-fe5e-4805-a5d8-fb477448cb3d")]
    public interface IMFSinkWriter
    {
        [PreserveSig]
        int AddStream(
            IMFMediaType pTargetMediaType,
            out int pdwStreamIndex
        );

        [PreserveSig]
        int SetInputMediaType(
            int dwStreamIndex,
            IMFMediaType pInputMediaType,
            IMFAttributes pEncodingParameters
        );

        [PreserveSig]
        int BeginWriting();

        [PreserveSig]
        int WriteSample(
            int dwStreamIndex,
            IMFSample pSample
        );

        [PreserveSig]
        int SendStreamTick(
            int dwStreamIndex,
            long llTimestamp
        );

        [PreserveSig]
        int PlaceMarker(
            int dwStreamIndex,
            IntPtr pvContext
        );

        [PreserveSig]
        int NotifyEndOfSegment(
            int dwStreamIndex
        );

        [PreserveSig]
        int Flush(
            int dwStreamIndex
        );

        [PreserveSig]
        int Finalize_();

        [PreserveSig]
        int GetServiceForStream(
            int dwStreamIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidService,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
        );

        [PreserveSig]
        int GetStatistics(
            int dwStreamIndex,
            out MF_SINK_WRITER_STATISTICS pStats
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("666f76de-33d2-41b9-a458-29ed0a972c58")]
    public interface IMFSinkWriterCallback
    {
        [PreserveSig]
        int OnFinalize(
            int hrStatus
        );

        [PreserveSig]
        int OnMarker(
            int dwStreamIndex,
            IntPtr pvContext
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("E7FE2E12-661C-40DA-92F9-4F002AB67627")]
    public interface IMFReadWriteClassFactory
    {
        [PreserveSig]
        int CreateInstanceFromURL(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
            IMFAttributes pAttributes,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
        );

        [PreserveSig]
        int CreateInstanceFromObject(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
            [MarshalAs(UnmanagedType.IUnknown)] object punkObject,
            IMFAttributes pAttributes,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("70ae66f2-c809-4e4f-8915-bdcb406b7993")]
    public interface IMFSourceReader
    {
        [PreserveSig]
        int GetStreamSelection(
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.Bool)] out bool pfSelected
        );

        [PreserveSig]
        int SetStreamSelection(
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.Bool)] bool fSelected
        );

        [PreserveSig]
        int GetNativeMediaType(
            int dwStreamIndex,
            int dwMediaTypeIndex,
            out IMFMediaType ppMediaType
        );

        [PreserveSig]
        int GetCurrentMediaType(
            int dwStreamIndex,
            out IMFMediaType ppMediaType
        );

        [PreserveSig]
        int SetCurrentMediaType(
            int dwStreamIndex,
            [In, Out] MFInt pdwReserved,
            IMFMediaType pMediaType
        );

        [PreserveSig]
        int SetCurrentPosition(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidTimeFormat,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant varPosition
        );

        [PreserveSig]
        int ReadSample(
            int dwStreamIndex,
            MF_SOURCE_READER_CONTROL_FLAG dwControlFlags,
            out int pdwActualStreamIndex,
            out  MF_SOURCE_READER_FLAG pdwStreamFlags,
            out long pllTimestamp,
            out IMFSample ppSample
        );

        [PreserveSig]
        int Flush(
            int dwStreamIndex
        );

        [PreserveSig]
        int GetServiceForStream(
            int dwStreamIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidService,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
        );

        [PreserveSig]
        int GetPresentationAttribute(
            int dwStreamIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidAttribute,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvarAttribute
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("deec8d99-fa1d-4d82-84c2-2c8969944867")]
    public interface IMFSourceReaderCallback
    {
        [PreserveSig]
        int OnReadSample(
            int hrStatus,
            int dwStreamIndex,
            MF_SOURCE_READER_FLAG dwStreamFlags,
            long llTimestamp,
            IMFSample pSample
        );

        [PreserveSig]
        int OnFlush(
            int dwStreamIndex
        );

        [PreserveSig]
        int OnEvent(
            int dwStreamIndex,
            IMFMediaEvent pEvent
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("7b981cf0-560e-4116-9875-b099895f23d7")]
    public interface IMFSourceReaderEx : IMFSourceReader
    {
        #region IMFSourceReader Methods

        [PreserveSig]
        new int GetStreamSelection(
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.Bool)] out bool pfSelected
        );

        [PreserveSig]
        new int SetStreamSelection(
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.Bool)] bool fSelected
        );

        [PreserveSig]
        new int GetNativeMediaType(
            int dwStreamIndex,
            int dwMediaTypeIndex,
            out IMFMediaType ppMediaType
        );

        [PreserveSig]
        new int GetCurrentMediaType(
            int dwStreamIndex,
            out IMFMediaType ppMediaType
        );

        [PreserveSig]
        new int SetCurrentMediaType(
            int dwStreamIndex,
            [In, Out] MFInt pdwReserved,
            IMFMediaType pMediaType
        );

        [PreserveSig]
        new int SetCurrentPosition(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidTimeFormat,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant varPosition
        );

        [PreserveSig]
        new int ReadSample(
            int dwStreamIndex,
            MF_SOURCE_READER_CONTROL_FLAG dwControlFlags,
            out int pdwActualStreamIndex,
            out  MF_SOURCE_READER_FLAG pdwStreamFlags,
            out long pllTimestamp,
            out IMFSample ppSample
        );

        [PreserveSig]
        new int Flush(
            int dwStreamIndex
        );

        [PreserveSig]
        new int GetServiceForStream(
            int dwStreamIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidService,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
        );

        [PreserveSig]
        new int GetPresentationAttribute(
            int dwStreamIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidAttribute,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvarAttribute
        );

        #endregion

        [PreserveSig]
        int SetNativeMediaType(
            int dwStreamIndex,
            IMFMediaType pMediaType,
            out MF_SOURCE_READER_FLAG pdwStreamFlags);

        [PreserveSig]
        int AddTransformForStream(
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.IUnknown)] object pTransformOrActivate);

        [PreserveSig]
        int RemoveAllTransformsForStream(
            int dwStreamIndex);

        [PreserveSig]
        int GetTransformForStream(
            int dwStreamIndex,
            int dwTransformIndex,
            out Guid pGuidCategory,
            out IMFTransform ppTransform);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("588d72ab-5Bc1-496a-8714-b70617141b25")]
    public interface IMFSinkWriterEx : IMFSinkWriter
    {
        #region IMFSinkWriter methods

        [PreserveSig]
        new int AddStream(
            IMFMediaType pTargetMediaType,
            out int pdwStreamIndex
        );

        [PreserveSig]
        new int SetInputMediaType(
            int dwStreamIndex,
            IMFMediaType pInputMediaType,
            IMFAttributes pEncodingParameters
        );

        [PreserveSig]
        new int BeginWriting();

        [PreserveSig]
        new int WriteSample(
            int dwStreamIndex,
            IMFSample pSample
        );

        [PreserveSig]
        new int SendStreamTick(
            int dwStreamIndex,
            long llTimestamp
        );

        [PreserveSig]
        new int PlaceMarker(
            int dwStreamIndex,
            IntPtr pvContext
        );

        [PreserveSig]
        new int NotifyEndOfSegment(
            int dwStreamIndex
        );

        [PreserveSig]
        new int Flush(
            int dwStreamIndex
        );

        [PreserveSig]
        new int Finalize_();

        [PreserveSig]
        new int GetServiceForStream(
            int dwStreamIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidService,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
        );

        [PreserveSig]
        new int GetStatistics(
            int dwStreamIndex,
            out MF_SINK_WRITER_STATISTICS pStats
        );

        #endregion

        [PreserveSig]
        int GetTransformForStream(
            int dwStreamIndex,
            int dwTransformIndex,
            out Guid pGuidCategory,
            out IMFTransform ppTransform);
    }

    #endregion
}
