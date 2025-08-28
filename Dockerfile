# =========================
# Stage 1: Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file và project csproj
COPY *.sln .
COPY SWP391Web.API/*.csproj ./SWP391Web.API/

# Restore packages
RUN dotnet restore

# Copy toàn bộ source code
COPY . .

# Build và publish
WORKDIR /src/SWP391Web.API
RUN dotnet publish -c Release -o /app/publish

# =========================
# Stage 2: Runtime
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy file publish từ stage build
COPY --from=build /app/publish .

# Mở port 8080 cho container
EXPOSE 8080

# Chạy app
ENTRYPOINT ["dotnet", "SWP391Web.API.dll"]
