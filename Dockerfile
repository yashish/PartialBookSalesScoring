FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PartialBookSalesScoring.csproj", "./"]
RUN dotnet restore "./PartialBookSalesScoring.csproj"
COPY . .

WORKDIR "/src/."
RUN dotnet publish "PartialBookSalesScoring.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PartialBookSalesScoring.dll"]
