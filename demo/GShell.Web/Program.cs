using GShell.Web.Components;
using GShell.Web.Shell;
using GShell.Web.XTerm;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GShell.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Configuration.AddJsonFile("shellsettings.json", optional: false, reloadOnChange: true);

            builder.Services.Configure<ShellSettings>(
                builder.Configuration.GetSection("Shell")
            );

            builder.Services.AddSingleton<IUnicodeVersionProvider, UnicodeV6>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
