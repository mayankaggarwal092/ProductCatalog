# Use official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ProductCatalog/*.csproj ./ProductCatalog/
RUN dotnet restore ProductCatalog/ProductCatalog.csproj

# Copy everything and build
COPY . ./
RUN dotnet publish ProductCatalog/ProductCatalog.csproj -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Expose port 80 for HTTP traffic
EXPOSE 80

# Start the app
ENTRYPOINT ["dotnet", "ProductCatalog.dll"]
