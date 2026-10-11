namespace Demolite.Db.Tests.Repository;

public abstract class RepositoryTestBase
{
	private TestDbContextFactory _factory = null!;

	protected TestRepository Sut { get; private set; } = null!;

	public TestContext TestContext { get; set; } = null!;

	protected CancellationToken Ct => TestContext.CancellationToken;

	[TestInitialize]
	public void TestInitialize()
	{
		_factory = new TestDbContextFactory();
		Sut = CreateRepository(_factory);
	}

	[TestCleanup]
	public void TestCleanup() => _factory.Dispose();

	protected virtual TestRepository CreateRepository(TestDbContextFactory factory) => new(factory);

	protected static TestEntity NewEntity(string name = "Elf") => new() { Name = name };

	protected TestEntity Seed(string name = "Elf") => Sut.Create(NewEntity(name)).Value!;

	protected async Task<TestEntity> SeedAsync(string name = "Elf")
		=> (await Sut.CreateAsync(NewEntity(name), Ct)).Value!;
}
