# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SchoolApp/SchoolApp.csproj", "SchoolApp/"]
RUN dotnet restore "SchoolApp/SchoolApp.csproj"

COPY . .
WORKDIR "/src/SchoolApp"
RUN dotnet build "SchoolApp.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "SchoolApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 80 443
ENTRYPOINT ["dotnet", "SchoolApp.dll"]
