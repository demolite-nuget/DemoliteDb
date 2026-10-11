# Demolite.Db

An abstract EF Core repository built on `IDbContextFactory<T>`.

- Every call creates its own short-lived context and disposes it afterwards.
- Every call returns a `Result<T>` from `Demolite.Core`. Failures are returned, not thrown.

## Entities

Entities inherit from `AbstractDbItem`. It provides an `int` key named `Id`.

```csharp
public sealed class Item : AbstractDbItem
{
    public required string Name { get; set; }
}
```

For another key type, use `AbstractDbItem<TKey>`. To change how the key is generated, override `Id` in your entity.

```csharp
public sealed class Document : AbstractDbItem<Guid>
{
    public required string Title { get; set; }
}
```

## Repositories

Inherit from `AbstractRepository<TEntity, TKey, TContext>`. It needs the entity, the key type and your `DbContext`. The factory is passed to the base class.

```csharp
public sealed class ItemRepository(IDbContextFactory<AppDbContext> factory)
    : AbstractRepository<Item, int, AppDbContext>(factory);
```

### Best practice: one base repository

Writing `TKey` and the context type in every repository is repetitive. Define one base repository for your application instead. It fixes the key type and the context. Your repositories then only name the entity.

```csharp
public abstract class AppRepository<TEntity>(IDbContextFactory<AppDbContext> factory)
    : AbstractRepository<TEntity, int, AppDbContext>(factory)
    where TEntity : class, IDbItem<int>;

public sealed class ItemRepository(IDbContextFactory<AppDbContext> factory)
    : AppRepository<Item>(factory);
```

The base repository is also the right place for behavior shared by all repositories, for example a different way to create the context.

## Registration

Register the context factory. The repositories receive it through constructor injection.

```csharp
services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString));
services.AddScoped<ItemRepository>();
```

Register the repository under its interface if you want to mock it in tests.

```csharp
services.AddScoped<IDbRepository<Item, int>, ItemRepository>();
```

## Usage

Check `IsSuccess` before you use `Value`. On failure, `Error` describes the problem.

```csharp
var result = await repository.GetAsync(id, ct);

if (!result.IsSuccess)
    return result.Error;

var item = result.Value;
```

Errors created by this package:

| Code | Meaning |
|---|---|
| `Db.NotFound` | A single entity was requested and does not exist. |
| `Db.SaveFailed` | EF Core threw a `DbUpdateException` while saving. The exception is attached to the error. |

## Operations

| Operation | Methods |
|---|---|
| Read all | `GetAll`, `GetAllAsync`, `GetAllCustomAsync` |
| Read one | `Get`, `GetAsync`, `GetCustomAsync`, `GetOrCreateAsync` |
| Write one | `Create`, `Update`, `Delete` (sync and async) |
| Write many | `CreateMany`, `UpdateMany`, `DeleteMany` (sync and async) |

- The `Custom` methods take an `Expression<Func<TEntity, bool>>`, so the filter runs in the database.
- `GetOrCreateAsync` returns the first entity. If the table is empty, it stores and returns the entity built by the factory method you pass in. This is useful for settings tables with one row.
- The `Many` methods save all entities in a single `SaveChanges` call, so they succeed or fail together.
- Async methods take an optional `CancellationToken`.

## Customizing

All public members are `virtual`. Override them in your repository to add behavior, for example `Include` calls.

Two protected members control the context handling:

- `CreateContext` creates the context for one call.
- `GetSet` selects the `DbSet` the repository works on.

```csharp
public sealed class ItemRepository(IDbContextFactory<AppDbContext> factory)
    : AppRepository<Item>(factory)
{
    protected override DbSet<Item> GetSet(AppDbContext context) => context.Items;
}
```

To add your own queries, create a context with `CreateContext` and return a `Result` yourself.
