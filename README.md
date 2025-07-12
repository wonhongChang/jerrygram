# Jerrygram 📸

**Jerrygram** is a full-stack Instagram clone built as a portfolio project.
It features modern UI/UX design, cloud integration, native mobile apps, and an AI chatbot powered by Azure OpenAI.

## 🧰 Tech Stack

### Frontend

* React + TypeScript + Tailwind CSS (Web)

### Backend

* ASP.NET Core (C#) — primary backend, hosted on Azure
* Spring Boot (Java) — secondary implementation (planned), hosted on GCP

### Mobile

* Android (Kotlin) — planned
* iOS (Swift) — planned

### Database

* PostgreSQL (managed via pgAdmin4)

### Cloud & Services

* Azure: Web API hosting, Blob Storage for image upload, Azure OpenAI for chatbot
* Google Cloud Platform: Java backend deployment (future plan)

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
git clone https://github.com/yourusername/jerrygram.git
```

2. Navigate to the desired project folder and follow instructions.

## 🤖 AI Chatbot

Integrated AI chatbot using Azure OpenAI to assist users, answer FAQs, and enhance user engagement across web and mobile apps.

## 📋 Project Management

Project planning is managed internally using Jira (private).
Technical documentation is publicly shared via Confluence (external link to be added).

## 📄 License

This project is licensed under the MIT License.
