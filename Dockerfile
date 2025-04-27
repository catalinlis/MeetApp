
# Stage 1 ------- Build C# Dotnet Backend ---------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /app

# Copy the project file and restore dependencies (use .csproj for the project name)
COPY Backend/API/*.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY Backend/API/ .
# Publish 
RUN dotnet publish -c Release -o out

# Stage 2 ------- Build the runtime image -------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS backend-runtime
COPY --from=backend-build /app/out /app/Backend
WORKDIR /app/Backend

# Expose the port your application will run on
EXPOSE 5100
ENV ASPNETCORE_URLS http://0.0.0.0:5100

# Start the application
ENTRYPOINT ["dotnet", "API.dll"]

# Stage 3 ------- Build Angular Frontend --------
FROM node:22-alpine AS frontend-build
WORKDIR /usr/local/app

COPY Frontend/client/ /usr/local/app/

RUN npm install

EXPOSE 4200

ENTRYPOINT ["npm", "run", "start"]
