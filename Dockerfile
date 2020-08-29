#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Template.BackgroundWorker.HealthApi/Template.BackgroundWorker.HealthApi.csproj", "Template.BackgroundWorker.HealthApi/"]
RUN dotnet restore "Template.BackgroundWorker.HealthApi/Template.BackgroundWorker.HealthApi.csproj"
COPY . .
WORKDIR "/src/Template.BackgroundWorker.HealthApi"
RUN dotnet build "Template.BackgroundWorker.HealthApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Template.BackgroundWorker.HealthApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Template.BackgroundWorker.HealthApi.dll"]