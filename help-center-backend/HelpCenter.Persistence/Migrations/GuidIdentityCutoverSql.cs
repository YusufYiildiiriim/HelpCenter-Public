namespace HelpCenter.Persistence.Migrations;

internal static class GuidIdentityCutoverSql
{
    internal const string Down = "THROW 51000, 'Guid identity cutover Down is disabled; restore the verified pre-cutover backup to roll back.', 1;";

    internal const string SpDashboardStats = """
CREATE OR ALTER PROCEDURE dbo.Sp_DashboardStats
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM dbo.CustomerRequests WHERE IsDeleted=0) AS TotalRequests,
        (SELECT COUNT(*) FROM dbo.CustomerRequests WHERE StatusId IN ('10000000-0000-4000-8000-000000000001','10000000-0000-4000-8000-000000000002','10000000-0000-4000-8000-000000000004') AND IsDeleted=0) AS ActiveRequests,
        (SELECT COUNT(*) FROM dbo.CustomerRequests WHERE StatusId='10000000-0000-4000-8000-000000000003' AND IsDeleted=0) AS CompletedRequests,
        (SELECT COUNT(*) FROM dbo.CustomerRequestMessages WHERE IsDeleted=0) AS TotalMessages,
        (SELECT COUNT(*) FROM dbo.Customers WHERE IsDeleted=0 AND IsActive=1) AS TotalCustomers,
        (SELECT COUNT(*) FROM dbo.Companies WHERE IsDeleted=0) AS TotalCompanies;
END;
""";

    internal const string SpCompletedRequests = """
CREATE OR ALTER PROCEDURE dbo.Sp_CompletedRequests
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM dbo.CustomerRequests WHERE StatusId='10000000-0000-4000-8000-000000000003' AND IsDeleted=0;
END;
""";

    internal const string SpActiveRequests = """
CREATE OR ALTER PROCEDURE dbo.Sp_ActiveRequests
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM dbo.CustomerRequests WHERE StatusId IN ('10000000-0000-4000-8000-000000000001','10000000-0000-4000-8000-000000000002','10000000-0000-4000-8000-000000000004') AND IsDeleted=0;
END;
""";

    internal const string SpDailyTrend = """
CREATE OR ALTER PROCEDURE dbo.Sp_DailyTrend @Days int=30
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @start date=CAST(DATEADD(DAY,-(@Days-1),GETDATE()) AS date);
    ;WITH DateSeq AS (SELECT @start AS D UNION ALL SELECT DATEADD(DAY,1,D) FROM DateSeq WHERE D<CAST(GETDATE() AS date))
    SELECT ds.D AS [Date],
      ISNULL(SUM(CASE WHEN CAST(cr.CreatedAt AS date)=ds.D THEN 1 ELSE 0 END),0) AS Opened,
      ISNULL(SUM(CASE WHEN cr.StatusId='10000000-0000-4000-8000-000000000003' AND CAST(cr.UpdatedAt AS date)=ds.D THEN 1 ELSE 0 END),0) AS Closed
    FROM DateSeq ds LEFT JOIN dbo.CustomerRequests cr ON cr.IsDeleted=0 AND (CAST(cr.CreatedAt AS date)=ds.D OR CAST(cr.UpdatedAt AS date)=ds.D)
    GROUP BY ds.D ORDER BY ds.D OPTION (MAXRECURSION 366);
END;
""";

    internal const string SpAvgResolutionMinutes = """
CREATE OR ALTER PROCEDURE dbo.Sp_AvgResolutionMinutes
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ISNULL(AVG(CAST(DATEDIFF(MINUTE,CreatedAt,UpdatedAt) AS bigint)),0) AS AvgMinutes
    FROM dbo.CustomerRequests WHERE IsDeleted=0 AND StatusId='10000000-0000-4000-8000-000000000003' AND UpdatedAt IS NOT NULL;
END;
""";

    internal const string SpAgentPerformance = """
CREATE OR ALTER PROCEDURE dbo.Sp_AgentPerformance @Top int=20
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@Top) u.Id AS UserId,(a.FirstName+' '+a.LastName) AS FullName,COUNT(cr.Id) AS Assigned,
      SUM(CASE WHEN cr.StatusId='10000000-0000-4000-8000-000000000003' THEN 1 ELSE 0 END) AS Completed,
      ISNULL(AVG(CASE WHEN cr.StatusId='10000000-0000-4000-8000-000000000003' THEN CAST(DATEDIFF(MINUTE,cr.CreatedAt,cr.UpdatedAt) AS bigint) END),0) AS AvgResolutionMinutes
    FROM dbo.Users u INNER JOIN dbo.Accounts a ON a.Id=u.AccountId LEFT JOIN dbo.CustomerRequests cr ON cr.AssignedUserId=u.Id AND cr.IsDeleted=0
    WHERE u.IsDeleted=0 AND u.IsActive=1 GROUP BY u.Id,a.FirstName,a.LastName HAVING COUNT(cr.Id)>0 ORDER BY Assigned DESC;
END;
""";

    internal const string SpReopenStats = """
CREATE OR ALTER PROCEDURE dbo.Sp_ReopenStats
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @totalCompleted int=(SELECT COUNT(*) FROM dbo.CustomerRequests WHERE IsDeleted=0 AND StatusId='10000000-0000-4000-8000-000000000003');
    DECLARE @reopened int=(SELECT COUNT(DISTINCT rh.RequestId) FROM dbo.RequestHistories rh WHERE rh.Action=2 AND rh.OldValue='10000000-0000-4000-8000-000000000003' AND rh.IsDeleted=0);
    SELECT @totalCompleted AS TotalCompleted,@reopened AS Reopened,
      CASE WHEN @totalCompleted=0 THEN 0 ELSE CAST(@reopened*100.0/@totalCompleted AS int) END AS RatePercent;
END;
""";

