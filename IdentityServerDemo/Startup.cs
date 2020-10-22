using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using IdentityServerDemo.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Api;
using Microsoft.AspNetCore.Http;
using IdentityServerDemo.Services;
using System.Threading.Tasks;

namespace IdentityServerDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static readonly string AssemblyName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
               options
                   .UseNpgsql(Configuration.GetConnectionString("DefaultConnection"), b =>
                   {
                       b.MigrationsAssembly(AssemblyName);
                       b.MigrationsHistoryTable("__EFMigrationsHistory");
                   }));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services
                .AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                    options.EmitStaticAudienceClaim = true;
                })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryApiResources(Config.ApiResources)
                .AddInMemoryClients(Config.Clients)
                .AddAspNetIdentity<IdentityUser>()
                .AddJwtBearerClientAuthentication()
                .AddProfileService<ProfileService>()
                .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
                .AddDeveloperSigningCredential();

            services
                .AddAuthentication()
                .AddFacebook(options =>
                    {
                        options.AppId = Configuration["Authentication:Facebook:AppId"];
                        options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                        options.Events.OnRemoteFailure = (context =>
                        {
                            context.Response.Redirect(context.Properties.RedirectUri);
                            context.HandleResponse();
                            return Task.CompletedTask;
                        });
                    })
                .AddGoogle(options =>
                {
                    options.ClientId = Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                    options.Events.OnRemoteFailure = (context =>
                    {
                        context.Response.Redirect(context.Properties.RedirectUri);
                        context.HandleResponse();
                        return Task.CompletedTask;
                    });
                })
                .AddIdentityServerAuthentication(options =>
                {
                    var settings = Configuration.GetSection("Authentication").Get<AuthenticationSettings>();
                    options.Authority = settings.Authority;
                    options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
                    options.ApiName = settings.ApiName;
                    options.ApiSecret = settings.ApiSecret;
                });
            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
            });
            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseStaticFiles();
            // fix chrome browser,Chrome enabled a new feature "Cookies without SameSite must be secure", 
            // the cookies shoold be expided from https, but in demo project, the internal comunicacion in docker compose is http.
            // To avoid this problem, the policy of cookies shold be in Lax mode.
            // vì mày mà t mất cả ngày
            app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax }); // 

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
