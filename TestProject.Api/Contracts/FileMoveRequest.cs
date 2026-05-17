namespace TestProject.Api.Contracts;

public class FileMoveRequest
{
    public required string SourcePath { get; set; }
    public required string DestinationPath { get; set; }
}
