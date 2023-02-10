﻿using Kipa_plus.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.StaticFiles;
using DynamicAuthorization.Mvc.Core.Extensions;
using DynamicAuthorization.Mvc.Ui;
using DynamicAuthorization.Mvc.MsSqlServerStore;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

// Add services to the container.
var DBHOST = Environment.GetEnvironmentVariable("DB_HOST");
var DBPORT = Environment.GetEnvironmentVariable("DB_PORT");
var DBNAME = Environment.GetEnvironmentVariable("DB_NAME");
var DBUSER = Environment.GetEnvironmentVariable("DB_USER");
var DBUSERPASSWD = Environment.GetEnvironmentVariable("DB_USER_PASSWORD");

var connectionString = $"Server={DBHOST},{DBPORT};Database={DBNAME};User ID={DBUSER};Password={DBUSERPASSWD};TrustServerCertificate=True;MultipleActiveResultSets=true;";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
               .AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
               .AddDefaultUI();

var mvcBuilder = builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDynamicAuthorization<ApplicationDbContext>(options => options.DefaultAdminUser = Environment.GetEnvironmentVariable("DefaultAdminUser"))
    .AddSqlServerStore(options => options.ConnectionString = connectionString)
    .AddUi(mvcBuilder);




var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();

//lisää lupa .lang tiedostojen jakoo palvelimella formbuilderia varten
var provider = new FileExtensionContentTypeProvider();
provider.Mappings.Add(".lang", "language");
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });

app.UseRouting();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");



app.MapRazorPages();
app.MapControllers();

app.Run();
