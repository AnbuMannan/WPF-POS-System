namespace POS.Domain.Entities;
using System.Text.Json.Serialization;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    [JsonIgnore]
    public DateTime RowVersion { get; set; }
}
