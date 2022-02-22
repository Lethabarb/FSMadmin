using Microsoft.AspNetCore.Mvc;
using System.Web.Helpers;
using Web2.Models;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using Web2.Helpers;

namespace Web2.Controllers
{
    public class OrganisationController : Controller
    {
        private readonly UserHelper UserManager;
        public OrganisationController(UserHelper userHelper)
        {
            UserManager = userHelper;
        }
        public IActionResult Organisations()
        {
            return View();
        }
        public IActionResult CreateOrganisation()
        {
            Organisation org = new()
            {
                name = "",
                shortName = "",
                guildId = 0,
                botConfig = ""
            };
            return View("CreateOrganisation");
        }

        public async Task<IActionResult> OrganisationCreate(IFormFile file, string OrgName = "My Org", string OrgShort = "MO", ulong GuildId = 00000000000000000000, ulong ChannelId = 00000000000000000000)
        {
            if (OrgName == null) OrgName = "My Org";
            if (OrgShort == null)
            {
                OrgShort = String.Join(String.Empty, OrgName.Split(new[] { ' ' }).Select(word => word.First()));
            }
            if (GuildId == null) GuildId = 00000000000000000000;
            if (ChannelId == null) ChannelId = 00000000000000000000;
            Organisation org = new()
            {
                name = OrgName,
                shortName = OrgShort,
                guildId = GuildId,
                botConfig = Newtonsoft.Json.JsonConvert.SerializeObject(new BotConfig()
                {
                    GuildId = GuildId,
                    BotChannel = ChannelId,
                    Teams = new List<TeamConfig>()
                }),
                ImageName = "",
                ImagePath = ""
            };
            if (file != null && file.Length > 0)
            {
                var fileId = Guid.NewGuid();
                var fileName = fileId + Path.GetFileName(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/OrgImages", fileName);
                org.ImageName = fileName;
                org.ImagePath = filePath;

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                Api api = new();
                var res = await api.CreateOrganisation(org, UserManager.GetUser());
                
            }
            return View("~/Views/Home/Home.cshtml");
        }
        public async Task<IActionResult> MyOrganisation()
        {
            
            Organisation org = await UserManager.GetOrganisation();
            if (org.name == null) return View("~/Views/Home/Landing.cshtml", new Donkey() { Value = "Not Signed In"});
            foreach (Team t in org.teams)
            {
                int c = t.players.Count();
                if (c < 6)
                {
                    for (int i = c; i < 6; i++)
                    {
                        t.players = t.players.Append(new()
                        {
                            TeamId = t.id,
                            battlenet = "...",
                            discord = "...",
                            prole = "none"
                        });
                    }
                    //TODO make new player list and set org.players to the new list
                }
                else if (c > 6)
                {
                    while (t.players.Count() >= 6)
                    {
                        t.players = t.players.Where(p => p.id != t.players.Last().id).ToList();
                    }
                    t.players = t.players.Append(new()
                    {
                        TeamId = t.id,
                        battlenet = "+more",
                        discord = "+more",
                        prole = "none"
                    });
                }
            }
            return View("MyOrganisation", org);
        }
        public async Task<IActionResult> UpdateOrganisation(IFormFile file, string OrgName = "My Org", string OrgShort = "MO", ulong GuildId = 00000000000000000000, ulong ChannelId = 00000000000000000000)
        {
            if (OrgName == null) OrgName = "My Org";
            if (OrgShort == null)
            {
                OrgShort = String.Join(String.Empty, OrgName.Split(new[] { ' ' }).Select(word => word.First()));
            }
            if (GuildId == null) GuildId = 00000000000000000000;
            if (ChannelId == null) ChannelId = 00000000000000000000;
            Api api = new();
            //Organisation Old = await UserManager.GetOrganisation();
            //System.IO.File.Delete($"./wwwroot/OrgImages/{Old.ImageName}");
            Organisation org = new()
            {
                name = OrgName,
                shortName = OrgShort,
                guildId = GuildId,
                botConfig = Newtonsoft.Json.JsonConvert.SerializeObject(new BotConfig()
                {
                    GuildId = GuildId,
                    BotChannel = ChannelId,
                    Teams = new List<TeamConfig>()
                }),
                ImageName = "",
                ImagePath = ""
            };
            if (file != null && file.Length > 0)
            {
                var fileId = Guid.NewGuid();
                var fileName = fileId + Path.GetFileName(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/OrgImages", fileName);
                org.ImageName = fileName;
                org.ImagePath = filePath;

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                var UserOrg = await UserManager.GetOrganisation();
                var res = await api.UpdateOrganisation(org, UserOrg.id, UserManager.GetUser());
                org = await UserManager.GetOrganisation();
            }

            return View("MyOrganisation", org);
        }
    }
}
