# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN sed -i 's/CipherString = DEFAULT@SECLEVEL=2/CipherString = DEFAULT@SECLEVEL=1/' /etc/ssl/openssl.cnf
WORKDIR /app
RUN mkdir -p /app/Uploads /app/wwwroot /app/Logs && chown -R $APP_UID:$APP_UID /app
USER $APP_UID
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HelpCenter.WebApi/HelpCenter.WebApi.csproj", "HelpCenter.WebApi/"]
COPY ["HelpCenter.Application/HelpCenter.Application.csproj", "HelpCenter.Application/"]
COPY ["HelpCenter.Domain/HelpCenter.Domain.csproj", "HelpCenter.Domain/"]
COPY ["HelpCenter.Infrastructure/HelpCenter.Infrastructure.csproj", "HelpCenter.Infrastructure/"]
COPY ["HelpCenter.Persistence/HelpCenter.Persistence.csproj", "HelpCenter.Persistence/"]
RUN dotnet restore "./HelpCenter.WebApi/HelpCenter.WebApi.csproj"
COPY . .
WORKDIR "/src/HelpCenter.WebApi"
RUN dotnet build "./HelpCenter.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HelpCenter.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Guarantee folder permissions (so that things don't crash, especially with volume mounts)
USER root
RUN mkdir -p /app/Uploads /app/wwwroot /app/Logs && chown -R $APP_UID:$APP_UID /app/Uploads /app/wwwroot /app/Logs
USER $APP_UID

ENTRYPOINT ["dotnet", "HelpCenter.WebApi.dll"]