using Microsoft.Owin;
using Owin;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(fyptest.Startup))]

namespace fyptest
{
  public class Startup
  {
    public void Configuration(IAppBuilder app)
    {
      app.MapSignalR();
      //app.Use(async (context, next) =>
      //{
      //  await next();

      //  var path = context.Request.Path.Value;
      //  // If there's no available file and the request doesn't contain an extension, we're probably trying to access a page
      //  if (context.Response.StatusCode == (int)HttpStatusCode.NotFound && !Path.HasExtension(path) && !path.StartsWith("/api"))
      //  {
      //    const string V = "/JobProvider/RegisterProvider";
      //    context.Request.Path = V; // attempts to redirect to the URL within the SPA
      //    context.Response.StatusCode = (int)HttpStatusCode.OK; // Make sure we update the status code, otherwise it returns 404
      //    await next();
      //  }
      //});
    }
  }
}
