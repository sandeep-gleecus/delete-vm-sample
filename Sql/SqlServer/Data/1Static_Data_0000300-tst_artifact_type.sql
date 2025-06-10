/***************************************************************
**	Insert script for table TST_ARTIFACT_TYPE
***************************************************************/
SET IDENTITY_INSERT TST_ARTIFACT_TYPE ON; 

INSERT INTO TST_ARTIFACT_TYPE
(
ARTIFACT_TYPE_ID, NAME, PREFIX, IS_NOTIFY, IS_ACTIVE, IS_DATA_SYNC, IS_ATTACHMENTS, IS_CUSTOM_PROPERTIES, IS_GLOBAL_ITEM
)
VALUES
(
1, 'Requirement', 'RQ', 1, 1, 1, 1, 1, 0
),
(
2, 'Test Case', 'TC', 1, 1, 1, 1, 1, 0
),
(
3, 'Incident', 'IN', 1, 1, 1, 1, 1, 0
),
(
4, 'Release', 'RL', 1, 1, 1, 1, 1, 0
),
(
5, 'Test Run', 'TR', 0, 1, 0, 1, 1, 0
),
(
6, 'Task', 'TK', 1, 1, 1, 1, 1, 0
),
(
7, 'Test Step', 'TS', 0, 1, 0, 1, 1, 0
),
(
8, 'Test Set', 'TX', 1, 1, 0, 1, 1, 0
),
(
9, 'Automation Host', 'AH', 0, 1, 0, 1, 1, 0
),
(
10, 'Automation Engine', 'AE', 0, 0, 0, 0, 0, 0
),
(
-1, 'Project', 'PR', 0, 0, 0, 0, 0, 1
),
(
11, 'Placeholder', 'PL', 0, 1, 0, 0, 0, 0
),
(
12, 'Requirement Step', 'RS', 0, 1, 0, 0, 0, 0
),
(
13, 'Document', 'DC', 1, 1, 0, 0, 1, 0
),
(
-3, 'User', 'US', 0, 0, 0, 0, 0, 1
),
(
-12, 'Program', 'PG', 0, 0, 0, 0, 0, 1
),
(
14, 'Risk', 'RK', 1, 1, 1, 1, 1, 0
),
(
15, 'Risk Mitigation', 'RM', 0, 1, 0, 0, 0, 0
),
(
16,	'All Project Audit Trail',	'HL',	0,	0,	1	,1	,0,	1
),
(
19,	'All Audit Trail',	'AL',	0,	0	,1	,1,	0,	1
),
(
20,	'All Admin Audit Trail',	'AT',	0,	0,	1	,1	,0,	1
),
(
21,	'Portfolios',	'PF',	0	,0,	0,	0,	0,	1
),
(
22,	'All User Audit Trail',	'UT',0	,0	,1	,1,	0	,1
),
(
23,	'ProjectTemplate',	'PT',0	,0,	0,	0,	0,	1
),
(
24,	'Project Role',	'PO',	0	,0,	0	,0	,0,	1
),
(
25,	'Project Role Permission',	'PP',	0,	0,	0	,0	,0	,1
),
(
26,	'Login Provider',	'LP', 	0,	0,	0,	0	,0,	1
),
(
27,	'File Type Icon',	'FT',	0,	0	,0	,0	,0	,1
),
(
28,	'SourceCode',	'SC',	0,	0,	0,	0,	0,	1
),
(
29,	'DataSync',	'DS',	0,	0,	0,	0,	0,	1
),
(
30,'Report',	'RP',	0,	0,	0,	0	,0,	1
),
(
31,	'Graph',	'GP',	0,	0,	0,	0	,0	,1
),
(
32,	'ReportSectionInstance',	'RS',0,	0,	0,	0,	0,	1
),
(
33,	'ReportCustomSection',	'RC',	0,	0,	0,	0,	0	,1
),
(
34,	'SystemUsageReport',	'SU',	0,	0,	1,	1,	0,	1
),
(
35,	'Project User Group',	'PG',	0,	0,	1,	1,	0,	1
),
(
36,	'Project Tag Frequency',	'PT',	0,	1,	1,	1,	0,	1
),
(
37,	'DocumentDiscussion',	'DD',	0,	1,	1,	1,	0,	1
),
(
38,	'ProjectBaseline',	'PB',	0,	1,	1,	1,	0,	1
),
(
39,	'ArtifactLink',	'AL',	0,	1,	1,	1,	0,	1
),
(
40,	'ReleaseDiscussion',	'RD',	0,	1,	1,	1,	0,	1
),
(
41,	'TestCaseDiscussion',	'TD',	0,	1,	1,	1,	0,	1
),
(
42,	'TaskDiscussion',	'TS',	0,	1,	1,	1,	0,	1
),
(
43,	'RiskDiscussion',	'RR',	0,	1,	1,	1,	0,	1
),
(
44,	'RequirementDiscussion',	'RQ',	0,	1,	1,	1,	0,	1
),
(
45,	'TestSetDiscussion',	'TS',	0,	1,	1,	1,	0,	1
),
(
47,	'HistoryDiscussion',	'TS',	0,	0,	1,	1,	0,	1
),
(
48,	'IncidentResolution',	'IR',	0,	1,	1,	1,	0,	1
),
(
49,	'ProjectAuditTrail',	'SP',	0,	0,	1,	1,	0,	1
),
(
50,	'AuditTrail',	'ST',	0,	0,	1,	1,	0,	1
),
(
51,	'AdminAuditTrail',	'SA',	0,	0,	1,	1,	0,	1
),
(
52,	'UserAudirTrail',	'SU',	0,	0,	1,	1,	0,	1
),
(
53,	'TestCaseParameter',	'TP',	0,	1,	1,	1,	0,	1
),
(
54,	'TestSetParameter',	'SP',	0,	1,	1,	1,	0,	1
),
(
55,	'Configuration',	'TC',	0,	1,	1,	1,	0,	1
),
(
56,	'ReleaseTestCase',	'RT',	0,	1,	1,	1,	0,	1
),
(
57,	'Event',	'EV',	0,	0,	0,	0,	0,	1
),
(
58,	'DocumentVersion',	'DV',	0,	1,	1,	1,	0,	1
),
(
59,	'TestCaseSignature',	'TS',	0,	1,	1,	1,	0,	1
),
(
60,	'RequirementSignature',	'RS',	0,	1,	1,	1,	0,	1
),
(
61,	'ReleaseSignature',	'RI',	0,	1,	1,	1,	0,	1
),
(
62,	'DocumentSignature',	'DS',	0,	1,	1,	1,	0,	1
),
(
63,	'IncidentSignature',	'IS',	0,	1,	1,	1,	0,	1
),
(
64,	'RiskSignature',	'SR',	0,	1,	1,	1,	0,	1
),
(
65,	'TaskSignature',	'TS',	0,	1,	1,	1,	0,	1
)

GO

SET IDENTITY_INSERT TST_ARTIFACT_TYPE OFF; 

