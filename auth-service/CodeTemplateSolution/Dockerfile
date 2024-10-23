#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM node:18-alpine AS build-frontend
WORKDIR /src
COPY ./src/Portal ./Portal
WORKDIR /src/Portal
RUN yarn install
RUN yarn build

FROM node:18-alpine AS copy-ui-bundles
WORKDIR /src
COPY --from=build-frontend /src/API/wwwroot/ /src/wwwroot

# ENTRYPOINT [ "sleep", "300" ]

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /src
EXPOSE 49672
EXPOSE 49671

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-backend
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["./src/Common/Common.Application/Common.Application.csproj", "./Common/Common.Application/"]
COPY ["./src/Common/Common.Authorization/Common.Authorization.csproj", "./Common/Common.Authorization/"]
COPY ["./src/Common/Common.Domain/Common.Domain.csproj", "./Common/Common.Domain/"]
COPY ["./src/Common/Common.Persistence/Common.Persistence.csproj", "./Common/Common.Persistence/"]
COPY ["./src/Common/Common.SharedKernel/Common.SharedKernel.csproj", "./Common/Common.SharedKernel/"]

COPY ["./src/Modules/Web/Modules.Web.Application/Modules.Web.Application.csproj", "./Modules/Web/Modules.Web.Application/"]
COPY ["./src/Modules/Web/Modules.Web.Endpoints/Modules.Web.Endpoints.csproj", "./Modules/Web/Modules.Web.Endpoints/"]

COPY ["./src/Modules/App/Modules.App.Application/Modules.App.Application.csproj", "./Modules/App/Modules.App.Application/"]
COPY ["./src/Modules/App/Modules.App.Endpoints/Modules.App.Endpoints.csproj", "./Modules/App/Modules.App.Endpoints/"]

COPY ["./src/API/API.csproj", "./API/"]
RUN dotnet restore "./API/API.csproj"
COPY ./src .

WORKDIR /src/API
RUN dotnet build "./API.csproj" -c $BUILD_CONFIGURATION -o /src/build

# # Continue from 'build' stage. Use the image created at the end of 'build' stage as 'base' for next stage
FROM build-backend AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./API.csproj" -c $BUILD_CONFIGURATION -o /src/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /src
COPY --from=build-backend /src/API/bkse_online_certificate.pfx .
COPY --from=publish /src/publish .
COPY --from=copy-ui-bundles /src .
# RUN update-ca-certificates
ENTRYPOINT [ "dotnet", "API.dll" ]
# ENTRYPOINT [ "sleep", "infinity" ]