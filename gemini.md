# Ay.Commerce — Backend Architecture Guide

> Bu dosya AI asistanların (Claude, Copilot, Cursor vb.) projeyi anlaması için yazılmıştır.
> SADECE backend. Frontend kodu üretme.

---

## Katman Yapısı & Bağımlılık Yönü

```
API → Application → Domain
Persistence → Application → Domain
```

| Katman | Proje | Bağımlı Olduğu |
|---|---|---|
| Domain | `Ay.Commerce.Domain` | Hiçbir şey |
| Application | `Ay.Commerce.Application` | Domain |
| Persistence | `Ay.Commerce.Persistence` | Application, Domain |
| API | `Ay.Commerce.Api` | Application |

Her assembly'de marker struct vardır:
```csharp
public struct PersistenceAssembly; // veya DomainAssembly, ApplicationAssembly, ApiAssembly
```

---

## Domain Katmanı

### Base Entity

```csharp
// Domain/Entities/Common/BaseEntity.cs
namespace Ay.Commerce.Domain.Entities.Common;

public abstract class BaseEntity<T>
{
    public T Id { get; set; } = default!;
}
```

### Audit Interface

```csharp
// Domain/Entities/Common/IAuditEntity.cs
namespace Ay.Commerce.Domain.Entities.Common;

public interface IAuditEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### Entity Yazma Kuralları

- Her entity `BaseEntity<TId>` kalıtır. `TId` genellikle `int` veya `Guid`.
- Audit gereken entity'ler ayrıca `IAuditEntity` implemente eder. `CreatedAt`/`UpdatedAt` interceptor tarafından otomatik set edilir, entity içinde set etme.
- Entity'ler `Domain/Entities/{KlasorAdi}/` altına eklenir.
- Navigation property'ler `null!` yerine `= new List<T>()` ile initialize edilir.

```csharp
// Domain/Entities/Catalog/Product.cs
namespace Ay.Commerce.Domain.Entities.Catalog;

public class Product : BaseEntity<int>, IAuditEntity
{
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Category Category { get; set; } = null!;
}
```

---

## Persistence Katmanı

### AppDbContext

`Ay.Commerce.Persistence.AppDbContext` — entity'ler region bloklarıyla gruplandırılır.

Yeni entity eklerken:
```csharp
#region Catalog
public DbSet<Product> Products { get; set; }
#endregion
```

`OnModelCreating` tüm configuration'ları assembly reflection ile yükler — elle kayıt gerekmez:
```csharp
modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
```

### EF Configuration

```csharp
// Persistence/Configurations/Catalog/ProductConfiguration.cs
namespace Ay.Commerce.Persistence.Configurations.Catalog;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Price).HasPrecision(18, 4);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId);
    }
}
```

- `decimal` için her zaman `HasPrecision(18, 4)` kullan.
- Koordinat gibi yüksek hassasiyet gereken alanlar: `HasPrecision(18, 10)`.

### Audit Interceptor

`AuditDbContextInterceptor` `IAuditEntity` implemente eden tüm entity'lerde:
- `Added` → `CreatedAt = DateTime.Now`
- `Modified` → `UpdatedAt = DateTime.Now`, `CreatedAt` değiştirilmez

Entity içinde bu alanları manuel set etme.

### Generic Repository

```csharp
IGenericRepository<T, TId>
// where T : BaseEntity<TId>
// where TId : struct
```

Mevcut metodlar: `AnyAsync`, `GetByIdAsync`, `GetAll`, `GetAllListAsync`, `Where`, `AddAsync`, `AddRangeAsync`, `Update`, `Delete`, `DeleteRange`, `DeleteAsync(TId)`.

### Unit of Work

```csharp
// Servis içinde kullanım:
var repo = unitOfWork.Repository<Product, int>();
await repo.AddAsync(entity, ct);
await unitOfWork.SaveChangesAsync(ct);

