# Сборка 
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/EventTicketApi/EventTicketApi.csproj src/EventTicketApi/
RUN dotnet restore src/EventTicketApi/EventTicketApi.csproj

COPY src/EventTicketApi/ src/EventTicketApi/
WORKDIR /src/src/EventTicketApi
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EventTicketApi.dll"]