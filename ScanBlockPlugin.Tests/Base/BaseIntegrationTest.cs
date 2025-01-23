using Alstra.ScanBlockPlugin;
using Microsoft.Extensions.Configuration;
using ServiceStack;

namespace ScanBlockPlugin.Tests.Base;

public class BaseIntegrationTest
{
    protected static string BaseUri => "http://localhost:2000";
    protected ServiceStackHost AppHost { get; private set; }
    protected ScanBlockFeature Plugin { get; init; }

    [OneTimeSetUp]
    protected void SetUpServiceStack()
    {
        var appHost = new AppSelfHost()
        {
            Configuration = BuildConfiguration()
        };

        appHost.Plugins.Add(Plugin);

        var serviceStackLicense = appHost.Configuration.GetSection("servicestack").GetValue<string>("license")
                                  ?? Environment.GetEnvironmentVariable("SERVICESTACK_LICENSE");
        
        Licensing.RegisterLicense(serviceStackLicense);

        AppHost = appHost.Init().Start(BaseUri);
    }

    [OneTimeTearDown]
    protected void TearDownServiceStack() => AppHost.Dispose();

    private static IConfiguration BuildConfiguration()
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        configurationBuilder.AddUserSecrets<AppSelfHost>(true, true);

        return configurationBuilder.Build();
    }
}
