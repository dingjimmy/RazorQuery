namespace RazorQuery;

/// <summary>
/// Represents an asynchronous operation that can mutate data based on the provided input.
/// </summary>
/// <remarks>This class encapsulates a mutation operation, allowing the caller to execute the operation
/// asynchronously and track its status.
/// </remarks>
/// <typeparam name="TInput">The type of input data to be processed by the mutation.</typeparam>
public class Mutation<TInput> : MutationBase
{
    private readonly Func<TInput, DefaultQueryFunctionContext, Task> _mutationFunc;

    public Mutation(Func<TInput, DefaultQueryFunctionContext, Task> mutateFunction)
    {
        _mutationFunc = mutateFunction ?? throw new ArgumentNullException(nameof(mutateFunction));
    }

    public async Task Execute(TInput input)
    {
        Error = null;
        Status = MutationStatus.Pending;

        // TODO: Link this up to DI container or HttpClient factory
        var funcContext = new DefaultQueryFunctionContext(null)
        {
            ErrorMessage = string.Empty,
            //HttpClient = new HttpClient() // this is v bad, should be injected instead. just using for prototype.
        };

        try
        {
            await _mutationFunc(input, funcContext);

            // check for errors raised by the function itself.
            // TODO: currently using exceptions for flow logic, but should be 
            //       replaced with a more structured error handling approach
            if (funcContext.ErrorMessage.Length > 0)
            {
                throw new Exception(funcContext.ErrorMessage);
            }

            Status = MutationStatus.Success;
        }
        catch (Exception e)
        {
            Status = MutationStatus.Error;
            Error = e;
        }
    }
}
