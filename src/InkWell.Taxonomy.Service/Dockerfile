FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/InkWell.Taxonomy.Service/InkWell.Taxonomy.Service.csproj", "src/InkWell.Taxonomy.Service/"]
RUN dotnet restore "src/InkWell.Taxonomy.Service/InkWell.Taxonomy.Service.csproj"
COPY . .
WORKDIR "/src/src/InkWell.Taxonomy.Service"
RUN dotnet build "InkWell.Taxonomy.Service.csproj" -c Release -o /app/build
RUN dotnet publish "InkWell.Taxonomy.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InkWell.Taxonomy.Service.dll"]
