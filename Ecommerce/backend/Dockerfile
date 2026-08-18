FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Ecommerce.API/Ecommerce.API.csproj", "Ecommerce.API/"]
COPY ["Ecommerce.BLL/Ecommerce.BLL.csproj", "Ecommerce.BLL/"]
COPY ["Ecommerce.DAL/Ecommerce.DAL.csproj", "Ecommerce.DAL/"]
COPY ["Ecommerce.Models/Ecommerce.Models.csproj", "Ecommerce.Models/"]
COPY ["Ecommerce.Contracts/Ecommerce.Contracts.csproj", "Ecommerce.Contracts/"]
COPY ["Ecommerce.Shared/Ecommerce.Shared.csproj", "Ecommerce.Shared/"]
RUN dotnet restore "Ecommerce.API/Ecommerce.API.csproj"
COPY . .
WORKDIR "/src/Ecommerce.API"
RUN dotnet build "Ecommerce.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ecommerce.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.API.dll"]
