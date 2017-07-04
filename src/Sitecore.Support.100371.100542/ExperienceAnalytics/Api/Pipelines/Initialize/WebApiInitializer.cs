namespace Sitecore.Support.ExperienceAnalytics.Api.Pipelines.Initialize
{
  using Sitecore.ExperienceAnalytics.Api;
  using Sitecore.ExperienceAnalytics.Core.Diagnostics;
  using Sitecore.Pipelines;
  using System;
  using System.Web.Http;

  internal class WebApiInitializer
  {
    private readonly ILogger logger;

    public WebApiInitializer(ILogger logger)
    {
      this.logger = logger;
    }

    public void Process(PipelineArgs args)
    {
      this.logger.Info("WebApiInitializer", this);

      ///// Fix issue////////
      HttpRouteCollectionExtensions.MapHttpRoute(GlobalConfiguration.Configuration.Routes, "AnalyticsDataApiSupport", "sitecore/api/ao/aggregates/{site}/{segments}/{keys}", new
      {
        controller = "AnalyticsDataSupport",
        action = "GetSupport"
      }, new { keys = "^[^.]+$" });
      //////////////////////

      ApiContainer.Configuration.GetWebApiConfiguration(this.logger).Configure(GlobalConfiguration.Configuration);
    }
  }
}