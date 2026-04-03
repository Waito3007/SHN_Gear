using BackgroundLogService.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SHNGearBE.Data;
using SHNGearBE.Infrastructure.Payment;
using SHNGearBE.Models.DTOs.Cart;
using SHNGearBE.Models.DTOs.Order;
using SHNGearBE.Models.Entities.Account;
using SHNGearBE.Models.Entities.Order;
using SHNGearBE.Models.Entities.Product;
using SHNGearBE.Models.Enums;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Repositorys.Interface.Account;
using SHNGearBE.Repositorys.Interface.Address;
using SHNGearBE.Repositorys.Interface.Order;
using SHNGearBE.Services.Cart;
using SHNGearBE.Services.Order;
using SHNGearBE.Services.Interfaces.Cart;
using SHNGearBE.UnitOfWork;
using SHNGearMailService.Abstractions;
using SHNGearMailService.Models;
using Xunit;

namespace SHNGearBE.Tests.UnitTests.OrderTests;

public class OrderServiceHardeningTests
{
    [Fact]
    public async Task CreateOrderAsync_WhenIdempotencyKeyReplays_ShouldReturnExistingOrderWithoutCreatingNewOne()
    {
        var accountId = Guid.NewGuid();
        var existingOrder = new Order
        {
            Id = Guid.NewGuid(),
            Code = "ORD20260402120000123",
            AccountId = accountId,
            DeliveryAddressId = Guid.NewGuid(),
            IdempotencyKey = "checkout-key-1",
            Status = OrderStatus.Pending,
            PaymentProvider = PaymentProvider.Cod,
            PaymentStatus = PaymentStatus.Pending,
            SubTotal = 120_000m,
            ShippingFee = 0m,
            TotalAmount = 120_000m,
            Items =
            {
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = Guid.NewGuid(),
                    ProductVariantId = Guid.NewGuid(),
                    ProductNameSnapshot = "Product",
                    VariantNameSnapshot = "Default",
                    SkuSnapshot = "SKU-001",
                    UnitPrice = 120_000m,
                    Quantity = 1,
                    SubTotal = 120_000m
                }
            }
        };

        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockAccountRepository = new Mock<IAccountRepository>();
        var mockAddressRepository = new Mock<IAddressRepository>();
        var mockCartService = new Mock<ICartService>();
        var mockStrategyResolver = new Mock<IPaymentStrategyResolver>();
        var mockPayPalGateway = new Mock<IPayPalGatewayService>();
        var mockEmailService = new Mock<IEmailService>();
        var mockTemplateRenderer = new Mock<IEmailTemplateRenderer>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<OrderService>>();

