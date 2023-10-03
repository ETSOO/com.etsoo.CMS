#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# FROM node:[tag_name] AS [alias_name]
FROM node:20-alpine AS nodebase

FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
COPY --from=nodebase . .
WORKDIR /src
COPY ["com.etsoo.CMS/com.etsoo.CMS.csproj", "com.etsoo.CMS/"]
RUN dotnet restore "com.etsoo.CMS/com.etsoo.CMS.csproj"

# First dot means root directory of the source image layer
# Second dot means destination location within the current image
COPY . .
WORKDIR "/src/com.etsoo.CMS"
RUN dotnet build "com.etsoo.CMS.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "com.etsoo.CMS.csproj" -c Release -o /app/publish \
  --no-restore \  
  --runtime alpine-x64 \
  /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "com.etsoo.CMS.dll"]