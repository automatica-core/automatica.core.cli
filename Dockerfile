# Build runtime image
FROM {RUNTIME}
WORKDIR /plugin

dotnet tool install automatica-plugin-standalone --global
dotnet tool install automtica-cli --global

COPY --from=build-env /plugin ./
ENTRYPOINT ["dotnet", "DpmWebsite.dll"]
EXPOSE 5000