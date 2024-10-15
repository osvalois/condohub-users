FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/${SERVICE_NAME}.Api/${SERVICE_NAME}.Api.csproj", "${SERVICE_NAME}.Api/"]
COPY ["src/${SERVICE_NAME}.Application/${SERVICE_NAME}.Application.csproj", "${SERVICE_NAME}.Application/"]
COPY ["src/${SERVICE_NAME}.Domain/${SERVICE_NAME}.Domain.csproj", "${SERVICE_NAME}.Domain/"]
COPY ["src/${SERVICE_NAME}.Infrastructure/${SERVICE_NAME}.Infrastructure.csproj", "${SERVICE_NAME}.Infrastructure/"]
RUN dotnet restore "${SERVICE_NAME}.Api/${SERVICE_NAME}.Api.csproj"
COPY . .
WORKDIR "/src/${SERVICE_NAME}.Api"
RUN dotnet build "${SERVICE_NAME}.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "${SERVICE_NAME}.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "${SERVICE_NAME}.Api.dll"]
