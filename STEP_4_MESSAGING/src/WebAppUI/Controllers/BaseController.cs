using CnAppForAzureDev.Managers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnAppForAzureDev.Controllers
{
    public class BaseController: Controller
    {
        public readonly IndustryManager _industryManager;

        public BaseController(IndustryManager industryManager)
        {
            
            
            _industryManager = industryManager;
            var industry = _industryManager.GetIndustry();
            ViewBag.Industry = industry;
        }
        //public override void OnActionExecuting(ActionExecutingContext context)
        //{
        //    //_industryManager.SetIndustry(Request);
        //    //var industry = _industryManager.GetIndustry();

        //    //ViewBag.Industry = industry;
        //    base.OnActionExecuting(context);
        //}
        
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var industry = _industryManager.GetIndustry();

            ViewBag.Industry = industry;

            base.OnActionExecuted(context);
        }
    }
}
