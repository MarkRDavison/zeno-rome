namespace mark.davison.common.web.playwright.test.Core;

public static class TaskHelper
{
    public static async Task<TOutput> ThenAsync<TInput, TOutput>(
        this Task<TInput> inputTask,
        Func<TInput, Task<TOutput>> continuationFunction,
        bool continueOnCapturedContext = true)
    {
        var input = await inputTask.ConfigureAwait(continueOnCapturedContext);
        var output = await continuationFunction(input).ConfigureAwait(continueOnCapturedContext);
        return output;
    }
    public static async Task<TOutput> ThenAsync<TInput, TOutput>(
        this Task<TInput> inputTask,
        Func<TInput, TOutput> continuationFunction,
        bool continueOnCapturedContext = true)
    {
        var input = await inputTask.ConfigureAwait(continueOnCapturedContext);
        var output = continuationFunction(input);
        return output;

    }

    public static async Task<TInput> ThenAsync<TInput>(
        this Task<TInput> inputTask,
        Action<TInput> continuationFunction,
        bool continueOnCapturedContext = true)
    {
        var input = await inputTask.ConfigureAwait(continueOnCapturedContext);
        continuationFunction(input);
        return input;
    }

    public static async Task<TInput> ThenAsync<TInput>(
        this Task<TInput> inputTask,
        Func<TInput, Task> continuationFunction,
        bool continueOnCapturedContext = true)
    {
        var input = await inputTask.ConfigureAwait(continueOnCapturedContext);
        await continuationFunction(input).ConfigureAwait(continueOnCapturedContext);
        return input;
    }

    public static async Task<TOutput> ThenAsync<TOutput>(
       this Task inputTask,
       Func<Task<TOutput>> continuationFunction,
       bool continueOnCapturedContext = true)
    {
        await inputTask.ConfigureAwait(continueOnCapturedContext);
        var output = await continuationFunction().ConfigureAwait(continueOnCapturedContext);
        return output;
    }

    public static async Task ThenAsync(
        this Task inputTask,
        Action continuationFunction,
        bool continueOnCapturedContext = true)
    {
        await inputTask.ConfigureAwait(continueOnCapturedContext);
        continuationFunction();
    }
}