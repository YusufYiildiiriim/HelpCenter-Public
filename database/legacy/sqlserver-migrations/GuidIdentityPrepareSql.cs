namespace HelpCenter.Persistence.Migrations;

/// <summary>
/// Read-only identity and reference staging for the later Guid cutover. This SQL only creates
/// migration-owned staging objects; it does not change entity rows, keys, columns, or constraints.
/// </summary>
internal static class GuidIdentityPrepareSql
{
    internal const string Down = "DROP TABLE IF EXISTS dbo.GuidIdentityPrepareModuleDependency; DROP TABLE IF EXISTS dbo.GuidIdentityPrepareTextReferenceMap; DROP TABLE IF EXISTS dbo.GuidIdentityPrepareForeignKeyInventory; DROP TABLE IF EXISTS dbo.GuidIdentityPrepareReferenceMap; DROP TABLE IF EXISTS dbo.GuidIdentityPrepareRowCounts; DROP TABLE IF EXISTS dbo.GuidIdentityPrepareMap;";

    internal const string Up = """
SET XACT_ABORT ON;
IF ISNULL(TRY_CONVERT(int,SERVERPROPERTY('ProductMajorVersion')),0)<15
    THROW 51000, 'GUID identity migration requires SQL Server 2019 (15.x) or later.', 1;
IF EXISTS(SELECT 1 FROM sys.objects WHERE type IN(N'PC',N'FS',N'FT',N'AF',N'TA'))
    THROW 51000, 'SQL CLR modules exist in this database; GUID identity migration is unsupported until they are removed or separately reviewed.', 1;
-- Take locks in stable order so all prepared source rows represent one write-frozen snapshot.
DECLARE @lockEntity sysname,@lockSql nvarchar(max),@lockCursor CURSOR;
DECLARE @LockEntities table(EntityName sysname PRIMARY KEY);
INSERT @LockEntities VALUES
(N'Accounts'),(N'Companies'),(N'CompanyModule'),(N'ConversationParticipants'),(N'Conversations'),
(N'CustomerRequestDocuments'),(N'CustomerRequestEvaluations'),(N'CustomerRequestMessages'),(N'CustomerRequests'),
(N'CustomerRequestStatuses'),(N'Customers'),(N'Documents'),(N'FAQs'),(N'Guides'),(N'MenuItems'),
(N'ModuleExperts'),(N'Modules'),(N'OrganizationInfos'),(N'ProjectModule'),(N'Projects'),(N'RequestHistories'),
(N'RequestSubjects'),(N'RolePermissionActions'),(N'RolePermissions'),(N'Roles'),(N'UserProjects'),(N'UserRoles'),
(N'Users'),(N'AuditLogs'),(N'RefreshTokens');
SET @lockCursor=CURSOR LOCAL FAST_FORWARD FOR SELECT EntityName FROM @LockEntities ORDER BY EntityName;
OPEN @lockCursor; FETCH NEXT FROM @lockCursor INTO @lockEntity;
WHILE @@FETCH_STATUS=0
BEGIN
    SET @lockSql=N'SELECT COUNT_BIG(*) FROM dbo.'+QUOTENAME(@lockEntity)+N' WITH(TABLOCKX,HOLDLOCK);';
    EXEC(@lockSql);
    FETCH NEXT FROM @lockCursor INTO @lockEntity;
END;
CLOSE @lockCursor; DEALLOCATE @lockCursor;
IF OBJECT_ID(N'dbo.GuidIdentityPrepareMap',N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.GuidIdentityPrepareRowCounts',N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.GuidIdentityPrepareReferenceMap',N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.GuidIdentityPrepareTextReferenceMap',N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.GuidIdentityPrepareForeignKeyInventory',N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.GuidIdentityPrepareModuleDependency',N'U') IS NOT NULL
    THROW 51000, 'Guid identity preparation artifacts already exist; inspect the database before retrying.', 1;

CREATE TABLE dbo.GuidIdentityPrepareMap
(
    EntityName sysname NOT NULL,
    LegacyId bigint NOT NULL,
    GuidId uniqueidentifier NOT NULL,
    OriginalPublicId uniqueidentifier NULL,
    CONSTRAINT PK_GuidIdentityPrepareMap PRIMARY KEY(EntityName,LegacyId),
    CONSTRAINT UQ_GuidIdentityPrepareMap_GuidId UNIQUE(EntityName,GuidId)
);
CREATE TABLE dbo.GuidIdentityPrepareRowCounts
(
    EntityName sysname NOT NULL PRIMARY KEY,
    SourceRowCount bigint NOT NULL,
    PreparedRowCount bigint NOT NULL,
    PreparedAtUtc datetime2(7) NOT NULL CONSTRAINT DF_GuidIdentityPrepareRowCounts_PreparedAtUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_GuidIdentityPrepareRowCounts_Parity CHECK(SourceRowCount=PreparedRowCount)
);
CREATE TABLE dbo.GuidIdentityPrepareReferenceMap
(
    ParentEntityName sysname NOT NULL,
    ParentLegacyId bigint NOT NULL,
    ColumnName sysname NOT NULL,
    ReferencedEntityName sysname NOT NULL,
    ReferencedLegacyId bigint NOT NULL,
    GuidId uniqueidentifier NOT NULL,
    CONSTRAINT PK_GuidIdentityPrepareReferenceMap PRIMARY KEY(ParentEntityName,ParentLegacyId,ColumnName)
);
CREATE TABLE dbo.GuidIdentityPrepareTextReferenceMap
(
    ParentLegacyId bigint NOT NULL,
    ColumnName sysname NOT NULL,
    OriginalValue nvarchar(max) NOT NULL,
    ReferencedEntityName sysname NOT NULL,
    ReferencedLegacyId bigint NULL,
    GuidId uniqueidentifier NOT NULL,
    CONSTRAINT PK_GuidIdentityPrepareTextReferenceMap PRIMARY KEY(ParentLegacyId,ColumnName)
);
CREATE TABLE dbo.GuidIdentityPrepareForeignKeyInventory
(
    ParentSchema sysname NOT NULL,
    ParentEntityName sysname NOT NULL,
    ConstraintName sysname NOT NULL,
    ParentColumnName sysname NOT NULL,
    ParentSqlType sysname NOT NULL,
    ReferencedSchema sysname NOT NULL,
    ReferencedEntityName sysname NOT NULL,
    ReferencedColumnName sysname NOT NULL,
    ReferencedSqlType sysname NOT NULL,
    DeleteAction nvarchar(60) NOT NULL,
    UpdateAction nvarchar(60) NOT NULL,
    IsDisabled bit NOT NULL,
    IsNotTrusted bit NOT NULL,
    IsNotForReplication bit NOT NULL,
    Disposition varchar(24) NOT NULL,
    CONSTRAINT PK_GuidIdentityPrepareForeignKeyInventory PRIMARY KEY(ParentSchema,ParentEntityName,ConstraintName,ParentColumnName,ReferencedColumnName),
    CONSTRAINT CK_GuidIdentityPrepareForeignKeyInventory_Disposition CHECK(Disposition IN('GuidIdentity','UnchangedExternal'))
);
CREATE TABLE dbo.GuidIdentityPrepareModuleDependency
(
    ReferencingSchema sysname NOT NULL,
    ReferencingObject sysname NOT NULL,
    ReferencingType nvarchar(60) NOT NULL,
    ReferencedEntityName sysname NOT NULL,
    ReferencedColumnName sysname NOT NULL,
    DefinitionHash varbinary(32) NULL,
    TriggerMetadata nvarchar(max) NULL,
    DiscoveryMethod varchar(24) NOT NULL,
    Disposition varchar(32) NOT NULL,
    CONSTRAINT PK_GuidIdentityPrepareModuleDependency PRIMARY KEY(ReferencingSchema,ReferencingObject,ReferencedEntityName,ReferencedColumnName,DiscoveryMethod)
);

DECLARE @Entities table(EntityName sysname PRIMARY KEY, HasPublicId bit NOT NULL);
INSERT @Entities(EntityName,HasPublicId) VALUES
(N'Accounts',1),(N'Companies',1),(N'CompanyModule',1),(N'ConversationParticipants',1),(N'Conversations',1),
(N'CustomerRequestDocuments',1),(N'CustomerRequestEvaluations',1),(N'CustomerRequestMessages',1),(N'CustomerRequests',1),
(N'CustomerRequestStatuses',1),(N'Customers',1),(N'Documents',1),(N'FAQs',1),(N'Guides',1),(N'MenuItems',1),
(N'ModuleExperts',1),(N'Modules',1),(N'OrganizationInfos',1),(N'ProjectModule',1),(N'Projects',1),(N'RequestHistories',1),
(N'RequestSubjects',1),(N'RolePermissionActions',1),(N'RolePermissions',1),(N'Roles',1),(N'UserProjects',1),(N'UserRoles',1),
(N'Users',1),(N'AuditLogs',0),(N'RefreshTokens',0);

IF EXISTS(SELECT 1 FROM @Entities e LEFT JOIN sys.tables t ON t.name=e.EntityName AND SCHEMA_NAME(t.schema_id)=N'dbo' WHERE t.object_id IS NULL)
    THROW 51000, 'Guid identity preparation found a missing entity table.', 1;
IF EXISTS
(
    SELECT 1 FROM sys.synonyms s CROSS JOIN @Entities e
    WHERE RIGHT(REPLACE(REPLACE(s.base_object_name,N'[',N''),N']',N''),LEN(e.EntityName))=e.EntityName
)
   OR EXISTS
(
    SELECT 1 FROM sys.sql_expression_dependencies sed JOIN sys.synonyms s ON s.object_id=sed.referenced_id
    CROSS JOIN @Entities e
    WHERE RIGHT(REPLACE(REPLACE(s.base_object_name,N'[',N''),N']',N''),LEN(e.EntityName))=e.EntityName
)
    THROW 51000, 'A SQL synonym or dependent module targets a converted entity; synonym cutover is unsupported.', 1;
IF EXISTS(SELECT 1 FROM @Entities e JOIN sys.tables t ON t.name=e.EntityName AND SCHEMA_NAME(t.schema_id)=N'dbo'
          LEFT JOIN sys.columns c ON c.object_id=t.object_id AND c.name=N'Id'
          LEFT JOIN sys.types ty ON ty.user_type_id=c.user_type_id
          WHERE c.column_id IS NULL OR ty.name NOT IN(N'int',N'bigint'))
    THROW 51000, 'Guid identity preparation requires every legacy Id to be int or bigint.', 1;
IF EXISTS(SELECT 1 FROM @Entities e JOIN sys.tables t ON t.name=e.EntityName AND SCHEMA_NAME(t.schema_id)=N'dbo'
          WHERE e.HasPublicId=1 AND NOT EXISTS(SELECT 1 FROM sys.columns c WHERE c.object_id=t.object_id AND c.name=N'PublicId' AND c.user_type_id=TYPE_ID(N'uniqueidentifier')))
    THROW 51000, 'Guid identity preparation found a BaseEntity table without a uniqueidentifier PublicId.', 1;

-- Existing non-empty PublicId values are the deterministic target IDs. Known fixed system rows
-- with empty PublicId use their stable numeric seed key; every other empty value stops safely.
CREATE TABLE #KnownEmptySeed(EntityName sysname NOT NULL, LegacyId bigint NOT NULL, PRIMARY KEY(EntityName,LegacyId));
INSERT #KnownEmptySeed(EntityName,LegacyId) VALUES
(N'Accounts',1),(N'MenuItems',1),(N'MenuItems',2),(N'MenuItems',3),(N'MenuItems',4),(N'MenuItems',5),(N'MenuItems',6),(N'MenuItems',7),(N'MenuItems',8),(N'MenuItems',10),(N'MenuItems',11),(N'MenuItems',12),(N'MenuItems',13),(N'MenuItems',14),(N'MenuItems',15),
(N'OrganizationInfos',1),(N'RolePermissionActions',1),(N'RolePermissionActions',2),(N'RolePermissionActions',3),(N'RolePermissionActions',4),(N'RolePermissionActions',5),(N'RolePermissionActions',6),(N'RolePermissionActions',7),(N'RolePermissionActions',8),(N'RolePermissionActions',9),(N'RolePermissionActions',10),(N'RolePermissionActions',11),(N'RolePermissionActions',12),(N'RolePermissionActions',13),(N'RolePermissionActions',14),(N'RolePermissionActions',15),(N'RolePermissionActions',16),(N'RolePermissionActions',17),(N'RolePermissionActions',18),(N'RolePermissionActions',19),(N'RolePermissionActions',20),(N'RolePermissionActions',21),(N'RolePermissionActions',22),(N'RolePermissionActions',23),(N'RolePermissionActions',24),(N'RolePermissionActions',25),(N'RolePermissionActions',26),(N'RolePermissionActions',27),(N'RolePermissionActions',50),(N'RolePermissionActions',51),(N'RolePermissionActions',52),(N'RolePermissionActions',53),(N'RolePermissionActions',54),(N'RolePermissionActions',70),(N'RolePermissionActions',71),(N'RolePermissionActions',72),(N'RolePermissionActions',80),(N'RolePermissionActions',81),
(N'RolePermissions',1),(N'RolePermissions',2),(N'RolePermissions',3),(N'RolePermissions',4),(N'RolePermissions',5),(N'RolePermissions',6),(N'RolePermissions',8),(N'RolePermissions',9),(N'RolePermissions',10),(N'RolePermissions',11),(N'RolePermissions',12),(N'RolePermissions',13),(N'RolePermissions',14),(N'RolePermissions',15),(N'RolePermissions',16),(N'RolePermissions',20),(N'RolePermissions',21),(N'RolePermissions',22),(N'RolePermissions',23),(N'RolePermissions',30),(N'RolePermissions',40),
(N'Roles',1),(N'Roles',2),(N'Roles',3),(N'Roles',4),(N'UserRoles',1),(N'Users',1),(N'CustomerRequestStatuses',1),(N'CustomerRequestStatuses',2),(N'CustomerRequestStatuses',3),(N'CustomerRequestStatuses',4);

DECLARE @entity sysname,@hasPublicId bit,@sql nvarchar(max);
DECLARE entity_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT EntityName,HasPublicId FROM @Entities ORDER BY EntityName;
OPEN entity_cursor; FETCH NEXT FROM entity_cursor INTO @entity,@hasPublicId;
WHILE @@FETCH_STATUS=0
BEGIN
    IF @hasPublicId=1
    BEGIN
        SET @sql=N'IF EXISTS(SELECT 1 FROM dbo.'+QUOTENAME(@entity)+N' x WHERE (x.PublicId IS NULL OR x.PublicId=''00000000-0000-0000-0000-000000000000'') AND NOT EXISTS(SELECT 1 FROM #KnownEmptySeed k WHERE k.EntityName=@n AND k.LegacyId=CONVERT(bigint,x.Id))) THROW 51000, ''Unexpected empty PublicId; refusing to invent an identity.'', 1;';
        EXEC sys.sp_executesql @sql,N'@n sysname',@n=@entity;
        IF @entity=N'CustomerRequestStatuses'
            SET @sql=N'INSERT dbo.GuidIdentityPrepareMap(EntityName,LegacyId,GuidId,OriginalPublicId) SELECT @n,CONVERT(bigint,Id),CASE CONVERT(bigint,Id) WHEN 1 THEN ''10000000-0000-4000-8000-000000000001'' WHEN 2 THEN ''10000000-0000-4000-8000-000000000002'' WHEN 3 THEN ''10000000-0000-4000-8000-000000000003'' WHEN 4 THEN ''10000000-0000-4000-8000-000000000004'' ELSE PublicId END,PublicId FROM dbo.'+QUOTENAME(@entity)+N';';
        ELSE
            SET @sql=N'INSERT dbo.GuidIdentityPrepareMap(EntityName,LegacyId,GuidId,OriginalPublicId) SELECT @n,CONVERT(bigint,x.Id),CASE WHEN x.PublicId IS NULL OR x.PublicId=''00000000-0000-0000-0000-000000000000'' THEN CONVERT(uniqueidentifier,''00000000-0000-0000-0000-''+RIGHT(REPLICATE(''0'',12)+CONVERT(varchar(12),CONVERT(bigint,x.Id)),12)) ELSE x.PublicId END,x.PublicId FROM dbo.'+QUOTENAME(@entity)+N' x;';
        EXEC sys.sp_executesql @sql,N'@n sysname',@n=@entity;
    END
    ELSE
    BEGIN
        SET @sql=N'INSERT dbo.GuidIdentityPrepareMap(EntityName,LegacyId,GuidId,OriginalPublicId) SELECT @n,CONVERT(bigint,Id),CONVERT(uniqueidentifier,SUBSTRING(HASHBYTES(''SHA2_256'',CONVERT(varbinary(max),CONCAT(@n,N'':'' ,CONVERT(varchar(20),CONVERT(bigint,Id))))),1,16)),NULL FROM dbo.'+QUOTENAME(@entity)+N';';
        EXEC sys.sp_executesql @sql,N'@n sysname',@n=@entity;
    END;
    IF EXISTS(SELECT 1 FROM dbo.GuidIdentityPrepareMap WHERE EntityName=@entity GROUP BY GuidId HAVING COUNT(*)>1)
        THROW 51000, 'Guid identity preparation found a non-bijective Guid mapping.', 1;
    SET @sql=N'INSERT dbo.GuidIdentityPrepareRowCounts(EntityName,SourceRowCount,PreparedRowCount) SELECT @n,(SELECT COUNT_BIG(*) FROM dbo.'+QUOTENAME(@entity)+N'),(SELECT COUNT_BIG(*) FROM dbo.GuidIdentityPrepareMap WHERE EntityName=@n);';
    EXEC sys.sp_executesql @sql,N'@n sysname',@n=@entity;
    FETCH NEXT FROM entity_cursor INTO @entity,@hasPublicId;
END;
CLOSE entity_cursor; DEALLOCATE entity_cursor;

-- Classify every FK touching a converted entity. The finalizer changes only Id-target relationships;
-- alternate-key/PublicId targets are conservatively rejected until explicitly proven unchanged.
IF EXISTS(
    SELECT 1 FROM sys.foreign_key_columns fkc JOIN sys.foreign_keys fk ON fk.object_id=fkc.constraint_object_id
    JOIN sys.tables rt ON rt.object_id=fk.referenced_object_id JOIN @Entities re ON re.EntityName=rt.name
    JOIN sys.columns rc ON rc.object_id=rt.object_id AND rc.column_id=fkc.referenced_column_id
    WHERE rc.name<>N'Id')
    THROW 51000, 'A foreign key targets a non-Id key on a converted entity; alternate/PublicId FK support is not proven.', 1;
IF EXISTS(
    SELECT 1 FROM sys.foreign_key_columns fkc JOIN sys.foreign_keys fk ON fk.object_id=fkc.constraint_object_id
    JOIN sys.tables rt ON rt.object_id=fk.referenced_object_id JOIN @Entities re ON re.EntityName=rt.name
    JOIN sys.foreign_key_columns allcols ON allcols.constraint_object_id=fkc.constraint_object_id
    GROUP BY fkc.constraint_object_id HAVING COUNT(DISTINCT allcols.constraint_column_id)>1)
    THROW 51000, 'A composite FK touches a converted entity key; explicit support is required.', 1;
IF EXISTS(
    SELECT 1 FROM sys.foreign_key_columns fkc JOIN sys.foreign_keys fk ON fk.object_id=fkc.constraint_object_id
    JOIN sys.tables rt ON rt.object_id=fk.referenced_object_id JOIN @Entities re ON re.EntityName=rt.name
    JOIN sys.tables pt ON pt.object_id=fk.parent_object_id LEFT JOIN @Entities pe ON pe.EntityName=pt.name
    WHERE pe.EntityName IS NULL)
    THROW 51000, 'An external parent FK references a converted entity; it cannot be safely rebuilt by this cutover.', 1;
IF EXISTS(
    SELECT 1 FROM sys.foreign_key_columns fkc JOIN sys.foreign_keys fk ON fk.object_id=fkc.constraint_object_id
    JOIN sys.tables pt ON pt.object_id=fk.parent_object_id JOIN @Entities pe ON pe.EntityName=pt.name
    JOIN sys.columns pc ON pc.object_id=pt.object_id AND pc.column_id=fkc.parent_column_id
    WHERE pc.name=N'Id')
    THROW 51000, 'A converted entity primary Id is also an FK parent column; this shape is unsupported.', 1;
IF EXISTS(
    SELECT 1 FROM sys.foreign_key_columns fkc JOIN sys.foreign_keys fk ON fk.object_id=fkc.constraint_object_id
    JOIN sys.tables pt ON pt.object_id=fk.parent_object_id JOIN @Entities pe ON pe.EntityName=pt.name
    JOIN sys.tables rt ON rt.object_id=fk.referenced_object_id LEFT JOIN @Entities re ON re.EntityName=rt.name
    JOIN sys.columns pc ON pc.object_id=pt.object_id AND pc.column_id=fkc.parent_column_id
    JOIN sys.types pty ON pty.user_type_id=pc.user_type_id
    JOIN sys.columns rc ON rc.object_id=rt.object_id AND rc.column_id=fkc.referenced_column_id
    WHERE re.EntityName IS NOT NULL AND (SCHEMA_NAME(pt.schema_id)<>N'dbo' OR SCHEMA_NAME(rt.schema_id)<>N'dbo')
       OR (re.EntityName IS NOT NULL AND rc.name=N'Id' AND pty.name NOT IN(N'int',N'bigint')))
    THROW 51000, 'An identity FK is outside dbo or does not use a supported numeric parent column.', 1;
IF EXISTS(
    SELECT 1 FROM sys.foreign_key_columns fkc JOIN sys.foreign_keys fk ON fk.object_id=fkc.constraint_object_id
    JOIN sys.tables pt ON pt.object_id=fk.parent_object_id JOIN @Entities pe ON pe.EntityName=pt.name
    JOIN sys.columns pc ON pc.object_id=pt.object_id AND pc.column_id=fkc.parent_column_id
    JOIN sys.types pty ON pty.user_type_id=pc.user_type_id
    JOIN sys.tables rt ON rt.object_id=fk.referenced_object_id LEFT JOIN @Entities re ON re.EntityName=rt.name
    WHERE re.EntityName IS NULL AND (pc.name=N'Id' OR pc.name LIKE N'%Id') AND pty.system_type_id IN(48,52,56,127,106,108,60,122,59,62,104,98))
    THROW 51000, 'A converted entity has a numeric Id-like FK to a table outside the GUID manifest.', 1;

INSERT dbo.GuidIdentityPrepareForeignKeyInventory(ParentSchema,ParentEntityName,ConstraintName,ParentColumnName,ParentSqlType,ReferencedSchema,ReferencedEntityName,ReferencedColumnName,ReferencedSqlType,DeleteAction,UpdateAction,IsDisabled,IsNotTrusted,IsNotForReplication,Disposition)
SELECT SCHEMA_NAME(pt.schema_id),pt.name,fk.name,pc.name,pty.name,SCHEMA_NAME(rt.schema_id),rt.name,rc.name,rty.name,
       fk.delete_referential_action_desc,fk.update_referential_action_desc,fk.is_disabled,fk.is_not_trusted,fk.is_not_for_replication,
       CASE WHEN re.EntityName IS NOT NULL AND rc.name=N'Id' THEN 'GuidIdentity' ELSE 'UnchangedExternal' END
FROM sys.foreign_key_columns fkc JOIN sys.foreign_keys fk ON fk.object_id=fkc.constraint_object_id
JOIN sys.tables pt ON pt.object_id=fk.parent_object_id JOIN sys.columns pc ON pc.object_id=pt.object_id AND pc.column_id=fkc.parent_column_id
JOIN sys.types pty ON pty.user_type_id=pc.user_type_id
JOIN sys.tables rt ON rt.object_id=fk.referenced_object_id JOIN sys.columns rc ON rc.object_id=rt.object_id AND rc.column_id=fkc.referenced_column_id
JOIN sys.types rty ON rty.user_type_id=rc.user_type_id
LEFT JOIN @Entities pe ON pe.EntityName=pt.name LEFT JOIN @Entities re ON re.EntityName=rt.name
WHERE pe.EntityName IS NOT NULL OR re.EntityName IS NOT NULL;

-- Persist every actual database FK to an identity, and fail on unsupported composite references.
IF EXISTS(SELECT fk.object_id FROM sys.foreign_keys fk JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id=fk.object_id
          JOIN sys.tables rt ON rt.object_id=fk.referenced_object_id JOIN @Entities e ON e.EntityName=rt.name
          GROUP BY fk.object_id HAVING COUNT(*)<>1)
    THROW 51000, 'Composite foreign keys to identity tables need explicit migration review.', 1;
IF EXISTS(SELECT 1 FROM sys.foreign_keys fk JOIN sys.tables rt ON rt.object_id=fk.referenced_object_id JOIN @Entities e ON e.EntityName=rt.name
          JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id=fk.object_id JOIN sys.columns rc ON rc.object_id=rt.object_id AND rc.column_id=fkc.referenced_column_id
          JOIN sys.tables pt ON pt.object_id=fk.parent_object_id LEFT JOIN @Entities pe ON pe.EntityName=pt.name
          WHERE rc.name=N'Id' AND pe.EntityName IS NULL)
    THROW 51000, 'An identity FK leaves the explicit entity manifest.', 1;

DECLARE @parent sysname,@column sysname,@target sysname,@parentId sysname,@fkCursor CURSOR;
SET @fkCursor=CURSOR LOCAL FAST_FORWARD FOR
SELECT pt.name,pc.name,rt.name
FROM sys.foreign_key_columns fkc JOIN sys.foreign_keys fk ON fk.object_id=fkc.constraint_object_id
JOIN sys.tables pt ON pt.object_id=fk.parent_object_id JOIN sys.columns pc ON pc.object_id=pt.object_id AND pc.column_id=fkc.parent_column_id
JOIN sys.tables rt ON rt.object_id=fk.referenced_object_id JOIN sys.columns rc ON rc.object_id=rt.object_id AND rc.column_id=fkc.referenced_column_id
JOIN @Entities e ON e.EntityName=rt.name WHERE rc.name=N'Id' AND SCHEMA_NAME(pt.schema_id)=N'dbo' AND SCHEMA_NAME(rt.schema_id)=N'dbo';
OPEN @fkCursor; FETCH NEXT FROM @fkCursor INTO @parent,@column,@target;
WHILE @@FETCH_STATUS=0
BEGIN
    SET @sql=N'INSERT dbo.GuidIdentityPrepareReferenceMap(ParentEntityName,ParentLegacyId,ColumnName,ReferencedEntityName,ReferencedLegacyId,GuidId) SELECT @p,CONVERT(bigint,x.Id),@c,@t,CONVERT(bigint,x.'+QUOTENAME(@column)+N'),m.GuidId FROM dbo.'+QUOTENAME(@parent)+N' x LEFT JOIN dbo.GuidIdentityPrepareMap m ON m.EntityName=@t AND m.LegacyId=CONVERT(bigint,x.'+QUOTENAME(@column)+N') WHERE x.'+QUOTENAME(@column)+N' IS NOT NULL; IF EXISTS(SELECT 1 FROM dbo.'+QUOTENAME(@parent)+N' x LEFT JOIN dbo.GuidIdentityPrepareMap m ON m.EntityName=@t AND m.LegacyId=CONVERT(bigint,x.'+QUOTENAME(@column)+N') WHERE x.'+QUOTENAME(@column)+N' IS NOT NULL AND m.GuidId IS NULL) THROW 51000, ''Identity FK value has no staged target.'', 1;';
    EXEC sys.sp_executesql @sql,N'@p sysname,@c sysname,@t sysname',@p=@parent,@c=@column,@t=@target;
    FETCH NEXT FROM @fkCursor INTO @parent,@column,@target;
END;
CLOSE @fkCursor; DEALLOCATE @fkCursor;

-- This manifest supplements SQL constraints for persisted Guid identity references that are
-- intentionally scalar-only in the model, including RequestHistory.ActorAccountId.
DECLARE @ScalarRefs table(ParentEntityName sysname,ColumnName sysname,ReferencedEntityName sysname,
                          PRIMARY KEY(ParentEntityName,ColumnName));
INSERT @ScalarRefs
SELECT e.EntityName,r.ColumnName,N'Accounts'
FROM @Entities e CROSS JOIN (VALUES(N'CreatedByAccountId'),(N'LastModifiedByAccountId')) r(ColumnName)
WHERE e.HasPublicId=1;
INSERT @ScalarRefs VALUES
(N'Companies',N'ProjectId',N'Projects'),
(N'CompanyModule',N'CompanyId',N'Companies'),(N'CompanyModule',N'ModuleId',N'Modules'),
(N'ConversationParticipants',N'ConversationId',N'Conversations'),(N'ConversationParticipants',N'UserId',N'Users'),(N'ConversationParticipants',N'CustomerId',N'Customers'),
(N'CustomerRequestDocuments',N'CustomerRequestId',N'CustomerRequests'),(N'CustomerRequestDocuments',N'CustomerRequestMessageId',N'CustomerRequestMessages'),
(N'CustomerRequestEvaluations',N'CustomerRequestId',N'CustomerRequests'),(N'CustomerRequestEvaluations',N'CustomerUserId',N'Customers'),
(N'CustomerRequestMessages',N'ConversationId',N'Conversations'),(N'CustomerRequestMessages',N'SenderParticipantId',N'ConversationParticipants'),
(N'CustomerRequests',N'CustomerId',N'Customers'),(N'CustomerRequests',N'RequestSubjectId',N'RequestSubjects'),(N'CustomerRequests',N'ModuleId',N'Modules'),
(N'CustomerRequests',N'AssignedUserId',N'Users'),(N'CustomerRequests',N'CurrentExpertId',N'Users'),(N'CustomerRequests',N'ConversationId',N'Conversations'),(N'CustomerRequests',N'StatusId',N'CustomerRequestStatuses'),
(N'CustomerRequestStatuses',N'ProjectId',N'Projects'),
(N'Customers',N'AccountId',N'Accounts'),(N'Customers',N'CompanyId',N'Companies'),
(N'Documents',N'GuideId',N'Guides'),
(N'FAQs',N'ProjectId',N'Projects'),(N'FAQs',N'ModuleId',N'Modules'),
(N'Guides',N'PreviousGuideId',N'Guides'),(N'Guides',N'ModuleId',N'Modules'),(N'Guides',N'ProjectId',N'Projects'),
(N'ModuleExperts',N'ModuleId',N'Modules'),(N'ModuleExperts',N'UserId',N'Users'),
(N'RequestHistories',N'RequestId',N'CustomerRequests'),(N'RequestHistories',N'ActorAccountId',N'Accounts'),
(N'RequestSubjects',N'ModuleId',N'Modules'),(N'RequestSubjects',N'ProjectId',N'Projects'),
(N'RolePermissionActions',N'RolePermissionId',N'RolePermissions'),(N'RolePermissions',N'RoleId',N'Roles'),
(N'UserProjects',N'ProjectId',N'Projects'),(N'UserProjects',N'UserId',N'Users'),
(N'UserRoles',N'UserId',N'Users'),(N'UserRoles',N'RoleId',N'Roles'),
(N'Users',N'AccountId',N'Accounts'),(N'MenuItems',N'ParentId',N'MenuItems'),
(N'AuditLogs',N'ActorUserId',N'Users'),(N'RefreshTokens',N'UserId',N'Users'),(N'RefreshTokens',N'CustomerId',N'Customers');

-- Require an explicit disposition for every numeric identity-shaped field in converted tables.
IF EXISTS
(
    SELECT 1 FROM sys.tables t JOIN @Entities e ON e.EntityName=t.name
    JOIN sys.columns c ON c.object_id=t.object_id JOIN sys.types ty ON ty.user_type_id=c.user_type_id
    WHERE SCHEMA_NAME(t.schema_id)=N'dbo' AND (c.name=N'Id' OR c.name LIKE N'%Id')
      AND ty.system_type_id IN(48,52,56,127,106,108,60,122,59,62,104,98)
      AND (ty.name NOT IN(N'int',N'bigint') OR
          (NOT (c.name=N'Id' AND EXISTS(SELECT 1 FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id WHERE i.object_id=t.object_id AND i.is_primary_key=1 AND ic.column_id=c.column_id))
           AND NOT EXISTS(SELECT 1 FROM sys.foreign_key_columns fkc WHERE fkc.parent_object_id=t.object_id AND fkc.parent_column_id=c.column_id)
           AND NOT EXISTS(SELECT 1 FROM @ScalarRefs s WHERE s.ParentEntityName=t.name AND s.ColumnName=c.name)
           AND NOT (t.name=N'AuditLogs' AND c.name=N'TargetId')))
)
    THROW 51000, 'A numeric Id-like column in a converted entity is not covered by PK/FK/scalar/audit manifests.', 1;

-- Inventory catalog dependencies and conservative dynamic-SQL name matches. Such modules are
-- currently fail-closed until explicitly rewritten or reviewed for GUID-safe behavior.
INSERT dbo.GuidIdentityPrepareModuleDependency(ReferencingSchema,ReferencingObject,ReferencingType,ReferencedEntityName,ReferencedColumnName,DefinitionHash,TriggerMetadata,DiscoveryMethod,Disposition)
SELECT DISTINCT SCHEMA_NAME(o.schema_id),o.name,o.type_desc,t.name,COALESCE(rc.name,N'*'),
       CASE WHEN sm.definition IS NULL THEN NULL ELSE HASHBYTES('SHA2_256',CONVERT(varbinary(max),sm.definition)) END,
       CASE WHEN tr.object_id IS NULL THEN NULL ELSE CONCAT(tr.is_disabled,N'|',tr.is_not_for_replication,N'|',tr.is_instead_of_trigger,N'|',ISNULL(te.EventList,N'')) END,
       CASE WHEN o.type=N'TR' AND EXISTS(SELECT 1 FROM sys.triggers tr WHERE tr.object_id=o.object_id AND tr.parent_id=t.object_id) THEN 'AttachedTrigger'
            WHEN sed.referenced_id=t.object_id THEN 'CatalogDependency' ELSE 'TextSearch' END,
       CASE WHEN sm.definition IS NULL OR SCHEMA_NAME(o.schema_id)<>N'dbo' THEN 'Unclassified'
            WHEN o.type=N'P' AND o.name IN(N'Sp_StatusDistribution',N'Sp_ModuleDistribution',N'Sp_CompanyTopN',N'Sp_AgentPerformance',N'Sp_DailyTrend',N'Sp_AvgResolutionMinutes',N'Sp_ReopenStats',N'Sp_CompletedRequests',N'Sp_ActiveRequests',N'Sp_DashboardStats') THEN 'RewriteForGuid'
            WHEN o.type=N'P' AND o.name IN(N'Sp_TotalRequest',N'Sp_TotalMessages',N'Sp_TotalCustomers',N'Sp_TotalCompanies') AND CHARINDEX(N'Id',sm.definition COLLATE Latin1_General_100_CI_AS)=0 AND sm.definition COLLATE Latin1_General_100_CI_AS NOT LIKE N'%sp_executesql%' AND sm.definition COLLATE Latin1_General_100_CI_AS NOT LIKE N'%EXEC(%' AND sm.definition COLLATE Latin1_General_100_CI_AS NOT LIKE N'%EXEC (%' AND sm.definition COLLATE Latin1_General_100_CI_AS NOT LIKE N'%EXECUTE(%' AND sm.definition COLLATE Latin1_General_100_CI_AS NOT LIKE N'%EXECUTE (%' AND sm.definition COLLATE Latin1_General_100_CI_AS NOT LIKE N'%EXEC @%' AND sm.definition COLLATE Latin1_General_100_CI_AS NOT LIKE N'%EXECUTE @%' THEN 'PreserveUnchanged'
            WHEN o.type=N'TR' AND o.name=N'TR_AuditLogs_AppendOnly' AND CHARINDEX(N'Id',sm.definition COLLATE Latin1_General_100_CI_AS)=0 AND sm.definition COLLATE Latin1_General_100_CI_AS NOT LIKE N'%sp_executesql%' AND sm.definition COLLATE Latin1_General_100_CI_AS NOT LIKE N'%EXEC(%' AND sm.definition COLLATE Latin1_General_100_CI_AS NOT LIKE N'%EXEC (%' AND sm.definition COLLATE Latin1_General_100_CI_AS NOT LIKE N'%EXECUTE(%' AND sm.definition COLLATE Latin1_General_100_CI_AS NOT LIKE N'%EXECUTE (%' AND sm.definition COLLATE Latin1_General_100_CI_AS NOT LIKE N'%EXEC @%' AND sm.definition COLLATE Latin1_General_100_CI_AS NOT LIKE N'%EXECUTE @%' THEN 'PreserveUnchanged'
            ELSE 'Unclassified' END
FROM sys.objects o JOIN sys.sql_modules sm ON sm.object_id=o.object_id
CROSS JOIN @Entities e JOIN sys.tables t ON t.name=e.EntityName AND SCHEMA_NAME(t.schema_id)=N'dbo'
LEFT JOIN sys.sql_expression_dependencies sed ON sed.referencing_id=o.object_id AND sed.referenced_id=t.object_id
LEFT JOIN sys.columns rc ON rc.object_id=t.object_id AND rc.column_id=sed.referenced_minor_id
LEFT JOIN sys.triggers tr ON tr.object_id=o.object_id
OUTER APPLY(SELECT STUFF((SELECT N','+CONCAT(tEvent.type_desc,N':',tEvent.event_group_type_desc,N':',tEvent.is_first,N':',tEvent.is_last) FROM sys.trigger_events tEvent WHERE tEvent.object_id=o.object_id ORDER BY tEvent.type_desc FOR XML PATH(''),TYPE).value(N'.',N'nvarchar(max)'),1,1,N'') AS EventList) te
WHERE o.type IN(N'V',N'P',N'FN',N'IF',N'TF',N'TR')
  AND (sed.referenced_id=t.object_id OR (o.type=N'TR' AND EXISTS(SELECT 1 FROM sys.triggers tr WHERE tr.object_id=o.object_id AND tr.parent_id=t.object_id)) OR sm.definition IS NULL OR sm.definition COLLATE Latin1_General_100_CI_AS LIKE (N'%'+t.name+N'%') COLLATE Latin1_General_100_CI_AS OR sm.definition COLLATE Latin1_General_100_CI_AS LIKE N'%sp_executesql%' OR sm.definition COLLATE Latin1_General_100_CI_AS LIKE N'%EXEC(%' OR sm.definition COLLATE Latin1_General_100_CI_AS LIKE N'%EXEC (%' OR sm.definition COLLATE Latin1_General_100_CI_AS LIKE N'%EXECUTE(%' OR sm.definition COLLATE Latin1_General_100_CI_AS LIKE N'%EXECUTE (%' OR sm.definition COLLATE Latin1_General_100_CI_AS LIKE N'%EXEC @%' OR sm.definition COLLATE Latin1_General_100_CI_AS LIKE N'%EXECUTE @%');
IF EXISTS(SELECT 1 FROM dbo.GuidIdentityPrepareModuleDependency WHERE Disposition='Unclassified')
    THROW 51000, 'An unclassified SQL module depends on a converted entity; add an explicit rewrite/preserve disposition before proceeding.', 1;

IF EXISTS(
    SELECT 1 FROM sys.foreign_key_columns fkc JOIN sys.foreign_keys fk ON fk.object_id=fkc.constraint_object_id
    JOIN sys.tables pt ON pt.object_id=fk.parent_object_id JOIN @ScalarRefs sr ON sr.ParentEntityName=pt.name
    JOIN sys.columns pc ON pc.object_id=pt.object_id AND pc.column_id=fkc.parent_column_id AND pc.name=sr.ColumnName
    JOIN sys.tables rt ON rt.object_id=fk.referenced_object_id JOIN sys.columns rc ON rc.object_id=rt.object_id AND rc.column_id=fkc.referenced_column_id
    WHERE rt.name<>sr.ReferencedEntityName OR rc.name<>N'Id' OR SCHEMA_NAME(rt.schema_id)<>N'dbo')
    THROW 51000, 'A declared identity scalar FK targets a different entity/key than the explicit migration manifest.', 1;
DECLARE @scalarParent sysname,@scalarColumn sysname,@scalarTarget sysname;
DECLARE scalar_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT ParentEntityName,ColumnName,ReferencedEntityName FROM @ScalarRefs;
OPEN scalar_cursor; FETCH NEXT FROM scalar_cursor INTO @scalarParent,@scalarColumn,@scalarTarget;
WHILE @@FETCH_STATUS=0
BEGIN
    IF NOT EXISTS(SELECT 1 FROM sys.columns c JOIN sys.tables t ON t.object_id=c.object_id WHERE t.name=@scalarParent AND SCHEMA_NAME(t.schema_id)=N'dbo' AND c.name=@scalarColumn)
        THROW 51000, 'An explicit scalar identity reference is missing from the database schema.', 1;
    SET @sql=N'IF NOT EXISTS(SELECT 1 FROM dbo.GuidIdentityPrepareReferenceMap WHERE ParentEntityName=@p AND ColumnName=@c) INSERT dbo.GuidIdentityPrepareReferenceMap(ParentEntityName,ParentLegacyId,ColumnName,ReferencedEntityName,ReferencedLegacyId,GuidId) SELECT @p,CONVERT(bigint,x.Id),@c,@t,CONVERT(bigint,x.'+QUOTENAME(@scalarColumn)+N'),m.GuidId FROM dbo.'+QUOTENAME(@scalarParent)+N' x LEFT JOIN dbo.GuidIdentityPrepareMap m ON m.EntityName=@t AND m.LegacyId=CONVERT(bigint,x.'+QUOTENAME(@scalarColumn)+N') WHERE x.'+QUOTENAME(@scalarColumn)+N' IS NOT NULL; IF EXISTS(SELECT 1 FROM dbo.'+QUOTENAME(@scalarParent)+N' x LEFT JOIN dbo.GuidIdentityPrepareMap m ON m.EntityName=@t AND m.LegacyId=CONVERT(bigint,x.'+QUOTENAME(@scalarColumn)+N') WHERE x.'+QUOTENAME(@scalarColumn)+N' IS NOT NULL AND m.GuidId IS NULL) THROW 51000, ''Scalar identity reference has no staged target.'', 1;';
    EXEC sys.sp_executesql @sql,N'@p sysname,@c sysname,@t sysname',@p=@scalarParent,@c=@scalarColumn,@t=@scalarTarget;
    FETCH NEXT FROM scalar_cursor INTO @scalarParent,@scalarColumn,@scalarTarget;
END;
CLOSE scalar_cursor; DEALLOCATE scalar_cursor;

-- Audit TargetId is polymorphic. Unknown type/value pairs stop before any entity schema change.
IF EXISTS(SELECT 1 FROM dbo.AuditLogs WHERE TargetId IS NOT NULL AND (TargetType IS NULL OR TargetType NOT IN
 (N'Account',N'Company',N'CompanyModule',N'Conversation',N'ConversationParticipant',N'Customer',N'CustomerRequest',N'CustomerRequestEvaluation',N'CustomerRequestMessage',N'CustomerRequestMessageDocument',N'CustomerRequestStatus',N'Document',N'FAQ',N'Guide',N'MenuItem',N'Module',N'ModuleExpert',N'OrganizationInfo',N'Project',N'ProjectModule',N'RequestHistory',N'RequestSubject',N'Role',N'RolePermission',N'RolePermissionAction',N'User',N'UserProject',N'UserRole')))
    THROW 51000, 'AuditLog TargetType is unknown; add the historic value to the explicit identity manifest.', 1;
DECLARE @AuditTargetMap table(TargetType sysname PRIMARY KEY,EntityName sysname NOT NULL);
INSERT @AuditTargetMap VALUES
(N'Account',N'Accounts'),(N'Company',N'Companies'),(N'CompanyModule',N'CompanyModule'),(N'Conversation',N'Conversations'),(N'ConversationParticipant',N'ConversationParticipants'),(N'Customer',N'Customers'),(N'CustomerRequest',N'CustomerRequests'),(N'CustomerRequestEvaluation',N'CustomerRequestEvaluations'),(N'CustomerRequestMessage',N'CustomerRequestMessages'),(N'CustomerRequestMessageDocument',N'CustomerRequestDocuments'),(N'CustomerRequestStatus',N'CustomerRequestStatuses'),(N'Document',N'Documents'),(N'FAQ',N'FAQs'),(N'Guide',N'Guides'),(N'MenuItem',N'MenuItems'),(N'Module',N'Modules'),(N'ModuleExpert',N'ModuleExperts'),(N'OrganizationInfo',N'OrganizationInfos'),(N'Project',N'Projects'),(N'ProjectModule',N'ProjectModule'),(N'RequestHistory',N'RequestHistories'),(N'RequestSubject',N'RequestSubjects'),(N'Role',N'Roles'),(N'RolePermission',N'RolePermissions'),(N'RolePermissionAction',N'RolePermissionActions'),(N'User',N'Users'),(N'UserProject',N'UserProjects'),(N'UserRole',N'UserRoles');
INSERT dbo.GuidIdentityPrepareReferenceMap(ParentEntityName,ParentLegacyId,ColumnName,ReferencedEntityName,ReferencedLegacyId,GuidId)
SELECT N'AuditLogs',CONVERT(bigint,a.Id),N'TargetId',t.EntityName,CONVERT(bigint,a.TargetId),m.GuidId
FROM dbo.AuditLogs a JOIN @AuditTargetMap t ON t.TargetType=a.TargetType
LEFT JOIN dbo.GuidIdentityPrepareMap m ON m.EntityName=t.EntityName AND m.LegacyId=CONVERT(bigint,a.TargetId)
WHERE a.TargetId IS NOT NULL;
IF EXISTS(SELECT 1 FROM dbo.AuditLogs a JOIN @AuditTargetMap t ON t.TargetType=a.TargetType LEFT JOIN dbo.GuidIdentityPrepareMap m ON m.EntityName=t.EntityName AND m.LegacyId=CONVERT(bigint,a.TargetId) WHERE a.TargetId IS NOT NULL AND m.GuidId IS NULL)
    THROW 51000, 'AuditLog TargetId has no staged identity target.', 1;

-- Status history is a persisted scalar reference encoded as text. Resolve every non-empty old/new
-- status value, not only the four currently seeded status IDs.
INSERT dbo.GuidIdentityPrepareTextReferenceMap(ParentLegacyId,ColumnName,OriginalValue,ReferencedEntityName,ReferencedLegacyId,GuidId)
SELECT CONVERT(bigint,h.Id),v.ColumnName,v.OriginalValue,N'CustomerRequestStatuses',m.LegacyId,m.GuidId
FROM dbo.RequestHistories h
CROSS APPLY(VALUES(N'OldValue',h.OldValue),(N'NewValue',h.NewValue)) v(ColumnName,OriginalValue)
LEFT JOIN dbo.GuidIdentityPrepareMap m ON m.EntityName=N'CustomerRequestStatuses'
 AND ((TRY_CONVERT(bigint,v.OriginalValue) IS NOT NULL AND m.LegacyId=TRY_CONVERT(bigint,v.OriginalValue))
   OR (TRY_CONVERT(uniqueidentifier,v.OriginalValue) IS NOT NULL AND (m.GuidId=TRY_CONVERT(uniqueidentifier,v.OriginalValue) OR m.OriginalPublicId=TRY_CONVERT(uniqueidentifier,v.OriginalValue))))
WHERE h.Action=2 AND NULLIF(LTRIM(RTRIM(v.OriginalValue)),N'') IS NOT NULL;
IF EXISTS(SELECT 1 FROM dbo.RequestHistories h CROSS APPLY(VALUES(h.OldValue),(h.NewValue)) v(OriginalValue)
          LEFT JOIN dbo.GuidIdentityPrepareTextReferenceMap r ON r.ParentLegacyId=CONVERT(bigint,h.Id) AND r.OriginalValue=v.OriginalValue
          WHERE h.Action=2 AND NULLIF(LTRIM(RTRIM(v.OriginalValue)),N'') IS NOT NULL AND r.GuidId IS NULL)
    THROW 51000, 'RequestHistory contains an unresolved status identity value.', 1;
""";
}
