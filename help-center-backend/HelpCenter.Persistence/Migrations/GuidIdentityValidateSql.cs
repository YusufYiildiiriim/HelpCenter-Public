namespace HelpCenter.Persistence.Migrations;

/// <summary>Validates the prepared snapshot and records a gate for the later, separately reviewed cutover.</summary>
internal static class GuidIdentityValidateSql
{
    internal const string Down = "DROP TABLE IF EXISTS dbo.GuidIdentityValidateGate;";

    internal const string Up = """
SET XACT_ABORT ON;
IF ISNULL(TRY_CONVERT(int,SERVERPROPERTY('ProductMajorVersion')),0)<15
    THROW 51000, 'GUID identity migration requires SQL Server 2019 (15.x) or later.', 1;
-- Validation executes inside EF's migration transaction. Hold exclusive table locks through its
-- completion so no ordinary writer can invalidate the snapshot between checks.
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
IF OBJECT_ID(N'dbo.GuidIdentityPrepareMap',N'U') IS NULL
   OR OBJECT_ID(N'dbo.GuidIdentityPrepareRowCounts',N'U') IS NULL
   OR OBJECT_ID(N'dbo.GuidIdentityPrepareReferenceMap',N'U') IS NULL
   OR OBJECT_ID(N'dbo.GuidIdentityPrepareTextReferenceMap',N'U') IS NULL
   OR OBJECT_ID(N'dbo.GuidIdentityPrepareForeignKeyInventory',N'U') IS NULL
   OR OBJECT_ID(N'dbo.GuidIdentityPrepareModuleDependency',N'U') IS NULL
    THROW 51000, 'Guid identity preparation artifacts are incomplete; Validate cannot proceed.', 1;
IF OBJECT_ID(N'dbo.GuidIdentityValidateGate',N'U') IS NOT NULL
    THROW 51000, 'A Guid identity validation gate already exists; inspect migration history before retrying.', 1;

CREATE TABLE dbo.GuidIdentityValidateGate
(
    GateId int NOT NULL CONSTRAINT PK_GuidIdentityValidateGate PRIMARY KEY,
    DatabaseName sysname NOT NULL,
    ValidatedAtUtc datetime2(7) NOT NULL,
    EntityCount int NOT NULL,
    SourceRows bigint NOT NULL,
    PreparedIds bigint NOT NULL,
    PreparedReferences bigint NOT NULL,
    PreparedTextReferences bigint NOT NULL,
    ForeignKeyCount bigint NOT NULL,
    ModuleDependencyCount bigint NOT NULL,
    SchemaFingerprint varbinary(32) NOT NULL,
    PreparedFingerprint varbinary(32) NOT NULL,
    CONSTRAINT CK_GuidIdentityValidateGate_Singleton CHECK(GateId=1)
);

DECLARE @Entities table(EntityName sysname PRIMARY KEY,HasPublicId bit NOT NULL);
INSERT @Entities(EntityName,HasPublicId) VALUES
(N'Accounts',1),(N'Companies',1),(N'CompanyModule',1),(N'ConversationParticipants',1),(N'Conversations',1),
(N'CustomerRequestDocuments',1),(N'CustomerRequestEvaluations',1),(N'CustomerRequestMessages',1),(N'CustomerRequests',1),
(N'CustomerRequestStatuses',1),(N'Customers',1),(N'Documents',1),(N'FAQs',1),(N'Guides',1),(N'MenuItems',1),
(N'ModuleExperts',1),(N'Modules',1),(N'OrganizationInfos',1),(N'ProjectModule',1),(N'Projects',1),(N'RequestHistories',1),
(N'RequestSubjects',1),(N'RolePermissionActions',1),(N'RolePermissions',1),(N'Roles',1),(N'UserProjects',1),(N'UserRoles',1),
(N'Users',1),(N'AuditLogs',0),(N'RefreshTokens',0);
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
    THROW 51000, 'A SQL synonym or dependent module now targets a converted entity.', 1;
IF (SELECT COUNT(*) FROM dbo.GuidIdentityPrepareRowCounts)<>(SELECT COUNT(*) FROM @Entities)
 OR EXISTS(SELECT e.EntityName FROM @Entities e LEFT JOIN dbo.GuidIdentityPrepareRowCounts c ON c.EntityName=e.EntityName WHERE c.EntityName IS NULL)
    THROW 51000, 'The prepared entity row-count manifest is incomplete.', 1;
IF EXISTS(SELECT 1 FROM dbo.GuidIdentityPrepareRowCounts WHERE SourceRowCount<>PreparedRowCount)
    THROW 51000, 'Prepared identity row counts are internally inconsistent.', 1;

-- Exact source-to-map parity. This detects inserted/deleted rows, changed Ids, changed PublicIds,
-- changed deterministic assignments, or replaced staging values after Prepare.
DECLARE @entity sysname,@hasPublicId bit,@sql nvarchar(max);
DECLARE entity_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT EntityName,HasPublicId FROM @Entities ORDER BY EntityName;
OPEN entity_cursor; FETCH NEXT FROM entity_cursor INTO @entity,@hasPublicId;
WHILE @@FETCH_STATUS=0
BEGIN
    SET @sql=N'IF (SELECT COUNT_BIG(*) FROM dbo.'+QUOTENAME(@entity)+N')<>(SELECT SourceRowCount FROM dbo.GuidIdentityPrepareRowCounts WHERE EntityName=@n) THROW 51000, ''Source row count changed after Prepare.'', 1;'
      +N' IF (SELECT COUNT_BIG(*) FROM dbo.GuidIdentityPrepareMap WHERE EntityName=@n)<>(SELECT SourceRowCount FROM dbo.GuidIdentityPrepareRowCounts WHERE EntityName=@n) THROW 51000, ''Prepared identity count changed after Prepare.'', 1;'
      +N' IF EXISTS(SELECT 1 FROM dbo.'+QUOTENAME(@entity)+N' x LEFT JOIN dbo.GuidIdentityPrepareMap m ON m.EntityName=@n AND m.LegacyId=CONVERT(bigint,x.Id) WHERE m.LegacyId IS NULL) THROW 51000, ''A source row has no prepared identity.'', 1;'
      +N' IF EXISTS(SELECT 1 FROM dbo.GuidIdentityPrepareMap m LEFT JOIN dbo.'+QUOTENAME(@entity)+N' x ON CONVERT(bigint,x.Id)=m.LegacyId WHERE m.EntityName=@n AND x.Id IS NULL) THROW 51000, ''A prepared identity has no source row.'', 1;';
    IF @hasPublicId=1
    BEGIN
        SET @sql+=N' IF EXISTS(SELECT 1 FROM dbo.'+QUOTENAME(@entity)+N' x JOIN dbo.GuidIdentityPrepareMap m ON m.EntityName=@n AND m.LegacyId=CONVERT(bigint,x.Id) WHERE ISNULL(x.PublicId,''00000000-0000-0000-0000-000000000000'')<>ISNULL(m.OriginalPublicId,''00000000-0000-0000-0000-000000000000'')) THROW 51000, ''PublicId changed after Prepare.'', 1;';
        IF @entity=N'CustomerRequestStatuses'
            SET @sql+=N' IF EXISTS(SELECT 1 FROM dbo.'+QUOTENAME(@entity)+N' x JOIN dbo.GuidIdentityPrepareMap m ON m.EntityName=@n AND m.LegacyId=CONVERT(bigint,x.Id) WHERE m.GuidId<>CASE CONVERT(bigint,x.Id) WHEN 1 THEN ''10000000-0000-4000-8000-000000000001'' WHEN 2 THEN ''10000000-0000-4000-8000-000000000002'' WHEN 3 THEN ''10000000-0000-4000-8000-000000000003'' WHEN 4 THEN ''10000000-0000-4000-8000-000000000004'' ELSE x.PublicId END) THROW 51000, ''Status identity mapping changed after Prepare.'', 1;';
        ELSE
            SET @sql+=N' IF EXISTS(SELECT 1 FROM dbo.'+QUOTENAME(@entity)+N' x JOIN dbo.GuidIdentityPrepareMap m ON m.EntityName=@n AND m.LegacyId=CONVERT(bigint,x.Id) WHERE m.GuidId<>CASE WHEN x.PublicId IS NULL OR x.PublicId=''00000000-0000-0000-0000-000000000000'' THEN CONVERT(uniqueidentifier,''00000000-0000-0000-0000-''+RIGHT(REPLICATE(''0'',12)+CONVERT(varchar(12),CONVERT(bigint,x.Id)),12)) ELSE x.PublicId END) THROW 51000, ''Entity Guid mapping changed after Prepare.'', 1;';
    END
    ELSE
        SET @sql+=N' IF EXISTS(SELECT 1 FROM dbo.'+QUOTENAME(@entity)+N' x JOIN dbo.GuidIdentityPrepareMap m ON m.EntityName=@n AND m.LegacyId=CONVERT(bigint,x.Id) WHERE m.GuidId<>CONVERT(uniqueidentifier,SUBSTRING(HASHBYTES(''SHA2_256'',CONVERT(varbinary(max),CONCAT(@n,N'':'' ,CONVERT(varchar(20),CONVERT(bigint,x.Id))))),1,16))) THROW 51000, ''Deterministic Guid mapping changed after Prepare.'', 1;';
    EXEC sys.sp_executesql @sql,N'@n sysname',@n=@entity;
    FETCH NEXT FROM entity_cursor INTO @entity,@hasPublicId;
END;
CLOSE entity_cursor; DEALLOCATE entity_cursor;

-- Recreate the explicit scalar manifest and union it with every staged database FK. This includes
-- nullable references with no values at Prepare time, so a later non-null write cannot go unseen.
DECLARE @ReferenceColumns table(ParentEntityName sysname,ColumnName sysname,ReferencedEntityName sysname);
INSERT @ReferenceColumns SELECT DISTINCT ParentEntityName,ParentColumnName,ReferencedEntityName
FROM dbo.GuidIdentityPrepareForeignKeyInventory WHERE Disposition='GuidIdentity';
INSERT @ReferenceColumns
SELECT e.EntityName,r.ColumnName,N'Accounts' FROM @Entities e
CROSS JOIN(VALUES(N'CreatedByAccountId'),(N'LastModifiedByAccountId')) r(ColumnName) WHERE e.HasPublicId=1;
INSERT @ReferenceColumns VALUES
(N'Companies',N'ProjectId',N'Projects'),(N'CompanyModule',N'CompanyId',N'Companies'),(N'CompanyModule',N'ModuleId',N'Modules'),
(N'ConversationParticipants',N'ConversationId',N'Conversations'),(N'ConversationParticipants',N'UserId',N'Users'),(N'ConversationParticipants',N'CustomerId',N'Customers'),
(N'CustomerRequestDocuments',N'CustomerRequestId',N'CustomerRequests'),(N'CustomerRequestDocuments',N'CustomerRequestMessageId',N'CustomerRequestMessages'),
(N'CustomerRequestEvaluations',N'CustomerRequestId',N'CustomerRequests'),(N'CustomerRequestEvaluations',N'CustomerUserId',N'Customers'),
(N'CustomerRequestMessages',N'ConversationId',N'Conversations'),(N'CustomerRequestMessages',N'SenderParticipantId',N'ConversationParticipants'),
(N'CustomerRequests',N'CustomerId',N'Customers'),(N'CustomerRequests',N'RequestSubjectId',N'RequestSubjects'),(N'CustomerRequests',N'ModuleId',N'Modules'),
(N'CustomerRequests',N'AssignedUserId',N'Users'),(N'CustomerRequests',N'CurrentExpertId',N'Users'),(N'CustomerRequests',N'ConversationId',N'Conversations'),(N'CustomerRequests',N'StatusId',N'CustomerRequestStatuses'),
(N'CustomerRequestStatuses',N'ProjectId',N'Projects'),(N'Customers',N'AccountId',N'Accounts'),(N'Customers',N'CompanyId',N'Companies'),
(N'Documents',N'GuideId',N'Guides'),(N'FAQs',N'ProjectId',N'Projects'),(N'FAQs',N'ModuleId',N'Modules'),
(N'Guides',N'PreviousGuideId',N'Guides'),(N'Guides',N'ModuleId',N'Modules'),(N'Guides',N'ProjectId',N'Projects'),
(N'ModuleExperts',N'ModuleId',N'Modules'),(N'ModuleExperts',N'UserId',N'Users'),(N'RequestHistories',N'RequestId',N'CustomerRequests'),(N'RequestHistories',N'ActorAccountId',N'Accounts'),
(N'RequestSubjects',N'ModuleId',N'Modules'),(N'RequestSubjects',N'ProjectId',N'Projects'),(N'RolePermissionActions',N'RolePermissionId',N'RolePermissions'),
(N'RolePermissions',N'RoleId',N'Roles'),(N'UserProjects',N'ProjectId',N'Projects'),(N'UserProjects',N'UserId',N'Users'),(N'UserRoles',N'UserId',N'Users'),
(N'UserRoles',N'RoleId',N'Roles'),(N'Users',N'AccountId',N'Accounts'),(N'MenuItems',N'ParentId',N'MenuItems'),
(N'AuditLogs',N'ActorUserId',N'Users'),(N'RefreshTokens',N'UserId',N'Users'),(N'RefreshTokens',N'CustomerId',N'Customers');

-- The suffix-based catalog scan fails closed for any numeric identity-shaped column not already
-- covered by a PK, FK inventory, scalar reference or polymorphic/text-specific manifest.
IF EXISTS
(
    SELECT 1 FROM sys.tables t JOIN @Entities e ON e.EntityName=t.name
    JOIN sys.columns c ON c.object_id=t.object_id JOIN sys.types ty ON ty.user_type_id=c.user_type_id
    WHERE SCHEMA_NAME(t.schema_id)=N'dbo' AND (c.name=N'Id' OR c.name LIKE N'%Id')
      AND (ty.system_type_id IN(48,52,56,127,106,108,60,122,59,62,104) OR ty.name=N'sql_variant')
      AND (ty.name NOT IN(N'int',N'bigint') OR
          (NOT (c.name=N'Id' AND EXISTS(SELECT 1 FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id WHERE i.object_id=t.object_id AND i.is_primary_key=1 AND ic.column_id=c.column_id))
           AND NOT EXISTS(SELECT 1 FROM sys.foreign_key_columns fkc WHERE fkc.parent_object_id=t.object_id AND fkc.parent_column_id=c.column_id)
           AND NOT EXISTS(SELECT 1 FROM @ReferenceColumns r WHERE r.ParentEntityName=t.name AND r.ColumnName=c.name)
           AND NOT (t.name=N'AuditLogs' AND c.name=N'TargetId')))
)
    THROW 51000, 'A numeric Id-like column in a converted entity is not covered by PK/FK/scalar/audit manifests.', 1;

-- Compare the complete current module dependency inventory, including conservative text matches
-- for dynamic SQL and attached triggers. Only known modules with explicit dispositions are allowed.
CREATE TABLE #CurrentModules
(
    ReferencingSchema sysname NOT NULL,ReferencingObject sysname NOT NULL,ReferencingType nvarchar(60) NOT NULL,
    ReferencedEntityName sysname NOT NULL,ReferencedColumnName sysname NOT NULL,DefinitionHash varbinary(32) NULL,TriggerMetadata nvarchar(max) NULL,
    DiscoveryMethod varchar(24) NOT NULL,Disposition varchar(32) NOT NULL,
    PRIMARY KEY(ReferencingSchema,ReferencingObject,ReferencedEntityName,ReferencedColumnName,DiscoveryMethod)
);
INSERT #CurrentModules
SELECT DISTINCT SCHEMA_NAME(o.schema_id),o.name,o.type_desc,t.name,COALESCE(rc.name,N'*'),
       CASE WHEN sm.definition IS NULL THEN NULL ELSE HASHBYTES('SHA2_256',CONVERT(varbinary(max),sm.definition)) END,
       CASE WHEN tr.object_id IS NULL THEN NULL ELSE CONCAT(tr.is_disabled,N'|',tr.is_not_for_replication,N'|',tr.is_instead_of_trigger,N'|',ISNULL(te.EventList,N'')) END,
       CASE WHEN o.type=N'TR' AND EXISTS(SELECT 1 FROM sys.triggers tr WHERE tr.object_id=o.object_id AND tr.parent_id=t.object_id) THEN 'AttachedTrigger'
            WHEN sed.referenced_id=t.object_id THEN 'CatalogDependency' ELSE 'TextSearch' END,
       CASE WHEN sm.definition IS NULL OR SCHEMA_NAME(o.schema_id)<>N'dbo' THEN 'Unclassified'
            WHEN o.type=N'P' AND o.name IN(N'Sp_StatusDistribution',N'Sp_ModuleDistribution',N'Sp_CompanyTopN',N'Sp_AgentPerformance',N'Sp_DailyTrend',N'Sp_AvgResolutionMinutes',N'Sp_ReopenStats',N'Sp_CompletedRequests',N'Sp_ActiveRequests',N'Sp_DashboardStats') THEN 'RewriteForGuid'
            WHEN o.type=N'P' AND o.name IN(N'Sp_TotalRequest',N'Sp_TotalMessages',N'Sp_TotalCustomers',N'Sp_TotalCompanies') AND CHARINDEX(N'Id',sm.definition)=0 AND sm.definition NOT LIKE N'%sp_executesql%' AND sm.definition NOT LIKE N'%EXEC(%' AND sm.definition NOT LIKE N'%EXEC (%' AND sm.definition NOT LIKE N'%EXECUTE(%' AND sm.definition NOT LIKE N'%EXECUTE (%' AND sm.definition NOT LIKE N'%EXEC @%' AND sm.definition NOT LIKE N'%EXECUTE @%' THEN 'PreserveUnchanged'
            WHEN o.type=N'TR' AND o.name=N'TR_AuditLogs_AppendOnly' AND CHARINDEX(N'Id',sm.definition)=0 AND sm.definition NOT LIKE N'%sp_executesql%' AND sm.definition NOT LIKE N'%EXEC(%' AND sm.definition NOT LIKE N'%EXEC (%' AND sm.definition NOT LIKE N'%EXECUTE(%' AND sm.definition NOT LIKE N'%EXECUTE (%' AND sm.definition NOT LIKE N'%EXEC @%' AND sm.definition NOT LIKE N'%EXECUTE @%' THEN 'PreserveUnchanged'
            ELSE 'Unclassified' END
FROM sys.objects o JOIN sys.sql_modules sm ON sm.object_id=o.object_id
CROSS JOIN @Entities e JOIN sys.tables t ON t.name=e.EntityName AND SCHEMA_NAME(t.schema_id)=N'dbo'
LEFT JOIN sys.sql_expression_dependencies sed ON sed.referencing_id=o.object_id AND sed.referenced_id=t.object_id
LEFT JOIN sys.columns rc ON rc.object_id=t.object_id AND rc.column_id=sed.referenced_minor_id
LEFT JOIN sys.triggers tr ON tr.object_id=o.object_id
OUTER APPLY(SELECT STUFF((SELECT N','+CONCAT(tEvent.type_desc,N':',tEvent.event_group_type_desc,N':',tEvent.is_first,N':',tEvent.is_last) FROM sys.trigger_events tEvent WHERE tEvent.object_id=o.object_id ORDER BY tEvent.type_desc FOR XML PATH(''),TYPE).value(N'.',N'nvarchar(max)'),1,1,N'') AS EventList) te
WHERE o.type IN(N'V',N'P',N'FN',N'IF',N'TF',N'TR')
  AND (sed.referenced_id=t.object_id OR (o.type=N'TR' AND EXISTS(SELECT 1 FROM sys.triggers tr WHERE tr.object_id=o.object_id AND tr.parent_id=t.object_id)) OR sm.definition IS NULL OR sm.definition LIKE N'%'+t.name+N'%' OR sm.definition LIKE N'%sp_executesql%' OR sm.definition LIKE N'%EXEC(%' OR sm.definition LIKE N'%EXEC (%' OR sm.definition LIKE N'%EXECUTE(%' OR sm.definition LIKE N'%EXECUTE (%' OR sm.definition LIKE N'%EXEC @%' OR sm.definition LIKE N'%EXECUTE @%');
IF EXISTS(SELECT 1 FROM #CurrentModules WHERE Disposition='Unclassified')
   OR EXISTS(SELECT ReferencingSchema,ReferencingObject,ReferencingType,ReferencedEntityName,ReferencedColumnName,DefinitionHash,TriggerMetadata,DiscoveryMethod,Disposition FROM #CurrentModules EXCEPT SELECT ReferencingSchema,ReferencingObject,ReferencingType,ReferencedEntityName,ReferencedColumnName,DefinitionHash,TriggerMetadata,DiscoveryMethod,Disposition FROM dbo.GuidIdentityPrepareModuleDependency)
   OR EXISTS(SELECT ReferencingSchema,ReferencingObject,ReferencingType,ReferencedEntityName,ReferencedColumnName,DefinitionHash,TriggerMetadata,DiscoveryMethod,Disposition FROM dbo.GuidIdentityPrepareModuleDependency EXCEPT SELECT ReferencingSchema,ReferencingObject,ReferencingType,ReferencedEntityName,ReferencedColumnName,DefinitionHash,TriggerMetadata,DiscoveryMethod,Disposition FROM #CurrentModules)
    THROW 51000, 'SQL module dependency inventory is unknown or changed after Prepare.', 1;

DECLARE @parent sysname,@column sysname,@target sysname;
IF EXISTS(SELECT ParentEntityName,ColumnName FROM @ReferenceColumns GROUP BY ParentEntityName,ColumnName HAVING COUNT(DISTINCT ReferencedEntityName)>1)
    THROW 51000, 'A scalar reference column has conflicting prepared targets.', 1;
DECLARE reference_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT DISTINCT ParentEntityName,ColumnName,ReferencedEntityName FROM @ReferenceColumns;
OPEN reference_cursor; FETCH NEXT FROM reference_cursor INTO @parent,@column,@target;
WHILE @@FETCH_STATUS=0
BEGIN
    IF NOT EXISTS(SELECT 1 FROM sys.columns c JOIN sys.tables t ON t.object_id=c.object_id WHERE t.name=@parent AND SCHEMA_NAME(t.schema_id)=N'dbo' AND c.name=@column)
        THROW 51000, 'A declared identity reference column is missing from the current schema.', 1;
    SET @sql=N'IF (SELECT COUNT_BIG(*) FROM dbo.'+QUOTENAME(@parent)+N' WHERE '+QUOTENAME(@column)+N' IS NOT NULL)<>(SELECT COUNT_BIG(*) FROM dbo.GuidIdentityPrepareReferenceMap WHERE ParentEntityName=@p AND ColumnName=@c) THROW 51000, ''Identity reference count changed after Prepare.'', 1;'
      +N' IF EXISTS(SELECT 1 FROM dbo.'+QUOTENAME(@parent)+N' x LEFT JOIN dbo.GuidIdentityPrepareReferenceMap r ON r.ParentEntityName=@p AND r.ParentLegacyId=CONVERT(bigint,x.Id) AND r.ColumnName=@c LEFT JOIN dbo.GuidIdentityPrepareMap m ON m.EntityName=@t AND m.LegacyId=CONVERT(bigint,x.'+QUOTENAME(@column)+N') WHERE x.'+QUOTENAME(@column)+N' IS NOT NULL AND (r.ParentLegacyId IS NULL OR m.LegacyId IS NULL OR r.ReferencedEntityName<>@t OR r.ReferencedLegacyId<>CONVERT(bigint,x.'+QUOTENAME(@column)+N') OR r.GuidId<>m.GuidId)) THROW 51000, ''Identity reference differs from prepared source and target.'', 1;'
      +N' IF EXISTS(SELECT 1 FROM dbo.GuidIdentityPrepareReferenceMap r LEFT JOIN dbo.'+QUOTENAME(@parent)+N' x ON CONVERT(bigint,x.Id)=r.ParentLegacyId WHERE r.ParentEntityName=@p AND r.ColumnName=@c AND (x.Id IS NULL OR x.'+QUOTENAME(@column)+N' IS NULL OR CONVERT(bigint,x.'+QUOTENAME(@column)+N')<>r.ReferencedLegacyId)) THROW 51000, ''Prepared identity reference is stale.'', 1;';
    EXEC sys.sp_executesql @sql,N'@p sysname,@c sysname,@t sysname',@p=@parent,@c=@column,@t=@target;
    FETCH NEXT FROM reference_cursor INTO @parent,@column,@target;
END;
CLOSE reference_cursor; DEALLOCATE reference_cursor;

-- Verify every polymorphic audit target still resolves to the exact staged entity/key pair.
DECLARE @AuditTargetMap table(TargetType sysname PRIMARY KEY,EntityName sysname NOT NULL);
INSERT @AuditTargetMap VALUES
(N'Account',N'Accounts'),(N'Company',N'Companies'),(N'CompanyModule',N'CompanyModule'),(N'Conversation',N'Conversations'),(N'ConversationParticipant',N'ConversationParticipants'),(N'Customer',N'Customers'),(N'CustomerRequest',N'CustomerRequests'),(N'CustomerRequestEvaluation',N'CustomerRequestEvaluations'),(N'CustomerRequestMessage',N'CustomerRequestMessages'),(N'CustomerRequestMessageDocument',N'CustomerRequestDocuments'),(N'CustomerRequestStatus',N'CustomerRequestStatuses'),(N'Document',N'Documents'),(N'FAQ',N'FAQs'),(N'Guide',N'Guides'),(N'MenuItem',N'MenuItems'),(N'Module',N'Modules'),(N'ModuleExpert',N'ModuleExperts'),(N'OrganizationInfo',N'OrganizationInfos'),(N'Project',N'Projects'),(N'ProjectModule',N'ProjectModule'),(N'RequestHistory',N'RequestHistories'),(N'RequestSubject',N'RequestSubjects'),(N'Role',N'Roles'),(N'RolePermission',N'RolePermissions'),(N'RolePermissionAction',N'RolePermissionActions'),(N'User',N'Users'),(N'UserProject',N'UserProjects'),(N'UserRole',N'UserRoles');
IF (SELECT COUNT_BIG(*) FROM dbo.AuditLogs WHERE TargetId IS NOT NULL)<>(SELECT COUNT_BIG(*) FROM dbo.GuidIdentityPrepareReferenceMap WHERE ParentEntityName=N'AuditLogs' AND ColumnName=N'TargetId')
    THROW 51000, 'Audit target count changed after Prepare.', 1;
IF EXISTS(SELECT 1 FROM dbo.AuditLogs a LEFT JOIN @AuditTargetMap t ON t.TargetType=a.TargetType
          LEFT JOIN dbo.GuidIdentityPrepareReferenceMap r ON r.ParentEntityName=N'AuditLogs' AND r.ParentLegacyId=CONVERT(bigint,a.Id) AND r.ColumnName=N'TargetId'
          LEFT JOIN dbo.GuidIdentityPrepareMap m ON m.EntityName=t.EntityName AND m.LegacyId=CONVERT(bigint,a.TargetId)
          WHERE a.TargetId IS NOT NULL AND (t.EntityName IS NULL OR r.GuidId IS NULL OR m.GuidId IS NULL OR r.ReferencedEntityName<>t.EntityName OR r.ReferencedLegacyId<>CONVERT(bigint,a.TargetId) OR r.GuidId<>m.GuidId))
    THROW 51000, 'Audit target mapping changed after Prepare or contains an unresolved target.', 1;

-- Status-history values are text in the legacy schema. The source strings and resolved Guid must
-- remain exactly the ones prepared; all numeric status IDs are covered, not only 1..4.
IF (SELECT COUNT_BIG(*) FROM dbo.RequestHistories h CROSS APPLY(VALUES(h.OldValue),(h.NewValue)) v(OriginalValue)
    WHERE h.Action=2 AND NULLIF(LTRIM(RTRIM(v.OriginalValue)),N'') IS NOT NULL)
   <>(SELECT COUNT_BIG(*) FROM dbo.GuidIdentityPrepareTextReferenceMap)
    THROW 51000, 'Status-history reference count changed after Prepare.', 1;
IF EXISTS(SELECT 1 FROM dbo.RequestHistories h CROSS APPLY(VALUES(N'OldValue',h.OldValue),(N'NewValue',h.NewValue)) v(ColumnName,OriginalValue)
          LEFT JOIN dbo.GuidIdentityPrepareTextReferenceMap r ON r.ParentLegacyId=CONVERT(bigint,h.Id) AND r.ColumnName=v.ColumnName AND r.OriginalValue=v.OriginalValue
          LEFT JOIN dbo.GuidIdentityPrepareMap m ON m.EntityName=N'CustomerRequestStatuses'
             AND ((TRY_CONVERT(bigint,v.OriginalValue) IS NOT NULL AND m.LegacyId=TRY_CONVERT(bigint,v.OriginalValue))
               OR (TRY_CONVERT(uniqueidentifier,v.OriginalValue) IS NOT NULL AND (m.GuidId=TRY_CONVERT(uniqueidentifier,v.OriginalValue) OR m.OriginalPublicId=TRY_CONVERT(uniqueidentifier,v.OriginalValue))))
          WHERE h.Action=2 AND NULLIF(LTRIM(RTRIM(v.OriginalValue)),N'') IS NOT NULL
            AND (r.GuidId IS NULL OR m.GuidId IS NULL OR r.GuidId<>m.GuidId OR r.OriginalValue<>v.OriginalValue))
    THROW 51000, 'Status-history values differ from their prepared identity mapping.', 1;

-- Rebuild live FK inventory and compare both directions with Prepare; metadata or relation changes
-- invalidate this gate. This includes FKs that remain untouched and those to be converted.
CREATE TABLE #CurrentFks
(
    ParentSchema sysname NOT NULL,ParentEntityName sysname NOT NULL,ConstraintName sysname NOT NULL,ParentColumnName sysname NOT NULL,ParentSqlType sysname NOT NULL,
    ReferencedSchema sysname NOT NULL,ReferencedEntityName sysname NOT NULL,ReferencedColumnName sysname NOT NULL,ReferencedSqlType sysname NOT NULL,
    DeleteAction nvarchar(60) NOT NULL,UpdateAction nvarchar(60) NOT NULL,IsDisabled bit NOT NULL,IsNotTrusted bit NOT NULL,IsNotForReplication bit NOT NULL,Disposition varchar(24) NOT NULL,
    PRIMARY KEY(ParentSchema,ParentEntityName,ConstraintName,ParentColumnName,ReferencedColumnName)
);
INSERT #CurrentFks
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
IF EXISTS(SELECT ParentSchema,ParentEntityName,ConstraintName,ParentColumnName,ParentSqlType,ReferencedSchema,ReferencedEntityName,ReferencedColumnName,ReferencedSqlType,DeleteAction,UpdateAction,IsDisabled,IsNotTrusted,IsNotForReplication,Disposition FROM #CurrentFks
          EXCEPT SELECT ParentSchema,ParentEntityName,ConstraintName,ParentColumnName,ParentSqlType,ReferencedSchema,ReferencedEntityName,ReferencedColumnName,ReferencedSqlType,DeleteAction,UpdateAction,IsDisabled,IsNotTrusted,IsNotForReplication,Disposition FROM dbo.GuidIdentityPrepareForeignKeyInventory)
   OR EXISTS(SELECT ParentSchema,ParentEntityName,ConstraintName,ParentColumnName,ParentSqlType,ReferencedSchema,ReferencedEntityName,ReferencedColumnName,ReferencedSqlType,DeleteAction,UpdateAction,IsDisabled,IsNotTrusted,IsNotForReplication,Disposition FROM dbo.GuidIdentityPrepareForeignKeyInventory
          EXCEPT SELECT ParentSchema,ParentEntityName,ConstraintName,ParentColumnName,ParentSqlType,ReferencedSchema,ReferencedEntityName,ReferencedColumnName,ReferencedSqlType,DeleteAction,UpdateAction,IsDisabled,IsNotTrusted,IsNotForReplication,Disposition FROM #CurrentFks)
    THROW 51000, 'Foreign-key schema or metadata changed after Prepare.', 1;

-- Fingerprint current table column/default/computed/index/check/trigger metadata for the later
-- finalizer. The finalizer must compare this fingerprint and rerun exact data/reference checks
-- under its own transaction; this migration's marker alone never authorizes a stale cutover.
CREATE TABLE #SchemaRows(EntityName sysname NOT NULL,ItemKind sysname NOT NULL,ItemName sysname NOT NULL,Definition nvarchar(max) NULL);
INSERT #SchemaRows
SELECT t.name,N'COLUMN',c.name,CONCAT(ty.name,N'|',c.max_length,N'|',c.precision,N'|',c.scale,N'|',c.is_nullable,N'|',c.is_identity,N'|',c.is_computed,N'|',dc.definition,N'|',cc.definition,N'|',c.collation_name)
FROM sys.tables t JOIN @Entities e ON e.EntityName=t.name JOIN sys.columns c ON c.object_id=t.object_id
JOIN sys.types ty ON ty.user_type_id=c.user_type_id
LEFT JOIN sys.default_constraints dc ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
LEFT JOIN sys.computed_columns cc ON cc.object_id=c.object_id AND cc.column_id=c.column_id
WHERE SCHEMA_NAME(t.schema_id)=N'dbo';
INSERT #SchemaRows
SELECT t.name,N'INDEX',i.name,CONCAT(i.type_desc,N'|',i.is_unique,N'|',i.is_primary_key,N'|',i.is_unique_constraint,N'|',i.is_disabled,N'|',i.has_filter,N'|',i.filter_definition,N'|',i.fill_factor,N'|',i.ignore_dup_key,N'|',i.is_padded,N'|',i.allow_row_locks,N'|',i.allow_page_locks,N'|',i.optimize_for_sequential_key,N'|',ic.key_ordinal,N'|',ic.is_included_column,N'|',ic.is_descending_key,N'|',c.name,N'|',p.data_compression_desc)
FROM sys.tables t JOIN @Entities e ON e.EntityName=t.name JOIN sys.indexes i ON i.object_id=t.object_id AND i.index_id>0
JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
LEFT JOIN sys.partitions p ON p.object_id=i.object_id AND p.index_id=i.index_id
WHERE SCHEMA_NAME(t.schema_id)=N'dbo';
INSERT #SchemaRows
SELECT t.name,N'CHECK',cc.name,CONCAT(cc.definition,N'|',cc.is_disabled,N'|',cc.is_not_trusted)
FROM sys.tables t JOIN @Entities e ON e.EntityName=t.name JOIN sys.check_constraints cc ON cc.parent_object_id=t.object_id
WHERE SCHEMA_NAME(t.schema_id)=N'dbo';
INSERT #SchemaRows
SELECT t.name,N'TRIGGER',tr.name,CONCAT(tr.is_disabled,N'|',sm.definition)
FROM sys.tables t JOIN @Entities e ON e.EntityName=t.name JOIN sys.triggers tr ON tr.parent_id=t.object_id
LEFT JOIN sys.sql_modules sm ON sm.object_id=tr.object_id WHERE SCHEMA_NAME(t.schema_id)=N'dbo';
INSERT #SchemaRows
SELECT ParentEntityName,N'FOREIGN KEY',ConstraintName,
       CONCAT(ParentSchema,N'|',ParentColumnName,N'|',ParentSqlType,N'|',ReferencedSchema,N'|',ReferencedEntityName,N'|',ReferencedColumnName,N'|',ReferencedSqlType,N'|',DeleteAction,N'|',UpdateAction,N'|',IsDisabled,N'|',IsNotTrusted,N'|',IsNotForReplication,N'|',Disposition)
FROM dbo.GuidIdentityPrepareForeignKeyInventory;
INSERT #SchemaRows
SELECT ReferencingSchema,N'MODULE DEPENDENCY',CONCAT(ReferencingObject,N'|',ReferencedEntityName,N'|',ReferencedColumnName),
       CONCAT(ReferencingType,N'|',CONVERT(varchar(64),DefinitionHash,2),N'|',ISNULL(TriggerMetadata,N''),N'|',DiscoveryMethod,N'|',Disposition)
FROM dbo.GuidIdentityPrepareModuleDependency;
DECLARE @SchemaJson nvarchar(max),@SchemaFingerprint varbinary(32),@PreparedFingerprint varbinary(32);
SELECT @SchemaJson=(SELECT EntityName,ItemKind,ItemName,Definition FROM #SchemaRows ORDER BY EntityName,ItemKind,ItemName,Definition FOR JSON PATH,INCLUDE_NULL_VALUES);
SET @SchemaFingerprint=HASHBYTES('SHA2_256',CONVERT(varbinary(max),ISNULL(@SchemaJson,N'[]')));
DECLARE @PreparedSummary nvarchar(max);
SELECT @PreparedSummary=(SELECT
  (SELECT EntityName,SourceRowCount,PreparedRowCount FROM dbo.GuidIdentityPrepareRowCounts ORDER BY EntityName FOR JSON PATH) AS EntityCounts,
  (SELECT EntityName,COUNT_BIG(*) AS MappingCount,CHECKSUM_AGG(BINARY_CHECKSUM(LegacyId,GuidId,OriginalPublicId)) AS MappingChecksum FROM dbo.GuidIdentityPrepareMap GROUP BY EntityName ORDER BY EntityName FOR JSON PATH) AS IdentityCounts,
  (SELECT ParentEntityName,ColumnName,COUNT_BIG(*) AS ReferenceCount,CHECKSUM_AGG(BINARY_CHECKSUM(ParentLegacyId,ReferencedEntityName,ReferencedLegacyId,GuidId)) AS ReferenceChecksum FROM dbo.GuidIdentityPrepareReferenceMap GROUP BY ParentEntityName,ColumnName ORDER BY ParentEntityName,ColumnName FOR JSON PATH) AS ReferenceCounts,
  (SELECT COUNT_BIG(*) AS TextReferenceCount FROM dbo.GuidIdentityPrepareTextReferenceMap FOR JSON PATH) AS TextReferences,
  (SELECT COUNT_BIG(*) AS ForeignKeyCount FROM dbo.GuidIdentityPrepareForeignKeyInventory FOR JSON PATH) AS ForeignKeys,
  (SELECT COUNT_BIG(*) AS ModuleDependencyCount FROM dbo.GuidIdentityPrepareModuleDependency FOR JSON PATH) AS ModuleDependencies
FOR JSON PATH,WITHOUT_ARRAY_WRAPPER);
SET @PreparedFingerprint=HASHBYTES('SHA2_256',CONVERT(varbinary(max),ISNULL(@PreparedSummary,N'{}')));
INSERT dbo.GuidIdentityValidateGate(GateId,DatabaseName,ValidatedAtUtc,EntityCount,SourceRows,PreparedIds,PreparedReferences,PreparedTextReferences,ForeignKeyCount,ModuleDependencyCount,SchemaFingerprint,PreparedFingerprint)
SELECT 1,DB_NAME(),SYSUTCDATETIME(),(SELECT COUNT(*) FROM @Entities),
       (SELECT SUM(SourceRowCount) FROM dbo.GuidIdentityPrepareRowCounts),(SELECT COUNT_BIG(*) FROM dbo.GuidIdentityPrepareMap),
       (SELECT COUNT_BIG(*) FROM dbo.GuidIdentityPrepareReferenceMap),(SELECT COUNT_BIG(*) FROM dbo.GuidIdentityPrepareTextReferenceMap),
       (SELECT COUNT_BIG(*) FROM dbo.GuidIdentityPrepareForeignKeyInventory),(SELECT COUNT_BIG(*) FROM dbo.GuidIdentityPrepareModuleDependency),@SchemaFingerprint,@PreparedFingerprint;
""";
}
