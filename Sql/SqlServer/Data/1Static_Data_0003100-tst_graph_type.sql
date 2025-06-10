/***************************************************************
**	Insert script for table TST_GRAPH_TYPE
***************************************************************/
SET IDENTITY_INSERT TST_GRAPH_TYPE ON; 

INSERT INTO TST_GRAPH_TYPE
(
GRAPH_TYPE_ID, NAME, IS_ACTIVE
)
VALUES
(
1, 'Date Range Graphs', 1
),
(
2, 'Summary Graphs', 1
),
(
3, 'Snapshot Graphs', 1
),
(
4, 'Custom Graphs', 1
)
GO

SET IDENTITY_INSERT TST_GRAPH_TYPE OFF; 

