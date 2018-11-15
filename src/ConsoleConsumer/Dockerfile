FROM microsoft/dotnet:2.1-runtime AS base

FROM microsoft/dotnet:2.1-sdk AS build


WORKDIR /src
COPY *.csproj ./
COPY ../KafkaAndAkka.sln ./
RUN dotnet restore
# Copy everything else and build
COPY ../ ./


FROM build AS publish
RUN dotnet publish -c Release -o out

FROM base AS final
WORKDIR /app
COPY --from=build /src/out .
COPY --from=publish /app .

ENTRYPOINT ["/bin/bash","get-dockerip.sh"]

CMD ["dotnet", "ConsoleConsumer.exe"]