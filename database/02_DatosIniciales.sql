-- =====================================================================
-- AM Soluciones de Camaras y Seguridad Electronica
-- Script 02: datos iniciales (ejemplos) y consultas de verificacion
-- Los precios estan en pesos dominicanos y son solo de ejemplo.
-- Se puede ejecutar varias veces sin duplicar datos.
-- =====================================================================

USE AMSolucionesCCTV;

-- Categorias ---------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Categorias)
INSERT INTO dbo.Categorias (Nombre, Descripcion) VALUES
    (N'Cámaras',            N'Cámaras bala, domo, PTZ, analógicas HD e IP'),
    (N'Grabadores DVR/NVR', N'Grabadores de video digital y de red'),
    (N'Discos duros',       N'Discos para videovigilancia'),
    (N'Fuentes de poder',   N'Fuentes individuales y centrales'),
    (N'Cables',             N'UTP, siamés y coaxial'),
    (N'Conectores',         N'BNC, balun, DC y similares'),
    (N'Switches PoE',       N'Switches para cámaras IP'),
    (N'Accesorios',         N'Cajas de paso, soportes y otros'),
    (N'Servicios',          N'Instalación y mantenimiento (sin stock)');

-- Tabla temporal con los productos de ejemplo --------------------------
IF OBJECT_ID(N'tempdb..#Seed') IS NOT NULL DROP TABLE #Seed;

CREATE TABLE #Seed (
    Sku NVARCHAR(32), Nombre NVARCHAR(120), Marca NVARCHAR(60), Modelo NVARCHAR(60),
    Categoria NVARCHAR(80), PrecioCompra DECIMAL(10,2), PrecioVenta DECIMAL(10,2),
    EsServicio BIT, Disponible INT, StockMinimo INT
);

INSERT INTO #Seed VALUES
    (N'CAM-BALA-2MP',   N'Cámara bala 2MP 1080p',            N'Hikvision', N'DS-2CE16D0T-IRF',  N'Cámaras',            950,  1450,  0, 20, 5),
    (N'CAM-DOMO-2MP',   N'Cámara domo 2MP 1080p',            N'Hikvision', N'DS-2CE56D0T-IRPF', N'Cámaras',            900,  1400,  0, 15, 5),
    (N'CAM-IP-4MP',     N'Cámara IP bala 4MP PoE',           N'Dahua',     N'IPC-HFW1431S',     N'Cámaras',            2800, 3900,  0,  8, 3),
    (N'CAM-PTZ-2MP',    N'Cámara PTZ 2MP zoom 4x',           N'Dahua',     N'SD1A204T',         N'Cámaras',            7500, 10500, 0,  3, 1),
    (N'DVR-4CH',        N'DVR 4 canales 1080p',              N'Hikvision', N'DS-7204HGHI',      N'Grabadores DVR/NVR', 2600, 3800,  0,  6, 2),
    (N'NVR-8CH-POE',    N'NVR 8 canales con PoE',            N'Dahua',     N'NVR2108HS-8P',     N'Grabadores DVR/NVR', 5200, 7200,  0,  4, 2),
    (N'HDD-1TB-SURV',   N'Disco duro 1TB videovigilancia',   N'Seagate',   N'SkyHawk 1TB',      N'Discos duros',       2700, 3600,  0, 10, 3),
    (N'HDD-2TB-SURV',   N'Disco duro 2TB videovigilancia',   N'Western Digital', N'WD Purple 2TB', N'Discos duros',      3600, 4800,  0,  8, 3),
    (N'FUENTE-12V-2A',  N'Fuente de poder 12V 2A',           N'Genérica',  N'12V 2A',           N'Fuentes de poder',   180,  320,   0, 40, 10),
    (N'FUENTE-12V-10A', N'Fuente central 12V 10A 9 salidas', N'Genérica',  N'12V 10A',          N'Fuentes de poder',   750,  1200,  0,  6, 2),
    (N'CABLE-UTP-305',  N'Rollo cable UTP Cat5e 305m',       N'Genérica',  N'Cat5e 305m',       N'Cables',             4200, 5800,  0,  5, 2),
    (N'CABLE-SIAMES-100',N'Cable siamés RG59 100m',          N'Genérica',  N'RG59 + 2x18',      N'Cables',             2300, 3200,  0,  6, 2),
    (N'CONECT-BNC',     N'Conector BNC de compresión',       N'Genérica',  N'BNC RG59',         N'Conectores',         12,   25,    0, 200, 50),
    (N'BALUN-HD',       N'Par de balun pasivo HD',           N'Genérica',  N'Balun 1CH',        N'Conectores',         90,   180,   0, 50, 15),
    (N'SW-POE-8P',      N'Switch PoE 8 puertos',             N'TP-Link',   N'TL-SF1008P',       N'Switches PoE',       3300, 4600,  0,  5, 2),
    (N'CAJA-PASO-EXT',  N'Caja de paso exterior para cámara',N'Genérica',  N'Caja plástica',    N'Accesorios',         60,   120,   0, 40, 10),
    (N'SRV-INST-CAM',   N'Instalación por cámara',           NULL,         NULL,                N'Servicios',          0,    1500,  1,  0, 0),
    (N'SRV-MANT-PREV',  N'Mantenimiento preventivo',         NULL,         NULL,                N'Servicios',          0,    2500,  1,  0, 0);

