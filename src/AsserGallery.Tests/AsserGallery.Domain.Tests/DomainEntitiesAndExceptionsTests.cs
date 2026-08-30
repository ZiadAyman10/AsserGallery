using AsserGallery.Domain.Entities;
using AsserGallery.Domain.Enums;
using AsserGallery.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace AsserGallery.Domain.Tests;

public class DomainEntitiesAndExceptionsTests
{
    [Fact]
    public void SaleItem_SubTotal_ShouldCalculateQuantityTimesUnitPrice()
    {
        var item = new SaleItem
        {
            Quantity = 3,
            UnitPrice = 250m
        };

        item.SubTotal.Should().Be(750m);
    }

    [Fact]
    public void Category_Properties_ShouldInitializeDefaults()
    {
        var cat = new Category
        {
            Name = "Women",
            ArabicName = "حريمي",
            IsActive = true
        };

        cat.Name.Should().Be("Women");
        cat.IsActive.Should().BeTrue();
        cat.SubCategories.Should().NotBeNull();
        cat.SubCategories.Should().BeEmpty();
    }

    [Fact]
    public void FinancialTransaction_DefaultType_ShouldBeExpense()
    {
        var tx = new FinancialTransaction
        {
            Title = "Packing tape",
            Amount = 50m
        };

        tx.Type.Should().Be(TransactionType.Expense);
        tx.Date.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CustomerRequest_DefaultStatus_ShouldBeNew()
    {
        var req = new CustomerRequest
        {
            CustomerName = "Salma",
            PhoneNumber = "01011111111"
        };

        req.Status.Should().Be(CustomerRequestStatus.New);
        req.PreferredChannel.Should().Be(ContactChannel.WhatsApp);
    }

    [Fact]
    public void FacebookDestination_Properties_ShouldHoldConfiguration()
    {
        var dest = new FacebookDestination
        {
            Name = "VIP Customer Group",
            DestinationType = DestinationType.Group,
            TargetIdOrUrl = "https://facebook.com/groups/12345",
            IsActive = true
        };

        dest.DestinationType.Should().Be(DestinationType.Group);
        dest.IsActive.Should().BeTrue();
        dest.Posts.Should().NotBeNull();
    }

    [Fact]
    public void StoreSetting_Properties_ShouldSetKeyValue()
    {
        var setting = new StoreSetting
        {
            Key = "Currency",
            Value = "EGP",
            Description = "Primary currency display"
        };

        setting.Key.Should().Be("Currency");
        setting.Value.Should().Be("EGP");
        setting.Description.Should().Be("Primary currency display");
    }

    [Fact]
    public void NotFoundException_ShouldSetEntityAndKeyMessage()
    {
        var ex = new NotFoundException("Product", 42);
        ex.Message.Should().Contain("Product");
        ex.Message.Should().Contain("42");
    }

    [Fact]
    public void InsufficientStockException_ShouldSetProductNameAndStockDetails()
    {
        var ex = new InsufficientStockException(1, 2, 5, 2);
        ex.Message.Should().Contain("1");
        ex.Message.Should().Contain("5");
        ex.Message.Should().Contain("2");
    }
}
