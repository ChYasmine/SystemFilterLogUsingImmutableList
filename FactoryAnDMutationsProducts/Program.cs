using System.Collections.Concurrent;


class Order
{
    private string? EntityId { get; set; }
    private int Quantite { get; set; }

    private readonly ConcurrentDictionary<string, List<Order>> items = new();
    public int Total = 0;
    public void AddLineItem(string item, int quantite)
    {
        Console.WriteLine(" AddLineItem called ");
        Total += quantite;
    }
    public async Task ApplyDiscount(int rate)
    {
        await Task.Delay(100);
        Total += 10;
        Console.WriteLine(" le rate est de ");
    }
}
class MutationEtat
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> semaphores = new();
    async Task<TResult> WithEphemeralMutationAsync<TEntity, TResult>(
     string entityId,
     Func<TEntity> baseFactory,
     Func<TEntity, Task<TResult>> mutationLogic)
    {
        var semaphore = semaphores.GetOrAdd(entityId, new SemaphoreSlim(1, 1));
        TResult t;
        await semaphore.WaitAsync();
        try
        {
            var result = baseFactory();
            t = await mutationLogic(result);

        }
        finally
        {
            semaphore.Release();
        }

        return t;
    }

    static async Task Main(string[] args)
    {
        MutationEtat p = new MutationEtat();
        await Task.WhenAll(
    p.WithEphemeralMutationAsync("order-001", () => new Order(), async o =>
    {
        o.ApplyDiscount(10);
        await Task.Delay(100);
        return o.Total;
    }),
    p.WithEphemeralMutationAsync("order-002", () => new Order(), o =>
    {
        o.AddLineItem("Book", 2);
        return Task.FromResult(o.Total);
    })
);

    }

}
