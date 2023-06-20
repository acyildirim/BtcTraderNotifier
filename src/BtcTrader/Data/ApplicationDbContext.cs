using BtcTrader.Domain.Instructions;
using BtcTrader.Domain.User;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BtcTrader.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        
    }
    
    public DbSet<Instruction> Instructions { get; set; }
    public DbSet<InstructionAudit> InstructionAudit { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Instruction>(x => x.HasKey(aa => new { aa.Id}));
        builder.Entity<InstructionAudit>(x => x.HasKey(aa => new { aa.Id}));

        base.OnModelCreating(builder);
    }
}