-- Productos ----------------------------------------------------------
INSERT INTO dbo.Productos (CategoriaId, Sku, Nombre, Marca, Modelo, PrecioCompra, PrecioVenta, EsServicio)
SELECT c.Id, s.Sku, s.Nombre, s.Marca, s.Modelo, s.PrecioCompra, s.PrecioVenta, s.EsServicio
FROM #Seed s
JOIN dbo.Categorias c ON c.Nombre = s.Categoria
WHERE NOT EXISTS (SELECT 1 FROM dbo.Productos p WHERE p.Sku = s.Sku);

-- Inventario (solo productos fisicos) --------------------------------
INSERT INTO dbo.Inventarios (ProductoId, Disponible, StockMinimo)
SELECT p.Id, s.Disponible, s.StockMinimo
FROM #Seed s
JOIN dbo.Productos p ON p.Sku = s.Sku
WHERE s.EsServicio = 0
  AND NOT EXISTS (SELECT 1 FROM dbo.Inventarios i WHERE i.ProductoId = p.Id);

-- Movimiento de entrada por el stock inicial ---------------------------
INSERT INTO dbo.MovimientosInventario (ProductoId, Tipo, Cantidad, Referencia)
SELECT p.Id, N'Entrada', s.Disponible, N'Stock inicial'
FROM #Seed s
JOIN dbo.Productos p ON p.Sku = s.Sku
WHERE s.EsServicio = 0 AND s.Disponible > 0
  AND NOT EXISTS (SELECT 1 FROM dbo.MovimientosInventario m
                  WHERE m.ProductoId = p.Id AND m.Referencia = N'Stock inicial');

DROP TABLE #Seed;

-- Clientes de ejemplo ------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Clientes)
INSERT INTO dbo.Clientes (Nombre, Telefono, Direccion, Email) VALUES
    (N'Cliente de ejemplo 1', N'809-000-0001', N'Hato Mayor del Rey', NULL),
    (N'Cliente de ejemplo 2', N'829-000-0002', N'San Pedro de Macorís', N'cliente2@ejemplo.com');

-- =====================================================================
-- VERIFICACION
-- =====================================================================

-- 1. Cantidad de filas por tabla (esperado: 9, 18, 16, 2, 0, 0, 16)
SELECT N'Categorias' AS Tabla, COUNT(*) AS Filas FROM dbo.Categorias
UNION ALL SELECT N'Productos',              COUNT(*) FROM dbo.Productos
UNION ALL SELECT N'Inventarios',            COUNT(*) FROM dbo.Inventarios
UNION ALL SELECT N'Clientes',               COUNT(*) FROM dbo.Clientes
UNION ALL SELECT N'Ventas',                 COUNT(*) FROM dbo.Ventas
UNION ALL SELECT N'DetallesVenta',          COUNT(*) FROM dbo.DetallesVenta
UNION ALL SELECT N'MovimientosInventario',  COUNT(*) FROM dbo.MovimientosInventario;

-- 2. Productos con categoria y stock
SELECT c.Nombre AS Categoria, p.Sku, p.Nombre, p.PrecioVenta, i.Disponible, i.StockMinimo
FROM dbo.Productos p
JOIN dbo.Categorias c ON c.Id = p.CategoriaId
LEFT JOIN dbo.Inventarios i ON i.ProductoId = p.Id
ORDER BY c.Nombre, p.Nombre;

-- 3. Productos con stock bajo (disponible <= minimo)
SELECT p.Sku, p.Nombre, i.Disponible, i.StockMinimo
FROM dbo.Inventarios i
JOIN dbo.Productos p ON p.Id = i.ProductoId
WHERE i.Disponible <= i.StockMinimo;
