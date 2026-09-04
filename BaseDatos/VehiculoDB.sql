-- 1. CREACIÓN DEL LOGIN Y LA BASE DE DATOS
USE [master];
GO

IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'usuario_vehiculosA')
BEGIN
    CREATE LOGIN [usuario_vehiculosA] WITH PASSWORD = 'Vehiculo123@', CHECK_EXPIRATION = OFF, CHECK_POLICY = OFF;
END
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'VehiculoDB_A')
BEGIN
    CREATE DATABASE [VehiculoDB_A];
END
GO

USE [VehiculoDB_A];
GO

-- 2. CREACIÓN DEL USUARIO Y PERMISOS
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'usuario_vehiculosA')
BEGIN
    CREATE USER [usuario_vehiculosA] FOR LOGIN [usuario_vehiculosA];
END
GO

ALTER ROLE [db_datareader] ADD MEMBER [usuario_vehiculosA];
ALTER ROLE [db_datawriter] ADD MEMBER [usuario_vehiculosA];
GO

-- 3. ESQUEMA DE TABLAS
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Vehiculos')
BEGIN
    CREATE TABLE [dbo].[Vehiculos] (
        [IdVehiculo] INT IDENTITY(1,1) NOT NULL,
        [IdCategoria] INT NOT NULL,
        [Marca] VARCHAR(150) NOT NULL,
        [Modelo] VARCHAR(150) NOT NULL,
        [Precio] DECIMAL(10,2) NOT NULL,
        [Stock] INT NOT NULL,
        [Estado] BIT NOT NULL DEFAULT 1,
        CONSTRAINT [PK_Vehiculos] PRIMARY KEY CLUSTERED ([IdVehiculo] ASC)
    );
END
GO

-- 4. INSERTAR DATOS
INSERT INTO [dbo].[Vehiculos] ([IdCategoria], [Marca], [Modelo], [Precio], [Stock], [Estado])
VALUES 
(1, 'Toyota', 'RAV4', 32500.00, 5, 1),
(2, 'Honda', 'Civic', 24800.00, 8, 1),
(3, 'Yamaha', 'MT-07', 8200.00, 10, 1);
GO