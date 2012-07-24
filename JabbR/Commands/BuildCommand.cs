using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using JabbR.Models;
using Newtonsoft.Json.Linq;

namespace JabbR.Commands
{
    [Command("build", "")]
    public class BuildCommand : UserCommand
    {
        public override void Execute(CommandContext context, CallerContext callerContext, ChatUser callingUser, string[] args)
        {
            if (args.Length == 0)
            {
                var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string serviceResult = client.GetStringAsync("http://builder/guestAuth/app/rest/buildTypes").Result;

                var jsonResults = JObject.Parse(serviceResult)["buildType"].Children();
                var buildInfo =
                    jsonResults.Select(
                        x =>
                        new
                            {
                                Name = string.Format("{0} (#{1})", x["projectName"].Value<string>(), x["id"].Value<string>().Substring(2)),
                                Description = string.Format("({0}) <a href='{1}'>{1}</a>", x["name"].Value<string>(), x["webUrl"].Value<string>())
                            });

                context.NotificationService.ShowInfo("Available Builds", buildInfo.ToArray());
            }
            else
            {
                var buildInfo = new[]
                                {
                                    new {Name = "help", Description = "Type /help to show the list of commands"},
                                    new
                                        {
                                            Name = "nick",
                                            Description =
                                        "Type /nick [user] [password] to create a user or change your nickname. You can change your password with /nick [user] [oldpassword] [newpassword]"
                                        },
                                    new
                                        {
                                            Name = "join",
                                            Description =
                                        "Type /join [room] [inviteCode] - to join a channel of your choice. If it is private and you have an invite code, enter it after the room name"
                                        }
                                };

                context.NotificationService.ShowInfo("Build Information", buildInfo);
            }
                
            
        }
    }
}