# Sistema de Seguimiento de Clasificación de Patrones

Este es un sistema completo de seguimiento de tareas desarrollado con ASP.NET Core 8 MVC, usando SQL Server y ADO.NET sin Entity Framework. El sistema permite la gestión de especialistas, plantillas de tareas con etapas, asignaciones a usuarios, seguimiento por etapas con evidencias, y un dashboard administrativo.

## Características Principales

- **Autenticación basada en cookies** sin Entity Framework Identity
- **Gestión de usuarios** con roles (Admin/Usuario)
- **Catálogo de especialistas** con operaciones CRUD
- **Plantillas de tareas** con etapas configurables
- **Asignación de tareas** a usuarios con especialistas
- **Seguimiento por etapas** con barras de progreso
- **Subida de evidencias** por etapa
- **Dashboard administrativo** con métricas y estadísticas
- **Interfaz moderna** con Bootstrap 5 y Font Awesome
- **Operaciones AJAX** sin uso de lógica Razor en las vistas

## Requisitos del Sistema

- Visual Studio 2022
- .NET 8 SDK
- SQL Server o SQL Server LocalDB
- Navegador web moderno

## Configuración e Instalación

### 1. Clonar el Repositorio

```bash
git clone https://github.com/CoralilloNet/SeguimientoClasificacionPatrones.git
cd SeguimientoClasificacionPatrones
```

### 2. Configurar la Base de Datos

**Opción A: Usando SQL Server LocalDB (recomendado para desarrollo)**

La cadena de conexión predeterminada en `appsettings.json` está configurada para LocalDB:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=SeguimientoTareas;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Opción B: Usando SQL Server completo**

Modifica la cadena de conexión en `src/SeguimientoTareas.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tu-servidor;Database=SeguimientoTareas;User Id=tu-usuario;Password=tu-contraseña;TrustServerCertificate=True;"
  }
}
```

### 3. Crear y Poblar la Base de Datos

Ejecuta el script de base de datos que contiene todo el DDL y datos de ejemplo:

```sql
-- Ejecutar el archivo: db/seguimiento.sql
-- Este script crea la base de datos completa con datos de ejemplo
```

**Desde SQL Server Management Studio:**
1. Abre SQL Server Management Studio
2. Conecta a tu instancia de SQL Server
3. Abre el archivo `db/seguimiento.sql`
4. Ejecuta el script completo

**Desde línea de comandos:**
```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -i "db/seguimiento.sql"
```

### 4. Abrir y Ejecutar el Proyecto

1. Abre `src/SeguimientoTareas.sln` en Visual Studio 2022
2. Restaura los paquetes NuGet (se hace automáticamente)
3. Compila la solución (Ctrl+Shift+B)
4. Ejecuta el proyecto (F5 o Ctrl+F5)

El navegador se abrirá automáticamente en `https://localhost:7xxx` o `http://localhost:5xxx`.

## Credenciales de Acceso

### Usuario Administrador
- **Email:** admin@example.com
- **Contraseña:** admin

### Usuarios de Prueba
- **Email:** juan.perez@example.com
- **Contraseña:** admin

- **Email:** maria.gonzalez@example.com
- **Contraseña:** admin

- **Email:** carlos.rodriguez@example.com
- **Contraseña:** admin

## Estructura del Proyecto

```
src/
├── SeguimientoTareas.sln              # Archivo de solución
└── SeguimientoTareas.Web/             # Proyecto web principal
    ├── Controllers/                   # Controladores MVC
    │   ├── Api/                      # Controladores de API
    │   ├── AccountController.cs      # Autenticación
    │   ├── AdminController.cs        # Vistas administrativas
    │   ├── HomeController.cs         # Página principal
    │   └── UserController.cs         # Vistas de usuario
    ├── Data/                         # Capa de acceso a datos
    │   └── Db.cs                     # Helper para ADO.NET
    ├── Models/                       # Modelos de datos
    ├── Services/                     # Servicios (hash de contraseñas, etc.)
    ├── Views/                        # Vistas Razor (sin lógica)
    │   ├── Account/                  # Vistas de autenticación
    │   ├── Admin/                    # Vistas administrativas
    │   ├── User/                     # Vistas de usuario
    │   └── Shared/                   # Layout y vistas compartidas
    └── wwwroot/                      # Archivos estáticos
        └── uploads/                  # Evidencias subidas por usuarios

db/
└── seguimiento.sql                   # Script completo de base de datos
```

## Arquitectura y Tecnologías

