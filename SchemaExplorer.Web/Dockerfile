FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["SchemaExplorer.Web/SchemaExplorer.Web.csproj", "SchemaExplorer.Web/"]
RUN dotnet restore "SchemaExplorer.Web/SchemaExplorer.Web.csproj"
COPY . .
WORKDIR "/src/SchemaExplorer.Web"
RUN dotnet build "SchemaExplorer.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SchemaExplorer.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SchemaExplorer.Web.dll"]
