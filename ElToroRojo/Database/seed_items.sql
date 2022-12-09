CREATE DATABASE [ItemDb]
GO
USE [ItemDb]
GO
CREATE TABLE [dbo].[Items]
(
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Content] varchar(max)
    CONSTRAINT [PK_Item] PRIMARY KEY CLUSTERED 
    (
        [Id] ASC
    ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT INTO [dbo].[Items]([Content]) VALUES("Item1", "Example item from seeds.")
INSERT INTO [dbo].[Items]([Content]) VALUES("Item2", "Example item from seeds.")
INSERT INTO [dbo].[Items]([Content]) VALUES("Item3", "Example item from seeds.")