# ===== Build stage =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files (only csproj first for caching)
COPY NotificationService.sln ./
COPY NotificationService/NotificationService.csproj NotificationService/
COPY MeetApp.DataEntities/MeetApp.DataEntities.csproj MeetApp.DataEntities/
COPY NotificationQueue.Services/NotificationQueue.Services.csproj NotificationQueue.Services/

COPY . .

# Restore dependencies
RUN dotnet restore NotificationService.sln

# Build and publish NotificationService
WORKDIR /app/NotificationService
RUN dotnet publish -c Release -o out

# ===== Runtime stage =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# Copy published output
COPY --from=build /app/NotificationService/out /app/NotificationService
WORKDIR /app/NotificationService

COPY NotificationService/.env /app/NotificationService/.env

# Expose port
EXPOSE 5104
ENV ASPNETCORE_URLS=http://0.0.0.0:5104

# Start the service
ENTRYPOINT ["dotnet", "NotificationService.dll"]
