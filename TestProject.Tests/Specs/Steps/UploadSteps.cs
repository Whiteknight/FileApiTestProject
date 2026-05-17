using System.Net.Http;
using System.Text;
using Reqnroll;

namespace TestProject.Tests.Specs.Steps;

[Binding]
public sealed class UploadSteps
{
    private readonly ScenarioContext _context;

    public UploadSteps(ScenarioContext context)
    {
        _context = context;
    }

    private HttpClient Client => _context.Get<HttpClient>();

    [When(@"I upload a file ""(.+)"" with content ""(.+)"" to ""(.+)""")]
    public async Task WhenIUploadAFile(string fileName, string content, string targetDirectory)
    {
        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
        form.Add(fileContent, "file", fileName);
        form.Add(new StringContent(targetDirectory), "targetDirectory");

        var response = await Client.PostAsync("/Files/upload", form);
        _context.Set(response, "Response");
    }
}
