﻿using System;
using System.Collections.Generic;

using Controllers.Stream;
using Controllers.Storage;
using Controllers.File;
using Controllers.Directory;
using Controllers.Uri;
using Controllers.Network;
using Controllers.FileDownload;
using Controllers.Language;
using Controllers.Serialization;
using Controllers.Extraction;
using Controllers.Collection;
using Controllers.Console;
using Controllers.Settings;
using Controllers.RequestPage;
using Controllers.Throttle;
using Controllers.ImageUri;
using Controllers.Formatting;
using Controllers.LineBreaking;
using Controllers.Destination.Directory;
using Controllers.Destination.Filename;
using Controllers.Cookies;
using Controllers.PropertiesValidation;
using Controllers.Validation;
using Controllers.Conversion;
using Controllers.Data;
using Controllers.SerializedStorage;
using Controllers.Indexing;
using Controllers.Measurement;
using Controllers.Presentation;
using Controllers.RecycleBin;
using Controllers.Routing;
using Controllers.TaskStatus;
using Controllers.Hash;
using Controllers.Containment;
using Controllers.Sanitization;
using Controllers.Session;
using Controllers.Expectation;

using Interfaces.ProductTypes;
using Interfaces.TaskActivity;

using GOG.Models;

using GOG.Controllers.PageResults;
using GOG.Controllers.Extraction;
using GOG.Controllers.Enumeration;
using GOG.Controllers.Network;
using GOG.Controllers.Connection;
using GOG.Controllers.UpdateUri;
using GOG.Controllers.FileDownload;
using GOG.Controllers.Collection;

using GOG.TaskActivities.Authorization;
using GOG.TaskActivities.Load;
using GOG.TaskActivities.Update;
using GOG.TaskActivities.Update.PageResult;
using GOG.TaskActivities.Update.Wishlisted;
using GOG.TaskActivities.Update.Screenshots;
using GOG.TaskActivities.Download;
using GOG.TaskActivities.Download.Sources;
using GOG.TaskActivities.Download.Processing;
using GOG.TaskActivities.Cleanup;

using GOG.TaskActivities.Validation;

using Models.ProductRoutes;
using Models.ProductScreenshots;
using Models.ProductDownloads;

using Models.TaskStatus;

