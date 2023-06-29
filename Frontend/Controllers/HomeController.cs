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
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> Index()
        {
            return View(new GenericViewModel());
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

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}