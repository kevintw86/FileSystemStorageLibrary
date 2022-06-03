using System;
using System.Threading.Tasks;

namespace FileSystemStorageLibrary.Interfaces
{
    public interface ILoggerService : IDisposable
    {
        Task Log(string msg);
    }
}

