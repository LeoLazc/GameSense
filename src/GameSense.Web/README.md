# GameSense Web

The first GameSense frontend surface is the reviewer qualification quiz. It uses the ASP.NET Core API without exposing evaluation criteria or expected answers.

## Setup

```bash
cd src/GameSense.Web
npm install
copy .env.example .env.local
npm run dev
```

Set `VITE_API_BASE_URL` to the API origin, for example `http://localhost:5000` or `https://localhost:7000`. The API must be running with Development CORS enabled.

Available scripts: `npm run dev`, `npm run build`, `npm run lint`, and `npm run test`.
