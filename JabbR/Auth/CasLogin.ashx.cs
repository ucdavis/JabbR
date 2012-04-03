using System.Web;
using System.Web.Security;

namespace JabbR.Auth
{
    /// <summary>
    /// Summary description for Login
    /// </summary>
    public class CasLogin : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            CASHelper.Login();
            HttpContext.Current.Response.Redirect("/index.htm", true);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}