# Azure Resource Location Shift: Migrating to South India

This guide outlines how to migrate your stateful and configuration resources (Azure PostgreSQL Database, Key Vault, and Blob Storage) from the **East Asia** region to **South India**.

By moving these resources to the same region as your **Container App** and **Container Environment** (South India), you will eliminate cross-region latency, reducing application response times from **~10 seconds** down to **sub-seconds**.

---

## 🗺️ Migration Overview
Since Azure does not allow changing the region of existing resources directly, you must provision new instances in the target region and migrate data.

We provide two approaches:
* **Option A: Clean Re-deployment (Recommended for Dev/Test)**: Delete the old resources first to keep the exact same resource names, then deploy via Bicep.
* **Option B: Parallel Deployment (Zero Downtime)**: Deploy resources to South India with a name suffix (e.g., `-ind`), migrate the data, switch the container app configuration, and then delete the old resources.

---

> [!IMPORTANT]
> **Troubleshooting: InvalidResourceLocation Error**
> If you run a regional shift deployment under the same resource group and encounter the following error:
> `{"code": "InvalidResourceLocation", "message": "The resource 'shophive-log' already exists in location 'eastasia'..."}`
> This is because Azure does not allow a resource (like a Log Analytics workspace or a Container Environment) to be relocated or recreated with the same name in a different region within the same resource group.
> 
> To resolve this:
> * **If using Option A (Clean Re-Deployment)**: Ensure you delete the East Asia resources (including the log analytics workspace `shophive-log`) first, as described in Step 3 of Option A.
> * **If using Option B (Parallel Deployment)**: You must override the Log Analytics Workspace and Container App Environment names in the deployment parameters so new ones are created in South India, as described in Option B below.

---

## 🛠️ Option A: Clean Re-Deployment (Recommended)
This method is the cleanest and preserves your existing resource names (`shophive`, `shophive-db`, `shophive-kv`), preventing you from having to update multiple configuration files.

### Step 1: Backup your Database (East Asia)
1. Add a firewall rule to allow your local machine's IP to connect:
   ```bash
   MY_IP=$(curl -s https://api.ipify.org)
   az postgres flexible-server firewall-rule create \
     --resource-group shophive-rg \
     --server-name shophive-db \
     --name AllowLocalDev \
     --start-ip-address $MY_IP \
     --end-ip-address $MY_IP
   ```
2. Run `pg_dump` locally to back up your PostgreSQL database schema and data:
   ```bash
   pg_dump "host=shophive-db.postgres.database.azure.com port=5432 dbname=ecommercedb user=shophiveadmin sslmode=require" -F c -b -v -f local_db_backup.dump
   ```
   *(Enter your PostgreSQL password when prompted).*

### Step 2: Download Blob Storage Assets (East Asia)
Download your product images and reviews locally to restore them later:
```bash
# Create local backup directories
mkdir -p local_blob_backup/products local_blob_backup/reviews local_blob_backup/logs

# Download products container
az storage blob download-batch \
  --source products \
  --destination ./local_blob_backup/products \
  --account-name shophive \
  --auth-mode key

# Download reviews container
az storage blob download-batch \
  --source reviews \
  --destination ./local_blob_backup/reviews \
  --account-name shophive \
  --auth-mode key

# Download logs container
az storage blob download-batch \
  --source logs \
  --destination ./local_blob_backup/logs \
  --account-name shophive \
  --auth-mode key
```

### Step 3: Delete East Asia Resources (To Free Up Names)
Because resource names like Key Vault and Storage Account must be globally unique, you must delete the old ones in East Asia before Bicep can recreate them in South India.

```bash
# 1. Delete PostgreSQL Flexible Server
az postgres flexible-server delete --name shophive-db --resource-group shophive-rg --yes

# 2. Delete Key Vault (does not accept --resource-group or --yes, confirm interactively)
az keyvault delete --name shophive-kv

# 3. PURGE the Key Vault immediately (Crucial: key vaults are soft-deleted by default and hold onto the name)
az keyvault purge --name shophive-kv --no-wait

# 4. Delete Storage Account
az storage account delete --name shophive --resource-group shophive-rg --yes

# 5. Delete Static Web App
az staticwebapp delete --name shophive-swa --resource-group shophive-rg --yes
```

