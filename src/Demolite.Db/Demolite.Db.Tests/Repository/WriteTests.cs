namespace Demolite.Db.Tests.Repository;

[TestClass]
public class WriteTests : RepositoryTestBase
{
	[TestMethod]
	public void Create_PersistsEntity()
	{
		var result = Sut.Create(NewEntity());

		Assert.IsTrue(result.IsSuccess);
		Assert.AreNotEqual(0, result.Value!.Id);
		Assert.IsTrue(Sut.Get(result.Value.Id).IsSuccess);
	}

	[TestMethod]
	public async Task CreateAsync_PersistsEntity()
	{
		var result = await Sut.CreateAsync(NewEntity(), Ct);

		Assert.IsTrue(result.IsSuccess);
		Assert.AreNotEqual(0, result.Value!.Id);
		Assert.IsTrue((await Sut.GetAsync(result.Value.Id, Ct)).IsSuccess);
	}

	[TestMethod]
	public void Create_ReturnsSaveFailed_OnConstraintViolation()
	{
		var result = Sut.Create(NewEntity(null!));

		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("Db.SaveFailed", result.Error!.Code);
	}

	[TestMethod]
	public async Task CreateAsync_ReturnsSaveFailed_OnConstraintViolation()
	{
		var result = await Sut.CreateAsync(NewEntity(null!), Ct);

		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("Db.SaveFailed", result.Error!.Code);
		Assert.IsNotNull(result.Error.Exception);
	}

	[TestMethod]
	public void Update_PersistsChanges()
	{
		var seeded = Seed();
		seeded.Name = "Dwarf";

		var result = Sut.Update(seeded);

		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual("Dwarf", Sut.Get(seeded.Id).Value!.Name);
	}

	[TestMethod]
	public async Task UpdateAsync_PersistsChanges()
	{
		var seeded = await SeedAsync();
		seeded.Name = "Dwarf";

		var result = await Sut.UpdateAsync(seeded, Ct);

		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual("Dwarf", (await Sut.GetAsync(seeded.Id, Ct)).Value!.Name);
	}

	[TestMethod]
	public void Delete_RemovesEntity()
	{
		var seeded = Seed();

		var result = Sut.Delete(seeded);

		Assert.IsTrue(result.IsSuccess);
		Assert.IsFalse(Sut.Get(seeded.Id).IsSuccess);
	}

	[TestMethod]
	public async Task DeleteAsync_RemovesEntity()
	{
		var seeded = await SeedAsync();

		var result = await Sut.DeleteAsync(seeded, Ct);

		Assert.IsTrue(result.IsSuccess);
		Assert.IsFalse((await Sut.GetAsync(seeded.Id, Ct)).IsSuccess);
	}
}
