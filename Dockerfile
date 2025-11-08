FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ./src/ ./

RUN dotnet restore Person.sln
RUN dotnet publish Person/Person.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/out .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Person.dll"]
