FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/InkWell.Comment.Service/InkWell.Comment.Service.csproj", "src/InkWell.Comment.Service/"]
RUN dotnet restore "src/InkWell.Comment.Service/InkWell.Comment.Service.csproj"
COPY . .
WORKDIR "/src/src/InkWell.Comment.Service"
RUN dotnet build "InkWell.Comment.Service.csproj" -c Release -o /app/build
RUN dotnet publish "InkWell.Comment.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InkWell.Comment.Service.dll"]