### Backend
- **ASP.NET Core 8 MVC** - Framework web
- **ADO.NET con SqlClient** - Acceso a datos sin ORM
- **Cookie Authentication** - Autenticación sin Identity
- **PBKDF2** - Hash de contraseñas seguro

### Frontend
- **Bootstrap 5** - Framework CSS via CDN
- **jQuery** - Manipulación DOM y AJAX via CDN
- **Font Awesome** - Iconografía via CDN
- **HTML estático** - Sin lógica Razor en las vistas

### Base de Datos
- **SQL Server** - Base de datos principal
- **Transacciones** - Para operaciones complejas
- **Índices** - Para optimización de consultas

## Funcionalidades Principales

### Panel de Administrador
- **Dashboard** con métricas en tiempo real
- **Gestión de especialistas** (CRUD completo)
- **Plantillas de tareas** con etapas configurables
- **Asignación de tareas** a usuarios
- **Seguimiento de progreso** por usuario

### Panel de Usuario
- **Mis tareas** con vista detallada
- **Actualización de progreso** por etapa
- **Subida de evidencias** (PDF, imágenes, documentos)
- **Indicadores visuales** de atraso y progreso

### Características Técnicas
- **Responsive design** compatible con móviles
- **Operaciones AJAX** para UX fluida
- **Validación client-side y server-side**
- **Manejo de errores** robusto
- **Subida de archivos** con validación

## API Endpoints

### Autenticación
- `POST /Account/Login` - Iniciar sesión
- `GET /Account/Logout` - Cerrar sesión

### Especialistas (Admin)
- `GET /api/specialists` - Listar especialistas
- `POST /api/specialists` - Crear especialista
- `PUT /api/specialists/{id}` - Actualizar especialista
- `DELETE /api/specialists/{id}` - Eliminar especialista

### Plantillas de Tareas (Admin)
- `GET /api/tasks/templates` - Listar plantillas
- `POST /api/tasks/templates` - Crear plantilla
- `PUT /api/tasks/templates/{id}` - Actualizar plantilla
- `DELETE /api/tasks/templates/{id}` - Eliminar plantilla
- `GET /api/tasks/templates/{id}/stages` - Listar etapas
- `POST /api/tasks/templates/{id}/stages` - Crear etapa
- `PUT /api/tasks/stages/{id}` - Actualizar etapa
- `DELETE /api/tasks/stages/{id}` - Eliminar etapa

### Asignaciones
- `POST /api/assignments` - Crear asignación (Admin)
- `GET /api/assignments/my` - Mis asignaciones (Usuario)
- `GET /api/assignments/{id}` - Detalles de asignación

### Progreso y Evidencias
- `POST /api/stages/{id}/progress` - Actualizar progreso
- `POST /api/stages/{id}/evidence` - Subir evidencia

### Dashboard
- `GET /api/admin/dashboard` - Métricas administrativas

## Desarrollo y Extensiones

### Agregar Nuevas Funcionalidades
1. Crear el modelo en `Models/Models.cs`
2. Implementar la lógica en un nuevo controlador de API
3. Crear la vista correspondiente sin lógica Razor
4. Usar AJAX para la comunicación con el backend

### Modificar la Base de Datos
1. Actualizar el script `db/seguimiento.sql`
2. Ejecutar los cambios en la base de datos
3. Actualizar los modelos C# correspondientes

### Personalizar la UI
- Modificar `Views/Shared/_Layout.cshtml` para cambios globales
- Personalizar estilos en secciones `@section Scripts` de cada vista
- Usar clases de Bootstrap 5 para el diseño

## Solución de Problemas

### Error de Conexión a Base de Datos
1. Verificar que SQL Server esté ejecutándose
2. Confirmar la cadena de conexión en `appsettings.json`
3. Ejecutar el script `db/seguimiento.sql` si la BD no existe

### Error de Autenticación
1. Verificar que las credenciales sean correctas
2. Comprobar que la tabla Users tenga datos
3. Revisar la configuración de cookies en `Program.cs`

### Archivos No Se Suben
1. Verificar permisos de escritura en `wwwroot/uploads/`
2. Comprobar el tamaño del archivo (máx. 20MB)
3. Verificar el tipo de archivo permitido

## Contribuciones

Este proyecto está diseñado como una implementación completa según los requerimientos especificados. Para contribuciones:

1. Fork el repositorio
2. Crea una rama para tu feature
3. Realiza tus cambios manteniendo la arquitectura existente
4. Envía un pull request

## Licencia

Este proyecto es de uso educativo y demostrativo.