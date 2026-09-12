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
        private readonly IAuditSuppressor _suppressor;
        private ICurrentUser _CurrentUser;
        public AuditingSaveChangesInterceptor(IAuditSuppressor suppressor,ICurrentUser uesr)
        {
            _suppressor = suppressor;
            _CurrentUser = uesr;
        }
     
        

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            if (eventData.Context is not null && !_suppressor.IsSuppressed)
            {
                AuditChanges(eventData.Context,_CurrentUser);
            }

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null && !_suppressor.IsSuppressed)
            {
                AuditChanges(eventData.Context, _CurrentUser);
            }

            return base.SavingChangesAsync(
                eventData,
                result,
                cancellationToken);
        }

        private enum AuditAction
        {
            Insert = 0,
            Update = 1,
            SoftDelete = 2
        }

        private void AuditChanges(DbContext context,ICurrentUser user)
        {
            var now = DateTime.UtcNow;
            var auditRows = new List<AuditTrail>();

            var auditTables = new Dictionary<string, string[]>
            {
                { "Order", new[] { "Status", "Drivers" } },
                { "Product", new[] { "ProductName", "IsOutOfStock", "Price", "Quantity", "StockQuantity" } },
                {"OrderLine",new[]{ "Price", "ProductId" } }
            };

            foreach (var entry in context.ChangeTracker
                         .Entries()
                         .Where(e => e.Entity is not AuditTrail)
                         .ToList())
            {
                var tableName = entry.Metadata.ClrType.Name; 

                if (!auditTables.TryGetValue(tableName, out var auditableColumns))
                {
                    continue; 
                }


                if (entry.State is not (EntityState.Added or EntityState.Modified))
                {
                    continue;
                }
                DateTime? updatedDate= DateTime.UtcNow;
                if (entry.State is not EntityState.Modified)
                {
                    updatedDate =null;
                }

                var changes = GetChangeSet(entry, auditableColumns);

                if (changes.Count == 0)
                {
                    continue;
                }

                auditRows.Add(new AuditTrail
                {
                    EntityName = entry.Metadata.ClrType.Name,

                    EntityId = GetPrimaryKeyValue(entry),

                    AuditAction = entry.State == EntityState.Added
                        ? (int)AuditAction.Insert
                            : (int)AuditAction.Update,
                    UpdatedAtUtc=updatedDate,
                    ChangeJson = JsonSerializer.Serialize(changes),

                    TimestampUtc = now,

                    UserId = user.UserId
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

            if (primaryKey is null)
            {
                return 0;
            }

            var property = primaryKey.Properties.First();

            var value = entry.Property(property.Name).CurrentValue;

            if (value is null)
            {
                return 0;
            }

            return Convert.ToInt32(value);
        }

        private static Dictionary<string, object?> GetChangeSet(
            EntityEntry entry,
            string[] auditProperties)
        {
            var changes = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                if (!auditProperties.Contains(property.Metadata.Name))
                {
                    continue;
                }

                if (entry.State == EntityState.Added)
                {
                    changes[property.Metadata.Name] =
                        property.CurrentValue;

                    continue;
                }

                if (property.IsModified &&
                    !Equals(
                        property.OriginalValue,
                        property.CurrentValue))
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