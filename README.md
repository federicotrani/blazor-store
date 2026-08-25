# StorePro — Suite de Gestión de E-commerce

Aplicación de e-commerce construida con **.NET 10** que implementa una API REST con
controladores y un frontend **Blazor WebAssembly standalone** con interfaz **MudBlazor**.

## Características

- **Backend** — API REST (.NET 10, controladores)
  - Patrón **Repositorio** + **Unidad de Trabajo** sobre **Entity Framework Core 10** (code-first + migraciones)
  - Conexión a **SQL Server Express** local con autenticación de Windows
  - **JWT** para proteger los endpoints; hash de contraseñas con **PBKDF2**
  - **CORS** configurado para el frontend
  - **Swagger UI** disponible en `/swagger`
  - Carga de **imágenes de producto** almacenadas en `wwwroot/uploads` (carpeta pública)
- **Frontend** — Blazor WebAssembly standalone + MudBlazor 9
  - Páginas en **español**: Inicio de Sesión / Registro, Panel de Control, Productos, Categorías, Usuarios, Catálogo, Registros y Soporte
  - Detalle y edición de productos, categorías y usuarios
  - Subida de imágenes con vista previa
  - Gestión de roles y estado de usuarios
  - Filtros, búsqueda, paginación y exportación a CSV

## Estructura del proyecto

```
blazor-store/
├── StorePro.sln
├── StorePro.Api/                # Backend (API REST)
│   ├── Controllers/             # AuthController, ProductsController, CategoriesController,
│   │                            # UsersController, DashboardController
│   ├── Data/                    # StoreProDbContext, DbSeeder
│   ├── DTOs/                    # DTOs y contratos de entrada/salida
│   ├── Entities/                # User, Category, Product
│   ├── Repositories/            # GenericRepository + interfaces y UnitOfWork
│   ├── Services/                # PasswordService (PBKDF2) y TokenService (JWT)
│   ├── Migrations/              # Migraciones EF Core
│   ├── wwwroot/uploads/         # Imágenes subidas (creada en ejecución)
│   ├── Program.cs               # Configuración del host
│   └── appsettings.json         # Cadena de conexión y clave JWT
├── StorePro.Web/                # Frontend Blazor WebAssembly standalone
│   ├── Layout/                  # MainLayout (sidebar + barra superior), AuthLayout
│   ├── Pages/                   # Login, Index (Dashboard), Products, ProductDetail,
│   │                            # Categories, Users, UserDetail, Catalog, Logs, Soporte, NotFound
│   ├── Components/              # Diálogos de edición (Producto, Categoría, Usuario)
│   ├── Models/                  # DTOs espejo de la API + helpers de formato
│   ├── Services/                # AuthStateService, AuthService, ProductService, CategoryService,
│   │                            # UserService, DashboardService, LocalStorageService
│   └── wwwroot/                 # index.html, appsettings.json, css/app.css, js/app.js
└── design/                      # Capturas de referencia de UI
```

## Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (probado con `10.0.400`)
- **SQL Server Express** local (ejecutándose en `localhost\SQLEXPRESS`) con autenticación de Windows
- Herramienta global `dotnet-ef` (ya incluida si restauró el SDK actual):

  ```powershell
  dotnet tool install --global dotnet-ef
  ```

## Puesta en marcha

### 1. Backend

```powershell
# Restaurar dependencias y compilar
dotnet build

# Crear/actualizar la base de datos y generar los datos iniciales
dotnet ef database update --project StorePro.Api

# Iniciar la API (HTTP en http://localhost:5100)
dotnet run --project StorePro.Api --urls "http://localhost:5100"
```

Al arrancar en modo Desarrollo, la aplicación ejecuta automáticamente las migraciones
pendientes y siembra datos de ejemplo:

- **Usuario administrador** — `admin@storepro.dev` / `Admin123$`
- Usuarios adicionales: Sarah Jenkins, Michael Ross, David Chen, Lucía Fernández
- 4 categorías (Electrónica, Accesorios, Ropa, Hogar) y 8 productos

La Swagger UI queda disponible en `http://localhost:5100/swagger`.

### 2. Frontend

```powershell
dotnet run --project StorePro.Web
```

La aplicación Blazor se sirve por defecto en `http://localhost:5173`. La URL base
de la API puede ajustarse en `StorePro.Web/wwwroot/appsettings.json` (`ApiBaseUrl`).

## Endpoints principales

| Método | Ruta                                  | Descripción                              | Auth     |
| ------ | ------------------------------------- | ---------------------------------------- | -------- |
| POST   | `/api/auth/login`                     | Iniciar sesión y obtener token JWT       | No       |
| POST   | `/api/auth/register`                  | Registrar nuevo cliente                  | No       |
| GET    | `/api/auth/me`                        | Devuelve el usuario autenticado          | Sí       |
| GET    | `/api/products`                       | Listado paginado con filtros             | No       |
| GET    | `/api/products/{id}`                  | Detalle de producto                      | No       |
| POST   | `/api/products`                       | Crear producto                           | Admin/Manager |
| PUT    | `/api/products/{id}`                  | Actualizar producto                      | Admin/Manager |
| DELETE | `/api/products/{id}`                  | Eliminar producto                        | Admin    |
| POST   | `/api/products/{id}/image`            | Subir imagen (multipart, máx. 5 MB)      | Admin/Manager |
| GET    | `/api/categories`                     | Listado de categorías con contador       | No       |
| CRUD   | `/api/categories`                     | Crear / actualizar / eliminar            | Admin    |
| GET    | `/api/users`                          | Listado paginado con filtros             | Admin/Manager |
| CRUD   | `/api/users`                          | Crear / actualizar / eliminar            | Admin    |
| PATCH  | `/api/users/{id}/status`              | Cambiar estado (Activo / Suspendido)     | Admin    |
| GET    | `/api/dashboard/stats`                | Métricas globales del sistema            | Sí       |

## Configuración

- **Cadena de conexión** — `StorePro.Api/appsettings.json` (`ConnectionStrings.DefaultConnection`).
  Por defecto: `Server=localhost\SQLEXPRESS;Database=StoreProDb;Trusted_Connection=True;TrustServerCertificate=True`.
- **JWT** — bloque `Jwt` de `appsettings.json`. Cambie `Key` por una cadena aleatoria
  de al menos 32 caracteres para entornos de producción.
- **CORS** — la política `Frontend` permite orígenes `http(s)://localhost:5173`
  y `http(s)://localhost:5174`. Añada los suyos en `Program.cs` si es necesario.

## Notas

- Las imágenes subidas se persisten en `StorePro.Api/wwwroot/uploads/` con nombres
  únicos (GUID). Tamaño máximo 5 MB y formatos permitidos: jpg, jpeg, png, webp, gif.
- Las contraseñas se almacenan con **PBKDF2-SHA256** (100 000 iteraciones, sal aleatoria).
- Las traducciones a español residen en `Models/Models.cs` (`ProductStatuses.ToSpanish`, etc.)
  y en las propias páginas Razor.