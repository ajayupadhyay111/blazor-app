# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY BlazorApp.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish BlazorApp.csproj -c Release -o /app

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# Most PaaS hosts (Render/Railway/Azure) inject $PORT; default to 8080 locally.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "BlazorApp.dll"]
