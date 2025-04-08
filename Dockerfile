# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore
COPY API/SubscriptionManager.csproj ./API/
RUN dotnet restore "API/SubscriptionManager.csproj"

# Copy everything and build
COPY API/. ./API/

WORKDIR /src/API
RUN dotnet build "SubscriptionManager.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "SubscriptionManager.csproj" -c Release -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SubscriptionManager.dll"]