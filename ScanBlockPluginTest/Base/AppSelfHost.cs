using ServiceStack;

namespace ScanBlockPluginTest.Base;

public class AppSelfHost() : AppSelfHostBase("Test", typeof(AppSelfHost).Assembly)
{
}
