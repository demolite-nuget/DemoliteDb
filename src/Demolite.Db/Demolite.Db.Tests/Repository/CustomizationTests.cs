namespace Demolite.Db.Tests.Repository;

[TestClass]
public class CustomizationTests : RepositoryTestBase
{
	private TrackingRepository Tracking => (TrackingRepository)Sut;

	protected override TestRepository CreateRepository(TestDbContextFactory factory) => new TrackingRepository(factory);

	[TestMethod]
	public void EachCall_UsesItsOwnContext()
	{
		Sut.Create(NewEntity());
		Sut.GetAll();
		Sut.GetAll();

		Assert.AreEqual(3, Tracking.CreatedContexts);
	}

	[TestMethod]
	public async Task EachAsyncCall_UsesItsOwnContext()
	{
		await Sut.CreateAsync(NewEntity(), Ct);
		await Sut.GetAllAsync(Ct);
		await Sut.GetAllAsync(Ct);

		Assert.AreEqual(3, Tracking.CreatedContexts);
	}

	[TestMethod]
	public void GetSetOverride_IsUsedByOperations()
	{
		Sut.Create(NewEntity());
		Sut.GetAll();

		Assert.AreEqual(2, Tracking.SelectedSets);
	}
}
