# Agentic Control Plane (.NET + React)

This app has been converted to:
- Backend: C# ASP.NET Core (`net8.0`)
- Frontend: TypeScript React (Vite + styled-components)

## Projects

- `src/AgenticControlPlane.Api`
- `src/AgenticControlPlane.Web`

## Run Backend

```powershell
cd src/AgenticControlPlane.Api
dotnet run --urls http://localhost:5174
```

## Run Frontend

In a second terminal:

```powershell
cd src/AgenticControlPlane.Web
npm install
npm run dev
```

Open:
- `http://localhost:5173`

## Reused Repository Assets

The API reads the existing governance files and workflows from this repo and returns dashboard data via:
- `GET /api/dashboard`

Assets include:
- `docs/agentic-operating-system/*.md`
- `docs/agentic-operating-system/templates/*.md`
- `.github/workflows/*.yml`

## Notes

- CORS is enabled for development in the API.
- The frontend proxies `/api` calls to `http://localhost:5174`.
