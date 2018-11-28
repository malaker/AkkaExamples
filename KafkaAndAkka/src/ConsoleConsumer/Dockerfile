FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ConsoleConsumer/ConsoleConsumer.csproj ConsoleConsumer/
COPY Shared.IoC/Shared.IoC.csproj Shared.IoC/
COPY Shared.Interfaces/Shared.Interfaces.csproj Shared.Interfaces/
COPY Shared/Shared.csproj Shared/
COPY Shared.Common/Shared.Common.csproj Shared.Common/
COPY Shared.Messages/Shared.Messages.csproj Shared.Messages/
RUN dotnet restore ConsoleConsumer/ConsoleConsumer.csproj
COPY . .
WORKDIR /src/ConsoleConsumer
RUN dotnet build ConsoleConsumer.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ConsoleConsumer.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ConsoleConsumer.dll"]
