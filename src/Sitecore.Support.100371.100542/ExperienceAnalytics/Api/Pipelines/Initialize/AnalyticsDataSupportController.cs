namespace Sitecore.Support.ExperienceAnalytics.Api.Pipelines.Initialize
{
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
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;
    using System.Web.Http.ModelBinding;

    public class AnalyticsDataSupportController : AnalyticsDataController
    {
        private readonly IEncoder<ReportResponse> encoder;
        private readonly ILogger logger;
        private readonly IReportingService reportingService;

        public AnalyticsDataSupportController()
            : this(ApiContainer.Repositories.GetReportingService(), ApiContainer.GetLogger(), ApiContainer.GetReportResponseEncoder())
        {
        }

        public AnalyticsDataSupportController(IReportingService reportingService, ILogger logger, IEncoder<ReportResponse> encoder)
            : base(reportingService, logger, encoder)
        {
            this.reportingService = reportingService;
            this.logger = logger;
            this.encoder = encoder;
        }

        [ValidateModelStateFilter, ContextSiteSwicherFilter(SiteName = "shell")]
        public IHttpActionResult GetSupport([ModelBinder(typeof(ReportQueryModelBinder))] ReportQuery reportQuery)
        {
            try
            {
                ReportResponse reportResults = this.encoder.Encode(this.reportingService.RunQuery(reportQuery));
                this.LogMessages(reportResults);
                return this.Ok<ReportResponse>(reportResults);
            }
            catch (BadRequestException exception)
            {
                this.logger.Warn(exception.ToString());
                return this.BadRequest(exception.Message);
            }
            catch (NotFoundException exception2)
            {
                this.logger.Info(exception2.Message);
                return this.NotFound();
            }
            catch (Exception exception3)
            {
                this.logger.Error(exception3.ToString());
                return this.InternalServerError();
            }
        }

        private void LogMessages(ReportResponse reportResults)
        {
            if (reportResults.Messages != null)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Request: {base.Request.RequestUri} returned messages.");
                foreach (Message message in reportResults.Messages)
                {
                    builder.AppendLine(message.Text);
                }
                this.logger.Info(builder.ToString());
            }
        }

        [Serializable]
        internal class BadRequestException : Exception
        {
        }

        internal class ContextSiteSwicherFilterAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuted(HttpActionExecutedContext filterContext)
            {
                Switcher<Sitecore.Sites.SiteContext, Sitecore.Sites.SiteContextSwitcher>.Exit();
            }

            public override void OnActionExecuting(HttpActionContext filterContext)
            {
                Assert.IsNotNullOrEmpty(this.SiteName, "site name must be provided");
                Switcher<Sitecore.Sites.SiteContext, Sitecore.Sites.SiteContextSwitcher>.Enter(Factory.GetSite(this.SiteName));
            }

            public string SiteName { get; set; }
        }

        [Serializable]
        internal class NotFoundException : Exception
        {
            public NotFoundException(string message)
                : base(message)
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
                {
                    throw new ArgumentNullException("textDecoder");
                }
                if (logger == null)
                {
                    throw new ArgumentNullException("logger");
                }
                this.keyTopDefaultValue = keyTopDefaultValue;
                this.textDecoder = textDecoder;
                this.logger = logger;
            }

            public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
            {
                if (bindingContext.ModelType != typeof(ReportQuery))
                {
                    return false;
                }
                Type[] types = new Type[] { typeof(int), typeof(IDecoder<string>), typeof(ILogger) };
                ConstructorInfo constructor = typeof(IParameterBinder).Assembly.GetType("Sitecore.ExperienceAnalytics.Api.Http.ModelBinding.ReportQueryModelBinder").GetConstructor(types);
                if (constructor != null)
                {
                    object[] parameters = new object[] { this.keyTopDefaultValue, this.textDecoder, this.logger };
                    object obj2 = constructor.Invoke(parameters);
                    object[] objArray2 = new object[] { actionContext, bindingContext };
                    bindingContext.Model = obj2.GetType().GetMethod("GetModelFromBindingContext", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(obj2, objArray2);
                }
                return true;
            }
        }

        internal class ValidateModelStateFilterAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(HttpActionContext actionContext)
            {
                if (!actionContext.ModelState.IsValid)
                {
                    actionContext.Response = HttpRequestMessageExtensions.CreateErrorResponse(actionContext.Request, HttpStatusCode.BadRequest, actionContext.ModelState);
                }
            }
        }
    }
}