namespace Demolite.Db.Tests.Repository;

[TestClass]
public class ReadTests : RepositoryTestBase
{
	[TestMethod]
	public void GetAll_ReturnsAllEntities()
	{
		Seed("Elf");
		Seed("Dwarf");

		var result = Sut.GetAll();

		Assert.IsTrue(result.IsSuccess);
		Assert.HasCount(2, result.Value!);
	}

	[TestMethod]
	public async Task GetAllAsync_ReturnsAllEntities()
	{
		await SeedAsync("Elf");
		await SeedAsync("Dwarf");

		var result = await Sut.GetAllAsync(Ct);

		Assert.IsTrue(result.IsSuccess);
		Assert.HasCount(2, result.Value!);
	}

	[TestMethod]
	public void GetAll_ReturnsEmptyList_WhenTableIsEmpty()
	{
		var result = Sut.GetAll();

		Assert.IsTrue(result.IsSuccess);
		Assert.IsEmpty(result.Value!);
	}

	[TestMethod]
	public async Task GetAllCustomAsync_ReturnsOnlyMatchingEntities()
	{
		await SeedAsync("Elf");
		await SeedAsync("Dwarf");

		var result = await Sut.GetAllCustomAsync(e => e.Name == "Dwarf", Ct);

		Assert.IsTrue(result.IsSuccess);
		Assert.HasCount(1, result.Value!);
		Assert.AreEqual("Dwarf", result.Value![0].Name);
	}

	[TestMethod]
	public void Get_ReturnsEntity_WhenFound()
	{
		var seeded = Seed();

		var result = Sut.Get(seeded.Id);

		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual("Elf", result.Value!.Name);
	}

	[TestMethod]
	public async Task GetAsync_ReturnsEntity_WhenFound()
	{
		var seeded = await SeedAsync();

		var result = await Sut.GetAsync(seeded.Id, Ct);

		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual("Elf", result.Value!.Name);
	}

	[TestMethod]
	public void Get_ReturnsNotFound_WhenMissing()
	{
		var result = Sut.Get(999);

		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("Db.NotFound", result.Error!.Code);
	}

	[TestMethod]
	public async Task GetAsync_ReturnsNotFound_WhenMissing()
	{
		var result = await Sut.GetAsync(999, Ct);

		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("Db.NotFound", result.Error!.Code);
	}

	[TestMethod]
	public async Task GetCustomAsync_ReturnsFirstMatch()
	{
		await SeedAsync("Elf");
		await SeedAsync("Dwarf");

		var result = await Sut.GetCustomAsync(e => e.Name == "Dwarf", Ct);

		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual("Dwarf", result.Value!.Name);
	}

	[TestMethod]
	public async Task GetCustomAsync_ReturnsNotFound_WhenNothingMatches()
	{
		await SeedAsync("Elf");

		var result = await Sut.GetCustomAsync(e => e.Name == "Orc", Ct);

		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("Db.NotFound", result.Error!.Code);
	}

	[TestMethod]
	public async Task GetOrCreateAsync_CreatesDefault_WhenTableIsEmpty()
	{
		var result = await Sut.GetOrCreateAsync(() => NewEntity("Default"), Ct);

		Assert.IsTrue(result.IsSuccess);
		Assert.AreNotEqual(0, result.Value!.Id);
		Assert.HasCount(1, Sut.GetAll().Value!);
	}

	[TestMethod]
	public async Task GetOrCreateAsync_ReturnsExisting_WhenTableHasEntity()
	{
		var seeded = await SeedAsync("Existing");

		var result = await Sut.GetOrCreateAsync(() => NewEntity("Default"), Ct);

		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(seeded.Id, result.Value!.Id);
		Assert.HasCount(1, Sut.GetAll().Value!);
	}

	[TestMethod]
	public async Task GetAllAsync_Throws_WhenTokenIsCancelled()
	{
		using var cts = new CancellationTokenSource();
		await cts.CancelAsync();

		await Assert.ThrowsAsync<OperationCanceledException>(() => Sut.GetAllAsync(cts.Token));
	}
}
