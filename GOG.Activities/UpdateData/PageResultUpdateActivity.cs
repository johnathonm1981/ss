﻿using System.Threading.Tasks;
using System.Linq;

using Interfaces.RequestPage;
using Interfaces.Data;
using Interfaces.DataRefinement;
using Interfaces.Status;

using Models.ProductCore;

using GOG.Interfaces.PageResults;
using GOG.Interfaces.Extraction;

namespace GOG.Activities.UpdateData
{
    public class PageResultUpdateActivity<PageType, Type> : Activity
        where PageType : Models.PageResult
        where Type : ProductCore
    {
        private string productParameter;

        private IPageResultsController<PageType> pageResultsController;
        private IPageResultsExtractionController<PageType, Type> pageResultsExtractingController;

        private IRequestPageController requestPageController;
        private IDataController<Type> dataController;

        private IDataRefinementController<Type> dataRefinementController;

        public PageResultUpdateActivity(
            string productParameter,
            IPageResultsController<PageType> pageResultsController,
            IPageResultsExtractionController<PageType, Type> pageResultsExtractingController,
            IRequestPageController requestPageController,
            IDataController<Type> dataController,
            IStatusController statusController,
            IDataRefinementController<Type> dataRefinementController = null) :
            base(statusController)
        {
            this.productParameter = productParameter;

            this.pageResultsController = pageResultsController;
            this.pageResultsExtractingController = pageResultsExtractingController;

            this.requestPageController = requestPageController;
            this.dataController = dataController;

            this.dataRefinementController = dataRefinementController;
        }

        public override async Task ProcessActivityAsync(IStatus status)
        {
            var updateAllProductsTask = statusController.Create(status, $"Update {productParameter} data");

            var productsPageResults = await pageResultsController.GetPageResults(updateAllProductsTask);

            var extractTask = statusController.Create(updateAllProductsTask, $"Extract {productParameter} data");
            var newProducts = pageResultsExtractingController.ExtractMultiple(productsPageResults);
            statusController.Complete(extractTask);

            var refineDataTask = statusController.Create(updateAllProductsTask, $"Refining {productParameter}");
            if (dataRefinementController != null)
                await dataRefinementController.RefineDataAsync(newProducts, refineDataTask);
            statusController.Complete(refineDataTask);

            var updateTask = statusController.Create(updateAllProductsTask, $"Update {productParameter}");
            await dataController.UpdateAsync(updateTask, newProducts.ToArray());
            statusController.Complete(updateTask);

            statusController.Complete(updateAllProductsTask);
        }
    }
}
