# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the solution file
COPY SmartFintechFinancial.sln ./

# Copy project files
COPY src/API/API.csproj ./src/API/
COPY src/Modules/Identity/Identity.csproj ./src/Modules/Identity/
COPY src/Shared/Infrastructure/Infrastructure.csproj ./src/Shared/Infrastructure/
COPY SmartFintechFinancial.Tests/SmartFintechFinancial.Tests.csproj ./SmartFintechFinancial.Tests/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Build and publish the API project
WORKDIR /app/src/API
RUN dotnet publish API.csproj -c Debug -o /app/out

# Runtime stage with debugging tools
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install debugging tools
RUN apt-get update && apt-get install -y unzip curl procps && \
    curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l /vsdbg

COPY --from=build /app/out .
EXPOSE 8080
EXPOSE 8081

# Set environment for debugging
ENV ASPNETCORE_ENVIRONMENT=Development
ENV COMPlus_EnableDiagnostics=1

ENTRYPOINT ["dotnet", "API.dll"]