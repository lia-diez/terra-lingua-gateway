
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY MapGenGateway.sln ./
COPY Api/Api.csproj ./Api/


RUN dotnet restore

COPY . ./
WORKDIR /src/Api
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

COPY ./Api/start.sh /app/

ENTRYPOINT ["bash", "/app/start.sh"]
