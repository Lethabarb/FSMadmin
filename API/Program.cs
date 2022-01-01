using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("FSMdb");
builder.Services.AddDbContext<FSMContext>(x => x.UseSqlServer(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
//#######################################
//#############ORGANISATIONS##################
//#######################################
app.MapGet("/Organisation/getall", ([FromServices] FSMContext context) => {
    return context.Set<Organisation>().ToList();
}).AllowAnonymous();
app.MapGet("/Organisation/get/{id}", ([FromServices] FSMContext context, int id) =>
{
    return context.Set<Organisation>().Where(x => x.id == id).FirstOrDefault();
}).AllowAnonymous();
app.MapPost("/Organisation/create", ([FromServices] FSMContext context, Organisation entity) =>
{
    context.Set<Organisation>().Add(entity);
    context.SaveChanges();
}).RequireAuthorization();
app.MapPut("/Organisation/update/{id}", ([FromServices] FSMContext context, Organisation entity, int id) =>
{
    Organisation old = context.Set<Organisation>().Where(x => x.id == id).FirstOrDefault();
    old.name = entity.name;
    old.guildId = entity.guildId;
    old.botConfig = entity.botConfig;
    context.SaveChanges();
}).RequireAuthorization();
app.MapDelete("/Organisation/delete/{id}", ([FromServices] FSMContext context, int id) =>
{
    Organisation entity = context.Set<Organisation>().Where(x => x.id == id).FirstOrDefault();
    context.Set<Organisation>().Remove(entity);
    context.SaveChanges();
}).RequireAuthorization();
//#######################################
//#############PLAYERSS##################
//#######################################
app.MapGet("/Player/getall", ([FromServices] FSMContext context) => {
    return context.Set<Player>().ToList();
}).AllowAnonymous();
app.MapGet("/Player/get/{id}", ([FromServices] FSMContext context, int id) =>
{
    return context.Set<Player>().Where(x => x.id == id).FirstOrDefault();
}).AllowAnonymous();
app.MapPost("/Player/create", ([FromServices] FSMContext context, Player entity) =>
{
    context.Set<Player>().Add(entity);
    context.SaveChanges();
}).RequireAuthorization();
app.MapPut("/Player/update/{id}", ([FromServices] FSMContext context, Player entity, int id) =>
{
    Player old = context.Set<Player>().Where(x => x.id == id).FirstOrDefault();
    old.discord = entity.discord;
    old.battlenet = entity.battlenet;
    old.ulongId = entity.ulongId;
    context.SaveChanges();
}).RequireAuthorization();
app.MapDelete("/Player/delete/{id}", ([FromServices] FSMContext context, int id) =>
{
    Player entity = context.Set<Player>().Where(x => x.id == id).FirstOrDefault();
    context.Set<Player>().Remove(entity);
    context.SaveChanges();
}).RequireAuthorization();

//#######################################
//#############SCRIMS##################
//#######################################
app.MapGet("/Scrim/getall", ([FromServices] FSMContext context) => {
    return context.Set<Scrim>().ToList();
}).RequireAuthorization();
app.MapGet("/Scrim/get/{id}", ([FromServices] FSMContext context, int id) =>
{
    return context.Set<Scrim>().Where(x => x.id == id).FirstOrDefault();
}).RequireAuthorization();
app.MapPost("/Scrim/create", ([FromServices] FSMContext context, Scrim entity) =>
{
    context.Set<Scrim>().Add(entity);
    context.SaveChanges();
}).RequireAuthorization();
app.MapPut("/Scrim/update/{id}", ([FromServices] FSMContext context, Scrim entity, int id) =>
{
    Scrim old = context.Set<Scrim>().Where(x => x.id == id).FirstOrDefault();
    old.Team1 = entity.Team1;
    old.team2 = entity.team2;
    old.datetime = entity.datetime;
    context.SaveChanges();
}).RequireAuthorization();
app.MapDelete("/Scrim/delete/{id}", ([FromServices] FSMContext context, int id) =>
{
    Scrim entity = context.Set<Scrim>().Where(x => x.id == id).FirstOrDefault();
    context.Set<Scrim>().Remove(entity);
    context.SaveChanges();
}).RequireAuthorization();
//#######################################
//#############TEAMS##################
//#######################################

app.MapGet("/Team/getall", ([FromServices] FSMContext context) => {
    return context.Set<Team>().ToList();
}).AllowAnonymous();
app.MapGet("/Team/get/{id}", ([FromServices] FSMContext context, int id) =>
{
    return context.Set<Team>().Where(x => x.id == id).FirstOrDefault();
}).AllowAnonymous();
app.MapPost("/Team/create", ([FromServices] FSMContext context, Team entity) =>
{
    context.Set<Team>().Add(entity);
    context.SaveChanges();
}).RequireAuthorization();
app.MapPut("/Team/update/{id}", ([FromServices] FSMContext context, Team entity, int id) =>
{
    Team old = context.Set<Team>().Where(x => x.id == id).FirstOrDefault();
    old.name = entity.name;
    old.captain = entity.captain;
    old.organisationId = entity.organisationId;
    context.SaveChanges();
}).RequireAuthorization();
app.MapDelete("/Team/delete/{id}", ([FromServices] FSMContext context, int id) =>
{
    Team entity = context.Set<Team>().Where(x => x.id == id).FirstOrDefault();
    context.Set<Team>().Remove(entity);
    context.SaveChanges();
}).RequireAuthorization();


app.MapControllers();

app.Run();
