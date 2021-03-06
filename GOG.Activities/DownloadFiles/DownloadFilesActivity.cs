﻿using System.Collections.Generic;
using System.Threading.Tasks;

using Interfaces.Controllers.Data;

using Interfaces.Status;
using Interfaces.Models.Entities;

using Models.ProductDownloads;
using Models.Separators;

using GOG.Interfaces.Delegates.DownloadProductFile;

namespace GOG.Activities.DownloadProductFiles
{
    public class DownloadFilesActivity : Activity
    {
        Entity context;
        readonly IDataController<ProductDownloads> productDownloadsDataController;
        readonly IDownloadProductFileAsyncDelegate downloadProductFileAsyncDelegate;

        public DownloadFilesActivity(
            Entity context,
            IDataController<ProductDownloads> productDownloadsDataController,
            IDownloadProductFileAsyncDelegate downloadProductFileAsyncDelegate,
            IStatusController statusController) :
            base(statusController)
        {
            this.context = context;
            this.productDownloadsDataController = productDownloadsDataController;
            this.downloadProductFileAsyncDelegate = downloadProductFileAsyncDelegate;
        }

        public override async Task ProcessActivityAsync(IStatus status)
        {
            var processDownloadsTask = await statusController.CreateAsync(status,
                $"Process updated {context} downloads");

            var current = 0;
            var productDownloadsData = await productDownloadsDataController.ItemizeAllAsync(processDownloadsTask);
            var total = await productDownloadsDataController.CountAsync(processDownloadsTask);

            var emptyProductDownloads = new List<ProductDownloads>();

            foreach (var id in productDownloadsData)
            {
                var productDownloads = await productDownloadsDataController.GetByIdAsync(id, processDownloadsTask);
                if (productDownloads == null) continue;

                await statusController.UpdateProgressAsync(
                    processDownloadsTask,
                    ++current,
                    total,
                    productDownloads.Title);

                // we'll need to remove successfully downloaded files, copying collection
                var downloadEntries = productDownloads.Downloads.FindAll(
                    d =>
                    d.Context == context).ToArray();

                var processDownloadEntriesTask = await statusController.CreateAsync(processDownloadsTask,
                    $"Download {context} entries");

                for (var ii = 0; ii < downloadEntries.Length; ii++)
                {
                    var entry = downloadEntries[ii];

                    var sanitizedUri = entry.SourceUri;
                    if (sanitizedUri.Contains(Separators.QueryString))
                        sanitizedUri = sanitizedUri.Substring(0, sanitizedUri.IndexOf(Separators.QueryString, System.StringComparison.Ordinal));

                    await statusController.UpdateProgressAsync(
                        processDownloadEntriesTask,
                        ii + 1,
                        downloadEntries.Length,
                        sanitizedUri);

                    await downloadProductFileAsyncDelegate?.DownloadProductFileAsync(
                        id,
                        productDownloads.Title,
                        sanitizedUri,
                        entry.Destination,
                        processDownloadEntriesTask);

                    var removeEntryTask = await statusController.CreateAsync(
                        processDownloadEntriesTask,
                        $"Remove scheduled {context} downloaded entry");

                    productDownloads.Downloads.Remove(entry);
                    await productDownloadsDataController.UpdateAsync(productDownloads, removeEntryTask);

                    await statusController.CompleteAsync(removeEntryTask);
                }

                // if there are no scheduled downloads left - mark file for removal
                if (productDownloads.Downloads.Count == 0)
                    emptyProductDownloads.Add(productDownloads);

                await statusController.CompleteAsync(processDownloadEntriesTask);
            }

            var clearEmptyDownloadsTask = await statusController.CreateAsync(processDownloadsTask, "Clear empty downloads");

            foreach (var productDownload in emptyProductDownloads)
                await productDownloadsDataController.DeleteAsync(productDownload, clearEmptyDownloadsTask);

            await statusController.CompleteAsync(clearEmptyDownloadsTask);

            await statusController.CompleteAsync(processDownloadsTask);
        }
    }
}
