using Discord;
using Discord.WebSocket;
using Web2.Helpers;
using Web2.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
//builder.Services.AddSingleton<UserHelper>();
DiscordSocketConfig discordConfig = new DiscordSocketConfig()
{
    GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.DirectMessages,
    MessageCacheSize = 100
};
builder.Services.AddSingleton<DiscordSocketClient>(new DiscordSocketClient(discordConfig));
builder.Services.AddSingleton<DiscordHelper>();
builder.Services.AddSingleton<UserHelper>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Home}/{id?}");

app.Run();
