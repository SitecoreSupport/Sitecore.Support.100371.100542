namespace Sitecore.Support.Cintel.Endpoint.Plumbing
{
    using Sitecore.Pipelines;
    using System;
    using System.Web;
    using System.Web.Http;
    using System.Web.Routing;

    public class InitializeRoutes : Sitecore.Cintel.Endpoint.Plumbing.InitializeRoutes
    {
        public override void Process(PipelineArgs args)
        {
            RouteCollectionExtensions.MapHttpRoute(RouteTable.Routes, "cintel_aggregate_views_support", "sitecore/api/ao/v1/aggregates/{viewId}", new
            {
                controller = "AggregateViewSupport",
                action = "Get"
            });

            RouteCollectionExtensions.MapHttpRoute(RouteTable.Routes, "cintel_contact_intel_views_support", "sitecore/api/ao/v1/contacts/{contactId}/intel/{viewId}/{id}", new
            {
                controller = "IntelSupport",
                id = RouteParameter.Optional
            });

            RegisterRoutes(RouteTable.Routes, args);
        }
    }
}