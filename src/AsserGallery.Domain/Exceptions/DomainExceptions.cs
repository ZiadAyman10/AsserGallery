namespace AsserGallery.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class InsufficientStockException : DomainException
{
    public int ProductId { get; }
    public int? VariantId { get; }
    public int Requested { get; }
    public int Available { get; }

    public InsufficientStockException(int productId, int? variantId, int requested, int available)
        : base($"Insufficient stock for product ID {productId} (variant {variantId}). Requested: {requested}, Available: {available}.")
    {
        ProductId = productId;
        VariantId = variantId;
        Requested = requested;
        Available = available;
    }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }
}

public class ValidationException : DomainException
{
    public ValidationException(string message)
        : base(message)
    {
    }
}
