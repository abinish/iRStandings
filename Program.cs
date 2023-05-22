using iRStandings.Hubs;
using iRStandings;
using Microsoft.AspNetCore.ResponseCompression;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<StandingsHelper>();
builder.Services.AddResponseCompression(opts =>
{
	opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
		new[] { "application/octet-stream" });
});
builder.Services.Configure<Settings>(
	builder.Configuration.GetSection("Settings"));

var app = builder.Build();
app.UseResponseCompression();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapHub<StandingsHub>("/standingshub");
app.MapHub<SettingsHub>("/settingshub");
app.MapFallbackToPage("/_Host");

try { 
	var url = "http://localhost:5000";

	if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
	{
		Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); // Works ok on windows
	}
	else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
	{
		Process.Start("xdg-open", url);  // Works ok on linux
	}
	else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
	{
		Process.Start("open", url); // Not tested
	}
}
catch (Exception ex)
{
    Console.WriteLine("Error opening browser | {0}", ex.Message);
}


app.Run();
