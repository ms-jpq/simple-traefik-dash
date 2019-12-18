FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

WORKDIR /build

RUN dotnet new tool-manifest && \
    dotnet tool install Paket


COPY paket.dependencies .
RUN dotnet paket install && \
    dotnet paket restore

COPY src/views out/views
COPY src src
RUN dotnet publish \
    -r linux-musl-x64 \
    -c Release \
    -o out \
    /p:PublishSingleFile=true \
    src/main.fsproj

#
#
#
FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1.0-alpine3.10

WORKDIR /std
EXPOSE 5050


COPY --from=build /build/out /std



ENTRYPOINT ["/std/main"]
