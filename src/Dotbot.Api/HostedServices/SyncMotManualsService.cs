using Dotbot.Infrastructure;
using Dotbot.Infrastructure.SeedWork;

namespace Dotbot.Api.HostedServices;

public class SyncMotManualsService(IServiceScopeFactory serviceScopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DotbotContext>();
        var newMotManualDefinitions = SeedMotInspectionDefectDefinitions.GenerateMotInspectionDefectDefinitions();
        foreach (var motManualDefinition in newMotManualDefinitions)
            if (!dbContext.MotInspectionDefectDefinitions.Any(x =>
                    x.DefectName == motManualDefinition.DefectName &&
                    x.CategoryArea == motManualDefinition.CategoryArea &&
                    x.TopLevelCategory == motManualDefinition.TopLevelCategory &&
                    x.DefectReferenceCode == motManualDefinition.DefectReferenceCode &&
                    x.CategoryArea == motManualDefinition.CategoryArea))
                await dbContext.MotInspectionDefectDefinitions.AddAsync(motManualDefinition, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        dbContext.ChangeTracker.Clear();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}