FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["CategoriaA.Api.csproj", "./"]

RUN dotnet restore "CategoriaA.Api.csproj"

COPY . . 

RUN dotnet publish "CategoriaA.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "CategoriaA.Api.dll"]