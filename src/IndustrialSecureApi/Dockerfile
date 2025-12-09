# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["src/IndustrialSecureApi/IndustrialSecureApi.csproj", "src/IndustrialSecureApi/"]
RUN dotnet restore "src/IndustrialSecureApi/IndustrialSecureApi.csproj"

# Copy everything else and build
COPY src/IndustrialSecureApi/ src/IndustrialSecureApi/
WORKDIR /src/src/IndustrialSecureApi
RUN dotnet build "IndustrialSecureApi.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
WORKDIR /src/src/IndustrialSecureApi
RUN dotnet publish "IndustrialSecureApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Create non-root user with UID 1000
RUN addgroup -g 1000 appgroup && \
    adduser -D -u 1000 -G appgroup appuser

# Copy published app
COPY --from=publish /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Entry point
ENTRYPOINT ["dotnet", "IndustrialSecureApi.dll"]