// Transaction gereken durumlar:
await unitOfWork.BeginTransactionAsync();
// ... işlemler
await unitOfWork.CommitAsync(); // içinde SaveChanges + Commit + Dispose
// Hata durumunda otomatik RollbackAsync
```

### Özel Repository (Sadece Gerekirse)

Sadece `GenericRepository`'nin karşılamadığı özel sorgular gerektiğinde aç.

```csharp
// Application/Contracts/Repositories/Catalog/IProductRepository.cs
namespace Ay.Commerce.Application.Contracts.Repositories.Catalog;

public interface IProductRepository : IGenericRepository<Product, int>
{
    Task<List<ProductResponseDto>> GetAllWithCategoryAsync(CancellationToken ct = default);
}

// Persistence/Repositories/Catalog/ProductRepository.cs
namespace Ay.Commerce.Persistence.Repositories.Catalog;

public class ProductRepository(AppDbContext context)
    : GenericRepository<Product, int>(context), IProductRepository
{
    public async Task<List<ProductResponseDto>> GetAllWithCategoryAsync(CancellationToken ct = default)
        => await context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Select(x => new ProductResponseDto(x.Id, x.Name, x.Category.Name))
            .ToListAsync(ct);
}
```

`RepositoryExtensions`'daki reflection mekanizması `*Repository` ile biten tüm class'ları `I*Repository` interface'iyle otomatik kaydeder — `AddRepositories`'e elle ekleme gerekmez.

### DI Kaydı (Persistence)

```csharp
// Persistence/Extensions/RepositoryExtensions.cs
services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString, sql =>
        sql.MigrationsAssembly(typeof(PersistenceAssembly).Assembly.FullName));
    options.AddInterceptors(new AuditDbContextInterceptor());
});

services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
services.AddScoped<IEntityContext, EntityContext>();
// Özel repository'ler reflection ile otomatik kaydedilir
```

---

## Application Katmanı

### Klasör Yapısı

```
Application/
├── Contracts/
│   ├── Persistence/          ← IUnitOfWork, IGenericRepository, IEntityContext
│   ├── Repositories/         ← Özel repo interface'leri (gerekirse)
│   │   └── Catalog/
│   └── Services/             ← Servis interface'leri
│       └── Catalog/
├── Features/
│   └── Catalog/
│       └── Product/
│           ├── Create/       ← Request + Validator
│           ├── Update/       ← Request + Validator
│           ├── Dto/          ← ResponseDto
│           ├── ProductMappingProfile.cs
│           └── ProductService.cs
└── Options/                  ← Strongly-typed config sınıfları
```

### CQRS — Request & DTO

```csharp
// Features/Catalog/Product/Create/CreateProductRequest.cs
namespace Ay.Commerce.Application.Features.Catalog.Product.Create;

public record CreateProductRequest(string Name, decimal Price, int CategoryId);

// Features/Catalog/Product/Update/UpdateProductRequest.cs
namespace Ay.Commerce.Application.Features.Catalog.Product.Update;

public record UpdateProductRequest(int Id, string Name, decimal Price);

// Features/Catalog/Product/Dto/ProductResponseDto.cs
namespace Ay.Commerce.Application.Features.Catalog.Product.Dto;

public record ProductResponseDto(int Id, string Name, decimal Price, string CategoryName, DateTime CreatedAt);
```

### FluentValidation

Validator, Request ile aynı klasörde yaşar. Async kural için `IUnitOfWork` inject edilir.

```csharp
// Features/Catalog/Product/Create/CreateProductRequestValidator.cs
namespace Ay.Commerce.Application.Features.Catalog.Product.Create;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz.")
            .MaximumLength(150).WithMessage("Ürün adı en fazla 150 karakter olabilir.")
            .MustAsync(async (name, ct) =>
                !await unitOfWork.Repository<Product, int>().AnyAsync(x => x.Name == name, ct))
            .WithMessage("Bu isimde bir ürün zaten mevcut.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Fiyat sıfırdan büyük olmalıdır.");

        RuleFor(x => x.CategoryId)
            .MustAsync(async (id, ct) =>
                await unitOfWork.Repository<Category, int>().AnyAsync(x => x.Id == id, ct))
            .WithMessage("Kategori bulunamadı.");
    }
}
```

### AutoMapper

Mapping profili, ilgili entity/feature klasöründe yaşar.

```csharp
// Features/Catalog/Product/ProductMappingProfile.cs
namespace Ay.Commerce.Application.Features.Catalog.Product;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductResponseDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

        CreateMap<CreateProductRequest, Product>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()));

        CreateMap<UpdateProductRequest, Product>();
    }
}
```

### ServiceResult

Tüm servis metodları `ServiceResult<T>` veya `ServiceResult` döner.

```csharp
ServiceResult<T>.SuccessAsOk(data)          // 200
ServiceResult<T>.SuccessAsCreated(data)      // 201
ServiceResult<T>.Fail("mesaj")               // 400
ServiceResult<T>.Fail("mesaj", HttpStatusCode.NotFound) // 404

