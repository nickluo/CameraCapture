namespace CameraCapture.Interface
{
    public interface IDeviceInfo
    {
        string Name { get; }

        /// <summary>
        /// Returns a unique identifier for a device
        /// </summary>
        string SymbolicName { get; }
    }
}
