using System.Linq.Expressions;
using Demolite.Db.Enum;
using Demolite.Db.Interfaces;
using Lite.Db.Result;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Demolite.Db.Repositories;

public abstract partial class AbstractBaseRepository<T, TContext>
{
	/// <summary>
	/// Override this method to include navigation properties for items.
	/// Only works for get calls, as saving automatically included entities can cause tracking conflicts.
	/// </summary>
	/// <param name="dbSet">Retrieved db set for the entity.</param>
	/// <returns>The db set with included entities.</returns>
	protected virtual IQueryable<T> Include(DbSet<T> dbSet)
	{
		return dbSet.AsQueryable();
	}
	
	/// <inheritdoc />
	public async Task<T?> GetAsync(string id)
	{
		await using var db = await GetContextAsync();
		return await Include(db.Set<T>()).FirstOrDefaultAsync(x => x.Id == id);
	}

    /// <inheritdoc />
    public async Task<T?> GetCustomAsync(Func<T, bool> match)
	{
		await using var db = await GetContextAsync();
		return Include(db.Set<T>()).AsEnumerable().FirstOrDefault(match.Invoke);
	}

    /// <inheritdoc />
    public async Task<T[]> GetAllAsync()
	{
		await using var db = await GetContextAsync();
		return await Include(db.Set<T>()).ToArrayAsync();
	}

    /// <inheritdoc />
    public async Task<T[]> GetAllCustomAsync(Expression<Func<T, bool>> match)
	{
		await using var db = await GetContextAsync();
		return await Include(db.Set<T>()).Where(match).ToArrayAsync();
	}

    /// <inheritdoc />
    public async Task<IDbResult<T>[]> CrudManyAsync(IEnumerable<T> items)
	{
		var results = new List<IDbResult<T>>();

		foreach (var item in items) results.Add(await CrudAsync(item));

		return results.ToArray();
	}

    /// <inheritdoc />
    public async Task<IDbResult<T>> CrudAsync(T item)
	{
		return item.OperationType switch
		{
			Operation.Created => await CreateAsync(item),
			Operation.Updated => await UpdateAsync(item),
			Operation.Removed => await DeleteAsync(item),
			Operation.None => DbResult<T>.Ok(item),
			_ => throw new ArgumentOutOfRangeException(),
		};
	}

    /// <inheritdoc />
    public async Task<IDbResult<T>> CreateAsync(T item)
	{
		await using var db = await GetContextAsync();
		db.Set<T>().Add(item);
		return await TrySaveAsync(db, item);
	}

    /// <inheritdoc />
    public async Task<IDbResult<T>> UpdateAsync(T item)
	{
		await using var db = await GetContextAsync();
		db.Set<T>().Update(item);
		return await TrySaveAsync(db, item);
	}

    /// <inheritdoc />
    public async Task<IDbResult<T>> DeleteAsync(T item)
	{
		await using var db = await GetContextAsync();
		db.Set<T>().Remove(item);
		return await TrySaveAsync(db, item);
	}

	protected abstract Task<TContext> GetContextAsync();

	private static async Task<IDbResult<T>> TrySaveAsync(TContext db, T item)
	{
		try
		{
			await db.SaveChangesAsync();
			return DbResult<T>.Ok(item);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Error while saving {Type}", typeof(T));
			return DbResult<T>.Failed(item, ex.Message);
		}
	}
}