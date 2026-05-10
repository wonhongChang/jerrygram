# Jerrygram React 프론트엔드

언어: [English](README.md) | 한국어 | [日本語](README.ja.md)

Jerrygram의 React + TypeScript 웹 UI입니다. 기본적으로 .NET API에 연결되며 인증, 피드, 게시물 생성, 프로필, 검색, 알림, 댓글, 저장한 게시물, Kafka 기반 검색 트렌드 화면을 제공합니다.

## 기술 스택

- React 18
- TypeScript
- React Router
- Axios
- Tailwind CSS
- React Icons
- Playwright E2E 테스트

## 로컬 환경

실행 전에 예시 환경 파일을 복사합니다.

```powershell
Copy-Item .env.example .env
```

기본 값:

```env
PORT=13000
REACT_APP_API_URL=http://localhost:5018/api
```

## 명령어

```powershell
npm install
npm start
```

```powershell
npm run test:ci
npm run build
npm run e2e
```

개발 서버는 `http://localhost:13000`에서 실행됩니다.

## 구조

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

## 메모

- `build/`, `playwright-report/`, `test-results/`, `node_modules/`, 로컬 `.env`, dev server 로그는 ignore 대상이며 커밋하지 않습니다.
- 루트 문서용 스크린샷은 `npm run screenshots`로 갱신할 수 있습니다.
- UI는 Docker 스택에서 사용하는 조정된 로컬 포트 구성을 기준으로 동작합니다.
