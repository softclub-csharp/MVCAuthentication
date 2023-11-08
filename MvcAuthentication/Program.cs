using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MvcAuthentication.Data;
using MvcAuthentication.Factory;
using MvcAuthentication.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var configuration = builder.Configuration;

builder.Services.AddDbContext<ApplicationContext>(opts =>
    opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>(opt =>
    {
        opt.Password.RequiredLength = 7;
        opt.Password.RequireDigit = false;
        opt.Password.RequireUppercase = false;

        opt.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationContext>();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<User>, CustomClaimsFactory>();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie")
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = "525656628108-600vlq66hvukemhp5o1l1ret5e0olpkq.apps.googleusercontent.com";
        googleOptions.ClientSecret = "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";
    });



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

app.MapGet("/login", () =>
{
    return Results.Challenge(
        new AuthenticationProperties()
        {
            RedirectUri = "http://localhost:5201/"
        },
        authenticationSchemes: new List<string>() { "google" });
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();