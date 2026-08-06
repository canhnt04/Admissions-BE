using System;

namespace Shared.Common.Helpers
{
    public static class TimeHelper
    {
        /// <summary>
        /// Lấy thời gian hiện tại theo múi giờ Việt Nam (UTC+7)
        /// Sử dụng thay cho DateTime.UtcNow để Database lưu đúng giờ thực tế
        /// </summary>
        public static DateTime VietnamNow => DateTime.UtcNow.AddHours(7);
    }
}
