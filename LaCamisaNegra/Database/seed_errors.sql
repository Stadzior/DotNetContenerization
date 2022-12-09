CREATE DATABASE [ErrorDb]
GO
USE [ErrorDb]
GO
CREATE TABLE [dbo].[Errors]
(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Payload] varchar(max),
    [Description] varchar(max)
    CONSTRAINT [PK_Error] PRIMARY KEY CLUSTERED 
    (
        [Id] ASC
    ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT INTO [dbo].[Errors]([Payload], [Description]) VALUES("Payload1", "Example error from seeds.")
INSERT INTO [dbo].[Errors]([Payload], [Description]) VALUES("Payload2", "Example error from seeds.")
INSERT INTO [dbo].[Errors]([Payload], [Description]) VALUES("Payload3", "Example error from seeds.")