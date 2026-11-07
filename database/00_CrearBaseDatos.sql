-- =====================================================================
-- AM Soluciones de Camaras y Seguridad Electronica
-- Script 00: crear la base de datos
-- EJECUTAR conectado a la base "master" (conexion normal de DBeaver).
-- =====================================================================

IF DB_ID(N'AMSolucionesCCTV') IS NULL
    CREATE DATABASE AMSolucionesCCTV;
