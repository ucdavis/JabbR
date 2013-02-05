using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using JabbR.Infrastructure;
using JabbR.Models;
using JabbR.Services;

namespace JabbR.WebApi
{
    public class ShibbolethController : ApiController
    {
        private readonly IJabbrRepository _repository;
        private readonly IAuthenticationTokenService _tokenService;

        public ShibbolethController(IJabbrRepository repository, IAuthenticationTokenService tokenService)
        {
            _repository = repository;
            _tokenService = tokenService;
        }

        public HttpResponseMessage Get(string returnUrl)
        {
            var context = HttpContext.Current;

            var identity = context.Request.ServerVariables["HTTP_REMOTEUSER"];
            var email = context.Request.ServerVariables["HTTP_MAIL"] ?? string.Empty;
            var username = context.Request.ServerVariables["HTTP_DISPLAYNAME"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(identity))
            {
                throw new SecurityException(string.Format("You need to be logged in with Shibboleth! EPPN: {0}, REMOTE_USER: {1}", identity, context.Request.ServerVariables["HTTP_REMOTEUSER"]));
            }

            username = Regex.Replace(
                string.IsNullOrWhiteSpace(username) ? identity : username.Replace(" ", ""),
                @"[^A-Za-z0-9]+", "");
            
            var user = GetUser(username, identity, email);
            string userToken = _tokenService.GetAuthenticationToken(user);
            
            var cookie = new CookieHeaderValue(Constants.UserTokenCookie, userToken)
                {
                    HttpOnly = true,
                    Expires = DateTime.Now + TimeSpan.FromDays(30)
                };
            
            var response = Request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.AddCookies(new[] {cookie});
            
            var url = context.Request.Url.Scheme + "://" + context.Request.Url.Authority;

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                url += returnUrl;
            }

            response.Headers.Location = new Uri(url);
            return response;
        }

        private ChatUser GetUser(string username, string identity, string email)
        {
            var user = _repository.Users.FirstOrDefault(x => x.Identity.Equals(identity, StringComparison.InvariantCultureIgnoreCase));
            
            if (user == null)
            {
                user = new ChatUser
                    {
                        Id = Guid.NewGuid().ToString("d"),
                        Identity = identity,
                        Name = username,
                        Email = email,
                        LastActivity = DateTime.Now
                    };

                _repository.Add(user);
            }

            return user;
        }
    }
}