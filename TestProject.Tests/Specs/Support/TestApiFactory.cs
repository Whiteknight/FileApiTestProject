using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TestProject.Api;

namespace TestProject.Tests.Specs.Support;

public sealed class TestApiFactory : WebApplicationFactory<Program>
{
    public DirectoryInfo ScenarioRoot { get; set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(FolderConfiguration));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddSingleton(new FolderConfiguration(ScenarioRoot));
        });
    }
}
