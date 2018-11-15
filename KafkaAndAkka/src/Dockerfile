FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build

WORKDIR /src

COPY ConsoleConsumer/*.csproj ./ConsoleConsumer/
COPY Shared/*.csproj ./Shared/
COPY Shared.Common/*.csproj ./Shared.Common/
COPY Shared.Interfaces/*.csproj ./Shared.Interfaces/
COPY Shared.IoC/*.csproj ./Shared.IoC/
COPY Shared.Messages/*.csproj ./Shared.Messages/
COPY *.sln ./
RUN dotnet restore
# Copy everything else and build
COPY . .


FROM build AS publish
RUN dotnet publish -c Release -o out

FROM base AS final
WORKDIR /app

COPY --from=publish /src/ConsoleConsumer/out .

ENTRYPOINT  ["dotnet", "ConsoleConsumer.dll"]