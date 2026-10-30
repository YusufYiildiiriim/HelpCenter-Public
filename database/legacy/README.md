# SQL Server migration archive

This directory preserves the pre-PostgreSQL Entity Framework migration history as
historical reference only. It is deliberately outside the active .NET project and
must not be applied to a PostgreSQL database.

The active PostgreSQL migration line starts from a clean initial migration after
the provider and concurrency model are finalized.

Historical SQL Server dashboard procedures in this archive are not part of the
runtime. Dashboard statistics and reports now use EF Core read-model queries;
no PostgreSQL-specific function or view was introduced because no measured
performance need has been established.
