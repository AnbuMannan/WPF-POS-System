namespace POS.Domain.Entities;

public class Brand
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
}
