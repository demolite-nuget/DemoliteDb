namespace Demolite.Db.Tests.Repository;

[TestClass]
public class BatchTests : RepositoryTestBase
{
	[TestMethod]
	public void CreateMany_PersistsAllEntities()
	{
		var result = Sut.CreateMany([NewEntity("Elf"), NewEntity("Dwarf")]);

		Assert.IsTrue(result.IsSuccess);
		Assert.HasCount(2, Sut.GetAll().Value!);
	}

	[TestMethod]
	public async Task CreateManyAsync_PersistsAllEntities()
	{
		var result = await Sut.CreateManyAsync([NewEntity("Elf"), NewEntity("Dwarf")], Ct);

		Assert.IsTrue(result.IsSuccess);
		Assert.HasCount(2, (await Sut.GetAllAsync(Ct)).Value!);
	}

	[TestMethod]
	public void CreateMany_SavesNothing_WhenOneEntityFails()
	{
		var result = Sut.CreateMany([NewEntity("Elf"), NewEntity(null!)]);

		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("Db.SaveFailed", result.Error!.Code);
		Assert.IsEmpty(Sut.GetAll().Value!);
	}

	[TestMethod]
	public async Task CreateManyAsync_SavesNothing_WhenOneEntityFails()
	{
		var result = await Sut.CreateManyAsync([NewEntity("Elf"), NewEntity(null!)], Ct);

		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("Db.SaveFailed", result.Error!.Code);
		Assert.IsEmpty((await Sut.GetAllAsync(Ct)).Value!);
	}

	[TestMethod]
	public void UpdateMany_PersistsAllChanges()
	{
		var first = Seed("Elf");
		var second = Seed("Dwarf");
		first.Name = "Orc";
		second.Name = "Troll";

		var result = Sut.UpdateMany([first, second]);

		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual("Orc", Sut.Get(first.Id).Value!.Name);
		Assert.AreEqual("Troll", Sut.Get(second.Id).Value!.Name);
	}

	[TestMethod]
	public async Task UpdateManyAsync_PersistsAllChanges()
	{
		var first = await SeedAsync("Elf");
		var second = await SeedAsync("Dwarf");
		first.Name = "Orc";
		second.Name = "Troll";

		var result = await Sut.UpdateManyAsync([first, second], Ct);

		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual("Orc", (await Sut.GetAsync(first.Id, Ct)).Value!.Name);
		Assert.AreEqual("Troll", (await Sut.GetAsync(second.Id, Ct)).Value!.Name);
	}

	[TestMethod]
	public void UpdateMany_SavesNothing_WhenOneEntityFails()
	{
		var first = Seed("Elf");
		var second = Seed("Dwarf");
		first.Name = "Orc";
		second.Name = null!;

		var result = Sut.UpdateMany([first, second]);

		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("Elf", Sut.Get(first.Id).Value!.Name);
	}

	[TestMethod]
	public void DeleteMany_RemovesAllEntities()
	{
		var first = Seed("Elf");
		var second = Seed("Dwarf");

		var result = Sut.DeleteMany([first, second]);

		Assert.IsTrue(result.IsSuccess);
		Assert.IsEmpty(Sut.GetAll().Value!);
	}

	[TestMethod]
	public async Task DeleteManyAsync_RemovesAllEntities()
	{
		var first = await SeedAsync("Elf");
		var second = await SeedAsync("Dwarf");

		var result = await Sut.DeleteManyAsync([first, second], Ct);

		Assert.IsTrue(result.IsSuccess);
		Assert.IsEmpty((await Sut.GetAllAsync(Ct)).Value!);
	}
}
