using tinyidp.infrastructure.bdd;

public interface IQueueSaveDB<T>
{
    Task EnqueueAsync(T item);
}
