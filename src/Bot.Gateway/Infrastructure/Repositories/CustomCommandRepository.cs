using Bot.Gateway.Infrastructure.Entities;
using Bot.Gateway.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace Bot.Gateway.Infrastructure.Repositories;

public class CustomCommandRepository : ICustomCommandRepository
{
    private readonly DotbotContext _context;
    public IUnitOfWork UnitOfWork => _context;

    public CustomCommandRepository(DotbotContext context)
    {
        _context = context;
    }
    
    public async Task<CustomCommand?> GetByNameAsync(string name)
    {
        return await _context.CustomCommands.FirstOrDefaultAsync(cc => cc.Name == name);
    }

    public CustomCommand Add(CustomCommand command)
    {
        return _context.CustomCommands.Add(command).Entity;
    }

    public void Update(CustomCommand command)
    {
        _context.Entry(command).State = EntityState.Modified;
    }
}