// NotificationService.cs
using InventorySolution.Data;
using InventorySolution.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySolution.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(string message, string type, int? relatedEntityId = null)
        {
            // Fixed admin user query using proper role navigation
            var adminUsers = await _context.Users
                .Where(u => _context.UserRoles
                    .Where(ur => ur.UserId == u.Id)
                    .Select(ur => ur.RoleId)
                    .Join(_context.Roles,
                        roleId => roleId,
                        role => role.Id,
                        (roleId, role) => role)
                    .Any(r => r.Name == "Admin"))
                .ToListAsync();

            foreach (var user in adminUsers)
            {
                var notification = new Notification
                {
                    Message = message,
                    Type = type,
                    RelatedEntityId = relatedEntityId,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUnreadNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }
            await _context.SaveChangesAsync();
        }
    }
}