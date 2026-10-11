using System.ComponentModel.DataAnnotations;
using Demolite.Db.Interfaces;

namespace Demolite.Db.Models;

/// <summary>
/// Base class for entities with a primary key of type <typeparamref name="TKey" />.
/// Override <see cref="Id" /> to customize key generation, e.g. with <c>DatabaseGenerated</c>.
/// </summary>
/// <typeparam name="TKey">The type of the primary key.</typeparam>
public abstract class AbstractDbItem<TKey> : IDbItem<TKey>
	where TKey : notnull
{
	/// <inheritdoc />
	[Key]
	public virtual TKey Id { get; set; } = default!;
}

/// <summary>
/// Base class for entities with an <see cref="int" /> primary key.
/// </summary>
public abstract class AbstractDbItem : AbstractDbItem<int>;
