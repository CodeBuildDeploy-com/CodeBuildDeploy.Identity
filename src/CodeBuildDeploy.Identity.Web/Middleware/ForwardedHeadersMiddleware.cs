using System.Linq;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;

using Serilog;

namespace CodeBuildDeploy.Identity.Web.Middleware
{
    internal static class ForwardedHeadersMiddleware
    {
        public static IApplicationBuilder UseForwardedProtoHeader(this IApplicationBuilder app)
        {
            app.Use((context, next) =>
            {
                foreach (var header in context.Request.Headers)
                {
                    if (header.Key.StartsWith("X-Forwarded-Proto"))
                    {
                        context.Request.Scheme = header.Value.First()!;
                        Log.Information("X-Forwarded-Proto: {Value}", header.Value);
                    }
                }

                return next();
            });

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
            });

            return app;
        }
    }
}
