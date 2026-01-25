using System;

namespace POS.UI.Modules.Admin.Common   // 👈 keep same namespace as ProductDto for simplicity
{
    public class LookupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
