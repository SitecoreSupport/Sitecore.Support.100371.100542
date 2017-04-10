using Sitecore.Common;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.ExperienceAnalytics.Api;
using Sitecore.ExperienceAnalytics.Api.Encoding;
using Sitecore.ExperienceAnalytics.Api.Http;
using Sitecore.ExperienceAnalytics.Api.Http.ModelBinding;
using Sitecore.ExperienceAnalytics.Api.Query;
using Sitecore.ExperienceAnalytics.Api.Response;
using Sitecore.ExperienceAnalytics.Core.Diagnostics;
using Sitecore.Sites;
using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;

namespace Sitecore.Support.ExperienceAnalytics.Api.Pipelines.Initialize
{
    public class AnalyticsDataSupportController : AnalyticsDataController
    {
        private readonly IEncoder<ReportResponse> encoder;
        private readonly Sitecore.ExperienceAnalytics.Core.Diagnostics.ILogger logger;
        private readonly IReportingService reportingService;

        public AnalyticsDataSupportController()
          : this(
            ApiContainer.Repositories.GetReportingService(), ApiContainer.GetLogger(),
            ApiContainer.GetReportResponseEncoder())
        {
        }

        public AnalyticsDataSupportController(IReportingService reportingService, ILogger logger,
          IEncoder<ReportResponse> encoder) : base(reportingService, logger, encoder)
        {
            this.reportingService = reportingService;
            this.logger = logger;
            this.encoder = encoder;
        }

        [ValidateModelStateFilter]
        [ContextSiteSwicherFilter(SiteName = "shell")]
        public IHttpActionResult GetSupport([ModelBinder(typeof(ReportQueryModelBinder))] ReportQuery reportQuery)
        {
            try
            {
                var reportResults = encoder.Encode(reportingService.RunQuery(reportQuery));
                LogMessages(reportResults);
                return Ok(reportResults);
            }
            catch (BadRequestException exception)
            {
                logger.Warn(exception.ToString());
                return BadRequest(exception.Message);
            }
            catch (NotFoundException exception2)
            {
                logger.Info(exception2.Message);
                return NotFound();
            }
            catch (Exception exception3)
            {
                logger.Error(exception3.ToString());
                return InternalServerError();
            }
        }

        private void LogMessages(ReportResponse reportResults)
        {
            if (reportResults.Messages != null)
            {
                var builder = new StringBuilder();
                builder.AppendLine("Request: {base.Request.RequestUri} returned messages.");
                foreach (var message in reportResults.Messages)
                    builder.AppendLine(message.Text);
                logger.Info(builder.ToString());
            }
        }

        [Serializable]
        internal class BadRequestException : Exception
        {
        }

        internal class ContextSiteSwicherFilterAttribute : ActionFilterAttribute
        {
            public string SiteName { get; set; }

            public override void OnActionExecuted(HttpActionExecutedContext filterContext)
            {
                Switcher<SiteContext, SiteContextSwitcher>.Exit();
            }

            public override void OnActionExecuting(HttpActionContext filterContext)
            {
                Assert.IsNotNullOrEmpty(SiteName, "site name must be provided");
                Switcher<SiteContext, SiteContextSwitcher>.Enter(Factory.GetSite(SiteName));
            }
        }

        [Serializable]
        internal class NotFoundException : Exception
        {
            public NotFoundException(string message) : base(message)
            {
            }
        }

        internal class ReportQueryModelBinder : IModelBinder
        {
            private readonly int keyTopDefaultValue;
            private readonly ILogger logger;
            private readonly IDecoder<string> textDecoder;

            public ReportQueryModelBinder()
              : this(Config.KeysTopDefault, ApiContainer.GetKeyCodec(), ApiContainer.GetLogger())
            {
            }

            public ReportQueryModelBinder(int keyTopDefaultValue, IDecoder<string> textDecoder, ILogger logger)
            {
                if (textDecoder == null)
                    throw new ArgumentNullException("textDecoder");
                if (logger == null)
                    throw new ArgumentNullException("logger");
                this.keyTopDefaultValue = keyTopDefaultValue;
                this.textDecoder = textDecoder;
                this.logger = logger;
            }

            public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
            {
                if (bindingContext.ModelType != typeof(ReportQuery))
                    return false;
                var constructorInfo =
                  typeof(IParameterBinder).Assembly.GetType(
                      "Sitecore.ExperienceAnalytics.Api.Http.ModelBinding.ReportQueryModelBinder")
                    .GetConstructor(new[] { typeof(int), typeof(IDecoder<string>), typeof(ILogger) });
                if (constructorInfo == null) return true;
                var obj2 = constructorInfo.Invoke(new object[] { keyTopDefaultValue, textDecoder, logger });
                bindingContext.Model =
                  obj2.GetType()
                    .GetMethod("GetModelFromBindingContext", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(obj2, new object[] { actionContext, bindingContext });
                return true;
            }
        }

        internal class ValidateModelStateFilterAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(HttpActionContext actionContext)
            {
                if (!actionContext.ModelState.IsValid)
                    actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                      actionContext.ModelState);
            }
        }
    }
}