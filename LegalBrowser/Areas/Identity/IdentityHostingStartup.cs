using LegalBrowser.Areas.Identity.Data;
using LegalBrowser.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(LegalBrowser.Areas.Identity.IdentityHostingStartup))]

namespace LegalBrowser.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddDbContext<LegalBrowserContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("LegalBrowserContextConnection")));

                services.AddDefaultIdentity<LegalBrowserUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<LegalBrowserContext>();
            });
        }
    }
}