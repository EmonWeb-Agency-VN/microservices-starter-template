using Common.Application.Time;
using Common.Domain.Entities;
using Common.SharedKernel;
using Common.SharedKernel.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Common.Persistence.Interceptors
{
    public class UpdateAuditableEntitiesInterceptor : SaveChangesInterceptor
    {
        private readonly ISystemTime systemTime;
        private readonly IHttpContextAccessor httpContextAccessor;

        public UpdateAuditableEntitiesInterceptor(ISystemTime systemTime, IHttpContextAccessor httpContextAccessor)
        {
            this.systemTime = systemTime;
            this.httpContextAccessor = httpContextAccessor;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context is null)
            {
                return base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            DateTimeOffset utcNow = this.systemTime.UtcNow;
            var auditableEntities = eventData.Context.ChangeTracker.Entries<AuditableEntities>();
            foreach (var entity in auditableEntities)
            {
                if (entity.Entity is AuditableEntities auditableEntity)
                {
                    var currentUserId = this.httpContextAccessor.HttpContext.CurrentUserId();
                    var userId = currentUserId != -1 ? currentUserId : Constants.SystemAdminId;
                    switch (entity.State)
                    {
                        case EntityState.Added:
                            auditableEntity.CreatedById = userId;
                            auditableEntity.ModifiedById = userId;
                            auditableEntity.CreatedTime = utcNow;
                            auditableEntity.ModifiedTime = utcNow;
                            break;
                        case EntityState.Modified:
                            auditableEntity.ModifiedById = userId;
                            auditableEntity.ModifiedTime = utcNow;
                            break;
                    }
                }
            }
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }


    }
}
