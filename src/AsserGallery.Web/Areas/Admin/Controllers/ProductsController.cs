using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Application.Features.Categories.Queries;
using AsserGallery.Application.Features.Products.Commands;
using AsserGallery.Application.Features.Products.Dtos;
using AsserGallery.Application.Features.Products.Queries;
using AsserGallery.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsserGallery.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductsController : Controller
{
    private readonly IMediator _mediator;
    private readonly IImageStorageService _imageStorage;

    public ProductsController(IMediator mediator, IImageStorageService imageStorage)
    {
        _mediator = mediator;
        _imageStorage = imageStorage;
    }

    public async Task<IActionResult> Index(
        string? search = null,
        int? categoryId = null,
        ProductStatus? status = null,
        int page = 1)
    {
        var query = new GetProductsQuery(
            Search: search,
            CategoryId: categoryId,
            Status: status,
            PageNumber: page,
            PageSize: 20
        );

        var products = await _mediator.Send(query);
        var categories = await _mediator.Send(new GetCategoriesQuery(OnlyActive: false));

        ViewBag.Categories = categories;
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.Status = status;

        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var categories = await _mediator.Send(new GetCategoriesQuery(OnlyActive: true));
        var colors = await _mediator.Send(new GetColorsQuery());

        ViewBag.Categories = categories;
        ViewBag.Colors = colors;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string name,
        string arabicName,
        string? description,
        string? arabicDescription,
        decimal price,
        decimal? discountedPrice,
        int subCategoryId,
        bool isFeatured,
        int displayOrder,
        int[] colorIds,
        int[] quantities,
        IFormFile? originalPhotoFile,
        IFormFile? aiPhotoFile,
        string? originalPhotoUrl,
        string? aiPhotoUrl,
        List<IFormFile>? morePhotos)
    {
        var variants = new List<CreateProductVariantInput>();
        if (colorIds != null && quantities != null)
        {
            for (int i = 0; i < colorIds.Length; i++)
            {
                if (colorIds[i] > 0)
                {
                    var qty = (i < quantities.Length) ? quantities[i] : 0;
                    variants.Add(new CreateProductVariantInput(colorIds[i], qty));
                }
            }
        }

        var images = new List<CreateProductImageInput>();
        int currentDisplayOrder = 1;

        // 1. Handle uploaded AI photo or URL (Primary studio photo)
        if (aiPhotoFile != null && aiPhotoFile.Length > 0)
        {
            using var stream = aiPhotoFile.OpenReadStream();
            var savedPath = await _imageStorage.SaveImageAsync(stream, aiPhotoFile.FileName, "products");
            images.Add(new CreateProductImageInput(savedPath, ImageType.AiEnhanced, true, currentDisplayOrder++));
        }
        else if (!string.IsNullOrWhiteSpace(aiPhotoUrl))
        {
            images.Add(new CreateProductImageInput(aiPhotoUrl.Trim(), ImageType.AiEnhanced, true, currentDisplayOrder++));
        }

        // 2. Handle uploaded original photo or URL
        if (originalPhotoFile != null && originalPhotoFile.Length > 0)
        {
            using var stream = originalPhotoFile.OpenReadStream();
            var savedPath = await _imageStorage.SaveImageAsync(stream, originalPhotoFile.FileName, "products");
            images.Add(new CreateProductImageInput(savedPath, ImageType.Original, images.Count == 0, currentDisplayOrder++));
        }
        else if (!string.IsNullOrWhiteSpace(originalPhotoUrl))
        {
            images.Add(new CreateProductImageInput(originalPhotoUrl.Trim(), ImageType.Original, images.Count == 0, currentDisplayOrder++));
        }

        // 3. Handle additional real photos (multi-file upload)
        if (morePhotos != null && morePhotos.Count > 0)
        {
            foreach (var photo in morePhotos)
            {
                if (photo.Length > 0)
                {
                    using var stream = photo.OpenReadStream();
                    var savedPath = await _imageStorage.SaveImageAsync(stream, photo.FileName, "products");
                    images.Add(new CreateProductImageInput(savedPath, ImageType.Original, images.Count == 0, currentDisplayOrder++));
                }
            }
        }

        var command = new CreateProductCommand(
            Name: name,
            ArabicName: arabicName,
            Description: description,
            ArabicDescription: arabicDescription,
            Price: price,
            DiscountedPrice: discountedPrice,
            SubCategoryId: subCategoryId,
            IsFeatured: isFeatured,
            DisplayOrder: displayOrder,
            Variants: variants,
            Images: images
        );

        var id = await _mediator.Send(command);
        TempData["SuccessMessage"] = $"Product '{name}' created successfully with {variants.Count} color variant(s) and {images.Count} image(s).";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _mediator.Send(new GetProductByIdQuery(id));
        if (product == null) return NotFound();

        var categories = await _mediator.Send(new GetCategoriesQuery(OnlyActive: false));
        var colors = await _mediator.Send(new GetColorsQuery());

        ViewBag.Categories = categories;
        ViewBag.Colors = colors;

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        string name,
        string arabicName,
        string? description,
        string? arabicDescription,
        decimal price,
        decimal? discountedPrice,
        int subCategoryId,
        bool isFeatured,
        int displayOrder,
        int[] colorIds,
        int[] quantities)
    {
        var variants = new List<CreateProductVariantInput>();
        if (colorIds != null && quantities != null)
        {
            for (int i = 0; i < colorIds.Length; i++)
            {
                if (colorIds[i] > 0)
                {
                    var qty = (i < quantities.Length) ? quantities[i] : 0;
                    variants.Add(new CreateProductVariantInput(colorIds[i], qty));
                }
            }
        }

        var command = new UpdateProductCommand(
            Id: id,
            Name: name,
            ArabicName: arabicName,
            Description: description,
            ArabicDescription: arabicDescription,
            Price: price,
            DiscountedPrice: discountedPrice,
            SubCategoryId: subCategoryId,
            IsFeatured: isFeatured,
            DisplayOrder: displayOrder,
            Variants: variants
        );

        await _mediator.Send(command);
        TempData["SuccessMessage"] = "Product updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdjustStock(int variantId, int newQuantity, string? returnUrl = null)
    {
        await _mediator.Send(new AdjustVariantStockCommand(variantId, newQuantity));
        TempData["SuccessMessage"] = "Stock updated successfully.";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddImage(int productId, IFormFile? file, List<IFormFile>? moreFiles, string? imageUrl, ImageType imageType, bool isPrimary)
    {
        int addedCount = 0;

        // Single file
        if (file != null && file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            var finalUrl = await _imageStorage.SaveImageAsync(stream, file.FileName, "products");
            await _mediator.Send(new AddProductImageCommand(productId, finalUrl, imageType, isPrimary));
            addedCount++;
        }

        // Direct URL
        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            await _mediator.Send(new AddProductImageCommand(productId, imageUrl.Trim(), imageType, isPrimary));
            addedCount++;
        }

        // Multiple additional files (real photos)
        if (moreFiles != null && moreFiles.Count > 0)
        {
            foreach (var extraFile in moreFiles)
            {
                if (extraFile.Length > 0)
                {
                    using var stream = extraFile.OpenReadStream();
                    var savedUrl = await _imageStorage.SaveImageAsync(stream, extraFile.FileName, "products");
                    await _mediator.Send(new AddProductImageCommand(productId, savedUrl, ImageType.Original, false));
                    addedCount++;
                }
            }
        }

        if (addedCount > 0)
        {
            TempData["SuccessMessage"] = $"{addedCount} image(s) added successfully.";
        }

        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(int imageId, int productId)
    {
        await _mediator.Send(new DeleteProductImageCommand(imageId));
        TempData["SuccessMessage"] = "Image deleted.";
        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteProductCommand(id));
        TempData["SuccessMessage"] = "Product deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
