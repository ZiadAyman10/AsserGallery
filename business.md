# Asser Gallery — Project & Technical Specification

**Type:** Freelance project
**Domain:** Clothing inventory, sales & business management system
**Client context:** Small clothing business currently selling through Facebook groups/pages

> This document is a restructured version of the original project brief, reorganized to lead with the technology decisions and architecture the build should follow. It is intended to be handed to an AI build agent (Antigravity) as the source spec.

---

## 1. Technology Stack (Primary Focus)

| Concern | Choice | Notes |
|---|---|---|
| Runtime / Language | **.NET 10**, C# | Latest LTS-track SDK |
| Architecture | **Clean Architecture** | See §2 for layer breakdown |
| Web framework | **ASP.NET Core** | MVC/Razor for server-rendered admin + public catalog, or Razor Pages where simpler |
| Data access | **Entity Framework Core** (Code-First) | Repository + Unit of Work pattern implemented in Infrastructure, never referenced directly from Presentation |
| Database | **SQL Server** | |
| Auth | **ASP.NET Core Identity** | Admin/staff only — public side has no accounts |
| CQRS mediator | **MediatR** | Commands/Queries in the Application layer |
| Validation | **FluentValidation** | Validators live in Application, run as MediatR pipeline behaviors |
| Object mapping | **Manual mapping** (explicit extension methods / static mapper classes, e.g. `ProductMapper.ToDto(product)`) | No AutoMapper/Mapster — mapping code lives in the Application layer, kept out of Domain, and stays explicit so field-level bugs surface at compile time instead of via reflection |
| Logging | **Serilog** | Structured logging, cross-cutting |
| Localization | **ASP.NET Core built-in localization** (`.resx`) | Arabic + English, RTL/LTR layout switching |
| Theming | CSS variables + a persisted user preference (cookie or DB setting) | Light/Dark mode |
| Image storage | Local `wwwroot`/disk for v1, swappable to **Azure Blob Storage** later | Abstracted behind an `IImageStorageService` in Application, implemented in Infrastructure |
| Frontend styling | **Bootstrap 5** (or equivalent) with a custom design system layer | See §6 for the visual language to implement |
| Testing | **xUnit** + **Moq**, with FluentAssertions | Unit tests target Domain + Application; integration tests target Infrastructure/API |
| API docs (if a separate API is exposed) | **Swagger / OpenAPI** | Only needed if the public catalog or a future mobile app consumes a Web API instead of server-rendered pages |

**Key architectural rule for the build agent:** every feature (products, stock, sales, finance, categories, image workflow, Facebook publishing) must be implemented by adding a use case in the Application layer and, where needed, an entity in Domain — never by writing business logic directly inside a controller or Razor page.

---

## 2. Clean Architecture Layout

```
src/
├── AsserGallery.Domain          // Enterprise business rules
├── AsserGallery.Application     // Use cases (CQRS), interfaces, DTOs, validation
├── AsserGallery.Infrastructure  // EF Core, external services, file storage
├── AsserGallery.Web             // ASP.NET Core MVC — Admin area + Public catalog
└── AsserGallery.Tests
    ├── AsserGallery.Domain.Tests
    ├── AsserGallery.Application.Tests
    └── AsserGallery.Infrastructure.Tests
```

**Dependency rule:** dependencies point inward only. `Web` → `Application` + `Infrastructure` (via DI registration only) → `Domain`. `Domain` has no outward dependencies at all.

### 2.1 Domain layer
Pure C# — entities, enums, value objects, domain exceptions, no EF Core or ASP.NET references.

Core entities (from the original brief, §19):
- `Product`
- `ProductImage` (distinguishes `Original` vs `AiEnhanced`)
- `Category` / `SubCategory` (self-referencing or parent/child structure, extensible)
- `Color`
- `ProductVariant` (Product × Color → quantity)
- `Sale` / `SaleItem`
- `FinancialTransaction` (typed `Income` / `Expense`)
- `CustomerRequest`
- `FacebookDestination`
- `ProductPost`

Key enums: `ProductStatus` (Available / LimitedStock / OutOfStock), `TransactionType` (Income / Expense), `ImageType` (Original / AiEnhanced).

### 2.2 Application layer
- One **Command** or **Query** per use case (e.g. `CreateProductCommand`, `AdjustStockCommand`, `RegisterSaleCommand`, `AddFinancialTransactionCommand`, `GetAvailableProductsQuery`, `GetProductDetailsQuery`).
- Interfaces for everything Infrastructure will implement: `IApplicationDbContext` or repository interfaces, `IImageStorageService`, `IWhatsAppLinkBuilder`, `IFacebookPublisher` (future).
- FluentValidation validators per command.
- DTOs for anything crossing into the Web layer — Domain entities should not be returned directly to views.

