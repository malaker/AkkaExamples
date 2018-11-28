IF  NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'AkkaTest')
    BEGIN
        CREATE DATABASE [AkkaTest]
    END;

GO

USE AkkaTest


IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SomeContract' AND xtype='U')
   CREATE TABLE SomeContract (

Id bigint  PRIMARY KEY,
Content xml,
ModifiedOn datetime2 
)

