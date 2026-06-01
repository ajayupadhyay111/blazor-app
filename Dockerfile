# ---- Stage 1: build the Blazor WebAssembly static site ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY BlazorApp.csproj ./
RUN dotnet restore
COPY . .
RUN dotnet publish BlazorApp.csproj -c Release -o /app

# ---- Stage 2: serve the static files with a tiny Caddy ----
FROM caddy:2-alpine
COPY Caddyfile /etc/caddy/Caddyfile
COPY --from=build /app/wwwroot /srv
EXPOSE 8080
# (caddy base image auto-runs: caddy run --config /etc/caddy/Caddyfile)
