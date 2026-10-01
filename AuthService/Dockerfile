# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY AuthService/AuthService.csproj AuthService/
COPY SharedLibrary/. SharedLibrary/
RUN dotnet restore AuthService/AuthService.csproj

# Copy source and build
COPY AuthService/. AuthService/
WORKDIR /src/AuthService
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "AuthService.dll"]
