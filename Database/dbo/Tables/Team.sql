CREATE TABLE [dbo].[Team] (
    [Id]             UNIQUEIDENTIFIER            NOT NULL,
    [Name]           NVARCHAR (MAX) NOT NULL,
    [OrganisationId] UNIQUEIDENTIFIER            NOT NULL,
    [Captain]        NUMERIC(20)            NULL,
    [Rank] INT NULL, 
    [ImageName] NVARCHAR(MAX) NULL, 
    [ImagePath] NVARCHAR(MAX) NULL, 
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

