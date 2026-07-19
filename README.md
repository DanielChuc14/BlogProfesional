# BlogPlatform

Sistema de blog full-stack compuesto por un **backend en .NET Web API** (Clean Architecture) y un **frontend en Angular**. Permite registro y autenticación de usuarios, publicación y gestión de posts, comentarios, etiquetas, seguimiento entre usuarios, panel de administración y analítica básica.

- **Backend:** .NET Web API sobre PostgreSQL con Entity Framework Core, autenticación JWT y roles.
- **Frontend:** SPA en Angular (standalone components, señales) que consume el API vía `HttpClient`.

---

## Estructura de la solución

```
BlogProfesional/
├── backend/
│   └── BlogPlatform/
│       ├── BlogPlatform.API/              # Capa de presentación (Web API)
│       ├── BlogPlatform.Application/      # Capa de aplicación (casos de uso)
│       ├── BlogPlatform.Domain/           # Capa de dominio (entidades y contratos)
│       ├── BlogPlatform.Infrastructure/   # Capa de infraestructura (EF Core, servicios)
│       ├── BlogPlatform.UnitTests/        # Pruebas unitarias
│       └── BlogPlatform.IntegrationTests/ # Pruebas de integración
└── frontend/
    └── BlogUI/                            # Aplicación Angular (interfaz de usuario)
```

### Capas principales

| Capa | Proyecto | Responsabilidad |
|------|----------|-----------------|
| **API / Presentación** | `BlogPlatform.API` | Controllers, middleware, autenticación, CORS, Swagger, health checks. Punto de entrada HTTP. |
| **Application** | `BlogPlatform.Application` | Lógica de casos de uso, DTOs, interfaces de servicios y validadores (FluentValidation). Sin dependencias de infraestructura. |
| **Domain** | `BlogPlatform.Domain` | Entidades, enums e interfaces del núcleo del negocio. Independiente de frameworks. |
| **Infrastructure** | `BlogPlatform.Infrastructure` | Implementación de persistencia (EF Core + Npgsql), migraciones, servicios externos (storage, email, caché Redis). |
| **UI** | `frontend/BlogUI` | SPA en Angular: componentes, servicios HTTP, guards, interceptores y enrutado. |

---

## Tecnologías utilizadas

### Backend
- **.NET Web API** (`net10.0`)
- **Entity Framework Core** con **Npgsql** (PostgreSQL, convención *snake_case*)
- **ASP.NET Core Identity** + **JWT Bearer** para autenticación y roles
- **FluentValidation** para validación de requests
- **Redis** (opcional) como caché distribuida, con *fallback* a caché en memoria
- **Swagger / OpenAPI** para documentación interactiva
- **Rate limiting** nativo de ASP.NET Core

### Frontend
- **Angular** (v21) con **Angular CLI**
- **TypeScript**
- **Tailwind CSS** + CSS (vía PostCSS)
- **npm** como gestor de paquetes

---

## Requisitos previos

| Herramienta | Versión recomendada | Notas |
|-------------|---------------------|-------|
| **.NET SDK** | 10.0+ | `dotnet --version` |
| **Node.js** | 20+ (probado con 24) | `node --version` |
| **npm** | 10+ | `npm --version` |
| **Angular CLI** | 21+ | `npm install -g @angular/cli` |
| **PostgreSQL** | 14+ | Requerido por el backend (puerto 5432 por defecto) |
| **Redis** | 7+ | Opcional; si no está disponible se usa caché en memoria |

---

## Pasos para iniciar el proyecto

### 1. Backend

```bash
cd backend/BlogPlatform/BlogPlatform.API

# Restaurar dependencias
dotnet restore

# (Opcional) aplicar migraciones a la base de datos
dotnet ef database update --project ../BlogPlatform.Infrastructure --startup-project .

# Levantar el API en modo desarrollo
dotnet run --launch-profile http
```

El backend queda disponible en:
- **HTTP:** `http://localhost:5017`
- **HTTPS:** `https://localhost:7182`
- **Swagger UI:** `http://localhost:5017/swagger`

