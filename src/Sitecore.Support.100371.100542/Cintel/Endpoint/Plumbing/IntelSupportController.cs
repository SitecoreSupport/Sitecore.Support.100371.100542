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
    public class IntelSupportController : ApiController
    {
        [HttpGet]
        public object Get([ModelBinder(typeof(ViewParameterModelBinder))] ViewParameters viewParams, Guid contactId,
          string viewId, string id = null)
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
                var resultSet = CustomerIntelligenceManager.ViewProvider.GenerateContactView(viewParams);
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