using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet_graph.Models;

namespace WebApp_OpenIDConnect_DotNet_graph.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        // /// </summary>
        // static readonly string[] scopeRequiredByApi = new string[] { "access_as_user" };

        // static readonly string[] scopesToAccessDownstreamApi = new string[] { "api://MyTodolistService/access_as_user" };

        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        readonly IAuthorizationHeaderProvider authorizationHeaderProvider;

        public HomeController(ILogger<HomeController> logger,
                            IConfiguration configuration,
                            IAuthorizationHeaderProvider authorizationHeaderProvider)
        {
            _logger = logger;
            _configuration = configuration;

            this.authorizationHeaderProvider = authorizationHeaderProvider;

            // Capture the Scopes for Graph that were used in the original request for an Access token (AT) for MS Graph as
            // they'd be needed again when requesting a fresh AT for Graph during claims challenge processing
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> Index()
        {

                var app = ConfidentialClientApplicationBuilder.Create("bfb7db00-f29d-484c-9bc6-c8abc75f12d4")
                .WithClientSecret("meX8Q~s_S_ZHPbluaIN9gW54GPn85aM.uHCE1ccK")
                .WithAuthority(new Uri($"https://login.microsoftonline.com/16b3c013-d300-468d-ac64-7eda0820b6d3"))
                .Build();

            string[] scopes = _configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');
            var token = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.AccessToken}");
            var response = await client.GetAsync("http://localhost:5114/get");
            Console.WriteLine(response.StatusCode);
            Console.WriteLine(token.AccessToken);
            
            var content = await response.Content.ReadAsStringAsync();

            return View(new GenericViewModel { Content = content});
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> Profile()
        {
            // Get an authorization header.
            string[] scopes = _configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');
            string authorizationHeader = await authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(scopes);
            
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", authorizationHeader);
            var response = await client.GetAsync("http://localhost:5114/get");
            Console.WriteLine(response.StatusCode);
            Console.WriteLine(authorizationHeader);
            
            var content = await response.Content.ReadAsStringAsync();
            
            return View(new GenericViewModel { Content = content});
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}