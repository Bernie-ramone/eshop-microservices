using System;
using System.Collections.Generic;
using System.Text;

namespace EShop.BuildingBlocks.Infrastructure.Auditing;


/// <summary>
/// Marca una entidad como auditable.
/// El AuditableEntityInterceptor poblará CreatedAtUtc, UpdatedAtUtc, etc.
/// automáticamente al guardar.
///
/// Las entidades implementan esta interface si quieren auditoría;
/// si no la implementan, no se les toca.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; set; }
    DateTime? UpdatedAtUtc { get; set; }
}
