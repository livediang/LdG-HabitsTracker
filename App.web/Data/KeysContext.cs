using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.web.Models
{
    public class KeysContext : DbContext, IDataProtectionKeyContext
    {
        public KeysContext(DbContextOptions<KeysContext> options) : base(options) { }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    }
}