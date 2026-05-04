FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the specific project file
COPY ["src/InkWell.Auth.Service/InkWell.Auth.Service.csproj", "src/InkWell.Auth.Service/"]

# Restore dependencies
RUN dotnet restore "src/InkWell.Auth.Service/InkWell.Auth.Service.csproj"

# Copy everything else
COPY . .
WORKDIR "/src/src/InkWell.Auth.Service"

# Build and publish
RUN dotnet build "InkWell.Auth.Service.csproj" -c Release -o /app/build
RUN dotnet publish "InkWell.Auth.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InkWell.Auth.Service.dll"]
