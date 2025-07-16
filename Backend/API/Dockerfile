FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies (use .csproj for the project name)
COPY API.sln ./
COPY API/API.csproj API/
COPY MeetApp.DataEntities/MeetApp.DataEntities.csproj MeetApp.DataEntities/
COPY NotificationQueue.Services/NotificationQueue.Services.csproj NotificationQueue.Services/

# Copy the rest of the application code
COPY . .

# Restore dependencies
RUN dotnet restore API.sln

# Publish
WORKDIR /app/API
RUN dotnet publish -c Release -o out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
COPY --from=build /app/API/out /app/API
WORKDIR /app/API
COPY API/.env /app/API/.env

# Expose the port your application will run on
EXPOSE 5100
ENV ASPNETCORE_URLS http://0.0.0.0:5100

# Start the application
ENTRYPOINT ["dotnet", "API.dll"]