### 2.3 Infrastructure layer
- `ApplicationDbContext` (EF Core), entity configurations, migrations.
- Repository / Unit of Work implementations.
- Image storage implementation.
- Contact-link generation (WhatsApp/Messenger deep links).
- Two separate Facebook clients, kept behind their own interfaces (see §4.9 for why they can't be one thing): `IFacebookPagePublisher` (real Graph API calls) and a `IFacebookGroupAssistHelper` (content/link generation only, no posting call).

### 2.4 Web (Presentation) layer
Two areas inside one ASP.NET Core MVC app (or split into two apps later if it grows):
- **Admin area** — behind ASP.NET Core Identity, full CRUD + dashboard.
- **Public area** — no login, read-mostly, optimized for mobile.

Both areas call into Application only through MediatR — controllers stay thin (map request → command/query → view model).

### 2.5 Cross-cutting concerns
- Localization middleware (Arabic/English, RTL/LTR).
- Theme (light/dark) resolved per-request from a cookie or user setting.
- Global exception handling + Serilog request logging.
- DI composition root in `Program.cs`.

---

## 3. Business Overview *(condensed from original brief)*

The business currently sells clothing through Facebook groups/pages. This creates three problem areas the system must solve:

- **Inventory:** hard to know what's available, how many pieces/colors remain, what's sold vs. still in stock, and what's old vs. new.
- **Customer experience:** buyers rely on scrolling old Facebook posts, can't tell if a color/item is still available, and have no single place to browse everything currently for sale.
- **Financials:** no centralized record of income (sales) and expenses (stock purchases, packaging, delivery, ads, etc.).

The system is **not** a full e-commerce platform — it's a lightweight operational center: *Products → Stock → Colors → Sales → Expenses → Customers → Online Visibility*, with a simple public catalog on top.

---

## 4. Functional Scope (mapped to the architecture above)

### 4.1 Product & Inventory Management
- Product fields: name, description, category, subcategory, price, optional discounted price, date added, status, quantity, colors, images.
- Stock is tracked **per color** (`ProductVariant`), e.g. Pink: 5, Blue: 2, Black: 0.
- Customers only ever see colors with stock > 0; admin sees and controls everything.

### 4.2 Categories
- Hierarchical: `Men / Women / Children` → `Casual / Formal / Pajama` (extensible — new categories/subcategories can be added without a schema change, so this should be data-driven, not hardcoded enums).

### 4.3 Stock Management
- Add/update quantity per color, auto-decrement on sale, auto-flag out-of-stock at zero.
- Public availability states: **Available / Limited Stock / Out of Stock**. Whether out-of-stock items are hidden or shown as unavailable should be a configurable admin setting, not hardcoded.

### 4.4 Search & Filtering
- Admin: name, category, subcategory, color, date added, in/out of stock, sold/unsold, old products.
- Public: category, subcategory, price, color, availability.

### 4.5 Sales Management
- Register a sale: product, color/variant, quantity, price, date → auto-updates inventory, builds sales history.

### 4.6 Financial Management
- Transactions typed `Income` or `Expense`, each with title, description, amount, date, and an optional linked product.

### 4.7 Customer Contact
- Two paths: (1) direct WhatsApp/Messenger deep links pre-filled with product name/link, (2) a simple contact-request form (name, phone/contact, optional message) stored for the admin to follow up on.

### 4.8 Product Image Workflow
- Admin uploads a real phone photo, optionally follows an in-app guide + copies an AI prompt to enhance it externally, then uploads the enhanced image.
- Both `Original` and `AiEnhanced` images are stored; customers can toggle to see the real photo before buying.

### 4.9 Facebook Publishing *(Phase 3 — split by what Meta's API actually allows)*
Meta discontinued the Groups API around April 2024, removing every third-party permission used to publish into a Facebook Group — there is no supported way for any external app to auto-post into a group today, regardless of app size or partnership status. The Pages side of the Graph API is still live and can be integrated normally, subject to Meta App Review for the publishing permissions. Because of that split, this can't be one generic "publisher" — it's two different features that happen to share a UI:

**a) Facebook Page Publisher — real integration**
- `IFacebookPagePublisher` calls the Graph API's Page endpoints directly.
- Requires completing Meta's App Review for page-publishing permissions before it can go live.
- Fully automatable: create the product post once → publish to one or more connected Pages.

**b) Facebook Group Assistant — manual-assist tool, not an API integration**
- No API path exists for groups, so this is a content/workflow helper rather than automation:
  - Auto-generate the post text from the product (name, price, description, image).
  - One-click "Copy post content".
  - One-click "Open group on Facebook" deep link, so the admin just pastes and posts.
  - Admin manually confirms which groups it was posted to, logged in `ProductPost` for tracking only — the system can't observe or guarantee the post actually happened, since it never touches the group.

