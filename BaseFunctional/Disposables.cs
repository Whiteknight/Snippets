namespace BaseFunctional;

public readonly record struct DisposablesList(List<IDisposable> Disposables) : IDisposable
{
    public void Dispose()
    {
        foreach (var disposable in Disposables)
            disposable.Dispose();

        Disposables.Clear();
    }
}
