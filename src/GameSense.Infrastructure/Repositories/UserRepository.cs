using GameSense.Core.Models;
using GameSense.Core.Repositories;
using GameSense.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameSense.Infrastructure.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly GameSenseDbContext _db;

        public UserRepository(GameSenseDbContext db) => _db = db;

        public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
            await _db.Users
                .AsNoTracking()
                .OrderBy(user => user.Id)
                .ToListAsync(cancellationToken);

        public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            _db.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
    }
}