> Antes del primer arranque debes configurar los secretos (cadena de conexión, secreto JWT, contraseña del `SuperAdmin`). Ver [Configuración de secretos](#configuración-de-secretos).

### 2. Frontend

```bash
cd frontend/BlogUI

# Instalar dependencias
npm install

# Levantar el servidor de desarrollo y abrir el navegador
ng serve -o
```

La aplicación queda disponible en `http://localhost:4200`. El servidor de desarrollo aplica un **proxy** que redirige `/api` y `/uploads` al backend, evitando problemas de CORS y de puertos hardcodeados.

---

## Configuración de secretos

Por seguridad, el `appsettings.json` versionado **no contiene secretos**: los campos sensibles (`ConnectionStrings:Default`, `Jwt:Secret`, `Seed:SuperAdminPassword`, `Analytics:DailySaltPrefix`, `Google:ClientSecret`) están vacíos. Usa el archivo [`BlogPlatform.API/appsettings.example.json`](backend/BlogPlatform/BlogPlatform.API/appsettings.example.json) como referencia de la estructura y los valores esperados.

### Desarrollo local: User Secrets

Los secretos se cargan con [**User Secrets**](https://learn.microsoft.com/aspnet/core/security/app-secrets), que los guarda fuera del repositorio (en tu perfil de usuario), no en la carpeta del proyecto. En Development, .NET los combina automáticamente sobre `appsettings.json`.

```bash
cd backend/BlogPlatform/BlogPlatform.API

# Inicializar (solo la primera vez; ya está hecho si existe <UserSecretsId> en el .csproj)
dotnet user-secrets init

# Definir los secretos requeridos
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=blogplatform;Username=postgres;Password=TU_PASSWORD"
dotnet user-secrets set "Jwt:Secret" "una-clave-secreta-de-al-menos-32-caracteres"
dotnet user-secrets set "Seed:SuperAdminPassword" "TuPasswordSeguro"
dotnet user-secrets set "Analytics:DailySaltPrefix" "un-salt-cualquiera"

# (Opcional, si usas login con Google)
dotnet user-secrets set "Google:ClientId" "TU_CLIENT_ID"
dotnet user-secrets set "Google:ClientSecret" "TU_CLIENT_SECRET"

# Verificar
dotnet user-secrets list
```

### Producción: variables de entorno

En producción no se usan User Secrets. Define los mismos valores como variables de entorno del proveedor, usando `__` (doble guion bajo) como separador de secciones:

```bash
ConnectionStrings__Default="Host=...;Database=...;Username=...;Password=..."
Jwt__Secret="clave-secreta-de-produccion-min-32-chars"
Seed__SuperAdminPassword="..."
Analytics__DailySaltPrefix="..."
```

---

## Configuración de conexión

### CORS en el backend

La política de CORS se configura en `BlogPlatform.API/Program.cs` y depende del entorno:

- **Development:** permite cualquier origen (`AllowAnyOrigin`), útil para pruebas locales.
- **Production:** restringe a los orígenes definidos en `appsettings.json` → `Cors:AllowedOrigins`.

```jsonc
// appsettings.json
"Cors": {
  "AllowedOrigins": [ "https://tu-dominio-frontend.com" ]
}
```

El middleware se activa con `app.UseCors()` en el pipeline.

### Endpoints en el frontend

El frontend usa un **origen relativo** para no depender de puertos:

```typescript
// frontend/BlogUI/src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: '', // mismo origen; el proxy reenvía /api al backend
};
```

En desarrollo, el reenvío lo gestiona `proxy.conf.json` (registrado en `angular.json`):

```jsonc
// frontend/BlogUI/proxy.conf.json
{
  "/api":     { "target": "http://localhost:5017", "secure": false, "changeOrigin": true },
  "/uploads": { "target": "http://localhost:5017", "secure": false, "changeOrigin": true }
}
```

> Para cambiar el puerto del backend basta con editar `proxy.conf.json` en un único sitio.

---

## Endpoints principales del API

Base URL en desarrollo: `http://localhost:5017`

### Autenticación (`/api/auth`)

| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/api/auth/register` | Registrar un nuevo usuario |
| `POST` | `/api/auth/login` | Iniciar sesión (devuelve JWT) |
| `POST` | `/api/auth/refresh` | Renovar el token de acceso |
| `POST` | `/api/auth/logout` | Cerrar sesión |

### Posts (`/api/posts`)

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| `GET` | `/api/posts` | Público | Listar posts (feed paginado por cursor) |
| `GET` | `/api/posts/{slug}` | Público | Obtener un post por su slug |
| `GET` | `/api/posts/{id}` | Blogger+ | Obtener un post por id |
| `POST` | `/api/posts` | Blogger+ | Crear un post |
| `PUT` | `/api/posts/{id}` | Blogger+ | Editar un post |
| `DELETE` | `/api/posts/{id}` | Blogger+ | Eliminar un post |
| `PATCH` | `/api/posts/{id}/publish` | Blogger+ | Publicar un borrador |
| `POST` | `/api/posts/{id}/like` | Autenticado | Dar "me gusta" |

### Comentarios (`/api`)

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| `GET` | `/api/posts/{postId}/comments` | Público | Listar comentarios de un post |
| `POST` | `/api/posts/{postId}/comments` | Autenticado | Crear un comentario |
| `PUT` | `/api/comments/{id}` | Autenticado | Editar un comentario |
| `DELETE` | `/api/comments/{id}` | Autenticado | Eliminar un comentario |

### Usuarios (`/api/users`)

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| `GET` | `/api/users/{username}` | Público | Obtener el perfil público de un usuario |
| `PUT` | `/api/users/me` | Autenticado | Actualizar el perfil propio |
| `PUT` | `/api/users/me/avatar` | Autenticado | Actualizar el avatar |
| `PUT` | `/api/users/me/preferences` | Autenticado | Actualizar preferencias |

> Las rutas protegidas requieren la cabecera `Authorization: Bearer <token>`. Las marcadas como **Blogger+** exigen el rol `Blogger`, `Admin` o `SuperAdmin`.

### Health checks

| Ruta | Descripción |
|------|-------------|
| `GET /api/health` | Estado agregado (PostgreSQL, Redis, almacenamiento) |
| `GET /api/health/db` | Sonda ligera de conectividad con la base de datos |

---

## Checklist de validación

Flujo mínimo para verificar la comunicación frontend ↔ backend de extremo a extremo:

- [ ] **Autenticación:** iniciar sesión con un usuario con rol `Blogger` y obtener un JWT válido.
- [ ] **Crear post:** desde el editor de la UI, crear un post → `POST /api/posts` responde `200/201`.
- [ ] **Listar posts:** el nuevo post aparece en el feed → `GET /api/posts` lo incluye.
- [ ] **Editar post:** modificar título/contenido → `PUT /api/posts/{id}` y el cambio se refleja en la UI.
- [ ] **Borrar post:** eliminar el post → `DELETE /api/posts/{id}` y desaparece del feed.
- [ ] **Comentarios:** crear un comentario en un post → visible en el detalle del post.
- [ ] **CORS / conexión:** las llamadas del `HttpClient` no muestran errores de red ni de CORS en la consola del navegador.
- [ ] **Health:** `GET /api/health` devuelve el estado de las dependencias.

---

## Notas adicionales

### Swagger
En entorno de desarrollo, la documentación interactiva del API está disponible en `http://localhost:5017/swagger`. Permite explorar y probar todos los endpoints, incluida la autorización con token JWT (botón **Authorize**).

### Base de datos (EF Core)
La persistencia usa **Entity Framework Core** con **PostgreSQL** y convención *snake_case*. Comandos útiles:

```bash
# Crear una nueva migración
dotnet ef migrations add NombreMigracion \
  --project backend/BlogPlatform/BlogPlatform.Infrastructure \
  --startup-project backend/BlogPlatform/BlogPlatform.API

# Aplicar migraciones pendientes
dotnet ef database update \
  --project backend/BlogPlatform/BlogPlatform.Infrastructure \
  --startup-project backend/BlogPlatform/BlogPlatform.API
```

### Recomendaciones de despliegue

| Componente | Opciones recomendadas | Notas |
|------------|----------------------|-------|
| **Backend (.NET API)** | Azure App Service, Azure Container Apps | Publicar con `dotnet publish -c Release`; configurar `ConnectionStrings`, `Jwt` y `Cors:AllowedOrigins` como variables de entorno. |
| **Base de datos** | Azure Database for PostgreSQL | Aplicar migraciones antes del primer arranque. |
| **Frontend (Angular)** | Vercel, Netlify | Build con `ng build`; publicar el contenido de `dist/`. |
| **CORS en producción** | — | Añadir el dominio del frontend a `Cors:AllowedOrigins`, o servir el frontend detrás del mismo dominio (mismo origen) para evitar CORS. |

> **Seguridad:** el `appsettings.json` versionado no incluye secretos. Configúralos con User Secrets en local y con variables de entorno en producción. Ver [Configuración de secretos](#configuración-de-secretos).
