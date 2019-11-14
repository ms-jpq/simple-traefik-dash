FROM mcr.microsoft.com/dotnet/core/sdk:3.0

WORKDIR /app

RUN apt update && \
    apt install -y tree

RUN dotnet new tool-manifest && dotnet tool install Paket

COPY paket.dependencies .
RUN dotnet tool run paket install

COPY src /app/src

RUN tree
RUN dotnet publish \
    -r linux-musl-x64 \
    -c Release \
    -o /app/out \
    /p:PublishSingleFile=true \
    src/main.fsproj


FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.0

EXPOSE 5050

COPY --from=build /app/out /app
COPY views /app

ENTRYPOINT ["./app/main"]
