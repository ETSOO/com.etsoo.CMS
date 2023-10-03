#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# FROM node:[tag_name] AS [alias_name]
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
RUN apt-get update
RUN apt-get install -y curl
RUN apt-get install -y libpng-dev libjpeg-dev curl libxi6 build-essential libgl1-mesa-glx
RUN curl -sL https://deb.nodesource.com/setup_lts.x | bash -
RUN apt-get install -y nodejs

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
RUN apt-get update
RUN apt-get install -y curl
RUN apt-get install -y libpng-dev libjpeg-dev curl libxi6 build-essential libgl1-mesa-glx
RUN curl -sL https://deb.nodesource.com/setup_lts.x | bash -
RUN apt-get install -y nodejs
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