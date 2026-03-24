FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["AdvertisingAgencyProject/AdvertisingAgencyProject.csproj", "AdvertisingAgencyProject/"]
RUN dotnet restore "AdvertisingAgencyProject/AdvertisingAgencyProject.csproj"

COPY . .
WORKDIR "/src/AdvertisingAgencyProject"
RUN dotnet publish "AdvertisingAgencyProject.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

RUN mkdir -p /app/data

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AdvertisingAgencyProject.dll"]