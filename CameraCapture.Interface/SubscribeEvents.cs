using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.PubSubEvents;

namespace CameraCapture.Interface
{
    public class ActivateDeviceEvent : PubSubEvent<IDeviceInfo>
    {
    }

    public class OperationEvent : PubSubEvent<string>
    {
    }

    public class NoticeFormatEvent : PubSubEvent<string>
    {
    }

}
