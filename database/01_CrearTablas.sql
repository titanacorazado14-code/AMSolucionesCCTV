-- =====================================================================
-- AM Soluciones de Camaras y Seguridad Electronica
-- Script 01: tablas de dominio (7 tablas)
-- Las tablas de ASP.NET Core Identity (usuarios y roles) las crea
-- EF Core mediante migraciones; no se incluyen aqui.
-- Se puede ejecutar varias veces sin error (IF OBJECT_ID ... IS NULL).
-- =====================================================================

USE AMSolucionesCCTV;

-- 1. Categorias ------------------------------------------------------
IF OBJECT_ID(N'dbo.Categorias', N'U') IS NULL
CREATE TABLE dbo.Categorias (
    Id           INT IDENTITY(1,1) NOT NULL,
    Nombre       NVARCHAR(80)  NOT NULL,
    Descripcion  NVARCHAR(250) NULL,
    Activa       BIT           NOT NULL CONSTRAINT DF_Categorias_Activa DEFAULT (1),
    CONSTRAINT PK_Categorias PRIMARY KEY (Id),
    CONSTRAINT UQ_Categorias_Nombre UNIQUE (Nombre),
    CONSTRAINT CK_Categorias_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0)
);

-- 2. Productos -------------------------------------------------------
IF OBJECT_ID(N'dbo.Productos', N'U') IS NULL
CREATE TABLE dbo.Productos (
    Id               INT IDENTITY(1,1) NOT NULL,
    CategoriaId      INT           NOT NULL,
    Sku              NVARCHAR(32)  NOT NULL,
    Nombre           NVARCHAR(120) NOT NULL,
    Marca            NVARCHAR(60)  NULL,
    Modelo           NVARCHAR(60)  NULL,
    Descripcion      NVARCHAR(500) NULL,
    PrecioCompra     DECIMAL(10,2) NOT NULL CONSTRAINT DF_Productos_PrecioCompra DEFAULT (0),
    PrecioVenta      DECIMAL(10,2) NOT NULL,
    EsServicio       BIT           NOT NULL CONSTRAINT DF_Productos_EsServicio DEFAULT (0),
    Activo           BIT           NOT NULL CONSTRAINT DF_Productos_Activo DEFAULT (1),
    FechaCreacionUtc DATETIME2(0)  NOT NULL CONSTRAINT DF_Productos_Fecha DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Productos PRIMARY KEY (Id),
    CONSTRAINT FK_Productos_Categorias FOREIGN KEY (CategoriaId) REFERENCES dbo.Categorias (Id),
    CONSTRAINT UQ_Productos_Sku UNIQUE (Sku),
    -- El SKU se guarda siempre en mayusculas (comparacion binaria)
    CONSTRAINT CK_Productos_SkuMayusculas CHECK (Sku = UPPER(Sku) COLLATE Latin1_General_BIN),
    CONSTRAINT CK_Productos_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0),
    CONSTRAINT CK_Productos_PrecioCompra CHECK (PrecioCompra >= 0),
    CONSTRAINT CK_Productos_PrecioVenta CHECK (PrecioVenta > 0),
    CONSTRAINT CK_Productos_Margen CHECK (PrecioVenta >= PrecioCompra)
);

-- 3. Inventarios (una fila por producto fisico) ------------------------
IF OBJECT_ID(N'dbo.Inventarios', N'U') IS NULL
CREATE TABLE dbo.Inventarios (
    ProductoId             INT          NOT NULL,
    Disponible             INT          NOT NULL CONSTRAINT DF_Inventarios_Disponible DEFAULT (0),
    StockMinimo            INT          NOT NULL CONSTRAINT DF_Inventarios_StockMinimo DEFAULT (0),
    FechaActualizacionUtc  DATETIME2(0) NOT NULL CONSTRAINT DF_Inventarios_Fecha DEFAULT (SYSUTCDATETIME()),
    VersionFila            ROWVERSION   NOT NULL,   -- control de concurrencia
    CONSTRAINT PK_Inventarios PRIMARY KEY (ProductoId),
    CONSTRAINT FK_Inventarios_Productos FOREIGN KEY (ProductoId) REFERENCES dbo.Productos (Id),
    CONSTRAINT CK_Inventarios_Disponible CHECK (Disponible >= 0),
    CONSTRAINT CK_Inventarios_StockMinimo CHECK (StockMinimo >= 0)
);

