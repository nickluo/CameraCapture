using System;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using CameraCapture.Interface;
using MediaFoundation;
using MediaFoundation.Misc;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;

namespace VideoModule
{
    [Export("WebCam",typeof(BindableBase))]
    class WebCamViewModel : BindableBase, IPartImportsSatisfiedNotification, IDisplayAdapter, IDisposable
    {
        private const int WM_APP = 0x8000;
        private const int WM_APP_PREVIEW_ERROR = WM_APP + 2;
        private const int WM_DEVICECHANGE = 0x0219;

        private const string DefaultFileName = "test.mp4";

        // Category for capture devices
        // ReSharper disable once InconsistentNaming
        private readonly Guid KSCATEGORY_CAPTURE = new Guid("65E8773D-8F56-11D0-A3B9-00A0C9223196");

        private RegisterDeviceNotifications rdn;
        private CProcess camProcess;

        private readonly IEventAggregator eventAggregator;

        public InteractionRequest<INotification> NotificationRequest { get; }

        [ImportingConstructor]
        public WebCamViewModel(IEventAggregator eventAggregator)
        {
            NotificationRequest = new InteractionRequest<INotification>();
            this.eventAggregator = eventAggregator;
            //Need use thread expect UI to dispose COM objects
            Application.Current.MainWindow.Closing += (o, args) =>
            {
                var task=Task.Factory.StartNew(Dispose);
                task.Wait();
            };
        }

        private MfDevice activeDevice;

        void OnActivate(IDeviceInfo moniker)
        {
            activeDevice = moniker as MfDevice;
            var format = string.Empty;
            var hr = camProcess.SetDevice(activeDevice,ref format);
            if (!string.IsNullOrEmpty(format))
                eventAggregator.GetEvent<NoticeFormatEvent>().Publish(format);
            MFError.ThrowExceptionForHR(hr);
        }


        public void OnImportsSatisfied()
        {
            eventAggregator.GetEvent<ActivateDeviceEvent>().Subscribe(OnActivate, ThreadOption.BackgroundThread, false);
            eventAggregator.GetEvent<OperationEvent>().Subscribe(OnOperation,ThreadOption.BackgroundThread);
        }

        private void OnOperation(string op)
        {
            var capture = camProcess as ICapture;
            switch (op)
            {
                case "Start":
                    var filename = ConfigurationManager.AppSettings["filename"];
                    capture?.StartCapture(filename?? DefaultFileName, MFMediaType.H264);
                    break;
                case "Stop":  
                    capture?.StopCapture();
                    break;
            }
        }

        public void InitDisplay(IntPtr hVideo, IntPtr hEvent)
        {
            if (rdn==null)
                rdn = new RegisterDeviceNotifications(hEvent, KSCATEGORY_CAPTURE);
            if (camProcess == null)
                //camProcess = new CPreview(hVideo, hEvent);
                camProcess = new CCapture(hVideo, hEvent);
        }

        public void OnResize()
        {
            camProcess?.ResizeVideo();
        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            switch (msg)
            {
                case WM_APP_PREVIEW_ERROR:
                    NotifyError("An error occurred.", (int)wparam);
                    break;

                case WM_DEVICECHANGE:
                    OnDeviceChange(wparam, lparam);
                    break;
            }
            return IntPtr.Zero;
        }

        private void NotifyError(string sErrorMessage, int hrErr)
        {
            var sErrMsg = MFError.GetErrorText(hrErr);
            string sMsg = $"{sErrorMessage} (HRESULT = 0x{hrErr:x}:{sErrMsg})";
            camProcess.CloseDevice();
            NotificationRequest.Raise(new Notification { Content = sMsg, Title = "Error" });
        }

        private void OnDeviceChange(IntPtr reason, IntPtr pHdr)
        {
            // Check for the right category of event
            if (rdn.CheckEventDetails(reason, pHdr))
            {
                if (camProcess != null)
                {
                    var sSym = RegisterDeviceNotifications.ParseDeviceSymbolicName(pHdr);
                    if (camProcess.CheckDeviceLost(sSym))
                    {
                        NotifyError("Lost the capture device", 0);
                    }
                }
            }
        }

        public void Dispose()
        {
            rdn?.Dispose();
            rdn = null;

            camProcess?.Dispose();
            camProcess = null;
            
            // Shut down MF
            MFExtern.MFShutdown();
        }
    }
}
