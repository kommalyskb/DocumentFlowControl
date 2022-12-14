#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DFM.Frontend/DFM.Frontend.csproj", "DFM.Frontend/"]
COPY ["DFM.Shared/DFM.Shared.csproj", "DFM.Shared/"]
RUN dotnet restore "DFM.Frontend/DFM.Frontend.csproj"
COPY . .
WORKDIR "/src/DFM.Frontend"
RUN dotnet build "DFM.Frontend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DFM.Frontend.csproj" -c Release -o /app/publish /p:UseAppHost=false

ENV ASPNETCORE_ENVIRONMENT Production

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DFM.Frontend.dll"]