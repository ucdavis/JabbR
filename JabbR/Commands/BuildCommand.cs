using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using JabbR.Models;
using Newtonsoft.Json.Linq;
using System;

namespace JabbR.Commands
{
    /// <summary>
    /// Usage: /build [show|run] [buildtype-id]
    /// /build: for list of projects with their build configurations and ids
    /// /build show id: to show latest build info for build config with id
    /// /build run id: to run the build configuration
    /// </summary>
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
                var cmd = args[0]; //TODO: change to support different commands
                var id = "bt" + args[1];

                var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string serviceResult =
                    client.GetStringAsync(string.Format("http://builder/guestAuth/app/rest/builds/buildType:(id:{0})", id)).Result;

                var jsonResults = JObject.Parse(serviceResult);
                var buildConfigToken = jsonResults["buildType"];
                var buildConfigInfo = string.Format("Latest {0} build for {1} <a href='{2}'>{2}</a>",
                                                    buildConfigToken["name"].Value<string>(),
                                                    buildConfigToken["projectName"].Value<string>(),
                                                    buildConfigToken["webUrl"].Value<string>());

                var buildInfo = new[]
                                    {
                                        new {Name = "Status", Description = string.Format("[<strong>{0}</strong>] -- {1}", jsonResults["status"].Value<string>(), jsonResults["statusText"].Value<string>())},
                                        new {Name = "Details", Description = string.Format("<a href='{0}'>{0}</a>", jsonResults["webUrl"].Value<string>())}
                                    };

                context.NotificationService.ShowInfo(buildConfigInfo, buildInfo);
            }
        }
    }
}