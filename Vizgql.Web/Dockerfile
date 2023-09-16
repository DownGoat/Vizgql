FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Vizgql.Web/Vizgql.Web.csproj", "Vizgql.Web/"]
RUN dotnet restore "Vizgql.Web/Vizgql.Web.csproj"
COPY . .
WORKDIR "/src/Vizgql.Web"
RUN dotnet build "Vizgql.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Vizgql.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Vizgql.Web.dll"]
