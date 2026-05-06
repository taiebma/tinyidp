FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /source

ARG TARGETPLATFORM
ARG TARGETARCH
ARG BUILDPLATFORM

# copy csproj and restore as distinct layers
COPY *.csproj .
RUN if [ "$TARGETPLATFORM" = "linux/amd64" ]; then \
    RID=linux-musl-x64 ; \
    elif [ "$TARGETPLATFORM" = "linux/arm64" ]; then \
    RID=linux-musl-arm64 ; \
    elif [ "$TARGETPLATFORM" = "linux/arm/v7" ]; then \
    RID=linux-musl-arm ; \
    fi \
    && dotnet restore -r $RID

# copy and publish app and libraries
COPY . .
RUN if [ "$TARGETPLATFORM" = "linux/amd64" ]; then \
    RID=linux-musl-x64 ; \
    elif [ "$TARGETPLATFORM" = "linux/arm64" ]; then \
    RID=linux-musl-arm64 ; \
    elif [ "$TARGETPLATFORM" = "linux/arm/v7" ]; then \
    RID=linux-musl-arm ; \
    fi \
    && dotnet publish tinyidp.csproj -f net10.0 -c Release -o /app -r $RID --self-contained false

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app .
RUN mkdir certs param
COPY certs/nas2.pfx certs

RUN apk update && apk add ca-certificates openssl && rm -rf /var/cache/apk/*
COPY certs/taiebma_int.crt /usr/local/share/ca-certificates/taiebma_int.crt
COPY certs/taiebma_ca.crt /usr/local/share/ca-certificates/taiebma_ca.crt
RUN update-ca-certificates

EXPOSE 8080/tcp
EXPOSE 8081/tcp
ENTRYPOINT ["dotnet", "/app/tinyidp.dll"]
