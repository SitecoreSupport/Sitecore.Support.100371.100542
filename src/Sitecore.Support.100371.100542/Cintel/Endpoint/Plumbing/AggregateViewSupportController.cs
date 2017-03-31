using Sitecore.Cintel;
using Sitecore.Cintel.Configuration;
using Sitecore.Cintel.Diagnostics;
using Sitecore.Cintel.Endpoint.Plumbing;
using Sitecore.Cintel.Endpoint.Transfomers;
using Sitecore.Cintel.Reporting;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace Sitecore.Support.Cintel.Endpoint.Plumbing
{
    [AuthorizedReportingUserFilter]
    public class AggregateViewSupportController : ApiController
    {
        [HttpGet]
        public object Get([ModelBinder(typeof(ViewParameterModelBinder))] ViewParameters viewParams, string viewId)
        {
            object obj2;
            try
            {
                Assert.IsNotNull(viewParams, "Valid parameters must be provided.");
                Assert.IsNotNullOrEmpty(viewId, "The view name must be specified.");
                viewParams.ViewName = viewId;
                var resultSet = CustomerIntelligenceManager.ViewProvider.GenerateAggregateView(viewParams);
                var str = HttpContext.Current.Request.Headers[WellknownIdentifiers.TransfomerClientNameHeader];
                var str2 = HttpContext.Current.Request.Headers[WellknownIdentifiers.TransformerKeyHeader];
                if (!string.IsNullOrEmpty(str) && !string.IsNullOrEmpty(str2))
                    return ResultTransformManager.GetIntelTransformer(str, str2).Transform(resultSet);
                obj2 = resultSet;
            }
            catch (Exception exception)
            {
                var message = exception.Message;
                Logger.Error(message, exception);
                throw new HttpResponseException(base.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, message));
            }
            return obj2;
        }
    }
}