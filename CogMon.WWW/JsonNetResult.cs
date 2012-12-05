using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Web.Mvc;

namespace CogMon.WWW
{
    public class JsonNetResult : ActionResult
    {
        private object _obj;
        public JsonNetResult(object obj)
        {
            _obj = obj;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "text/json";
            context.HttpContext.Response.Write(JsonConvert.SerializeObject(_obj, new Newtonsoft.Json.Converters.IsoDateTimeConverter()));
        }
    }
}