ServiceResult.SuccessAsNoContent()           // 204
ServiceResult.Fail("mesaj")                  // 400
```

### Servis Interface

```csharp
// Contracts/Services/Catalog/IProductService.cs
namespace Ay.Commerce.Application.Contracts.Services.Catalog;

public interface IProductService
{
    Task<ServiceResult<List<ProductResponseDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ServiceResult<ProductResponseDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<int>> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ServiceResult> UpdateAsync(UpdateProductRequest request, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);
}
```

### Servis Implementasyonu

**Kural:** Sadece CRUD yeterliyse `IUnitOfWork` ile `GenericRepository` kullan. Özel sorgular varsa `IProductRepository` inject et.

```csharp
// Features/Catalog/Product/ProductService.cs
namespace Ay.Commerce.Application.Features.Catalog.Product;

// Sadece CRUD:
public class ProductService(IUnitOfWork unitOfWork, IMapper mapper) : IProductService
{
    public async Task<ServiceResult<List<ProductResponseDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await unitOfWork.Repository<Product, int>().GetAllListAsync();
        return ServiceResult<List<ProductResponseDto>>.SuccessAsOk(mapper.Map<List<ProductResponseDto>>(list));
    }

    public async Task<ServiceResult<ProductResponseDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<Product, int>().GetByIdAsync(id, ct);
        if (entity is null)
            return ServiceResult<ProductResponseDto>.Fail("Ürün bulunamadı.", HttpStatusCode.NotFound);

        return ServiceResult<ProductResponseDto>.SuccessAsOk(mapper.Map<ProductResponseDto>(entity));
    }

    public async Task<ServiceResult<int>> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        var entity = mapper.Map<Product>(request);
        await unitOfWork.Repository<Product, int>().AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return ServiceResult<int>.SuccessAsCreated(entity.Id);
    }

    public async Task<ServiceResult> UpdateAsync(UpdateProductRequest request, CancellationToken ct = default)
    {
        var entity = await unitOfWork.Repository<Product, int>().GetByIdAsync(request.Id, ct);
        if (entity is null)
            return ServiceResult.Fail("Ürün bulunamadı.", HttpStatusCode.NotFound);

        mapper.Map(request, entity);
        unitOfWork.Repository<Product, int>().Update(entity);
        await unitOfWork.SaveChangesAsync(ct);
        return ServiceResult.SuccessAsNoContent();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        await unitOfWork.Repository<Product, int>().DeleteAsync(id);
        return ServiceResult.SuccessAsNoContent();
    }
}
```

### DI Kaydı (Application)

```csharp
// Application/Extensions/ApplicationServiceExtensions.cs
services.AddScoped<IProductService, ProductService>();
// Validator'lar FluentValidation assembly scan ile kaydedilir:
services.AddValidatorsFromAssemblyContaining<ApplicationAssembly>();
// AutoMapper:
services.AddAutoMapper(typeof(ApplicationAssembly).Assembly);
```

### Options Pattern

```csharp
// Application/Options/JwtSettings.cs
public class JwtSettings
{
    public const string Key = "JwtSettings";
    public string SecretKey { get; set; } = null!;
    public int AccessTokenExpirationMinutes { get; set; } = 15;
}

