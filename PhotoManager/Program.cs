using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PhotoManager.Areas.Identity;
using PhotoManager.Data;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.ResponseCompression;
using PhotoManager.Api.v1.Messages;
using PhotoManager.Api.v1.Helper.Auth;
using PhotoManager.Api.v1.Service;
using PhotoManager.Api.v1.DocIndexer;

using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

// Add services to the container.
// Identity Services
var MSSQL_Auth_Connection = Environment.GetEnvironmentVariable("MSSQL_Auth_Connection");
var connectionString = Environment.GetEnvironmentVariable("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (MSSQL_Auth_Connection is not null)
    {
        options.UseSqlServer(MSSQL_Auth_Connection);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

/**/
builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = configuration["JWT:ValidIssuer"],

            ValidateAudience = true,
            ValidAudience = configuration["JWT:ValidAudience"],

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]))
        };
    });


string AdminRole = "Administrator";

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireRole(new[] { AdminRole });
    });
});

builder.Services.AddLogging();
builder.Services.AddScoped<MessagePopup>();

// Photo models
var MSSQL_Photo_Connection = Environment.GetEnvironmentVariable("MSSQL_Photo_Connection");
string SQLitephotoConnectionString = $"DataSource={System.IO.Directory.GetCurrentDirectory()}/photo.db;Cache=Shared";

// Blazor
builder.Services.AddDbContextFactory<PhotoDbContext>(options =>
{
    if (MSSQL_Photo_Connection is not null)
    {
        options.UseSqlServer(MSSQL_Photo_Connection);
    }
    else
    {
        options.UseSqlite(SQLitephotoConnectionString);
    }
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
});

builder.Services.AddScoped<IIndexingService, IndexingService>();
builder.Services.AddScoped<IPhotoIndexerCore, PhotoIndexerCore>();

builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddScoped<TokenIssuer>();

// builder.Services.AddHttpClient("LocalApi", client => client.BaseAddress = new Uri("https://localhost:44333/"));

//builder.Services.AddDirectoryBrowser();


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

using (var scope = app.Services.CreateScope())
{
    IServiceProvider services = scope.ServiceProvider;

    var logger = services.GetRequiredService<ILogger<Program>>();

    // Create a database for ASP.NET Identity Authentication
    //ApplicationDbContext identityContext = services.GetRequiredService<ApplicationDbContext>();
    //logger.LogInformation("Main program will create a database for ASP.NET Identity Authentication if it does not exist");
    //identityContext.Database.EnsureCreated();


    // Create a new database if not exists
    PhotoDbContext context = services.GetRequiredService<PhotoDbContext>();
    logger.LogInformation("Create a database for PhotoManager App if it does not exist");
    context.Database.EnsureCreated();


    // Create an "Administrator" Role if not exists
    RoleManager<IdentityRole> roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    try
    {
        logger.LogInformation("Check if 'Administrator' role does exist");
        bool hasRole = await roleManager.RoleExistsAsync(AdminRole);

        if (!hasRole)
        {
            logger.LogInformation("Creating 'Administrator' Role.....");
            await roleManager.CreateAsync(new IdentityRole(AdminRole));
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred creating Role.");
    }

    // Create an admin user if not exits
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    logger.LogInformation("Check if admin user exists in the database");
    IdentityUser adminUser = await userManager.FindByNameAsync("admin");

    if (adminUser is null)
    {
        logger.LogInformation("Could not find admin user entity");
        try
        {
            var userStore = services.GetRequiredService<IUserStore<IdentityUser>>();
            logger.LogInformation("Starting to create the admin user");
            var user = Activator.CreateInstance<IdentityUser>();

            user.EmailConfirmed = true;

            await userStore.SetUserNameAsync(user, "admin", CancellationToken.None);

            await userManager.CreateAsync(user, "Admin1234!");
            logger.LogInformation("'admin' user has been created successfully");

            await userManager.AddToRoleAsync(user, AdminRole);
            logger.LogInformation("Setting role 'Administrator' for admin user");

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw new Exception("Could not create an admin user");
        }
    }
    else
    {
        logger.LogInformation("Finishing the check process if admin user exists or not");
    }    

}

app.UseHttpsRedirection();

PhysicalFileProvider PhotoRootDirProvider = new(Environment.GetEnvironmentVariable("PHOTO_ROOT_DIR"));
string PhotoRootDirPath = "/static";

string ThumbnailRootDir = Path.Combine(Directory.GetCurrentDirectory(), "thumbnail");

if (!System.IO.Directory.Exists(ThumbnailRootDir))
{
    System.IO.Directory.CreateDirectory(ThumbnailRootDir);
}

PhysicalFileProvider ThumbnailRootDirProvider = new(ThumbnailRootDir);

app.UseStaticFiles();

app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider = PhotoRootDirProvider,
        RequestPath = PhotoRootDirPath
    });
/*
app.UseDirectoryBrowser(
    new DirectoryBrowserOptions
    {
        FileProvider = PhotoRootDirProvider,
        RequestPath = PhotoRootDirPath
    });
*/
app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider = ThumbnailRootDirProvider,
        RequestPath = "/thumbnail"
    }
);

app.UseRouting();

app.UseStatusCodePages();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

