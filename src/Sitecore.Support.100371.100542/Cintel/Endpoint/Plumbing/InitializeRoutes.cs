using Sitecore.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace Sitecore.Support.Cintel.Endpoint.Plumbing
{
    public class InitializeRoutes : Sitecore.Cintel.Endpoint.Plumbing.InitializeRoutes
    {
        public override void Process(PipelineArgs args)
        {
            RouteTable.Routes.MapHttpRoute("cintel_aggregate_views_support", "sitecore/api/ao/v1/aggregates/{viewId}", new
            {
                controller = "AggregateViewSupport",
                action = "Get"
            });
            RouteTable.Routes.MapHttpRoute("cintel_contact_intel_views_support",
              "sitecore/api/ao/v1/contacts/{contactId}/intel/{viewId}/{id}", new
              {
                  controller = "IntelSupport",
                  id = RouteParameter.Optional
              });
            RegisterRoutes(RouteTable.Routes, args);
        }
    }
}