using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using tinyidp.infrastructure.bdd;

public class BackgroundSaveDB : BackgroundService, IQueueSaveDB<Credential> 
{
    private readonly Channel<Credential> _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackgroundSaveDB> _logger;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // Un seul batch à la fois
    
    public BackgroundSaveDB(
        IServiceProvider serviceProvider,
        ILogger<BackgroundSaveDB> logger)
    {
        // Crée un channel unbounded (illimité)
        _queue = Channel.CreateUnbounded<Credential>(
            new UnboundedChannelOptions
            {
                SingleReader = true, // Optimisation : un seul consumer
                SingleWriter = false // Plusieurs producers possibles
            }
        );
        
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task EnqueueAsync(Credential credential)
    {
        await _queue.Writer.WriteAsync(credential);
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("RefreshTokenWriter started");

        var batch = new List<Credential>();

        while (!ct.IsCancellationRequested)
        {
           // Attend qu'au moins 1 item soit disponible
            var token = await _queue.Reader.ReadAsync(ct);
            batch.Add(token);

            // Collecte les items additionnels disponibles (max 100 total)
            while (batch.Count < 100 && _queue.Reader.TryRead(out var additionalToken))
            {
                batch.Add(additionalToken);
            }

            try
            {
                await _semaphore.WaitAsync(ct);
                await SaveBatchToDatabase(batch);
                _logger.LogWarning("Saved batch of {Count} credentials", batch.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save batch of credentials");
                // Optionnel : re-queue les tokens en échec
            }
            finally
            {
                _semaphore.Release();
                batch.Clear();
            }

            await Task.Delay(100, ct);
        }
    }

    private async Task SaveBatchToDatabase(IList<Credential> credentials)
    {
        // Crée un scope et DbContext frais pour chaque batch
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TinyidpContext>();
        
        dbContext.Credentials.AttachRange(credentials);
        foreach (var credential in credentials)
        {
            dbContext.Entry(credential).State = EntityState.Modified;
        }
        await dbContext.SaveChangesAsync();
    }

    public override void Dispose()
    {
        _semaphore?.Dispose();
        base.Dispose();
    }
}