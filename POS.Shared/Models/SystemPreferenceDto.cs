namespace POS.Shared.Models
{
    public class SystemPreferenceDto
    {
        public int Id { get; set; }
        public int StoreCode { get; set; }
        public int SidebarIdleTimeoutSeconds { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdateSystemPreferenceDto
    {
        public int SidebarIdleTimeoutSeconds { get; set; }
    }
}