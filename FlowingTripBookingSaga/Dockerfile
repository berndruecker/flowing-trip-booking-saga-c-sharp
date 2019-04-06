FROM microsoft/dotnet:2.2-runtime AS base

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY . .
WORKDIR /src/FlowingTripBookingSaga
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "FlowingTripBookingSaga.dll"]