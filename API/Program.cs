using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Security.Cryptography;
using System.Web.Http;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("FSMdb");
builder.Services.AddDbContext<FSMContext>(x => x.UseSqlServer(connectionString));

builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        Type = SecuritySchemeType.Http
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateActor = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
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

string Login(UserLogin user, IUserService service, FSMContext context)
{
    if (!string.IsNullOrEmpty(user.Email) &&
        !string.IsNullOrEmpty(user.Password))
    {
        var loggedInUser = service.Get(user, context.Set<User>().ToList());
        if (loggedInUser is null) return "User not found";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, loggedInUser.Username),
            new Claim(ClaimTypes.Email, loggedInUser.EmailAddress),
            new Claim(ClaimTypes.Role, loggedInUser.Role)
        };

        var token = new JwtSecurityToken
        (
            issuer: builder.Configuration["Jwt:Issuer"],
            audience: builder.Configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(60),
            notBefore: DateTime.UtcNow,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                SecurityAlgorithms.HmacSha256)
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return tokenString;
    }
    return "Invalid user credentials";
}

app.MapPost("/login", (UserLogin user, IUserService service, [FromServices] FSMContext context) => {
    user.Password = sha256_hash(user.Password);
    var token = Login(user, service, context).ToString();
    User myUser = new();
    if (token != "Invalid user credentials" && token != "User not found")
    {
        myUser = context.Set<User>().Where(u => u.EmailAddress == user.Email).FirstOrDefault();
        myUser.Token = token;
        context.Set<Login>().Add(new Login()
        {
            UserId = myUser.Id
        });
        context.SaveChanges();
        return myUser;
    }
    myUser.EmailAddress = "Invalid";
    return myUser;
    }).Accepts<UserLogin>("application/json")
    .Produces<User>().AllowAnonymous();

app.MapGet("/User/{email}", ([FromServices] FSMContext context, string email) =>
{
    var res = context.Set<User>().Where(u => u.EmailAddress == email).FirstOrDefault();
    return res;
});

app.MapGet("/logins", ([FromServices] FSMContext context) =>
{
    return context.Set<Login>().ToList();
});

string sha256_hash(string value)
{
    StringBuilder Sb = new StringBuilder();

    using (var hash = SHA256.Create())
    {
        Encoding enc = Encoding.UTF8;
        byte[] result = hash.ComputeHash(enc.GetBytes(value));

        foreach (byte b in result)
            Sb.Append(b.ToString("x2"));
    }

    return Sb.ToString();
}


app.MapPost("/regrister", ([FromServices] FSMContext context, User user) =>
{
    string pass = user.Password;
    user.Password = sha256_hash(pass);
    user.Role = "basic";
    user.Token = "noToken";
    user.DiscordUserId = 0;
    context.Set<User>().Add(user);
    context.SaveChanges();
}).AllowAnonymous();

app.MapPost("/forgotpassword", ([FromServices] FSMContext context, Donkey Forgor, IEmailService service) =>
{
    try
    {
        User user = context.Set<User>().Where(u => u.EmailAddress == Forgor.Value).First();
    } catch (Exception ex)
    {
        return Results.BadRequest("Email not found");
    }
    List<AccountRecovery> prevRecoveries = context.Set<AccountRecovery>().Where(rec => rec.email == Forgor.Value).ToList();
    bool Active = false;
    foreach (AccountRecovery rec in prevRecoveries)
    {
        DateTime now = DateTime.Now;
        if (!rec.used && rec.TimeStamp.AddHours(1) >= now)
        {
            Active = true;
        }
    }
    if (!Active)
    {
        AccountRecovery acc = service.SendEmail(Forgor.Value);
        context.Set<AccountRecovery>().Add(acc);
        context.SaveChanges();
        return Results.Ok("An email has been sent to recover the account");
    }
    return Results.BadRequest("There is already an active token");
}).AllowAnonymous().Produces<string>();


app.MapPost("/forgotpassword/{token}", ([FromServices] FSMContext context, Guid token, Donkey newpass) =>
{
    AccountRecovery acc;
    try
    {
        acc = context.Set<AccountRecovery>().Where(a => a.Id == token).First();
    } catch (Exception ex)
    {
        return Results.BadRequest("token not found");
    }

    if (acc.used)
    {
        return Results.BadRequest("Token already used");
    }
    else if (acc.TimeStamp.AddHours(1) <= DateTime.Now)
    {
        return Results.BadRequest("Token has expired");
    }
    else
    {
        User user = context.Set<User>().Where(u => u.EmailAddress == acc.email).First();
        user.Password = newpass.Value;
        context.SaveChanges();
        acc.used = true;
    }
    return Results.Ok("Successfully changed password");
}).AllowAnonymous();

