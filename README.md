# 🧠 Project Manager App

Link to screenshots : https://drive.google.com/drive/folders/1t3jTAZ2XxzhEeT7NnWkZTteVHz7wGNJj?usp=sharing

**Please wait and try 2-3 times after a minute as backend is hosted on render free tier and it may take some time due to no frequent use**

A full-stack **Project Management Application** built using:
- 🖥️ **Frontend:** React + TypeScript + Vite + TailwindCSS  
- ⚙️ **Backend:** ASP.NET Core 8 Minimal API + Entity Framework Core  
- 🐳 **Containerized:** Docker (ready for Render or local Docker run)  
- 💾 **Database:** SQLite (in container)

Users can **Register**, **Login**, **Create Projects**, **Add Tasks**, and **Generate Smart Schedules**.

---

## 🚀 Live Demo

| Component | Deployment | Link |
|:--|:--|:--|
| **Frontend** | Netlify | calm-begonia-4b41bf.netlify.app |
| **Backend API** | Render (Docker) | https://project-manager-3hb7.onrender.com |

*(Replace with your actual deployed URLs)*

---

## 🧩 Tech Stack

| Layer | Technology |
|:--|:--|
| Frontend | React + TypeScript + Vite |
| Styling | Tailwind CSS |
| Backend | .NET 8 Minimal API |
| Database | SQLite / PostgreSQL |
| Auth | JWT (HS256) |
| Containerization | Docker |
| Deployment | Render (Docker) + Netlify |

---

## 📂 Folder Structure

```
PROJECT_MANAGER/
├── apps/
│   ├── apis/
│   │   └── ProjectManager.Api/         # Backend (ASP.NET 8)
│   └── project-manager-frontend/       # Frontend (React + Vite)
└── README.md
```

---

## ⚙️ Local Setup (Run Backend + Frontend)

### 1️⃣ Clone the repository
```bash
git clone https://github.com/manav-g27/project__manager_p.git
cd project__manager_p
```

---

### 2️⃣ Run the Backend (API)

#### Using .NET SDK (no Docker)
```bash
cd apps/apis/ProjectManager.Api
dotnet restore
dotnet run --urls "http://localhost:8080"
```

✅ API will run at: `http://localhost:8080`

---

#### Using Docker (recommended)
```bash
cd apps/apis/ProjectManager.Api
docker build -t projectmanager-api .
docker run -p 5288:8080   -e Jwt__Secret="$(openssl rand -base64 32)"   projectmanager-api
```

Test health:
```bash
curl http://localhost:5288/health
```
Output:
```json
{"ok":true,"app":"ProjectManager.Api"}
```

---

### 3️⃣ Run the Frontend (React)
```bash
cd ../project-manager-frontend
npm install
echo VITE_PM_API=http://localhost:8080 > .env.local
npm run dev
```

Frontend runs at:  
👉 [http://localhost:5173](http://localhost:5173)

---

### 4️⃣ Test the App
1. Open `http://localhost:5173`
2. Register a new user
3. Login
4. Create a project → add tasks → generate schedule ✅

---

## 🧱 API Endpoints

| Method | Endpoint | Description |
|:--|:--|:--|
| `POST` | `/api/v1/auth/register` | Register a new user |
| `POST` | `/api/v1/auth/login` | Login and get JWT |
| `GET` | `/api/v1/projects` | List user projects |
| `POST` | `/api/v1/projects` | Create a new project |
| `GET` | `/api/v1/projects/{id}/tasks` | Get tasks for a project |
| `POST` | `/api/v1/projects/{id}/tasks` | Add a new task |
| `PATCH` | `/api/v1/tasks/{id}/toggle` | Toggle task completion |
| `DELETE` | `/api/v1/tasks/{id}` | Delete a task |

---

## 🐳 Deploy Backend to Render (Docker)

### 1️⃣ Dockerfile
Located at:  
`apps/apis/ProjectManager.Api/dockerfile`

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

FROM base AS final
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet","ProjectManager.Api.dll"]
```

---

### 2️⃣ Render Setup
1. Go to [Render.com](https://render.com) → **New → Web Service**
2. Connect your GitHub repo
3. **Root Directory:** `apps/apis/ProjectManager.Api`
4. **Runtime:** Docker
5. **Environment Variables:**
   - `ASPNETCORE_ENVIRONMENT = Production`
   - `Jwt__Secret = <openssl rand -base64 32>`
6. Click **Create Web Service**
7. Once live, your API URL will look like:
   ```
   https://projectmanager-api.onrender.com
   ```

---

## 🌍 Deploy Frontend to Netlify

### 1️⃣ Build Settings
| Setting | Value |
|:--|:--|
| Base Directory | `apps/project-manager-frontend` |
| Build Command | `npm ci && npm run build` |
| Publish Directory | `apps/project-manager-frontend/dist` |

### 2️⃣ Environment Variables
```
VITE_PM_API=https://projectmanager-api.onrender.com
NODE_VERSION=20
```

Click **Deploy Site** — done 🎉

---

## 🧠 Troubleshooting

| Issue | Cause | Fix |
|:--|:--|:--|
| `IDX10720` | JWT secret too short | Use `openssl rand -base64 32` |
| 400 on Register | Username already exists | Use a new username |
| API not reachable | Wrong port or sleeping Render service | Check `/health` endpoint |
| 404 on `/api/tasks` | Wrong API (belongs to TaskManager, not ProjectManager) | Use `/api/v1/...` routes |
| Frontend error “Check credentials / server” | Wrong `VITE_PM_API` | Update `.env.local` or Netlify env |

---

## 🧑‍💻 Author

**Manav Gupta**  
🎓 M.Tech Computer Science | Full-Stack + AI/ML Enthusiast  
📧 [mg2002.gupta@gmail.com](mailto:mg2002.gupta@gmail.com)  
🔗 [LinkedIn](https://linkedin.com/in/manav-g27) • [GitHub](https://github.com/manav-g27)


