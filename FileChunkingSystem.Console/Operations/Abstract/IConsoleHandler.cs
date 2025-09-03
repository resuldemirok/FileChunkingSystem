namespace FileChunkingSystem.Console.Handlers.Abstract;

/// <summary>
/// Interface for console handlers that can process user interactions asynchronously.
/// </summary>
public interface IConsoleHandler
{
    /// <summary>
    /// Handles the console interaction asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    Task HandleAsync();
}

/// <summary>
/// Generic interface for console handlers that can process user interactions asynchronously.
/// </summary>
public interface IConsoleHandler<T>
{
    /// <summary>
    /// Handles the console interaction asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    Task<T> HandleAsync();
}
