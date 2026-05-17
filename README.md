# File Manipulation Website

## Get Started

Create an environment variable `ROOT_DIRECTORY` pointing to the file location you want to serve files from.

```bash
dotnet build
dotnet test
dotnet run --project TestProject.Api
```

Open your browser to https://localhost:7146

## Design Considerations

### AI Use and Tooling

The API code was largely hand-written, though some of the test cases were generated with AI and verified manually. Portions of the UI were delegated to AI tooling. I wanted to demonstrate both facets: that I can write code myself and that I can effectively prompt the tool to create code on my behalf. VisualStudio's built-in Copilot is used occasionally for completions in the API portion, though it was not a major contributor. My AI tools of choice for this were Google Gemini for some research and initial hashing out of the feature set, and AWS Kiro for code implementation.

### API Concept

API organization borrows some ideas from Clean Architecture and Vertical Slices. I have opted to co-locate everything in the .Api project instead of creating the more traditional ".Contracts", ".Domain"/".Core" and ".UseCases" projects commonly found in templates for Clean Architecture. Breaking things out into separate projects can help with long-term organization and navigation, but for a small limited project like this, it's overkill and it distracts from code readability.

I have opted to use a very modern, functional approach to the API implementation based around the `Result<T>` (a specialized Either monad implementation) type, expressions and pattern-matching. It is worth mentioning that while this is perfectly doable in .NET 8, 9 and 10, it won't feel like a first-class citizen until discriminated unions land in .NET 11. While some libraries implement this pattern just as well or better than I have here, I opted to create my own as a demonstration of my understanding of the concept, especially how it demonstrates error-handling and structuring of high-level flows.

I have opted for error objects instead of throwing exceptions throughout the application. Exceptions bring performance issues, even with the optimizations in .NET 9, and can lead to confusion about non-local control flow. Error objects as I have implemented them here are small and short-lived objects which should play nicely with the GC, and control flow is more clear. This also means there is no custom middleware for catching exceptions and translating them to meaningful http error codes, that is done in an explicit mapping step in the controller instead. I have tried to test, automated and manually, most code paths but a true fuzzing solution would be required to make sure we weren't letting exceptions slip through to create obtuse 500 error responses on the client.

Several types throughout are implemented as `readonly struct`. While this helps keep data immutable and requires no heap allocation, there are inherent dangers in passing `default` uninitialized values around which can break things like compile-time null checking (`PathLocation` contains a reference to `DirectoryInfo` for example which the compiler flags as non-null but may be null in the case of `default`). For this small project we can get away with manually inspecting callsites to make sure we aren't leaving uninitialized values around. In a larger production system more consideration should be paid to the design to see if the risks (and the added Debug assertions) are worth the low memory usage in all cases. This application is IO-bound due to the non-async nature of the default C# file APIs, so small optimizations like this may not be worthwhile in the long run if it is leading to developer mistakes.

`PathLocation` in the API is one of the most important domain models in the system. It keeps track of the current location, does sanitation and validation, and keeps separate the concept of the local full path (which on windows uses backslashes) and the canonical display name (which is relative to the directory root and always uses forward-slashes). There are some memory optimizations we could make here (`string.Create()` instead of repeated Trim/Replace operations, for example), but I opted not to go that far in this iteration.

### UI Design

I went for a simple implementation and single-file organization for the UI code. There are a couple places where I could have broken functions out into separate modules, but given the desire for simplicity and the small size of the implementation I opted to keep it in a single file. 

A small amount of CSS is added for basic usability.

### Tests

I did not include UI tests. The UI is tested manually and there are a few notes in there about known areas where we can make future improvements. 

The API has some unit-tests for key domain types and spec/integration tests using Reqnroll. I wanted to demonstrate the operation of several important flows without going overboard to trying to reach 100% coverage.

### Obvious Omissions

The task description explicitly said to avoid boilerplate and drive towards simplicity. Scripts and features that I normally would have included were intentionally omitted: Dockerfile and docker-compose.yml, .env, documentation, AGENTS.md, ARCHITECTURE.md, CI/CD pipeline definitions, deployment scripts, etc.

I have likewise avoided a lot of setup ceremony and security that otherwise would be required: authentication and authorization, CORS, logging and telemetry, response caching. I do not like giving unauthenticated access to the local filesystem, but that's what has been asked of me. I make a note here that I am aware that this is a BAD IDEA. 

I did not create any indirection around data storage. In another system I might have retargettable backends for S3 buckets or network blob stores or other alternatives. In this case I do not expect to need that flexibility in the future. I could use an abstraction ato facilitate unit testing, but I have opted to use integration/spec testing instead for these flows instead of using mocks. For such a simple app, mocking out the file system for tests feels like it cuts out the meat of the functionality. Testing that our application interacts correctly with the file system is the entire purpose of the exercise.

In the UI I did not use any external libraries or frameworks as requested, and I did not do any minification. A "real" production website would be so fundamentally different that it's not worth listing out all the possible changes we would consider.

