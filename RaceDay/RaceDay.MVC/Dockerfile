# Stage 1: Runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080


# Stage 2: Build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy MVC project file
COPY ["RaceDay.MVC/RaceDay.MVC.csproj", "RaceDay.MVC/"]

# Copy Contracts project file
COPY ["RaceDay.contracts/RaceDay.contracts.csproj", "RaceDay.contracts/"]

# Restore dependencies
RUN dotnet restore "RaceDay.MVC/RaceDay.MVC.csproj"

# Copy remaining source code
COPY . .

# Build and publish MVC
WORKDIR "/src/RaceDay.MVC"

RUN dotnet publish "RaceDay.MVC.csproj" -c Release -o /app/publish /p:UseAppHost=false


# Stage 3: Final container
FROM base AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "RaceDay.MVC.dll"]