using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventorySolution.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string message, string type, int? relatedEntityId = null);
        Task<List<Notification>> GetUnreadNotificationsAsync(string userId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(string userId);
    }
}