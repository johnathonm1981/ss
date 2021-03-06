﻿using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using Interfaces.Delegates.Itemize;

using Interfaces.Controllers.Data;
using Interfaces.Controllers.Records;

using Interfaces.Models.RecordsTypes;

using Interfaces.Status;

using Models.ProductCore;

using GOG.Interfaces.Delegates.GetPageResults;

using Interfaces.ActivityContext;

using AC = System.ValueTuple<Interfaces.ActivityDefinitions.Activity, Interfaces.Models.Entities.Entity>;

namespace GOG.Activities.UpdateData
{
    public class PageResultUpdateActivity<PageType, DataType> : Activity
        where PageType : Models.PageResult
        where DataType : ProductCore
    {
        AC activityContext;
        readonly IActivityContextController activityContextController;

        readonly IGetPageResultsAsyncDelegate<PageType> getPageResultsAsyncDelegate;
        readonly IItemizeDelegate<IList<PageType>, DataType> itemizePageResultsDelegate;

        readonly IDataController<DataType> dataController;
        readonly IRecordsController<string> activityRecordsController;

        public PageResultUpdateActivity(
            AC activityContext,
            IActivityContextController activityContextController,
            IGetPageResultsAsyncDelegate<PageType> getPageResultsAsyncDelegate,
            IItemizeDelegate<IList<PageType>, DataType> itemizePageResultsDelegate,
            IDataController<DataType> dataController,
            IRecordsController<string> activityRecordsController,
            IStatusController statusController) :
            base(statusController)
        {
            this.activityContext = activityContext;
            this.activityContextController = activityContextController;

            this.getPageResultsAsyncDelegate = getPageResultsAsyncDelegate;
            this.itemizePageResultsDelegate = itemizePageResultsDelegate;

            this.dataController = dataController;
            this.activityRecordsController = activityRecordsController;
        }

        public override async Task ProcessActivityAsync(IStatus status)
        {
            var updateAllProductsTask = await statusController.CreateAsync(status, $"Update {activityContext.Item2}");

            var activityContextString = activityContextController.ToString(activityContext);

            await activityRecordsController.SetRecordAsync(activityContextString, RecordsTypes.Started, updateAllProductsTask);

            var productsPageResults = await getPageResultsAsyncDelegate.GetPageResultsAsync(updateAllProductsTask);

            var extractTask = await statusController.CreateAsync(updateAllProductsTask, $"Extract {activityContext.Item2}");
            var newProducts = itemizePageResultsDelegate.Itemize(productsPageResults);
            await statusController.CompleteAsync(extractTask);

            if (newProducts.Any())
            {
                var updateTask = await statusController.CreateAsync(updateAllProductsTask, $"Save {activityContext.Item2}");
                var current = 0;
                var updateProgressEvery = 10;

                foreach (var product in newProducts)
                {
                    if (++current % updateProgressEvery == 0)
                        await statusController.UpdateProgressAsync(updateTask, current, newProducts.Count(), product.Title);

                    await dataController.UpdateAsync(product, updateTask);
                }

                await statusController.CompleteAsync(updateTask);
            }

            await activityRecordsController.SetRecordAsync(activityContextString, RecordsTypes.Completed, updateAllProductsTask);

            await dataController.CommitAsync(updateAllProductsTask);

            await statusController.CompleteAsync(updateAllProductsTask);
        }
    }
}
