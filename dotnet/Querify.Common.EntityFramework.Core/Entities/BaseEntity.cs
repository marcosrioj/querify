using System.ComponentModel.DataAnnotations;

namespace Querify.Common.EntityFramework.Core.Entities;

public class BaseEntity : AuditableEntity
{
    [Key] public Guid Id { get; set; }
}