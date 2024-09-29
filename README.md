# Demolite.Db

Demolite.Db is a simple database wrapper package utilizing EF Core and Serilog.
It is meant to reduce the amount of boilerplate code needed for creating new projects and for handling db operations.

Using this package works as follows:

1. Create an abstract repository for your DbContext
```csharp
    public abstract class AbstractMyContextRepository<T>(IDbContextFactory<MyDbContext> contextFactory)
        : AbstractBaseRepository<T, MyContext> 
        where T: class, IHasOperation
    {
        
    }
```

2. In the context, override the methods for GetContextAsync() and GetContext().
In this example, a context factory is injected, but you can use any method to instantiate your DbContext.


3. Create a db model which inherits AbstractDbItem, or create your own AbstractDbItem which implements the IDbItem and IHasOperation interface
```csharp
    public class ModelType : AbstractDbItem
```

4. Create an empty interface class for your repository which implements IDbRepository for your model class.
```csharp
    public interface IModelTypeRepository : IDbRepository<ModelType>;
```

5. Implement the interface and inherit from your abstract repository in your model type repository:
```csharp
    public class ModelTypeRepository(IDbContextFactory<MyDbContext> contextFactory) 
        : AbstractMyContextRepository<ModelType>(contextFactory), IModelTypeRepository
```

6. Now just register the repository in your DI and it is ready to use.