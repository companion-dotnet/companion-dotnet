namespace BlazorLighthouseTest.Types;

public class TestTaskSynchronizer
{
    private readonly TaskCompletionSource blockTask = new();
    private readonly TaskCompletionSource coroutineTask = new();

    public Task BlockTest()
    {
        return blockTask.Task;
    }

    public Task ContinueTest()
    {
        blockTask.SetResult();
        return coroutineTask.Task;
    }

    public void ContinueCoroutine()
    {
        coroutineTask.SetResult();
    }
}
