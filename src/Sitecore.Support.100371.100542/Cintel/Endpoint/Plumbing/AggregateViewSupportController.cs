namespace Sitecore.Support.Cintel.Endpoint.Plumbing
{
    using Sitecore.Cintel;
    using Sitecore.Cintel.Commons;
    using Sitecore.Cintel.Configuration;
    using Sitecore.Cintel.Diagnostics;
    using Sitecore.Cintel.Endpoint.Plumbing;
    using Sitecore.Cintel.Endpoint.Transfomers;
    using Sitecore.Cintel.Reporting;
    using Sitecore.Diagnostics;
    using System;
    using System.Data;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.ModelBinding;

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
                ResultSet<DataTable> set = CustomerIntelligenceManager.ViewProvider.GenerateAggregateView(viewParams);
                string str = HttpContext.Current.Request.Headers[WellknownIdentifiers.TransfomerClientNameHeader];
                string str2 = HttpContext.Current.Request.Headers[WellknownIdentifiers.TransformerKeyHeader];
                if (!string.IsNullOrEmpty(str) && !string.IsNullOrEmpty(str2))
                {
                    return ResultTransformManager.GetIntelTransformer(str, str2).Transform(set);
                }
                obj2 = set;
            }
            catch (Exception exception)
            {
                string message = exception.Message;
                Logger.Error(message, exception);
                throw new HttpResponseException(HttpRequestMessageExtensions.CreateErrorResponse(base.Request, HttpStatusCode.InternalServerError, message));
            }
            return obj2;
        }
    }
}