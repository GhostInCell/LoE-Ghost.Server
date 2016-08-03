namespace Ghost.Server.Utilities.Interfaces
{
    public interface ITimer
    {
        bool IsCanceled
        {
            get;
        }

        void Cancel();
    }
}