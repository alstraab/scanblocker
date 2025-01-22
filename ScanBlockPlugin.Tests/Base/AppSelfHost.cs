using ServiceStack;

namespace ScanBlockPlugin.Tests.Base;

public class AppSelfHost() : AppSelfHostBase("Test", typeof(AppSelfHost).Assembly)
{
}
