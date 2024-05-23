
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

COPY . /source

WORKDIR /source

RUN dotnet restore
  
ARG TARGETARCH

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish -a ${TARGETARCH/amd64/x64} --use-current-runtime --self-contained false -o /app
# RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
#     dotnet publish -a $(echo $TARGETARCH | sed 's/amd64/x64/') --use-current-runtime --self-contained false -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

WORKDIR /app

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl --fail http://localhost:80/health || exit 1

COPY --from=build /app .

USER $APP_UID

ENTRYPOINT ["dotnet", "KanbamApi.dll"]
