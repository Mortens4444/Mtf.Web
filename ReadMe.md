## Mtf.Web.WebAPI Library Documentation

## Overview

The `Mtf.Web.WebAPI` layer provides a generic, base implementation for ASP.NET Core controllers and DTO/model conversion. It includes:

* **`ApiControllerBase<TDto, TModel, TIdType, TRepository, TConverter>`**
  A generic CRUD controller with virtual actions for Get, Create, Update, Delete.
* **`ApiControllerBaseWithIntModelId<…>`** and **`ApiControllerBaseWithLongModelId<…>`**
  Specializations for integer- and long-typed model IDs.
* **`IConverter<TModel, TDto>`** and **`BaseConverter<TModel, TDto>`**
  A standard interface and base class for shallow‐copying between models and DTOs.
* **`PropertyCopier`**
  A helper for copying, including/excluding, or mapping properties via attributes.

---

## Namespace: `Mtf.Web.WebAPI`

### Class: `ApiControllerBase<TDto, TModel, TIdType, TRepository, TConverter>`

```csharp
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase<TDto, TModel, TIdType, TRepository, TConverter> 
    : ControllerBase
    where TIdType : struct
    where TDto     : IHaveIdWithSetter<TIdType>
    where TModel   : IHaveId<TIdType>
    where TRepository : IRepository<TModel>
    where TConverter  : IConverter<TModel, TDto>
{
    protected ILogger Logger { get; }
    protected TRepository Repository { get; }
    protected TConverter Converter { get; }

    protected ApiControllerBase(
        ILogger logger,
        TRepository repository,
        TConverter converter)
    { … }

    [HttpGet]
    public virtual ActionResult<IEnumerable<TDto>> GetAll() { … }

    [HttpGet("{id}")]
    public virtual ActionResult<TDto> GetById(TIdType id) { … }

    [HttpPost]
    public virtual ActionResult<TDto> Create(TDto dto) { … }

    [HttpPut("{id}")]
    public virtual IActionResult Update(TIdType id, TDto dto) { … }

    [HttpDelete("{id}")]
    public virtual IActionResult Delete(TIdType id) { … }

    protected abstract TModel Select(TIdType id);
}
```

* **Purpose**: Exposes basic CRUD endpoints wired to a repository and converter.
* **Type Parameters**:

  * `TDto`: DTO type (must have settable ID).
  * `TModel`: Domain model type (ID-getter).
  * `TIdType`: ID’s value type (e.g. `int`, `long`).
  * `TRepository`: Repository implementing `IRepository<TModel>`.
  * `TConverter`: Converter implementing `IConverter<TModel,TDto>`.
* **Key Methods**:

  * `GetAll()`: GET → all records.
  * `GetById(id)`: GET → record by ID.
  * `Create(dto)`: POST → insert new.
  * `Update(id, dto)`: PUT → update existing.
  * `Delete(id)`: DELETE → remove.
  * `Select(id)`: Abstract hook for fetching a single model.

---

### Class: `ApiControllerBaseWithIntModelId<TDto, TModel, TRepository, TConverter>`

```csharp
public abstract class ApiControllerBaseWithIntModelId<TDto, TModel, TRepository, TConverter>
    : ApiControllerBase<TDto, TModel, int, TRepository, TConverter>
    where TDto       : IHaveIdWithSetter<int>
    where TModel     : IHaveId<int>
    where TRepository : IRepository<TModel>
    where TConverter  : IConverter<TModel, TDto>
{
    protected ApiControllerBaseWithIntModelId(
        ILogger logger,
        TRepository repository,
        TConverter converter)
        : base(logger, repository, converter) { }

    protected override TModel Select(int id)
        => Repository.Select(id);
}
```

* **Specialization** for `int` IDs.

---

### Class: `ApiControllerBaseWithLongModelId<TDto, TModel, TRepository, TConverter>`

```csharp
public abstract class ApiControllerBaseWithLongModelId<TDto, TModel, TRepository, TConverter>
    : ApiControllerBase<TDto, TModel, long, TRepository, TConverter>
    where TDto       : IHaveIdWithSetter<long>
    where TModel     : IHaveId<long>
    where TRepository : IRepository<TModel>
    where TConverter  : IConverter<TModel, TDto>
{
    protected ApiControllerBaseWithLongModelId(
        ILogger logger,
        TRepository repository,
        TConverter converter)
        : base(logger, repository, converter) { }

    protected override TModel Select(long id)
        => Repository.Select(id);
}
```

* **Specialization** for `long` IDs.

---

## Namespace: `Mtf.Web.Interfaces`

### Interface: `IConverter<TModel, TDto>`

```csharp
public interface IConverter<TModel, TDto>
{
    TDto ToDto(TModel model);
    TModel ToModel(TDto dto);
}
```

* **Purpose**: Standard contract for converting between domain models and DTOs.

---

## Namespace: `LiveView.Web.Services`

### Class: `BaseConverter<TModel, TDto>`

```csharp
public abstract class BaseConverter<TModel, TDto>
    : IConverter<TModel, TDto>
    where TModel : class, new()
    where TDto   : class, new()
{
    public virtual TDto ToDto(TModel model)
    {
        if (model == null) return null;
        var dto = new TDto();
        PropertyCopier.CopyMatchingProperties(model, dto);
        return dto;
    }

    public virtual TModel ToModel(TDto dto)
    {
        if (dto == null) return null;
        var model = new TModel();
        PropertyCopier.CopyMatchingProperties(dto, model);
        return model;
    }
}
```

* **Purpose**: Implements shallow‐copy conversion using `PropertyCopier`.

---

## Namespace: `LiveView.Web.Services`

### Static Class: `PropertyCopier`

```csharp
public static class PropertyCopier
{
    public static void CopyMatchingProperties<TSource, TTarget>(TSource source, TTarget target) { … }
    public static void CopyAllExceptExcluded<TSource, TTarget>(TSource source, TTarget target) { … }
    public static void CopyOnlyIncluded<TSource, TTarget>(TSource source, TTarget target) { … }
    public static void CopyWithMapping<TSource, TTarget>(
        TSource source,
        TTarget target,
        Func<PropertyInfo, bool> includeFilter = null) { … }
}
```

* **Methods**:

  * `CopyMatchingProperties`: Copy all matching name/type props.
  * `CopyAllExceptExcluded`: Skip `[Exclude]`-marked props.
  * `CopyOnlyIncluded`: Copy only `[Include]`-marked props.
  * `CopyWithMapping`: Map via `[Map]` attributes & optional filter.

---

## Example Usage

```csharp
[ApiController]
[Route("api/widgets")]
public class WidgetController
    : ApiControllerBaseWithIntModelId<WidgetDto, WidgetModel, IWidgetRepo, WidgetConverter>
{
    public WidgetController(
        ILogger<WidgetController> logger,
        IWidgetRepo repo,
        WidgetConverter conv)
        : base(logger, repo, conv) { }
}
```

```csharp
public class WidgetConverter : BaseConverter<WidgetModel, WidgetDto> { }
```

This gives you a ready-to-use controller with full CRUD behavior and model/DTO mapping out of the box.
