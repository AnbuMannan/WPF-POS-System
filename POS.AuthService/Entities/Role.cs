namespace POS.AuthService.Entities
{
    public class AuthController
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentRoleId { get; set; }
    }
}
