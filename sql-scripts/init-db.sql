USE [master]
GO

DROP DATABASE [people_db]
GO

CREATE DATABASE [people_db]
GO

USE [people_db]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[persons]
(
    [id] [numeric](18, 0) IDENTITY(1,1) NOT NULL
    ,[name] [nvarchar](50) NULL
    ,[age] [numeric](18, 0) NULL
) ON [PRIMARY]
GO
