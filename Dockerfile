# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. Copy solution file
COPY *.sln ./

# 2. Copy all project csproj to restore cache
COPY SWP391Web.API/*.csproj ./SWP391Web.API/
COPY SWP391Web.Application/*.csproj ./SWP391Web.Application/
COPY SWP391Web.Domain/*.csproj ./SWP391Web.Domain/
COPY SWP391Web.Infrastructure/*.csproj ./SWP391Web.Infrastructure/

# 3. Restore packages
RUN dotnet restore

# 4. Copy all source code
COPY . .

# 5. Publish project API
WORKDIR /src/SWP391Web.API
RUN dotnet publish -c Release -o /app/publish


# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy publish from stage build
COPY --from=build /app/publish .

# Open port 8080 for container
EXPOSE 8080

# Run app
ENTRYPOINT ["dotnet", "SWP391Web.API.dll"]

