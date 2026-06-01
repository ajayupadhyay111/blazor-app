# Deployment Guide — UPSMFAC Affiliation Portal

App type: **.NET 10 Blazor Web App (Interactive Server)**. Yeh ek server-rendered interactive
app hai, isliye ek **server host** chahiye (Docker-based). localStorage client pe chalta hai;
form data browser mein save hota hai.

> ⚠️ **Note:** Pehle "WASM + Netlify (pure static)" plan tha. Lekin client ne ab
> **session tracking / concurrent users / secure retention** maanga — yeh server-side concepts
> hain. Isliye app abhi **Interactive Server** par hai (Docker se subdomain pe deploy hota hai,
> aur Phase-2 backend/DB plug karne ke liye ready hai). Detail: see IMPLEMENTATION_PLAN.md §architecture.

---

## Local run
```powershell
dotnet run --project BlazorApp.csproj
# → http://localhost:5271/affiliation
```

## Option A — Render.com (free tier, easiest subdomain)
1. Code ko GitHub repo mein push karein.
2. https://render.com → **New → Web Service** → repo connect.
3. Render `Dockerfile` auto-detect karega → **Create Web Service**.
4. Deploy hone par free URL milega: `https://<name>.onrender.com/affiliation` — yahi externally verifiable subdomain hai.

## Option B — Railway.app
1. `railway init` → `railway up` (Dockerfile auto-detected), ya GitHub repo connect.
2. **Settings → Networking → Generate Domain** → `https://<name>.up.railway.app`.

## Option C — Azure App Service (Linux, container)
```bash
az group create -n upsmfac-rg -l centralindia
az appservice plan create -g upsmfac-rg -n upsmfac-plan --is-linux --sku B1
az webapp create -g upsmfac-rg -p upsmfac-plan -n upsmfac-affiliation --deployment-container-image-name <your-registry>/blazorapp:latest
# → https://upsmfac-affiliation.azurewebsites.net/affiliation
```

## Build & test the container locally
```powershell
docker build -t upsmfac-affiliation .
docker run -p 8080:8080 upsmfac-affiliation
# → http://localhost:8080/affiliation
```

---

### Custom sub-domain (e.g. affiliation.yourdomain.gov.in)
Render/Railway/Azure sab custom domain support karte hain:
1. Host ke dashboard mein **Custom Domain** add karein.
2. Apne DNS provider pe ek **CNAME** record banayein jo host ke diye target ko point kare.
3. SSL automatically provision ho jaata hai.

> ❗ Main aapke account/DNS pe live deploy **khud nahi** kar sakta (credentials chahiye).
> Upar diye steps follow karke 5–10 min mein live subdomain mil jayega; ya credentials
> dene par main guide kar dunga.
