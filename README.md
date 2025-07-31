# Goldfinch.me

[Personal website](https://www.goldfinch.me) for Liam Goldfinch, featuring blog articles primarily focused on Kentico development and .NET technologies.

![License](https://img.shields.io/github/license/liamgold/goldfinch.me)
![Last Commit](https://img.shields.io/github/last-commit/liamgold/goldfinch.me)
![.NET](https://img.shields.io/badge/.NET-9.0-512bd4?logo=dotnet)
![Xperience](https://img.shields.io/badge/Xperience_by_Kentico-7F09B7?logo=kentico&logoColor=white)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-%2338B2AC?logo=tailwind-css&logoColor=white)

## 🧰 Tech Stack

**Backend:**
- [.NET 9](https://dotnet.microsoft.com/en-us/download) with ASP.NET Core MVC
- [Xperience by Kentico](https://docs.kentico.com/x/6wocCQ)

**Frontend:**
- [Tailwind CSS](https://tailwindcss.com)
- [Vite](https://vitejs.dev) (build tool)
- [React](https://react.dev) (for admin customizations)

## 📁 Project Structure

- `Goldfinch.Core` – Shared business logic and data models
- `Goldfinch.Admin` – Kentico admin interface customizations
- `Goldfinch.Web` – Main web application with blog functionality

## 🚀 Getting Started

### Prerequisites

- .NET 9 SDK (9.0.106)
- Microsoft SQL Server 2019 or newer (including SQL Server Express or LocalDB)
- Node.js (22.x)

### Setup Instructions

1. Clone the repository
2. Configure your database connection using either:
   - Add connection string to `appsettings.json` under `ConnectionStrings:CMSConnectionString`
   - Create a `connectionstrings.json` file in the `Goldfinch.Web` project root (recommended for local development)
3. **Navigate to the web project and restore Kentico CI objects:**
   ```bash
   cd src/Goldfinch.Web
   dotnet run --kxp-ci-restore
   ```
4. **Run the project:**	
   ```bash
   dotnet run
   ```
5. Visit the site and log in using the admin credentials below

> **Note:** This project uses Kentico's Continuous Integration (CI) system.  
> This repository does not include a database backup. Kentico objects can be created from the included CI files.

## 🔐 Admin Login (Local Only)

If using the included CI files, an admin user will be created automatically:

- **Username:** `admin`
- **Password:** `Test123!`

> ⚠️ For local development only.  
> Do not reuse these credentials in any production environment.

## 🌍 Environment URLs

| Environment | URL                        |
|-------------|----------------------------|
| Local       | `https://localhost:52623`  |
| Production  | `https://www.goldfinch.me` |

## 🤝 Contributions

This repository is primarily for reference and personal use.

- Public contributions are **not expected**
- Any PRs will be accepted **only after explicit review and approval** by myself

## 📜 License

This project is licensed under the [MIT License](./LICENSE).