`FacebookDestination` should carry a `DestinationType` (`Page` / `Group`) so the UI routes Pages through (a) and Groups through (b) automatically, without the admin needing to understand why they behave differently.

### 4.10 Responsive Design, Localization, Theming
- Mobile-first for both admin and public sides — admin should be fully usable from a phone (add product, update stock) without a computer.
- Arabic + English with proper RTL/LTR support, built into the layout from the start.
- Light and dark mode, remembered per user/session.

---

## 5. Suggested Roles (v1)

- **Admin:** full access — products, inventory, sales, finances, customer requests, images, Facebook publishing (Pages) and posting assistant (Groups).
- **Public customer:** no account — browse, search/filter, view details, check availability, contact seller.
- Additional staff/employee roles can be layered in later without restructuring, since access control sits at the Web layer and doesn't touch Domain/Application use cases.

---

## 6. UI Style Reference

The attached reference screenshots define the visual language to carry across the **public catalog**:

- **Base palette:** near-white background, black for primary text/CTAs, with a small set of vivid accent colors (used specifically as color-variant swatches/dots — pink, blue, green — rather than as general UI color).
- **Typography:** bold, condensed sans-serif for headings ("Discover"), lighter weight for secondary text ("Explore the new Shirts").
- **Product browsing:** horizontal "rail" of product cards (hanger-style visual treatment), each card showing image, name, price with an original price struck through when discounted, and small color-swatch dots.
- **Discount/offer badges:** a small ribbon-style "OFF" tag on discounted items; a dedicated banner card for site-wide offers ("Get 30% OFF").
- **Product detail:** full-bleed product photo with a bottom sheet overlaying it, containing: product name, color toggle/swatches, a size selector row (chips, active state filled black), price block (struck-through original + discounted price + percentage-off in red), and a full-width black "Add to Bag" button.
- **Navigation:** minimal icon-only bottom nav (search, home, wishlist) on the public side; a slide-out/hamburger menu and a cart icon with a notification dot in the top bar.

**Guidance for the build agent:** reuse this same design system (spacing, corner radii, button styles, badge treatment) for the public catalog pages, and apply a denser, more data-oriented variant of the same visual language (same typography/colors, more information per screen) for the admin dashboard, tables, and forms — so the two sides feel like one product rather than two different apps.

---

## 7. Suggested Pages

**Public:** Home (featured products, categories, offers) · Products (listing + filters) · Product Details (images incl. original vs. enhanced, description, price/offer, colors, stock status, contact buttons).

**Admin:** Dashboard (totals, in/out-of-stock counts, recent sales/expenses, financial summary) · Products (CRUD, search/filter) · Inventory (stock per product/color, low-stock view) · Sales (history, register sale) · Financial Transactions (income, expenses, history) · Customer Requests · Categories · Facebook Publishing (real publish flow for Pages, copy/open/track assistant for Groups) · Image Guide (photography tips, size recommendations, AI prompt templates).

---

## 8. Delivery Phases

**Phase 1 — Foundation**
Clean Architecture solution scaffold (Domain/Application/Infrastructure/Web), admin auth, product CRUD, categories/subcategories, multiple images, colors & quantities, stock tracking, sold/unsold filtering, public catalog with search/filter, WhatsApp/Messenger contact, responsive layout, Arabic/English, light/dark mode.

**Phase 2 — Operations**
Sales management with automatic stock reduction, income/expense tracking, dashboard & reports, customer contact-request form, original-vs-AI-enhanced image comparison on the public side.

**Phase 3 — Facebook Integration**
Real Graph API publishing for connected Pages (pending Meta App Review), plus a copy/open/track "posting assistant" for Groups — Meta no longer allows API-based posting to groups at all, so the Groups side is scoped as a manual-assist tool from the start, not a reduced version of automation (see §4.9).

---

## 9. Notes for the Build Agent (Antigravity)

- Scaffold the solution as a **Clean Architecture** solution in **.NET 10** per the project structure in §2 before writing any feature code.
- Treat §1's stack table as fixed choices, not suggestions to re-evaluate.
- Build every feature as an Application-layer use case (MediatR command/query) with a validator, regardless of how small it looks — this keeps Phase 2/3 additions from requiring rework.
- Apply the visual language in §6 to the public catalog first, then reuse its tokens (colors, spacing, radii, typography scale) for the admin side.
- Keep `IFacebookPagePublisher`, `IFacebookGroupAssistHelper`, and `IImageStorageService` behind interfaces from day one, even in Phase 1, so Phase 2/3 work is additive rather than a refactor.
- Do not build any Group auto-posting call — Meta's API does not support it as of 2026. The Groups feature is content-generation + deep links + manual tracking only; only the Pages path should call the Graph API directly.
