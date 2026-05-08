$ErrorActionPreference = "Stop"
cd c:\Users\dhars\Desktop\Presidio\Genspark\frontend

# Create environments
New-Item -ItemType Directory -Force src\environments | Out-Null
Set-Content -Path src\environments\environment.ts -Value "export const environment = { production: false, apiUrl: 'http://localhost:5000/api' };"
Set-Content -Path src\environments\environment.development.ts -Value "export const environment = { production: false, apiUrl: 'http://localhost:5000/api' };"

# Services
npx ng g s services/auth --skip-tests
npx ng g s services/user --skip-tests
npx ng g s services/operator --skip-tests
npx ng g s services/admin --skip-tests

# Guards
npx ng g guard guards/auth --skip-tests --functional=true
npx ng g guard guards/role --skip-tests --functional=true

# Shared
npx ng g c shared/navbar --skip-tests
npx ng g c shared/footer --skip-tests

# Auth
npx ng g c auth/login --skip-tests
npx ng g c auth/register --skip-tests
npx ng g c auth/operator-register --skip-tests

# User
npx ng g c user/home --skip-tests
npx ng g c user/bus-list --skip-tests
npx ng g c user/bus-layout --skip-tests
npx ng g c user/checkout --skip-tests
npx ng g c user/bookings --skip-tests
npx ng g c user/profile --skip-tests

# Operator
npx ng g c operator/dashboard --skip-tests
npx ng g c operator/buses --skip-tests
npx ng g c operator/schedules --skip-tests
npx ng g c operator/layouts --skip-tests

# Admin
npx ng g c admin/dashboard --skip-tests
npx ng g c admin/operators --skip-tests
npx ng g c admin/buses --skip-tests
npx ng g c admin/routes --skip-tests
