using System;

namespace Ghost.Server.Utilities.Interfaces
{
    public interface IUpdatable
    {
        bool Enabled { get; set; }
        void Update(TimeSpan time);
    }
}