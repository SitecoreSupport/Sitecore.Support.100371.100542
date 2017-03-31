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
            var logger = ApiContainer.GetLogger();
            logger.Info("WebApiInitializer", this);
            GlobalConfiguration.Configuration.Routes.MapHttpRoute("AnalyticsDataApiSupport", "sitecore/api/ao/aggregates/{site}/{segments}/{keys}", new
            {
                controller = "AnalyticsDataSupport",
                action = "GetSupport"
            }, new { keys = "^[^.]+$" });
            ApiContainer.Configuration.GetWebApiConfiguration(logger).Configure(GlobalConfiguration.Configuration);
        }
    }
}