FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ConsoleProducer/ConsoleConsumer.csproj ConsoleProducer/
COPY Shared.Messages/Shared.Messages.csproj Shared.Messages/
RUN dotnet restore ConsoleProducer/ConsoleProducer.csproj
COPY . .
WORKDIR /src/ConsoleProducer
RUN dotnet build ConsoleProducer.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ConsoleProducer.csproj -c Release -o /app
ENV m ${messageToProducer}
FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ConsoleProducer.dll","-f","50000"]
