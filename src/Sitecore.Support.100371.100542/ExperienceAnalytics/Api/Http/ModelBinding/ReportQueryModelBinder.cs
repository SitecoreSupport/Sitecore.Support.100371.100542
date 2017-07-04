namespace Sitecore.Support.ExperienceAnalytics.Api.Http.ModelBinding
{
  using Sitecore.Diagnostics;
  using Sitecore.ExperienceAnalytics.Aggregation.Data.Model;
  using Sitecore.ExperienceAnalytics.Api;
  using Sitecore.ExperienceAnalytics.Api.Encoding;
  using Sitecore.ExperienceAnalytics.Api.Query;
  using Sitecore.ExperienceAnalytics.Core.Diagnostics;
  using Sitecore.ExperienceAnalytics.Core.Repositories.Contracts;
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.InteropServices;
  using System.Web.Http.Controllers;
  using System.Web.Http.ModelBinding;

  public class ReportQueryModelBinder : IModelBinder
  {
    #region Modified code

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
      bindingContext.Model = this.GetModelFromBindingContext(actionContext, bindingContext);
      return true;
    }

    #endregion

    #region Added code
    
    private ReportQuery GetModelFromBindingContext(HttpActionContext actionContext, ModelBindingContext bindingContext)
    {
      try
      {
        Type type = Assembly.GetAssembly(typeof(HashMapper)).GetType("Sitecore.ExperienceAnalytics.Api.Http.ModelBinding.ReportQueryModelBinder");
        object obj2 = Activator.CreateInstance(type);
        object[] parameters = new object[] { actionContext, bindingContext };
        return (ReportQuery)type.GetMethod("GetModelFromBindingContext", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(obj2, parameters);
      }
      catch (Exception exception)
      {
        Log.Error("Fix for issues 100371, 100542. Provided by Sitecore Support.", exception, this);
        return null;
      }
    }

    private static void ValidateModel(ReportQuery reportQuery, HttpActionContext actionContext)
    {
      ICollection<ValidationResult> is2;
      if (!new DataAnnotationsValidator().TryValidate(reportQuery, out is2))
      {
        foreach (ValidationResult result in is2)
        {
          actionContext.ModelState.AddModelError(result.MemberNames.First<string>(), result.ErrorMessage);
        }
      }
    }

    internal class DataAnnotationsValidator
    {
      public bool TryValidate(object @object, out ICollection<ValidationResult> results)
      {
        ValidationContext validationContext = new ValidationContext(@object, null, null);
        results = new List<ValidationResult>();
        return Validator.TryValidateObject(@object, validationContext, results, true);
      }
    }

    #endregion
  }
}