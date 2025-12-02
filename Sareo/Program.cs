using Sareoo.Areas.Admin.Data;
using Sareoo.Data;
using Sareoo.Hubs;
using Sareoo.Models;
using Sareoo.Services;
using Sareoo.Trackers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);



var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PlatformDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<PlatformDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddSignalR();
builder.Services.AddSingleton<PresenceTracker>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddLocalization();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";


    options.AccessDeniedPath = "/Account/Login";
});


builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IFileService, FileService>();


var app = builder.Build();
var supportedCultures = new[]
{
    new CultureInfo("ar-EG")
};
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("ar-EG"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapHub<PresenceHub>("/presenceHub");

app.MapControllerRoute(
    name: "AdminArea",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
