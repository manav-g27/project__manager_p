using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectManager.Api.Auth;
using ProjectManager.Api.Data;
using ProjectManager.Api.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

builder.Services.AddDbContext<AppDb>(o => o.UseSqlite(cfg.GetConnectionString("db") ?? "Data Source=pm.db"));
builder.Services.AddCors(o=>o.AddDefaultPolicy(p=>p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var secret = cfg["Jwt:Secret"] ?? "dev-secret-change-me";
var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => {
        o.TokenValidationParameters = new(){ ValidateIssuer=false, ValidateAudience=false, IssuerSigningKey=key };
    });

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    db.Database.Migrate();
}

int GetUserId(ClaimsPrincipal u) => int.Parse(u.FindFirstValue(ClaimTypes.NameIdentifier)!);

app.MapPost("/api/v1/auth/register", async (AppDb db, UserDto dto) => {
    if (await db.Users.AnyAsync(u=>u.Username==dto.Username)) return Results.BadRequest("Username taken");
    Jwt.CreatePasswordHash(dto.Password, out var hash, out var salt);
    var user = new User{ Username=dto.Username, PasswordHash=hash, PasswordSalt=salt };
    db.Users.Add(user); await db.SaveChangesAsync();
    var token = Jwt.CreateToken(user, secret);
    return Results.Ok(new { token, user = new { user.Id, user.Username } });
});

app.MapPost("/api/v1/auth/login", async (AppDb db, UserDto dto) => {
    var user = await db.Users.FirstOrDefaultAsync(u=>u.Username==dto.Username);
    if (user is null || !Jwt.VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt)) return Results.Unauthorized();
    var token = Jwt.CreateToken(user, secret);
    return Results.Ok(new { token, user = new { user.Id, user.Username } });
});

app.MapGet("/api/v1/projects", async (AppDb db, ClaimsPrincipal u) => {
    var uid = GetUserId(u);
    var list = await db.Projects.Where(p=>p.OwnerId==uid).OrderByDescending(p=>p.Id).ToListAsync();
    return Results.Ok(list);
}).RequireAuthorization();

app.MapPost("/api/v1/projects", async (AppDb db, ClaimsPrincipal u, ProjectDto dto) => {
    var uid = GetUserId(u);
    var p = new Project{ Title=dto.Title, Description=dto.Description, OwnerId=uid };
    db.Projects.Add(p); await db.SaveChangesAsync();
    return Results.Created($"/api/v1/projects/{p.Id}", p);
}).RequireAuthorization();

app.MapDelete("/api/v1/projects/{id:int}", async (AppDb db, ClaimsPrincipal u, int id) => {
    var uid = GetUserId(u);
    var p = await db.Projects.FirstOrDefaultAsync(x=>x.Id==id && x.OwnerId==uid);
    if (p is null) return Results.NotFound();
    db.Projects.Remove(p); await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

app.MapGet("/api/v1/projects/{pid:int}/tasks", async (AppDb db, ClaimsPrincipal u, int pid) => {
    var uid = GetUserId(u);
    var ok = await db.Projects.AnyAsync(p=>p.Id==pid && p.OwnerId==uid);
    if(!ok) return Results.NotFound();
    var t = await db.Tasks.Where(t=>t.ProjectId==pid).OrderByDescending(t=>t.Id).ToListAsync();
    return Results.Ok(t);
}).RequireAuthorization();

app.MapPost("/api/v1/projects/{pid:int}/tasks", async (AppDb db, ClaimsPrincipal u, int pid, TaskDto dto) => {
    var uid = GetUserId(u);
    var ok = await db.Projects.AnyAsync(p=>p.Id==pid && p.OwnerId==uid);
    if(!ok) return Results.NotFound();
    var t = new TaskItem{ Title=dto.Title, DueDate=dto.DueDate, ProjectId=pid };
    db.Tasks.Add(t); await db.SaveChangesAsync();
    return Results.Created($"/api/v1/projects/{pid}/tasks/{t.Id}", t);
}).RequireAuthorization();

app.MapPatch("/api/v1/tasks/{id:int}/toggle", async (AppDb db, ClaimsPrincipal u, int id) => {
    var uid = GetUserId(u);
    var t = await db.Tasks.Include(x=>x.Project).FirstOrDefaultAsync(x=>x.Id==id && x.Project.OwnerId==uid);
    if (t is null) return Results.NotFound();
    t.IsCompleted = !t.IsCompleted; await db.SaveChangesAsync();
    return Results.Ok(t);
}).RequireAuthorization();

app.MapDelete("/api/v1/tasks/{id:int}", async (AppDb db, ClaimsPrincipal u, int id) => {
    var uid = GetUserId(u);
    var t = await db.Tasks.Include(x=>x.Project).FirstOrDefaultAsync(x=>x.Id==id && x.Project.OwnerId==uid);
    if (t is null) return Results.NotFound();
    db.Tasks.Remove(t); await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

app.MapPost("/api/v1/projects/{pid:int}/schedule", async (AppDb db, ClaimsPrincipal u, int pid, ScheduleInput input) => {
    var uid = GetUserId(u);
    var ok = await db.Projects.AnyAsync(p=>p.Id==pid && p.OwnerId==uid);
    if(!ok) return Results.NotFound();

    var tasks = await db.Tasks
        .Where(t=>t.ProjectId==pid && !t.IsCompleted)
        .OrderBy(t=>t.DueDate ?? DateTime.MaxValue)
        .ThenBy(t=>t.Id)
        .ToListAsync();

    var start = input.StartDate?.Date ?? DateTime.UtcNow.Date;
    var end = input.EndDate?.Date ?? (tasks.Count==0 ? start.AddDays(6) : (tasks.Max(t=>t.DueDate) ?? start.AddDays(6))).Date;
    var days = (end - start).Days + 1;
    var buckets = Enumerable.Range(0,days).ToDictionary(i => start.AddDays(i), _ => new List<object>());

    var i = 0; foreach (var t in tasks){
        var day = start.AddDays(i % days);
        buckets[day].Add(new { t.Id, t.Title, t.DueDate });
        i++;
    }
    var result = buckets.Select(kv => new { date = kv.Key, tasks = kv.Value });
    return Results.Ok(new { start, end, days, schedule = result });
}).RequireAuthorization();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");
app.Run();

record UserDto(string Username, string Password);
record ProjectDto(string Title, string? Description);
record TaskDto(string Title, DateTime? DueDate);
record ScheduleInput(DateTime? StartDate, DateTime? EndDate);
