# Jerrygram React Frontend

Language: [English](README.md) | [한국어](README.ko.md) | 日本語

Jerrygram の React + TypeScript web UI です。default では .NET API に接続し、authentication、feed、post creation、profile、search、notifications、comments、saved posts、Kafka based search trend screens を扱います。

## Stack

- React 18
- TypeScript
- React Router
- Axios
- Tailwind CSS
- React Icons
- Playwright E2E tests

## Local Environment

実行前に example environment file をコピーします。

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

Development server は `http://localhost:13000` で動作します。

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

- `build/`, `playwright-report/`, `test-results/`, `node_modules/`, local `.env`, dev server logs は ignore 対象で、commit しません。
- Root docs 用の screenshots は `npm run screenshots` で更新できます。
- UI は Docker stack の adjusted local ports を前提にしています。
