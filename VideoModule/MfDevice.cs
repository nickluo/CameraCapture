using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CameraCapture.Interface;
using MediaFoundation;

namespace VideoModule
{
    internal class MfDevice : IDisposable, IDeviceInfo
    {
        private string friendlyName;
        private string symbolicName;

        public MfDevice(IMFActivate moniker)
        {
            Activator = moniker;
            friendlyName = null;
            symbolicName = null;
        }

        ~MfDevice()
        {
            Dispose();
        }

        public IMFActivate Activator { get; private set; }

        public string Name
        {
            get
            {
                if (friendlyName == null)
                {
                    int iSize;
                    Activator?.GetAllocatedString(
                        MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME,
                        out friendlyName,
                        out iSize
                        );
                }

                return friendlyName;
            }
        }

        /// <summary>
        /// Returns a unique identifier for a device
        /// </summary>
        public string SymbolicName
        {
            get
            {
                if (symbolicName != null)
                    return symbolicName;
                int iSize;
                Activator?.GetAllocatedString(
                    MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK,
                    out symbolicName,
                    out iSize
                    );

                return symbolicName;
            }
        }

        /// <summary>
        /// Returns an array of DsDevices of type.
        /// </summary>
        /// <param name="filterCategory">Any one of FilterCategory</param>
        public static IList<IDeviceInfo> GetCategoryDevices(Guid filterCategory)
        {
            var list = new List<IDeviceInfo>();

            IMFAttributes pAttributes;

            // Initialize an attribute store. We will use this to
            // specify the enumeration parameters.

            var hr = MFExtern.MFCreateAttributes(out pAttributes, 1);

            // Ask for source type = video capture devices
            if (hr >= 0)
            {
                hr = pAttributes.SetGUID(
                    MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE,
                    filterCategory
                    );
            }

            // Enumerate devices.
            if (hr >= 0)
            {
                IMFActivate[] ppDevices;
                int cDevices;
                hr = MFExtern.MFEnumDeviceSources(pAttributes, out ppDevices, out cDevices);

                if (hr >= 0)
                {
                    for (var x = 0; x < cDevices; x++)
                        list.Add(new MfDevice(ppDevices[x]));
                }
            }

            if (pAttributes != null)
            {
                Marshal.ReleaseComObject(pAttributes);
            }

            return list;
        }

        public override string ToString()
        {
            return Name;
        }

        public void Dispose()
        {
            if (Activator != null)
            {
                Marshal.ReleaseComObject(Activator);
                Activator = null;
            }
            friendlyName = null;
            symbolicName = null;

            GC.SuppressFinalize(this);
        }
    }
}