//#######################################
//#############ORGANISATIONS##################
//#######################################
app.MapGet("/Organisation/getall", ([FromServices] FSMContext context) => {
    return context.Set<Organisation>().ToList();
}).AllowAnonymous();
app.MapGet("/Organisation/get/{id}", ([FromServices] FSMContext context, Guid id) =>
{
    return context.Set<Organisation>().Where(x => x.id == id).FirstOrDefault();
}).AllowAnonymous();
app.MapPost("/Organisation/create", ([FromServices] FSMContext context, Organisation entity) =>
{
    if (entity.ImageName == "")
    {
        entity.ImageName = "default.png";
        entity.ImagePath = "default.png";
    }
    context.Set<Organisation>().Add(entity);
    context.SaveChanges();
    return new Donkey() {
        Value = entity.id.ToString()
    };
}).AllowAnonymous().Produces<Donkey>();
app.MapPut("/Organisation/update/{id}", ([FromServices] FSMContext context, Organisation entity, Guid id) =>
{
    Organisation old = context.Set<Organisation>().Where(x => x.id == id).FirstOrDefault();
    if (entity.ImageName == "")
    {
        entity.ImageName = old.ImageName;
        entity.ImagePath = old.ImagePath;
    }

    if (entity.name != "My Org") old.name = entity.name;
    if (entity.shortName != "MO") old.shortName = entity.shortName;
    if (entity.guildId != 0000000000000000000) old.guildId = entity.guildId;
    old.botConfig = entity.botConfig;
    old.ImageName = entity.ImageName;
    old.ImagePath = entity.ImagePath;
    context.SaveChanges();
}).AllowAnonymous();
app.MapDelete("/Organisation/delete/{id}", ([FromServices] FSMContext context, Guid id) =>
{
    Organisation entity = context.Set<Organisation>().Where(x => x.id == id).FirstOrDefault();
    context.Set<Organisation>().Remove(entity);
    context.SaveChanges();
}).AllowAnonymous();
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
}).AllowAnonymous();
app.MapPut("/Player/update/{id}", ([FromServices] FSMContext context, Player entity, int id) =>
{
    Player old = context.Set<Player>().Where(x => x.id == id).FirstOrDefault();
    old.discord = entity.discord;
    old.battlenet = entity.battlenet;
    old.prole = entity.prole;
    context.SaveChanges();
}).AllowAnonymous();
app.MapDelete("/Player/delete/{id}", ([FromServices] FSMContext context, int id) =>
{
    Player entity = context.Set<Player>().Where(x => x.id == id).FirstOrDefault();
    context.Set<Player>().Remove(entity);
    context.SaveChanges();
}).AllowAnonymous();

//#######################################
//#############SCRIMS##################
//#######################################
app.MapGet("/Scrim/getall", ([FromServices] FSMContext context) => {
    return context.Set<Scrim>().ToList();
}).AllowAnonymous();
app.MapGet("/Scrim/get/{id}", ([FromServices] FSMContext context, int id) =>
{
    return context.Set<Scrim>().Where(x => x.id == id).FirstOrDefault();
}).AllowAnonymous();
app.MapPost("/Scrim/create", ([FromServices] FSMContext context, Scrim entity) =>
{
    context.Set<Scrim>().Add(entity);
    context.SaveChanges();
}).AllowAnonymous();
app.MapPut("/Scrim/update/{id}", ([FromServices] FSMContext context, Scrim entity, int id) =>
{
    Scrim old = context.Set<Scrim>().Where(x => x.id == id).FirstOrDefault();
    old.Team1 = entity.Team1;
    old.Team2 = entity.Team2;
    old.datetime = entity.datetime;
    context.SaveChanges();
}).AllowAnonymous();
app.MapDelete("/Scrim/delete/{id}", ([FromServices] FSMContext context, int id) =>
{
    Scrim entity = context.Set<Scrim>().Where(x => x.id == id).FirstOrDefault();
    context.Set<Scrim>().Remove(entity);
    context.SaveChanges();
}).AllowAnonymous();
//#######################################
//#############TEAMS##################
//#######################################

app.MapGet("/Team/getall", ([FromServices] FSMContext context) => {
    return context.Set<Team>().ToList();
}).AllowAnonymous();
app.MapGet("/Team/get/{id}", ([FromServices] FSMContext context, Guid id) =>
{
    return context.Set<Team>().Where(x => x.id == id).FirstOrDefault();
}).AllowAnonymous();
app.MapPost("/Team/create", ([FromServices] FSMContext context, Team entity) =>
{
    if (entity.ImageName == "")
    {
        entity.ImageName = "default.png";
        entity.ImagePath = "default.png";
    }
    context.Set<Team>().Add(entity);
    context.SaveChanges();
}).AllowAnonymous();
app.MapPut("/Team/update/{id}", ([FromServices] FSMContext context, Team entity, Guid id) =>
{
    Team old = context.Set<Team>().Where(x => x.id == id).FirstOrDefault();
    if (entity.ImageName == "")
    {
        entity.ImageName = old.ImageName;
    }
    
    if (entity.name != "myTeam") old.name = entity.name;
    if (entity.captain != 00000000000000000000) old.captain = entity.captain;
    old.organisationId = entity.organisationId;
    old.ImageName = entity.ImageName;
    old.ImagePath = entity.ImagePath;
    context.SaveChanges();
}).AllowAnonymous();
app.MapDelete("/Team/delete/{id}", ([FromServices] FSMContext context, Guid id) =>
{
    Team entity = context.Set<Team>().Where(x => x.id == id).FirstOrDefault();
    context.Set<Team>().Remove(entity);
    List<Player> players = context.Set<Player>().Where(p => p.TeamId == id).ToList();
    foreach (Player p in players)
    {
        context.Set<Player>().Remove(p);
    }
    context.SaveChanges();
}).AllowAnonymous();

app.MapPost("/Organisation/AddUser", ([FromServices] FSMContext context, OrganisationUsers entity) =>
{
    if (context.Set<OrganisationUsers>().Where(e => e.UserId == entity.UserId).ToList().Count < 1)
    {
        context.Set<OrganisationUsers>().Add(entity);
        context.SaveChanges();
    }
}).AllowAnonymous();


app.MapGet("/OrganisationUsers", ([FromServices] FSMContext context) => {
    return context.Set<OrganisationUsers>().ToList();
}).AllowAnonymous();
app.MapControllers();

app.Run();
