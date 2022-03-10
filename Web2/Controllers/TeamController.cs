using Discord;
using Microsoft.AspNetCore.Mvc;
using Web2.Helpers;
using Web2.Models;

namespace Web2.Controllers
{
    public class TeamController : Controller
    {
        private readonly UserHelper userManager;
        private readonly Api api = new();
        private readonly DiscordHelper discord;
        public TeamController(UserHelper userHelper, DiscordHelper discord)
        {
            userManager = userHelper;
            this.discord = discord;
        }
        public IActionResult CreateTeam()
        {
            return View("CreateTeam", new Team());
        }

        public async Task<IActionResult> TeamCreate(IFormFile file, bool myTeam, string name, ulong Captain, int rank, string Players, ulong TimeTable)
        {
            Api api = new Api();
            Organisation org = new();
            if (myTeam) org = await userManager.GetOrganisation();
            if (!myTeam) org = await api.GetOrganisation(26);

            if (name == null) name = org.shortName + " " + "myTeam";
            if (Players == null) Players = "[]";
            List<Player> players = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Player>>(Players);
            Guid TeamId = Guid.NewGuid();
            
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
            //check captain is in roster
            if (myTeam)
            {
                IUser captain = await discord.getUser(Captain);
                Player captainPlayer;
                try
                {
                    captainPlayer = players.Find(p => p.discord == $"{captain.Username}#{captain.DiscriminatorValue.ToString().PadLeft(4, '0')}");
                } catch
                {
                    team.exception = "captain discord is not in player list";
                    return View("CreateTeam", team);
                }
                if (captainPlayer == null)
                {
                    team.exception = "captain discord is not in player list";
                    return View("CreateTeam", team);
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
            IUser captain = await discord.getUser(Captain);
            Player captainPlayer = players.Find(p => p.discord == $"{captain.Username}#{captain.DiscriminatorValue.ToString().PadLeft(4, '0')}");
            if (captainPlayer == null)
            {
                team.exception = "captain discord is not in player list";
                return View("EditTeam", team);
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
