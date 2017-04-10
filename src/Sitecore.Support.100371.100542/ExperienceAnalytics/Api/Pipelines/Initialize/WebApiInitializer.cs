using Sitecore.ExperienceAnalytics.Api;
using Sitecore.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Sitecore.Support.ExperienceAnalytics.Api.Pipelines.Initialize
{
    public class WebApiInitializer
    {
        public void Process(PipelineArgs args)
        {
            ApiContainer.GetLogger().Info("WebApiInitializer", this);
            HttpRouteCollectionExtensions.MapHttpRoute(GlobalConfiguration.Configuration.Routes,"AnalyticsDataApiSupport", "sitecore/api/ao/aggregates/{site}/{segments}/{keys}", new
            {
                controller = "AnalyticsDataSupport",
                action = "GetSupport"
            }, new
            {
                keys = "^[^.]+$"
            });
            ApiContainer.Configuration.GetWebApiConfiguration().Configure(GlobalConfiguration.Configuration);
        }
    }
}