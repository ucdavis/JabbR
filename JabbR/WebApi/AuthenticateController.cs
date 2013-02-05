using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using JabbR.Infrastructure;
using JabbR.Models;
using JabbR.Services;
using Nancy.Cookies;
using Newtonsoft.Json.Linq;

namespace JabbR.WebApi
{
    public class ShibbolethController : ApiController
    {
        private readonly IChatService _chatService;
        private readonly IJabbrRepository _repository;
        private readonly IAuthenticationTokenService _tokenService;

        public ShibbolethController(IChatService chatService, IJabbrRepository repository, IAuthenticationTokenService tokenService)
        {
            _chatService = chatService;
            _repository = repository;
            _tokenService = tokenService;
        }

        public HttpResponseMessage Get()
        {
            var context = HttpContext.Current;

            var identity = context.Request.ServerVariables["HTTP_REMOTEUSER"];
            var email = context.Request.ServerVariables["HTTP_MAIL"] ?? string.Empty;
            var username = context.Request.ServerVariables["HTTP_DISPLAYNAME"] ?? string.Empty;

            identity = "bdobalina";
            email = "bob@bob.com";
            username = identity;

            if (string.IsNullOrWhiteSpace(identity))
            {
                throw new SecurityException(string.Format("You need to be logged in with Shibboleth! EPPN: {0}, REMOTE_USER: {1}", identity, context.Request.ServerVariables["HTTP_REMOTEUSER"]));
            }

            username = Regex.Replace(
                            string.IsNullOrWhiteSpace(username) ? identity : username.Replace(" ", ""),
                            @"[^A-Za-z0-9]+", "");

            //EnsureUserExists(username, identity, email);
            
            var user = _repository.VerifyUser(identity);
            string userToken = _tokenService.GetAuthenticationToken(user);
            
            var cookie = new CookieHeaderValue(Constants.UserTokenCookie, userToken)
                {
                    HttpOnly = true,
                    Expires = DateTime.Now + TimeSpan.FromDays(30)
                };

            //FormsAuthentication.SetAuthCookie(identity, true);
            //FormsAuthentication.RedirectFromLoginPage(identity, true);
            
            var response = Request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.AddCookies(new[] {cookie});
            response.Headers.Location = new Uri("http://localhost:16207");
            return response;
        }

        private void EnsureUserExists(string username, string identity, string email)
        {
            var identityExists = _repository.Users.Any(u => u.Identity.Equals(identity, StringComparison.OrdinalIgnoreCase));
            if (!identityExists)
            {
                //_chatService.AddUser(username, identity, email);
            }
        }
    }

    public class AuthenticateController : ApiController
    {
        private readonly IMembershipService _membershipService;
        private readonly IAuthenticationTokenService _tokenService;

        public AuthenticateController(IMembershipService membershipService, IAuthenticationTokenService tokenService)
        {
            _membershipService = membershipService;
            _tokenService = tokenService;
        }

        // POST  { username:, password: }
        public async Task<HttpResponseMessage> Post()
        {
            JObject body = null;

            try
            {
                body = await Request.Content.ReadAsAsync<JObject>();

                if (body == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            string username = body.Value<string>("username");
            string password = body.Value<string>("password");

            if (String.IsNullOrEmpty(username))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Missing username");
            }

            if (String.IsNullOrEmpty(password))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Missing password");
            }

            ChatUser user = null;

            try
            {
                user = _membershipService.AuthenticateUser(username, password);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, ex.Message);
            }

            string token = _tokenService.GetAuthenticationToken(user);

            return Request.CreateResponse(HttpStatusCode.OK, token);
        }
    }
}