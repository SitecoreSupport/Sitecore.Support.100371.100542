namespace Sitecore.Support.ExperienceAnalytics.Api.Http
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
  using Sitecore.Support.ExperienceAnalytics.Api.Http.ModelBinding;
  using Sitecore.Xdb.Configuration;
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
    #region Original code

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

    #endregion

    #region Modified code

    [ValidateModelStateFilter, ContextSiteSwicherFilter(SiteName = "shell"), RequirexDbFilter]
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

    #endregion

    #region Added code

    [Serializable]
    internal class BadRequestException : Exception
    {
    }

    [Serializable]
    internal class NotFoundException : Exception
    {
      public NotFoundException(string message)
        : base(message)
      {
      }
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

    internal class RequirexDbFilterAttribute : ActionFilterAttribute
    {
      private readonly ILogger logger;
      private readonly AnalyticsDataSupportController.IXDbSettings settings;

      public RequirexDbFilterAttribute()
        : this(new AnalyticsDataSupportController.XDbSettings(), Logger.Instance)
      {
      }

      public RequirexDbFilterAttribute(AnalyticsDataSupportController.IXDbSettings settings, ILogger logger)
      {
        this.settings = settings;
        this.logger = logger;
      }

      public override void OnActionExecuting(HttpActionContext actionContext)
      {
        if (!this.settings.Enabled)
        {
          this.logger.Info("Access to Experience Analytics denied because xDB is not enabled", this);
          actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
        }
      }
    }

    internal interface IXDbSettings
    {
      bool Enabled { get; }

      bool HasValidLicense { get; }

      string XdbDisabledUrl { get; }
    }

    internal sealed class XDbSettings : AnalyticsDataSupportController.IXDbSettings
    {
      public bool Enabled
      {
        get
        {
          return XdbSettings.Enabled;
        }
      }

      public bool HasValidLicense
      {
        get
        {
          return XdbSettings.HasValidLicense;
        }
      }

      public string XdbDisabledUrl
      {
        get
        {
          return XdbSettings.XdbDisabledUrl;
        }
      }
    }

    #endregion
  }
}