using DFM.Shared.Common;
using DFM.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFM.Shared.Interfaces
{
    public interface INotificationManager
    {
        Task<CommonResponseId> CreateNotice(NotificationModel request, CancellationToken cancellationToken = default);
        Task<(CommonResponse Response, NotificationModel Content)> GetNotice(string id, CancellationToken cancellationToken = default);
        Task<(CommonResponse Response, IEnumerable<NotificationModel> Contents)> ListNotices(IEnumerable<string> roles, CancellationToken cancellationToken = default);
        Task<CommonResponseId> ReadNotice(string? id, string? userIDRead, CancellationToken cancellationToken = default);
    }
}
