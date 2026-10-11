using System.Linq.Expressions;
using Demolite.Core.Models;

namespace Demolite.Db.Interfaces;

/// <summary>
/// Basic CRUD access to the entities of type <typeparamref name="TEntity" />.
/// Every call uses its own context and reports failures as <see cref="Result{T}" />.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TKey">The type of the primary key.</typeparam>
public interface IDbRepository<TEntity, in TKey>
	where TEntity : class, IDbItem<TKey>
	where TKey : notnull
{
	/// <summary>
	/// Loads all entities.
	/// </summary>
	Result<IReadOnlyList<TEntity>> GetAll();

	/// <inheritdoc cref="GetAll" />
	Task<Result<IReadOnlyList<TEntity>>> GetAllAsync(CancellationToken ct = default);

	/// <summary>
	/// Loads all entities matching the <paramref name="predicate" />.
	/// </summary>
	Task<Result<IReadOnlyList<TEntity>>> GetAllCustomAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken ct = default);

	/// <summary>
	/// Loads the entity with the given <paramref name="id" />. Fails with <c>Db.NotFound</c> if missing.
	/// </summary>
	Result<TEntity> Get(TKey id);

	/// <inheritdoc cref="Get" />
	Task<Result<TEntity>> GetAsync(TKey id, CancellationToken ct = default);

	/// <summary>
	/// Loads the first entity matching the <paramref name="predicate" />. Fails with <c>Db.NotFound</c> if none matches.
	/// </summary>
	Task<Result<TEntity>> GetCustomAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

	/// <summary>
	/// Loads the first entity, or stores and returns the one built by <paramref name="createDefault" /> if the table is empty.
	/// </summary>
	Task<Result<TEntity>> GetOrCreateAsync(Func<TEntity> createDefault, CancellationToken ct = default);

	/// <summary>
	/// Inserts the entity.
	/// </summary>
	Result<TEntity> Create(TEntity entity);

	/// <inheritdoc cref="Create" />
	Task<Result<TEntity>> CreateAsync(TEntity entity, CancellationToken ct = default);

	/// <summary>
	/// Updates the entity.
	/// </summary>
	Result<TEntity> Update(TEntity entity);

	/// <inheritdoc cref="Update" />
	Task<Result<TEntity>> UpdateAsync(TEntity entity, CancellationToken ct = default);

	/// <summary>
	/// Deletes the entity.
	/// </summary>
	Result<TEntity> Delete(TEntity entity);

	/// <inheritdoc cref="Delete" />
	Task<Result<TEntity>> DeleteAsync(TEntity entity, CancellationToken ct = default);

	/// <summary>
	/// Inserts all entities in a single transaction.
	/// </summary>
	Result<IReadOnlyList<TEntity>> CreateMany(IEnumerable<TEntity> entities);

	/// <inheritdoc cref="CreateMany" />
	Task<Result<IReadOnlyList<TEntity>>> CreateManyAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

	/// <summary>
	/// Updates all entities in a single transaction.
	/// </summary>
	Result<IReadOnlyList<TEntity>> UpdateMany(IEnumerable<TEntity> entities);

	/// <inheritdoc cref="UpdateMany" />
	Task<Result<IReadOnlyList<TEntity>>> UpdateManyAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

	/// <summary>
	/// Deletes all entities in a single transaction.
	/// </summary>
	Result<IReadOnlyList<TEntity>> DeleteMany(IEnumerable<TEntity> entities);

	/// <inheritdoc cref="DeleteMany" />
	Task<Result<IReadOnlyList<TEntity>>> DeleteManyAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
}
