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
                teams = teams,
                myTeams = teams.Where(t => t.organisationId == org.id),
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
                teams = teams,
                myTeams = teams.Where(t => t.organisationId == org.id),
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
        public async Task<IActionResult> EditScrim2(int id)
        {
            var Scrims = await api.GetScrims();
            Scrim scrim = Scrims.Where(s => s.id == id).FirstOrDefault();
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
        //public static long UnixTimestampFromDateTime(DateTime date)
        //{
        //    long unixTimestamp = date.Ticks - new DateTime(1970, 1, 1).Ticks;
        //    unixTimestamp /= TimeSpan.TicksPerSecond;
        //    return unixTimestamp;
        //}
        public async Task<IActionResult> ScrimReminder(string raction, string controller, object? routevalues)
        {
            Organisation org = await userManager.GetOrganisation();
            List<Scrim> scrims = await api.GetScrims();
            List<Team> allteams = await api.GetTeams();
            scrims = scrims.Where(s => s.datetime.Date >= DateTime.Today.Date).ToList();
            foreach (Scrim s in scrims)
            {
                if (s.datetime.Date == DateTime.Today.Date)
                {
                    Team[] team = allteams.Where(t => t.id == s.Team1 || t.id == s.Team2).ToArray();
                    List<EmbedFieldBuilder> fields = new()
                    {
                        new EmbedFieldBuilder().WithName("Team1").WithValue(team[0].name).WithIsInline(true),
                        new EmbedFieldBuilder().WithValue("vs").WithIsInline(true),
                        new EmbedFieldBuilder().WithName("Team2").WithValue(team[1].name).WithIsInline(true),
                        new EmbedFieldBuilder().WithName("Time").WithValue(s.datetime.TimeOfDay).WithIsInline(false)
                    };
                    long unix = new DateTimeOffset(s.datetime.Year, s.datetime.Month, s.datetime.Day, s.datetime.Hour, s.datetime.Minute, s.datetime.Second, TimeSpan.Zero).ToUnixTimeSeconds() - 39600;
                    var em = new EmbedBuilder
                    {
                        Description = $"{unix}"

                        // Embed property can be set within object initializer
                    };
                    // Or with methods
                    var Team1Capt = await discord.getUser(team[0].captain, org.guildId);
                    var Team2Capt = await discord.getUser(team[1].captain, org.guildId);
                    string team1bnet; 
                    string team2bnet;
                    try
                    {
                        team1bnet = team[0].players.Where(p => p.discord == $"{Team1Capt.Username}#{Team2Capt.DiscriminatorValue.ToString().PadLeft(4, '0')}").FirstOrDefault().battlenet;
                    } catch
                    {
                        team1bnet = team[0].players.First().battlenet;
                    }
                    try
                    {
                        team2bnet = team[1].players.Where(p => p.discord == $"{Team1Capt.Username}#{Team2Capt.DiscriminatorValue.ToString().PadLeft(4, '0')}").FirstOrDefault().battlenet;
                    }
                    catch
                    {
                        team2bnet = team[1].players.First().battlenet;
                    }
                    em.AddField(team[0].name, team1bnet, true)
                        .WithAuthor("Scrim Tomorrow!")
                        .WithFooter(footer => footer.Text = "FSM scrim management")
                        .WithColor(new Color(0, 255, 255))
                        .WithCurrentTimestamp();
                    em.AddField("Vs", "-", true);
                    em.AddField(team[1].name, team2bnet, true);
                    Embed m = em.Build();
                    try
                    {
                        await discord.SendUserMessage(m, team[0].captain);

                    }
                    catch (Exception ex) { }
                    try
                    {
                        await discord.SendUserMessage(m, team[1].captain);

                    }
                    catch (Exception ex) { }
                }
                if (s.datetime.Date == DateTime.Today.Date.AddDays(1))
                {
                    Team[] team = allteams.Where(t => t.id == s.Team1 || t.id == s.Team2).ToArray();
                    List<EmbedFieldBuilder> fields = new()
                    {
                        new EmbedFieldBuilder().WithName("Team1").WithValue(team[0].name).WithIsInline(true),
                        new EmbedFieldBuilder().WithValue("vs").WithIsInline(true),
                        new EmbedFieldBuilder().WithName("Team2").WithValue(team[1].name).WithIsInline(true),
                        new EmbedFieldBuilder().WithName("Time").WithValue(s.datetime.TimeOfDay).WithIsInline(false)
                    };
                    long unix = new DateTimeOffset(s.datetime.Year, s.datetime.Month, s.datetime.Day, s.datetime.Hour, s.datetime.Minute, s.datetime.Second, TimeSpan.Zero).ToUnixTimeSeconds() - 39600;

                    var em = new EmbedBuilder
                    {
                        Description = $"{unix}"
                        // Embed property can be set within object initializer
                    };
                    // Or with methods
                    var Team1Capt = await discord.getUser(team[0].captain, org.guildId);
                    var Team2Capt = await discord.getUser(team[1].captain, org.guildId);
                    string team1bnet;
                    string team2bnet;
                    try
                    {
                        team1bnet = team[0].players.Where(p => p.discord == $"{Team1Capt.Username}#{Team2Capt.DiscriminatorValue.ToString().PadLeft(4, '0')}").FirstOrDefault().battlenet;
                    }
                    catch
                    {
                        team1bnet = team[0].players.First().battlenet;
                    }
                    try
                    {
                        team2bnet = team[1].players.Where(p => p.discord == $"{Team1Capt.Username}#{Team2Capt.DiscriminatorValue.ToString().PadLeft(4, '0')}").FirstOrDefault().battlenet;
                    }
                    catch
                    {
                        team2bnet = team[1].players.First().battlenet;
                    }
                    em.AddField(team[0].name, team1bnet, true)
                        .WithAuthor("Scrim Tomorrow!")
                        .WithFooter(footer => footer.Text = "FSM scrim management")
                        .WithColor(new Color(0, 255, 255))
                        .WithCurrentTimestamp();
                    em.AddField("Vs", "-", true);
                    em.AddField(team[1].name, team2bnet, true);
                    Embed m = em.Build();
                    try
                    {
                        await discord.SendUserMessage(m, team[0].captain);

                    }
                    catch (Exception ex) { }
                    try
                    {
                        await discord.SendUserMessage(m, team[1].captain);

                    }
                    catch (Exception ex) { }
                }
            }
            return RedirectToAction(raction, controller, routevalues);
        }
        public string getScrimEnemy(Team t, Scrim s, List<Team> teams)
        {
            if (t.id == s.Team1)
            {
                return teams.Where(team => team.id == s.Team2).FirstOrDefault().name;
            } else
            {
                return t.name;
            }
        }
        public async Task<IActionResult> UpdateTimetables(string raction, string controller, object? routevalues)
        {
            var orgs = await api.GetOrganisations();
            var allScrims = await api.GetScrims();
            var allTeams = await api.GetTeams();
            foreach (Organisation o in orgs.Where(o => o.id != new Guid("68ae694f-6055-4348-869d-c22780e255d4")))
            {
                foreach (Team t in o.teams)
                {
                    int today = (int)DateTime.Today.DayOfWeek;
                    today *= -1;
                    List<Scrim> scrims = allScrims.Where(s => s.Team2 == t.id || s.Team1 == t.id).ToList();
                    ulong channel = o.Config.Teams.Where(team => team.Name == t.name).FirstOrDefault().TimetableId;
                    scrims = scrims.OrderBy(s => s.datetime).ToList();
                    scrims.Reverse();
                    scrims = scrims.Where(s => DateTime.Today.CompareTo(s.datetime) <= 0).ToList();
                    await discord.deleteMessages(channel);
                    foreach (Scrim scrim in scrims)
                    { if (scrim.datetime > DateTime.Today.AddDays(7 - (today * -1)))
                        {
                            EmbedBuilder timetable = new EmbedBuilder()
                            {
                                Title = $"{getScrimEnemy(t,scrim,allTeams)}, in {scrim.datetime.Subtract(DateTime.Today).Days} days"
                            };
                            timetable.AddField(scrim.datetime.ToShortTimeString(), scrim.datetime.ToShortDateString());
                            await discord.SendMessage(timetable.Build(), channel);
                        }

                    }
                    scrims.Reverse();
                    scrims = scrims.Where(s => s.datetime >= DateTime.Today.AddDays(today) && s.datetime <= DateTime.Today.AddDays(7 - (today * -1))).ToList();
                    
                    
                    if (channel != 0)
                    {
                        EmbedBuilder week = new EmbedBuilder()
                        {
                            Title = $"{t.name} Week TimeTable"
                        };
                        string[] days = {"Sun", "Mon", "Tues", "Wed", "Thurs", "Fri", "Sat" };
                        foreach (Scrim s in scrims)
                        {
                            week.AddField(days[(int)s.datetime.DayOfWeek], $"{getScrimEnemy(t, s, allTeams)} @ {s.datetime.ToShortTimeString()}", false);
                        }
                        await discord.SendMessage(week.Build(), channel);
                    }
                }
            }
            return RedirectToAction(raction, controller, routevalues);
        }
        public async Task<IActionResult> ScrimDelete(int id)
        {
            await api.DeleteScrim(id, userManager.GetUser());
            return RedirectToAction("Scrims");
        }
    }
}
