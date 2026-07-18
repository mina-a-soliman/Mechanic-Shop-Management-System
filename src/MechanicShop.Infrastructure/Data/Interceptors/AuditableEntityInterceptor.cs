using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MechanicShop.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor(IUser user, TimeProvider dateTime)
            : SaveChangesInterceptor
{


    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData,
            InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public void UpdateEntities(DbContext? context)
    {
        if (context is null)
            return;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                var utcNow = dateTime.GetUtcNow();
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = user.Id;
                    entry.Entity.CreatedAtUtc = utcNow;
                }
                entry.Entity.LastModifiedBy = user.Id;
                entry.Entity.LastModifiedUtc = utcNow;

                foreach (var ownedEntry in entry.References)
                {
                    if (ownedEntry.TargetEntry is { Entity: AuditableEntity ownedEntity } && ownedEntry.TargetEntry.State is EntityState.Added or EntityState.Modified)
                    {
                        if (ownedEntry.TargetEntry.State == EntityState.Added)
                        {
                            ownedEntity.CreatedBy = user.Id;
                            ownedEntity.CreatedAtUtc = utcNow;
                        }

                        ownedEntity.LastModifiedBy = user.Id;
                        ownedEntity.LastModifiedUtc = utcNow;
                    }

                }
            }
        }
    }
}

public static class Extensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry)
    {
        return entry.References.Any(r =>
                   r.TargetEntry?.Metadata.IsOwned() == true &&
                   r.TargetEntry.State is EntityState.Added
                                         or EntityState.Modified
                                         or EntityState.Deleted)
               ||
               entry.Collections.Any(c =>
                   c.Metadata.TargetEntityType.IsOwned() &&
                   entry.Context.ChangeTracker.Entries()
                        .Any(e =>
                            e.Metadata.IsOwned() &&
                            e.State is EntityState.Added
                                      or EntityState.Modified
                                      or EntityState.Deleted &&
                            IsOwnedBy(e, entry)));
    }

    private static bool IsOwnedBy(EntityEntry ownedEntry, EntityEntry ownerEntry)
    {
        var ownership = ownedEntry.Metadata.FindOwnership();
        if (ownership == null)
            return false;

        for (int i = 0; i < ownership.Properties.Count; i++)
        {
            var dependentProperty = ownership.Properties[i];
            var principalProperty = ownership.PrincipalKey.Properties[i];

            var ownerValue = ownerEntry.Property(principalProperty.Name).CurrentValue;
            var ownedValue = ownedEntry.Property(dependentProperty.Name).CurrentValue;

            if (!Equals(ownerValue, ownedValue))
                return false;
        }

        return true;
    }
}