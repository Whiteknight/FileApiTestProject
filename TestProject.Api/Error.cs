namespace TestProject.Api;

public abstract record Error(string Message, Exception? Exception = null);

public sealed record NothingToDo()
    : Error("The operation requested involved doing no work, so none was done");