using System;
using System.Threading.Tasks;

namespace FileSystemStorageLibrary.Interfaces
{
    public interface ILoggerService : IDisposable
    {
        Task LogAsync(string msg);
        void Log(string msg);
    }
}

