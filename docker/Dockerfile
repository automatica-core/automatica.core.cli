# Build runtime image
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build


RUN dotnet tool install automatica-plugin-standalone --global
RUN dotnet tool install automatica-cli --global


ENV PATH="${PATH}:/root/.dotnet/tools"