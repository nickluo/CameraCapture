/* license

alt.cs - Part of MediaFoundationLib, which provide access to MediaFoundation interfaces via .NET

Copyright (C) 2015, by the Administrators of the Media Foundation .NET SourceForge Project
http://mfnet.sourceforge.net

This is free software; you can redistribute it and/or modify it under the terms of either:

a) The Lesser General Public License version 2.1 (see license.txt)
b) The BSD License (see BSDL.txt)

*/

// This entire file only exists to work around bugs in Media Foundation.  The core problem
// is that there are some objects in MF that don't correctly support QueryInterface.  In c++
// this isn't a problem, since if you tell c++ that something is a pointer to an interface,
// it just believes you.  In fact, that's one of the places where c++ gets its performance:
// it doesn't check anything.

// In .Net, it checks.  And the way it checks is that every time it receives an interfaces
// from unmanaged code, it does a couple of QI calls on it.  First it does a QI for IUnknown.
// Second it does a QI for the specific interface it is supposed to be (ie IMFMediaSink or
// whatever).

// Since c++ *doesn't* check, oftentimes the first people to even try to call QI on some of
// MF's objects are c# programmers.  And, not surprisingly, sometimes the first time code is
// run, it doesn't work correctly.

// The only way you can work around it is to change the definition of the method from
// IMFMediaSink (or whatever interface MF is trying to pass you) to IntPtr.  Of course,
// that limits what you can do with it.  You can't call methods on an IntPtr.

// Something to keep in mind is that while the work-around involves changing the interface,
// the problem isn't in the interface, it is in the object that implements the inteface.
// This means that while the inteface may experience problems on one object, it may work
// correctly on another object.  If you are unclear on the differences between an interface
// and an object, it's time to hit the books.

// In W7, MS has fixed a few of these issues that were reported in Vista.  The problem
// is that even if they are fixed in W7, if your program also needs to run on Vista, you
// still have to use the work-arounds.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;

using MediaFoundation.Misc;
using MediaFoundation.EVR;
using MediaFoundation.ReadWrite;

namespace MediaFoundation.Alt
{
    #region Bugs in Vista and W7

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("FA993889-4383-415A-A930-DD472A8CF6F7"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFTopologyServiceLookupAlt
    {
        [PreserveSig]
        int LookupService(
            [In] MFServiceLookupType type,
            [In] int dwIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidService,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.SysInt)] IntPtr[] ppvObjects,
            [In, Out] ref int pnObjects
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("FA99388A-4383-415A-A930-DD472A8CF6F7"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFTopologyServiceLookupClientAlt
    {
        [PreserveSig]
        int InitServicePointers(
            IntPtr pLookup
            );

        [PreserveSig]
        int ReleaseServicePointers();
    }

    #endregion

    #region "Limitation of .Net"

    // When I originally added IMFGetService to this file, it was my intent that p3 should be
    //
    //    [MarshalAs(UnmanagedType.Interface)] out object ppvObject
    //
    // However, that isn't possible due to limitations in .Net.  If the MarshalAs is Interface (aka IUnknown)
    // the pointer that the caller gets *isn't* a pointer to the interface requested in the riid, it's a pointer to an
    // IUnknown, which is a different value.  While (in theory) the caller could QI the returned pointer for the desired
    // interface, why should they?  They already called a function specifying what interface they wanted.  It is not
    // unreasonable for them to expect that that's what they got.
    //
    // Were I starting from scratch, I would name this interface IMFGetServiceImpl, indicating that you should use this
    // interface (with the IntPtr) if you are implementing IMFGetService, while still having IMFGetService (with
    // UnmanagedType.IUnknown) for the people who just want to call it.

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("FA993888-4383-415A-A930-DD472A8CF6F7")]
    public interface IMFGetServiceAlt
    {
        [PreserveSig]
        int GetService(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidService,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            out IntPtr ppvObject
            );
    }

    // This is the ASync version of IMFSourceReader.  The only difference is the ReadSample method, which must allow
    // the final 4 params to be null.

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("70ae66f2-c809-4e4f-8915-bdcb406b7993")]
    public interface IMFSourceReaderAsync
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
            IntPtr pdwActualStreamIndex,
            IntPtr pdwStreamFlags,
            IntPtr pllTimestamp,
            IntPtr ppSample
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

    #endregion

    #region Bugs in Vista that appear to be fixed in W7

