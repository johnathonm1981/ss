﻿using System.Threading.Tasks;

using Interfaces.Status;

namespace Interfaces.Delegates.Download
{
    public interface IDownloadFromUriAsyncDelegate
    {
        Task DownloadFromUriAsync(string uri, string destination, IStatus status);
    }
}
