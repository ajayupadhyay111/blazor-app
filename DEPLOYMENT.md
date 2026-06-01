# Deployment Guide — UPSMFAC Affiliation Portal

App type: **.NET 10 Blazor WebAssembly (Standalone)** — poori app browser mein chalti hai,
**koi server/WebSocket nahi**. Bas static files serve karne hain. Deployment aapke hi
**Docker + Caddy** workflow se hota hai (image banao → container run karo → host Caddy mein add karo).

> ✅ Pehle Interactive Server tha (WebSocket disconnect aata tha). Ab WASM hai — woh issue khatam.
> Container ke andar ek chhota Caddy static files deta hai; aapka host Caddy us container ko reverse-proxy karta hai.

---

## Local run (development)
```powershell
dotnet run --project BlazorApp.csproj
# → http://localhost:5271/affiliation
```

## Docker build & run  ← aapka workflow
```bash
# 1) image banao (multi-stage: SDK publish → caddy serve)
docker build -t upsmfac-affiliation .

# 2) container run karo (host ke 8080 ko container ke 8080 se map)
docker run -d --restart unless-stopped -p 8080:8080 --name upsmfac upsmfac-affiliation

# test: http://SERVER-IP:8080/affiliation
```

## Host Caddy mein add karo
Apne host ke `/etc/caddy/Caddyfile` mein:
```
form.ajayupadhyay.online {
	reverse_proxy localhost:8080
}
```
Phir:
```bash
sudo systemctl reload caddy
```
✅ Live: **https://form.ajayupadhyay.online/affiliation** — host Caddy SSL khud handle karega.
SPA routing (/affiliation jaise deep links) container ke andar wale Caddyfile ke `try_files` se handle hota hai.

---

## Future updates (naya code deploy)
```bash
docker build -t upsmfac-affiliation .
docker stop upsmfac && docker rm upsmfac
docker run -d --restart unless-stopped -p 8080:8080 --name upsmfac upsmfac-affiliation
```
(Host Caddy ko chhune ki zaroorat nahi — same port pe naya container.)

---

## Files
- `Dockerfile` — 2-stage: .NET SDK se WASM publish → `caddy:2-alpine` se serve.
- `Caddyfile` — container ke andar ka config (port 8080, SPA fallback, gzip, asset caching).
- `.dockerignore` — bin/obj/.git ko image se bahar rakhta hai.
