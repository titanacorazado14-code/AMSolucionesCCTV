# AM Soluciones - Sistema de Inventario y Ventas

Sistema de inventario y ventas para **AM Soluciones de Cámaras y Seguridad Electrónica: gestiona el catálogo de productos de un sistema CCTV (cámaras, DVR/NVR, discos duros, fuentes de poder, cableado y conectores), controla las existencias y registra ventas.

Proyecto final de la asignatura INF-5130. Desarrollado en C# con .NET 10, SQL Server y Entity Framework Core, con Clean Architecture, API REST, interfaz Razor Pages y pruebas automatizadas con xUnit.

Autor: Alberto Mercedes Astacio - Matrícula: 100648527

## Contenido

1. [Arquitectura](#arquitectura)
2. [Base de datos](#base-de-datos)
3. [Reglas de negocio](#reglas-de-negocio)
4. [Casos de uso](#casos-de-uso)
5. [Seguridad y roles](#seguridad-y-roles)
6. [API REST](#api-rest)
7. [Interfaz web](#interfaz-web)
8. [Requisitos](#requisitos)
9. [Cómo ejecutar el proyecto](#cómo-ejecutar-el-proyecto)
10. [Pruebas automatizadas](#pruebas-automatizadas)
11. [Control de versiones](#control-de-versiones)
12. [Uso de herramientas de inteligencia artificial](#uso-de-herramientas-de-inteligencia-artificial)

## Arquitectura

El proyecto sigue Clean Architecture con cinco proyectos. Las dependencias siempre apuntan hacia adentro, hacia `Domain`, que no depende de nadie.

| Proyecto | Responsabilidad | Depende de |
|---|---|---|
| `AMSoluciones.Domain` | Entidades con sus reglas de negocio, excepciones personalizadas e interfaces de repositorio | Nadie |
| `AMSoluciones.Application` | Casos de uso, servicios, DTOs y la abstracción de autenticación | Domain |
| `AMSoluciones.Infrastructure` | Entity Framework Core, SQL Server, repositorios, migraciones y ASP.NET Core Identity | Application, Domain |
| `AMSoluciones.Api` | API REST con autenticación JWT | Application, Infrastructure |
| `AMSoluciones.Web` | Interfaz Razor Pages con autenticación por cookie | Application, Infrastructure |

```
Api ──┐
      ├──> Application ──> Domain <── Infrastructure (implementa las interfaces de Domain)
Web ──┘
```

Conceptos de POO aplicados:

- Encapsulamiento: las entidades tienen setters privados y constructores que validan; el estado solo cambia mediante métodos (`Inventario.Reponer`, `Inventario.Descontar`, `Venta.Anular`).
- Composición: `Venta` controla sus `DetalleVenta`; el constructor de `DetalleVenta` es `internal`, por lo que solo `Venta` puede crearlos, y la colección se expone como solo lectura.
- Relación Producto-Categoría: cada producto pertenece a una categoría.
- Abstracción:los repositorios se definen como interfaces en Domain y se implementan en Infrastructure.
- Polimorfismo / herencia:jerarquía de excepciones (`DominioException` → `ConflictoException` → `StockInsuficienteException`, etc.) que la API traduce a códigos HTTP según el tipo.

### Estructura de carpetas

```
AMSolucionesCCTV/
├── src/
│   ├── AMSoluciones.Domain/          Entidades, Excepciones, Repositorios (interfaces), Enums, Constantes
│   ├── AMSoluciones.Application/     CasosDeUso, Servicios, Dtos, Abstracciones
│   ├── AMSoluciones.Infrastructure/  Persistencia (DbContext, Configuraciones, Repositorios), Migrations, Seguridad
│   ├── AMSoluciones.Api/             Controllers, Seguridad (JWT), Errores
│   └── AMSoluciones.Web/             Pages (Razor Pages), Paginas, wwwroot
├── tests/
│   ├── AMSoluciones.Domain.Tests/
│   ├── AMSoluciones.Application.Tests/
│   └── AMSoluciones.Infrastructure.Tests/
├── database/                         Script SQL generado desde las migraciones
├── docs/                             Diagrama entidad-relación
└── AMSolucionesCCTV.slnx
```

## Base de datos

Motor: SQL Server. Nombre de la base: `AMSolucionesCCTV`. La estructura se crea mediante migraciones de Entity Framework Core** (carpeta `src/AMSoluciones.Infrastructure/Migrations`). Además, `database/esquema.sql` contiene el script SQL equivalente generado a partir de esas migraciones.

### Tablas de dominio (5)

| Tabla | Descripción | Restricciones principales |
|---|---|---|
| `Categorias` | Categorías de productos | Nombre único, sin distinguir mayúsculas; no vacío |
| `Productos` | Catálogo | SKU único (solo A-Z, 0-9 y guiones, máx. 32); precio entre 0.01 y 999,999.99; FK a Categorías |
| `Inventarios` | Existencias, una fila por producto (clave primaria = `ProductoId`) | Disponible entre 0 y 1,000,000; columna `VersionFila` (rowversion) |
| `Ventas` | Cabecera de venta | Estado `Confirmada` o `Anulada`; total mayor que cero; fechas en UTC; anulada requiere fecha de anulación |
| `DetallesVenta` | Líneas de cada venta | Cantidad entre 1 y 1,000,000; no repetir producto en la misma venta; subtotal calculado por SQL Server |

Además, ASP.NET Core Identity crea sus propias tablas (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, etc.) y EF Core crea `__EFMigrationsHistory`.

### Decisiones de diseño

- `DetallesVenta` guarda una **copia del nombre y del precio** del producto al momento de la venta, para que el historial no cambie si el producto se edita después.
- `Inventarios` y `Ventas` incluyen una columna **rowversion** (`VersionFila`) para el **control de concurrencia optimista**: si dos operaciones intentan modificar las mismas existencias a la vez, la segunda falla y se revierte.
- El **subtotal** de cada línea es una columna calculada en SQL Server.
- Las restricciones `CHECK`, `UNIQUE` y las claves foráneas refuerzan en la base de datos las reglas que ya valida Domain.

### Diagrama entidad-relación

![Diagrama ER](docs/diagrama-er.png)

### Reconstruir la base de datos

Opción 1 (automática):** al iniciar la API o la Web se aplican las migraciones pendientes y se cargan los datos iniciales.

Opción 2 (manual, con la herramienta de EF):**

```bash
dotnet tool install --global dotnet-ef --version 10.0.0
dotnet ef database update --project src/AMSoluciones.Infrastructure --startup-project src/AMSoluciones.Api
```

**Opción 3 (script SQL):** ejecutar `database/esquema.sql` sobre una base vacía. Para regenerarlo:

```bash
dotnet ef migrations script --idempotent --project src/AMSoluciones.Infrastructure --startup-project src/AMSoluciones.Api --output database/esquema.sql
```

### Datos iniciales

Se cargan automáticamente si la base está vacía: 5 categorías, 11 productos CCTV con sus existencias, una venta confirmada, una venta anulada y los dos usuarios de prueba.

## Reglas de negocio

1. El precio debe ser mayor que cero, no superar 999,999.99 y tener como máximo dos decimales.
2. El SKU se guarda en mayúsculas, solo admite letras (A-Z), números y guiones, y es único.
3. Las existencias van de 0 a 1,000,000. Una reposición no puede superar ese máximo.
4. Una venta tiene entre 1 y 50 líneas, sin productos repetidos, con cantidades entre 1 y 1,000,000.
5. No se vende más de lo que hay en existencia; si una sola línea no alcanza, se rechaza toda la venta y no cambia nada.
6. El precio y el nombre de cada línea los toma el servidor del catálogo; el cliente solo envía producto y cantidad.
7. Anular una venta devuelve las cantidades al inventario, una sola vez; una venta anulada no se puede anular de nuevo y conserva su total y sus detalles.
8. No se puede eliminar una categoría con productos, ni un producto con existencias o que aparezca en alguna venta (aunque esté anulada).
9. Un producto nuevo nace con su inventario en cero, creado en la misma transacción.

## Casos de uso

Además del CRUD de categorías y productos:

| Caso de uso | Clase | Qué hace |
|---|---|---|
| Registrar venta | `RegistrarVentaUseCase` | Valida líneas y existencias, aplica precios del catálogo, descuenta el inventario y guarda venta, detalles y descuentos en una sola transacción |
| Reponer inventario | `ReponerInventarioUseCase` | Suma unidades a las existencias de un producto, respetando el máximo |
| Anular venta | `AnularVentaUseCase` | Marca la venta como anulada y devuelve las unidades al inventario |

Transacciones y concurrencia: cada caso de uso guarda todos sus cambios en un único `SaveChanges` (atómico). Si otra operación modificó las mismas existencias, el `rowversion` lo detecta, se revierte todo y se lanza `ConcurrenciaException` (HTTP 409).

## Seguridad y roles

- Identidad: ASP.NET Core Identity (contraseñas almacenadas con hash y sal).
- API: autenticación con JWT (firmado con HMAC-SHA256, expira a los 60 minutos).
- Web: autenticación con cookie de sesión.
- Roles:

| Rol | Permisos |
|---|---|
| `Administrador` | Todo: crear, editar y eliminar categorías y productos; reponer; vender; anular ventas |
| `Gestor` | Consultar, reponer inventario y registrar ventas |

### Usuarios de prueba (solo desarrollo)

| Rol | Correo | Contraseña |
|---|---|---|
| Administrador | admin@amsoluciones.com | Admin123! |
| Gestor | gestor@amsoluciones.com | Gestor123! |

## API REST

18 endpoints. Todos requieren el encabezado `Authorization: Bearer <token>`, excepto el login. Los errores se devuelven como `ProblemDetails` con una propiedad `code` estable.

| Método | Ruta | Rol | Descripción |
|---|---|---|---|
| POST | `/api/auth/login` | Público | Devuelve el JWT, el nombre y los roles |
| GET | `/api/dashboard` | Ambos | Indicadores del inicio |
| GET | `/api/categorias` | Ambos | Lista de categorías |
| GET | `/api/categorias/{id}` | Ambos | Detalle de una categoría |
| POST | `/api/categorias` | Administrador | Crear categoría |
| PUT | `/api/categorias/{id}` | Administrador | Actualizar categoría |
| DELETE | `/api/categorias/{id}` | Administrador | Eliminar categoría |
| GET | `/api/productos` | Ambos | Lista de productos |
| GET | `/api/productos/{id}` | Ambos | Detalle de un producto |
| POST | `/api/productos` | Administrador | Crear producto (con inventario en cero) |
| PUT | `/api/productos/{id}` | Administrador | Actualizar producto |
| DELETE | `/api/productos/{id}` | Administrador | Eliminar producto |
| GET | `/api/inventario` | Ambos | Existencias por producto |
| POST | `/api/inventario/{id}/reposiciones` | Ambos | Reponer inventario del producto `{id}` |
| GET | `/api/ventas` | Ambos | Lista de ventas |
| GET | `/api/ventas/{id}` | Ambos | Detalle de una venta |
| POST | `/api/ventas` | Ambos | Registrar venta |
| POST | `/api/ventas/{id}/anulacion` | Administrador | Anular venta |

| Código HTTP | Cuándo |
|---|---|
| 400 | Regla de negocio o validación (`DominioException`) |
| 401 / 403 | Sin sesión / rol sin permiso |
| 404 | Recurso inexistente (`NoEncontradoException`) |
| 409 | Conflicto: stock insuficiente, límite de inventario, venta ya anulada, SKU duplicado, concurrencia |

## Interfaz web

Razor Pages con diseño propio (sin dependencias externas). La Web usa los mismos casos de uso que la API.

| Pantalla | Función |
|---|---|
| Login | Inicio de sesión |
| Inicio | Indicadores y productos con pocas existencias |
| Categorías | Listar, crear, editar y eliminar (Administrador) |
| Productos | Listar, crear, editar y eliminar (Administrador) |
| Inventario | Ver existencias y reponer |
| Ventas | Historial, registrar venta, ver detalle y anular (Administrador) |

## Requisitos

- .NET SDK 10
- SQL Server (instancia local, por ejemplo `localhost,1433`)
- Git
- Opcional: DBeaver (o cualquier cliente SQL) para inspeccionar la base de datos

## Cómo ejecutar el proyecto

1. Clonar el repositorio

   ```bash
   git clone https://github.com/titanacorazado14-code/AMSolucionesCCTV.git
   cd AMSolucionesCCTV
   ```

2. Configurar la cadena de conexión en `src/AMSoluciones.Api/appsettings.json` y en `src/AMSoluciones.Web/appsettings.json`. Por defecto usa autenticación de Windows:

   ```
   Server=localhost,1433;Database=AMSolucionesCCTV;Trusted_Connection=True;TrustServerCertificate=True;
   ```

   Para autenticación con usuario y contraseña, reemplazar `Trusted_Connection=True;` por `User Id=...;Password=...;`.

3. Compilar

   ```bash
   dotnet build
   ```

4. Ejecutar la Web (crea la base de datos y carga los datos iniciales la primera vez):

   ```bash
   dotnet run --project src/AMSoluciones.Web
   ```

   Abrir en el navegador la dirección que indique la consola.

5. Ejecutar la API (opcional, para consumirla desde otro cliente):

   ```bash
   dotnet run --project src/AMSoluciones.Api
   ```

   Escucha en `http://localhost:5269`. CORS permite el origen `http://localhost:5080`, configurable en `appsettings.json` (sección `Cors`).

> La clave del JWT incluida en `appsettings.json` es solo para desarrollo; en un entorno real debe guardarse como secreto.

## Pruebas automatizadas

```bash
dotnet test
```

27 pruebas con xUnit:

| Proyecto | Pruebas | Qué verifican |
|---|---|---|
| `Domain.Tests` | 20 | Reglas de las entidades: precios, SKU, límites de inventario, total de venta, productos repetidos, máximo de líneas, anulación. Incluyen escenarios válidos y rechazados |
| `Application.Tests` | 5 | Casos de uso con repositorios en memoria: precio tomado del catálogo, venta rechazada sin cambiar el inventario, reposición, anulación con devolución de unidades y segunda anulación rechazada |
| `Infrastructure.Tests` | 2 | Contra **SQL Server real** (base temporal que se elimina al terminar): rollback completo ante un error de persistencia y ausencia de sobreventa con dos operaciones simultáneas |

Las pruebas de Infrastructure usan `localhost,1433` con autenticación de Windows. Para otra conexión, definir la variable de entorno `TF04_TEST_CONNECTION`.

## Control de versiones

El repositorio muestra la evolución incremental del proyecto mediante commits en Git: estructura de la solución, base de datos y documentación, Domain, Application, Infrastructure, API e interfaz web.

## Uso de herramientas de inteligencia artificial

Se utilizó Claude (Anthropic) como apoyo durante el desarrollo: para planificar el trabajo, proponer el diseño de la base de datos y de las capas, generar borradores de código y explicar conceptos de arquitectura. El autor revisó el código, lo compiló, lo ejecutó y lo probó (27 pruebas automatizadas y pruebas manuales de la API y de la interfaz), y está en capacidad de explicar y defender cada parte del proyecto.
