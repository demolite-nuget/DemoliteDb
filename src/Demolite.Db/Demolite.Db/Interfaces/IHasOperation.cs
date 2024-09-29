using Demolite.Db.Enum;

namespace Demolite.Db.Interfaces;

public interface IHasOperation
{
	public Operation OperationType { get; set; }
}