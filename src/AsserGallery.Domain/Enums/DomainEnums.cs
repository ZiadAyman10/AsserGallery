namespace AsserGallery.Domain.Enums;

public enum ProductStatus
{
    Available = 1,
    LimitedStock = 2,
    OutOfStock = 3
}

public enum TransactionType
{
    Income = 1,
    Expense = 2
}

public enum ImageType
{
    Original = 1,
    AiEnhanced = 2
}

public enum DestinationType
{
    Page = 1,
    Group = 2
}

public enum CustomerRequestStatus
{
    New = 1,
    Contacted = 2,
    Completed = 3,
    Cancelled = 4
}

public enum ContactChannel
{
    WhatsApp = 1,
    Messenger = 2,
    Phone = 3
}
