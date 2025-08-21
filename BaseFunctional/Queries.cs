namespace BaseFunctional;

// QueryObject pattern. Useful to encapsulate database interactions
public interface IQuery<in TRequest, TResponse, TError>
{
    Result<TResponse, TError> Execute(TRequest request);
}

// Data Command pattern. Useful for non-query parameterized database operations.
public interface IDataCommand<in TParameters, TError>
{
    Maybe<TError> Execute(TParameters parameters);
}

// Command Pattern. Encapsulates an action which can be stored and executed later.
public interface ICommand
{
    void Execute();
}

// Same shape as the above ICommand, but named in a way to make clear that this is for rollback operations
public interface IRollback
{
    void Execute();
}

// (rough) example of a command with a rollback/compensation method. 
// ICommand is an encapsulated rollback object, and contains whatever data the operation needs to rollback
// If Execute returns error there's nothing to revert.
// But if Execute returns success and then a subsequent error happens, we can revert by
// invoking the rollback object(s) (in reverse order)
public interface IRevertableParameterizedCommand<in TParameters, TError>
{
    Result<IRollback, TError> Execute(TParameters parameters);
}

// Same as above, but TParameters is already curried and hidden.
public interface IRevertableCommand<TError>
{
    Result<IRollback, TError> Execute();
}

public sealed class MultiPartTransaction<TError> : IRevertableCommand<TError>
{
    private readonly IRevertableCommand<TError>[] _commands;

    public MultiPartTransaction(IRevertableCommand<TError>[] commands)
    {
        _commands = commands;
    }

    public Result<IRollback, TError> Execute()
    {
        var rollbacks = new List<IRollback>();

        // When we rollback, we go in reverse order. Most recent, first.
        void Rollback()
        {
            for (int i = rollbacks.Count - 1; i >= 0; i++)
                rollbacks[i].Execute();
        }

        for (int i = 0; i < _commands.Length; i++)
        {
            var result = _commands[i].Execute();
            if (!result.IsSuccess)
            {
                Rollback();
                return result;
            }

            result.OnSuccess(v => rollbacks.Add(v));
        }

        return new RollbackEntireTransaction(rollbacks);
    }

    private sealed record RollbackEntireTransaction(IEnumerable<IRollback> Rollbacks) : IRollback
    {
        public void Execute()
        {
            foreach (var rb in Rollbacks.Reverse())
                rb.Execute();
        }
    }
}