    public class MFExternAlt
    {
        [DllImport("MFPlat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int MFCreateEventQueue(
            out IMFMediaEventQueueAlt ppMediaEventQueue
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("2CD0BD52-BCD5-4B89-B62C-EADC0C031E7D"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaEventGeneratorAlt
    {
        [PreserveSig]
        int GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        int BeginGetEvent(
            //[In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            IntPtr pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object o
            );

        [PreserveSig]
        int EndGetEvent(
            //IMFAsyncResult pResult,
            IntPtr pResult,
            out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        int QueueEvent(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In] int hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvValue
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("D182108F-4EC6-443F-AA42-A71106EC825F")]
    public interface IMFMediaStreamAlt : IMFMediaEventGeneratorAlt
    {
        #region IMFMediaEventGeneratorAlt methods

        [PreserveSig]
        new int GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        new int BeginGetEvent(
            //[In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            IntPtr pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object o
            );

        [PreserveSig]
        new int EndGetEvent(
            //IMFAsyncResult pResult,
            IntPtr pResult,
            out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        new int QueueEvent(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In] int hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvValue
            );

        #endregion

        [PreserveSig]
        int GetMediaSource(
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaSource ppMediaSource
            );

        [PreserveSig]
        int GetStreamDescriptor(
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamDescriptor ppStreamDescriptor
            );

        [PreserveSig]
        int RequestSample(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pToken
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("36F846FC-2256-48B6-B58E-E2B638316581"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaEventQueueAlt
    {
        [PreserveSig]
        int GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        int BeginGetEvent(
            IntPtr pCallBack,
            //[In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkState
            );

        [PreserveSig]
        int EndGetEvent(
            IntPtr p1,
            //[In, MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        int QueueEvent(
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaEvent pEvent
            );

        [PreserveSig]
        int QueueEventParamVar(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In, MarshalAs(UnmanagedType.Error)] int hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvValue
            );

        [PreserveSig]
        int QueueEventParamUnk(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In, MarshalAs(UnmanagedType.Error)] int hrStatus,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnk
            );

        [PreserveSig]
        int Shutdown();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("279A808D-AEC7-40C8-9C6B-A6B492C78A66")]
    public interface IMFMediaSourceAlt : IMFMediaEventGeneratorAlt
    {
        #region IMFMediaEventGeneratorAlt methods

        [PreserveSig]
        new int GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        new int BeginGetEvent(
            //[In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            IntPtr pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object o
            );

        [PreserveSig]
        new int EndGetEvent(
            //IMFAsyncResult pResult,
            IntPtr pResult,
            out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        new int QueueEvent(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In] int hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvValue
            );

        #endregion

        [PreserveSig]
        int GetCharacteristics(
            out MFMediaSourceCharacteristics pdwCharacteristics
            );

        [PreserveSig]
        int CreatePresentationDescriptor(
            out IMFPresentationDescriptor ppPresentationDescriptor
            );

        [PreserveSig]
        int Start(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPresentationDescriptor pPresentationDescriptor,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pguidTimeFormat,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvarStartPosition
            );

        [PreserveSig]
        int Stop();

        [PreserveSig]
        int Pause();

        [PreserveSig]
        int Shutdown();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("0A97B3CF-8E7C-4A3D-8F8C-0C843DC247FB"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFStreamSinkAlt : IMFMediaEventGeneratorAlt
    {
        #region IMFMediaEventGeneratorAlt methods

        [PreserveSig]
        new int GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        new int BeginGetEvent(
            //[In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            IntPtr pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object o
            );

        [PreserveSig]
        new int EndGetEvent(
            //IMFAsyncResult pResult,
            IntPtr pResult,
            out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        new int QueueEvent(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In] int hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvValue
            );

        #endregion

        [PreserveSig]
        int GetMediaSink(
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaSinkAlt ppMediaSink
            );

        [PreserveSig]
        int GetIdentifier(
            out int pdwIdentifier
            );

        [PreserveSig]
        int GetMediaTypeHandler(
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaTypeHandler ppHandler
            );

        [PreserveSig]
        int ProcessSample(
            [In, MarshalAs(UnmanagedType.Interface)] IMFSample pSample
            );

        [PreserveSig]
        int PlaceMarker(
            [In] MFStreamSinkMarkerType eMarkerType,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvarMarkerValue,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvarContextValue
            );

        [PreserveSig]
        int Flush();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("6EF2A660-47C0-4666-B13D-CBB717F2FA2C"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaSinkAlt
    {
        [PreserveSig]
        int GetCharacteristics(
            out MFMediaSinkCharacteristics pdwCharacteristics
            );

        [PreserveSig]
        int AddStreamSink(
            [In] int dwStreamSinkIdentifier,
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pMediaType,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSinkAlt ppStreamSink
            );

        [PreserveSig]
        int RemoveStreamSink(
            [In] int dwStreamSinkIdentifier
            );

        [PreserveSig]
        int GetStreamSinkCount(
            out int pcStreamSinkCount
            );

        [PreserveSig]
        int GetStreamSinkByIndex(
            [In] int dwIndex,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSinkAlt ppStreamSink
            );

        [PreserveSig]
        int GetStreamSinkById(
            [In] int dwStreamSinkIdentifier,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSinkAlt ppStreamSink
            );

        [PreserveSig]
        int SetPresentationClock(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPresentationClock pPresentationClock
            );

        [PreserveSig]
        int GetPresentationClock(
            [MarshalAs(UnmanagedType.Interface)] out IMFPresentationClock ppPresentationClock
            );

        [PreserveSig]
        int Shutdown();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("EAECB74A-9A50-42CE-9541-6A7F57AA4AD7"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFFinalizableMediaSinkAlt : IMFMediaSinkAlt
    {
        #region IMFMediaSinkAlt methods

        [PreserveSig]
        new int GetCharacteristics(
            out MFMediaSinkCharacteristics pdwCharacteristics
            );

        [PreserveSig]
        new int AddStreamSink(
            [In] int dwStreamSinkIdentifier,
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pMediaType,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSinkAlt ppStreamSink
            );

        [PreserveSig]
        new int RemoveStreamSink(
            [In] int dwStreamSinkIdentifier
            );

        [PreserveSig]
        new int GetStreamSinkCount(
            out int pcStreamSinkCount
            );

        [PreserveSig]
        new int GetStreamSinkByIndex(
            [In] int dwIndex,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSinkAlt ppStreamSink
            );

        [PreserveSig]
        new int GetStreamSinkById(
            [In] int dwStreamSinkIdentifier,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSinkAlt ppStreamSink
            );

        [PreserveSig]
        new int SetPresentationClock(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPresentationClock pPresentationClock
            );

        [PreserveSig]
        new int GetPresentationClock(
            [MarshalAs(UnmanagedType.Interface)] out IMFPresentationClock ppPresentationClock
            );

        [PreserveSig]
        new int Shutdown();

        #endregion

        [PreserveSig]
        int BeginFinalize(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnkState
            );

        [PreserveSig]
        int EndFinalize(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pResult
            );
    }

    #endregion

}
