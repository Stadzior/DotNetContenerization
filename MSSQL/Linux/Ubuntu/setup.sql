CREATE DATABASE [MessageDb]
GO
USE [MessageDb]
GO
CREATE TABLE [dbo].[Message]
(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Content] [nvarchar](max),
    CONSTRAINT [PK_Message] PRIMARY KEY CLUSTERED 
    (
        [Id] ASC
    ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT INTO [dbo].[Message](Content) VALUES("First message")
INSERT INTO [dbo].[Message](Content) VALUES("Second message")
INSERT INTO [dbo].[Message](Content) VALUES("Third message")