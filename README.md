# Jerrygram 📸

**Jerrygram** is a full-stack Instagram clone built as a portfolio project.
It features modern UI/UX design, cloud integration, native mobile apps, and an AI chatbot powered by Azure OpenAI.

## 🧰 Tech Stack

### Frontend

* React + TypeScript + Tailwind CSS (Web)

### Backend

* ASP.NET Core (C#) — primary backend, hosted on Azure
* Spring Boot (Java) — secondary implementation (planned), hosted on AWS

### Mobile

* Android (Kotlin) — planned
* iOS (Swift) — planned

### Database

* PostgreSQL (managed via pgAdmin4)

### Cloud & Services

* Azure: Web API hosting, Blob Storage for image upload, Azure OpenAI for chatbot
* AWS: Bedrock, S3, EKS/Kubernetes

### AI

* Azure OpenAI — GPT-powered chatbot for user support

## 🧩 Features

* User authentication (signup/login)
* Posting photos with captions
* Like, comment, and follow system
* Responsive web and native mobile apps
* AI-powered chatbot assistant for user support and interaction

## 📁 Project Structure

```
jerrygram/
├── backend-dotnet/       # ASP.NET Core API (C#)
│   └── Jerrygram.Api/        # Main backend project
│       ├── Controllers/          # API endpoints (UserController, PostController, etc)
│       ├── Models/               # Database entities (User, Post, Comment, etc)
│       ├── Dtos/                 # Request/response DTOs
│       ├── Services/             # Business logic (BlobService, JwtService, etc)
│       ├── Interfaces/           # Interface definitions for services
│       ├── Data/                 # AppDbContext, Migrations, Seed logic
│       ├── Configurations/       # JWT, appsettings models
│       ├── Extensions/           # Startup extension methods (DI, middleware setup)
│       ├── Middleware/           # Custom middleware (error handling, JWT auth)
│       ├── Helpers/              # Utility functions (e.g. hash, token)
│       └── appsettings.json      # Environment configuration
├── backend-java/         # Spring Boot API (Java)
├── frontend-react/       # React web frontend (TypeScript + Tailwind CSS)
├── mobile-android/       # Android app (Kotlin)
├── mobile-ios/           # iOS app (Swift)
├── docs/                 # Documentation, ERD, API specs
```

## 🚀 Getting Started

Each directory contains its own README with setup and running instructions.

1. Clone the repository:

```bash
git clone https://github.com/wonhonChang/jerrygram.git
```

2. Navigate to the desired project folder and follow instructions.

## 🤖 AI Chatbot

Integrated AI chatbot using Azure OpenAI to assist users, answer FAQs, and enhance user engagement across web and mobile apps.

## 📋 Project Management

Project planning is managed internally using Jira (private).
Technical documentation is publicly shared via Confluence (external link to be added).

## 🐳 Docker Support (Planned)

Containerized environment setup is planned for:

* Backend (ASP.NET Core API)
* PostgreSQL database
* Frontend (React app)
* Azure AI Chatbot service (via Docker + Azure CLI)

Docker Compose configuration and usage instructions will be added as development progresses.

## 📄 License

This project is licensed under the MIT License.
