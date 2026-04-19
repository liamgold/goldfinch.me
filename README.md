# Goldfinch.me

[![License](https://img.shields.io/badge/license-MIT-brightgreen?style=flat)](https://github.com/liamgold/goldfinch.me/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512bd4?logo=dotnet)](https://learn.microsoft.com/en-us/dotnet/)
[![Xperience](https://img.shields.io/badge/Xperience_by_Kentico-7F09B7?logo=kentico&logoColor=white)](https://www.kentico.com)
[![Vite](https://img.shields.io/badge/Vite-646CFF?logo=vite&logoColor=white)](https://vitejs.dev)
[![Stars](https://img.shields.io/github/stars/liamgold/goldfinch.me?style=flat&label=stars&logo=github)](https://github.com/liamgold/goldfinch.me/stargazers)

## Description

[Personal website](https://www.goldfinch.me) for Liam Goldfinch, featuring blog articles primarily focused on Kentico development and .NET technologies.

## 🧰 Tech Stack

**Backend:**
- [.NET 10](https://dotnet.microsoft.com/en-us/download) with ASP.NET Core MVC
- [Xperience by Kentico](https://docs.kentico.com/x/6wocCQ)

**Frontend:**
- Hand-written CSS (dark-only terminal/IDE aesthetic) — JetBrains Mono + Geist
- Vanilla JS progressive-enhancement modules (mobile drawer, ⌘K command palette, live search, TOC scrollspy)
- [Vite](https://vitejs.dev) — bundler for CSS + TypeScript entrypoints
- [highlight.js](https://highlightjs.org) — syntax highlighting inside code-block widgets
- [React](https://react.dev) — admin customisations only (not used on the public site)

## 📁 Project Structure

- `Goldfinch.Core` – Shared business logic and data models
- `Goldfinch.Admin` – Kentico admin interface customizations
- `Goldfinch.Web` – Main web application with blog functionality

## 🚀 Getting Started

### Prerequisites

- .NET 10 SDK (10.0.101)
- Microsoft SQL Server 2019 or newer (including SQL Server Express or LocalDB)
- Node.js (22.x)

### Setup Instructions

1. Clone the repository
2. Configure your database connection using either:
   - Add connection string to `appsettings.json` under `ConnectionStrings:CMSConnectionString`
   - Create a `connectionstrings.json` file in the `Goldfinch.Web` project root (recommended for local development)
3. **Build the front-end bundle:**
   ```bash
   cd src/Goldfinch.Web/wwwroot/sitefiles
   npm install
   npm run build
   ```
4. **Navigate to the web project and restore Kentico CI objects:**
   ```bash
   cd src/Goldfinch.Web
   dotnet run --kxp-ci-restore
   ```
5. **Run the project:**
   ```bash
   dotnet run
   ```
6. Visit the site and log in using the admin credentials below

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
