using FlowBoard.Auth.Data;
using FlowBoard.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Auth.Repositories
{
    /// <summary>
    /// Data access layer for the Users table.
    /// All database queries are centralised here – services never touch DbContext directly.
    /// </summary>
    public class UserRepository
    {
        private readonly AuthDbContext _db;

        public UserRepository(AuthDbContext db)
        {
            _db = db;
        }

        // ── READ ─────────────────────────────────────────────────────────────

        /// <summary>Find a user by their primary key (UUID).</summary>
        public async Task<User?> GetByIdAsync(Guid userId)
            => await _db.Users.FindAsync(userId);

        /// <summary>Find a user by email (case-insensitive).</summary>
        public async Task<User?> GetByEmailAsync(string email)
            => await _db.Users
                        .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        /// <summary>
        /// Find a user by OAuth provider + provider-specific ID.
        /// Used to match returning OAuth users and avoid creating duplicate accounts.
        /// </summary>
        public async Task<User?> GetByProviderAsync(string provider, string providerId)
            => await _db.Users
                        .FirstOrDefaultAsync(u => u.Provider == provider
                                               && u.ProviderId == providerId);

        /// <summary>
        /// Search users by full name or email (case-insensitive, partial match).
        /// Returns up to 20 results – enough for a member-picker UI.
        /// </summary>
        public async Task<IEnumerable<User>> SearchAsync(string query)
        {
            var q = query.ToLower();
            return await _db.Users
                            .Where(u => u.IsActive &&
                                        ((u.FullName != null && u.FullName.ToLower().Contains(q))
                                       || u.Email.ToLower().Contains(q)))
                            .OrderBy(u => u.FullName)
                            .Take(20)
                            .ToListAsync();
        }

        /// <summary>Returns ALL users (active and suspended) – for admin dashboard.</summary>
        public async Task<IEnumerable<User>> GetAllAsync()
            => await _db.Users
                        .OrderBy(u => u.FullName)
                        .ToListAsync();

        // ── WRITE ────────────────────────────────────────────────────────────

        /// <summary>Persist a new user and return the saved entity.</summary>
        public async Task<User> CreateAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        /// <summary>Persist changes for an existing user.</summary>
        public async Task<User> UpdateAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return user;
        }

        /// <summary>Hard-delete a user account (admin only).</summary>
        public async Task DeleteAsync(Guid userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>Returns true when the email is already registered (any provider).</summary>
        public async Task<bool> EmailExistsAsync(string email)
            => await _db.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }
}
