using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Batch;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using AirVinyl.Model;
using Microsoft.OData.Edm;

namespace AirVinyl.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapODataServiceRoute("ODataRoute", "odata", GetEdmModel(), new DefaultODataBatchHandler(GlobalConfiguration.DefaultServer));

            config.EnsureInitialized();
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.Namespace = "AirVinyl";
            builder.ContainerName = "AirVinylContainer";

            builder.EntitySet<Person>("People");
            //builder.EntitySet<VinylRecord>("VinylRecords");
            builder.EntitySet<RecordStore>("RecordStores");

            var isHighRatedFunction = builder.EntityType<RecordStore>().Function("IsHighRated");
            isHighRatedFunction.Returns<bool>();
            isHighRatedFunction.Parameter<int>("minimumRating");
            isHighRatedFunction.Namespace = "AirVinyl.Functions";

            var areRatedByFunction = builder.EntityType<RecordStore>().Collection.Function("AreRatedBy");
            areRatedByFunction.ReturnsCollectionFromEntitySet<RecordStore>("RecordStore");
            areRatedByFunction.CollectionParameter<int>("personIds");
            areRatedByFunction.Namespace = "AirVinyl.Functions";

            var getHighRatedRecordStoresFunction = builder.Function("GetHighRatedRecordStores");
            getHighRatedRecordStoresFunction.Parameter<int>("minimumRating");
            getHighRatedRecordStoresFunction.ReturnsCollectionFromEntitySet<RecordStore>("RecordStores");
            getHighRatedRecordStoresFunction.Namespace = "AirVinyl.Functions";

            var rateAction = builder.EntityType<RecordStore>().Action("Rate");
            rateAction.Returns<bool>();
            rateAction.Parameter<int>("rating");
            rateAction.Parameter<int>("personId");
            rateAction.Namespace = "AirVinyl.Actions";

            var removeRatingsAction = builder.EntityType<RecordStore>().Collection.Action("RemoveRatings");
            removeRatingsAction.Returns<bool>();
            removeRatingsAction.Parameter<int>("personId");
            removeRatingsAction.Namespace = "AirVinyl.Actions";

            var removeRecordStoreRatingsAction = builder.Action("RemoveRecordStoreRatings");
            removeRecordStoreRatingsAction.Parameter<int>("personId");
            removeRecordStoreRatingsAction.Namespace = "AirVinyl.Actions";

            builder.Singleton<Person>("Tim");

            return builder.GetEdmModel();
        }
    }
}
