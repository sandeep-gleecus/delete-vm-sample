/***************************************************************
**	Insert script for table TST_USER_COLLECTION
***************************************************************/
SET IDENTITY_INSERT TST_USER_COLLECTION ON; 

INSERT INTO TST_USER_COLLECTION
(
USER_COLLECTION_ID, NAME, ACTIVE_YN
)
VALUES
(
1, 'MyPage.MyProjects', 'N'
),
(
2, 'MyPage.MyTests', 'N'
),
(
3, 'MyPage.MyIncidentsOwner', 'N'
),
(
4, 'MyPage.MyIncidentsDetector', 'N'
),
(
5, 'MyPage.MyTasks', 'N'
),
(
6, 'MyPage.MySavedTestRuns', 'N'
),
(
7, 'MyPage.MyTestSets', 'N'
),
(
8, 'MyPage.MyRequirements', 'N'
),
(
9, 'MyPage.MySavedSearches', 'N'
),
(
10, 'Administration.ProjectListFilters', 'Y'
),
(
11, 'Administration.ProjectPagination', 'Y'
),
(
12, 'Administration.UserListFilters', 'Y'
),
(
13, 'Administration.UserPagination', 'Y'
),
(
14, 'Administration.ProjectGroupListFilters', 'Y'
),
(
15, 'Administration.ProjectGroupListPagination', 'Y'
),
(
16, 'ProjectMembershipAdd.Filters', 'Y'
),
(
17, 'ProjectMembershipAdd.Pagination', 'Y'
),
(
18, 'ProjectGroupMembershipAdd.Filters', 'Y'
),
(
19, 'ProjectGroupMembershipAdd.Pagination', 'Y'
),
(
20, 'MyPage.GeneralSettings', 'Y'
),
(
21, 'UserProfile.GeneralSettings', 'Y'
),
(
22, 'UserProfile.Action.Filter', 'Y'
),
(
23, 'Administration.UserRequests.Filters', 'Y'
),
(
24, 'Administration.UserRequests.General', 'Y'
),
(
25, 'Administration.EventLog.Filters', 'Y'
),
(
26, 'Administration.EventLog.General', 'Y'
),
(
27, 'CollapsiblePanel.State', 'Y'
),
(
28, 'TabControl.State', 'Y'
),
(
29, 'GuidedTours.State', 'Y'
),
(
30, 'ProjectGroup.PlanningBoard', 'Y'
),
(
31, 'ProjectGroup.ExpandedGroups', 'Y'
),
(
32, 'GroupIncidents.Filters', 'Y'
),
(
33, 'GroupIncidents.General', 'Y'
),
(
34, 'GroupReleases.Filters', 'Y'
),
(
35, 'GroupReleases.General', 'Y'
),
(
36, 'GroupIncidents.Columns', 'Y'
),
(
37, 'GroupReleases.Columns', 'Y'
),
(
38, 'Administration.ProjectTemplateListFilters', 'Y'
),
(
39, 'Administration.ProjectTemplateListPagination', 'Y'
),
(
40, 'ProjectGroupHome.General', 'Y'
),
(
41, 'Administration.PortfolioList.Filters', 'Y'
),
(
42, 'Administration.PortfolioList.General', 'Y'
),
(
43, 'MyPage.RecentWorkspaces', 'Y'
),
(
44, 'MyPage.RecentArtifacts', 'Y'
)
GO

SET IDENTITY_INSERT TST_USER_COLLECTION OFF; 

