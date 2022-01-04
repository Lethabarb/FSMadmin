CREATE TABLE [dbo].[Team] (
    [Id]             INT            NOT NULL IDENTITY(1,1),
    [Name]           NVARCHAR (MAX) NOT NULL,
    [OrganisationId] INT            NOT NULL,
    [Captain]        INT            NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

