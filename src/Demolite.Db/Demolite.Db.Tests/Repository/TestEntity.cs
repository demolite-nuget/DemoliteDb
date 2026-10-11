using Demolite.Db.Models;

namespace Demolite.Db.Tests.Repository;

public sealed class TestEntity : AbstractDbItem
{
	public required string Name { get; set; }
}
