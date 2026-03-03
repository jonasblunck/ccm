FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . ./
RUN dotnet restore
RUN dotnet build -c Release

ENTRYPOINT ["bin/Release/net8.0/CCM"]
