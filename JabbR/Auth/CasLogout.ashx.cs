using System.Web;
using System.Web.Security;

namespace JabbR.Auth
{
    /// <summary>
    /// Summary description for Login
    /// </summary>
    public class CasLogout : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            FormsAuthentication.SignOut();
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}