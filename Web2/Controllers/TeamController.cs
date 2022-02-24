using Microsoft.AspNetCore.Mvc;
using Web2.Helpers;
using Web2.Models;

namespace Web2.Controllers
{
    public class TeamController : Controller
    {
        private readonly UserHelper userManager;
        private readonly Api api = new();
        public TeamController(UserHelper userHelper)
        {
            userManager = userHelper;
        }
        public IActionResult CreateTeam()
        {
            return View("CreateTeam", new Team());
        }

        public async Task<IActionResult> TeamCreate(IFormFile file, string name = "myTeam", ulong Captain = 00000000000000000000, int rank = 0, string Players = "[]", ulong TimeTable = 00000000000000000000)
        {
            Organisation org = await userManager.GetOrganisation();
            if (name == null) name = org.shortName + " " + "myTeam";
            if (Captain == null) Captain = 00000000000000000000;
            if (rank == null) rank = 0;
            if (Players == null) Players = "[]";
            if (TimeTable == null) TimeTable = 00000000000000000000;
            List<Player> players = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Player>>(Players);
            Guid TeamId = Guid.NewGuid();
            Api api = new Api();
            foreach (Player p in players)
            {
                p.TeamId = TeamId;
                var res1 = await api.CreatePlayer(p, userManager.GetUser());
            }
            Team team = new()
            {
                id = TeamId,
                name = $"[{org.shortName}] {name}",
                captain = Captain,
                organisationId = org.id,
                rank = rank,
                ImagePath = "",
                ImageName = ""
            };
            if (file != null && file.Length > 0)
            {
                var fileId = Guid.NewGuid();
                var fileName = fileId + Path.GetFileName(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/TeamImages", fileName);
                team.ImageName = fileName;
                team.ImagePath = filePath;

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                

            }
            var res = await api.CreateTeam(team, userManager.GetUser());
            BotConfig oldConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<BotConfig>(org.botConfig);
            oldConfig.Teams.Add(new TeamConfig()
            {
                CaptainId = Captain,
                TimetableId = TimeTable,
                Name = $"[{org.shortName}] {name}"
            });
            org.botConfig = Newtonsoft.Json.JsonConvert.SerializeObject(oldConfig);
            await api.UpdateOrganisation(org, org.id, userManager.GetUser());

            return View("~/Views/Home/Home.cshtml");
        }
        public async Task<IActionResult> EditTeam(Guid id)
        {
            List<Team> teams = await api.GetTeams();
            Team team = teams.Where(t => t.id == id).FirstOrDefault();
            return View("EditTeam", team);

        }
        public async Task<IActionResult> TeamEdit(Guid id, string oldplayers, string oldname, IFormFile file, string name, ulong Captain, int rank, string Players , ulong TimeTable)
        {
            Organisation org = await userManager.GetOrganisation();
            if (name == null) name = $"[{org.shortName}] myTeam";
            if (Players == null) Players = "[]";
            List<Player> oldPlayers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Player>>(oldplayers);
            List<Player> players = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Player>>(Players);
            Api api = new Api();
            foreach (Player p in oldPlayers)
            {
                await api.DeletePlayer(p.id, userManager.GetUser());
            }
            foreach (Player p in players)
            {
                p.TeamId = id;
                var res = await api.CreatePlayer(p, userManager.GetUser());
            }
            Team team = new()
            {
                id = id,
                name = $"[{org.shortName}] {name}",
                captain = Captain,
                organisationId = org.id,
                rank = rank,
                ImageName = "",
                ImagePath = ""
            };
            if (file != null && file.Length > 0)
            {
                var fileId = Guid.NewGuid();
                var fileName = fileId + Path.GetFileName(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/TeamImages", fileName);
                team.ImageName = fileName;
                team.ImagePath = filePath;

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            var res2 = await api.UpdateTeam(team, id, userManager.GetUser());
            BotConfig oldConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<BotConfig>(org.botConfig);
            oldConfig.Teams.Remove(oldConfig.Teams.Where(t => t.Name == oldname).FirstOrDefault());
            oldConfig.Teams.Add(new TeamConfig()
            {
                CaptainId = Captain,
                TimetableId = TimeTable,
                Name = $"[{org.shortName}] {name}"
            });
            org.botConfig = Newtonsoft.Json.JsonConvert.SerializeObject(oldConfig);
            await api.UpdateOrganisation(org, org.id, userManager.GetUser());

            return View("~/Views/Home/Landing.cshtml", new Donkey()
            {
                Value = "Team Updated"
            });
        }
        public async Task<IActionResult> TeamDelete(Guid id)
        {
            await api.DeleteTeam(id, userManager.GetUser());
            return View("~/Views/Home/Landing.cshtml", new Donkey()
            {
                Value = "Team Deleted"
            });
        }
    }
}