        var context = CreateContext();
        mockUoW.SetupGet(u => u.Context).Returns(context);
        mockLog.Setup(l => l.WriteMessageAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        mockAddressRepository.Setup(r => r.GetByIdAndAccountAsync(existingOrder.DeliveryAddressId, accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Address { Id = existingOrder.DeliveryAddressId, AccountId = accountId, RecipientName = "A", PhoneNumber = "1", Province = "P", District = "D", Ward = "W", Street = "S" });
        mockCartService.Setup(s => s.GetCartAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CartDto { AccountId = accountId, Items = new List<CartItemDto> { new() { ProductVariantId = Guid.NewGuid(), ProductName = "Product", VariantName = "Default", Sku = "SKU-001", UnitPrice = 120_000m, Currency = "VND", Quantity = 1, SubTotal = 120_000m, AvailableStock = 10 } }, TotalAmount = 120_000m, TotalItems = 1, UpdatedAt = DateTime.UtcNow });
        mockOrderRepository.Setup(r => r.GetByIdempotencyKeyAsync(accountId, "checkout-key-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        var service = CreateService(
            mockOrderRepository,
            mockAccountRepository,
            mockAddressRepository,
            mockCartService,
            mockStrategyResolver,
            mockPayPalGateway,
            mockEmailService,
            mockTemplateRenderer,
            context,
            mockUoW,
            mockLog);

        var request = new CreateOrderRequest
        {
            DeliveryAddressId = existingOrder.DeliveryAddressId,
            PaymentProvider = PaymentProvider.Cod,
            IdempotencyKey = "checkout-key-1"
        };

        var result = await service.CreateOrderAsync(accountId, request, CancellationToken.None);

        Assert.Equal(existingOrder.Id, result.Id);
        Assert.Equal(existingOrder.Code, result.Code);
        mockOrderRepository.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
        mockUoW.Verify(u => u.BeginTransactionAsync(), Times.Never);
        mockUoW.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task ProcessPayPalWebhookAsync_WhenEventAlreadyStored_ShouldIgnoreDuplicate()
    {
        var context = CreateContext();
        context.WebhookEvents.Add(new WebhookEvent
        {
            Id = Guid.NewGuid(),
            Provider = "PayPal",
            EventId = "WH-123",
            EventType = "PAYMENT.CAPTURE.COMPLETED",
            CaptureId = "CAP-123",
            ProcessedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockAccountRepository = new Mock<IAccountRepository>();
        var mockAddressRepository = new Mock<IAddressRepository>();
        var mockCartService = new Mock<ICartService>();
        var mockStrategyResolver = new Mock<IPaymentStrategyResolver>();
        var mockPayPalGateway = new Mock<IPayPalGatewayService>();
        var mockEmailService = new Mock<IEmailService>();
        var mockTemplateRenderer = new Mock<IEmailTemplateRenderer>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<OrderService>>();

        mockUoW.SetupGet(u => u.Context).Returns(context);
        mockLog.Setup(l => l.WriteMessageAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        var service = CreateService(
            mockOrderRepository,
            mockAccountRepository,
            mockAddressRepository,
            mockCartService,
            mockStrategyResolver,
            mockPayPalGateway,
            mockEmailService,
            mockTemplateRenderer,
            context,
            mockUoW,
            mockLog);

        await service.ProcessPayPalWebhookAsync("WH-123", "PAYMENT.CAPTURE.COMPLETED", "CAP-123", CancellationToken.None);

        Assert.Equal(1, await context.WebhookEvents.CountAsync());
        mockLog.Verify(l => l.WriteMessageAsync(It.Is<string>(message => message.Contains("duplicate", StringComparison.OrdinalIgnoreCase))), Times.Once);
    }

    [Fact]
    public async Task CancelMyOrderAsync_WhenPendingPayPalOrderCancelled_ShouldRefundAndStoreRecord()
    {
        var accountId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var deliveryAddressId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        var context = CreateContext();
        SeedOrderGraph(context, accountId, orderId, deliveryAddressId, variantId);

        var order = await context.Orders
            .Include(o => o.Items)
            .FirstAsync(o => o.Id == orderId);

        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockAccountRepository = new Mock<IAccountRepository>();
        var mockAddressRepository = new Mock<IAddressRepository>();
        var mockCartService = new Mock<ICartService>();
        var mockStrategyResolver = new Mock<IPaymentStrategyResolver>();
        var mockPayPalGateway = new Mock<IPayPalGatewayService>();
        var mockEmailService = new Mock<IEmailService>();
        var mockTemplateRenderer = new Mock<IEmailTemplateRenderer>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<OrderService>>();

        mockUoW.SetupGet(u => u.Context).Returns(context);
        mockUoW.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        mockUoW.Setup(u => u.CommitAsync()).Returns(() => context.SaveChangesAsync());
        mockUoW.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);
        mockUoW.Setup(u => u.SaveAsync()).Returns(Task.CompletedTask);
        mockLog.Setup(l => l.WriteMessageAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        mockOrderRepository.Setup(r => r.GetByIdAndAccountAsync(orderId, accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        mockOrderRepository.Setup(r => r.UpdateAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);
        mockAccountRepository.Setup(r => r.GetAccountWithDetailsAsync(accountId))
            .ReturnsAsync(new Account
            {
                Id = accountId,
                Email = "customer@example.com",
                Username = "customer",
                PasswordHash = "hash",
                Salt = "salt",
                AccountDetail = new AccountDetail
                {
                    Id = Guid.NewGuid(),
                    AccountId = accountId,
                    Name = "Customer Name",
                    FirstName = "Customer"
                }
            });

        mockPayPalGateway.Setup(s => s.GetCaptureAmountAsync("CAPTURE-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PayPalCaptureAmountResult
            {
                Success = true,
                AmountUsd = 10m,
                CurrencyCode = "USD"
            });
        mockPayPalGateway.Setup(s => s.RefundCaptureAsync("CAPTURE-123", 10m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PayPalRefundResult
            {
                Success = true,
                RefundId = "REF-123",
                RefundedAmountUsd = 10m
            });
        mockTemplateRenderer.Setup(r => r.RenderRefundCompletedTemplate(It.IsAny<RefundCompletedEmailTemplateData>()))
            .Returns(new RenderedEmailTemplate { Subject = "Refund", HtmlBody = "<p>Refund</p>" });
        mockEmailService.Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailSendResult.Ok());

        var service = CreateService(
            mockOrderRepository,
            mockAccountRepository,
            mockAddressRepository,
            mockCartService,
            mockStrategyResolver,
            mockPayPalGateway,
            mockEmailService,
            mockTemplateRenderer,
            context,
            mockUoW,
            mockLog);

        var result = await service.CancelMyOrderAsync(accountId, orderId, "Change of mind", CancellationToken.None);

        Assert.Equal(OrderStatus.Cancelled, result.Status);
        Assert.Equal(PaymentStatus.Refunded, result.PaymentStatus);
        Assert.Single(context.RefundRecords);
        Assert.Equal(3, (await context.ProductVariants.FirstAsync(v => v.Id == variantId)).Quantity);
        mockEmailService.Verify(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        mockLog.Verify(l => l.WriteMessageAsync(It.Is<string>(message => message.Contains("Order cancelled by customer"))), Times.Once);
    }

    private static OrderService CreateService(
        Mock<IOrderRepository> orderRepository,
        Mock<IAccountRepository> accountRepository,
        Mock<IAddressRepository> addressRepository,
        Mock<ICartService> cartService,
        Mock<IPaymentStrategyResolver> strategyResolver,
        Mock<IPayPalGatewayService> payPalGatewayService,
        Mock<IEmailService> emailService,
        Mock<IEmailTemplateRenderer> templateRenderer,
        ApplicationDbContext context,
        Mock<IUnitOfWork> unitOfWork,
        Mock<ILogService<OrderService>> logService)
    {
        return new OrderService(
            orderRepository.Object,
            accountRepository.Object,
            addressRepository.Object,
            cartService.Object,
            strategyResolver.Object,
            payPalGatewayService.Object,
            emailService.Object,
            templateRenderer.Object,
            context,
            unitOfWork.Object,
            logService.Object);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static void SeedOrderGraph(ApplicationDbContext context, Guid accountId, Guid orderId, Guid deliveryAddressId, Guid variantId)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Category",
            Slug = "category"
        };

        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            Name = "Brand"
        };

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Code = "PRD-001",
            Name = "Product",
            Slug = "product",
            CategoryId = category.Id,
            BrandId = brand.Id,
            Category = category,
            Brand = brand
        };

        var variant = new ProductVariant
        {
            Id = variantId,
            ProductId = product.Id,
            Product = product,
            Sku = "SKU-001",
            Name = "Default",
            Quantity = 2,
            ReservedStock = 0,
            SafetyStock = 0
        };

        var order = new Order
        {
            Id = orderId,
            AccountId = accountId,
            DeliveryAddressId = deliveryAddressId,
            Code = "ORD20260402120000999",
            Status = OrderStatus.Pending,
            PaymentProvider = PaymentProvider.PayPal,
            PaymentStatus = PaymentStatus.Completed,
            SubTotal = 120_000m,
            ShippingFee = 0m,
            TotalAmount = 120_000m,
            PaymentTransactionId = "CAPTURE-123",
            Items =
            {
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductVariantId = variantId,
                    ProductNameSnapshot = "Product",
                    VariantNameSnapshot = "Default",
                    SkuSnapshot = "SKU-001",
                    UnitPrice = 120_000m,
                    Quantity = 1,
                    SubTotal = 120_000m
                }
            }
        };

        context.Categories.Add(category);
        context.Brands.Add(brand);
        context.Products.Add(product);
        context.ProductVariants.Add(variant);
        context.Orders.Add(order);
        context.SaveChanges();
    }
}
