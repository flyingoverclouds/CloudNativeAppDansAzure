FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 32080
ENV ASPNETCORE_URLS=http://*:32080

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["CnaCatalogService.csproj", ""]
RUN dotnet restore "./CnaCatalogService.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "CnaCatalogService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CnaCatalogService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CnaCatalogService.dll"]