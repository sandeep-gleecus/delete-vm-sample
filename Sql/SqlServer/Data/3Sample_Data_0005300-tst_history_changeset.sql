/***************************************************************
**	Insert script for table TST_HISTORY_CHANGESET
***************************************************************/
SET IDENTITY_INSERT TST_HISTORY_CHANGESET ON; 

INSERT INTO TST_HISTORY_CHANGESET
(
CHANGESET_ID, USER_ID, ARTIFACT_TYPE_ID, ARTIFACT_ID, CHANGE_DATE, CHANGETYPE_ID, PROJECT_ID, ARTIFACT_DESC
)
VALUES
(
1, 3, 4, 1, DATEADD(minute, -215224, SYSUTCDATETIME()), 1, 1, 'Library System Release 1'
),
(
2, 3, 2, 2, DATEADD(minute, -213735, SYSUTCDATETIME()), 1, 1, 'Ability to create new book'
),
(
3, 3, 7, 3, DATEADD(minute, -210828, SYSUTCDATETIME()), 1, 1, 'User clicks link to create author'
),
(
4, 3, 1, 4, DATEADD(minute, -216584, SYSUTCDATETIME()), 1, 1, 'Ability to add new books to the system'
),
(
5, 2, 3, 6, DATEADD(minute, -206503, SYSUTCDATETIME()), 1, 1, 'The book listing screen doesn''t sort'
),
(
6, 3, 3, 7, DATEADD(minute, -206503, SYSUTCDATETIME()), 1, 1, 'Cannot add a new book to the system'
),
(
7, 2, 3, 6, DATEADD(minute, -206728, SYSUTCDATETIME()), 1, 1, 'The book listing screen doesn''t sort'
),
(
8, 2, 3, 7, DATEADD(minute, -200965, SYSUTCDATETIME()), 1, 1, 'Cannot add a new book to the system'
),
(
9, 2, 2, 2, DATEADD(minute, -211126, SYSUTCDATETIME()), 1, 1, 'Ability to create new book'
),
(
10, 2, 4, 1, DATEADD(minute, -118828, SYSUTCDATETIME()), 1, 1, 'Library System Release 1'
),
(
11, 2, 7, 3, DATEADD(minute, -210784, SYSUTCDATETIME()), 1, 1, 'User clicks link to create author'
),
(
12, 2, 1, 4, DATEADD(minute, -207899, SYSUTCDATETIME()), 1, 1, 'Ability to add new books to the system'
),
(
13, 2, 3, 7, DATEADD(minute, -200606, SYSUTCDATETIME()), 1, 1, 'Cannot add a new book to the system'
),
(
14, 5, 4, 20, SYSUTCDATETIME(), 3, 4, 'FIN Release 1.0'
),
(
15, 5, 4, 21, SYSUTCDATETIME(), 3, 4, 'FIN Release 1.0 Patch 1'
),
(
16, 5, 4, 22, SYSUTCDATETIME(), 3, 4, 'FIN Release 1.0 Patch 2'
),
(
17, 5, 4, 23, SYSUTCDATETIME(), 3, 4, 'FIN Release 1.0 Patch 3'
),
(
18, 8, 4, 24, SYSUTCDATETIME(), 3, 4, 'FIN Release 1.1'
),
(
19, 8, 4, 25, SYSUTCDATETIME(), 3, 4, 'FIN Release 1.1 Patch 1'
),
(
20, 8, 4, 26, SYSUTCDATETIME(), 3, 4, 'FIN Release 1.1 Patch 2'
),
(
21, 1, 1, 37, SYSUTCDATETIME(), 3, 4, 'General Ledger'
),
(
22, 1, 1, 38, SYSUTCDATETIME(), 3, 4, 'Chart of Accounts'
),
(
23, 1, 1, 39, SYSUTCDATETIME(), 3, 4, 'Admins can create new account in the expense, income, asset and liability categories'
),
(
24, 1, 1, 40, SYSUTCDATETIME(), 3, 4, 'Admins can edit and update existing account entries, including name and account number'
),
(
25, 1, 1, 41, SYSUTCDATETIME(), 3, 4, 'Journal Entries'
),
(
26, 1, 1, 42, SYSUTCDATETIME(), 3, 4, 'Users can add new journal entries with debit and credit lines'
),
(
27, 1, 1, 43, SYSUTCDATETIME(), 3, 4, 'Users can clone and edit existing journal entries'
),
(
28, 1, 1, 44, SYSUTCDATETIME(), 3, 4, 'Managers can void existing journal entries with a reason'
),
(
29, 1, 1, 45, SYSUTCDATETIME(), 3, 4, 'Accounts Payable'
),
(
30, 1, 1, 46, SYSUTCDATETIME(), 3, 4, 'Vendors'
),
(
31, 1, 1, 47, SYSUTCDATETIME(), 3, 4, 'Admins can create and modify vendors in the system'
),
(
32, 1, 1, 48, SYSUTCDATETIME(), 3, 4, 'Admins can create and modify vendor types and categories'
),
(
33, 1, 1, 49, SYSUTCDATETIME(), 3, 4, 'Admins can associate vendors with a type and/or category'
),
(
34, 1, 1, 50, SYSUTCDATETIME(), 3, 4, 'Purchases'
),
(
35, 1, 1, 51, SYSUTCDATETIME(), 3, 4, 'Users can create new purchase orders in the system'
),
(
36, 1, 1, 52, SYSUTCDATETIME(), 3, 4, 'Users can edit existing purchase orders in the system, including voiding them'
),
(
37, 1, 1, 53, SYSUTCDATETIME(), 3, 4, 'Users can enter bills associated with purchase orders'
),
(
38, 1, 1, 54, SYSUTCDATETIME(), 3, 4, 'Users can receive goods associated with purchase orders'
),
(
39, 1, 1, 55, SYSUTCDATETIME(), 3, 4, 'Managers can pay bills associated with received purchase orders'
),
(
40, 1, 1, 56, SYSUTCDATETIME(), 3, 4, 'Accounts Receivable'
),
(
41, 1, 1, 57, SYSUTCDATETIME(), 3, 4, 'Customers'
),
(
42, 1, 1, 58, SYSUTCDATETIME(), 3, 4, 'Admins can create and modify customers in the system'
),
(
43, 1, 1, 59, SYSUTCDATETIME(), 3, 4, 'Admins can create and modify customer types and categories'
),
(
44, 1, 1, 60, SYSUTCDATETIME(), 3, 4, 'Admins can associate customers with a type and/or category'
),
(
45, 1, 1, 61, SYSUTCDATETIME(), 3, 4, 'Quotes'
),
(
46, 1, 1, 62, SYSUTCDATETIME(), 3, 4, 'Users can create price quotes associated with a customer'
),
(
47, 1, 1, 63, SYSUTCDATETIME(), 3, 4, 'Users can approve a quote which converts it into a sales order'
),
(
48, 1, 1, 64, SYSUTCDATETIME(), 3, 4, 'Users can create sales orders associated with a customer and optionally a quote'
),
(
49, 1, 1, 65, SYSUTCDATETIME(), 3, 4, 'Invoices'
),
(
50, 1, 1, 66, SYSUTCDATETIME(), 3, 4, 'Users can create invoices for a customer, optionally linked to quote or sales order'
),
(
51, 1, 1, 67, SYSUTCDATETIME(), 3, 4, 'Users can automatically covert an existing quote or sales order into an invoice'
),
(
52, 1, 1, 68, SYSUTCDATETIME(), 3, 4, 'Managers can issue a credit memo against an existing invoice'
),
(
53, 1, 1, 69, SYSUTCDATETIME(), 3, 4, 'Payments'
),
(
54, 1, 1, 70, SYSUTCDATETIME(), 3, 4, 'Users can receive payments against an existing invoice'
),
(
55, 1, 1, 71, SYSUTCDATETIME(), 3, 4, 'Managers can write off an invoice partially or completely with an appropriate justification'
),
(
56, 1, 1, 72, SYSUTCDATETIME(), 3, 4, 'Managers can adjust the payment terms for an invoice with an appropriate justification'
),
(
57, 1, 1, 73, SYSUTCDATETIME(), 3, 4, 'Reporting'
),
(
58, 1, 1, 74, SYSUTCDATETIME(), 3, 4, 'Financial Statements'
),
(
59, 1, 1, 75, SYSUTCDATETIME(), 3, 4, 'Income Statement'
),
(
60, 1, 1, 76, SYSUTCDATETIME(), 3, 4, 'Balance Sheet'
),
(
61, 1, 1, 77, SYSUTCDATETIME(), 3, 4, 'Cash Flow Statement'
),
(
62, 1, 1, 78, SYSUTCDATETIME(), 3, 4, 'Sales Reports'
),
(
63, 1, 1, 79, SYSUTCDATETIME(), 3, 4, 'Sales by Item'
),
(
64, 1, 1, 80, SYSUTCDATETIME(), 3, 4, 'Sales by Country'
),
(
65, 5, 4, 27, SYSUTCDATETIME(), 3, 5, 'HR Release 1.0'
),
(
66, 5, 4, 28, SYSUTCDATETIME(), 3, 5, 'HR Release 1.0 Patch 1'
),
(
67, 5, 4, 29, SYSUTCDATETIME(), 3, 5, 'HR Release 1.0 Patch 2'
),
(
68, 8, 4, 30, SYSUTCDATETIME(), 3, 5, 'HR Release 1.1'
),
(
69, 8, 4, 31, SYSUTCDATETIME(), 3, 5, 'HR Release 1.1 Patch 1'
),
(
70, 8, 4, 32, SYSUTCDATETIME(), 3, 5, 'HR Release 1.1 Patch 2'
),
(
71, 8, 4, 34, SYSUTCDATETIME(), 3, 5, 'HR Release 1.1 Patch 3'
),
(
72, 1, 1, 81, SYSUTCDATETIME(), 3, 5, 'Employee Administration'
),
(
73, 1, 1, 82, SYSUTCDATETIME(), 3, 5, 'Can add new employees to the system with their tax ID and personal information'
),
(
74, 1, 1, 83, SYSUTCDATETIME(), 3, 5, 'Can update an existing employee with audit tracking of changes made'
),
(
75, 1, 1, 84, SYSUTCDATETIME(), 3, 5, 'Can terminate an employee with an appropriate termination code and reason'
),
(
76, 1, 1, 85, SYSUTCDATETIME(), 3, 5, 'Payroll Management'
),
(
77, 1, 1, 86, SYSUTCDATETIME(), 3, 5, 'Employee Payroll data'
),
(
78, 1, 1, 87, SYSUTCDATETIME(), 3, 5, 'Can assign a base salary, bonus and overtime to employees'
),
(
79, 1, 1, 88, SYSUTCDATETIME(), 3, 5, 'Can assign appropriate tax locality, rates, allowances and family status'
),
(
80, 1, 1, 89, SYSUTCDATETIME(), 3, 5, 'Can store an employee''s banking information for deposit of pay'
),
(
81, 1, 1, 90, SYSUTCDATETIME(), 3, 5, 'Payroll Transaction Processing'
),
(
82, 1, 1, 91, SYSUTCDATETIME(), 3, 5, 'Can process a regular payroll run for base salary and overtime'
),
(
83, 1, 1, 92, SYSUTCDATETIME(), 3, 5, 'Can process a bonus payroll run'
),
(
84, 1, 1, 93, SYSUTCDATETIME(), 3, 5, 'Electronically deposit the net salary into the employee''s bank account'
),
(
85, 1, 1, 94, SYSUTCDATETIME(), 3, 5, 'Payroll Taxes'
),
(
86, 1, 1, 95, SYSUTCDATETIME(), 3, 5, 'Can deposit appropriate federal, state and local taxes into appropriate accounts'
),
(
87, 1, 1, 96, SYSUTCDATETIME(), 3, 5, 'Can deposit unemployment insurance into appropriate accounts'
),
(
88, 1, 1, 97, SYSUTCDATETIME(), 3, 5, 'Deductions'
),
(
89, 1, 1, 98, SYSUTCDATETIME(), 3, 5, 'Deposit pre-tax deductions for health insurance into benefit provider''s account'
),
(
90, 1, 1, 99, SYSUTCDATETIME(), 3, 5, 'Deposit pre-tax deductions for retirement fund into custodian accounts'
),
(
91, 1, 1, 100, SYSUTCDATETIME(), 3, 5, 'Time and Expenses'
),
(
92, 1, 1, 101, SYSUTCDATETIME(), 3, 5, 'Time Tracking'
),
(
93, 1, 1, 102, SYSUTCDATETIME(), 3, 5, 'Can log time spent on different activities and job codes'
),
(
94, 1, 1, 103, SYSUTCDATETIME(), 3, 5, 'Can segregate time into billable and non-billable time for the same job code'
),
(
95, 1, 1, 104, SYSUTCDATETIME(), 3, 5, 'Expense Tracking'
),
(
96, 1, 1, 105, SYSUTCDATETIME(), 3, 5, 'Can record reimburseable employee expenses against specific job codes'
),
(
97, 1, 1, 106, SYSUTCDATETIME(), 3, 5, 'Can segerate expenses into billable and non-billable for the same job code'
),
(
98, 1, 1, 107, SYSUTCDATETIME(), 3, 5, 'Deposit expense reimbursements into employee''s bank account'
),
(
99, 1, 1, 108, SYSUTCDATETIME(), 3, 5, 'Benefits Administration'
),
(
100, 1, 1, 109, SYSUTCDATETIME(), 3, 5, 'Medical Insurance'
),
(
101, 1, 1, 110, SYSUTCDATETIME(), 3, 5, 'Can enroll employees in medical insurance for self and family members'
),
(
102, 1, 1, 111, SYSUTCDATETIME(), 3, 5, 'Can select different medical plan options'
),
(
103, 1, 1, 112, SYSUTCDATETIME(), 3, 5, 'Specify company premium sharing options by plan'
),
(
104, 1, 1, 113, SYSUTCDATETIME(), 3, 5, 'Tools to modify and end coverage for employees'
),
(
105, 1, 1, 114, SYSUTCDATETIME(), 3, 5, 'Retirement'
),
(
106, 1, 1, 115, SYSUTCDATETIME(), 3, 5, 'Can enroll employees in retirement plan'
),
(
107, 1, 1, 116, SYSUTCDATETIME(), 3, 5, 'Can manage employee payroll contribution percentages'
),
(
108, 1, 1, 117, SYSUTCDATETIME(), 3, 5, 'Ability to configure company matching contributions'
),
(
109, 1, 1, 118, SYSUTCDATETIME(), 3, 5, 'Can change and remove employee enrollments'
),
(
110, 1, 1, 119, SYSUTCDATETIME(), 3, 5, 'Other'
),
(
111, 1, 1, 120, SYSUTCDATETIME(), 3, 5, 'Tracking life events and open seasons'
),
(
112, 1, 1, 121, SYSUTCDATETIME(), 3, 5, 'Employee Self-Service Benefits'
),
(
113, 1, 1, 122, SYSUTCDATETIME(), 3, 5, 'Employees can see summary of company benefits'
),
(
114, 1, 1, 123, SYSUTCDATETIME(), 3, 5, 'User Management'
),
(
115, 1, 1, 124, SYSUTCDATETIME(), 3, 5, 'Employees can login with company email address and password'
),
(
116, 1, 1, 125, SYSUTCDATETIME(), 3, 5, 'Employees can manage their communication preferences'
),
(
117, 1, 1, 126, SYSUTCDATETIME(), 3, 5, 'Retirement'
),
(
118, 1, 1, 127, SYSUTCDATETIME(), 3, 5, 'Employees can change retirement mutual fund allocations'
),
(
119, 1, 1, 128, SYSUTCDATETIME(), 3, 5, 'Employees can change retirement contribution percentages'
),
(
120, 1, 1, 129, SYSUTCDATETIME(), 3, 5, 'Medical Insurance'
),
(
121, 1, 1, 130, SYSUTCDATETIME(), 3, 5, 'Employees can login to verify coverage information'
),
(
122, 1, 1, 131, SYSUTCDATETIME(), 3, 5, 'Employees can search for in-network providers'
),
(
123, 1, 1, 132, SYSUTCDATETIME(), 3, 5, 'Employee Performance Management'
),
(
124, 1, 1, 133, SYSUTCDATETIME(), 3, 5, 'Annual Reviews'
),
(
125, 1, 1, 134, SYSUTCDATETIME(), 3, 5, 'Employees can download annual review templates and checklists'
),
(
126, 1, 1, 135, SYSUTCDATETIME(), 3, 5, 'Employees can enter annual reviews'
),
(
127, 1, 1, 136, SYSUTCDATETIME(), 3, 5, 'Managers can review annual reviews and rate employees'
),
(
128, 1, 1, 137, SYSUTCDATETIME(), 3, 5, 'Underperformance Tracking'
),
(
129, 1, 1, 138, SYSUTCDATETIME(), 3, 5, 'Managers can initiate a ''get-well'' plan for an employee'
),
(
130, 1, 1, 139, SYSUTCDATETIME(), 3, 5, 'Managers can record interactions and next steps'
),
(
131, 8, 4, 35, SYSUTCDATETIME(), 3, 6, 'Website Initial Lanch'
),
(
132, 5, 4, 36, SYSUTCDATETIME(), 3, 6, 'Sprint 1'
),
(
133, 5, 4, 37, SYSUTCDATETIME(), 3, 6, 'Sprint 2'
),
(
134, 5, 4, 38, SYSUTCDATETIME(), 3, 6, 'Sprint 3'
),
(
135, 5, 4, 39, SYSUTCDATETIME(), 3, 6, 'Sprint 4'
),
(
136, 5, 4, 40, SYSUTCDATETIME(), 3, 6, 'Sprint 5'
),
(
137, 8, 4, 41, SYSUTCDATETIME(), 3, 6, 'Website Second Phase'
),
(
138, 5, 4, 42, SYSUTCDATETIME(), 3, 6, 'Sprint 1'
),
(
139, 5, 4, 43, SYSUTCDATETIME(), 3, 6, 'Sprint 2'
),
(
140, 5, 4, 44, SYSUTCDATETIME(), 3, 6, 'Sprint 3'
),
(
141, 5, 4, 45, SYSUTCDATETIME(), 3, 6, 'Sprint 4'
),
(
142, 5, 4, 46, SYSUTCDATETIME(), 3, 6, 'Sprint 5'
),
(
143, 1, 1, 140, SYSUTCDATETIME(), 3, 6, 'Home Page'
),
(
144, 1, 1, 141, SYSUTCDATETIME(), 3, 6, 'Hero image that introduces company and value proposition'
),
(
145, 1, 1, 142, SYSUTCDATETIME(), 3, 6, 'Section introductions that provide teasers about our products and services'
),
(
146, 1, 1, 143, SYSUTCDATETIME(), 3, 6, 'News widget that displays the top 5 articles and blog entries'
),
(
147, 1, 1, 144, SYSUTCDATETIME(), 3, 6, 'Main Navigation'
),
(
148, 1, 1, 145, SYSUTCDATETIME(), 3, 6, 'Primary navigation for each of the website sections'
),
(
149, 1, 1, 146, SYSUTCDATETIME(), 3, 6, 'Footer that contains legal notices and lesser user links'
),
(
150, 1, 1, 147, SYSUTCDATETIME(), 3, 6, 'Sub-section navigation for the more complex sections (products and services)'
),
(
151, 1, 1, 148, SYSUTCDATETIME(), 3, 6, 'Login control that is used to login to the CMS'
),
(
152, 1, 1, 149, SYSUTCDATETIME(), 3, 6, 'Company Information'
),
(
153, 1, 1, 150, SYSUTCDATETIME(), 3, 6, 'Company profile and legal information'
),
(
154, 1, 1, 151, SYSUTCDATETIME(), 3, 6, 'Media kit and marketing assets'
),
(
155, 1, 1, 152, SYSUTCDATETIME(), 3, 6, 'Contact-us form'
),
(
156, 1, 1, 153, SYSUTCDATETIME(), 3, 6, 'List of upcoming events'
),
(
157, 1, 1, 154, SYSUTCDATETIME(), 3, 6, 'Product Information'
),
(
158, 1, 1, 155, SYSUTCDATETIME(), 3, 6, 'Product #1 Information'
),
(
159, 1, 1, 156, SYSUTCDATETIME(), 3, 6, 'Product overview page'
),
(
160, 1, 1, 157, SYSUTCDATETIME(), 3, 6, 'Product testimonials page'
),
(
161, 1, 1, 158, SYSUTCDATETIME(), 3, 6, 'Product benefits and details feature list'
),
(
162, 1, 1, 159, SYSUTCDATETIME(), 3, 6, 'Product pricing and business information'
),
(
163, 1, 1, 160, SYSUTCDATETIME(), 3, 6, 'Product #2 Information'
),
(
164, 1, 1, 161, SYSUTCDATETIME(), 3, 6, 'Product overview page'
),
(
165, 1, 1, 162, SYSUTCDATETIME(), 3, 6, 'Product testimonials page'
),
(
166, 1, 1, 163, SYSUTCDATETIME(), 3, 6, 'Product benefits and details feature list'
),
(
167, 1, 1, 164, SYSUTCDATETIME(), 3, 6, 'Product pricing and business information'
),
(
168, 1, 1, 165, SYSUTCDATETIME(), 3, 6, 'Product #3 Information'
),
(
169, 1, 1, 166, SYSUTCDATETIME(), 3, 6, 'Product overview page'
),
(
170, 1, 1, 167, SYSUTCDATETIME(), 3, 6, 'Product testimonials page'
),
(
171, 1, 1, 168, SYSUTCDATETIME(), 3, 6, 'Product benefits and details feature list'
),
(
172, 1, 1, 169, SYSUTCDATETIME(), 3, 6, 'Product pricing and business information'
),
(
173, 1, 1, 170, SYSUTCDATETIME(), 3, 6, 'Content Management System'
),
(
174, 1, 1, 171, SYSUTCDATETIME(), 3, 6, 'News Articles'
),
(
175, 1, 1, 172, SYSUTCDATETIME(), 3, 6, 'Display a nicely formatted news list page in the company section of the website'
),
(
176, 1, 1, 173, SYSUTCDATETIME(), 3, 6, 'Display the details of a single article with embedded images'
),
(
177, 1, 1, 174, SYSUTCDATETIME(), 3, 6, 'Content editors can create new articles with headline, description and publish date'
),
(
178, 1, 1, 175, SYSUTCDATETIME(), 3, 6, 'Content editors can update existing articles and remove from publication'
),
(
179, 1, 1, 176, SYSUTCDATETIME(), 3, 6, 'Display the list of news items as an RSS / ATOM newsfeed'
),
(
180, 1, 1, 177, SYSUTCDATETIME(), 3, 6, 'Blog'
),
(
181, 1, 1, 178, SYSUTCDATETIME(), 3, 6, 'Display a list of blog entries with tags'
),
(
182, 1, 1, 179, SYSUTCDATETIME(), 3, 6, 'Display a ''tag cloud'' that shows a filtered list of articles when a tag is clicked on'
),
(
183, 1, 1, 180, SYSUTCDATETIME(), 3, 6, 'Display the details of a single blog entry with embedded images'
),
(
184, 1, 1, 181, SYSUTCDATETIME(), 3, 6, 'Content editors can create new blog entries with headline, description and publish date'
),
(
185, 1, 1, 182, SYSUTCDATETIME(), 3, 6, 'Content editors can update existing blog entries and remove from publication'
),
(
186, 1, 1, 183, SYSUTCDATETIME(), 3, 6, 'Authentication'
),
(
187, 1, 1, 184, SYSUTCDATETIME(), 3, 6, 'User can login with email address and password'
),
(
188, 1, 1, 185, SYSUTCDATETIME(), 3, 6, 'User can register for an account'
),
(
189, 1, 1, 186, SYSUTCDATETIME(), 3, 6, 'User can update their online profile'
),
(
190, 1, 1, 187, SYSUTCDATETIME(), 3, 6, 'Users will be notified when their account is approved for publishing content'
),
(
191, 1, 1, 188, SYSUTCDATETIME(), 3, 6, 'Authorization'
),
(
192, 1, 1, 189, SYSUTCDATETIME(), 3, 6, 'Site administrators can go to a screen to see a list of new user registrations'
),
(
193, 1, 1, 190, SYSUTCDATETIME(), 3, 6, 'Administrators can approve/deny new registration requests'
),
(
194, 1, 1, 191, SYSUTCDATETIME(), 3, 6, 'Administrators can view a list of existing users'
),
(
195, 1, 1, 192, SYSUTCDATETIME(), 3, 6, 'Administrators to can view, edit or delete an existing user profile'
),
(
196, 1, 1, 193, SYSUTCDATETIME(), 3, 6, 'Social Media'
),
(
197, 1, 1, 194, SYSUTCDATETIME(), 3, 6, 'Footer that displays links to social media accounts'
),
(
198, 1, 1, 195, SYSUTCDATETIME(), 3, 6, 'Facebook'
),
(
199, 1, 1, 196, SYSUTCDATETIME(), 3, 6, 'LinkedIn'
),
(
200, 1, 1, 197, SYSUTCDATETIME(), 3, 6, 'Twitter'
),
(
201, 1, 1, 198, SYSUTCDATETIME(), 3, 6, 'Instagram'
),
(
202, 1, 1, 199, SYSUTCDATETIME(), 3, 6, 'YouTube'
),
(
203, 1, 1, 200, SYSUTCDATETIME(), 3, 6, 'XING'
),
(
204, 1, 1, 201, SYSUTCDATETIME(), 3, 6, 'Share buttons for blog'
),
(
205, 1, 1, 202, SYSUTCDATETIME(), 3, 6, 'Facebook'
),
(
206, 1, 1, 203, SYSUTCDATETIME(), 3, 6, 'LinkedIn'
),
(
207, 1, 1, 204, SYSUTCDATETIME(), 3, 6, 'Twitter'
),
(
208, 1, 1, 205, SYSUTCDATETIME(), 3, 6, 'XING'
),
(
209, 8, 4, 47, SYSUTCDATETIME(), 3, 7, 'CRM Release 1.0'
),
(
210, 5, 4, 48, SYSUTCDATETIME(), 3, 7, 'Discovery'
),
(
211, 5, 4, 49, SYSUTCDATETIME(), 3, 7, 'Design'
),
(
212, 5, 4, 50, SYSUTCDATETIME(), 3, 7, 'Development'
),
(
213, 5, 4, 51, SYSUTCDATETIME(), 3, 7, 'System Testing'
),
(
214, 5, 4, 52, SYSUTCDATETIME(), 3, 7, 'User Acceptance Testing'
),
(
215, 8, 4, 53, SYSUTCDATETIME(), 3, 7, 'CRM Release 1.1'
),
(
216, 5, 4, 54, SYSUTCDATETIME(), 3, 7, 'Discovery'
),
(
217, 5, 4, 55, SYSUTCDATETIME(), 3, 7, 'Design'
),
(
218, 5, 4, 56, SYSUTCDATETIME(), 3, 7, 'Development'
),
(
219, 5, 4, 57, SYSUTCDATETIME(), 3, 7, 'System Testing'
),
(
220, 5, 4, 58, SYSUTCDATETIME(), 3, 7, 'User Acceptance Testing'
),
(
221, 1, 1, 206, SYSUTCDATETIME(), 3, 7, 'Marketing Automation'
),
(
222, 1, 1, 207, SYSUTCDATETIME(), 3, 7, 'Campaign management'
),
(
223, 1, 1, 208, SYSUTCDATETIME(), 3, 7, 'Ability to create new marketing campaigns'
),
(
224, 1, 1, 209, SYSUTCDATETIME(), 3, 7, 'Ability to edit and update marketing campaigns'
),
(
225, 1, 1, 210, SYSUTCDATETIME(), 3, 7, 'Can retire old marketing campaigns'
),
(
226, 1, 1, 211, SYSUTCDATETIME(), 3, 7, 'Contact Management'
),
(
227, 1, 1, 212, SYSUTCDATETIME(), 3, 7, 'Ability to create new contacts'
),
(
228, 1, 1, 213, SYSUTCDATETIME(), 3, 7, 'Ability to manage and update existing contacts'
),
(
229, 1, 1, 214, SYSUTCDATETIME(), 3, 7, 'Organization Management'
),
(
230, 1, 1, 215, SYSUTCDATETIME(), 3, 7, 'Ability to create new organizations'
),
(
231, 1, 1, 216, SYSUTCDATETIME(), 3, 7, 'Ability to manage and update existing organizations'
),
(
232, 1, 1, 217, SYSUTCDATETIME(), 3, 7, 'Can associate contacts with organizations'
),
(
233, 1, 1, 218, SYSUTCDATETIME(), 3, 7, 'Campaign list generation'
),
(
234, 1, 1, 219, SYSUTCDATETIME(), 3, 7, 'Can create marketing lists'
),
(
235, 1, 1, 220, SYSUTCDATETIME(), 3, 7, 'Can associate contacts and organizations with lists'
),
(
236, 1, 1, 221, SYSUTCDATETIME(), 3, 7, 'Can link campaign lists to specific campaigns and goals'
),
(
237, 1, 1, 222, SYSUTCDATETIME(), 3, 7, 'Campaign execution'
),
(
238, 1, 1, 223, SYSUTCDATETIME(), 3, 7, 'Email marketing'
),
(
239, 1, 1, 224, SYSUTCDATETIME(), 3, 7, 'Can create email templates'
),
(
240, 1, 1, 225, SYSUTCDATETIME(), 3, 7, 'Can associate email templates with specific campaigns'
),
(
241, 1, 1, 226, SYSUTCDATETIME(), 3, 7, 'Can schedule email campaigns based on users'' own timezones'
),
(
242, 1, 1, 227, SYSUTCDATETIME(), 3, 7, 'Option to include tracking pixels to email to see open and click rates'
),
(
243, 1, 1, 228, SYSUTCDATETIME(), 3, 7, 'Event marketing'
),
(
244, 1, 1, 229, SYSUTCDATETIME(), 3, 7, 'Can create online and face to face events'
),
(
245, 1, 1, 230, SYSUTCDATETIME(), 3, 7, 'Can associate events with campaigns'
),
(
246, 1, 1, 231, SYSUTCDATETIME(), 3, 7, 'Can send out invitations to attendees based on lists'
),
(
247, 1, 1, 232, SYSUTCDATETIME(), 3, 7, 'Tracking and analytics'
),
(
248, 1, 1, 233, SYSUTCDATETIME(), 3, 7, 'Can add tracking pixels to website and other online properties'
),
(
249, 1, 1, 234, SYSUTCDATETIME(), 3, 7, 'Tracking code will record which content, URL and other items are viewed'
),
(
250, 1, 1, 235, SYSUTCDATETIME(), 3, 7, 'System will score contacts based on engagement with content, emails and events'
),
(
251, 1, 1, 236, SYSUTCDATETIME(), 3, 7, 'Sales Force Automation'
),
(
252, 1, 1, 237, SYSUTCDATETIME(), 3, 7, 'Salesperson Management'
),
(
253, 1, 1, 238, SYSUTCDATETIME(), 3, 7, 'Can create new salespersons'
),
(
254, 1, 1, 239, SYSUTCDATETIME(), 3, 7, 'Can edit and update existing salespersons'
),
(
255, 1, 1, 240, SYSUTCDATETIME(), 3, 7, 'Can associate salesperson with role'
),
(
256, 1, 1, 241, SYSUTCDATETIME(), 3, 7, 'Can assign target and bonus for salespersons'
),
(
257, 1, 1, 242, SYSUTCDATETIME(), 3, 7, 'Can assign organizations and contacts with salesperson'
),
(
258, 1, 1, 243, SYSUTCDATETIME(), 3, 7, 'Lead and Opportunity management'
),
(
259, 1, 1, 244, SYSUTCDATETIME(), 3, 7, 'Place to register new leads during events'
),
(
260, 1, 1, 245, SYSUTCDATETIME(), 3, 7, 'Contact widget for website that lets visitors self-register'
),
(
261, 1, 1, 246, SYSUTCDATETIME(), 3, 7, 'Screen to review and update leads'
),
(
262, 1, 1, 247, SYSUTCDATETIME(), 3, 7, 'Automated purging of leads after 6 months'
),
(
263, 1, 1, 248, SYSUTCDATETIME(), 3, 7, 'Pipeline management'
),
(
264, 1, 1, 249, SYSUTCDATETIME(), 3, 7, 'Can create/modify pipeline stages for different kinds of opportunity'
),
(
265, 1, 1, 250, SYSUTCDATETIME(), 3, 7, 'Deal Flow'
),
(
266, 1, 1, 251, SYSUTCDATETIME(), 3, 7, 'Can create new deals linked to contacts and organizations'
),
(
267, 1, 1, 252, SYSUTCDATETIME(), 3, 7, 'Can associate a deal with a specific pipeline stage'
),
(
268, 1, 1, 253, SYSUTCDATETIME(), 3, 7, 'Can add notes and attachments to deals'
),
(
269, 1, 1, 254, SYSUTCDATETIME(), 3, 7, 'Activity Management'
),
(
270, 1, 1, 255, SYSUTCDATETIME(), 3, 7, 'Abilility to schedule tasks, activities, calls with a deal'
),
(
271, 1, 1, 256, SYSUTCDATETIME(), 3, 7, 'Can mark activities as done and add information from the activity'
),
(
272, 1, 1, 257, SYSUTCDATETIME(), 3, 7, 'System reminders salesperson if activities are upcoming and overdue'
),
(
273, 1, 1, 258, SYSUTCDATETIME(), 3, 7, 'Option to display pipeline as a Kanban board'
),
(
274, 1, 1, 259, SYSUTCDATETIME(), 3, 7, 'Can drag and drop deals between the different pipeline stages'
),
(
275, 1, 1, 260, SYSUTCDATETIME(), 3, 7, 'Partner management'
),
(
276, 1, 1, 261, SYSUTCDATETIME(), 3, 7, 'Can tag an organization as a partner'
),
(
277, 1, 1, 262, SYSUTCDATETIME(), 3, 7, 'Partners can upload referrals to a partner portal'
),
(
278, 1, 1, 263, SYSUTCDATETIME(), 3, 7, 'Ability to track partner sales and attribute commissions'
),
(
279, 1, 1, 264, SYSUTCDATETIME(), 3, 7, 'Customer Support'
),
(
280, 1, 1, 265, SYSUTCDATETIME(), 3, 7, 'Can link support users to organization'
),
(
281, 1, 1, 266, SYSUTCDATETIME(), 3, 7, 'Customer support portal where they can see SLAs, knowledge base and open tickets'
),
(
282, 1, 1, 267, SYSUTCDATETIME(), 3, 7, 'Ticket management'
),
(
283, 1, 1, 268, SYSUTCDATETIME(), 3, 7, 'Customer can log new help desk ticket'
),
(
284, 1, 1, 269, SYSUTCDATETIME(), 3, 7, 'Customer can review open tickets'
),
(
285, 1, 1, 270, SYSUTCDATETIME(), 3, 7, 'Responding to tickets'
),
(
286, 1, 1, 271, SYSUTCDATETIME(), 3, 7, 'Customer support agent can reply to ticket with public note'
),
(
287, 1, 1, 272, SYSUTCDATETIME(), 3, 7, 'Customer support agent can reply to ticket with private note'
),
(
288, 1, 1, 273, SYSUTCDATETIME(), 3, 7, 'Customer can reply to notes on ticket and reopen ticket'
),
(
289, 1, 1, 274, SYSUTCDATETIME(), 3, 7, 'Customer can close ticket once resolved'
),
(
290, 1, 1, 275, SYSUTCDATETIME(), 3, 7, 'Ticket auto-closes after 90 days of not being replied to'
),
(
291, 1, 1, 276, SYSUTCDATETIME(), 3, 7, 'Escalation'
),
(
292, 1, 1, 277, SYSUTCDATETIME(), 3, 7, 'Tickets can be esclataed to 2nd tier support if unable to be solved by 1st tier'
),
(
293, 1, 1, 278, SYSUTCDATETIME(), 3, 7, 'Tickets can be esclataed to 3rd tier support if unable to be solved by 2nd tier'
),
(
294, 1, 1, 279, SYSUTCDATETIME(), 3, 7, 'Knowledgebase'
),
(
295, 1, 1, 280, SYSUTCDATETIME(), 3, 7, 'Customer support agents can create KB articles'
),
(
296, 1, 1, 281, SYSUTCDATETIME(), 3, 7, 'Customer support agents can view and modify KB articles'
),
(
297, 1, 1, 282, SYSUTCDATETIME(), 3, 7, 'Customer support agents can create and manage KB categories'
),
(
298, 1, 1, 283, SYSUTCDATETIME(), 3, 7, 'The system tracks how popular specific articles are (# views)'
),
(
299, 1, 1, 284, SYSUTCDATETIME(), 3, 7, 'Customer support agents can quickly insert a KB article into a support ticket'
)
GO

SET IDENTITY_INSERT TST_HISTORY_CHANGESET OFF; 

