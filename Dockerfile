# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore
COPY ["SubscriptionManager.csproj", "./"]
RUN dotnet restore "SubscriptionManager.csproj"

# Copy everything and build
COPY . .
COPY .env /app/.env

WORKDIR "/src"
RUN dotnet build "SubscriptionManager.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "SubscriptionManager.csproj" -c Release -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SubscriptionManager.dll"]