namespace GoodOfflineGames
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Foundation Controllers

            var streamController = new StreamController();
            var fileController = new FileController();
            var directoryController = new DirectoryController();

            var storageController = new StorageController(
                streamController,
                fileController);
            var serializationController = new JSONStringController();

            var jsonFilenameDelegate = new JsonFilenameDelegate();
            var uriHashesFilenameDelegate = new FixedFilenameDelegate("hashes", jsonFilenameDelegate);

            var hashTrackingController = new HashTrackingController(
                uriHashesFilenameDelegate,
                serializationController,
                storageController);

            var bytesToStringConversionController = new BytesToStringConvertionController();
            var md5HashController = new BytesToStringMd5HashController(bytesToStringConversionController);

            var serializedStorageController = new SerializedStorageController(
                hashTrackingController,
                storageController,
                serializationController);

            var consoleController = new ConsoleController();
            var formattedStringMeasurementController = new FormattedStringMeasurementController();
            var lineBreakingController = new LineBreakingController(formattedStringMeasurementController);

            var presentationController = new PresentationController(
                formattedStringMeasurementController,
                lineBreakingController,
                consoleController);

            var bytesFormattingController = new BytesFormattingController();
            var secondsFormattingController = new SecondsFormattingController();

            var taskStatusTreeToListController = new TaskStatusTreeToListController();

            var applicationTaskStatus = new TaskStatus() { Title = "Welcome to GoodOfflineGames" };

            var taskStatusViewController = new TaskStatusViewController(
                applicationTaskStatus,
                bytesFormattingController,
                secondsFormattingController,
                taskStatusTreeToListController,
                presentationController);

            var taskStatusController = new TaskStatusController(taskStatusViewController);

            var cookiesController = new CookiesController(
                storageController,
                serializationController);

            var uriController = new UriController();
            var networkController = new NetworkController(
                cookiesController,
                uriController);

            var fileDownloadController = new FileDownloadController(
                networkController,
                streamController,
                fileController,
                taskStatusController);

            var requestPageController = new RequestPageController(
                networkController);

            var languageController = new LanguageController();

            var loginTokenExtractionController = new LoginTokenExtractionController();
            var loginIdExtractionController = new LoginIdExtractionController();
            var loginUsernameExtractionController = new LoginUsernameExtractionController();

            var gogDataExtractionController = new GOGDataExtractionController();
            var screenshotExtractionController = new ScreenshotExtractionController();

            var collectionController = new CollectionController();

            var throttleController = new ThrottleController(
                taskStatusController,
                secondsFormattingController,
                200, // don't throttle if less than N items
                2 * 60); // 2 minutes

            var imageUriController = new ImageUriController();
            var screenshotUriController = new ScreenshotUriController();

            #endregion

            #region Data Controllers

            // Data controllers for products, game details, game product data, etc.

            var productCoreIndexingController = new ProductCoreIndexingController();

            // directories

            var dataDirectoryDelegate = new FixedDirectoryDelegate("data");

            var accountProductsDirectoryDelegate = new FixedDirectoryDelegate("accountProducts", dataDirectoryDelegate);
            var apiProductsDirectoryDelegate = new FixedDirectoryDelegate("apiProducts", dataDirectoryDelegate);
            var gameDetailsDirectoryDelegate = new FixedDirectoryDelegate("gameDetails", dataDirectoryDelegate);
            var gameProductDataDirectoryDelegate = new FixedDirectoryDelegate("gameProductData", dataDirectoryDelegate);
            var productsDirectoryDelegate = new FixedDirectoryDelegate("products", dataDirectoryDelegate);
            var productDownloadsDirectoryDelegate = new FixedDirectoryDelegate("productDownloads", dataDirectoryDelegate);
            var productRoutesDirectoryDelegate = new FixedDirectoryDelegate("productRoutes", dataDirectoryDelegate);
            var productScreenshotsDirectoryDelegate = new FixedDirectoryDelegate("productScreenshots", dataDirectoryDelegate);

            var recycleBinDirectoryDelegate = new FixedDirectoryDelegate("recycleBin");
            var imagesDirectoryDelegate = new FixedDirectoryDelegate("images");
            var logsDirectoryDelegate = new FixedDirectoryDelegate("logs");
            var validationDirectoryDelegate = new FixedDirectoryDelegate("md5");
            var productFilesBaseDirectoryDelegate = new FixedDirectoryDelegate("productFiles");
            var screenshotsDirectoryDelegate = new FixedDirectoryDelegate("screenshots");

            var productFilesDirectoryDelegate = new UriDirectoryDelegate(productFilesBaseDirectoryDelegate);

            // filenames

            var indexFilenameDelegate = new FixedFilenameDelegate("index", jsonFilenameDelegate);

            var wishlistedFilenameDelegate = new FixedFilenameDelegate("wishlisted", jsonFilenameDelegate);
            var updatedFilenameDelegate = new FixedFilenameDelegate("updated", jsonFilenameDelegate);
            var scheduledScreenshotsUpdatesFilenameDelegate = new FixedFilenameDelegate("scheduledScreenshotsUpdates", jsonFilenameDelegate);
            var scheduledCleanupFilenameDelegate = new FixedFilenameDelegate("scheduledCleanup", jsonFilenameDelegate);
            var scheduledRepairFilenameDelegate = new FixedFilenameDelegate("scheduledRepair", jsonFilenameDelegate);
            var lastKnownValidFilenameDelegate = new FixedFilenameDelegate("lastKnownValid", jsonFilenameDelegate);

            var uriFilenameDelegate = new UriFilenameDelegate();
            var logsFilenameDelegate = new LogFilenameDelegate();
            var validationFilenameDelegate = new ValidationFilenameDelegate();

            // index filenames

            var productsIndexDataController = new IndexDataController(
                collectionController,
                productsDirectoryDelegate,
                indexFilenameDelegate,
                serializedStorageController,
                taskStatusController);

            var accountProductsIndexDataController = new IndexDataController(
                collectionController,
                accountProductsDirectoryDelegate,
                indexFilenameDelegate,
                serializedStorageController,
                taskStatusController);

            var gameDetailsIndexDataController = new IndexDataController(
                collectionController,
                gameDetailsDirectoryDelegate,
                indexFilenameDelegate,
                serializedStorageController,
                taskStatusController);

            var gameProductDataIndexDataController = new IndexDataController(
                collectionController,
                gameProductDataDirectoryDelegate,
                indexFilenameDelegate,
                serializedStorageController,
                taskStatusController);

            var apiProductsIndexDataController = new IndexDataController(
                collectionController,
                apiProductsDirectoryDelegate,
                indexFilenameDelegate,
                serializedStorageController,
                taskStatusController);

            var productScreenshotsIndexDataController = new IndexDataController(
                collectionController,
                productScreenshotsDirectoryDelegate,
                indexFilenameDelegate,
                serializedStorageController,
                taskStatusController);

            var productDownloadsIndexDataController = new IndexDataController(
                collectionController,
                productDownloadsDirectoryDelegate,
                indexFilenameDelegate,
                serializedStorageController,
                taskStatusController);

            var productRoutesIndexDataController = new IndexDataController(
                collectionController,
                productRoutesDirectoryDelegate,
                indexFilenameDelegate,
                serializedStorageController,
                taskStatusController);

            // index data controllers that are data controllers

            var wishlistedDataController = new IndexDataController(
                collectionController,
                dataDirectoryDelegate,
                wishlistedFilenameDelegate,
                serializedStorageController,
                taskStatusController);

            var updatedDataController = new IndexDataController(
                collectionController,
                dataDirectoryDelegate,
                updatedFilenameDelegate,
                serializedStorageController,
                taskStatusController);

            var scheduledScreenshotsUpdatesDataController = new IndexDataController(
                collectionController,
                dataDirectoryDelegate,
                scheduledScreenshotsUpdatesFilenameDelegate,
                serializedStorageController,
                taskStatusController);

            var lastKnownValidDataController = new IndexDataController(
                collectionController,
                dataDirectoryDelegate,
                lastKnownValidFilenameDelegate,
                serializedStorageController,
                taskStatusController);

            var scheduledCleanupDataController = new IndexDataController(
                collectionController,
                dataDirectoryDelegate,
                scheduledCleanupFilenameDelegate,
                serializedStorageController,
                taskStatusController);

            // data controllers

            var recycleBinController = new RecycleBinController(
                recycleBinDirectoryDelegate,
                fileController,
                directoryController);

            var productsDataController = new DataController<Product>(
                productsIndexDataController,
                serializedStorageController,
                productCoreIndexingController,
                collectionController,
                productsDirectoryDelegate,
                jsonFilenameDelegate,
                recycleBinController,
                taskStatusController);

            var accountProductsDataController = new DataController<AccountProduct>(
                accountProductsIndexDataController,
                serializedStorageController,
                productCoreIndexingController,
                collectionController,
                accountProductsDirectoryDelegate,
                jsonFilenameDelegate,
                recycleBinController,
                taskStatusController);

            var gameDetailsDataController = new DataController<GameDetails>(
                gameDetailsIndexDataController,
                serializedStorageController,
                productCoreIndexingController,
                collectionController,
                gameDetailsDirectoryDelegate,
                jsonFilenameDelegate,
                recycleBinController,
                taskStatusController);

            var gameProductDataController = new DataController<GameProductData>(
                gameProductDataIndexDataController,
                serializedStorageController,
                productCoreIndexingController,
                collectionController,
                gameProductDataDirectoryDelegate,
                jsonFilenameDelegate,
                recycleBinController,
                taskStatusController);

            var apiProductsDataController = new DataController<ApiProduct>(
                apiProductsIndexDataController,
                serializedStorageController,
                productCoreIndexingController,
                collectionController,
                apiProductsDirectoryDelegate,
                jsonFilenameDelegate,
                recycleBinController,
                taskStatusController);

            var screenshotsDataController = new DataController<ProductScreenshots>(
                productScreenshotsIndexDataController,
                serializedStorageController,
                productCoreIndexingController,
                collectionController,
                productScreenshotsDirectoryDelegate,
                jsonFilenameDelegate,
                recycleBinController,
                taskStatusController);

            var productDownloadsDataController = new DataController<ProductDownloads>(
                productDownloadsIndexDataController,
                serializedStorageController,
                productCoreIndexingController,
                collectionController,
                productDownloadsDirectoryDelegate,
                jsonFilenameDelegate,
                recycleBinController,
                taskStatusController);

            var productRoutesDataController = new DataController<ProductRoutes>(
                productRoutesIndexDataController,
                serializedStorageController,
                productCoreIndexingController,
                collectionController,
                productRoutesDirectoryDelegate,
                jsonFilenameDelegate,
                recycleBinController,
                taskStatusController);

            #endregion

            #region Settings: Load, Validation

            var settingsController = new SettingsController(
                storageController,
                serializationController);

            var loadSettingsTask = taskStatusController.Create(applicationTaskStatus, "Load settings");
            var settings = settingsController.Load().Result;

            var validateSettingsTask = taskStatusController.Create(loadSettingsTask, "Validate settings");

            var validateDownloadSettingsTask = taskStatusController.Create(validateSettingsTask, "Validate download settings");
            var downloadPropertiesValidationController = new DownloadPropertiesValidationController(languageController);
            settings.Download = downloadPropertiesValidationController.ValidateProperties(settings.Download) as Models.Settings.DownloadProperties;
            taskStatusController.Complete(validateDownloadSettingsTask);

            taskStatusController.Complete(validateSettingsTask);
            taskStatusController.Complete(loadSettingsTask);

            // set user agent string used for network requests
            if (!string.IsNullOrEmpty(settings.Connection.UserAgent))
                networkController.UserAgent = settings.Connection.UserAgent;

            #endregion

            #region Task Activity Controllers

            #region Load

            var loadDataController = new LoadDataController(
                taskStatusController,
                hashTrackingController,
                productsDataController,
                accountProductsDataController,
                gameDetailsDataController,
                gameProductDataController,
                screenshotsDataController,
                apiProductsDataController,
                wishlistedDataController,
                updatedDataController,
                scheduledScreenshotsUpdatesDataController,
                productDownloadsDataController,
                productRoutesDataController,
                lastKnownValidDataController,
                scheduledCleanupDataController);

            #endregion

            #region Authorization

            var authenticationPropertiesValidationController = new AuthenticationPropertiesValidationController(consoleController);

            var authorizationController = new AuthorizationController(
                uriController,
                networkController,
                serializationController,
                loginTokenExtractionController,
                loginIdExtractionController,
                loginUsernameExtractionController,
                consoleController,
                settings.Authentication,
                authenticationPropertiesValidationController,
                taskStatusController);

            #endregion

            #region Update.PageResults

            var productsPageResultsController = new PageResultsController<ProductsPageResult>(
                ProductTypes.Product,
                requestPageController,
                hashTrackingController,
                serializationController,
                taskStatusController);

            var productsExtractionController = new ProductsExtractionController();

            var productsUpdateController = new PageResultUpdateController<
                ProductsPageResult,
                Product>(
                    ProductTypes.Product,
                    productsPageResultsController,
                    productsExtractionController,
                    requestPageController,
                    productsDataController,
                    taskStatusController);

            var accountProductsPageResultsController = new PageResultsController<AccountProductsPageResult>(
                ProductTypes.AccountProduct,
                requestPageController,
                hashTrackingController,
                serializationController,
                taskStatusController);

            var accountProductsExtractionController = new AccountProductsExtractionController();

            var newUpdatedCollectionProcessingController = new NewUpdatedCollectionProcessingController(
                collectionController,
                updatedDataController,
                lastKnownValidDataController,
                taskStatusController);

            var accountProductsUpdateController = new PageResultUpdateController<
                AccountProductsPageResult,
                AccountProduct>(
                    ProductTypes.AccountProduct,
                    accountProductsPageResultsController,
                    accountProductsExtractionController,
                    requestPageController,
                    accountProductsDataController,
                    taskStatusController,
                    newUpdatedCollectionProcessingController);

            #endregion

            #region Update.Wishlisted

            var getProductsPageResultDelegate = new GetDeserializedGOGDataDelegate<ProductsPageResult>(networkController,
                gogDataExtractionController,
                serializationController);

            var wishlistedUpdateController = new WishlistedUpdateController(
                getProductsPageResultDelegate,
                wishlistedDataController,
                taskStatusController);

            #endregion

            #region Update.Products

            // dependencies for update controllers

            var getGOGDataDelegate = new GetDeserializedGOGDataDelegate<GOGData>(networkController,
                gogDataExtractionController,
                serializationController);

            var getGameProductDataDeserializedDelegate = new GetGameProductDataDeserializedDelegate(
                getGOGDataDelegate);

            var productIdUpdateUriDelegate = new ProductIdUpdateUriDelegate();
            var productUrlUpdateUriDelegate = new ProductUrlUpdateUriDelegate();
            var accountProductIdUpdateUriDelegate = new AccountProductIdUpdateUriDelegate();

            var gameDetailsAccountProductConnectDelegate = new GameDetailsAccountProductConnectDelegate();

            // product update controllers

            var gameProductDataUpdateController = new ProductCoreUpdateController<GameProductData, Product>(
                ProductTypes.GameProductData,
                gameProductDataController,
                productsDataController,
                updatedDataController,
                getGameProductDataDeserializedDelegate,
                productUrlUpdateUriDelegate,
                taskStatusController);

            var getApriProductDelegate = new GetDeserializedGOGModelDelegate<ApiProduct>(
                networkController,
                serializationController);

            var apiProductUpdateController = new ProductCoreUpdateController<ApiProduct, Product>(
                ProductTypes.AccountProduct,
                apiProductsDataController,
                productsDataController,
                updatedDataController,
                getApriProductDelegate,
                productIdUpdateUriDelegate,
                taskStatusController);

            var getDeserializedGameDetailsDelegate = new GetDeserializedGOGModelDelegate<GameDetails>(
                networkController,
                serializationController);

            var languageDownloadsContainmentController = new StringContainmentController(
                collectionController,
                Models.Separators.Separators.GameDetailsDownloadsStart,
                Models.Separators.Separators.GameDetailsDownloadsEnd);

            var gameDetailsLanguagesExtractionController = new GameDetailsLanguagesExtractionController();
            var gameDetailsDownloadsExtractionController = new GameDetailsDownloadsExtractionController();

            var sanitizationController = new SanitizationController();

            var operatingSystemsDownloadsExtractionController = new OperatingSystemsDownloadsExtractionController(
                sanitizationController,
                languageController);

            var getGameDetailsDelegate = new GetDeserializedGameDetailsDelegate(
                networkController,
                serializationController,
                languageController,
                languageDownloadsContainmentController,
                gameDetailsLanguagesExtractionController,
                gameDetailsDownloadsExtractionController,
                sanitizationController,
                operatingSystemsDownloadsExtractionController);

            var gameDetailsUpdateController = new ProductCoreUpdateController<GameDetails, AccountProduct>(
                ProductTypes.GameDetails,
                gameDetailsDataController,
                accountProductsDataController,
                updatedDataController,
                getGameDetailsDelegate,
                accountProductIdUpdateUriDelegate,
                taskStatusController,
                throttleController,
                gameDetailsAccountProductConnectDelegate);

            #endregion

            #region Update.Screenshots

            var screenshotUpdateController = new ScreenshotUpdateController(
                screenshotsDataController,
                scheduledScreenshotsUpdatesDataController,
                productsDataController,
                networkController,
                screenshotExtractionController,
                taskStatusController);

            #endregion

            // dependencies for download controllers

            var productsImagesDownloadSourcesController = new ProductsImagesDownloadSourcesController(
                updatedDataController,
                productsDataController,
                imageUriController);

            var accountProductsImagesDownloadSourcesController = new AccountProductsImagesDownloadSourcesController(
                updatedDataController,
                accountProductsDataController,
                imageUriController);

            var screenshotsDownloadSourcesController = new ScreenshotsDownloadSourcesController(
                scheduledScreenshotsUpdatesDataController,
                screenshotsDataController,
                screenshotUriController,
                taskStatusController);

            var routingController = new RoutingController(productRoutesDataController);

            var gameDetailsManualUrlsEnumerationController = new GameDetailsManualUrlEnumerationController(
                settings.Download.Languages,
                settings.Download.OperatingSystems,
                gameDetailsDataController);

            var gameDetailsDirectoryEnumerationController = new GameDetailsDirectoryEnumerationController(
                settings.Download.Languages,
                settings.Download.OperatingSystems,
                gameDetailsDataController,
                productFilesDirectoryDelegate);

            var gameDetailsFilesEnumerationController = new GameDetailsFileEnumerationController(
                settings.Download.Languages,
                settings.Download.OperatingSystems,
                gameDetailsDataController,
                routingController,
                productFilesDirectoryDelegate,
                uriFilenameDelegate);

            // product files and validation files are driven through gameDetails manual urls
            // so this sources enumerates all manual urls for all updated game details
            var manualUrlDownloadSourcesController = new ManualUrlDownloadSourcesController(
                updatedDataController,
                gameDetailsManualUrlsEnumerationController);

            // schedule download controllers

            var updateProductsImagesDownloadsController = new UpdateDownloadsController(
                ProductDownloadTypes.Image,
                productsImagesDownloadSourcesController,
                imagesDirectoryDelegate,
                productDownloadsDataController,
                accountProductsDataController,
                taskStatusController);

            var updateAccountProductsImagesDownloadsController = new UpdateDownloadsController(
                ProductDownloadTypes.Image,
                accountProductsImagesDownloadSourcesController,
                imagesDirectoryDelegate,
                productDownloadsDataController,
                accountProductsDataController,
                taskStatusController);

            var updateScreenshotsDownloadsController = new UpdateDownloadsController(
                ProductDownloadTypes.Screenshot,
                screenshotsDownloadSourcesController,
                screenshotsDirectoryDelegate,
                productDownloadsDataController,
                accountProductsDataController,
                taskStatusController);

            var updateProductFilesDownloadsController = new UpdateDownloadsController(
                ProductDownloadTypes.ProductFile,
                manualUrlDownloadSourcesController,
                productFilesDirectoryDelegate,
                productDownloadsDataController,
                accountProductsDataController,
                taskStatusController);

            // downloads processing

            var imagesProcessScheduledDownloadsController = new ProcessScheduledDownloadsController(
                ProductDownloadTypes.Image,
                updatedDataController,
                productDownloadsDataController,
                fileDownloadController,
                taskStatusController);

            var screenshotsProcessScheduledDownloadsController = new ProcessScheduledDownloadsController(
                ProductDownloadTypes.Screenshot,
                updatedDataController,
                productDownloadsDataController,
                fileDownloadController,
                taskStatusController);

            var sesionUriExtractionController = new SessionUriExtractionController();
            var sessionController = new SessionController(
                networkController, 
                sesionUriExtractionController);

            var manualUrlDownloadFromSourceDelegate = new ManualUrlDownloadFromSourceDelegate(
                networkController,
                sessionController,
                routingController,
                fileDownloadController,
                taskStatusController);

            var productFilesProcessScheduledDownloadsController = new ProcessScheduledDownloadsController(
                ProductDownloadTypes.ProductFile,
                updatedDataController,
                productDownloadsDataController,
                manualUrlDownloadFromSourceDelegate,
                taskStatusController);

            // validation controllers

            var updateValidationDownloadsController = new UpdateDownloadsController(
                ProductDownloadTypes.Validation,
                manualUrlDownloadSourcesController,
                validationDirectoryDelegate,
                productDownloadsDataController,
                accountProductsDataController,
                taskStatusController);

            var validationExpectedDelegate = new ValidationExpectedDelegate();
            var validationUriController = new ValidationUriController(uriController);

            var validationDownloadFromSourceDelegate = new ValidationDownloadFromSourceDelegate(
                routingController, 
                sessionController,
                validationExpectedDelegate,
                validationUriController,
                fileDownloadController, 
                taskStatusController);

            var validationProcessScheduledDownloadsController = new ProcessScheduledDownloadsController(
                ProductDownloadTypes.Validation,
                updatedDataController,
                productDownloadsDataController,
                validationDownloadFromSourceDelegate,
                taskStatusController);

            var validationController = new ValidationController(
                validationDirectoryDelegate,
                validationFilenameDelegate,
                fileController,
                streamController,
                md5HashController,
                taskStatusController);

            var processValidationController = new ProcessValidationController(
                productFilesDirectoryDelegate,
                uriFilenameDelegate,
                validationController,
                gameDetailsManualUrlsEnumerationController, 
                validationExpectedDelegate,
                updatedDataController,
                lastKnownValidDataController,
                scheduledCleanupDataController,
                routingController,
                taskStatusController);

            #region Cleanup

            var directoryCleanupController = new DirectoryCleanupController(
                gameDetailsDataController,
                gameDetailsDirectoryEnumerationController,
                productFilesDirectoryDelegate,
                directoryController,
                recycleBinController,
                taskStatusController);

            var filesCleanupController = new FilesCleanupController(
                scheduledCleanupDataController,
                accountProductsDataController,
                gameDetailsFilesEnumerationController,
                gameDetailsDirectoryEnumerationController,
                directoryController,
                validationDirectoryDelegate,
                uriFilenameDelegate,
                recycleBinController,
                taskStatusController);

            #endregion

            #endregion

            #region TACs Execution

            #region Initialization Task Activities (always performed)

            var taskActivityControllers = new List<ITaskActivityController>
            {
                // load initial data
                loadDataController,
                // authorize
                authorizationController
            };

            #endregion

            #region Data Updates Task Activities

            // data updates
            if (settings.Update.Products)
                taskActivityControllers.Add(productsUpdateController);
            if (settings.Update.AccountProducts)
                taskActivityControllers.Add(accountProductsUpdateController);
            if (settings.Update.Wishlist)
                taskActivityControllers.Add(wishlistedUpdateController);

            // product/account product dependent data updates
            if (settings.Update.GameProductData)
                taskActivityControllers.Add(gameProductDataUpdateController);
            if (settings.Update.ApiProducts)
                taskActivityControllers.Add(apiProductUpdateController);
            if (settings.Update.GameDetails)
                taskActivityControllers.Add(gameDetailsUpdateController);
            if (settings.Update.Screenshots)
                taskActivityControllers.Add(screenshotUpdateController);

            #endregion

            #region Download Task Activities

            // schedule downloads
            if (settings.Download.ProductsImages)
                taskActivityControllers.Add(updateProductsImagesDownloadsController);
            if (settings.Download.AccountProductsImages)
                taskActivityControllers.Add(updateAccountProductsImagesDownloadsController);
            if (settings.Download.Screenshots)
                taskActivityControllers.Add(updateScreenshotsDownloadsController);
            if (settings.Download.ProductsFiles)
                taskActivityControllers.Add(updateProductFilesDownloadsController);

            //actually download images, screenshots, product files, extras
            if (settings.Download.ProductsImages ||
                settings.Download.AccountProductsImages)
                taskActivityControllers.Add(imagesProcessScheduledDownloadsController);
            if (settings.Download.Screenshots)
                taskActivityControllers.Add(screenshotsProcessScheduledDownloadsController);
            if (settings.Download.ProductsFiles)
                taskActivityControllers.Add(productFilesProcessScheduledDownloadsController);

            #endregion

            #region Validation Task Activities

            // validation downloads should follow productFiles download processing, because they use timed CDN key
            if (settings.Validation.Download)
                taskActivityControllers.Add(updateValidationDownloadsController);
            if (settings.Validation.Download)
                taskActivityControllers.Add(validationProcessScheduledDownloadsController);
            if (settings.Validation.ValidateUpdated)
                taskActivityControllers.Add(processValidationController);

            #endregion

            #region Cleanup Task Activities

            // cleanup directories
            if (settings.Cleanup.Directories)
                taskActivityControllers.Add(directoryCleanupController);
            // cleanup files
            if (settings.Cleanup.Files)
                taskActivityControllers.Add(filesCleanupController);

            #endregion

            foreach (var taskActivityController in taskActivityControllers)
            {
                try
                {
                    taskActivityController.ProcessTaskAsync(applicationTaskStatus).Wait();
                    taskStatusViewController.CreateView(true);
                }
                catch (AggregateException ex)
                {
                    List<string> errorMessages = new List<string>();

                    foreach (var innerException in ex.InnerExceptions)
                        errorMessages.Add(innerException.Message);

                    taskStatusController.Fail(applicationTaskStatus, string.Join(", ", errorMessages));
                    break;
                }
            }

            taskStatusController.Complete(applicationTaskStatus);
            taskStatusViewController.CreateView(true);

            #region Save log 

            if (settings.Log)
            {
                var uri = System.IO.Path.Combine(
                    logsDirectoryDelegate.GetDirectory(),
                    logsFilenameDelegate.GetFilename());

                presentationController.Present(new List<Tuple<string, string[]>>
                {
                    Tuple.Create(string.Format("Save log to {0}", uri), new string[] { "white" })
                });

                serializedStorageController.SerializePushAsync(uri, applicationTaskStatus).Wait();
            }

            #endregion

            var defaultColor = new string[] { " default" };

            presentationController.Present(
                new List<Tuple<string, string[]>> {
                    Tuple.Create("%cAll GoodOfflineGames tasks are complete.", new string[] { "white" }),
                    Tuple.Create("", defaultColor),
                    Tuple.Create("%cPress ENTER to close the window...", defaultColor)
                }, true);
            consoleController.ReadLine();

            #endregion
        }
    }
}
