-- 1. CREACIÓN DEL LOGIN Y LA BASE DE DATOS
USE [master];
GO

IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'usuario_categoriasA')
BEGIN
    CREATE LOGIN [usuario_categoriasA] WITH PASSWORD = 'Categoria123@', CHECK_EXPIRATION = OFF, CHECK_POLICY = OFF;
END
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CategoriaDB_A')
BEGIN
    CREATE DATABASE [CategoriaDB_A];
END
GO

USE [CategoriaDB_A];
GO

-- 2. CREACIÓN DEL USUARIO Y PERMISOS
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'usuario_categoriasA')
BEGIN
    CREATE USER [usuario_categoriasA] FOR LOGIN [usuario_categoriasA];
END
GO

ALTER ROLE [db_datareader] ADD MEMBER [usuario_categoriasA];
ALTER ROLE [db_datawriter] ADD MEMBER [usuario_categoriasA];
GO

-- 3. ESQUEMA DE TABLAS
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categorias')
BEGIN
    CREATE TABLE [dbo].[Categorias] (
        [IdCategoria] INT IDENTITY(1,1) NOT NULL,
        [Nombre] NVARCHAR(100) NOT NULL,
        [Descripcion] NVARCHAR(250) NULL,
		[Estado] BIT NOT NULL DEFAULT 1,
        CONSTRAINT [PK_Categorias] PRIMARY KEY CLUSTERED ([IdCategoria] ASC)
    );
END
GO

-- 4. INSERTAR DATOS
INSERT INTO [dbo].[Categorias] ([Nombre], [Descripcion], [Estado])
VALUES 
('SUV', 'Vehículos de utilidad deportiva',1),
('Sedán', 'Vehículos turísticos de 3 volúmenes',1),
('Hatchback', 'Vehículos compactos',1);
GO