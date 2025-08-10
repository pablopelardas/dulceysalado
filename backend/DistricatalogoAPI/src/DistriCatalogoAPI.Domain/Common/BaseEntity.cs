using System;

namespace DistriCatalogoAPI.Domain.Common
{
    public abstract class BaseEntity
    {
        public DateTime CreatedAt { get; protected set; }
        public DateTime UpdatedAt { get; protected set; }

        protected BaseEntity()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetCreatedAt(DateTime createdAt)
        {
            CreatedAt = createdAt;
        }

        public void SetUpdatedAt(DateTime updatedAt)
        {
            UpdatedAt = updatedAt;
        }

        public void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}