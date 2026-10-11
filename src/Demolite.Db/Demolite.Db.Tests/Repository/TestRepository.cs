using Demolite.Db.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Demolite.Db.Tests.Repository;

public class TestRepository(IDbContextFactory<TestDbContext> factory)
	: AbstractRepository<TestEntity, int, TestDbContext>(factory);

public sealed class TrackingRepository(IDbContextFactory<TestDbContext> factory) : TestRepository(factory)
{
	public int CreatedContexts { get; private set; }

	public int SelectedSets { get; private set; }

	protected override TestDbContext CreateContext()
	{
		CreatedContexts++;
		return base.CreateContext();
	}

	protected override DbSet<TestEntity> GetSet(TestDbContext context)
	{
		SelectedSets++;
		return base.GetSet(context);
	}
}
