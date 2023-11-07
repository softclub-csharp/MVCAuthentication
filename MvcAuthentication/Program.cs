using EmailService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MvcAuthentication.CustomTokenProviders;
using MvcAuthentication.Data;
using MvcAuthentication.Factory;
using MvcAuthentication.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddScoped<IEmailSender, EmailSender>();

var emailConfig = builder.Configuration
    .GetSection("EmailConfiguration")
    .Get<EmailConfiguration>();
builder.Services.AddSingleton(emailConfig);

var configuration = builder.Configuration;

builder.Services.AddDbContext<ApplicationContext>(opts =>
    opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>(opt =>
    {
        opt.Password.RequiredLength = 7;
        opt.Password.RequireDigit = false;
        opt.Password.RequireUppercase = false;
        opt.User.RequireUniqueEmail = true;
        opt.SignIn.RequireConfirmedEmail = true;
        opt.Tokens.EmailConfirmationTokenProvider = "emailconfirmation";

    })
    .AddEntityFrameworkStores<ApplicationContext>()
    .AddDefaultTokenProviders()
    .AddTokenProvider<EmailConfirmationTokenProvider<User>>("emailconfirmation");

builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
    opt.TokenLifespan = TimeSpan.FromHours(2));

builder.Services.Configure<EmailConfirmationTokenProviderOptions>(opt =>
    opt.TokenLifespan = TimeSpan.FromDays(3));

builder.Services.AddScoped<IUserClaimsPrincipalFactory<User>, CustomClaimsFactory>(); 

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie")
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = "525656628108-600vlq66hvukemhp5o1l1ret5e0olpkq.apps.googleusercontent.com";
        googleOptions.ClientSecret = "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";
        googleOptions.CallbackPath = "/Account/ExternalLoginCallback/";
    })
    .AddOAuth("github", o =>
    {
        o.SignInScheme = "cookie";
        o.ClientId = "Iv1.4a603566a85c657b";
        o.ClientSecret = "cc46ccefd73dd325dceaf515758e9477554907ed";
        o.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
        o.TokenEndpoint = "https://github.com/login/oauth/access_token";
        o.UserInformationEndpoint = "https://api.github.com/user";
        o.CallbackPath = "/Account/ExternalLoginCallback";
        o.SaveTokens = true;
    });



var app = builder.Build();

app.UseCookiePolicy(new CookiePolicyOptions()
{
    MinimumSameSitePolicy = SameSiteMode.Lax
});

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