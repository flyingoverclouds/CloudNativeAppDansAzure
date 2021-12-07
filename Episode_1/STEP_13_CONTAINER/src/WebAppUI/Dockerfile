FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 31080
ENV ASPNETCORE_URLS=http://+:31080

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["CnAppForAzureDev.csproj", ""]
RUN dotnet restore "CnAppForAzureDev.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "CnAppForAzureDev.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CnAppForAzureDev.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CnAppForAzureDev.dll"]
