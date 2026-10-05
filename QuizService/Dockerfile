# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY QuizService/QuizService.csproj QuizService/
RUN dotnet restore QuizService/QuizService.csproj

# Copy source and build
COPY QuizService/. QuizService/
WORKDIR /src/QuizService
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "QuizService.dll"]
