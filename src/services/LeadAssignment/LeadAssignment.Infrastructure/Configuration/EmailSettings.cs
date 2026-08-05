namespace LeadAssignment.Infrastructure.Configuration
{
    public class EmailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool UseSsl { get; set; } = true;
        
        /// <summary>
        /// Nếu có giá trị, tất cả email sẽ được gửi đến địa chỉ này thay vì người nhận thực sự (dùng cho môi trường Dev)
        /// </summary>
        public string? DevEmailOverride { get; set; }
    }
}
