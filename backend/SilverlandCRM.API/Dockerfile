FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["SilverlandCRM.API/SilverlandCRM.API.csproj", "SilverlandCRM.API/"]
COPY ["SilverlandCRM.Application/SilverlandCRM.Application.csproj", "SilverlandCRM.Application/"]
COPY ["SilverlandCRM.Domain/SilverlandCRM.Domain.csproj", "SilverlandCRM.Domain/"]
COPY ["SilverlandCRM.Infrastructure/SilverlandCRM.Infrastructure.csproj", "SilverlandCRM.Infrastructure/"]

RUN dotnet restore "SilverlandCRM.API/SilverlandCRM.API.csproj"

COPY . .
WORKDIR "/src/SilverlandCRM.API"
RUN dotnet build "SilverlandCRM.API.csproj" -c Release -o /app/build
RUN dotnet publish "SilverlandCRM.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SilverlandCRM.API.dll"]