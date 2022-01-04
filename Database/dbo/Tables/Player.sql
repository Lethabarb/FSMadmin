CREATE TABLE [dbo].[Player] (
    [Id]        INT            NOT NULL IDENTITY(1,1),
    [Discord]   NVARCHAR (MAX) NOT NULL,
    [Battlenet] NVARCHAR (MAX) NOT NULL,
    [ulongId]   NUMERIC (20)   NOT NULL
);

