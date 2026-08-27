// using Skafetin.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Skafetin.Api.Data;

public class SkafetinDbContext: DbContext
{
    public SkafetinDbContext(DbContextOptions<SkafetinDbContext> options) : base(options)
    {

    }
}

