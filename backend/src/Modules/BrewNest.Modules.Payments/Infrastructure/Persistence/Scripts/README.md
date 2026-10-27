# Payments migration scripts

`payments.sql` is the idempotent SQL Server deployment script generated from the module-owned migrations.

The second migration adds a nullable Sales order link to refunds, backfills rows with an original payment link, and leaves unmatched historical refunds unchanged. Do not drop or recreate the `payments` schema to apply it.