// DI kaydı:
services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.Key));

// Kullanım (constructor inject):
// IOptions<JwtSettings> options → options.Value.SecretKey
```

---

## API Katmanı

### CustomBaseController

Tüm controller'lar `CustomBaseController`'dan kalıtır. `[Route("api/[controller]")]` ve `[ApiController]` orada tanımlıdır.

```csharp
// Kullanılabilir overload'lar:
CreateActionResult(ServiceResult result)
CreateActionResult<T>(ServiceResult<T> result)
CreateActionResult<T>(ServiceResult<T> result, string? actionName, object? routeValues) // 201 için
```

### Controller

```csharp
// Api/Controllers/Catalog/ProductController.cs
namespace Ay.Commerce.Api.Controllers.Catalog;

public class ProductController(IProductService service) : CustomBaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => CreateActionResult(await service.GetAllAsync());

    [ServiceFilter(typeof(ValidateAndFetchFilter<Product, int>))]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => CreateActionResult(await service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request)
    {
        var result = await service.CreateAsync(request);
        return CreateActionResult(result, nameof(GetById), new { id = result.Data });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateProductRequest request)
        => CreateActionResult(await service.UpdateAsync(request));

    [ServiceFilter(typeof(ValidateAndFetchFilter<Product, int>))]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => CreateActionResult(await service.DeleteAsync(id));
}
```

### ValidateAndFetchFilter

`[ServiceFilter(typeof(ValidateAndFetchFilter<TEntity, TId>))]` — route'daki `id`'yi alır, entity'nin varlığını kontrol eder, `IEntityContext`'e set eder. Servis içinde `entityContext.Get<TEntity>()` ile ulaşılır. 404 dönmesi için controller'da gerekirse servis çağrısı atlanabilir.

---

## Checklist — Yeni Özellik Eklerken

Tüm bu adımları sırasıyla uygula:

- [ ] `Domain/Entities/{Klasor}/{Entity}.cs` — `BaseEntity<TId>` kalıt, gerekirse `IAuditEntity` ekle
- [ ] `AppDbContext`'e `DbSet<Entity>` ekle (ilgili region'a)
- [ ] `Persistence/Configurations/{Klasor}/{Entity}Configuration.cs` — `IEntityTypeConfiguration<Entity>`
- [ ] `Application/Features/{Domain}/{Entity}/Create/Create{Entity}Request.cs` — record
- [ ] `Application/Features/{Domain}/{Entity}/Create/Create{Entity}RequestValidator.cs` — `AbstractValidator`
- [ ] `Application/Features/{Domain}/{Entity}/Update/Update{Entity}Request.cs` — record
- [ ] `Application/Features/{Domain}/{Entity}/Dto/{Entity}ResponseDto.cs` — record
- [ ] `Application/Features/{Domain}/{Entity}/{Entity}MappingProfile.cs` — `Profile`
- [ ] `Application/Contracts/Services/{Domain}/I{Entity}Service.cs`
- [ ] `Application/Features/{Domain}/{Entity}/{Entity}Service.cs`
- [ ] *(Opsiyonel)* `Application/Contracts/Repositories/{Domain}/I{Entity}Repository.cs` + `Persistence/Repositories/...` implementasyonu
- [ ] `Application/Extensions/ApplicationServiceExtensions.cs`'e servis kaydı
- [ ] `Api/Controllers/{Domain}/{Entity}Controller.cs`

---

## Genel Kurallar

- **Soft delete yok.** Silme işlemi `ExecuteDeleteAsync` ile fiziksel silme.
- **Transaction:** Birden fazla aggregate değişiyorsa `BeginTransactionAsync / CommitAsync` kullan.
- **Türkçe hata mesajları** `ServiceResult.Fail(...)` içinde.
- **Frontend yok.** Razor, Blazor, JS, CSS, HTML üretme.
- **Her katmanda namespace klasör yapısıyla birebir eşleşir.**
- **`null!`** sadece required reference type property'lerde kullanılır; koleksiyonlar `= new List<T>()` ile init edilir.