-- 4. Clientes --------------------------------------------------------
IF OBJECT_ID(N'dbo.Clientes', N'U') IS NULL
CREATE TABLE dbo.Clientes (
    Id         INT IDENTITY(1,1) NOT NULL,
    Nombre     NVARCHAR(120) NOT NULL,
    Telefono   NVARCHAR(30)  NULL,
    Direccion  NVARCHAR(250) NULL,
    Email      NVARCHAR(120) NULL,
    CONSTRAINT PK_Clientes PRIMARY KEY (Id),
    CONSTRAINT CK_Clientes_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0)
);

-- 5. Ventas (cabecera) -----------------------------------------------
IF OBJECT_ID(N'dbo.Ventas', N'U') IS NULL
CREATE TABLE dbo.Ventas (
    Id          INT IDENTITY(1,1) NOT NULL,
    ClienteId   INT           NULL,    -- NULL = venta de mostrador
    FechaUtc    DATETIME2(0)  NOT NULL CONSTRAINT DF_Ventas_Fecha DEFAULT (SYSUTCDATETIME()),
    Estado      NVARCHAR(20)  NOT NULL CONSTRAINT DF_Ventas_Estado DEFAULT (N'Completada'),
    Total       DECIMAL(12,2) NOT NULL,
    AnuladaUtc  DATETIME2(0)  NULL,
    VersionFila ROWVERSION    NOT NULL,
    CONSTRAINT PK_Ventas PRIMARY KEY (Id),
    CONSTRAINT FK_Ventas_Clientes FOREIGN KEY (ClienteId) REFERENCES dbo.Clientes (Id),
    CONSTRAINT CK_Ventas_Estado CHECK (Estado IN (N'Completada', N'Anulada')),
    CONSTRAINT CK_Ventas_Total CHECK (Total >= 0),
    -- Una venta anulada debe tener fecha de anulacion; una completada no
    CONSTRAINT CK_Ventas_Anulacion CHECK (
        (Estado = N'Completada' AND AnuladaUtc IS NULL) OR
        (Estado = N'Anulada'    AND AnuladaUtc IS NOT NULL))
);

-- 6. DetallesVenta (composicion: no existen sin su Venta) ---------------
IF OBJECT_ID(N'dbo.DetallesVenta', N'U') IS NULL
CREATE TABLE dbo.DetallesVenta (
    Id              INT IDENTITY(1,1) NOT NULL,
    VentaId         INT           NOT NULL,
    ProductoId      INT           NOT NULL,
    ProductoNombre  NVARCHAR(120) NOT NULL,   -- copia historica del nombre
    Cantidad        INT           NOT NULL,
    PrecioUnitario  DECIMAL(10,2) NOT NULL,   -- copia historica del precio
    Subtotal        DECIMAL(12,2) NOT NULL,
    CONSTRAINT PK_DetallesVenta PRIMARY KEY (Id),
    CONSTRAINT FK_DetallesVenta_Ventas FOREIGN KEY (VentaId) REFERENCES dbo.Ventas (Id) ON DELETE CASCADE,
    CONSTRAINT FK_DetallesVenta_Productos FOREIGN KEY (ProductoId) REFERENCES dbo.Productos (Id),
    CONSTRAINT UQ_DetallesVenta_VentaProducto UNIQUE (VentaId, ProductoId),
    CONSTRAINT CK_DetallesVenta_Cantidad CHECK (Cantidad > 0),
    CONSTRAINT CK_DetallesVenta_Precio CHECK (PrecioUnitario > 0),
    CONSTRAINT CK_DetallesVenta_Subtotal CHECK (Subtotal = Cantidad * PrecioUnitario)
);

-- 7. MovimientosInventario (historial de entradas y salidas) -------------
IF OBJECT_ID(N'dbo.MovimientosInventario', N'U') IS NULL
CREATE TABLE dbo.MovimientosInventario (
    Id          BIGINT IDENTITY(1,1) NOT NULL,
    ProductoId  INT           NOT NULL,
    Tipo        NVARCHAR(20)  NOT NULL,
    Cantidad    INT           NOT NULL,
    FechaUtc    DATETIME2(0)  NOT NULL CONSTRAINT DF_Movimientos_Fecha DEFAULT (SYSUTCDATETIME()),
    Referencia  NVARCHAR(100) NULL,
    VentaId     INT           NULL,
    CONSTRAINT PK_MovimientosInventario PRIMARY KEY (Id),
    CONSTRAINT FK_Movimientos_Productos FOREIGN KEY (ProductoId) REFERENCES dbo.Productos (Id),
    CONSTRAINT FK_Movimientos_Ventas FOREIGN KEY (VentaId) REFERENCES dbo.Ventas (Id),
    CONSTRAINT CK_Movimientos_Tipo CHECK (Tipo IN (N'Entrada', N'Salida', N'Anulacion')),
    CONSTRAINT CK_Movimientos_Cantidad CHECK (Cantidad > 0)
);
