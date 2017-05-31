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
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.ModelBinding;

    [AuthorizedReportingUserFilter]
    public class IntelSupportController : ApiController
    {
        [HttpGet]
        public object Get([ModelBinder(typeof(ViewParameterModelBinder))] ViewParameters viewParams, Guid contactId, string viewId, string id = null)
        {
            object obj2;
            try
            {
                Assert.IsFalse(contactId == Guid.Empty, "A contact Id must be provided.");
                Assert.IsNotNull(viewParams, "Valid parameters must be provided.");
                Assert.IsNotNullOrEmpty(viewId, "The viewId must be specified.");
                viewParams.ContactId = contactId;
                viewParams.ViewName = viewId.ToLowerInvariant();
                viewParams.ViewEntityId = id;
                ResultSet<DataTable> set = CustomerIntelligenceManager.ViewProvider.GenerateContactView(viewParams);
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