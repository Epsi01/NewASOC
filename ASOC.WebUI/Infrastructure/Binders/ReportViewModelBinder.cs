using ASOC.WebUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ASOC.Domain;

namespace ASOC.WebUI.Infrastructure.Binders
{
    public class ReportViewModelBinder : IModelBinder
    {
        private const string sessionKey = "Report";

        public object BindModel(ControllerContext controllerContext,
            ModelBindingContext bindingContext)
        {
            IEnumerable<CostViewModel> report = null;
            if (controllerContext.HttpContext.Session != null)
            {
                report = (IEnumerable<CostViewModel>)controllerContext.HttpContext.Session[sessionKey];
            }

            return report;
        }
    }
}