# Code Build Deploy Identity Service

The Code Build Deploy identity service (https://www.codebuilddeploy.com/identity / https://www.codebuilddeploy.co.uk/identity).

The site is deployed as a container, into [AKS](https://azure.microsoft.com/en-gb/products/kubernetes-service/).

[![Build Status](https://markpollard.visualstudio.com/CodeBuildDeploy/_apis/build/status%2FCodeBuildDeploy.Identity?branchName=main)](https://markpollard.visualstudio.com/CodeBuildDeploy/_build/latest?definitionId=19&branchName=main)

# Standard DotNet Build

## Building

```bash
dotnet build
```

## Publishing

```bash
dotnet publish ./CodeBuildDeploy.Web/CodeBuildDeploy.Identity.Web.csproj --framework net8.0 --self-contained:false --no-restore -o ./publish
```

## Running

```bash
 dotnet run ./CodeBuildDeploy.Web/CodeBuildDeploy.Identity.Web.csproj
```

# Docker Build

## Generate Devcert

```powershell
dotnet dev-certs https -ep "$env:USERPROFILE\.aspnet\https\code-build-deploy.pfx" -p SOME_PASSWORD
dotnet dev-certs https --trust
```

## Configure the environment variable
Create a .env file based on the .env.example
```bash
FEED_ACCESSTOKEN=Access_Token_To_AzureDevOps_Feeds
CERT_PASSWORD=SOME_PASSWORD
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__AccountsConnection=Connection_String_To_Accounts_Db
ConnectionStrings__AccountMigrationConnection=Admin_Connection_String_For_Running_Migrations
Authentication__Google__ClientId=Google_Client_Id
Authentication__Google__ClientSecret=Google_Client_Secret
Authentication__Microsoft__ClientId=Microsoft_Client_Id
Authentication__Microsoft__ClientSecret=Microsoft_Client_Secret
```

## Building and Running

```powershell
docker compose up -d --build
```

# Db Migration

The services use entity framework migrations code first to manage their associated schemas. 
The services have a dedicated project for the migrations code which builds a container that can be run against the appropriate database.

## Creating Migrations

The following commands have been used to create the ef migrations and scripts have also been generated for manual running and inspection. 

From the root directory of the project:

### Add Inital Db Migration

```bash
dotnet ef migrations add CreateIdentitySchema --project .\src\CodeBuildDeploy.Identity.DA.EF.Deploy
```

### Script the database

```bash
dotnet ef migrations script 0 CreateIdentitySchema --project .\src\CodeBuildDeploy.Identity.DA.EF.Deploy -o .\src\CodeBuildDeploy.Identity.DA.EF.Deploy\DbScripts\10-CreateIdentitySchema.sql
```

## Running the migrations container

Running the 'finalMigration' target of the docker container will execute the migrations aginst the database connection specified in the environment variable:

```bash
FEED_ACCESSTOKEN=Access_Token_To_AzureDevOps_Feeds
ConnectionStrings__AccountMigrationConnection=Admin_Connection_String_For_Running_Migrations
``` 