    internal const string Up = """
SET XACT_ABORT ON;
IF OBJECT_ID(N'dbo.GuidIdentityMap', N'U') IS NOT NULL
    THROW 51000, 'GuidIdentityMap already exists; migration is not safe to rerun.', 1;

CREATE TABLE dbo.GuidIdentityMap
(
    EntityName sysname NOT NULL,
    LegacyId bigint NOT NULL,
    NewId uniqueidentifier NOT NULL,
    OriginalPublicId uniqueidentifier NULL,
    CONSTRAINT PK_GuidIdentityMap PRIMARY KEY (EntityName, LegacyId),
    CONSTRAINT UQ_GuidIdentityMap_Entity_NewId UNIQUE (EntityName, NewId)
);
CREATE TABLE dbo.GuidIdentityColumnMap
(
    EntityName sysname NOT NULL,
    ColumnName sysname NOT NULL,
    ReferencedEntityName sysname NULL,
    SqlType sysname NOT NULL,
    IsNullable bit NOT NULL,
    IsPrimaryKey bit NOT NULL,
    CONSTRAINT PK_GuidIdentityColumnMap PRIMARY KEY (EntityName, ColumnName)
);
CREATE TABLE dbo.GuidIdentityPrimaryKeyMap
(
    EntityName sysname NOT NULL PRIMARY KEY,
    ConstraintName sysname NOT NULL,
    IndexType nvarchar(60) NOT NULL
);
CREATE TABLE dbo.GuidIdentityForeignKeyMap
(
    ParentEntityName sysname NOT NULL,
    ConstraintName sysname NOT NULL,
    ParentColumnName sysname NOT NULL,
    ReferencedEntityName sysname NOT NULL,
    ReferencedColumnName sysname NOT NULL,
    DeleteAction nvarchar(60) NOT NULL,
    UpdateAction nvarchar(60) NOT NULL,
    IsNotForReplication bit NOT NULL,
    CONSTRAINT PK_GuidIdentityForeignKeyMap PRIMARY KEY (ParentEntityName, ConstraintName)
);
CREATE TABLE dbo.GuidIdentityIndexMap
(
    EntityName sysname NOT NULL,
    IndexName sysname NOT NULL,
    DropSql nvarchar(max) NOT NULL,
    CreateSql nvarchar(max) NOT NULL,
    CONSTRAINT PK_GuidIdentityIndexMap PRIMARY KEY (EntityName, IndexName)
);

DECLARE @Entities table (EntityName sysname PRIMARY KEY);
INSERT @Entities(EntityName) VALUES
(N'Accounts'),(N'Companies'),(N'CompanyModule'),(N'ConversationParticipants'),(N'Conversations'),
(N'CustomerRequestDocuments'),(N'CustomerRequestEvaluations'),(N'CustomerRequestMessages'),(N'CustomerRequests'),
(N'CustomerRequestStatuses'),(N'Customers'),(N'Documents'),(N'FAQs'),(N'Guides'),(N'MenuItems'),
(N'ModuleExperts'),(N'Modules'),(N'OrganizationInfos'),(N'ProjectModule'),(N'Projects'),(N'RequestHistories'),
(N'RequestSubjects'),(N'RolePermissionActions'),(N'RolePermissions'),(N'Roles'),(N'UserProjects'),(N'UserRoles'),
(N'Users'),(N'AuditLogs'),(N'RefreshTokens');

-- Only known legacy system seed identities may synthesize a deterministic ID when PublicId was absent.
CREATE TABLE #KnownSeedRows(EntityName sysname NOT NULL, LegacyId bigint NOT NULL, PRIMARY KEY(EntityName,LegacyId));
INSERT #KnownSeedRows(EntityName,LegacyId) VALUES (N'Accounts',1),(N'MenuItems',1),(N'MenuItems',10),(N'MenuItems',11),(N'MenuItems',12),(N'MenuItems',13),(N'MenuItems',14),(N'MenuItems',15),(N'MenuItems',2),(N'MenuItems',3),(N'MenuItems',4),(N'MenuItems',5),(N'MenuItems',6),(N'MenuItems',7),(N'MenuItems',8),(N'OrganizationInfos',1),(N'RolePermissionActions',1),(N'RolePermissionActions',10),(N'RolePermissionActions',11),(N'RolePermissionActions',12),(N'RolePermissionActions',13),(N'RolePermissionActions',14),(N'RolePermissionActions',15),(N'RolePermissionActions',16),(N'RolePermissionActions',17),(N'RolePermissionActions',18),(N'RolePermissionActions',19),(N'RolePermissionActions',2),(N'RolePermissionActions',20),(N'RolePermissionActions',21),(N'RolePermissionActions',22),(N'RolePermissionActions',23),(N'RolePermissionActions',24),(N'RolePermissionActions',25),(N'RolePermissionActions',26),(N'RolePermissionActions',27),(N'RolePermissionActions',3),(N'RolePermissionActions',4),(N'RolePermissionActions',5),(N'RolePermissionActions',50),(N'RolePermissionActions',51),(N'RolePermissionActions',52),(N'RolePermissionActions',53),(N'RolePermissionActions',54),(N'RolePermissionActions',6),(N'RolePermissionActions',7),(N'RolePermissionActions',70),(N'RolePermissionActions',71),(N'RolePermissionActions',72),(N'RolePermissionActions',8),(N'RolePermissionActions',80),(N'RolePermissionActions',81),(N'RolePermissionActions',9),(N'RolePermissions',1),(N'RolePermissions',10),(N'RolePermissions',11),(N'RolePermissions',12),(N'RolePermissions',13),(N'RolePermissions',14),(N'RolePermissions',15),(N'RolePermissions',16),(N'RolePermissions',2),(N'RolePermissions',20),(N'RolePermissions',21),(N'RolePermissions',22),(N'RolePermissions',23),(N'RolePermissions',3),(N'RolePermissions',30),(N'RolePermissions',4),(N'RolePermissions',40),(N'RolePermissions',5),(N'RolePermissions',6),(N'RolePermissions',8),(N'RolePermissions',9),(N'Roles',1),(N'Roles',2),(N'Roles',3),(N'Roles',4),(N'UserRoles',1),(N'Users',1),(N'CustomerRequestStatuses',1),(N'CustomerRequestStatuses',2),(N'CustomerRequestStatuses',3),(N'CustomerRequestStatuses',4);
IF EXISTS (SELECT 1 FROM @Entities e LEFT JOIN sys.tables t ON t.name=e.EntityName AND SCHEMA_NAME(t.schema_id)=N'dbo' WHERE t.object_id IS NULL)
    THROW 51000, 'Expected HelpCenter entity table is missing; stop before cutover.', 1;
IF EXISTS (
    SELECT 1 FROM @Entities e JOIN sys.tables t ON t.name=e.EntityName AND SCHEMA_NAME(t.schema_id)=N'dbo'
    LEFT JOIN sys.columns c ON c.object_id=t.object_id AND c.name=N'Id'
    LEFT JOIN sys.types ty ON ty.user_type_id=c.user_type_id
    WHERE c.column_id IS NULL OR ty.name NOT IN (N'int',N'bigint')
)
    THROW 51000, 'Expected legacy numeric Id column is missing or has an unsupported type.', 1;
IF EXISTS (
    SELECT 1 FROM @Entities e JOIN sys.tables t ON t.name=e.EntityName AND SCHEMA_NAME(t.schema_id)=N'dbo'
    WHERE e.EntityName NOT IN (N'AuditLogs',N'RefreshTokens')
      AND NOT EXISTS (SELECT 1 FROM sys.columns c WHERE c.object_id=t.object_id AND c.name=N'PublicId' AND c.user_type_id=TYPE_ID(N'uniqueidentifier'))
)
    THROW 51000, 'A BaseEntity table is missing its preserved PublicId column.', 1;
IF EXISTS (
    SELECT 1 FROM sys.indexes i JOIN sys.tables t ON t.object_id=i.object_id JOIN @Entities e ON e.EntityName=t.name
    WHERE i.index_id>0 AND i.is_primary_key=0 AND i.type NOT IN (1,2) AND SCHEMA_NAME(t.schema_id)=N'dbo'
)
    THROW 51000, 'Only rowstore clustered/nonclustered indexes are supported by this cutover manifest.', 1;
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys fk JOIN sys.tables rt ON rt.object_id=fk.referenced_object_id
    JOIN sys.tables pt ON pt.object_id=fk.parent_object_id
    JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id=fk.object_id
    JOIN sys.columns rc ON rc.object_id=rt.object_id AND rc.column_id=fkc.referenced_column_id
    JOIN @Entities er ON er.EntityName=rt.name
    LEFT JOIN @Entities ep ON ep.EntityName=pt.name
    WHERE rc.name=N'Id' AND (ep.EntityName IS NULL OR SCHEMA_NAME(pt.schema_id)<>N'dbo' OR SCHEMA_NAME(rt.schema_id)<>N'dbo')
)
    THROW 51000, 'An entity Id is referenced by a FK outside the explicit dbo entity manifest.', 1;
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys fk JOIN sys.tables rt ON rt.object_id=fk.referenced_object_id JOIN @Entities e ON e.EntityName=rt.name
    JOIN sys.indexes ri ON ri.object_id=rt.object_id AND ri.index_id=fk.key_index_id
    WHERE ri.is_unique=1 AND ri.is_primary_key=0
)
    THROW 51000, 'A foreign key references an alternate unique constraint; this manifest only migrates primary identity FKs.', 1;
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys fk JOIN sys.tables rt ON rt.object_id=fk.referenced_object_id JOIN @Entities e ON e.EntityName=rt.name
    JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id=fk.object_id JOIN sys.columns rc ON rc.object_id=rt.object_id AND rc.column_id=fkc.referenced_column_id
    WHERE rc.name=N'Id' AND (fk.is_disabled=1 OR fk.is_not_trusted=1)
)
    THROW 51000, 'Disabled or untrusted identity FK found; reconcile it before cutover.', 1;
IF EXISTS (
    SELECT 1 FROM sys.indexes i JOIN sys.tables t ON t.object_id=i.object_id JOIN @Entities e ON e.EntityName=t.name
    JOIN sys.partitions p ON p.object_id=i.object_id AND p.index_id=i.index_id
    WHERE i.index_id>0 AND i.is_primary_key=0 AND i.is_disabled=0 AND p.data_compression<>0
)
    THROW 51000, 'Compressed rowstore indexes are unsupported by this cutover manifest.', 1;
IF EXISTS (
    SELECT 1 FROM sys.indexes i JOIN sys.tables t ON t.object_id=i.object_id JOIN @Entities e ON e.EntityName=t.name
    WHERE i.index_id>0 AND i.is_primary_key=0 AND i.is_disabled=1 AND SCHEMA_NAME(t.schema_id)=N'dbo'
)
    THROW 51000, 'Disabled index found in the cutover manifest; rebuild or reconcile it first.', 1;
IF EXISTS (
    SELECT 1 FROM sys.indexes i JOIN sys.tables t ON t.object_id=i.object_id JOIN @Entities e ON e.EntityName=t.name
    WHERE i.index_id>0 AND i.is_primary_key=1
      AND (i.fill_factor<>0 OR i.ignore_dup_key=1 OR i.is_padded=1 OR i.allow_row_locks=0 OR i.allow_page_locks=0
           OR i.optimize_for_sequential_key=1 OR i.is_disabled=1)
)
    THROW 51000, 'Non-default primary-key index options are unsupported by this cutover manifest.', 1;
IF EXISTS (
    SELECT 1 FROM sys.indexes i JOIN sys.tables t ON t.object_id=i.object_id JOIN @Entities e ON e.EntityName=t.name
    JOIN sys.index_columns xc ON xc.object_id=i.object_id AND xc.index_id=i.index_id
    JOIN sys.columns cc ON cc.object_id=xc.object_id AND cc.column_id=xc.column_id
    JOIN dbo.GuidIdentityColumnMap cm ON cm.EntityName=t.name AND cm.ColumnName=cc.name
    WHERE i.index_id>0 AND i.is_primary_key=0
      AND (i.is_padded=1 OR i.optimize_for_sequential_key=1)
)
    THROW 51000, 'PAD_INDEX or OPTIMIZE_FOR_SEQUENTIAL_KEY on a migrated index is unsupported.', 1;
IF EXISTS (
    SELECT 1 FROM sys.stats s JOIN sys.indexes i ON i.object_id=s.object_id AND i.index_id=s.stats_id
    JOIN sys.tables t ON t.object_id=i.object_id JOIN @Entities e ON e.EntityName=t.name
    JOIN sys.index_columns xc ON xc.object_id=i.object_id AND xc.index_id=i.index_id
    JOIN sys.columns cc ON cc.object_id=xc.object_id AND cc.column_id=xc.column_id
    JOIN dbo.GuidIdentityColumnMap cm ON cm.EntityName=t.name AND cm.ColumnName=cc.name
    WHERE s.no_recompute=1
)
    THROW 51000, 'NO_RECOMPUTE index statistics on migrated columns are unsupported.', 1;
IF EXISTS (
    SELECT 1 FROM sys.key_constraints kc JOIN sys.indexes i ON i.object_id=kc.parent_object_id AND i.index_id=kc.unique_index_id
    WHERE kc.type=N'UQ' AND i.ignore_dup_key=1
)
    THROW 51000, 'IGNORE_DUP_KEY on a unique constraint is unsupported.', 1;

-- Empty PublicId may be repaired only for an explicitly known legacy system seed key.
DECLARE @checkEntity sysname,@checkSql nvarchar(max);
DECLARE public_id_check CURSOR LOCAL FAST_FORWARD FOR SELECT EntityName FROM @Entities WHERE EntityName NOT IN (N'AuditLogs',N'RefreshTokens');
OPEN public_id_check; FETCH NEXT FROM public_id_check INTO @checkEntity;
WHILE @@FETCH_STATUS=0
BEGIN
    SET @checkSql=N'IF EXISTS(SELECT 1 FROM dbo.'+QUOTENAME(@checkEntity)+N' x WHERE (x.PublicId IS NULL OR x.PublicId=''00000000-0000-0000-0000-000000000000'') AND NOT EXISTS(SELECT 1 FROM #KnownSeedRows s WHERE s.EntityName=@n AND s.LegacyId=CONVERT(bigint,x.Id))) THROW 51000, ''Empty PublicId found outside the known system-seed manifest.'', 1;';
    EXEC sys.sp_executesql @checkSql,N'@n sysname',@n=@checkEntity;
    FETCH NEXT FROM public_id_check INTO @checkEntity;
END
CLOSE public_id_check; DEALLOCATE public_id_check;

-- Disable the append-only audit trigger only for the backfill transaction.
IF OBJECT_ID(N'dbo.TR_AuditLogs_AppendOnly', N'TR') IS NOT NULL DISABLE TRIGGER dbo.TR_AuditLogs_AppendOnly ON dbo.AuditLogs;

DECLARE @entity sysname, @obj int, @sql nvarchar(max), @idType sysname;
DECLARE entity_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT EntityName FROM @Entities ORDER BY EntityName;
OPEN entity_cursor; FETCH NEXT FROM entity_cursor INTO @entity;
WHILE @@FETCH_STATUS=0
BEGIN
    SELECT @obj=OBJECT_ID(QUOTENAME(N'dbo')+N'.'+QUOTENAME(@entity));
    SELECT @idType=ty.name FROM sys.columns c JOIN sys.types ty ON ty.user_type_id=c.user_type_id WHERE c.object_id=@obj AND c.name=N'Id';
    IF COL_LENGTH(N'dbo.'+@entity,N'__Guid_Id') IS NOT NULL
        THROW 51000, 'Shadow Guid column already exists; manual review required.', 1;
    INSERT dbo.GuidIdentityColumnMap VALUES(@entity,N'Id',NULL,@idType,0,1);
    SELECT @sql=N'ALTER TABLE dbo.'+QUOTENAME(@entity)+N' ADD [__Guid_Id] uniqueidentifier NULL;';
    EXEC sys.sp_executesql @sql;
    IF @entity IN (N'AuditLogs',N'RefreshTokens')
        SET @sql=N'UPDATE dbo.'+QUOTENAME(@entity)+N' SET [__Guid_Id]=NEWID();';
    ELSE IF @entity=N'CustomerRequestStatuses'
        SET @sql=N'UPDATE dbo.CustomerRequestStatuses SET [__Guid_Id]=CASE CONVERT(bigint,[Id]) WHEN 1 THEN ''10000000-0000-4000-8000-000000000001'' WHEN 2 THEN ''10000000-0000-4000-8000-000000000002'' WHEN 3 THEN ''10000000-0000-4000-8000-000000000003'' WHEN 4 THEN ''10000000-0000-4000-8000-000000000004'' ELSE [PublicId] END;';
    ELSE
        SET @sql=N'UPDATE dbo.'+QUOTENAME(@entity)+N' SET [__Guid_Id]=CASE WHEN [PublicId] IS NULL OR [PublicId]=''00000000-0000-0000-0000-000000000000'' THEN (SELECT CONVERT(uniqueidentifier,''00000000-0000-0000-0000-''+RIGHT(REPLICATE(''0'',12)+CONVERT(varchar(12),CONVERT(bigint,[Id])),12)) FROM #KnownSeedRows s WHERE s.EntityName=@n AND s.LegacyId=CONVERT(bigint,[Id])) ELSE [PublicId] END;';
    EXEC sys.sp_executesql @sql,N'@n sysname',@n=@entity;
    SET @sql=N'ALTER TABLE dbo.'+QUOTENAME(@entity)+N' ALTER COLUMN [__Guid_Id] uniqueidentifier NOT NULL;';
    EXEC sys.sp_executesql @sql;
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=@obj AND name=N'PublicId')
    BEGIN
        SET @sql=N'INSERT dbo.GuidIdentityMap(EntityName,LegacyId,NewId,OriginalPublicId) SELECT @n,CONVERT(bigint,[Id]),[__Guid_Id],[PublicId] FROM dbo.'+QUOTENAME(@entity)+N';';
    END
    ELSE
    BEGIN
        SET @sql=N'INSERT dbo.GuidIdentityMap(EntityName,LegacyId,NewId,OriginalPublicId) SELECT @n,CONVERT(bigint,[Id]),[__Guid_Id],NULL FROM dbo.'+QUOTENAME(@entity)+N';';
    END
    EXEC sys.sp_executesql @sql,N'@n sysname',@n=@entity;
    IF EXISTS (SELECT 1 FROM dbo.GuidIdentityMap WHERE EntityName=@entity GROUP BY NewId HAVING COUNT(*)>1)
        THROW 51000, 'PublicId collision found within an entity table; stop before cutover.', 1;
    FETCH NEXT FROM entity_cursor INTO @entity;
END
CLOSE entity_cursor; DEALLOCATE entity_cursor;

-- Shadow and backfill every declared relational FK to an entity Id. Composite and non-Id references fail closed.
IF EXISTS (
    SELECT fkc.constraint_object_id FROM sys.foreign_key_columns fkc
    JOIN sys.foreign_keys fk ON fk.object_id=fkc.constraint_object_id
    JOIN sys.tables rt ON rt.object_id=fk.referenced_object_id
    JOIN @Entities e ON e.EntityName=rt.name
    GROUP BY fkc.constraint_object_id HAVING COUNT(*)<>1
)
    THROW 51000, 'Composite FK to an entity identity is unsupported by this migration manifest.', 1;
DECLARE @parent sysname,@column sysname,@referenced sysname,@nullable bit,@type sysname,@fk sysname,@delete nvarchar(60),@update nvarchar(60),@nfr bit;
DECLARE fk_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT pt.name,pc.name,rt.name,pc.is_nullable,ty.name,fk.name,fk.delete_referential_action_desc,fk.update_referential_action_desc,fk.is_not_for_replication
FROM sys.foreign_key_columns fkc
JOIN sys.foreign_keys fk ON fk.object_id=fkc.constraint_object_id
JOIN sys.tables pt ON pt.object_id=fk.parent_object_id
JOIN sys.columns pc ON pc.object_id=pt.object_id AND pc.column_id=fkc.parent_column_id
JOIN sys.tables rt ON rt.object_id=fk.referenced_object_id
JOIN sys.columns rc ON rc.object_id=rt.object_id AND rc.column_id=fkc.referenced_column_id
JOIN sys.types ty ON ty.user_type_id=pc.user_type_id
JOIN @Entities ep ON ep.EntityName=pt.name
JOIN @Entities er ON er.EntityName=rt.name
WHERE SCHEMA_NAME(pt.schema_id)=N'dbo' AND SCHEMA_NAME(rt.schema_id)=N'dbo' AND rc.name=N'Id';
OPEN fk_cursor; FETCH NEXT FROM fk_cursor INTO @parent,@column,@referenced,@nullable,@type,@fk,@delete,@update,@nfr;
WHILE @@FETCH_STATUS=0
BEGIN
    IF @type NOT IN (N'int',N'bigint') THROW 51000, 'Unsupported FK column type.', 1;
    INSERT dbo.GuidIdentityColumnMap VALUES(@parent,@column,@referenced,@type,@nullable,0);
    INSERT dbo.GuidIdentityForeignKeyMap VALUES(@parent,@fk,@column,@referenced,N'Id',@delete,@update,@nfr);
    SET @sql=N'ALTER TABLE dbo.'+QUOTENAME(@parent)+N' ADD '+QUOTENAME(N'__Guid_'+@column)+N' uniqueidentifier NULL; UPDATE p SET '+QUOTENAME(N'__Guid_'+@column)+N'=m.NewId FROM dbo.'+QUOTENAME(@parent)+N' p LEFT JOIN dbo.GuidIdentityMap m ON m.EntityName=@target AND m.LegacyId=CONVERT(bigint,p.'+QUOTENAME(@column)+N') WHERE p.'+QUOTENAME(@column)+N' IS NOT NULL;';
    EXEC sys.sp_executesql @sql,N'@target sysname',@target=@referenced;
    IF @nullable=0 BEGIN
        SET @sql=N'ALTER TABLE dbo.'+QUOTENAME(@parent)+N' ALTER COLUMN '+QUOTENAME(N'__Guid_'+@column)+N' uniqueidentifier NOT NULL;';
        EXEC sys.sp_executesql @sql;
    END
    SET @sql=N'IF EXISTS(SELECT 1 FROM dbo.'+QUOTENAME(@parent)+N' WHERE '+QUOTENAME(@column)+N' IS NOT NULL AND '+QUOTENAME(N'__Guid_'+@column)+N' IS NULL) THROW 51000, ''Orphan FK value found; stop before cutover.'', 1;';
    EXEC sys.sp_executesql @sql;
    FETCH NEXT FROM fk_cursor INTO @parent,@column,@referenced,@nullable,@type,@fk,@delete,@update,@nfr;
END
CLOSE fk_cursor; DEALLOCATE fk_cursor;

-- Explicit scalar references not represented by SQL FK constraints.
DECLARE scalar_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT t.name,c.name,CASE c.name WHEN N'ActorUserId' THEN N'Users' ELSE N'Accounts' END,c.is_nullable,ty.name
FROM sys.tables t JOIN sys.columns c ON c.object_id=t.object_id JOIN sys.types ty ON ty.user_type_id=c.user_type_id
JOIN @Entities e ON e.EntityName=t.name
WHERE SCHEMA_NAME(t.schema_id)=N'dbo' AND c.name IN (N'CreatedByAccountId',N'LastModifiedByAccountId',N'ActorUserId')
AND ty.name IN (N'int',N'bigint')
AND NOT EXISTS(SELECT 1 FROM dbo.GuidIdentityColumnMap m WHERE m.EntityName=t.name AND m.ColumnName=c.name);
OPEN scalar_cursor; FETCH NEXT FROM scalar_cursor INTO @parent,@column,@referenced,@nullable,@type;
WHILE @@FETCH_STATUS=0
BEGIN
    INSERT dbo.GuidIdentityColumnMap VALUES(@parent,@column,@referenced,@type,@nullable,0);
    SET @sql=N'ALTER TABLE dbo.'+QUOTENAME(@parent)+N' ADD '+QUOTENAME(N'__Guid_'+@column)+N' uniqueidentifier NULL; UPDATE p SET '+QUOTENAME(N'__Guid_'+@column)+N'=m.NewId FROM dbo.'+QUOTENAME(@parent)+N' p LEFT JOIN dbo.GuidIdentityMap m ON m.EntityName=@target AND m.LegacyId=CONVERT(bigint,p.'+QUOTENAME(@column)+N') WHERE p.'+QUOTENAME(@column)+N' IS NOT NULL;';
    EXEC sys.sp_executesql @sql,N'@target sysname',@target=@referenced;
    IF @nullable=0 BEGIN
        SET @sql=N'ALTER TABLE dbo.'+QUOTENAME(@parent)+N' ALTER COLUMN '+QUOTENAME(N'__Guid_'+@column)+N' uniqueidentifier NOT NULL;';
        EXEC sys.sp_executesql @sql;
    END
    SET @sql=N'IF EXISTS(SELECT 1 FROM dbo.'+QUOTENAME(@parent)+N' WHERE '+QUOTENAME(@column)+N' IS NOT NULL AND '+QUOTENAME(N'__Guid_'+@column)+N' IS NULL) THROW 51000, ''Orphan scalar reference found; stop before cutover.'', 1;';
    EXEC sys.sp_executesql @sql;
    FETCH NEXT FROM scalar_cursor INTO @parent,@column,@referenced,@nullable,@type;
END
CLOSE scalar_cursor; DEALLOCATE scalar_cursor;

-- Audit TargetId is polymorphic. Every non-null target must resolve through this explicit class/table map.
IF COL_LENGTH(N'dbo.AuditLogs',N'TargetId') IS NOT NULL
BEGIN
    INSERT dbo.GuidIdentityColumnMap VALUES(N'AuditLogs',N'TargetId',NULL,N'bigint',1,0);
    ALTER TABLE dbo.AuditLogs ADD __Guid_TargetId uniqueidentifier NULL;
    UPDATE a SET __Guid_TargetId=m.NewId
    FROM dbo.AuditLogs a
    JOIN dbo.GuidIdentityMap m ON m.LegacyId=CONVERT(bigint,a.TargetId)
      AND m.EntityName=CASE a.TargetType
        WHEN N'Account' THEN N'Accounts' WHEN N'Company' THEN N'Companies' WHEN N'CompanyModule' THEN N'CompanyModule'
        WHEN N'Conversation' THEN N'Conversations' WHEN N'ConversationParticipant' THEN N'ConversationParticipants'
        WHEN N'Customer' THEN N'Customers' WHEN N'CustomerRequest' THEN N'CustomerRequests'
        WHEN N'CustomerRequestEvaluation' THEN N'CustomerRequestEvaluations' WHEN N'CustomerRequestMessage' THEN N'CustomerRequestMessages'
        WHEN N'CustomerRequestMessageDocument' THEN N'CustomerRequestDocuments' WHEN N'CustomerRequestStatus' THEN N'CustomerRequestStatuses'
        WHEN N'Document' THEN N'Documents' WHEN N'FAQ' THEN N'FAQs' WHEN N'Guide' THEN N'Guides' WHEN N'MenuItem' THEN N'MenuItems'
        WHEN N'Module' THEN N'Modules' WHEN N'ModuleExpert' THEN N'ModuleExperts' WHEN N'OrganizationInfo' THEN N'OrganizationInfos'
        WHEN N'Project' THEN N'Projects' WHEN N'ProjectModule' THEN N'ProjectModule' WHEN N'RequestHistory' THEN N'RequestHistories'
        WHEN N'RequestSubject' THEN N'RequestSubjects' WHEN N'Role' THEN N'Roles' WHEN N'RolePermission' THEN N'RolePermissions'
        WHEN N'RolePermissionAction' THEN N'RolePermissionActions' WHEN N'User' THEN N'Users' WHEN N'UserProject' THEN N'UserProjects'
        WHEN N'UserRole' THEN N'UserRoles' ELSE NULL END
    WHERE a.TargetId IS NOT NULL;
    IF EXISTS(SELECT 1 FROM dbo.AuditLogs WHERE TargetId IS NOT NULL AND __Guid_TargetId IS NULL)
        THROW 51000, 'Unmapped polymorphic AuditLog.TargetId; add its TargetType to the manifest before cutover.', 1;
END

-- User-created statistics, non-default filegroups and partition schemes cannot be round-tripped by this manifest.
IF EXISTS (
    SELECT 1 FROM sys.stats s JOIN sys.tables t ON t.object_id=s.object_id JOIN @Entities e ON e.EntityName=t.name
    JOIN sys.stats_columns sc ON sc.object_id=s.object_id AND sc.stats_id=s.stats_id
    JOIN sys.columns c ON c.object_id=sc.object_id AND c.column_id=sc.column_id
    JOIN dbo.GuidIdentityColumnMap cm ON cm.EntityName=t.name AND cm.ColumnName=c.name
    WHERE s.user_created=1
)
    THROW 51000, 'User-created or NO_RECOMPUTE statistics on migrated identity columns are unsupported.', 1;
IF EXISTS (
    SELECT 1 FROM sys.indexes i JOIN sys.tables t ON t.object_id=i.object_id JOIN @Entities e ON e.EntityName=t.name
    JOIN sys.index_columns xc ON xc.object_id=i.object_id AND xc.index_id=i.index_id
    JOIN sys.columns c ON c.object_id=xc.object_id AND c.column_id=xc.column_id
    JOIN dbo.GuidIdentityColumnMap cm ON cm.EntityName=t.name AND cm.ColumnName=c.name
    JOIN sys.data_spaces ds ON ds.data_space_id=i.data_space_id
    LEFT JOIN sys.filegroups fg ON fg.data_space_id=ds.data_space_id
    WHERE ds.type=N'PS' OR ISNULL(fg.is_default,0)=0 OR i.is_padded=1 OR i.optimize_for_sequential_key=1
)
    THROW 51000, 'Partitioned, non-default-filegroup, PAD_INDEX, or OPTIMIZE_FOR_SEQUENTIAL_KEY index metadata is unsupported.', 1;
-- Persist index definitions before replacing any identity-bearing column.
DECLARE @indexEntity sysname,@indexName sysname,@keys nvarchar(max),@includes nvarchar(max),@filter nvarchar(max),@unique bit,@clustered bit,@isConstraint bit,@schema sysname,@create nvarchar(max),@drop nvarchar(max),@indexObject int,@fillfactor tinyint,@ignoredup bit,@allowrow bit,@allowpage bit;
DECLARE index_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT t.name,i.name,i.object_id,i.is_unique,CONVERT(bit,CASE WHEN i.type=1 THEN 1 ELSE 0 END),i.is_unique_constraint,SCHEMA_NAME(t.schema_id),i.filter_definition,i.fill_factor,i.ignore_dup_key,i.allow_row_locks,i.allow_page_locks
FROM sys.indexes i JOIN sys.tables t ON t.object_id=i.object_id JOIN @Entities e ON e.EntityName=t.name
WHERE i.index_id>0 AND i.is_primary_key=0 AND i.is_hypothetical=0 AND SCHEMA_NAME(t.schema_id)=N'dbo'
AND EXISTS(SELECT 1 FROM sys.index_columns xc JOIN sys.columns cc ON cc.object_id=xc.object_id AND cc.column_id=xc.column_id JOIN dbo.GuidIdentityColumnMap cm ON cm.EntityName=t.name AND cm.ColumnName=cc.name WHERE xc.object_id=i.object_id AND xc.index_id=i.index_id);
OPEN index_cursor; FETCH NEXT FROM index_cursor INTO @indexEntity,@indexName,@indexObject,@unique,@clustered,@isConstraint,@schema,@filter,@fillfactor,@ignoredup,@allowrow,@allowpage;
WHILE @@FETCH_STATUS=0
BEGIN
    SELECT @keys=STRING_AGG(QUOTENAME(c.name)+CASE WHEN ic.is_descending_key=1 THEN N' DESC' ELSE N' ASC' END,N',') WITHIN GROUP(ORDER BY ic.key_ordinal)
    FROM sys.index_columns ic JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
    WHERE ic.object_id=@indexObject AND ic.index_id=(SELECT index_id FROM sys.indexes WHERE object_id=@indexObject AND name=@indexName) AND ic.is_included_column=0;
    SELECT @includes=STRING_AGG(QUOTENAME(c.name),N',') WITHIN GROUP(ORDER BY ic.index_column_id)
    FROM sys.index_columns ic JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
    WHERE ic.object_id=@indexObject AND ic.index_id=(SELECT index_id FROM sys.indexes WHERE object_id=@indexObject AND name=@indexName) AND ic.is_included_column=1;
    IF @isConstraint=1 AND (@filter IS NOT NULL OR @includes IS NOT NULL) THROW 51000, 'Unique constraint has unsupported filtered/include metadata.', 1;
    SET @create=CASE WHEN @isConstraint=1 THEN N'ALTER TABLE '+QUOTENAME(@schema)+N'.'+QUOTENAME(@indexEntity)+N' ADD CONSTRAINT '+QUOTENAME(@indexName)+N' UNIQUE '+CASE WHEN @clustered=1 THEN N'CLUSTERED ' ELSE N'NONCLUSTERED ' END+N'('+@keys+N') WITH (FILLFACTOR='+CONVERT(nvarchar(3),ISNULL(NULLIF(@fillfactor,0),0))+N');'
        ELSE N'CREATE '+CASE WHEN @unique=1 THEN N'UNIQUE ' ELSE N'' END+CASE WHEN @clustered=1 THEN N'CLUSTERED ' ELSE N'NONCLUSTERED ' END+N'INDEX '+QUOTENAME(@indexName)+N' ON '+QUOTENAME(@schema)+N'.'+QUOTENAME(@indexEntity)+N' ('+@keys+N')'+CASE WHEN @includes IS NULL THEN N'' ELSE N' INCLUDE ('+@includes+N')' END+CASE WHEN @filter IS NULL THEN N'' ELSE N' WHERE '+@filter END+N' WITH (FILLFACTOR='+CONVERT(nvarchar(3),ISNULL(NULLIF(@fillfactor,0),0))+N', IGNORE_DUP_KEY='+CASE WHEN @ignoredup=1 THEN N'ON' ELSE N'OFF' END+N', ALLOW_ROW_LOCKS='+CASE WHEN @allowrow=1 THEN N'ON' ELSE N'OFF' END+N', ALLOW_PAGE_LOCKS='+CASE WHEN @allowpage=1 THEN N'ON' ELSE N'OFF' END+N');' END;
    SET @drop=CASE WHEN @isConstraint=1 THEN N'ALTER TABLE '+QUOTENAME(@schema)+N'.'+QUOTENAME(@indexEntity)+N' DROP CONSTRAINT '+QUOTENAME(@indexName)+N';' ELSE N'DROP INDEX '+QUOTENAME(@indexName)+N' ON '+QUOTENAME(@schema)+N'.'+QUOTENAME(@indexEntity)+N';' END;
    INSERT dbo.GuidIdentityIndexMap VALUES(@indexEntity,@indexName,@drop,@create);
    FETCH NEXT FROM index_cursor INTO @indexEntity,@indexName,@indexObject,@unique,@clustered,@isConstraint,@schema,@filter,@fillfactor,@ignoredup,@allowrow,@allowpage;
END
CLOSE index_cursor; DEALLOCATE index_cursor;

-- All legacy FK metadata is now durable. Drop FK constraints before dropping their numeric columns.
DECLARE @constraint sysname,@parentColumn sysname,@refColumn sysname,@parentObject int;
DECLARE drop_fk CURSOR LOCAL FAST_FORWARD FOR SELECT ParentEntityName,ConstraintName FROM dbo.GuidIdentityForeignKeyMap;
OPEN drop_fk; FETCH NEXT FROM drop_fk INTO @parent,@constraint;
WHILE @@FETCH_STATUS=0
BEGIN
    SET @sql=N'ALTER TABLE dbo.'+QUOTENAME(@parent)+N' DROP CONSTRAINT '+QUOTENAME(@constraint)+N';'; EXEC sys.sp_executesql @sql;
    FETCH NEXT FROM drop_fk INTO @parent,@constraint;
END
CLOSE drop_fk; DEALLOCATE drop_fk;

-- Save and drop every simple Id primary key.
INSERT dbo.GuidIdentityPrimaryKeyMap(EntityName,ConstraintName,IndexType)
SELECT t.name,k.name,i.type_desc FROM sys.key_constraints k JOIN sys.tables t ON t.object_id=k.parent_object_id
JOIN sys.indexes i ON i.object_id=k.parent_object_id AND i.index_id=k.unique_index_id
JOIN @Entities e ON e.EntityName=t.name WHERE k.type=N'PK' AND SCHEMA_NAME(t.schema_id)=N'dbo'
AND (SELECT COUNT(*) FROM sys.index_columns ic WHERE ic.object_id=i.object_id AND ic.index_id=i.index_id AND ic.key_ordinal>0)=1
AND EXISTS(SELECT 1 FROM sys.index_columns ic JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id WHERE ic.object_id=i.object_id AND ic.index_id=i.index_id AND c.name=N'Id');
IF (SELECT COUNT(*) FROM dbo.GuidIdentityPrimaryKeyMap)<>(SELECT COUNT(*) FROM @Entities)
    THROW 51000, 'Expected single-column Id primary keys were not found for all entity tables.', 1;
DECLARE pk_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT EntityName,ConstraintName FROM dbo.GuidIdentityPrimaryKeyMap;
OPEN pk_cursor; FETCH NEXT FROM pk_cursor INTO @entity,@constraint;
WHILE @@FETCH_STATUS=0 BEGIN SET @sql=N'ALTER TABLE dbo.'+QUOTENAME(@entity)+N' DROP CONSTRAINT '+QUOTENAME(@constraint)+N';'; EXEC sys.sp_executesql @sql; FETCH NEXT FROM pk_cursor INTO @entity,@constraint; END
CLOSE pk_cursor; DEALLOCATE pk_cursor;

DECLARE index_drop CURSOR LOCAL FAST_FORWARD FOR SELECT DropSql FROM dbo.GuidIdentityIndexMap;
OPEN index_drop; FETCH NEXT FROM index_drop INTO @drop;
WHILE @@FETCH_STATUS=0 BEGIN EXEC sys.sp_executesql @drop; FETCH NEXT FROM index_drop INTO @drop; END
CLOSE index_drop; DEALLOCATE index_drop;

-- Replace each old key/reference column with its backfilled Guid shadow. Identity PK columns are dropped, never altered.
DECLARE @col sysname,@shadow sysname,@oldname sysname;
DECLARE column_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT EntityName,ColumnName FROM dbo.GuidIdentityColumnMap ORDER BY IsPrimaryKey DESC,EntityName,ColumnName;
OPEN column_cursor; FETCH NEXT FROM column_cursor INTO @entity,@column;
WHILE @@FETCH_STATUS=0
BEGIN
    SET @shadow=N'__Guid_'+@column;
    SET @sql=N'ALTER TABLE dbo.'+QUOTENAME(@entity)+N' DROP COLUMN '+QUOTENAME(@column)+N'; EXEC sys.sp_rename N''dbo.'+REPLACE(@entity,'''','''''')+N'.'+REPLACE(@shadow,'''','''''')+N''',N'''+REPLACE(@column,'''','''''')+N''',N''COLUMN'';';
    EXEC sys.sp_executesql @sql;
    FETCH NEXT FROM column_cursor INTO @entity,@column;
END
CLOSE column_cursor; DEALLOCATE column_cursor;

-- Restore Guid PKs and relational constraints from their captured metadata.
DECLARE pk_add CURSOR LOCAL FAST_FORWARD FOR SELECT EntityName,ConstraintName,IndexType FROM dbo.GuidIdentityPrimaryKeyMap;
OPEN pk_add; FETCH NEXT FROM pk_add INTO @entity,@constraint,@type;
WHILE @@FETCH_STATUS=0
BEGIN
    SET @sql=N'ALTER TABLE dbo.'+QUOTENAME(@entity)+N' ADD CONSTRAINT '+QUOTENAME(@constraint)+N' PRIMARY KEY '+CASE WHEN @type=N'CLUSTERED' THEN N'CLUSTERED' ELSE N'NONCLUSTERED' END+N' ([Id]);';
    EXEC sys.sp_executesql @sql;
    FETCH NEXT FROM pk_add INTO @entity,@constraint,@type;
END
CLOSE pk_add; DEALLOCATE pk_add;
DECLARE index_add CURSOR LOCAL FAST_FORWARD FOR SELECT CreateSql FROM dbo.GuidIdentityIndexMap;
OPEN index_add; FETCH NEXT FROM index_add INTO @create;
WHILE @@FETCH_STATUS=0 BEGIN EXEC sys.sp_executesql @create; FETCH NEXT FROM index_add INTO @create; END
CLOSE index_add; DEALLOCATE index_add;
DECLARE fk_add CURSOR LOCAL FAST_FORWARD FOR
SELECT ParentEntityName,ConstraintName,ParentColumnName,ReferencedEntityName,ReferencedColumnName,DeleteAction,UpdateAction,IsNotForReplication FROM dbo.GuidIdentityForeignKeyMap;
OPEN fk_add; FETCH NEXT FROM fk_add INTO @parent,@fk,@column,@referenced,@refColumn,@delete,@update,@nfr;
WHILE @@FETCH_STATUS=0
BEGIN
    SET @sql=N'ALTER TABLE dbo.'+QUOTENAME(@parent)+N' WITH CHECK ADD CONSTRAINT '+QUOTENAME(@fk)+N' FOREIGN KEY ('+QUOTENAME(@column)+N') REFERENCES dbo.'+QUOTENAME(@referenced)+N' ('+QUOTENAME(@refColumn)+N')'+CASE WHEN @delete=N'CASCADE' THEN N' ON DELETE CASCADE' WHEN @delete=N'SET_NULL' THEN N' ON DELETE SET NULL' WHEN @delete=N'SET_DEFAULT' THEN N' ON DELETE SET DEFAULT' ELSE N'' END+CASE WHEN @update=N'CASCADE' THEN N' ON UPDATE CASCADE' WHEN @update=N'SET_NULL' THEN N' ON UPDATE SET NULL' WHEN @update=N'SET_DEFAULT' THEN N' ON UPDATE SET DEFAULT' ELSE N'' END+CASE WHEN @nfr=1 THEN N' NOT FOR REPLICATION' ELSE N'' END+N'; ALTER TABLE dbo.'+QUOTENAME(@parent)+N' CHECK CONSTRAINT '+QUOTENAME(@fk)+N';';
    EXEC sys.sp_executesql @sql;
    FETCH NEXT FROM fk_add INTO @parent,@fk,@column,@referenced,@refColumn,@delete,@update,@nfr;
END
CLOSE fk_add; DEALLOCATE fk_add;

-- The single-Guid/PublicId alias is preserved; statuses are the sole canonical-ID exception.
UPDATE e SET PublicId=Id FROM dbo.Accounts e;
UPDATE e SET PublicId=Id FROM dbo.Companies e;
UPDATE e SET PublicId=Id FROM dbo.CompanyModule e;
UPDATE e SET PublicId=Id FROM dbo.ConversationParticipants e;
UPDATE e SET PublicId=Id FROM dbo.Conversations e;
UPDATE e SET PublicId=Id FROM dbo.CustomerRequestDocuments e;
UPDATE e SET PublicId=Id FROM dbo.CustomerRequestEvaluations e;
UPDATE e SET PublicId=Id FROM dbo.CustomerRequestMessages e;
UPDATE e SET PublicId=Id FROM dbo.CustomerRequests e;
UPDATE e SET PublicId=Id FROM dbo.CustomerRequestStatuses e;
UPDATE e SET PublicId=Id FROM dbo.Customers e;
UPDATE e SET PublicId=Id FROM dbo.Documents e;
UPDATE e SET PublicId=Id FROM dbo.FAQs e;
UPDATE e SET PublicId=Id FROM dbo.Guides e;
UPDATE e SET PublicId=Id FROM dbo.MenuItems e;
UPDATE e SET PublicId=Id FROM dbo.ModuleExperts e;
UPDATE e SET PublicId=Id FROM dbo.Modules e;
UPDATE e SET PublicId=Id FROM dbo.OrganizationInfos e;
UPDATE e SET PublicId=Id FROM dbo.ProjectModule e;
UPDATE e SET PublicId=Id FROM dbo.Projects e;
UPDATE e SET PublicId=Id FROM dbo.RequestHistories e;
UPDATE e SET PublicId=Id FROM dbo.RequestSubjects e;
UPDATE e SET PublicId=Id FROM dbo.RolePermissionActions e;
UPDATE e SET PublicId=Id FROM dbo.RolePermissions e;
UPDATE e SET PublicId=Id FROM dbo.Roles e;
UPDATE e SET PublicId=Id FROM dbo.UserProjects e;
UPDATE e SET PublicId=Id FROM dbo.UserRoles e;
UPDATE e SET PublicId=Id FROM dbo.Users e;
UPDATE dbo.RequestHistories SET OldValue='10000000-0000-4000-8000-000000000001' WHERE Action=2 AND OldValue='1';
UPDATE dbo.RequestHistories SET OldValue='10000000-0000-4000-8000-000000000002' WHERE Action=2 AND OldValue='2';
UPDATE dbo.RequestHistories SET OldValue='10000000-0000-4000-8000-000000000003' WHERE Action=2 AND OldValue='3';
UPDATE dbo.RequestHistories SET OldValue='10000000-0000-4000-8000-000000000004' WHERE Action=2 AND OldValue='4';
UPDATE dbo.RequestHistories SET NewValue='10000000-0000-4000-8000-000000000001' WHERE Action=2 AND NewValue='1';
UPDATE dbo.RequestHistories SET NewValue='10000000-0000-4000-8000-000000000002' WHERE Action=2 AND NewValue='2';
UPDATE dbo.RequestHistories SET NewValue='10000000-0000-4000-8000-000000000003' WHERE Action=2 AND NewValue='3';
UPDATE dbo.RequestHistories SET NewValue='10000000-0000-4000-8000-000000000004' WHERE Action=2 AND NewValue='4';
IF OBJECT_ID(N'dbo.TR_AuditLogs_AppendOnly', N'TR') IS NOT NULL ENABLE TRIGGER dbo.TR_AuditLogs_AppendOnly ON dbo.AuditLogs;
""";
}
