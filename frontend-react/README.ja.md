# Jerrygram React フロントエンド

Language: [English](README.md) | [한국어](README.ko.md) | 日本語

Jerrygram の React + TypeScript Web UI です。デフォルトで .NET API に接続し、認証、フィード、投稿作成、プロフィール、検索、通知、コメント、保存済み投稿、Kafka ベースの検索トレンド画面を提供します。

## 技術スタック

- React 18
- TypeScript
- React Router
- Axios
- Tailwind CSS
- React Icons
- Playwright E2E テスト

## ローカル環境

アプリ起動前にサンプルファイルをコピーします。

```powershell
Copy-Item .env.example .env
```

デフォルト値:

```env
PORT=13000
REACT_APP_API_URL=http://localhost:5018/api
```

## コマンド

```powershell
npm install
npm start
```

```powershell
npm.cmd run test:ci
npm.cmd run build
npm.cmd run e2e
```

開発サーバーは `http://localhost:13000` で起動します。

## 構成

```text
src/
|- components/
|  |- layout/
|  |- post/
|  \- ui/
|- contexts/
|- pages/
|- services/
|- types/
|- utils/
|- App.tsx
\- index.tsx

e2e/
|- fixtures/
\- jerrygram.spec.ts
```

## メモ

- `build/`, `playwright-report/`, `test-results/`, `node_modules/`, ローカル `.env`, 開発サーバーログは ignore 対象で、コミットしません。
- ルートドキュメント用スクリーンショットは `npm run screenshots` で更新できます。
- UI は Docker スタックで使用する調整済みローカルポートを前提にしています。
