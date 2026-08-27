using AsserGallery.Domain.Common;
using AsserGallery.Domain.Enums;

namespace AsserGallery.Domain.Entities;

public class FinancialTransaction : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public TransactionType Type { get; set; } = TransactionType.Expense;
    public string Category { get; set; } = "General"; // e.g. StockPurchase, Packaging, Delivery, Ads, SalesRevenue, Other

    public int? LinkedProductId { get; set; }
    public Product? LinkedProduct { get; set; }
}

public class CustomerRequest : BaseEntity
{
    public string CustomerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public ContactChannel PreferredChannel { get; set; } = ContactChannel.WhatsApp;
    public string? Message { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    public CustomerRequestStatus Status { get; set; } = CustomerRequestStatus.New;
    public string? AdminNotes { get; set; }
}

public class FacebookDestination : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public DestinationType DestinationType { get; set; } = DestinationType.Group;
    public string TargetIdOrUrl { get; set; } = string.Empty; // Page ID or Group URL
    public string? AccessToken { get; set; } // for Pages
    public bool IsActive { get; set; } = true;

    public ICollection<ProductPost> Posts { get; set; } = new List<ProductPost>();
}

public class ProductPost : BaseEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int FacebookDestinationId { get; set; }
    public FacebookDestination? FacebookDestination { get; set; }

    public DateTime PostedAt { get; set; } = DateTime.UtcNow;
    public string PostContent { get; set; } = string.Empty;
    public string? PostUrlOrId { get; set; }
    public string Status { get; set; } = "Posted"; // e.g. Posted, Pending, Failed
    public string? Notes { get; set; }
}

public class StoreSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}
