FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY ./release .
ENTRYPOINT ["dotnet", "MRogalski.SplitLoot.dll"]