FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
# Turn off Invariant mode and install icu packages for EntityFramework. 
# See: https://github.com/dotnet/SqlClient/issues/220
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS sdkbase
# Turn off Invariant mode and install icu packages for EntityFramework. 
# See: https://github.com/dotnet/SqlClient/issues/220
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN curl -L https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh  | sh
ARG FEED_ACCESSTOKEN
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://markpollard.pkgs.visualstudio.com/CodeBuildDeploy/_packaging/CodeBuildDeploy/nuget/v3/index.json\", \"username\":\"docker\", \"password\":\"${FEED_ACCESSTOKEN}\"}]}"
ARG BUILD_CONFIGURATION=Release
ARG VERSION=0.0.0-local
ARG ASSEMBLY_VERSION=0.0.0
ARG FILE_VERSION=0.0.0.0
ARG INFORMATIONAL_VERSION=0.0.0-local
ARG PACKAGE_VERSION=0.0.0-local
WORKDIR /work

FROM sdkbase AS build
WORKDIR /work
COPY ["src/CodeBuildDeploy.Identity.Web/CodeBuildDeploy.Identity.Web.csproj", "src/CodeBuildDeploy.Identity.Web/"]
COPY ["src/CodeBuildDeploy.Identity.DA.Abstractions/CodeBuildDeploy.Identity.DA.Abstractions.csproj", "src/CodeBuildDeploy.Identity.DA.Abstractions/"]
COPY ["src/CodeBuildDeploy.Identity.DA.EF/CodeBuildDeploy.Identity.DA.EF.csproj", "src/CodeBuildDeploy.Identity.DA.EF/"]
COPY ["./nuget.config", "./"]
RUN dotnet restore "./src/CodeBuildDeploy.Identity.Web/CodeBuildDeploy.Identity.Web.csproj"
COPY . .
WORKDIR "/work/src/CodeBuildDeploy.Identity.Web"
RUN dotnet build "./CodeBuildDeploy.Identity.Web.csproj" --no-restore -p:Version=${VERSION} -p:AssemblyVersion=${ASSEMBLY_VERSION} -p:FileVersion=${FILE_VERSION} -p:InformationalVersion=${INFORMATIONAL_VERSION} -p:PackageVersion=${PACKAGE_VERSION} -c $BUILD_CONFIGURATION -o /app/build

FROM build AS push
WORKDIR /work
RUN dotnet pack "./src/CodeBuildDeploy.Identity.DA.Abstractions/CodeBuildDeploy.Identity.DA.Abstractions.csproj" --no-build -p:OutDir=/app/build -p:Version=${VERSION} -p:AssemblyVersion=${ASSEMBLY_VERSION} -p:FileVersion=${FILE_VERSION} -p:InformationalVersion=${INFORMATIONAL_VERSION} -p:PackageVersion=${PACKAGE_VERSION} -c $BUILD_CONFIGURATION -o /app/packages
RUN dotnet pack "./src/CodeBuildDeploy.Identity.DA.EF/CodeBuildDeploy.Identity.DA.EF.csproj" --no-build -p:OutDir=/app/build -p:Version=${VERSION} -p:AssemblyVersion=${ASSEMBLY_VERSION} -p:FileVersion=${FILE_VERSION} -p:InformationalVersion=${INFORMATIONAL_VERSION} -p:PackageVersion=${PACKAGE_VERSION} -c $BUILD_CONFIGURATION -o /app/packages
RUN dotnet nuget push "/app/packages/*.nupkg" -s CodeBuildDeploy -k "${FEED_ACCESSTOKEN}"

FROM build AS publish
WORKDIR "/work/src/CodeBuildDeploy.Identity.Web"
RUN dotnet publish "./CodeBuildDeploy.Identity.Web.csproj" --no-build -p:OutDir=/app/build -p:Version=${VERSION} -p:AssemblyVersion=${ASSEMBLY_VERSION} -p:FileVersion=${FILE_VERSION} -p:InformationalVersion=${INFORMATIONAL_VERSION} -p:PackageVersion=${PACKAGE_VERSION} -c $BUILD_CONFIGURATION -o /app/publish

FROM build AS buildMigration
RUN dotnet tool install --version 8.0.3 --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
WORKDIR /work
COPY ["src/CodeBuildDeploy.Identity.DA.EF.Deploy/CodeBuildDeploy.Identity.DA.EF.Deploy.csproj", "src/CodeBuildDeploy.Identity.DA.EF.Deploy/"]
RUN dotnet restore "src/CodeBuildDeploy.Identity.DA.EF.Deploy/CodeBuildDeploy.Identity.DA.EF.Deploy.csproj"
WORKDIR "/work/src/CodeBuildDeploy.Identity.DA.EF.Deploy"
RUN dotnet build "CodeBuildDeploy.Identity.DA.EF.Deploy.csproj" --no-restore -p:Version=${VERSION} -p:AssemblyVersion=${ASSEMBLY_VERSION} -p:FileVersion=${FILE_VERSION} -p:InformationalVersion=${INFORMATIONAL_VERSION} -p:PackageVersion=${PACKAGE_VERSION} -c $BUILD_CONFIGURATION -o /app/build
WORKDIR /work
RUN dotnet ef migrations bundle --project src/CodeBuildDeploy.Identity.DA.EF.Deploy --verbose

FROM base AS finalMigration
WORKDIR /app
COPY --from=buildMigration /work/efbundle .
COPY --from=buildMigration /work/src/CodeBuildDeploy.Identity.DA.EF.Deploy/ShellScripts/migrateDatabase.sh .
RUN chmod +x migrateDatabase.sh
ENTRYPOINT ["./migrateDatabase.sh"]

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CodeBuildDeploy.Identity.Web.dll"]