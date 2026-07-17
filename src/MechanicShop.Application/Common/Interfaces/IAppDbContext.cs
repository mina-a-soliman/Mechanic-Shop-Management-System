using System;
using System.Collections.Generic;
using System.Text;
using MechanicShop.Domain.Identity;
using Microsoft.EntityFrameworkCore;
namespace MechanicShop.Application.Common.Interfaces;
public interface IAppDbContext
{

    public DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}