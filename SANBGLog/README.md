# Background Log Service

> **🌐 Language / Ngôn ngữ:** [English](#english) | [Tiếng Việt](#tiếng-việt)

---

# English

## 📋 Description

A modern .NET background logging library with clean architecture, easy testing, and extensibility.

### Features
- ✅ Background log writing to text files
- ✅ Separate logs per service (source)
- ✅ Auto file rotation by date and size (4MB)
- ✅ Sensitive data filtering/masking
- ✅ Session tracking for requests
- ✅ **Direct DI injection** - No need for `IServiceProvider`
- ✅ **Easy unit testing** with mockable interfaces

---

## 🏗️ Architecture

```
BackgroundLogService/
├── Abstractions/              # Interfaces for DI
│   ├── ILogService.cs         # Generic typed log service (recommended)
│   ├── ILogWriter.cs          # Write logs (file, database, cloud...)
│   ├── ILogFormatter.cs       # Format logs (text, JSON...)
│   ├── ILogQueue.cs           # Queue management
│   ├── ILogFilter.cs          # Filter sensitive data
│   └── IDateTimeProvider.cs   # DateTime abstraction (testable)
├── Infrastructure/            # Implementations
│   ├── FileLogWriter.cs
│   ├── PlainTextLogFormatter.cs
│   ├── InMemoryLogQueue.cs
│   ├── DefaultLogFilter.cs
│   └── SystemDateTimeProvider.cs
├── Services/
│   ├── LogService.cs               # Main log service
│   ├── LogServiceGeneric.cs        # Generic typed service
│   ├── LogServiceFactory.cs        # Factory pattern
│   ├── LogFlushingHostedService.cs # Background flush
│   └── SessionLogService.cs
├── Models/
│   ├── LogEntry.cs
│   ├── LogBackgroundServiceConfig.cs
│   └── CustomException.cs
└── Extensions/
    ├── ExtensionLibrary.cs
    └── LogExtensionLibrary.cs
```

---

## 🚀 Installation

### Step 1: Add Project Reference

```xml
<ItemGroup>
  <ProjectReference Include="..\SANBGLog\BackgroundLogService.csproj" />
</ItemGroup>
```

Or via NuGet:

```xml
<PackageReference Include="BackgroundLogService" Version="1.0.0" />
```

---

## ⚙️ Configuration

### Step 2: Add to `appsettings.json`

```json
{
  "BackgroundLogServiceConfig": {
    "ProjectName": "SANProductService",
    "LogDirectory": "C:\\Logs",
    "MaxFileSizeBytes": 4194304,
    "FlushIntervalSeconds": 5,
    "FilterLogBySources": {
      "SANProductService": {
        "IgnoreByMethodList": ["HealthCheck", "Ping"],
        "FilterList": [
          { "PrototypeList": ["Password", "Token", "SecretKey"], "Type": 1 },
          { "PrototypeList": ["PhoneNumber"], "Type": 2, "Start": 3, "End": 3 }
        ]
      }
    }
  }
}
```

### Configuration Reference

| Field | Description |
|-------|-------------|
| `ProjectName` | **Project/Application name** - used as source for log files and filtering |
| `LogDirectory` | Root directory for log files |
| `MaxFileSizeBytes` | Max file size before rotation (default 4MB) |
| `FlushIntervalSeconds` | Interval to flush logs to file (default 5s) |
| `FilterLogBySources` | Filter config per project (key must match `ProjectName`) |
| `IgnoreByMethodList` | Methods to exclude from logging |
| `FilterList` | Sensitive data filters |

---

## 📝 Service Registration

### Step 3: Register in `Program.cs`

**Option 1: Typed Injection Only (Recommended for new projects)**

```csharp
using BackgroundLogService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);

// ✅ Log Service - Typed injection (recommended)
// Allows: ILogService<BrandService>, ILogService<OrderService>, etc.
builder.Services.AddTypedLogService(builder.Configuration);

var app = builder.Build();
```

**Option 2: Both Typed + Named Sources (Migration/Compatibility)**

```csharp
using BackgroundLogService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);

// ✅ Log Service - Supports both:
// - ILogService<T> for direct injection
// - IBackgroundLogService via IServiceProvider (legacy)
builder.Services.AddBackgroundLogService(
    builder.Configuration, 
    "SANProductService"  // Named source for legacy code
);

var app = builder.Build();
```

**Option 3: Named Sources Only (Legacy)**

```csharp
using BackgroundLogService;

var builder = WebApplication.CreateBuilder(args);

// Register typed log service - allows direct constructor injection
builder.Services.AddTypedLogService(builder.Configuration);

var app = builder.Build();
```

**Option 2: Named Sources**

```csharp
// Register with named sources
builder.Services.AddBackgroundLogService(
    builder.Configuration, 
    "ServiceA", "ServiceB"
);
```

**Option 3: Custom Implementations**

```csharp
builder.Services.AddBackgroundLogService(builder.Configuration, builder =>
{
    builder.AddSources("ServiceA", "ServiceB")
           .UseLogWriter<CustomLogWriter>()
           .UseLogFormatter<JsonLogFormatter>();
});
```

---

## 💉 Usage

### Method 1: Direct Constructor Injection (Recommended)

```csharp
using BackgroundLogService.Abstractions;

public class BrandService
{
    private readonly IBrandRepository _brandRepository;
    private readonly ILogService<BrandService> _logService;

    // ✅ Direct injection - no IServiceProvider needed!
    public BrandService(
        IBrandRepository brandRepository,
        ILogService<BrandService> logService)
    {
        _brandRepository = brandRepository;
        _logService = logService;
    }

    public async Task<Brand> CreateBrand(CreateBrandRequest request)
    {
        await _logService.WriteMessageAsync("Creating brand...");

        try
        {
            var result = await _brandRepository.Create(request);
            
            await _logService.WriteLogDataAsync("CreateBrand", request, result);
            
            return result;
        }
        catch (Exception ex)
        {
            await _logService.WriteExceptionAsync(ex);
            throw;
        }
    }
}
```

### Method 2: Using IServiceProvider (Legacy)

```csharp
using BackgroundLogService.Extensions;
using BackgroundLogService.Services.Interfaces;

public class OrderController : ControllerBase
{
    private readonly IBackgroundLogService _logService;
    private readonly ISessionLogService _sessionLogService;

    public OrderController(IServiceProvider serviceProvider)
    {
        _logService = serviceProvider.GetLogService("YourServiceName")!;
        _sessionLogService = serviceProvider.GetSessionLogService();
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        await _logService.WriteMessageAsync(_sessionLogService, "Creating order...");

        try
        {
            var response = await ProcessOrder(request);
            await _logService.WriteLogDataAsync(_sessionLogService, "CreateOrder", request, response);
            return Ok(response);
        }
        catch (Exception ex)
        {
            await _logService.WriteExceptionAsync(_sessionLogService, ex);
            throw;
        }
    }
}
```

---

## 🧪 Unit Testing

### Easy Mocking with ILogService<T>

```csharp
using Moq;
using Xunit;

public class BrandServiceTests
{
    [Fact]
    public async Task CreateBrand_ShouldLogMessage()
    {
        // Arrange
        var mockRepo = new Mock<IBrandRepository>();
        var mockLogService = new Mock<ILogService<BrandService>>();
        
        mockRepo.Setup(x => x.Create(It.IsAny<CreateBrandRequest>()))
                .ReturnsAsync(new Brand { Id = 1, Name = "Test" });

        var service = new BrandService(mockRepo.Object, mockLogService.Object);

        // Act
        await service.CreateBrand(new CreateBrandRequest { Name = "Test" });

        // Assert
        mockLogService.Verify(x => x.WriteMessageAsync("Creating brand..."), Times.Once);
        mockLogService.Verify(x => x.WriteLogDataAsync(
            "CreateBrand", 
            It.IsAny<CreateBrandRequest>(), 
            It.IsAny<Brand>()), Times.Once);
    }

    [Fact]
    public async Task CreateBrand_WhenException_ShouldLogException()
    {
        // Arrange
        var mockRepo = new Mock<IBrandRepository>();
        var mockLogService = new Mock<ILogService<BrandService>>();
        
        mockRepo.Setup(x => x.Create(It.IsAny<CreateBrandRequest>()))
                .ThrowsAsync(new InvalidOperationException("Test error"));

        var service = new BrandService(mockRepo.Object, mockLogService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateBrand(new CreateBrandRequest { Name = "Test" }));

        mockLogService.Verify(x => x.WriteExceptionAsync(It.IsAny<Exception>()), Times.Once);
    }
}
```

### Full Mock Example (Advanced)

```csharp
var mockWriter = new Mock<ILogWriter>();
var mockFormatter = new Mock<ILogFormatter>();
var mockFilter = new Mock<ILogFilter>();
var mockDateTime = new Mock<IDateTimeProvider>();

mockDateTime.Setup(x => x.Now).Returns(new DateTime(2025, 1, 1));
mockFilter.Setup(x => x.ShouldIgnoreMethod(It.IsAny<string>(), It.IsAny<string>()))
          .Returns(false);

var logService = new LogService(
    "TestSource",
    new InMemoryLogQueue(),
    new InMemoryLogQueue(),
    mockFilter.Object,
    mockFormatter.Object,
    mockWriter.Object,
    mockDateTime.Object
);
```

---

## 📁 Log File Structure

### File Path Pattern

Logs are now organized by **class name** (CategoryName) within the project folder:

```
{LogDirectory}/{SourceName}/{CategoryName}/{CategoryName}_Log_{yyyyMMdd}_{Index}.txt
{LogDirectory}/{SourceName}/{CategoryName}/{CategoryName}_Data_{yyyyMMdd}_{Index}.txt
```

**Example:** When using `ILogService<BrandService>` and `ILogService<OrderService>`:
```
C:\Logs\
└── SANProductService\              ← SourceName (from config ProjectName)
    ├── BrandService\               ← CategoryName (class name)
    │   ├── BrandService_Log_20251219_001.txt
    │   └── BrandService_Data_20251219_001.txt
    └── OrderService\
        ├── OrderService_Log_20251219_001.txt
        └── OrderService_Data_20251219_001.txt
```

### Log Formats

**Message:**
```
[2025-12-18 09:30:00.123] [abc-def-123] [MSG] Creating brand...
```

**Data (Request/Response):**
```
[2025-12-18 09:30:00.123] [abc-def-123] [DATA] CreateBrand
  Request: {"name":"Test","password":"********"}
  Response: {"id":1,"name":"Test"}
```

**Exception:**
```
[2025-12-18 09:30:00.123] [abc-def-123] [ERR] InvalidOperationException
  Message: Test error
  Method: BrandService.CreateBrand
  Source: BrandService
  StackTrace: ...
```

---

## 🔒 Filter Types

| Type | Enum | Description | Example |
|------|------|-------------|---------|
| 1 | `Hidden` | Replace entire value | `password123` → `********` |
| 2 | `PartHidden` | Show start/end, hide middle | `0901234567` → `090****567` |
| 3 | `Json2Object` | Parse JSON then filter | Filter nested objects |
| 4 | `Regex` | Replace by regex pattern | Custom pattern |
| 5 | `Array` | Summarize array | `[1,2,3,4,5]` → `{First:1,Last:5,Count:5}` |

---

## 📋 Requirements

- .NET 8.0+
- NuGet packages (auto-included):
  - `Microsoft.Extensions.Configuration`
  - `Microsoft.Extensions.DependencyInjection`
  - `Microsoft.Extensions.Hosting.Abstractions`
  - `Microsoft.Extensions.Options`
  - `Microsoft.Extensions.Logging.Abstractions`
  - `Newtonsoft.Json`

---
---

# Tiếng Việt

## 📋 Mô tả

Thư viện ghi log background cho .NET với kiến trúc hiện đại, dễ test và mở rộng.

### Tính năng
- ✅ Ghi log background vào file text
- ✅ Tách log riêng theo từng service (source)
- ✅ Tự động xoay file theo ngày và kích thước (4MB)
- ✅ Lọc/ẩn dữ liệu nhạy cảm
- ✅ Theo dõi session cho request
- ✅ **Inject trực tiếp qua DI** - Không cần `IServiceProvider`
- ✅ **Dễ dàng unit test** với các interface có thể mock

---

## 🏗️ Kiến trúc

```
BackgroundLogService/
├── Abstractions/              # Interfaces cho DI
│   ├── ILogService.cs         # Generic typed log service (khuyến nghị)
│   ├── ILogWriter.cs          # Ghi log (file, database, cloud...)
│   ├── ILogFormatter.cs       # Format log (text, JSON...)
│   ├── ILogQueue.cs           # Quản lý queue
│   ├── ILogFilter.cs          # Lọc dữ liệu nhạy cảm
│   └── IDateTimeProvider.cs   # Abstraction DateTime (testable)
├── Infrastructure/            # Implementations
│   ├── FileLogWriter.cs
│   ├── PlainTextLogFormatter.cs
│   ├── InMemoryLogQueue.cs
│   ├── DefaultLogFilter.cs
│   └── SystemDateTimeProvider.cs
├── Services/
│   ├── LogService.cs               # Service log chính
│   ├── LogServiceGeneric.cs        # Generic typed service
│   ├── LogServiceFactory.cs        # Factory pattern
│   ├── LogFlushingHostedService.cs # Background flush
│   └── SessionLogService.cs
├── Models/
│   ├── LogEntry.cs
│   ├── LogBackgroundServiceConfig.cs
│   └── CustomException.cs
└── Extensions/
    ├── ExtensionLibrary.cs
    └── LogExtensionLibrary.cs
```

---

## 🚀 Cài đặt

### Bước 1: Thêm Project Reference

```xml
<ItemGroup>
  <ProjectReference Include="..\SANBGLog\BackgroundLogService.csproj" />
</ItemGroup>
```

Hoặc qua NuGet:

```xml
<PackageReference Include="BackgroundLogService" Version="1.0.0" />
```

---

## ⚙️ Cấu hình

### Bước 2: Thêm vào `appsettings.json`

```json
{
  "BackgroundLogServiceConfig": {
    "ProjectName": "SANProductService",
    "LogDirectory": "C:\\Logs",
    "MaxFileSizeBytes": 4194304,
    "FlushIntervalSeconds": 5,
    "FilterLogBySources": {
      "SANProductService": {
        "IgnoreByMethodList": ["HealthCheck", "Ping"],
        "FilterList": [
          { "PrototypeList": ["Password", "Token", "SecretKey"], "Type": 1 },
          { "PrototypeList": ["PhoneNumber"], "Type": 2, "Start": 3, "End": 3 }
        ]
      }
    }
  }
}
```

### Bảng tham chiếu cấu hình

| Field | Mô tả |
|-------|-------|
| `ProjectName` | **Tên Project/Application** - dùng làm source cho file log và filter |
| `LogDirectory` | Thư mục gốc chứa file log |
| `MaxFileSizeBytes` | Kích thước tối đa file trước khi xoay (mặc định 4MB) |
| `FlushIntervalSeconds` | Khoảng thời gian flush log ra file (mặc định 5s) |
| `FilterLogBySources` | Cấu hình filter theo project (key phải khớp với `ProjectName`) |
| `IgnoreByMethodList` | Các method không ghi log |
| `FilterList` | Các filter dữ liệu nhạy cảm |

---

## 📝 Đăng ký Service

### Bước 3: Đăng ký trong `Program.cs`

**Cách 1: Typed Injection (Khuyến nghị)**

```csharp
using BackgroundLogService;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký typed log service - cho phép inject trực tiếp qua constructor
builder.Services.AddTypedLogService(builder.Configuration);

var app = builder.Build();
```

**Cách 2: Named Sources**

```csharp
// Đăng ký với các source có tên
builder.Services.AddBackgroundLogService(
    builder.Configuration, 
    "ServiceA", "ServiceB"
);
```

**Cách 3: Custom Implementations**

```csharp
builder.Services.AddBackgroundLogService(builder.Configuration, builder =>
{
    builder.AddSources("ServiceA", "ServiceB")
           .UseLogWriter<CustomLogWriter>()
           .UseLogFormatter<JsonLogFormatter>();
});
```

---

## 💉 Sử dụng

### Cách 1: Inject trực tiếp qua Constructor (Khuyến nghị)

```csharp
using BackgroundLogService.Abstractions;

public class BrandService
{
    private readonly IBrandRepository _brandRepository;
    private readonly ILogService<BrandService> _logService;

    // ✅ Inject trực tiếp - không cần IServiceProvider!
    public BrandService(
        IBrandRepository brandRepository,
        ILogService<BrandService> logService)
    {
        _brandRepository = brandRepository;
        _logService = logService;
    }

    public async Task<Brand> CreateBrand(CreateBrandRequest request)
    {
        await _logService.WriteMessageAsync("Đang tạo brand...");

        try
        {
            var result = await _brandRepository.Create(request);
            
            await _logService.WriteLogDataAsync("CreateBrand", request, result);
            
            return result;
        }
        catch (Exception ex)
        {
            await _logService.WriteExceptionAsync(ex);
            throw;
        }
    }
}
```

### Cách 2: Sử dụng IServiceProvider (Legacy)

```csharp
using BackgroundLogService.Extensions;
using BackgroundLogService.Services.Interfaces;

public class OrderController : ControllerBase
{
    private readonly IBackgroundLogService _logService;
    private readonly ISessionLogService _sessionLogService;

    public OrderController(IServiceProvider serviceProvider)
    {
        _logService = serviceProvider.GetLogService("YourServiceName")!;
        _sessionLogService = serviceProvider.GetSessionLogService();
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        await _logService.WriteMessageAsync(_sessionLogService, "Đang tạo đơn hàng...");

        try
        {
            var response = await ProcessOrder(request);
            await _logService.WriteLogDataAsync(_sessionLogService, "CreateOrder", request, response);
            return Ok(response);
        }
        catch (Exception ex)
        {
            await _logService.WriteExceptionAsync(_sessionLogService, ex);
            throw;
        }
    }
}
```

---

## 🧪 Unit Testing

### Dễ dàng Mock với ILogService<T>

```csharp
using Moq;
using Xunit;

public class BrandServiceTests
{
    [Fact]
    public async Task CreateBrand_ShouldLogMessage()
    {
        // Arrange
        var mockRepo = new Mock<IBrandRepository>();
        var mockLogService = new Mock<ILogService<BrandService>>();
        
        mockRepo.Setup(x => x.Create(It.IsAny<CreateBrandRequest>()))
                .ReturnsAsync(new Brand { Id = 1, Name = "Test" });

        var service = new BrandService(mockRepo.Object, mockLogService.Object);

        // Act
        await service.CreateBrand(new CreateBrandRequest { Name = "Test" });

        // Assert
        mockLogService.Verify(x => x.WriteMessageAsync("Đang tạo brand..."), Times.Once);
        mockLogService.Verify(x => x.WriteLogDataAsync(
            "CreateBrand", 
            It.IsAny<CreateBrandRequest>(), 
            It.IsAny<Brand>()), Times.Once);
    }

    [Fact]
    public async Task CreateBrand_WhenException_ShouldLogException()
    {
        // Arrange
        var mockRepo = new Mock<IBrandRepository>();
        var mockLogService = new Mock<ILogService<BrandService>>();
        
        mockRepo.Setup(x => x.Create(It.IsAny<CreateBrandRequest>()))
                .ThrowsAsync(new InvalidOperationException("Lỗi test"));

        var service = new BrandService(mockRepo.Object, mockLogService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateBrand(new CreateBrandRequest { Name = "Test" }));

        mockLogService.Verify(x => x.WriteExceptionAsync(It.IsAny<Exception>()), Times.Once);
    }
}
```

### Ví dụ Mock đầy đủ (Nâng cao)

```csharp
var mockWriter = new Mock<ILogWriter>();
var mockFormatter = new Mock<ILogFormatter>();
var mockFilter = new Mock<ILogFilter>();
var mockDateTime = new Mock<IDateTimeProvider>();

mockDateTime.Setup(x => x.Now).Returns(new DateTime(2025, 1, 1));
mockFilter.Setup(x => x.ShouldIgnoreMethod(It.IsAny<string>(), It.IsAny<string>()))
          .Returns(false);

var logService = new LogService(
    "TestSource",
    new InMemoryLogQueue(),
    new InMemoryLogQueue(),
    mockFilter.Object,
    mockFormatter.Object,
    mockWriter.Object,
    mockDateTime.Object
);
```

---

## 📁 Cấu trúc File Log

### Pattern đường dẫn file

Log được tổ chức theo **tên class** (CategoryName) trong thư mục project:

```
{LogDirectory}/{SourceName}/{CategoryName}/{CategoryName}_Log_{yyyyMMdd}_{Index}.txt
{LogDirectory}/{SourceName}/{CategoryName}/{CategoryName}_Data_{yyyyMMdd}_{Index}.txt
```

**Ví dụ:** Khi sử dụng `ILogService<BrandService>` và `ILogService<OrderService>`:
```
C:\Logs\
└── SANProductService\              ← SourceName (từ config ProjectName)
    ├── BrandService\               ← CategoryName (tên class)
    │   ├── BrandService_Log_20251219_001.txt
    │   └── BrandService_Data_20251219_001.txt
    └── OrderService\
        ├── OrderService_Log_20251219_001.txt
        └── OrderService_Data_20251219_001.txt
```

### Định dạng Log

**Message:**
```
[2025-12-18 09:30:00.123] [abc-def-123] [MSG] Đang tạo brand...
```

**Data (Request/Response):**
```
[2025-12-18 09:30:00.123] [abc-def-123] [DATA] CreateBrand
  Request: {"name":"Test","password":"********"}
  Response: {"id":1,"name":"Test"}
```

**Exception:**
```
[2025-12-18 09:30:00.123] [abc-def-123] [ERR] InvalidOperationException
  Message: Lỗi test
  Method: BrandService.CreateBrand
  Source: BrandService
  StackTrace: ...
```

---

## 🔒 Các loại Filter

| Type | Enum | Mô tả | Ví dụ |
|------|------|-------|-------|
| 1 | `Hidden` | Thay thế toàn bộ giá trị | `password123` → `********` |
| 2 | `PartHidden` | Hiện đầu/cuối, ẩn giữa | `0901234567` → `090****567` |
| 3 | `Json2Object` | Parse JSON rồi filter | Filter nested objects |
| 4 | `Regex` | Thay thế theo regex pattern | Custom pattern |
| 5 | `Array` | Rút gọn array | `[1,2,3,4,5]` → `{First:1,Last:5,Count:5}` |

### Ví dụ cấu hình Filter

```json
{
  "FilterList": [
    {
      "PrototypeList": ["Password", "Token", "ApiKey"],
      "Type": 1,
      "ReplaceBy": "********"
    },
    {
      "PrototypeList": ["PhoneNumber"],
      "Type": 2,
      "Start": 3,
      "End": 3
    },
    {
      "PrototypeList": ["Email"],
      "Type": 2,
      "Start": 2,
      "End": 4
    },
    {
      "PrototypeList": ["CardNumber"],
      "Type": 2,
      "Start": 4,
      "End": 4
    }
  ]
}
```

---

## 📋 Yêu cầu

- .NET 8.0+
- NuGet packages (tự động bao gồm):
  - `Microsoft.Extensions.Configuration`
  - `Microsoft.Extensions.DependencyInjection`
  - `Microsoft.Extensions.Hosting.Abstractions`
  - `Microsoft.Extensions.Options`
  - `Microsoft.Extensions.Logging.Abstractions`
  - `Newtonsoft.Json`

---

## 📊 So sánh cách sử dụng

| Cách cũ (IServiceProvider) | Cách mới (ILogService<T>) |
|---------------------------|---------------------------|
| `IServiceProvider provider` | `ILogService<BrandService> logService` |
| `provider.GetLogService("Name")` | Inject trực tiếp |
| Cần gọi extension method | Constructor injection thuần |
| Khó mock trong test | Dễ mock |
| Runtime errors nếu sai tên | Compile-time type safety |

