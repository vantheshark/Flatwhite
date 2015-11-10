using System.Collections.Generic;
using System.Web.Http.Controllers;

namespace Flatwhite.WebApi
{
    public class WebApiContextProvider : IContextProvider
    {
        private readonly HttpActionContext _httpActionContext;

        public WebApiContextProvider(HttpActionContext httpActionContext)
        {
            _httpActionContext = httpActionContext;
        }

        public IDictionary<string, object> GetContext()
        {
            var result = new Dictionary<string, object>
            {
                {"__webApi", true},
                {"headers", _httpActionContext.Request.Headers},
                {"method", _httpActionContext.Request.Method},
                {"requestUri", _httpActionContext.Request.RequestUri}
            };
            return result;
        }
    }
}