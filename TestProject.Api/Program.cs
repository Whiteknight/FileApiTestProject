using TestProject.Api;

namespace TestProject;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddSingleton(ConfigFromEnvironmentVariables());
        builder.Services.AddControllers();

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.MapControllers();

        app.Run();
    }

    private static FolderConfiguration ConfigFromEnvironmentVariables()
    {
        var rootPath = Environment.GetEnvironmentVariable("ROOT_DIRECTORY");
        if (string.IsNullOrEmpty(rootPath))
            throw new InvalidOperationException("Environment variable ROOT_DIRECTORY must be provided and not empty");
        if (!Path.IsPathRooted(rootPath))
            throw new InvalidOperationException($"Environment variable ROOT_DIRECTORY '{rootPath}' must be a fully-qualified directory location");
        return new FolderConfiguration(new DirectoryInfo(rootPath));
    }
}
