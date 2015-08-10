using System.Collections.Generic;

namespace CameraCapture.Interface
{
    public interface IMoniker
    {
        IList<IDeviceInfo> DeviceTable { get; }
    }
}
