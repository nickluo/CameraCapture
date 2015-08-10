using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CameraCapture.Interface;
using MediaFoundation;
using Point = System.Drawing.Point;

namespace VideoModule
{
    [Export(typeof(IMoniker))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MonikerProvider : IMoniker
    {
        public IList<IDeviceInfo> DeviceTable { get; }

        public MonikerProvider()
        {
            // Query MF for the devices
            DeviceTable = MfDevice.GetCategoryDevices(CLSID.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID);
        }
    }
}
