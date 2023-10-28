#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["TripleTriad.Server/TripleTriad.Server.csproj", "TripleTriad.Server/"]
COPY ["TripleTriad.Infrastructure/TripleTriad.Infrastructure.csproj", "TripleTriad.Infrastructure/"]
COPY ["TripleTriad.Domain/TripleTriad.Domain.csproj", "TripleTriad.Domain/"]
RUN dotnet restore "TripleTriad.Server/TripleTriad.Server.csproj"
COPY . .
WORKDIR "/src/TripleTriad.Server"
RUN dotnet build "TripleTriad.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TripleTriad.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TripleTriad.Server.dll"]