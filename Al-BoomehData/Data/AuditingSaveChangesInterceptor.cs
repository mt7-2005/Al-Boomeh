using Al_BoomehDAL.Models;
using Al_BoomehDAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;
using Al_BoomehDAL.Interfaces;

namespace Al_BoomehDAL.Data
{
    public class AuditingSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IAuditScope _auditScope;
        private readonly ICurrentUser _currentUser;

        public AuditingSaveChangesInterceptor(IAuditScope auditScope, ICurrentUser currentUser)
        {
            _auditScope = auditScope;
            _currentUser = currentUser;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            if (eventData.Context is not null)
            {
                ApplyChanges(eventData.Context);
            }
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
            {
                ApplyChanges(eventData.Context);
            }
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private enum AuditAction
        {
            Insert = 0,
            Update = 1,
            SoftDelete = 2
        }

        private void ApplyChanges(DbContext context)
        {
            var now = DateTime.UtcNow;
            var auditRows = new List<AuditTrail>();

            foreach (var entry in context.ChangeTracker.Entries<BaseEntity>().ToList())
            {
                var originalState = entry.State;

                if (originalState == EntityState.Added)
                {
                    entry.Entity.CreatedAtUtc = now;
                }
                else if (originalState == EntityState.Modified)
                {
                    entry.Entity.UpdatedAtUtc = now;
                }

                if (originalState == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAtUtc = now;
                    entry.Entity.UpdatedAtUtc = now;
                }

                if (entry.Entity is not IAuditable) continue;
                if (!_auditScope.IsEnabled) continue;
                if (originalState is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)) continue;

                var changes = GetChangeSet(entry);
                if (changes.Count == 0) continue;

                var action = originalState switch
                {
                    EntityState.Added => AuditAction.Insert,
                    EntityState.Deleted => AuditAction.SoftDelete,
                    _ => AuditAction.Update
                };

                auditRows.Add(new AuditTrail
                {
                    EntityName = entry.Metadata.ClrType.Name,
                    EntityId = GetPrimaryKeyValue(entry),
                    AuditAction = (int)action,
                    ChangeJson = JsonSerializer.Serialize(changes),
                    TimestampUtc = now,
                    UserId = _currentUser.UserId
                });
            }

            if (auditRows.Count > 0)
            {
                context.Set<AuditTrail>().AddRange(auditRows);
            }
        }

        private static int GetPrimaryKeyValue(EntityEntry entry)
        {
            var primaryKey = entry.Metadata.FindPrimaryKey();
            if (primaryKey is null) return 0;

            var property = primaryKey.Properties.First();
            var value = entry.Property(property.Name).CurrentValue;

            return value is null ? 0 : Convert.ToInt32(value);
        }

        private static Dictionary<string, object?> GetChangeSet(EntityEntry entry)
        {
            var changes = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                // UpdatedAtUtc changes on every single save — recording it tells you nothing.
                if (property.Metadata.Name == nameof(BaseEntity.UpdatedAtUtc)) continue;

                if (entry.State == EntityState.Added)
                {
                    changes[property.Metadata.Name] = property.CurrentValue;
                    continue;
                }

                if (property.IsModified && !Equals(property.OriginalValue, property.CurrentValue))
                {
                    changes[property.Metadata.Name] = new
                    {
                        old = property.OriginalValue,
                        @new = property.CurrentValue
                    };
                }
            }

            return changes;
        }
    }
}
