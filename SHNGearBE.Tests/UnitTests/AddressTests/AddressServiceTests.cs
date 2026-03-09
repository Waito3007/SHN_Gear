using BackgroundLogService.Abstractions;
using Moq;
using SHNGearBE.Models.DTOs.Address;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Repositorys.Interface.Address;
using SHNGearBE.Services.Address;
using SHNGearBE.UnitOfWork;
using Xunit;
using AddressEntity = SHNGearBE.Models.Entities.Account.Address;

namespace SHNGearBE.Tests.UnitTests.AddressTests;

public class AddressServiceTests
{
    [Fact]
    public async Task CreateAsync_FirstAddress_ShouldSetDefaultAndCommit()
    {
        var accountId = Guid.NewGuid();
        var mockRepo = new Mock<IAddressRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<AddressService>>();

        AddressEntity? addedEntity = null;
        mockRepo.Setup(r => r.CountByAccountIdAsync(accountId, It.IsAny<CancellationToken>())).ReturnsAsync(0);
        mockRepo.Setup(r => r.AddAsync(It.IsAny<AddressEntity>()))
            .Callback<AddressEntity>(entity => addedEntity = entity)
            .ReturnsAsync((AddressEntity entity) => entity);

        var service = new AddressService(mockRepo.Object, mockUoW.Object, mockLog.Object);
        var request = new CreateAddressRequest
        {
            RecipientName = " Nguyen Van A ",
            PhoneNumber = " 0900000000 ",
            Province = " HCM ",
            District = " Quan 1 ",
            Ward = " Ben Nghe ",
            Street = " 123 Le Loi ",
            Note = " Nha rieng ",
            IsDefault = false
        };

        var result = await service.CreateAsync(accountId, request, CancellationToken.None);

        Assert.NotNull(addedEntity);
        Assert.True(result.IsDefault);
        Assert.Equal("Nguyen Van A", addedEntity!.RecipientName);
        Assert.Equal("0900000000", addedEntity.PhoneNumber);
        mockRepo.Verify(r => r.ClearDefaultAsync(accountId, It.IsAny<CancellationToken>()), Times.Once);
        mockUoW.Verify(u => u.BeginTransactionAsync(), Times.Once);
        mockUoW.Verify(u => u.CommitAsync(), Times.Once);
        mockLog.Verify(l => l.WriteMessageAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_MaxAddressReached_ShouldThrowBadRequest()
    {
        var accountId = Guid.NewGuid();
        var mockRepo = new Mock<IAddressRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<AddressService>>();

        mockRepo.Setup(r => r.CountByAccountIdAsync(accountId, It.IsAny<CancellationToken>())).ReturnsAsync(10);

        var service = new AddressService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        var request = new CreateAddressRequest
        {
            RecipientName = "A",
            PhoneNumber = "1",
            Province = "P",
            District = "D",
            Ward = "W",
            Street = "S"
        };

        var ex = await Assert.ThrowsAsync<ProjectException>(() => service.CreateAsync(accountId, request, CancellationToken.None));
        Assert.Equal(ResponseType.BadRequest, ex.ResponseType);
        mockUoW.Verify(u => u.BeginTransactionAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_AddressNotFound_ShouldThrowNotFound()
    {
        var accountId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        var mockRepo = new Mock<IAddressRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<AddressService>>();

        mockRepo.Setup(r => r.GetByIdAndAccountAsync(addressId, accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AddressEntity?)null);

        var service = new AddressService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        var request = new UpdateAddressRequest
        {
            RecipientName = "A",
            PhoneNumber = "1",
            Province = "P",
            District = "D",
            Ward = "W",
            Street = "S"
        };

        var ex = await Assert.ThrowsAsync<ProjectException>(() => service.UpdateAsync(addressId, accountId, request, CancellationToken.None));
        Assert.Equal(ResponseType.NotFound, ex.ResponseType);
    }

    [Fact]
    public async Task DeleteAsync_DefaultAddress_ShouldAssignNewDefault()
    {
        var accountId = Guid.NewGuid();
        var deletedId = Guid.NewGuid();
        var remaining = new AddressEntity
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            RecipientName = "B",
            PhoneNumber = "2",
            Province = "P",
            District = "D",
            Ward = "W",
            Street = "S",
            IsDefault = false
        };

        var deleted = new AddressEntity
        {
            Id = deletedId,
            AccountId = accountId,
            RecipientName = "A",
            PhoneNumber = "1",
            Province = "P",
            District = "D",
            Ward = "W",
            Street = "S",
            IsDefault = true
        };

        var mockRepo = new Mock<IAddressRepository>();
        var mockUoW = new Mock<IUnitOfWork>();
        var mockLog = new Mock<ILogService<AddressService>>();

        mockRepo.Setup(r => r.GetByIdAndAccountAsync(deletedId, accountId, It.IsAny<CancellationToken>())).ReturnsAsync(deleted);
        mockRepo.Setup(r => r.GetByAccountIdAsync(accountId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<AddressEntity> { remaining });

        var service = new AddressService(mockRepo.Object, mockUoW.Object, mockLog.Object);

        await service.DeleteAsync(deletedId, accountId, CancellationToken.None);

        Assert.True(remaining.IsDefault);
        mockRepo.Verify(r => r.DeleteAsync(deletedId), Times.Once);
        mockUoW.Verify(u => u.CommitAsync(), Times.Once);
        mockLog.Verify(l => l.WriteMessageAsync(It.IsAny<string>()), Times.Once);
    }
}
