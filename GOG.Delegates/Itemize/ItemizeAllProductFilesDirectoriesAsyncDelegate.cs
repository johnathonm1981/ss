﻿using System.Collections.Generic;
using System.Threading.Tasks;

using Interfaces.Delegates.GetDirectory;
using Interfaces.Delegates.Itemize;

using Interfaces.Controllers.Directory;

using Interfaces.Status;

namespace GOG.Delegates.Itemize
{
    public class ItemizeAllProductFilesDirectoriesAsyncDelegate : IItemizeAllAsyncDelegate<string>
    {
        readonly IGetDirectoryDelegate productFilesDirectoryDelegate;
        readonly IDirectoryController directoryController;
        readonly IStatusController statusController;

        public ItemizeAllProductFilesDirectoriesAsyncDelegate(
            IGetDirectoryDelegate productFilesDirectoryDelegate,
            IDirectoryController directoryController,
            IStatusController statusController)
        {
            this.productFilesDirectoryDelegate = productFilesDirectoryDelegate;
            this.directoryController = directoryController;
            this.statusController = statusController;
        }

        public async Task<IEnumerable<string>> ItemizeAllAsync(IStatus status)
        {
            var enumerateProductFilesDirectoriesTask = await statusController.CreateAsync(
                status,
                "Enumerate productFiles directories");

            var directories = new List<string>();

            var productFilesDirectory = productFilesDirectoryDelegate.GetDirectory(string.Empty);
            directories.AddRange(directoryController.EnumerateDirectories(productFilesDirectory));

            await statusController.CompleteAsync(enumerateProductFilesDirectoriesTask);

            return directories;
        }
    }
}
