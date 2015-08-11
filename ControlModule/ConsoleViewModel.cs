using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows.Input;
using CameraCapture.Interface;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;

namespace ControlModule
{
    [Export("Console", typeof(BindableBase))]
    public class ConsoleViewModel : BindableBase
    {
        private readonly IEventAggregator eventAggregator;

        [ImportingConstructor]
        public ConsoleViewModel(IMoniker source, IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            DeviceItems = new ObservableCollection<IDeviceInfo>(source.DeviceTable);
            PlayCommand = new DelegateCommand<string>(op =>
                {
                    if (SelectedDevice == null)
                        return;
                    eventAggregator.GetEvent<OperationEvent>().Publish(op);
                    OperationString = op == "Start" ? "Stop" : "Start";
                    ButtonShown = (OperationString == "Stop");
                });
            SnapCommand = new DelegateCommand(() =>
            {
                if (SelectedDevice == null)
                    return;
                eventAggregator.GetEvent<OperationEvent>().Publish("Snap");
            });
            eventAggregator.GetEvent<NoticeFormatEvent>().Subscribe(str => FormatString = str);
        }
        public ObservableCollection<IDeviceInfo> DeviceItems { get; private set; }

        public ICommand PlayCommand { get; private set; }
        public ICommand SnapCommand { get; private set; }

        private IDeviceInfo selectedDevice;
        public IDeviceInfo SelectedDevice
        {
            get { return selectedDevice; }
            set
            {
                SetProperty(ref selectedDevice, value);
                if (selectedDevice!=null)
                    eventAggregator.GetEvent<ActivateDeviceEvent>().Publish(selectedDevice);
            }
        }

        private string operationString = "Start";
        public string OperationString
        {
            get { return operationString; }
            set
            {
                SetProperty(ref operationString, value);
            }
        }

        private string formatString = string.Empty;
        public string FormatString
        {
            get { return formatString; }
            set
            {
                SetProperty(ref formatString, value);
            }
        }

        private bool buttonShown;
        public bool ButtonShown
        {
            get { return buttonShown; }
            set
            {
                SetProperty(ref buttonShown, value);
            }
        }


    }
}
