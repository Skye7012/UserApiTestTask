#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/UserApiTestTask.Api/UserApiTestTask.Api.csproj", "src/UserApiTestTask.Api/"]
COPY ["src/UserApiTestTask.Contracts/UserApiTestTask.Contracts.csproj", "src/UserApiTestTask.Contracts/"]
COPY ["src/UserApiTestTask.Infrastructure/UserApiTestTask.Infrastructure.csproj", "src/UserApiTestTask.Infrastructure/"]
COPY ["src/UserApiTestTask.Domain/UserApiTestTask.Domain.csproj", "src/UserApiTestTask.Domain/"]
COPY ["src/UserApiTestTask.Application/UserApiTestTask.Application.csproj", "src/UserApiTestTask.Application/"]
RUN dotnet restore "src/UserApiTestTask.Api/UserApiTestTask.Api.csproj"
COPY . .
WORKDIR "/src/src/UserApiTestTask.Api"
RUN dotnet build "UserApiTestTask.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserApiTestTask.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserApiTestTask.Api.dll"]