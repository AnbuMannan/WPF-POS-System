namespace POS.Shared.Models
{
    public class UomDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string UomName { get => Name; set => Name = value; }
        public string Symbol { get; set; } = string.Empty;
        public int DecimalPlaces { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
