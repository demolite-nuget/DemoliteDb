namespace Demolite.Db.Interfaces;

/// <summary>
/// An entity that can be stored by a repository.
/// </summary>
/// <typeparam name="TKey">The type of the primary key.</typeparam>
public interface IDbItem<TKey>
	where TKey : notnull
{
	/// <summary>
	/// The primary key of the entity.
	/// </summary>
	TKey Id { get; set; }
}
