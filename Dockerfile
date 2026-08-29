# ==========================================
# 1. Base Runtime Image (.NET 10 ASP.NET)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
ENV ASPNETCORE_URLS=http://+:8080

# ==========================================
# 2. Build Stage (.NET 10 SDK)
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for caching layer restore
COPY ["src/AsserGallery.Domain/AsserGallery.Domain.csproj", "AsserGallery.Domain/"]
COPY ["src/AsserGallery.Application/AsserGallery.Application.csproj", "AsserGallery.Application/"]
COPY ["src/AsserGallery.Infrastructure/AsserGallery.Infrastructure.csproj", "AsserGallery.Infrastructure/"]
COPY ["src/AsserGallery.Web/AsserGallery.Web.csproj", "AsserGallery.Web/"]

RUN dotnet restore "AsserGallery.Web/AsserGallery.Web.csproj"

# Copy source code and build
COPY src/AsserGallery.Domain/ AsserGallery.Domain/
COPY src/AsserGallery.Application/ AsserGallery.Application/
COPY src/AsserGallery.Infrastructure/ AsserGallery.Infrastructure/
COPY src/AsserGallery.Web/ AsserGallery.Web/

WORKDIR "/src/AsserGallery.Web"
RUN dotnet build "AsserGallery.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ==========================================
# 3. Publish Stage
# ==========================================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AsserGallery.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ==========================================
# 4. Final Runtime Container
# ==========================================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AsserGallery.Web.dll"]
