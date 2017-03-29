﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Interfaces.Validation;
using Interfaces.Destination.Directory;
using Interfaces.Destination.Filename;
using Interfaces.Data;
using Interfaces.Enumeration;
using Interfaces.Routing;
using Interfaces.Status;

using GOG.Models;

namespace GOG.Activities.Validate
{
    public class ValidateActivity : Activity
    {
        private IGetDirectoryDelegate productFileDirectoryDelegate;
        private IGetFilenameDelegate productFileFilenameDelegate;
        private IEnumerateDelegate<string> validationFileEnumerateDelegate;
        private IValidationController validationController;
        private IDataController<GameDetails> gameDetailsDataController;
        private IEnumerateDelegate<GameDetails> manualUrlsEnumerationController;
        private IDataController<long> updatedDataController;
        private IRoutingController routingController;

        public ValidateActivity(
            IGetDirectoryDelegate productFileDirectoryDelegate,
            IGetFilenameDelegate productFileFilenameDelegate,
            IEnumerateDelegate<string> validationFileEnumerateDelegate,
            IValidationController validationController,
            IDataController<GameDetails> gameDetailsDataController,
            IEnumerateDelegate<GameDetails> manualUrlsEnumerationController,
            IDataController<long> updatedDataController,
            IRoutingController routingController,
            IStatusController statusController) :
            base(statusController)
        {
            this.productFileDirectoryDelegate = productFileDirectoryDelegate;
            this.productFileFilenameDelegate = productFileFilenameDelegate;
            this.validationFileEnumerateDelegate = validationFileEnumerateDelegate;
            this.validationController = validationController;
            this.gameDetailsDataController = gameDetailsDataController;
            this.manualUrlsEnumerationController = manualUrlsEnumerationController;

            this.updatedDataController = updatedDataController;
            this.routingController = routingController;
        }

        public override async Task ProcessActivityAsync(IStatus status)
        {
            var validateProductFilesTask = statusController.Create(status, "Validate products files");

            var counter = 0;

            var updated = updatedDataController.EnumerateIds();
            var updatedCount = updatedDataController.Count();

            foreach (var id in updated)
            {
                var productIsValid = true;

                var gameDetails = await gameDetailsDataController.GetByIdAsync(id);

                var manualUrls = manualUrlsEnumerationController.Enumerate(gameDetails);

                statusController.UpdateProgress(validateProductFilesTask,
                    ++counter,
                    updatedCount,
                    id.ToString());

                foreach (var manualUrl in manualUrls)
                {
                    var resolvedUri = await routingController.TraceRouteAsync(id, manualUrl);

                    // use directory from source and file from resolved URI
                    var localFile = Path.Combine(
                        productFileDirectoryDelegate.GetDirectory(manualUrl),
                        productFileFilenameDelegate.GetFilename(resolvedUri));

                    var validationFile = validationFileEnumerateDelegate.Enumerate(localFile).Single();

                    var validateFileTask = statusController.Create(
                        validateProductFilesTask,
                        "Validate product file");

                    try
                    {
                        await validationController.ValidateAsync(localFile, validationFile, validateFileTask);
                        productIsValid &= true;
                    }
                    catch (Exception ex)
                    {
                        statusController.Fail(validateProductFilesTask,
                            $"{localFile}: {ex.Message}");
                        productIsValid &= false;
                    }
                    finally
                    {
                        statusController.Complete(validateFileTask);
                    }
                }

                // TODO: Save validation report

            }

            statusController.Complete(validateProductFilesTask);
        }
    }
}