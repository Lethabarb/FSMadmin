CREATE TABLE [dbo].[Team] (
    [Id]             INT            NOT NULL,
    [Name]           NVARCHAR (MAX) NOT NULL,
    [OrganisationId] INT            NOT NULL,
    [Captain]        INT            NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

