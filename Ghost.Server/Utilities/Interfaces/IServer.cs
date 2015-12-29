using System;

namespace Ghost.Server.Utilities.Interfaces
{
    public interface IServer
    {
        string ID { get; }
        Guid Guid { get; }
        string Name { get; }
        string Status { get; }
        bool IsRunning { get; }
    }
}