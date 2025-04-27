FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies (use .csproj for the project name)
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY . .
# Publish
RUN dotnet publish -c Release -o out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
COPY --from=build /app/out /app/Backend
WORKDIR /app/Backend


# Expose the port your application will run on
EXPOSE 5100
ENV ASPNETCORE_URLS http://0.0.0.0:5100

# Start the application
ENTRYPOINT ["dotnet", "API.dll"]

