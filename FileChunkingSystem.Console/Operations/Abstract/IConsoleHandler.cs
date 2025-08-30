namespace FileChunkingSystem.Console.Handlers.Abstract;

public interface IConsoleHandler
{
    Task HandleAsync();
}

public interface IConsoleHandler<T>
{
    Task<T> HandleAsync();
}
