using System;

namespace AppCore.Models;

public abstract class EntityBase
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
