using System.Linq.Expressions;
using Demolite.Core.Models;
using Demolite.Db.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Demolite.Db.Repositories;

/// <summary>
/// Base repository that creates a short-lived context per call from an <see cref="IDbContextFactory{TContext}" />.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TKey">The type of the primary key.</typeparam>
/// <typeparam name="TContext">The context that contains <typeparamref name="TEntity" />.</typeparam>
/// <param name="contextFactory">Creates the context for each call.</param>
public abstract class AbstractRepository<TEntity, TKey, TContext>(IDbContextFactory<TContext> contextFactory)
	: IDbRepository<TEntity, TKey>
	where TEntity : class, IDbItem<TKey>
	where TKey : notnull
	where TContext : DbContext
{
	/// <summary>
	/// Creates the context used by a single call.
	/// </summary>
	protected virtual TContext CreateContext() => contextFactory.CreateDbContext();

	/// <summary>
	/// Selects the set the repository works on.
	/// </summary>
	protected virtual DbSet<TEntity> GetSet(TContext context) => context.Set<TEntity>();

	/// <inheritdoc />
	public virtual Result<IReadOnlyList<TEntity>> GetAll()
	{
		using var context = CreateContext();

		return Result<IReadOnlyList<TEntity>>.Success(GetSet(context).ToList());
	}

	/// <inheritdoc />
	public virtual async Task<Result<IReadOnlyList<TEntity>>> GetAllAsync(CancellationToken ct = default)
	{
		await using var context = CreateContext();

		return Result<IReadOnlyList<TEntity>>.Success(await GetSet(context).ToListAsync(ct));
	}

	/// <inheritdoc />
	public virtual async Task<Result<IReadOnlyList<TEntity>>> GetAllCustomAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken ct = default)
	{
		await using var context = CreateContext();

		return Result<IReadOnlyList<TEntity>>.Success(await GetSet(context).Where(predicate).ToListAsync(ct));
	}

	/// <inheritdoc />
	public virtual Result<TEntity> Get(TKey id)
	{
		using var context = CreateContext();

		return ToResult(GetSet(context).Find(id), id);
	}

	/// <inheritdoc />
	public virtual async Task<Result<TEntity>> GetAsync(TKey id, CancellationToken ct = default)
	{
		await using var context = CreateContext();

		return ToResult(await GetSet(context).FindAsync([id], ct), id);
	}

	/// <inheritdoc />
	public virtual async Task<Result<TEntity>> GetCustomAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken ct = default)
	{
		await using var context = CreateContext();

		var entity = await GetSet(context).FirstOrDefaultAsync(predicate, ct);

		return entity is not null
			? Result<TEntity>.Success(entity)
			: Result<TEntity>.Failure(new Error("Db.NotFound", $"No {typeof(TEntity).Name} matches '{predicate}'."));
	}

	/// <inheritdoc />
	public virtual async Task<Result<TEntity>> GetOrCreateAsync(
		Func<TEntity> createDefault,
		CancellationToken ct = default)
	{
		await using var context = CreateContext();

		var entity = await GetSet(context).FirstOrDefaultAsync(ct);

		if (entity is not null)
			return Result<TEntity>.Success(entity);

		entity = createDefault();
		GetSet(context).Add(entity);

		return await TrySaveAsync(context, entity, ct);
	}

	/// <inheritdoc />
	public virtual Result<TEntity> Create(TEntity entity)
	{
		using var context = CreateContext();
		GetSet(context).Add(entity);

		return TrySave(context, entity);
	}

	/// <inheritdoc />
	public virtual async Task<Result<TEntity>> CreateAsync(TEntity entity, CancellationToken ct = default)
	{
		await using var context = CreateContext();
		GetSet(context).Add(entity);

		return await TrySaveAsync(context, entity, ct);
	}

	/// <inheritdoc />
	public virtual Result<TEntity> Update(TEntity entity)
	{
		using var context = CreateContext();
		GetSet(context).Update(entity);

		return TrySave(context, entity);
	}

	/// <inheritdoc />
	public virtual async Task<Result<TEntity>> UpdateAsync(TEntity entity, CancellationToken ct = default)
	{
		await using var context = CreateContext();
		GetSet(context).Update(entity);

		return await TrySaveAsync(context, entity, ct);
	}

	/// <inheritdoc />
	public virtual Result<TEntity> Delete(TEntity entity)
	{
		using var context = CreateContext();
		GetSet(context).Remove(entity);

		return TrySave(context, entity);
	}

	/// <inheritdoc />
	public virtual async Task<Result<TEntity>> DeleteAsync(TEntity entity, CancellationToken ct = default)
	{
		await using var context = CreateContext();
		GetSet(context).Remove(entity);

		return await TrySaveAsync(context, entity, ct);
	}

	/// <inheritdoc />
	public virtual Result<IReadOnlyList<TEntity>> CreateMany(IEnumerable<TEntity> entities)
		=> SaveMany(entities, (set, items) => set.AddRange(items));

	/// <inheritdoc />
	public virtual Task<Result<IReadOnlyList<TEntity>>> CreateManyAsync(
		IEnumerable<TEntity> entities,
		CancellationToken ct = default)
		=> SaveManyAsync(entities, (set, items) => set.AddRange(items), ct);

	/// <inheritdoc />
	public virtual Result<IReadOnlyList<TEntity>> UpdateMany(IEnumerable<TEntity> entities)
		=> SaveMany(entities, (set, items) => set.UpdateRange(items));

	/// <inheritdoc />
	public virtual Task<Result<IReadOnlyList<TEntity>>> UpdateManyAsync(
		IEnumerable<TEntity> entities,
		CancellationToken ct = default)
		=> SaveManyAsync(entities, (set, items) => set.UpdateRange(items), ct);

	/// <inheritdoc />
	public virtual Result<IReadOnlyList<TEntity>> DeleteMany(IEnumerable<TEntity> entities)
		=> SaveMany(entities, (set, items) => set.RemoveRange(items));

	/// <inheritdoc />
	public virtual Task<Result<IReadOnlyList<TEntity>>> DeleteManyAsync(
		IEnumerable<TEntity> entities,
		CancellationToken ct = default)
		=> SaveManyAsync(entities, (set, items) => set.RemoveRange(items), ct);

	private Result<IReadOnlyList<TEntity>> SaveMany(
		IEnumerable<TEntity> entities,
		Action<DbSet<TEntity>, List<TEntity>> apply)
	{
		var items = entities.ToList();
		using var context = CreateContext();
		apply(GetSet(context), items);

		try
		{
			context.SaveChanges();
			return Result<IReadOnlyList<TEntity>>.Success(items);
		}
		catch (DbUpdateException ex)
		{
			return Result<IReadOnlyList<TEntity>>.Failure(SaveFailed(ex));
		}
	}

	private async Task<Result<IReadOnlyList<TEntity>>> SaveManyAsync(
		IEnumerable<TEntity> entities,
		Action<DbSet<TEntity>, List<TEntity>> apply,
		CancellationToken ct)
	{
		var items = entities.ToList();
		await using var context = CreateContext();
		apply(GetSet(context), items);

		try
		{
			await context.SaveChangesAsync(ct);
			return Result<IReadOnlyList<TEntity>>.Success(items);
		}
		catch (DbUpdateException ex)
		{
			return Result<IReadOnlyList<TEntity>>.Failure(SaveFailed(ex));
		}
	}

	private static Result<TEntity> TrySave(TContext context, TEntity entity)
	{
		try
		{
			context.SaveChanges();
			return Result<TEntity>.Success(entity);
		}
		catch (DbUpdateException ex)
		{
			return Result<TEntity>.Failure(SaveFailed(ex));
		}
	}

	private static async Task<Result<TEntity>> TrySaveAsync(TContext context, TEntity entity, CancellationToken ct)
	{
		try
		{
			await context.SaveChangesAsync(ct);
			return Result<TEntity>.Success(entity);
		}
		catch (DbUpdateException ex)
		{
			return Result<TEntity>.Failure(SaveFailed(ex));
		}
	}

	private static Error SaveFailed(DbUpdateException ex) => new("Db.SaveFailed", ex.Message, ex);

	private static Result<TEntity> ToResult(TEntity? entity, TKey id) => entity is not null
		? Result<TEntity>.Success(entity)
		: Result<TEntity>.Failure(new Error("Db.NotFound", $"{typeof(TEntity).Name} with id '{id}' not found."));
}