> [!WARNING]
> Wait **3 to 5 minutes** after purging the Key Vault and deleting the Storage Account to allow Azure DNS to release the global names.

### Step 4: Re-Deploy using Bicep to South India
Deploy your Bicep template, specifying `southindia` as the location:
```bash
az deployment group create \
  --resource-group shophive-rg \
  --template-file azure-bicep/main.bicep \
  --parameters postgresAdminPassword="YourPostgresSecurePassword123!" location="southindia"
```

Bicep will automatically detect that `shophive-api` (Container App) and `shophive-ind-env` (Container Environment) are already in South India, keep them intact, and deploy the new Key Vault, Database, and Storage Account next to them.

### Step 5: Restore Database to South India DB
1. Get the new PostgreSQL Flexible Server FQDN:
   ```bash
   az postgres flexible-server show \
     --name shophive-db \
     --resource-group shophive-rg \
     --query fullyQualifiedDomainName -o tsv
   ```
2. Allow your local machine's IP on the new database firewall:
   ```bash
   MY_IP=$(curl -s https://api.ipify.org)
   az postgres flexible-server firewall-rule create \
     --resource-group shophive-rg \
     --server-name shophive-db \
     --name AllowLocalDev \
     --start-ip-address $MY_IP \
     --end-ip-address $MY_IP
   ```
3. Restore the database dump:
   ```bash
   pg_restore -h <new-db-fqdn> -U shophiveadmin -d ecommercedb -v local_db_backup.dump
   ```

### Step 6: Restore Blob Assets
Upload the product and review images back to your newly created Storage Account in South India:
```bash
# Upload products
az storage blob upload-batch \
  --source ./local_blob_backup/products \
  --destination products \
  --account-name shophive \
  --auth-mode key

# Upload reviews
az storage blob upload-batch \
  --source ./local_blob_backup/reviews \
  --destination reviews \
  --account-name shophive \
  --auth-mode key

# Upload logs
az storage blob upload-batch \
  --source ./local_blob_backup/logs \
  --destination logs \
  --account-name shophive \
  --auth-mode key
```

---

## 🔄 Option B: Parallel Deployment (Zero Downtime)
If you want to migrate with zero service interruption, you can deploy the new resources alongside the old ones using a name suffix (e.g., `-ind`), copy the data cloud-to-cloud, update the container environment variables, and clean up afterwards.

### Step 1: Run Bicep with New Resource Names
To retain and reuse your existing **Container App (`shophive-api`)** and **Container Environment (`shophive-ind-env`)** that are already running in South India, as well as the **Static Web App (`shophive-swa`)** in East Asia, you must pass their exact existing names as parameters to the Bicep template.

Run the following command:
```bash
az deployment group create \
  --resource-group shophive-rg \
  --template-file azure-bicep/main.bicep \
  --parameters postgresAdminPassword="YourPostgresSecurePassword123!" \
               location="southindia" \
               storageAccountName="shophiveind" \
               postgresServerName="shophive-db-ind" \
               keyVaultName="shophive-kv-ind" \
               staticWebAppName="shophive-swa" \
               logAnalyticsWorkspaceName="shophive-log-ind" \
               containerAppEnvName="shophive-ind-env" \
               staticWebAppLocation="eastasia"
```

### Step 2: Migrate Database
1. Allow your IP on both database firewalls:
   ```bash
   MY_IP=$(curl -s https://api.ipify.org)
   
   # Allow on East Asia DB
   az postgres flexible-server firewall-rule create \
     --resource-group shophive-rg \
     --server-name shophive-db \
     --name AllowLocalDev \
     --start-ip-address $MY_IP \
     --end-ip-address $MY_IP

   # Allow on South India DB
   az postgres flexible-server firewall-rule create \
     --resource-group shophive-rg \
     --server-name shophive-db-ind \
     --name AllowLocalDev \
     --start-ip-address $MY_IP \
     --end-ip-address $MY_IP
   ```
2. Dump the database from East Asia and restore it into South India:
   ```bash
   # 1. Dump old DB
   pg_dump "host=shophive-db.postgres.database.azure.com port=5432 dbname=ecommercedb user=shophiveadmin sslmode=require" -F c -b -v -f live_db_backup.dump

   # 2. Restore into new DB
   pg_restore -h shophive-db-ind.postgres.database.azure.com -U shophiveadmin -d ecommercedb -v live_db_backup.dump
   ```

