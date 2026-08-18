# Runbook: Deploying Recent Changes to the Azure Cloud

This document outlines the step-by-step instructions to build, package, and deploy all recent frontend and backend modifications to Azure.

---

## 1. Backend Changes (Product Activation & Model Fallback Fixes)

Because compiling `linux/amd64` images inside Docker Desktop on Apple Silicon Macs is prone to QEMU virtualization lockups, use the **native macOS publish workaround**. This compiles the binaries natively in 5 seconds and packages them instantly.

### Deployment Steps:
Run these commands from the `backend` directory in your terminal:

```bash
# 1. Navigate to the backend directory
cd /Users/dharshinik/Desktop/Presidio/Genspark/Ecommerce/backend

# 2. Compile natively on macOS (takes ~5-10 seconds)
dotnet publish Ecommerce.API/Ecommerce.API.csproj -c Release -o ./publish /p:UseAppHost=false

# 3. Package the precompiled binaries into the Docker image
docker build --platform linux/amd64 -f Dockerfile.local -t acrshophive.azurecr.io/shophive-api:v1 .

# 4. Push the updated image to Azure Container Registry (ACR)
docker push acrshophive.azurecr.io/shophive-api:v1

# 5. Force Container App to pull the new image and deploy (Revision: rev6)
az containerapp update \
  --name shophive-api \
  --resource-group shophive-rg \
  --image acrshophive.azurecr.io/shophive-api:v1 \
  --revision-suffix rev6
```

---

## 2. Frontend Changes (UI Enhancements & API URL Update)

Frontend deployments are fully automated using GitHub Actions connected to your Azure Static Web Apps (SWA). Any changes pushed to the `main` branch will automatically build and publish to the web.

### Deployment Steps:
Run these commands from the project root directory:

```bash
# 1. Navigate to the project root directory
cd /Users/dharshinik/Desktop/Presidio/Genspark/Ecommerce

# 2. Add all modified files (environment configs, category styles, carousel markup, and promo css)
git add frontend/src/app/components/customer/customer-dashboard/
git add frontend/src/environments/environment.prod.ts

# 3. Commit the changes
git commit -m "feat: implement categories slider, auto hero slider, and deploy product activation bugfix"

# 4. Push to main to trigger GitHub Actions CI/CD pipeline
git push origin main
```

### How to Monitor:
1. Go to your GitHub repository: `https://github.com/dharshini2022/E-Commerce`
2. Click the **Actions** tab.
3. Select the running workflow run (usually named after your commit message) to watch the deployment build progress. 
4. Once completed, refresh the browser, log out, log in, and verify the checkout and chatbot features.
