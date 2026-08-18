# BBS Multi-Vendor E-Commerce Platform

[![Framework](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Frontend](https://img.shields.io/badge/Angular-19-DD0031?logo=angular)](https://angular.dev/)
[![Database](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A full-stack, enterprise-grade multi-vendor e-commerce platform built with **ASP.NET Core Web API (.NET 10)** and **Angular (v19)**. The platform features real-time notifications via SignalR, Stripe checkout integration, background processing via Hangfire, automated image conversion to WebP using SixLabors.ImageSharp, and LLM-powered assistant capabilities.

---

## Tech Stack

| Layer | Technologies Used |
| :--- | :--- |
| **Frontend** | Angular 19, TypeScript, RxJS, HTML5 Canvas WebP compression, CSS3 |
| **Backend API** | ASP.NET Core 10 Web API, C#, Entity Framework Core 10 |
| **Database** | PostgreSQL |
| **Authentication** | JWT (JSON Web Tokens) with Refresh Tokens & Role-Based Access Control (Admin, Vendor, Customer) |
| **Media Storage** | Local File Storage (`wwwroot/uploads`) & Azure Blob Storage fallback, ImageSharp WebP conversion |
| **Payments** | Stripe Payment Gateway API & Webhook processing |
| **Background Jobs**| Hangfire with PostgreSQL storage (Wishlist reminders, Token cleanup, Delivery scheduling) |
| **Real-time** | SignalR Websockets (Order notifications & live chat) |
| **Testing** | NUnit, Moq |

---

## Getting Started

### Prerequisites

Ensure you have the following installed on your machine:
* [Node.js](https://nodejs.org) (v18.0 or higher) & `npm`
* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* [PostgreSQL](https://www.postgresql.org/download/) (v15+)
* [Git](https://git-scm.com/)

---

### Installation & Quick Start

#### 1. Clone the Repository
```bash
git clone https://github.com/dharshini2022/E-Commerce.git
cd E-Commerce
```

#### 2. Database Setup & Restore

Create a PostgreSQL database named `Ecommerce` and restore the schema/seed data:

```bash
# Create database
createdb -U postgres Ecommerce

# Restore database dump
psql -U postgres -d Ecommerce -f ecommerce_backup.sql
```

*Or perform initial Entity Framework Core migrations:*
```bash
cd backend/Ecommerce.API
dotnet ef database update
```

#### 3. Backend API Startup
1. Update `backend/Ecommerce.API/appsettings.json` with your PostgreSQL database password and Stripe/SMTP credentials.
2. Launch the backend API:
   ```bash
   cd backend/Ecommerce.API
   dotnet restore
   dotnet run
   ```
3. Open Swagger API documentation at: **`http://localhost:5000/swagger`**

#### 4. Frontend Application Startup
In a new terminal window:
```bash
cd frontend
npm install
npm start
```
Open your browser at: **`http://localhost:4200`**

---

## Key Features

* 🛍️ **Multi-Vendor Management**: Vendors can manage product catalogs, inventory variants, images, and track settlements.
* 📷 **Automated WebP Image Processing**: Local files uploaded during product variant creation are automatically converted to compressed WebP format using `SixLabors.ImageSharp`.
* 💳 **Secure Stripe Checkout**: Complete checkout integration with Stripe sessions and webhook verification.
* 🔔 **Real-Time SignalR Notifications**: Live order confirmation and delivery status updates.
* ⏰ **Hangfire Background Scheduler**: Automatic cleanup of expired tokens and automated email notifications via SMTP.

---

## Running Unit Tests

Run all unit tests across API controllers, BLL services, and repository layers:

```bash
dotnet test backend/Ecommerce.Test/Ecommerce.Test.csproj
```

To run a specific test suite:
```bash
dotnet test backend/Ecommerce.Test/Ecommerce.Test.csproj --filter "FullyQualifiedName~UploadControllerTest"
```

---

## Directory Structure

```
.
├── backend/
│   ├── Ecommerce.API/          # ASP.NET Core Web API Controllers & Program.cs
│   ├── Ecommerce.BLL/          # Business logic services (Orders, Payments, LocalStorage)
│   ├── Ecommerce.DAL/          # Entity Framework Core DbContext & Repositories
│   ├── Ecommerce.Contracts/    # Interfaces & DTO contracts
│   ├── Ecommerce.Models/       # Domain Entities & Data Transfer Objects
│   └── Ecommerce.Test/         # NUnit & Moq test suite
├── frontend/
│   ├── src/app/components/     # Angular standalone components (Vendor, Customer, Admin)
│   ├── src/app/services/       # API Services (Product, Order, Payment, Auth)
│   └── src/app/models/         # TypeScript interfaces
└── ecommerce_backup.sql        # PostgreSQL database dump
```