### Step 3: Fast Cloud-to-Cloud Blob Copy
Copy files directly between Azure storage accounts over Microsoft's high-speed backbone network:
```bash
# Retrieve connection string for target
DST_CONN=$(az storage account show-connection-string --name shophiveind --resource-group shophive-rg --query connectionString -o tsv)

# Retrieve account key for source
SRC_KEY=$(az storage account keys list --account-name shophive --resource-group shophive-rg --query "[0].value" -o tsv)

# Copy products container
az storage blob copy start-batch \
  --source-container products \
  --destination-container products \
  --source-account-name shophive \
  --source-account-key $SRC_KEY \
  --connection-string $DST_CONN

# Copy reviews container
az storage blob copy start-batch \
  --source-container reviews \
  --destination-container reviews \
  --source-account-name shophive \
  --source-account-key $SRC_KEY \
  --connection-string $DST_CONN

# Copy logs container
az storage blob copy start-batch \
  --source-container logs \
  --destination-container logs \
  --source-account-name shophive \
  --source-account-key $SRC_KEY \
  --connection-string $DST_CONN
```

### Step 3.5: Migrate Key Vault Secrets
Any secrets that Bicep dynamically generates (like the connection strings for PostgreSQL and Storage Account) have **already been created** inside the new Key Vault in South India (`shophive-kv-ind`) automatically.

However, if you have custom secrets (e.g. Stripe API keys, JWT secret keys, AI configuration keys) that were added manually to the old Key Vault (`shophive-kv`), you can copy them using this automated script.

#### 1. Grant Yourself Permissions to Both Vaults
Run this to allow your signed-in user account to read secrets from the old vault and write them to the new vault:
```bash
# Get your User Object ID
USER_OBJECT_ID=$(az ad signed-in-user show --query id -o tsv)

# Grant secret access to yourself on the old Key Vault
az keyvault set-policy --name shophive-kv --object-id $USER_OBJECT_ID --secret-permissions get list

# Grant secret access to yourself on the new Key Vault
az keyvault set-policy --name shophive-kv-ind --object-id $USER_OBJECT_ID --secret-permissions set get list
```

#### 2. Run the Secrets Migration Script
Execute this script in your terminal to automatically copy all custom secrets. This uses a `bash` subshell to ensure proper newline-splitting of the secret list:
```bash
bash -c '
SECRETS=$(az keyvault secret list --vault-name shophive-kv --query "[].name" -o tsv)

for SECRET in $SECRETS; do
  # Skip Bicep-managed connection strings to avoid overwriting them
  if [ "$SECRET" = "ConnectionStrings--Default" ] || [ "$SECRET" = "AzureStorage--ConnectionString" ] || [ "$SECRET" = "AzureStorage--BlobServiceUri" ]; then
    echo "Skipping Bicep-managed secret: $SECRET"
    continue
  fi

  echo "Migrating secret: $SECRET..."
  # Get secret value from old vault
  VAL=$(az keyvault secret show --vault-name shophive-kv --name "$SECRET" --query "value" -o tsv)
  # Write secret value to new vault
  az keyvault secret set --vault-name shophive-kv-ind --name "$SECRET" --value "$VAL" > /dev/null
done
'
```

### Step 4: Point the Container App to the New Key Vault
Update your API container app configuration so it fetches secrets from the new Key Vault in South India:
```bash
az containerapp update \
  --name shophive-api \
  --resource-group shophive-rg \
  --set-env-vars "KeyVault__VaultUri=https://shophive-kv-ind.vault.azure.net/"
```

### Step 5: Clean Up East Asia Resources (Once Verified)
Verify your application works correctly, then delete the old resources to stop incurring costs:
```bash
az postgres flexible-server delete --name shophive-db --resource-group shophive-rg --yes
az keyvault delete --name shophive-kv
az storage account delete --name shophive --resource-group shophive-rg --yes
az staticwebapp delete --name shophive-swa --resource-group shophive-rg --yes
```

---

## 🔐 A Note on Azure Container Registry (ACR)
You will notice `acrshophive` (Container Registry) is in **East Asia**.
* **Do you need to move it?** **No.**
* **Why?** ACR is only contacted when the Container App deploys a new revision or scales up new replicas. Once a replica is running, the container image is cached locally on the host. An East Asia ACR will **not** impact your application's transaction/API response times.
