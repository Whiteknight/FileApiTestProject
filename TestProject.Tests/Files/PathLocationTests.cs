using AwesomeAssertions;
using TestProject.Api;
using TestProject.Api.Files;

namespace TestProject.Tests.Files;

public class PathLocationTests
{
    public class CreateSanitizeValidate
    {
        private string _testDirectoryName = string.Empty;
        private string _testDirectoryPath = string.Empty;
        private DirectoryInfo _rootDirectory = null!;

        [SetUp]
        public void CreateWorkingDirectory()
        {
            _rootDirectory = TestFixture.RootDirectoryInfo;
            (_testDirectoryName, _testDirectoryPath) = TestFixture.CreateUniqueDirectory();
        }

        [TestCase("DOES/NOT/EXIST")]
        [TestCase("/DOES/NOT/EXIST")]
        [TestCase("DOES/NOT/EXIST/")]
        [TestCase("DOES\\NOT\\EXIST")]
        [TestCase("\\DOES\\NOT\\EXIST")]
        [TestCase("DOES\\NOT\\EXIST\\")]
        public void CreateNew_NotExists(string inputPath)
        {
            CreateNewTest(inputPath, "DOES/NOT/EXIST", false, EntryType.Unknown, false);
        }

        [TestCase("")]
        [TestCase("/")]
        [TestCase("\\")]
        public void CreateNew_FileExists(string prefix)
        {
            var fileName = "a.txt";
            var path = Path.Combine(_testDirectoryPath, fileName);
            File.WriteAllText(path, "test");
            CreateNewTest($"{prefix}{_testDirectoryName}/{fileName}", $"{_testDirectoryName}/{fileName}", true, EntryType.File, false);
        }

        [TestCase("", "")]
        [TestCase("/", "")]
        [TestCase("", "/")]
        [TestCase("\\", "")]
        [TestCase("", "\\")]
        public void CreateNew_FolderExists(string prefix, string suffix)
        {
            var folderName = "b";
            var path = Path.Combine(_testDirectoryPath, folderName);
            File.WriteAllText(path, "test");
            CreateNewTest($"{prefix}{_testDirectoryName}/{folderName}{suffix}", $"{_testDirectoryName}/{folderName}", true, EntryType.File, false);
        }

        [TestCase("..")]
        [TestCase("../../../../../Windows/System32")]
        public void CreateNew_AttemptJailbreak(string inputPath)
        {
            CreateNewError<LocationIsOutsideTheRootFolder>(inputPath);
        }

        [TestCase("test/|")]
        [TestCase("\a\b")]
        public void CreateNew_InvalidCharacters(string inputPath)
        {
            CreateNewError<PathIsInvalid>(inputPath);
        }

        [TestCase("")]
        [TestCase("/")]
        [TestCase("\\")]
        public void CreateNew_RootDir(string inputPath)
        {
            CreateNewTest(inputPath, "", true, EntryType.Folder, false);
        }

        private PathLocation CreateNewTest(string path, string name, bool exists, EntryType type, bool hidden)
        {
            var result = PathLocation.SanitizeAndCanonicalize(_rootDirectory, path)
                .Then(loc => loc.Validate());
            result.IsError.Should().BeFalse();
            var pathLocation = result.GetValueOrDefault(default);
            pathLocation.Name.Should().Be(name);
            pathLocation.Exists.Should().Be(exists);
            pathLocation.Type.Should().Be(type);
            pathLocation.Hidden.Should().Be(hidden);
            return pathLocation;
        }

        private void CreateNewError<TError>(string path)
            where TError : Error
        {
            var result = PathLocation.SanitizeAndCanonicalize(_rootDirectory, path)
                .Then(loc => loc.Validate());
            result.IsError.Should().BeTrue();
            result.GetErrorOrThrow().Should().BeOfType<TError>();
        }
    }

    public class Append
    {
        private string _testDirectoryName = string.Empty;
        private string _testDirectoryPath = string.Empty;
        private DirectoryInfo _rootDirectory = null!;

        [SetUp]
        public void CreateWorkingDirectory()
        {
            _rootDirectory = TestFixture.RootDirectoryInfo;
            (_testDirectoryName, _testDirectoryPath) = TestFixture.CreateUniqueDirectory();
        }

        [TestCase("test")]
        [TestCase("/test")]
        [TestCase("\\test")]
        public void Append_ToRoot(string inputName)
        {
            var rootPath = new PathLocation(_rootDirectory, _rootDirectory.FullName, "", true, EntryType.Folder, false);
            var result = rootPath.Append(inputName);
            result.Name.Should().Be("test");
        }

        [TestCase("test")]
        [TestCase("/test")]
        [TestCase("\\test")]
        public void Append_ToWorkDir(string inputName)
        {
            var rootPath = new PathLocation(_rootDirectory, _testDirectoryPath, _testDirectoryName, true, EntryType.Folder, false);
            var result = rootPath.Append(inputName);
            result.Name.Should().Be($"{_testDirectoryName}/test");
        }
    }

    public class From
    {
        private string _testDirectoryName = string.Empty;
        private string _testDirectoryPath = string.Empty;
        private DirectoryInfo _rootDirectory = null!;

        [SetUp]
        public void CreateWorkingDirectory()
        {
            _rootDirectory = TestFixture.RootDirectoryInfo;
            (_testDirectoryName, _testDirectoryPath) = TestFixture.CreateUniqueDirectory();
        }

        [Test]
        public void From_WorkingDirectory()
        {
            var dir = new DirectoryInfo(_testDirectoryPath);
            var result = PathLocation.From(_rootDirectory, dir);
            result.Name.Should().Be(_testDirectoryName);
            result.Type.Should().Be(EntryType.Folder);
            result.Exists.Should().BeTrue();
            result.Hidden.Should().BeFalse();
        }
    }
}
