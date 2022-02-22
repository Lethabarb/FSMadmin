using Discord;
using Microsoft.AspNetCore.Mvc;
using Web2.Helpers;
using Web2.Models;

namespace Web2.Controllers
{
    public class ScrimController : Controller
    {
        private readonly Api api = new();
        private readonly UserHelper userManager;
        private readonly DiscordHelper discord;
        public ScrimController(UserHelper _userManager, DiscordHelper _discord)
        {
            this.userManager = _userManager;
            this.discord = _discord;
        }
        public async Task<IActionResult> Scrims()
        {
            var scrims = await api.GetScrims();
            var teams = await api.GetTeams();
            Organisation org = await userManager.GetOrganisation();
            scrims = scrims.Where(s => teams.Where(t => t.id == s.Team1).FirstOrDefault().organisationId == org.id || teams.Where(t => t.id == s.Team2).FirstOrDefault().organisationId == org.id).ToList();
            ScrimList scrimlist = new()
            {
                scrims = scrims,
                teams = teams.Where(t => t.organisationId == org.id),
            };
            return View("Scrims", scrimlist);
        }
        public async Task<IActionResult> CreateScrim()
        {
            Scrim scrim = new()
            {
                teams = await api.GetTeams()
            };
            return View("CreateScrim", scrim);
        }
        public async Task<IActionResult> ScrimCreate(Guid Team1, Guid Team2, DateTime datetime)
        {
            Scrim scrim = new()
            {
                Team1 = Team1,
                Team2 = Team2,
                datetime = datetime
            };
            await api.CreateScrim(scrim, userManager.GetUser());
            var scrims = await api.GetScrims();
            var teams = await api.GetTeams();
            Organisation org = await userManager.GetOrganisation();
            scrims = scrims.Where(s => teams.Where(t => t.id == s.Team1).FirstOrDefault().organisationId == org.id || teams.Where(t => t.id == s.Team2).FirstOrDefault().organisationId == org.id).ToList();
            ScrimList scrimlist = new()
            {
                scrims = scrims,
                teams = teams.Where(t => t.organisationId == org.id),
            };
            return View("Scrims", scrimlist);
        }
        public async Task<IActionResult> Calender(Guid id)
        {
            var s = await api.GetScrims();
            List<Team> team = new() { await api.GetTeam(id) };
            List<Team> teams = await api.GetTeams();
            teams = teams.Where(t => t.id != id).ToList();
            team.AddRange(teams);
            Scrim model = new()
            {
                teams = team,
                scrims = s.Where(s => s.Team1 == id || s.Team2 == id).ToList(),
            };
            return View("Calender", model);
        }
        public async Task<IActionResult> EditScrim(int day)
        {
            var Scrims = await api.GetScrims();
            Scrim scrim = Scrims.Where(s => s.datetime.Day == day && s.datetime.Month == DateTime.Today.Month && s.datetime.Year == DateTime.Today.Year).FirstOrDefault();
            scrim.teams = await api.GetTeams();
            return View("EditScrim", scrim);
        }
        public async Task<IActionResult> ScrimEdit(int id, Guid Team1, Guid Team2, DateTime datetime)
        {
            Organisation org = await userManager.GetOrganisation();
            await api.UpdateScrim(new Scrim()
            {
                datetime = datetime,
                id = id,
                Team1 = Team1,
                Team2 = Team2,
            }, id, userManager.GetUser());
            var teams = await api.GetTeams();
            teams = teams.Where(t => t.organisationId == org.id).ToList();
            var scrims = await api.GetScrims();
            scrims = scrims.Where(s => teams.Where(t => t.id == s.Team1).FirstOrDefault().organisationId == org.id || teams.Where(t => t.id == s.Team2).FirstOrDefault().organisationId == org.id).ToList();
            ScrimList scrimlist = new()
            {
                scrims = scrims,
                teams = teams.Where(t => t.organisationId == org.id),
            };
            return View("Scrims", scrimlist);
        }
        public async Task<IActionResult> ScrimReminder(string raction, string controller, object? routevalues)
        {
            List<Scrim> scrims = await api.GetScrims();
            scrims = scrims.Where(s => s.datetime.Date >= DateTime.Today.Date).ToList();
            List<Team> teams = await api.GetTeams();
            foreach (Scrim s in scrims)
            {
                if (s.datetime.Date == DateTime.Today.Date)
                {
                    var team = teams.Where(t => t.id == s.Team1 || t.id == s.Team2).ToArray();
                    List<EmbedFieldBuilder> fields = new()
                    {
                        new EmbedFieldBuilder().WithName("Team1").WithValue(team[0].name).WithIsInline(true),
                        new EmbedFieldBuilder().WithValue("vs").WithIsInline(true),
                        new EmbedFieldBuilder().WithName("Team2").WithValue(team[1].name).WithIsInline(true),
                        new EmbedFieldBuilder().WithName("Time").WithValue(s.datetime.TimeOfDay).WithIsInline(false)
                    };
                    EmbedBuilder em = new EmbedBuilder()
                        .WithAuthor("FSM scrim management")
                        .WithColor(new Color(0, 255, 255))
                        .WithCurrentTimestamp()
                        .WithTitle("ScrimNotification")
                        .WithFields(fields);
                    await discord.SendUserMessage(em.Build(), team[0].captain);
                    await discord.SendUserMessage(em.Build(), team[1].captain);
                }
            }
            return RedirectToAction(raction, controller, routevalues);
        }
    }
}
