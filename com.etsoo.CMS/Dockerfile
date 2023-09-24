#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# FROM node:[tag_name] AS [alias_name]
FROM node:latest AS node_base

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# First dot means root directory of the source image layer
# Second dot means destination location within the current image
COPY --from=node_base . .

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["com.etsoo.CMS/com.etsoo.CMS.csproj", "com.etsoo.CMS/"]
RUN dotnet restore "com.etsoo.CMS/com.etsoo.CMS.csproj"
COPY . .
WORKDIR "/src/com.etsoo.CMS"
RUN dotnet build "com.etsoo.CMS.csproj" -c Release -o /app/build

COPY --from=node_base . .

FROM build AS publish
RUN dotnet publish "com.etsoo.CMS.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
#COPY --from=publish /app/publish .
COPY --from=build /app .
ENTRYPOINT ["dotnet", "com.etsoo.CMS.dll"]