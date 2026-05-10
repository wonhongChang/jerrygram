# Jerrygram React Frontend

Language: English | [한국어](README.ko.md) | [日本語](README.ja.md)

React + TypeScript web UI for Jerrygram. It connects to the .NET API by default and covers authentication, feed, post creation, profile, search, notifications, comments, saved posts, and Kafka-backed search trend screens.

## Stack

- React 18
- TypeScript
- React Router
- Axios
- Tailwind CSS
- React Icons
- Playwright E2E tests

## Local Environment

Copy the example file before running the app:

```powershell
Copy-Item .env.example .env
```

Default values:

```env
PORT=13000
REACT_APP_API_URL=http://localhost:5018/api
```

## Commands

```powershell
npm install
npm start
```

```powershell
npm run test:ci
npm run build
npm run e2e
```

The development server runs on `http://localhost:13000`.

## Structure

```text
src/
├── components/
│   ├── layout/
│   ├── post/
│   └── ui/
├── contexts/
├── pages/
├── services/
├── types/
├── utils/
├── App.tsx
└── index.tsx

e2e/
├── fixtures/
└── jerrygram.spec.ts
```

## Notes

- `build/`, `playwright-report/`, `test-results/`, `node_modules/`, local `.env`, and dev server logs are ignored and should not be committed.
- Screenshots for the root documentation can be refreshed with `npm run screenshots`.
- The UI is designed around the adjusted local port set used by the Docker stack.
