FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/SchoolCRM.API/SchoolCRM.API.csproj", "src/SchoolCRM.API/"]
COPY ["src/SchoolCRM.Application/SchoolCRM.Application.csproj", "src/SchoolCRM.Application/"]
COPY ["src/SchoolCRM.Domain/SchoolCRM.Domain.csproj", "src/SchoolCRM.Domain/"]
COPY ["src/SchoolCRM.Infrastructure/SchoolCRM.Infrastructure.csproj", "src/SchoolCRM.Infrastructure/"]
COPY ["src/SchoolCRM.Shared/SchoolCRM.Shared.csproj", "src/SchoolCRM.Shared/"]
RUN dotnet restore "src/SchoolCRM.API/SchoolCRM.API.csproj"
COPY . .
WORKDIR "/src/src/SchoolCRM.API"
RUN dotnet build "SchoolCRM.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SchoolCRM.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SchoolCRM.API.dll"]
