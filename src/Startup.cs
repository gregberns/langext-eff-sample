using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
// using static Logging.Logger;
using Microsoft.Extensions.Logging;
// using Prometheus;
// using CorrelationId;
// using CorrelationId.DependencyInjection;
// using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;

namespace LangExtEffSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // services.AddSwaggerGen();
            // services.AddSwaggerGen(c =>
            // {
            //     c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Plankk API", Version = "v1" });
            //     // var basePath = PlatformServices.Default.Application.ApplicationBasePath;
            //     // var xmlPath = Path.Combine(basePath, "SampleApi.xml");
            //     // c.IncludeXmlComments(xmlPath);
            // });

            // services.AddAuthentication("BasicAuthentication")
            //     .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

            // services.AddTransient(typeof(Connection), (_collection) => new Connection(Program.Configuration));

            // https://github.com/stevejgordon/CorrelationId/blob/master/samples/3.1/MvcSample/Startup.cs#L32
            // services.AddDefaultCorrelationId(options =>
            // {
            //     options.CorrelationIdGenerator = () => Guid.NewGuid().ToString();
            //     options.AddToLoggingScope = true;
            //     options.EnforceHeader = false;
            //     options.IgnoreRequestHeader = false;
            //     options.IncludeInResponse = true;
            //     options.RequestHeader = "X-Correlation-Id";
            //     options.ResponseHeader = "X-Correlation-Id";
            //     options.UpdateTraceIdentifier = false;
            // });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // Log.LogInformation("In Dev");
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Log.LogInformation("In Prod");
            }

            // app.UseSwagger(c =>
            // {
            //     c.RouteTemplate = "swagger/{documentName}/swagger.json";
            // });

            // app.UseSwaggerUI(c =>
            // {
            //     // This uses a relative path which helps with the reverse proxy url re-writing
            //     c.SwaggerEndpoint("v1/swagger.json", "Plankk API");
            // });

            app.UseRouting();

            // app.UseMetricServer();
            // app.UseCorrelationId();

            // Must run after `UseCorrelationId()`
            app.UseRequestResponseLogging();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                // endpoints.MapMetrics();
            });
        }
    }
    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
    public class RequestResponseLoggingMiddleware
    {
        // Comes from: https://github.com/elanderson/ASP.NET-Core-Basics-Refresh/commit/c1b24de0d44dfc45d379b91d721842656c4ba3d8
        private readonly RequestDelegate _next;
        // private readonly ILogger _logger;
        // private readonly ICorrelationContextAccessor _correlationContextAccessor;

        public RequestResponseLoggingMiddleware(
            RequestDelegate next
            //, ICorrelationContextAccessor correlationContextAccessor
            )
        {
            _next = next;
            // _logger = Logging.Logger.Log;
            // _correlationContextAccessor = correlationContextAccessor;
        }
        public async Task Invoke(HttpContext context)
        {
            await LogResponse(context);
        }
        private async Task LogResponse(HttpContext context)
        {
            await _next(context);

            //https://en.wikipedia.org/wiki/Common_Log_Format
            // var clientIp = context.Connection.RemoteIpAddress;
            // // var dateReceived = context.Request.
            // var method = context.Request.Method;
            // var path = context.Request.Path;
            // var protocol = context.Request.Protocol;
            // var statusCode = context.Response.StatusCode;
            // var contentLength = context.Response.ContentLength;
            // var correlationId = _correlationContextAccessor.CorrelationContext.CorrelationId;

            // _logger.LogInformation($"{clientIp} - - [] \"{method} {path} {protocol}\" {statusCode} {contentLength} {correlationId}");
        }
    }

    // public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    // {
    //     private readonly Configuration _configuration;
    //     // private readonly ICorrelationContextAccessor _correlationContextAccessor;

    //     public BasicAuthenticationHandler(
    //         IOptionsMonitor<AuthenticationSchemeOptions> options,
    //         ILoggerFactory logger,
    //         UrlEncoder encoder,
    //         ISystemClock clock
    //         // ,ICorrelationContextAccessor correlationContextAccessor
    //         )
    //         : base(options, logger, encoder, clock)
    //     {
    //         // _configuration = Program.Configuration;
    //         // _correlationContextAccessor = correlationContextAccessor;
    //     }

    //     protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    //     {
    //         // skip authentication if endpoint has [AllowAnonymous] attribute
    //         var endpoint = Context.GetEndpoint();
    //         if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
    //             return AuthenticateResult.NoResult();

    //         if (!Request.Headers.ContainsKey("Authorization"))
    //             return AuthenticateResult.Fail("Missing Authorization Header");

    //         try
    //         {
    //             var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
    //             var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
    //             var credentials = System.Text.Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
    //             var username = credentials[0];
    //             var password = credentials[1];

    //             if (_configuration.BasicAuthUsername == username && _configuration.BasicAuthPassword == password)
    //             {
    //                 var claims = new[] {
    //                     new Claim(ClaimTypes.NameIdentifier, ""),
    //                     new Claim(ClaimTypes.Name, ""),
    //                 };
    //                 var identity = new ClaimsIdentity(claims, Scheme.Name);
    //                 var principal = new ClaimsPrincipal(identity);
    //                 var ticket = new AuthenticationTicket(principal, Scheme.Name);
    //                 return AuthenticateResult.Success(ticket);
    //             }
    //             else
    //             {
    //                 var passLen = password == null ? 0 : password.Length;
    //                 //Log.LogWarning($"BasicAuth - Invalid Username or Password - Username: {username}, Password Length: {passLen}, CorrelationId: {_correlationContextAccessor.CorrelationContext.CorrelationId}");

    //                 // Sleep just to try and maybe slow an attack
    //                 await Task.Delay(2000);

    //                 return AuthenticateResult.Fail("Invalid Username or Password");
    //             }
    //         }
    //         catch
    //         {
    //             //Log.LogWarning($"BasicAuth - Invalid Authorization Header, CorrelationId: {_correlationContextAccessor.CorrelationContext.CorrelationId}");
    //             return AuthenticateResult.Fail("Invalid Authorization Header");
    //         }
    //     }
    // }
}
