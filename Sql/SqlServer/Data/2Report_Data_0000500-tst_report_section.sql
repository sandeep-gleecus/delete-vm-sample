/***************************************************************
**	Insert script for table TST_REPORT_SECTION
***************************************************************/
SET IDENTITY_INSERT [dbo].[TST_REPORT_SECTION] ON 

INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (1, NULL, N'ProjectOverview', N'Project Overview', N'This section displays the name and description of the current project.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/ProjectData/Project">
    <div class="Title1">
      Project <xsl:value-of select="ProjectId"/>: <xsl:value-of select="Name"/>
    </div>
    <p>
      <xsl:value-of select="Description" disable-output-escaping="yes"/>
    </p>
  </xsl:template>
</xsl:stylesheet>', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (2, 1, N'RequirementList', N'Requirements List', N'This section displays a simple table containing all the requirements with a column for each of their standard and custom fields.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/RequirementData">
    <table class="DataGrid" style="width:100%">
      <tr>
        <th>Req #</th>
        <th>Name</th>
        <th>Description</th>
        <th>Type</th>
        <th>Priority</th>
        <th>Status</th>
        <th>Author</th>
        <th>Owner</th>
        <th>Creation Date</th>
        <th>Test Coverage</th>
        <th>Task Progress</th>
        <th>Last Modified</th>
        <th>Release #</th>
        <th>Component</th>
        <th>Estimate</th>
        <th>Est. Effort</th>
        <th>Task Effort</th>
        <th>Actual Effort</th>
        <th>Remaining Effort</th>
        <th>Projected Effort</th>
        <xsl:for-each select="Requirement[1]/CustomProperties/CustomProperty">
          <th>
            <xsl:value-of select="Alias"/>
          </th>
        </xsl:for-each>
      </tr>
      <xsl:for-each select="Requirement">
        <tr>
          <td>
            <xsl:value-of select="RequirementId"/>
          </td>
          <td>
            <xsl:attribute name="style">
              padding-left: <xsl:value-of select="string-length(IndentLevel)*2"/>px;
            </xsl:attribute>
            <xsl:if test="IsSummary=''True''">
              <b>
                <xsl:value-of select="Name"/>
              </b>
            </xsl:if>
            <xsl:if test="IsSummary=''False''">
              <xsl:value-of select="Name"/>
            </xsl:if>
          </td>
          <td>
            <xsl:value-of select="Description" disable-output-escaping="yes"/>
          </td>
          <td>
            <xsl:value-of select="RequirementTypeName"/>
          </td>
          <td>
            <xsl:value-of select="ImportanceName"/>
          </td>
          <td>
            <xsl:value-of select="RequirementStatusName"/>
          </td>
          <td>
            <xsl:value-of select="AuthorName"/>
          </td>
          <td>
            <xsl:value-of select="OwnerName"/>
          </td>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="CreationDate" />
            </xsl:call-template>
          </td>
          <td>
            <xsl:value-of select="CoverageCountTotal"/><xsl:text> </xsl:text>Covering,
            <xsl:value-of select="CoverageCountFailed"/><xsl:text> </xsl:text>Failed,
            <xsl:value-of select="CoverageCountPassed"/><xsl:text> </xsl:text>Passed,
            <xsl:value-of select="CoverageCountBlocked"/><xsl:text> </xsl:text>Blocked,
            <xsl:value-of select="CoverageCountCaution"/><xsl:text> </xsl:text>Caution
          </td>
          <td>
            <xsl:value-of select="TaskCount"/><xsl:text> </xsl:text>Tasks;
            <xsl:value-of select="TaskPercentOnTime"/>%<xsl:text> </xsl:text>On Schedule,
            <xsl:value-of select="TaskPercentLateFinish"/>%<xsl:text> </xsl:text>Running Late,
            <xsl:value-of select="TaskPercentNotStart"/>%<xsl:text> </xsl:text>Starting Late,
            <xsl:value-of select="TaskPercentLateStart"/>%<xsl:text> </xsl:text>Not Started
          </td>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="LastUpdateDate" />
            </xsl:call-template>
          </td>
          <td>
            <xsl:value-of select="ReleaseVersionNumber"/>
          </td>
          <td>
            <xsl:value-of select="ComponentName"/>
          </td>
          <td class="Timespan">
            <xsl:value-of select="EstimatePoints" />
          </td>
          <td>
			  <xsl:variable name="durationInDays" select="EstimatedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			 <xsl:variable name="durationInDays" select="TaskEstimatedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			 <xsl:variable name="durationInDays" select="TaskActualEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="TaskRemainingEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="TaskProjectedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <xsl:for-each select="CustomProperties/CustomProperty">
            <xsl:choose>
              <xsl:when test="Type=''Date''">
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="Value" />
                  </xsl:call-template>
                </td>
              </xsl:when>
              <xsl:otherwise>
                <td>
                  <xsl:value-of select="Value" disable-output-escaping="yes"/>
                </td>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>
  <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-before(substring-after($datetime, ''T''), ''.'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year , '' '', $time)" />
  </xsl:template>
</xsl:stylesheet>', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (3, 1, N'RequirementDetails', N'Requirements Details', N'This section displays all of the requirements in a document format with embedded tables for each of the associated elements.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/RequirementData">
    <xsl:for-each select="Requirement">
      <div>
        <xsl:attribute name="style">
          margin-left: <xsl:value-of select="string-length(IndentLevel)*3"/>px;
        </xsl:attribute>
        <xsl:if test="IsSummary=''True''">
          <div class="Title2">
            RQ:<xsl:value-of select="RequirementId"/> - <xsl:value-of select="Name"/>
          </div>
          <div class="Description">
            <xsl:value-of select="Description" disable-output-escaping="yes"/>
          </div>
          <br />
        </xsl:if>
        <xsl:if test="IsSummary=''False''">
          <xsl:attribute name="style">
            padding-left: <xsl:value-of select="string-length(IndentLevel)*3"/>px;
          </xsl:attribute>
          <div class="Title3">
            RQ:<xsl:value-of select="RequirementId"/> - <xsl:value-of select="Name"/>
          </div>
          <p>
            <xsl:value-of select="Description" disable-output-escaping="yes"/>
          </p>
        </xsl:if>
        <table class="HiddenTable">
          <tr>
            <th>Type:</th>
            <td>
              <xsl:value-of select="RequirementTypeName"/>
            </td>
            <th>Component:</th>
            <td>
              <xsl:value-of select="ComponentName"/>
            </td>
          </tr>
          <tr>
            <th>Priority:</th>
            <td>
              <xsl:value-of select="ImportanceName"/>
            </td>
            <th>Status:</th>
            <td>
              <xsl:value-of select="RequirementStatusName"/>
            </td>
          </tr>
          <tr>
            <th>Author:</th>
            <td>
              <xsl:value-of select="AuthorName"/>
            </td>
            <th>Creation Date:</th>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="CreationDate" />
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <th>Coverage:</th>
            <td>
              <xsl:value-of select="CoverageCountTotal"/>
              Covering,
              <xsl:value-of select="CoverageCountFailed"/>
              Failed,
              <xsl:value-of select="CoverageCountPassed"/>
              Passed
              <xsl:value-of select="CoverageCountBlocked"/>
              Blocked
              <xsl:value-of select="CoverageCountCaution"/>
              Caution
            </td>
            <th>Last Modified:</th>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="LastUpdateDate" />
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <th>Owner:</th>
            <td>
              <xsl:value-of select="OwnerName"/>
            </td>
            <th>Estimate:</th>
            <td class="Timespan">
              <xsl:value-of select="EstimatePoints"/>
            </td>
          </tr>
          <tr>
            <th>Release #:</th>
            <td>
              <xsl:value-of select="ReleaseVersionNumber"/>
            </td>
            <th>Est. Effort:</th>
            <td>
			  <xsl:variable name="durationInDays" select="EstimatedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
          </tr>
          <tr>
            <th>Task Est. Effort:</th>
            <td>
			  <xsl:variable name="durationInDays" select="TaskEstimatedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <th>Task Actual Effort:</th>
            <td>
			  <xsl:variable name="durationInDays" select="TaskActualEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
       <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
          </tr>
          <tr>
            <th>Task Remaining Effort:</th>
            <td>
			  <xsl:variable name="durationInDays" select="TaskRemainingEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <th>Task Projected Effort:</th>
            <td>
			  <xsl:variable name="durationInDays" select="TaskProjectedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
          </tr>
          <xsl:for-each select="CustomProperties/CustomProperty">
            <tr>
              <th>
                <xsl:value-of select="Alias"/>:
              </th>
              <xsl:choose>
                <xsl:when test="Type=''Date''">
                  <td class="Date" colspan="3">
                    <xsl:call-template name="format-date">
                      <xsl:with-param name="datetime" select="Value" />
                    </xsl:call-template>
                  </td>
                </xsl:when>
                <xsl:otherwise>
                  <td colspan="3">
                    <xsl:value-of select="Value" disable-output-escaping="yes"/>
                  </td>
                </xsl:otherwise>
              </xsl:choose>
            </tr>
          </xsl:for-each>
        </table>
        <br />
        <xsl:if test="RequirementSteps/RequirementStep">
          <div class="Title3">
            Scenario Steps:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Step</th>
              <th>Description</th>
            </tr>
            <xsl:for-each select="RequirementSteps/RequirementStep">
              <tr>
                <td>
                  <xsl:value-of select="position()"/>
                </td>
                <td>
                  <xsl:value-of select="Description" disable-output-escaping="yes"/>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="Discussions/Discussion">
          <div class="Title3">
            Comments:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Date</th>
              <th>Creator</th>
              <th>Comment</th>
              <th>Id</th>
            </tr>
            <xsl:for-each select="Discussions/Discussion">
              <tr>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="CreationDate" />
                  </xsl:call-template>
                </td>
                <td>
                  <xsl:value-of select="CreatorName"/>
                </td>
                <td>
                  <xsl:value-of select="Text" disable-output-escaping="yes"/>
                </td>
                <td>
                  <xsl:value-of select="DiscussionId"/>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="TestCases/TestCase">
          <div class="Title4">
            Test Coverage:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Test #</th>
              <th>Name</th>
              <th>Status</th>
              <th>Est. Duration</th>
              <th>Last Execution Date</th>
            </tr>
            <xsl:for-each select="TestCases/TestCase">
              <tr>
                <td>
                  TC<xsl:value-of select="TestCaseId"/>
                </td>
                <td>
                  <xsl:value-of select="Name"/>
                </td>
                <td>
                  <xsl:value-of select="ExecutionStatusName"/>
                </td>
                <td>
				   <xsl:variable name="durationInDays" select="EstimatedDuration"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
       <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
                </td>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="ExecutionDate" />
                  </xsl:call-template>
                </td>
              </tr>
              <xsl:if test="TestRuns/TestRun">
                <tr>
                  <td colspan="5">
					 <table class="DataGrid">
						<tr>
						  <th>Run #</th>
						  <th>Tester</th>
						  <th>Release</th>
						  <th>Status</th>
						  <th>Execution Date</th>
						  <th>Incidents</th>
						</tr>
						<xsl:for-each select="TestRuns/TestRun">
						  <tr>
							<td>
							  TR<xsl:value-of select="TestRunId"/>
							</td>
							<td>
							  <xsl:value-of select="TesterName"/>
							</td>
							<td>
							  <xsl:value-of select="ReleaseVersionNumber"/>
							</td>
							<td>
							  <xsl:value-of select="ExecutionStatusName"/>
							</td>
							<td class="Date">
							  <xsl:call-template name="format-date">
								<xsl:with-param name="datetime" select="EndDate" />
							  </xsl:call-template>
							</td>
							<td>
								<xsl:if test="Incidents/Incident">
									<xsl:for-each select="Incidents/Incident">
									  <xsl:if test="position() > 1">
										, IN<xsl:value-of select="IncidentId"/>
									  </xsl:if>
									  <xsl:if test="position() = 1">
										IN<xsl:value-of select="IncidentId"/>
									  </xsl:if>
									</xsl:for-each>
								</xsl:if>
							</td>
						  </tr>
						</xsl:for-each>
					 </table>
				  </td>
				</tr>
			  </xsl:if>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="Requirements/ArtifactLink">
          <div class="Title4">
            Linked Requirements
          </div>
          <table class="DataGrid">
            <tr>
              <th>Association</th>
              <th>Name</th>
              <th>Created By</th>
              <th>Comment</th>
              <th>Date</th>
              <th>Req Id</th>
            </tr>
            <xsl:for-each select="Requirements/ArtifactLink">
              <tr>
                <td>
                  <xsl:value-of select="ArtifactLinkTypeName"/>
                </td>
                <td>
                  <xsl:value-of select="ArtifactName"/>
                </td>
                <td>
                  <xsl:value-of select="CreatorName"/>
                </td>
                <td>
                  <xsl:value-of select="Comment"/>
                </td>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="CreationDate" />
                  </xsl:call-template>
                </td>
                <td>
                  RQ<xsl:value-of select="ArtifactId"/>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="Incidents/ArtifactLink">
          <div class="Title4">
            Associated Incidents
          </div>
          <table class="DataGrid">
            <tr>
              <th>Association</th>
              <th>Name</th>
              <th>Created By</th>
              <th>Comment</th>
              <th>Date</th>
              <th>Inc Id</th>
            </tr>
            <xsl:for-each select="Incidents/ArtifactLink">
              <tr>
                <td>
                  <xsl:value-of select="ArtifactLinkTypeName"/>
                </td>
                <td>
                  <xsl:value-of select="ArtifactName"/>
                </td>
                <td>
                  <xsl:value-of select="CreatorName"/>
                </td>
                <td>
                  <xsl:value-of select="Comment"/>
                </td>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="CreationDate" />
                  </xsl:call-template>
                </td>
                <td>
                  IN<xsl:value-of select="ArtifactId"/>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="Tasks/Task">
          <div class="Title4">
            Associated Tasks:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Task #</th>
              <th>Name</th>
              <th>Status</th>
              <th>Priority</th>
              <th>Owned By</th>
              <th>Start Date</th>
              <th>End Date</th>
              <th>Iteration #</th>
              <th>% Completed</th>
              <th>Est. Effort</th>
              <th>Actual Effort</th>
              <th>Projected Effort</th>
            </tr>
            <xsl:for-each select="Tasks/Task">
              <tr>
                <td>
                  TK<xsl:value-of select="TaskId"/>
                </td>
                <td>
                  <xsl:value-of select="Name"/>
                </td>
                <td>
                  <xsl:value-of select="TaskStatusName"/>
                </td>
                <td>
                  <xsl:value-of select="TaskPriorityName"/>
                </td>
                <td>
                  <xsl:value-of select="OwnerName"/>
                </td>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="StartDate" />
                  </xsl:call-template>
                </td>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="EndDate" />
                  </xsl:call-template>
                </td>
                <td>
                  <xsl:value-of select="ReleaseVersionNumber"/>
                </td>
                <td>
                  <xsl:value-of select="CompletionPercent"/> %
                </td>
                <td class="Timespan">
                  <xsl:value-of select="EstimatedEffort"/>
                </td>
                <td class="Timespan">
                  <xsl:value-of select="ActualEffort"/>
                </td>
                <td class="Timespan">
                  <xsl:value-of select="ProjectedEffort"/>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="Attachments/Attachment">
          <div class="Title4">
            File Attachments:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Filename</th>
              <th>Description</th>
              <th>Author</th>
              <th>Date Uploaded</th>
            </tr>
            <xsl:for-each select="Attachments/Attachment">
              <tr>
                <td>
                  <xsl:value-of select="Filename"/>
                </td>
                <td>
                  <xsl:value-of select="Description"/>
                </td>
                <td>
                  <xsl:value-of select="AuthorName"/>
                </td>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="UploadDate" />
                  </xsl:call-template>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="SourceCodeRevisions/SourceCodeRevision">
          <div class="Title3">
            Source Code Revisions:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Name</th>
              <th>Message</th>
              <th>Author</th>
              <th>Date</th>
            </tr>
            <xsl:for-each select="SourceCodeRevisions/SourceCodeRevision">
              <tr>
                <td>
                  <xsl:value-of select="Name"/>
                </td>
                <td>
                  <xsl:value-of select="Message"/>
                </td>
                <td>
                  <xsl:value-of select="AuthorName"/>
                </td>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="UpdateDate" />
                  </xsl:call-template>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="History/ArtifactHistory">
          <div class="Title4">
            Requirement Change History:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Change Date</th>
              <th>Field Name</th>
              <th>Old Value</th>
              <th>New Value</th>
              <th>Changed By</th>
            </tr>
            <xsl:for-each select="History/ArtifactHistory">
              <tr>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="ChangeDate" />
                  </xsl:call-template>
                </td>
                <td>
                  <xsl:value-of select="FieldName"/>
                </td>
                <td>
                  <xsl:value-of select="OldValue" disable-output-escaping="yes"/>
                </td>
                <td>
                  <xsl:value-of select="NewValue" disable-output-escaping="yes"/>
                </td>
                <td>
                  <xsl:value-of select="UserName"/>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <br />
      </div>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-before(substring-after($datetime, ''T''), ''.'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year , '' '', $time)" />
  </xsl:template>
</xsl:stylesheet>', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (4, 1, N'RequirementPlan', N'Requirements Plan', N'This section displays a table that contains all of the requirements in the project together with the incidents and tasks arranged in a single GANTT view.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt">
  <xsl:template match="RequirementData">
    <table class="DataGrid" style="width:100%">
      <tr>
        <th>ID #</th>
        <th colspan="2">Name</th>
        <th>Type</th>
        <th>Status</th>
        <th>Priority</th>
        <th>Owner</th>
        <th>Start Date</th>
        <th>End Date</th>
        <th>Release #</th>
        <th>% Complete</th>
        <th>Req. Est. Effort</th>
        <th>Task Effort</th>
        <th>Actual Effort</th>
        <th>Rem. Effort</th>
        <th>Proj. Effort</th>
      </tr>
      <xsl:for-each select="Requirement">
        <tr>
          <xsl:attribute name="class">
            <xsl:if test="IsSummary=''True''">
              BoldRow
            </xsl:if>
          </xsl:attribute>
          <td>
            <xsl:value-of select="RequirementId"/>
          </td>
          <td colspan="2">
            <xsl:attribute name="style">
              padding-left: <xsl:value-of select="string-length(IndentLevel)*4"/>px;
            </xsl:attribute>
            <xsl:value-of select="Name"/>
          </td>
          <td>
            <xsl:value-of select="RequirementTypeName"/>
          </td>
          <td>
            <xsl:value-of select="RequirementStatusName"/>
          </td>
          <td>
            <xsl:value-of select="ImportanceName"/>
          </td>
          <td>
            <xsl:value-of select="OwnerName"/>
          </td>
          <td></td>
          <td></td>
          <td>
            <xsl:value-of select="ReleaseVersionNumber"/>
          </td>
          <td></td>
          <td>
			<xsl:variable name="durationInDays" select="EstimatedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="TaskEstimatedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="TaskActualEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="TaskRemainingEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="TaskProjectedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
        </tr>
        <xsl:for-each select="Tasks/Task">
          <tr class="SmallRow">
            <td>
              <xsl:value-of select="TaskId"/>
            </td>
            <td style="width:50px"></td>
            <td>
              <xsl:value-of select="Name"/>
            </td>
            <td>
              <xsl:value-of select="TaskTypeName"/> Task
            </td>
            <td>
              <xsl:value-of select="TaskStatusName"/>
            </td>
            <td>
              <xsl:value-of select="TaskPriorityName"/>
            </td>
            <td style="white-space:nowrap">
              <xsl:value-of select="OwnerName"/>
            </td>
            <td style="white-space:nowrap" class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="StartDate" />
              </xsl:call-template>
            </td>
            <td style="white-space:nowrap" class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="EndDate" />
              </xsl:call-template>
            </td>
            <td>
              <xsl:value-of select="ReleaseVersionNumber"/>
            </td>
            <td>
              <xsl:value-of select="CompletionPercent"/> %
            </td>
            <td></td>
            <td class="Timespan">
              <xsl:value-of select="EstimatedEffort"/>
            </td>
            <td class="Timespan">
              <xsl:value-of select="ActualEffort"/>
            </td>
            <td class="Timespan">
              <xsl:value-of select="RemainingEffort"/>
            </td>
            <td class="Timespan">
              <xsl:value-of select="ProjectedEffort"/>
            </td>
          </tr>
        </xsl:for-each>
      </xsl:for-each>
    </table>
  </xsl:template>
  <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-before(substring-after($datetime, ''T''), ''.'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year , '' '', $time)" />
  </xsl:template>
</xsl:stylesheet>
', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (5, 2, N'TestCaseList', N'Test Case List', N'This section displays a simple table containing all the test cases with a column for each of their standard and custom fields.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/TestCaseData">
    <table class="DataGrid" style="width:100%">
      <tr>
        <th>Test #</th>
        <th>Name</th>
        <th>Description</th>
        <th>Priority</th>
        <xsl:if test="//TestCase/TestSteps">
          <th>Test Step</th>
          <th>Test Step Description</th>
          <th>Test Step Expected Result</th>
          <th>Test Step Sample Data</th>
        </xsl:if>
        <th>Exec. Status</th>
        <th>Type</th>
        <th>Status</th>
        <th>Author</th>
        <th>Owner</th>
        <th>Automation Engine</th>
        <th>Component(s)</th>
        <th>Est. Duration</th>
        <th>Created On</th>
        <th>Last Modified</th>
        <th>Last Executed</th>
        <xsl:for-each select="(//TestCase)[1]/CustomProperties/CustomProperty">
          <th>
            <xsl:value-of select="Alias"/>
          </th>
        </xsl:for-each>
      </tr>
      <xsl:for-each select="TestCaseFolder">
        <tr>
          <td>
            <b>
              <xsl:value-of select="TestCaseFolderId"/>
            </b>
          </td>
          <td>
            <xsl:attribute name="style">
              padding-left: <xsl:value-of select="string-length(IndentLevel)*2"/>px;
            </xsl:attribute>
            <b>
              <xsl:value-of select="Name"/>
            </b>
          </td>
          <td>
            <xsl:value-of select="Description" disable-output-escaping="yes"/>
          </td>
          <td></td>
          <xsl:if test="//TestCase/TestSteps">
            <td></td>
            <td></td>
            <td></td>
            <td></td>
          </xsl:if>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <xsl:for-each select="(//TestCase)[1]/CustomProperties/CustomProperty">
            <td>
            </td>
          </xsl:for-each>
        </tr>
        <xsl:for-each select="TestCases/TestCase">
          <tr>
            <td>
              <xsl:value-of select="TestCaseId"/>
            </td>
            <td>
              <xsl:value-of select="Name"/>
            </td>
            <td>
              <xsl:value-of select="Description" disable-output-escaping="yes"/>
            </td>
            <td>
              <xsl:value-of select="TestCasePriorityName"/>
            </td>
            <xsl:if test="TestSteps">
              <td></td>
              <td></td>
              <td></td>
              <td></td>
            </xsl:if>
            <td>
              <xsl:value-of select="ExecutionStatusName"/>
            </td>
            <td>
              <xsl:value-of select="TestCaseTypeName"/>
            </td>
            <td>
              <xsl:value-of select="TestCaseStatusName"/>
            </td>
            <td>
              <xsl:value-of select="AuthorName"/>
            </td>
            <td>
              <xsl:value-of select="OwnerName"/>
            </td>
            <td>
              <xsl:value-of select="AutomationEngineName"/>
            </td>
            <td>
              <xsl:value-of select="ComponentNames"/>
            </td>
            <td class="Timespan">
              <xsl:value-of select="EstimatedDuration"/>
			  <xsl:variable name="durationInDays" select="EstimatedDuration"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="CreationDate" />
              </xsl:call-template>
            </td>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="LastUpdateDate" />
              </xsl:call-template>
            </td>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="ExecutionDate" />
              </xsl:call-template>
            </td>
            <xsl:for-each select="CustomProperties/CustomProperty">
              <xsl:choose>
                <xsl:when test="Type=''Date''">
                  <td class="Date">
                    <xsl:call-template name="format-date">
                      <xsl:with-param name="datetime" select="Value" />
                    </xsl:call-template>
                  </td>
                </xsl:when>
                <xsl:otherwise>
                  <td>
                    <xsl:value-of select="Value" disable-output-escaping="yes"/>
                  </td>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:for-each>
          </tr>
          <xsl:for-each select="TestSteps/TestStep">
            <tr>
              <td></td>
              <td></td>
              <td></td>
              <td></td>
              <td>
                <xsl:value-of select="position()"/>
              </td>
              <td>
                <xsl:value-of select="Description" disable-output-escaping="yes"/>
                <xsl:value-of select="'' ''"/>
                <xsl:value-of select="LinkedTestCaseName"/>
              </td>
              <td>
                <xsl:value-of select="ExpectedResult" disable-output-escaping="yes"/>
              </td>
              <td>
                <xsl:value-of select="SampleData" disable-output-escaping="yes"/>
              </td>
              <td>
                <xsl:value-of select="ExecutionStatusName"/>
              </td>
              <td></td>
              <td></td>
              <td></td>
              <td></td>
              <td></td>
              <td></td>
              <td></td>
              <td></td>
              <td></td>
              <td></td>
              <xsl:for-each select="../../CustomProperties/CustomProperty">
                <td>
                </td>
              </xsl:for-each>
            </tr>
          </xsl:for-each>
        </xsl:for-each>
      </xsl:for-each>
    </table>
  </xsl:template>
  <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-before(substring-after($datetime, ''T''), ''.'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year , '' '', $time)" />
  </xsl:template>
</xsl:stylesheet>

', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (6, 2, N'TestCaseDetails', N'Test Case Details', N'This section displays all of the test cases in a document format with embedded tables for each of the associated elements.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/TestCaseData">
    <xsl:for-each select="TestCaseFolder">
      <div class="Title2">
        Folder: <xsl:value-of select="Name"/>
      </div>
      <div class="Description">
        <xsl:value-of select="Description" disable-output-escaping="yes"/>
      </div>
      <xsl:for-each select="TestCases/TestCase">
        <div class="Title3">
          Test TC:<xsl:value-of select="TestCaseId"/> - <xsl:value-of select="Name"/>
        </div>
        <div class="Description">
          <xsl:value-of select="Description" disable-output-escaping="yes"/>
        </div>
        <table class="HiddenTable">
          <tr>
            <th>Execution Status:</th>
            <td>
              <xsl:value-of select="ExecutionStatusName"/>
            </td>
            <th>Priority:</th>
            <td>
              <xsl:value-of select="TestCasePriorityName"/>
            </td>
          </tr>
          <tr>
            <th>Type:</th>
            <td>
              <xsl:value-of select="TestCaseTypeName"/>
            </td>
            <th>Status:</th>
            <td>
              <xsl:value-of select="TestCaseStatusName"/>
            </td>
          </tr>
          <tr>
            <th>Author:</th>
            <td>
              <xsl:value-of select="AuthorName"/>
            </td>
            <th>Creation Date:</th>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="CreationDate" />
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <th>Owner:</th>
            <td>
              <xsl:value-of select="OwnerName"/>
            </td>
            <th>Last Execution:</th>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="ExecutionDate" />
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <th>Automation Engine:</th>
            <td>
              <xsl:value-of select="AutomationEngineName"/>
            </td>
            <th>Last Updated:</th>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="LastUpdateDate" />
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <th>Component(s):</th>
            <td colspan="3">
              <xsl:value-of select="ComponentNames"/>
            </td>            
          </tr>
          <xsl:for-each select="CustomProperties/CustomProperty">
            <tr>
              <th>
                <xsl:value-of select="Alias"/>:
              </th>
              <xsl:choose>
                <xsl:when test="Type=''Date''">
                  <td class="Date" colspan="3">
                    <xsl:call-template name="format-date">
                      <xsl:with-param name="datetime" select="Value" />
                    </xsl:call-template>
                  </td>
                </xsl:when>
                <xsl:otherwise>
                  <td colspan="3">
                    <xsl:value-of select="Value" disable-output-escaping="yes"/>
                  </td>
                </xsl:otherwise>
              </xsl:choose>
            </tr>
          </xsl:for-each>
        </table>
        <xsl:if test="TestSteps/TestStep">
          <table class="DataGrid">
            <tr>
              <th>Step</th>
              <th>Description</th>
              <th>Expected Result</th>
              <th>Sample Data</th>
              <th>Last Status</th>
            </tr>
            <xsl:for-each select="TestSteps/TestStep">
              <tr>
                <td>
                  <xsl:value-of select="position()"/>
                </td>
                <xsl:if test="string(LinkedTestCaseId)">
                  <td colspan="3">
                    <xsl:value-of select="Description" disable-output-escaping="yes"/>
                    ''<xsl:value-of select="LinkedTestCaseName"/>''
                    <xsl:for-each select="Parameters">
                      <i>
                        with
                        <xsl:for-each select="TestStepParameter">
                          <xsl:value-of select="Name"/> = ''<xsl:value-of select="Value"/>'',
                        </xsl:for-each>
                      </i>
                    </xsl:for-each>
                  </td>
                </xsl:if>
                <xsl:if test="not(string(LinkedTestCaseId))">
                  <td>
                    <xsl:value-of select="Description" disable-output-escaping="yes"/>
                  </td>
                  <td>
                    <xsl:value-of select="ExpectedResult" disable-output-escaping="yes"/>
                  </td>
                  <td>
                    <xsl:value-of select="SampleData" disable-output-escaping="yes"/>
                  </td>
                </xsl:if>
                <td>
                  <xsl:value-of select="ExecutionStatusName"/>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="Discussions/Discussion">
          <div class="Title3">
            Comments:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Date</th>
              <th>Creator</th>
              <th>Comment</th>
              <th>Id</th>
            </tr>
            <xsl:for-each select="Discussions/Discussion">
              <tr>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="CreationDate" />
                  </xsl:call-template>
                </td>
                <td>
                  <xsl:value-of select="CreatorName"/>
                </td>
                <td>
                  <xsl:value-of select="Text" disable-output-escaping="yes"/>
                </td>
                <td>
                  <xsl:value-of select="DiscussionId"/>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="TestRuns/TestRun">
          <div class="Title4">
            Test Runs:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Run #</th>
              <th>Tester</th>
              <th>Test Set</th>
              <th>Release</th>
              <th>Version</th>
              <th>Status</th>
              <th>Type</th>
              <th>Automation Host</th>
              <th>Est. Duration</th>
              <th>Actual Duration</th>
              <th>Execution Date</th>
            </tr>
            <xsl:for-each select="TestRuns/TestRun">
              <tr>
                <td>
                  TR<xsl:value-of select="TestRunId"/>
                </td>
                <td>
                  <xsl:value-of select="TesterName"/>
                </td>
                <td>
                  <xsl:value-of select="TestSetName"/>
                </td>
                <td>
                  <xsl:value-of select="ReleaseName"/>
                </td>
                <td>
                  <xsl:value-of select="ReleaseVersionNumber"/>
                </td>
                <td>
                  <xsl:value-of select="ExecutionStatusName"/>
                </td>
                <td>
                  <xsl:value-of select="TestRunTypeName"/>
                </td>
                <td>
                  <xsl:value-of select="AutomationHostName"/>
                </td>
                <td>
				  <xsl:variable name="durationInDays" select="EstimatedDuration"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
                </td>
                <td class="Timespan">
				  <xsl:variable name="durationInDays" select="ActualDuration"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
                </td>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="EndDate" />
                  </xsl:call-template>
                </td>
              </tr>
              <xsl:if test="TestRunSteps/TestRunStep">
                <tr>
                  <td colspan="11">
                    <table class="DataGrid">
                      <tr>
                        <th>Step</th>
                        <th>Description</th>
                        <th>Expected Result</th>
                        <th>Sample Data</th>
                        <th>ActualResult</th>
                        <th>Status</th>
                      </tr>
                      <xsl:for-each select="TestRunSteps/TestRunStep">
                        <tr>
                          <td>
                            <xsl:value-of select="Position"/>
                          </td>
                          <td>
                            <xsl:value-of select="Description" disable-output-escaping="yes"/>
                          </td>
                          <td>
                            <xsl:value-of select="ExpectedResult" disable-output-escaping="yes"/>
                          </td>
                          <td>
                            <xsl:value-of select="SampleData" disable-output-escaping="yes"/>
                          </td>
                          <td>
                            <xsl:value-of select="ActualResult" disable-output-escaping="yes"/>
                          </td>
                          <td>
                            <xsl:value-of select="ExecutionStatusName"/>
                          </td>
                        </tr>
                      </xsl:for-each>
                    </table>
                  </td>
                </tr>
              </xsl:if>
            </xsl:for-each>
          </table>
        </xsl:if>
        <br />
        <xsl:if test="Requirements/Requirement">
          <div class="Title4">
            Requirements Coverage:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Req #</th>
              <th>Name</th>
              <th>Status</th>
              <th>Type</th>
              <th>Priority</th>
            </tr>
            <xsl:for-each select="Requirements/Requirement">
              <tr>
                <td>
                  RQ<xsl:value-of select="RequirementId"/>
                </td>
                <td>
                  <xsl:value-of select="Name"/>
                </td>
                <td>
                  <xsl:value-of select="RequirementStatusName"/>
                </td>
                <td>
                  <xsl:value-of select="RequirementTypeName"/>
                </td>
                <td>
                  <xsl:value-of select="ImportanceName"/>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="Incidents/Incident">
          <div class="Title4">
            Open Incidents:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Inc #</th>
              <th>Type</th>
              <th>Status</th>
              <th>Priority</th>
              <th>Severity</th>
              <th>Name</th>
              <th>Owned By</th>
              <th>Detected On</th>
            </tr>
            <xsl:for-each select="Incidents/Incident">
              <tr>
                <td>
                  IN<xsl:value-of select="IncidentId"/>
                </td>
                <td>
                  <xsl:value-of select="IncidentTypeName"/>
                </td>
                <td>
                  <xsl:value-of select="IncidentStatusName"/>
                </td>
                <td>
                  <xsl:value-of select="PriorityName"/>
                </td>
                <td>
                  <xsl:value-of select="SeverityName"/>
                </td>
                <td>
                  <xsl:value-of select="Name"/>
                </td>
                <td>
                  <xsl:value-of select="OwnerName"/>
                </td>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="CreationDate" />
                  </xsl:call-template>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="Releases/Release">
          <div class="Title3">
            Release Coverage:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Name</th>
              <th>Version Number</th>
              <th>Type</th>
              <th>Status</th>
              <th>Start Date</th>
              <th>End Date</th>
              <th>Release #</th>
            </tr>
            <xsl:for-each select="Releases/Release">
              <tr>
                <td>
                  <xsl:attribute name="style">
                    padding-left: <xsl:value-of select="string-length(IndentLevel)*2"/>px;
                  </xsl:attribute>
                  <xsl:if test="IsSummary=''True''">
                    <b>
                      <xsl:value-of select="Name"/>
                    </b>
                  </xsl:if>
                  <xsl:if test="IsSummary=''False''">
                    <xsl:value-of select="Name"/>
                  </xsl:if>
                </td>
                <td>
                  <xsl:value-of select="VersionNumber"/>
                </td>
                <td>
                  <xsl:value-of select="ReleaseTypeName"/>
                </td>
                <td>
                  <xsl:value-of select="ReleaseStatusName"/>
                </td>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="StartDate" />
                  </xsl:call-template>
                </td>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="EndDate" />
                  </xsl:call-template>
                </td>
                <td>
                  RL<xsl:value-of select="ReleaseId"/>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="Attachments/Attachment">
          <div class="Title4">
            File Attachments:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Filename</th>
              <th>Description</th>
              <th>Author</th>
              <th>Date Uploaded</th>
            </tr>
            <xsl:for-each select="Attachments/Attachment">
              <tr>
                <td>
                  <xsl:value-of select="Filename"/>
                </td>
                <td>
                  <xsl:value-of select="Description"/>
                </td>
                <td>
                  <xsl:value-of select="AuthorName"/>
                </td>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="UploadDate" />
                  </xsl:call-template>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="History/ArtifactHistory">
          <div class="Title4">
            Test Case Change History:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Change Date</th>
              <th>Field Name</th>
              <th>Old Value</th>
              <th>New Value</th>
              <th>Changed By</th>
            </tr>
            <xsl:for-each select="History/ArtifactHistory">
              <tr>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="ChangeDate" />
                  </xsl:call-template>
                </td>
                <td>
                  <xsl:value-of select="FieldName"/>
                </td>
                <td>
                  <xsl:value-of select="OldValue" disable-output-escaping="yes"/>
                </td>
                <td>
                  <xsl:value-of select="NewValue" disable-output-escaping="yes"/>
                </td>
                <td>
                  <xsl:value-of select="UserName"/>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <br/>
      </xsl:for-each>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-before(substring-after($datetime, ''T''), ''.'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year , '' '', $time)" />
  </xsl:template>
</xsl:stylesheet>', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (7, 8, N'TestSetList', N'Test Set List', N'This section displays a simple table containing all the test sets with a column for each of their standard and custom fields.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/TestSetData">
    <table class="DataGrid" style="width:100%">
      <tr>
        <th>Test Set #</th>
        <th>Name</th>
        <th>Description</th>
        <th>Status</th>
        <th>Release</th>
        <th>Creator</th>
        <th>Owner</th>
        <th>Automation Host</th>
        <th>Created On</th>
        <th>Planned Date</th>
        <th>Last Modified</th>
        <th>Last Executed</th>
        <th># Passed</th>
        <th># Failed</th>
        <th># Caution</th>
        <th># Blocked</th>
        <th># Not Run</th>
        <th># N/A</th>
        <th>Est. Duration</th>
        <th>Act. Duration</th>
        <xsl:for-each select="(//TestSet)[1]/CustomProperties/CustomProperty">
          <th>
            <xsl:value-of select="Alias"/>
          </th>
        </xsl:for-each>
      </tr>
      <xsl:for-each select="TestSetFolder">
        <xsl:if test="TestSetFolderId != ''0''">
          <tr>
            <td>
              <b>
                <xsl:value-of select="TestSetFolderId"/>
              </b>
            </td>
            <td>
              <xsl:attribute name="style">
                padding-left: <xsl:value-of select="string-length(IndentLevel)*2"/>px;
              </xsl:attribute>
              <b>
                <xsl:value-of select="Name"/>
              </b>
            </td>
            <td>
              <xsl:value-of select="Description" disable-output-escaping="yes"/>
            </td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="LastUpdateDate" />
              </xsl:call-template>
            </td>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="ExecutionDate" />
              </xsl:call-template>
            </td>
            <td>
              <xsl:value-of select="CountPassed"/>
            </td>
            <td>
              <xsl:value-of select="CountFailed"/>
            </td>
            <td>
              <xsl:value-of select="CountCaution"/>
            </td>
            <td>
              <xsl:value-of select="CountBlocked"/>
            </td>
            <td>
              <xsl:value-of select="CountNotRun"/>
            </td>
            <td>
              <xsl:value-of select="CountNotApplicable"/>
            </td>
            <td>
			  <xsl:variable name="durationInDays" select="EstimatedDuration"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <td>
			  <xsl:variable name="durationInDays" select="ActualDuration"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <xsl:for-each select="(//TestSet)[1]/CustomProperties/CustomProperty">
              <td>
              </td>
            </xsl:for-each>
          </tr>
        </xsl:if>
        <xsl:for-each select="TestSets/TestSet">
          <tr>
            <td>
              TX<xsl:value-of select="TestSetId"/>
            </td>
            <td>
              <xsl:value-of select="Name"/>
            </td>
            <td>
              <xsl:value-of select="Description" disable-output-escaping="yes"/>
            </td>
            <td>
              <xsl:value-of select="TestSetStatusName"/>
            </td>
            <td>
              <xsl:value-of select="ReleaseVersionNumber"/>
            </td>
            <td>
              <xsl:value-of select="CreatorName"/>
            </td>
            <td>
              <xsl:value-of select="OwnerName"/>
            </td>
            <td>
              <xsl:value-of select="AutomationHostName"/>
            </td>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="CreationDate" />
              </xsl:call-template>
            </td>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="PlannedDate" />
              </xsl:call-template>
            </td>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="LastUpdateDate" />
              </xsl:call-template>
            </td>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="ExecutionDate" />
              </xsl:call-template>
            </td>
            <td>
              <xsl:value-of select="CountPassed"/>
            </td>
            <td>
              <xsl:value-of select="CountFailed"/>
            </td>
            <td>
              <xsl:value-of select="CountCaution"/>
            </td>
            <td>
              <xsl:value-of select="CountBlocked"/>
            </td>
            <td>
              <xsl:value-of select="CountNotRun"/>
            </td>
            <td>
              <xsl:value-of select="CountNotApplicable"/>
            </td>
            <td>
			  <xsl:variable name="durationInDays" select="EstimatedDuration"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <td>
			  <xsl:variable name="durationInDays" select="ActualDuration"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <xsl:for-each select="CustomProperties/CustomProperty">
              <xsl:choose>
                <xsl:when test="Type=''Date''">
                  <td class="Date">
                    <xsl:call-template name="format-date">
                      <xsl:with-param name="datetime" select="Value" />
                    </xsl:call-template>
                  </td>
                </xsl:when>
                <xsl:otherwise>
                  <td>
                    <xsl:value-of select="Value" disable-output-escaping="yes"/>
                  </td>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:for-each>
          </tr>
        </xsl:for-each>
      </xsl:for-each>
    </table>
  </xsl:template>
  <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-before(substring-after($datetime, ''T''), ''.'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year , '' '', $time)" />
  </xsl:template>
</xsl:stylesheet>

', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (8, 8, N'TestSetDetails', N'Test Set Details', N'This section displays all of the test sets in a document format with embedded tables for each of the associated elements.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/TestSetData">
    <xsl:for-each select="TestSetFolder">
      <div class="Title2">
        Folder: <xsl:value-of select="Name"/>
      </div>
      <div class="Description">
        <xsl:value-of select="Description" disable-output-escaping="yes"/>
      </div>
      <xsl:for-each select="TestSets/TestSet">
        <div class="Title3">
          Test Set TX:<xsl:value-of select="TestSetId"/> - <xsl:value-of select="Name"/>
        </div>
        <div class="Description">
          <xsl:value-of select="Description" disable-output-escaping="yes"/>
        </div>
        <table class="HiddenTable">
          <tr>
            <th>Owner:</th>
            <td>
              <xsl:value-of select="OwnerName"/>
            </td>
            <th>Creator:</th>
            <td>
              <xsl:value-of select="CreatorName"/>
            </td>
          </tr>
          <tr>
            <th>Release:</th>
            <td>
              <xsl:value-of select="ReleaseVersionNumber"/>
            </td>
            <th>Creation Date:</th>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="CreationDate" />
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <th>Status:</th>
            <td>
              <xsl:value-of select="TestSetStatusName"/>
            </td>
            <th>Last Execution:</th>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="ExecutionDate" />
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <th>Planned Date:</th>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="PlannedDate" />
              </xsl:call-template>
            </td>
            <th>Last Updated:</th>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="LastUpdateDate" />
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <th>Est. Duration:</th>
            <td>
			  <xsl:variable name="durationInDays" select="EstimatedDuration"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <th>Act. Duration:</th>
            <td>
			   <xsl:variable name="durationInDays" select="ActualDuration"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
          </tr>
          <tr>
            <th>Automation Host:</th>
            <td>
              <xsl:value-of select="AutomationHostName"/>
            </td>
            <th></th>
            <td>
            </td>
          </tr>
          <xsl:for-each select="CustomProperties/CustomProperty">
            <tr>
              <th>
                <xsl:value-of select="Alias"/>:
              </th>
              <xsl:choose>
                <xsl:when test="Type=''Date''">
                  <td class="Date" colspan="3">
                    <xsl:call-template name="format-date">
                      <xsl:with-param name="datetime" select="Value" />
                    </xsl:call-template>
                  </td>
                </xsl:when>
                <xsl:otherwise>
                  <td colspan="3">
                    <xsl:value-of select="Value" disable-output-escaping="yes"/>
                  </td>
                </xsl:otherwise>
              </xsl:choose>
            </tr>
          </xsl:for-each>
        </table>
        <xsl:if test="Discussions/Discussion">
          <div class="Title3">
            Comments:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Date</th>
              <th>Creator</th>
              <th>Comment</th>
              <th>Id</th>
            </tr>
            <xsl:for-each select="Discussions/Discussion">
              <tr>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="CreationDate" />
                  </xsl:call-template>
                </td>
                <td>
                  <xsl:value-of select="CreatorName"/>
                </td>
                <td>
                  <xsl:value-of select="Text" disable-output-escaping="yes"/>
                </td>
                <td>
                  <xsl:value-of select="DiscussionId"/>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
      
        <xsl:if test="Parameters/TestSetParameter">
          <div class="Title3">
            Test Set Parameter Values:
          </div>
            <ul>
              <xsl:for-each select="Parameters/TestSetParameter">
                <li>
                  <xsl:value-of select="Name"/> = <xsl:value-of select="Value"/>
                </li>
              </xsl:for-each>
            </ul>
        </xsl:if>
          
        <xsl:if test="TestCases/TestCase">
          <div class="Title3">
            Test Cases:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Test #</th>
              <th>Name</th>
              <th>Parameters</th>
              <th>Owner</th>
              <th>Priority</th>
              <th>Est. Duration</th>
              <th>Act. Duration</th>
              <th>Status</th>
              <th>Last Execution Date</th>
            </tr>
            <xsl:for-each select="TestCases/TestCase">
              <tr>
                <td>
                  TC<xsl:value-of select="TestCaseId"/>
                </td>
                <td>
                  <xsl:value-of select="Name"/>
                </td>
                <td>
                  <xsl:for-each select="Parameters">
                    <ul>
                      <xsl:for-each select="TestSetTestCaseParameter">
                        <li>
                          <xsl:value-of select="Name"/> = <xsl:value-of select="Value"/>
                        </li>
                      </xsl:for-each>
                    </ul>
                  </xsl:for-each>
                </td>
                <td>
                  <xsl:value-of select="OwnerName"/>
                </td>
                <td>
                  <xsl:value-of select="TestCasePriorityName"/>
                </td>
                <td>
				  <xsl:variable name="durationInDays" select="EstimatedDuration"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
                </td>
                <td>
				  <xsl:variable name="durationInDays" select="ActualDuration"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
                </td>
                <td>
                  <xsl:value-of select="ExecutionStatusName"/>
                </td>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="ExecutionDate" />
                  </xsl:call-template>
                </td>
              </tr>
              <xsl:if test="TestSteps/TestStep">
                <tr>
                  <td colspan="9">
                    <table class="DataGrid">
                      <tr>
                        <th>Step</th>
                        <th>Description</th>
                        <th>Expected Result</th>
                        <th>Sample Data</th>
                        <th>Status</th>
                      </tr>
                      <xsl:for-each select="TestSteps/TestStep">
                        <tr>
                          <td>
                            <xsl:value-of select="position()"/>
                          </td>
                          <xsl:if test="string(LinkedTestCaseId)">
                            <td colspan="3">
                              <xsl:value-of select="Description" disable-output-escaping="yes"/>
                              ''<xsl:value-of select="LinkedTestCaseName"/>''
                              <xsl:for-each select="Parameters">
                                <i>
                                  with
                                  <xsl:for-each select="TestStepParameter">
                                    <xsl:value-of select="Name"/> = ''<xsl:value-of select="Value"/>'',
                                  </xsl:for-each>
                                </i>
                              </xsl:for-each>
                            </td>
                          </xsl:if>
                          <xsl:if test="not(string(LinkedTestCaseId))">
                            <td>
                              <xsl:value-of select="Description" disable-output-escaping="yes"/>
                            </td>
                            <td>
                              <xsl:value-of select="ExpectedResult" disable-output-escaping="yes"/>
                            </td>
                            <td>
                              <xsl:value-of select="SampleData" disable-output-escaping="yes"/>
                            </td>
                          </xsl:if>
                          <td>
                            <xsl:value-of select="ExecutionStatusName"/>
                          </td>
                        </tr>
                      </xsl:for-each>
                    </table>
                  </td>
                </tr>
              </xsl:if>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="TestRuns/TestRun">
          <div class="Title3">
            Test Runs:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Run #</th>
              <th>Name</th>
              <th>Tester</th>
              <th>Release</th>
              <th>Version</th>
              <th>Status</th>
              <th>Type</th>
              <th>Automation Host</th>
              <th>Automation Engine</th>
              <th>Est. Duration</th>
              <th>Actual Duration</th>
              <th>Execution Date</th>
            </tr>
            <xsl:for-each select="TestRuns/TestRun">
              <tr>
                <td>
                  TR<xsl:value-of select="TestRunId"/>
                </td>
                <td>
                  <xsl:value-of select="Name"/>
                </td>
                <td>
                  <xsl:value-of select="TesterName"/>
                </td>
                <td>
                  <xsl:value-of select="ReleaseName"/>
                </td>
                <td>
                  <xsl:value-of select="ReleaseVersionNumber"/>
                </td>
                <td>
                  <xsl:value-of select="ExecutionStatusName"/>
                </td>
                <td>
                  <xsl:value-of select="TestRunTypeName"/>
                </td>
                <td>
                  <xsl:value-of select="AutomationHostName"/>
                </td>
                <td>
                  <xsl:value-of select="RunnerName"/>
                </td>
                <td>
				  <xsl:variable name="durationInDays" select="EstimatedDuration"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
                </td>
                <td>
				  <xsl:variable name="durationInDays" select="ActualDuration"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
       <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
                </td>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="EndDate" />
                  </xsl:call-template>
                </td>
              </tr>
              <xsl:if test="TestRunSteps/TestRunStep">
                <tr>
                  <td colspan="12">
                    <table class="DataGrid">
                      <tr>
                        <th>Step</th>
                        <th>Description</th>
                        <th>Expected Result</th>
                        <th>Sample Data</th>
                        <th>ActualResult</th>
                        <th>Status</th>
                      </tr>
                      <xsl:for-each select="TestRunSteps/TestRunStep">
                        <tr>
                          <td>
                            <xsl:value-of select="Position"/>
                          </td>
                          <td>
                            <xsl:value-of select="Description" disable-output-escaping="yes"/>
                          </td>
                          <td>
                            <xsl:value-of select="ExpectedResult" disable-output-escaping="yes"/>
                          </td>
                          <td>
                            <xsl:value-of select="SampleData" disable-output-escaping="yes"/>
                          </td>
                          <td>
                            <xsl:value-of select="ActualResult" disable-output-escaping="yes"/>
                          </td>
                          <td>
                            <xsl:value-of select="ExecutionStatusName"/>
                          </td>
                        </tr>
                      </xsl:for-each>
                    </table>
                  </td>
                </tr>
              </xsl:if>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="Attachments/Attachment">
          <div class="Title3">
            File Attachments:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Filename</th>
              <th>Description</th>
              <th>Author</th>
              <th>Date Uploaded</th>
            </tr>
            <xsl:for-each select="Attachments/Attachment">
              <tr>
                <td>
                  <xsl:value-of select="Filename"/>
                </td>
                <td>
                  <xsl:value-of select="Description"/>
                </td>
                <td>
                  <xsl:value-of select="AuthorName"/>
                </td>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="UploadDate" />
                  </xsl:call-template>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="History/ArtifactHistory">
          <div class="Title3">
            Test Set Change History:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Change Date</th>
              <th>Field Name</th>
              <th>Old Value</th>
              <th>New Value</th>
              <th>Changed By</th>
            </tr>
            <xsl:for-each select="History/ArtifactHistory">
              <tr>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="ChangeDate" />
                  </xsl:call-template>
                </td>
                <td>
                  <xsl:value-of select="FieldName"/>
                </td>
                <td>
                  <xsl:value-of select="OldValue" disable-output-escaping="yes"/>
                </td>
                <td>
                  <xsl:value-of select="NewValue" disable-output-escaping="yes"/>
                </td>
                <td>
                  <xsl:value-of select="UserName"/>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <br/>
      </xsl:for-each>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-before(substring-after($datetime, ''T''), ''.'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year , '' '', $time)" />
  </xsl:template>
</xsl:stylesheet>', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (9, 2, N'TestPrintable', N'Printable Test Scripts', N'This section displays the selected test cases in a format that is designed for printing and handing to testers. It traverses all the linked test cases and recursively adds them to the test step list.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/TestRunData">
    <xsl:for-each select="TestRun">
      <div class="Title2">
        Test #<xsl:value-of select="TestCaseId"/> - <xsl:value-of select="Name"/>
      </div>
      <div class="Description">
        <xsl:value-of select="Description" disable-output-escaping="yes"/>
      </div>

      <xsl:if test="TestRunSteps">
        <div class="Title3">
          Test Steps:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Step</th>
            <th>Description</th>
            <th>Expected Result</th>
            <th>Sample Data</th>
            <th>Status</th>
            <th>Actual Result</th>
          </tr>
          <xsl:for-each select="TestRunSteps/TestRunStep">
            <tr>
              <td>
                <xsl:value-of select="position()"/>
              </td>
              <td>
                <xsl:value-of select="Description" disable-output-escaping="yes"/>
              </td>
              <td>
                <xsl:value-of select="ExpectedResult" disable-output-escaping="yes"/>
              </td>
              <td>
                <xsl:value-of select="SampleData" disable-output-escaping="yes"/>
              </td>
              <td style="width:150px">
                <input type="checkbox"></input>Passed <input type="checkbox"></input>Failed<br></br>
                <input type="checkbox"></input>Blocked <input type="checkbox"></input>Caution
              </td>
              <td style="width:200px">
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>

      <div class="Spacer"></div>
      <xsl:if test="Attachments/Attachment">
        <div class="Title3">
          File Attachments:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Filename</th>
            <th>Description</th>
            <th>Author</th>
            <th>Date Uploaded</th>
          </tr>
          <xsl:for-each select="Attachments/Attachment">
            <tr>
              <td>
                <xsl:value-of select="Filename"/>
              </td>
              <td>
                <xsl:value-of select="Description"/>
              </td>
              <td>
                <xsl:value-of select="AuthorName"/>
              </td>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="UploadDate" />
                </xsl:call-template>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <br/>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-before(substring-after($datetime, ''T''), ''.'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year , '' '', $time)" />
  </xsl:template>
</xsl:stylesheet>

', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (10, 5, N'TestRunList', N'Test Run List', N'This section displays a simple table containing all the test runs with a column for each of their standard and custom fields.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/TestRunData">
    <table class="DataGrid" style="width:100%">
      <tr>
        <th>Test Run #</th>
        <th>Name</th>
        <th>Test Case #</th>
        <xsl:if test="TestRun/TestRunSteps">
          <th>Test Step</th>
          <th>Test Step Description</th>
          <th>Test Step Expected Result</th>
          <th>Test Step Sample Data</th>
          <th>Test Step Actual Result</th>
        </xsl:if>
        <th>Status</th>
        <th>Release</th>
        <th>Build</th>
        <th>Test Set</th>
        <th>Type</th>
        <th>Tester</th>
        <th>Est. Duration</th>
        <th>Actual Duration</th>
        <th>Start Date</th>
        <th>End Date</th>
        <th>Automation Engine</th>
        <th>Automation Host</th>
        <th>Message</th>
        <xsl:for-each select="TestRun[1]/CustomProperties/CustomProperty">
          <th>
            <xsl:value-of select="Alias"/>
          </th>
        </xsl:for-each>
      </tr>
      <xsl:for-each select="TestRun">
        <tr>
          <td>
            TR<xsl:value-of select="TestRunId"/>
          </td>
          <td>
            <xsl:value-of select="Name"/>
          </td>
          <td>
            TC<xsl:value-of select="TestCaseId"/>
          </td>
          <xsl:if test="TestRunSteps">
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
          </xsl:if>
          <td>
            <xsl:value-of select="ExecutionStatusName"/>
          </td>
          <td>
            <xsl:value-of select="ReleaseVersionNumber"/>
          </td>
          <td>
            <xsl:value-of select="BuildName"/>
          </td>
          <td>
            <xsl:value-of select="TestSetName"/>
          </td>
          <td>
            <xsl:value-of select="TestRunTypeName"/>
          </td>
          <td>
            <xsl:value-of select="TesterName"/>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="EstimatedDuration"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="ActualDuration"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="StartDate" />
            </xsl:call-template>
          </td>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="EndDate" />
            </xsl:call-template>
          </td>
          <td>
            <xsl:value-of select="RunnerName"/>
          </td>
          <td>
            <xsl:value-of select="AutomationHostName"/>
          </td>
          <td>
            <xsl:value-of select="RunnerMessage"/>
          </td>
          <xsl:for-each select="CustomProperties/CustomProperty">
            <xsl:choose>
              <xsl:when test="Type=''Date''">
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="Value" />
                  </xsl:call-template>
                </td>
              </xsl:when>
              <xsl:otherwise>
                <td>
                  <xsl:value-of select="Value" disable-output-escaping="yes"/>
                </td>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>
        </tr>
        <xsl:for-each select="TestRunSteps/TestRunStep">
          <tr>
            <td>
              RS<xsl:value-of select="TestRunStepId"/>
            </td>
            <td></td>
            <td></td>
            <td>
              <xsl:value-of select="position()"/>
            </td>
            <td>
              <xsl:value-of select="Description" disable-output-escaping="yes"/>
            </td>
            <td>
              <xsl:value-of select="ExpectedResult" disable-output-escaping="yes"/>
            </td>
            <td>
              <xsl:value-of select="SampleData" disable-output-escaping="yes"/>
            </td>
            <td>
              <xsl:value-of select="ActualResult" disable-output-escaping="yes"/>
            </td>
            <td>
              <xsl:value-of select="ExecutionStatusName"/>
            </td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <xsl:for-each select="../../CustomProperties/CustomProperty">
              <td>
              </td>
            </xsl:for-each>
          </tr>
        </xsl:for-each>
      </xsl:for-each>
    </table>
  </xsl:template>
  <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-before(substring-after($datetime, ''T''), ''.'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year , '' '', $time)" />
  </xsl:template>
</xsl:stylesheet>

', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (11, 5, N'TestRunDetails', N'Test Run Details', N'This section displays all of the test runs in a document format with embedded tables for each of the associated elements.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/TestRunData">
    <xsl:for-each select="TestRun">
      <div class="Title2">
        Test Run TR:<xsl:value-of select="TestRunId"/> - <xsl:value-of select="Name"/>
      </div>
      <div class="Description">
        <xsl:value-of select="Description" disable-output-escaping="yes"/>
      </div>
      <table class="HiddenTable">
        <tr>
          <th>Test Case #:</th>
          <td>
            TC<xsl:value-of select="TestCaseId"/>
          </td>
          <td colspan="2"></td>
        </tr>
        <tr>
          <th>Release #:</th>
          <td>
            <xsl:value-of select="ReleaseVersionNumber"/>
            <xsl:if test="BuildName">
              (Build: <xsl:value-of select="BuildName"/>)
            </xsl:if>
          </td>
          <th>Status:</th>
          <td>
            <xsl:value-of select="ExecutionStatusName"/>
          </td>
        </tr>
        <tr>
          <th>Tester:</th>
          <td>
            <xsl:value-of select="TesterName"/>
          </td>
          <th>Execution Date:</th>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="StartDate" />
            </xsl:call-template>
          </td>
        </tr>
        <tr>
          <th>Automation Host:</th>
          <td>
            <xsl:value-of select="AutomationHostName"/>
          </td>
          <th>Test Set:</th>
          <td>
            <xsl:value-of select="TestSetName"/>
          </td>
        </tr>
        <tr>
          <th>Est. Duration:</th>
          <td>
			<xsl:variable name="durationInDays" select="EstimatedDuration"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <th>Actual Duration:</th>
          <td>
			<xsl:variable name="durationInDays" select="ActualDuration"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
       <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
        </tr>
        <xsl:if test="RunnerName">
          <tr>
            <th>Automation Engine:</th>
            <td>
              <xsl:value-of select="RunnerName"/>
            </td>
            <th>Runner Test:</th>
            <td>
              <xsl:value-of select="RunnerTestName"/>
            </td>
          </tr>
          <tr>
            <th># Asserts:</th>
            <td>
              <xsl:value-of select="RunnerAssertCount"/>
            </td>
            <th>Message:</th>
            <td>
              <xsl:value-of select="RunnerMessage"/>
            </td>
          </tr>
          <tr>
            <th style="vertical-align:top">Stack Trace:</th>
            <td colspan="3">
              <pre>
                <xsl:value-of select="RunnerStackTrace"/>
              </pre>
            </td>
          </tr>
        </xsl:if>
        <xsl:for-each select="CustomProperties/CustomProperty">
          <tr>
            <th>
              <xsl:value-of select="Alias"/>:
            </th>
            <xsl:choose>
              <xsl:when test="Type=''Date''">
                <td class="Date" colspan="3">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="Value" />
                  </xsl:call-template>
                </td>
              </xsl:when>
              <xsl:otherwise>
                <td colspan="3">
                  <xsl:value-of select="Value" disable-output-escaping="yes"/>
                </td>
              </xsl:otherwise>
            </xsl:choose>
          </tr>
        </xsl:for-each>
      </table>
      <xsl:if test="TestRunSteps/TestRunStep">
        <table class="DataGrid">
          <tr>
            <th>Step</th>
            <th>Description</th>
            <th>Expected Result</th>
            <th>Sample Data</th>
            <th>ActualResult</th>
            <th>Status</th>
          </tr>
          <xsl:for-each select="TestRunSteps/TestRunStep">
            <tr>
              <td>
                <xsl:value-of select="Position"/>
              </td>
              <td>
                <xsl:value-of select="Description" disable-output-escaping="yes"/>
              </td>
              <td>
                <xsl:value-of select="ExpectedResult" disable-output-escaping="yes"/>
              </td>
              <td>
                <xsl:value-of select="SampleData" disable-output-escaping="yes"/>
              </td>
              <td>
                <xsl:value-of select="ActualResult" disable-output-escaping="yes"/>
              </td>
              <td>
                <xsl:value-of select="ExecutionStatusName"/>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <xsl:if test="Incidents/Incident">
        <div class="Title3">
          Associated Incidents:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Inc #</th>
            <th>Type</th>
            <th>Status</th>
            <th>Priority</th>
            <th>Severity</th>
            <th>Name</th>
            <th>Owned By</th>
            <th>Detected On</th>
          </tr>
          <xsl:for-each select="Incidents/Incident">
            <tr>
              <td>
                <xsl:value-of select="IncidentId"/>
              </td>
              <td>
                <xsl:value-of select="IncidentTypeName"/>
              </td>
              <td>
                <xsl:value-of select="IncidentStatusName"/>
              </td>
              <td>
                <xsl:value-of select="PriorityName"/>
              </td>
              <td>
                <xsl:value-of select="SeverityName"/>
              </td>
              <td>
                <xsl:value-of select="Name"/>
              </td>
              <td>
                <xsl:value-of select="OwnerName"/>
              </td>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="CreationDate" />
                </xsl:call-template>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <xsl:if test="Attachments/Attachment">
        <div class="Title3">
          File Attachments:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Filename</th>
            <th>Description</th>
            <th>Author</th>
            <th>Date Uploaded</th>
          </tr>
          <xsl:for-each select="Attachments/Attachment">
            <tr>
              <td>
                <xsl:value-of select="Filename"/>
              </td>
              <td>
                <xsl:value-of select="Description"/>
              </td>
              <td>
                <xsl:value-of select="AuthorName"/>
              </td>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="UploadDate" />
                </xsl:call-template>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <br/>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-before(substring-after($datetime, ''T''), ''.'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year , '' '', $time)" />
  </xsl:template>
</xsl:stylesheet>', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (12, 3, N'IncidentList', N'Incident List', N'This section displays a simple table containing all the incidents with a column for each of their standard and custom fields.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/IncidentData">
    <table class="DataGrid" style="width:100%">
      <tr>
        <th>Inc #</th>
        <th>Name</th>
        <th>Description</th>
        <th>Resolution</th>
        <th>Type</th>
        <th>Status</th>
        <th>Priority</th>
        <th>Severity</th>
        <th>Detected By</th>
        <th>Owned By</th>
        <th>Detected On</th>
        <th>Last Modified</th>
        <th>Closed On</th>
        <th>Detected Release</th>
        <th>Planned Release</th>
        <th>Verified Release</th>
        <th>Fixed Build</th>
        <th>Component(s)</th>
        <th>% Complete</th>
        <th>Est. Effort</th>
        <th>Actual Effort</th>
        <th>Remaining Effort</th>
        <th>Projected Effort</th>
        <xsl:for-each select="Incident[1]/CustomProperties/CustomProperty">
          <th>
            <xsl:value-of select="Alias"/>
          </th>
        </xsl:for-each>
      </tr>
      <xsl:for-each select="Incident">
        <tr>
          <td>
            <xsl:value-of select="IncidentId"/>
          </td>
          <td>
            <xsl:value-of select="Name"/>
          </td>
          <td>
            <xsl:value-of select="Description" disable-output-escaping="yes"/>
          </td>
          <td>
            <xsl:value-of select="IncidentResolutions/IncidentResolution[last()]/Resolution" disable-output-escaping="yes"/>
          </td>
          <td>
            <xsl:value-of select="IncidentTypeName"/>
          </td>
          <td>
            <xsl:value-of select="IncidentStatusName"/>
          </td>
          <td>
            <xsl:value-of select="PriorityName"/>
          </td>
          <td>
            <xsl:value-of select="SeverityName"/>
          </td>
          <td>
            <xsl:value-of select="OpenerName"/>
          </td>
          <td>
            <xsl:value-of select="OwnerName"/>
          </td>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="CreationDate" />
            </xsl:call-template>
          </td>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="LastUpdateDate" />
            </xsl:call-template>
          </td>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="ClosedDate" />
            </xsl:call-template>
          </td>
          <td>
            <xsl:value-of select="DetectedReleaseVersionNumber"/>
          </td>
          <td>
            <xsl:value-of select="ResolvedReleaseVersionNumber"/>
          </td>
          <td>
            <xsl:value-of select="VerifiedReleaseVersionNumber"/>
          </td>
          <td>
            <xsl:value-of select="BuildName"/>
          </td>
          <td>
            <xsl:value-of select="ComponentNames"/>
          </td>
          <td>
            <xsl:value-of select="CompletionPercent"/>%
          </td>
          <td>
			<xsl:variable name="durationInDays" select="EstimatedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="ActualEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="RemainingEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="ProjectedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <xsl:for-each select="CustomProperties/CustomProperty">
            <xsl:choose>
              <xsl:when test="Type=''Date''">
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="Value" />
                  </xsl:call-template>
                </td>
              </xsl:when>
              <xsl:otherwise>
                <td>
                  <xsl:value-of select="Value" disable-output-escaping="yes" />
                </td>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>
  <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-before(substring-after($datetime, ''T''), ''.'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year , '' '', $time)" />
  </xsl:template>
</xsl:stylesheet>

', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (13, 3, N'IncidentDetails', N'Incident Details', N'This section displays all of the incidents in a document format with embedded tables for each of the associated elements.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/IncidentData">
    <xsl:for-each select="Incident">
      <div class="Title2">
        IN:<xsl:value-of select="IncidentId"/> - <xsl:value-of select="Name"/>
      </div>
      <div class="Description">
        <xsl:value-of select="Description" disable-output-escaping="yes"/>
      </div>
      <table class="HiddenTable">
        <tr>
          <th>Type:</th>
          <td>
            <xsl:value-of select="IncidentTypeName"/>
          </td>
          <th>Priority:</th>
          <td>
            <xsl:value-of select="PriorityName"/>
          </td>
        </tr>
        <tr>
          <th>Status:</th>
          <td>
            <xsl:value-of select="IncidentStatusName"/>
          </td>
          <th>Severity:</th>
          <td>
            <xsl:value-of select="SeverityName"/>
          </td>
        </tr>
        <tr>
          <th>Opened By:</th>
          <td>
            <xsl:value-of select="OpenerName"/>
          </td>
          <th>Opened On:</th>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="CreationDate" />
            </xsl:call-template>
          </td>
        </tr>
        <tr>
          <th>Assigned To:</th>
          <td>
            <xsl:value-of select="OwnerName"/>
          </td>
          <th>Last Modified:</th>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="LastUpdateDate" />
            </xsl:call-template>
          </td>
        </tr>
        <tr>
          <th>Detected In Release:</th>
          <td>
            <xsl:value-of select="DetectedReleaseVersionNumber"/>
          </td>
          <th>Closed On:</th>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="ClosedDate" />
            </xsl:call-template>
          </td>
        </tr>
        <tr>
          <th>Planned For Release:</th>
          <td>
            <xsl:value-of select="ResolvedReleaseVersionNumber"/>
          </td>
          <th>Verified In Release:</th>
          <td>
            <xsl:value-of select="VerifiedReleaseVersionNumber"/>
          </td>
        </tr>
        <tr>
          <th>Component(s):</th>
          <td>
            <xsl:value-of select="ComponentNames"/>
          </td>
          <th>Fixed Build:</th>
          <td>
            <xsl:value-of select="BuildName"/>
          </td>
        </tr>

        <tr>
          <th>Estimated Effort:</th>
          <td>
			  <xsl:variable name="durationInDays" select="EstimatedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <th>Actual Effort:</th>
          <td>
			<xsl:variable name="durationInDays" select="ActualEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
       <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
        </tr>
        <tr>
          <th>Remaining Effort:</th>
          <td>
			<xsl:variable name="durationInDays" select="RemainingEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <th>Projected Effort:</th>
          <td>
			<xsl:variable name="durationInDays" select="ProjectedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
        </tr>
        <xsl:for-each select="CustomProperties/CustomProperty">
          <tr>
            <th>
              <xsl:value-of select="Alias"/>:
            </th>
            <xsl:choose>
              <xsl:when test="Type=''Date''">
                <td class="Date" colspan="3">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="Value" />
                  </xsl:call-template>
                </td>
              </xsl:when>
              <xsl:otherwise>
                <td colspan="3">
                  <xsl:value-of select="Value" disable-output-escaping="yes"/>
                </td>
              </xsl:otherwise>
            </xsl:choose>
          </tr>
        </xsl:for-each>
      </table>
      <xsl:if test="IncidentResolutions/IncidentResolution">
        <div class="Title3">
          Resolutions/Comments:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Date</th>
            <th>Creator</th>
            <th>Resolution/Comment</th>
            <th>Resolution Id</th>
          </tr>
          <xsl:for-each select="IncidentResolutions/IncidentResolution">
            <tr>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="CreationDate" />
                </xsl:call-template>
              </td>
              <td>
                <xsl:value-of select="CreatorName"/>
              </td>
              <td>
                <xsl:value-of select="Resolution" disable-output-escaping="yes"/>
              </td>
              <td>
                <xsl:value-of select="IncidentResolutionId"/>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <xsl:if test="ArtifactLinks/ArtifactLink">
        <div class="Title3">
          Associations:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Association</th>
            <th>Artifact Type</th>
            <th>Name</th>
            <th>Created By</th>
            <th>Comment</th>
            <th>Date</th>
            <th>Status</th>
            <th>Artifact Id</th>
          </tr>
          <xsl:for-each select="ArtifactLinks/ArtifactLink">
            <tr>
              <td>
                <xsl:value-of select="ArtifactLinkTypeName"/>
              </td>
              <td>
                <xsl:value-of select="ArtifactTypeName"/>
              </td>
              <td>
                <xsl:value-of select="ArtifactName"/>
              </td>
              <td>
                <xsl:value-of select="CreatorName"/>
              </td>
              <td>
                <xsl:value-of select="Comment"/>
              </td>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="CreationDate" />
                </xsl:call-template>
              </td>
              <td>
                <xsl:value-of select="ArtifactStatusName"/>
              </td>
              <td>
                <xsl:if test="ArtifactTypeId = 1">
                  RQ
                </xsl:if>
                <xsl:if test="ArtifactTypeId = 3">
                  IN
                </xsl:if>
                <xsl:if test="ArtifactTypeId = 5">
                  TR
                </xsl:if>
                <xsl:if test="ArtifactTypeId = 6">
                  TK
                </xsl:if>
                <xsl:if test="ArtifactTypeId = 7">
                  TS
                </xsl:if>
                <xsl:value-of select="ArtifactId"/>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <xsl:if test="Attachments/Attachment">
        <div class="Title3">
          File Attachments:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Filename</th>
            <th>Description</th>
            <th>Author</th>
            <th>Date Uploaded</th>
          </tr>
          <xsl:for-each select="Attachments/Attachment">
            <tr>
              <td>
                <xsl:value-of select="Filename"/>
              </td>
              <td>
                <xsl:value-of select="Description"/>
              </td>
              <td>
                <xsl:value-of select="AuthorName"/>
              </td>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="UploadDate" />
                </xsl:call-template>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <xsl:if test="SourceCodeRevisions/SourceCodeRevision">
        <div class="Title3">
          Source Code Revisions:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Name</th>
            <th>Message</th>
            <th>Author</th>
            <th>Date</th>
          </tr>
          <xsl:for-each select="SourceCodeRevisions/SourceCodeRevision">
            <tr>
              <td>
                <xsl:value-of select="Name"/>
              </td>
              <td>
                <xsl:value-of select="Message"/>
              </td>
              <td>
                <xsl:value-of select="AuthorName"/>
              </td>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="UpdateDate" />
                </xsl:call-template>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <xsl:if test="History/ArtifactHistory">
        <div class="Title3">
          Incident Change History:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Change Date</th>
            <th>Field Name</th>
            <th>Old Value</th>
            <th>New Value</th>
            <th>Changed By</th>
          </tr>
          <xsl:for-each select="History/ArtifactHistory">
            <tr>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="ChangeDate" />
                </xsl:call-template>
              </td>
              <td>
                <xsl:value-of select="FieldName"/>
              </td>
              <td>
                <xsl:value-of select="OldValue" disable-output-escaping="yes"/>
              </td>
              <td>
                <xsl:value-of select="NewValue" disable-output-escaping="yes"/>
              </td>
              <td>
                <xsl:value-of select="UserName"/>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <br/>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-before(substring-after($datetime, ''T''), ''.'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year , '' '', $time)" />
  </xsl:template>
</xsl:stylesheet>', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (14, 6, N'TaskList', N'Task List', N'This section displays a simple table containing all the tasks with a column for each of their standard and custom fields.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/TaskData">
    <table class="DataGrid" style="width:100%">
      <tr>
        <th>Task #</th>
        <th>Name</th>
        <th>Description</th>
        <th>Type</th>
        <th>Status</th>
        <th>Priority</th>
        <th>Owned By</th>
        <th>Created On</th>
        <th>Last Modified</th>
        <th>Release #</th>
        <th>Requirement #</th>
        <th>Requirement Name</th>
        <th>Component</th>
        <th>Start Date</th>
        <th>End Date</th>
        <th>% Complete</th>
        <th>Est. Effort</th>
        <th>Actual Effort</th>
        <th>Remaining Effort</th>
        <th>Projected Effort</th>
        <xsl:for-each select="(//Task)[1]/CustomProperties/CustomProperty">
          <th>
            <xsl:value-of select="Alias"/>
          </th>
        </xsl:for-each>
      </tr>
      <xsl:for-each select="TaskFolder">
        <tr>
          <td>
            <b>
              <xsl:value-of select="TaskFolderId"/>
            </b>
          </td>
          <td>
            <xsl:attribute name="style">
              padding-left: <xsl:value-of select="string-length(IndentLevel)*2"/>px;
            </xsl:attribute>
            <b>
              <xsl:value-of select="Name"/>
            </b>
          </td>
          <td>
            <xsl:value-of select="Description" disable-output-escaping="yes"/>
          </td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
          <xsl:for-each select="(//Task)[1]/CustomProperties/CustomProperty">
            <td>
            </td>
          </xsl:for-each>
        </tr>
        <xsl:for-each select="Tasks/Task">
          <tr>
            <td>
              <xsl:value-of select="TaskId"/>
            </td>
            <td>
              <xsl:value-of select="Name"/>
            </td>
            <td>
              <xsl:value-of select="Description" disable-output-escaping="yes"/>
            </td>
            <td>
              <xsl:value-of select="TaskTypeName"/>
            </td>
            <td>
              <xsl:value-of select="TaskStatusName"/>
            </td>
            <td>
              <xsl:value-of select="TaskPriorityName"/>
            </td>
            <td>
              <xsl:value-of select="OwnerName"/>
            </td>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="CreationDate" />
              </xsl:call-template>
            </td>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="LastUpdateDate" />
              </xsl:call-template>
            </td>
            <td>
              <xsl:value-of select="ReleaseVersionNumber"/>
            </td>
            <td>
              RQ<xsl:value-of select="RequirementId"/>
            </td>
            <td>
              <xsl:value-of select="RequirementName"/>
            </td>
            <td>
              <xsl:value-of select="ComponentName"/>
            </td>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="StartDate" />
              </xsl:call-template>
            </td>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="EndDate" />
              </xsl:call-template>
            </td>
            <td>
              <xsl:value-of select="CompletionPercent"/>%
            </td>
            <td>
			  <xsl:variable name="durationInDays" select="EstimatedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
         
            <td>
			   <xsl:variable name="durationInDays" select="ActualEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <td>
			  <xsl:variable name="durationInDays" select="RemainingEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <td>
			   <xsl:variable name="durationInDays" select="ProjectedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>/xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <xsl:for-each select="CustomProperties/CustomProperty">
              <xsl:choose>
                <xsl:when test="Type=''Date''">
                  <td class="Date">
                    <xsl:call-template name="format-date">
                      <xsl:with-param name="datetime" select="Value" />
                    </xsl:call-template>
                  </td>
                </xsl:when>
                <xsl:otherwise>
                  <td>
                    <xsl:value-of select="Value" disable-output-escaping="yes"/>
                  </td>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:for-each>
          </tr>
        </xsl:for-each>
      </xsl:for-each>
    </table>
  </xsl:template>
  <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-before(substring-after($datetime, ''T''), ''.'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year , '' '', $time)" />
  </xsl:template>
</xsl:stylesheet>

', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (15, 6, N'TaskDetails', N'Task Details', N'This section displays all of the tasks in a document format with embedded tables for each of the associated elements.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/TaskData">
    <xsl:for-each select="TaskFolder">
      <div class="Title2">
        Folder: <xsl:value-of select="Name"/>
      </div>
      <xsl:for-each select="Tasks/Task">
        <div class="Title2">
          Task TK:<xsl:value-of select="TaskId"/> - <xsl:value-of select="Name"/>
        </div>
        <div class="Description">
          <xsl:value-of select="Description" disable-output-escaping="yes"/>
        </div>
        <table class="HiddenTable">
          <tr>
            <th>Component:</th>
            <td>
              <xsl:value-of select="ComponentName"/>
            </td>
            <th>Requirement Name:</th>
            <td>
              <xsl:value-of select="RequirementName"/> (RQ <xsl:value-of select="RequirementId"/>)
            </td>
          </tr>
          <tr>
            <th>Status:</th>
            <td>
              <xsl:value-of select="TaskStatusName"/>
            </td>
            <th>Type:</th>
            <td>
              <xsl:value-of select="TaskTypeName"/>
            </td>
          </tr>
          <tr>
            <th>Release #:</th>
            <td>
              <xsl:value-of select="ReleaseVersionNumber"/>
            </td>
            <th>Priority:</th>
            <td>
              <xsl:value-of select="TaskPriorityName"/>
            </td>
          </tr>
          <tr>
            <th>Assigned To:</th>
            <td>
              <xsl:value-of select="OwnerName"/>
            </td>
            <th>Created On:</th>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="CreationDate" />
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <th>Start Date:</th>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="StartDate" />
              </xsl:call-template>
            </td>
            <th>Last Modified:</th>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="LastUpdateDate" />
              </xsl:call-template>
            </td>
          </tr>
          <tr>
            <th>End Date:</th>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="EndDate" />
              </xsl:call-template>
            </td>
            <th>Estimated Effort:</th>
            <td>
			   <xsl:variable name="durationInDays" select="EstimatedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
          </tr>
          <tr>
            <th>% Complete:</th>
            <td>
              <xsl:value-of select="CompletionPercent"/>%
            </td>
            <th>Actual Effort:</th>
            <td>
			  <xsl:variable name="durationInDays" select="ActualEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
      <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
          </tr>
          <tr>
            <th>Remaining Effort:</th>
            <td>
			  <xsl:variable name="durationInDays" select="RemainingEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <th>Projected Effort:</th>
            <td>
			  <xsl:variable name="durationInDays" select="ProjectedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
          </tr>
          <xsl:for-each select="CustomProperties/CustomProperty">
            <tr>
              <th>
                <xsl:value-of select="Alias"/>:
              </th>
              <xsl:choose>
                <xsl:when test="Type=''Date''">
                  <td class="Date" colspan="3">
                    <xsl:call-template name="format-date">
                      <xsl:with-param name="datetime" select="Value" />
                    </xsl:call-template>
                  </td>
                </xsl:when>
                <xsl:otherwise>
                  <td colspan="3">
                    <xsl:value-of select="Value" disable-output-escaping="yes"/>
                  </td>
                </xsl:otherwise>
              </xsl:choose>
            </tr>
          </xsl:for-each>
        </table>
        <xsl:if test="ArtifactLinks/ArtifactLink">
          <div class="Title3">
            Associations:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Association</th>
              <th>Artifact Type</th>
              <th>Name</th>
              <th>Created By</th>
              <th>Comment</th>
              <th>Date</th>
              <th>Status</th>
              <th>Artifact Id</th>
            </tr>
            <xsl:for-each select="ArtifactLinks/ArtifactLink">
              <tr>
                <td>
                  <xsl:value-of select="ArtifactLinkTypeName"/>
                </td>
                <td>
                  <xsl:value-of select="ArtifactTypeName"/>
                </td>
                <td>
                  <xsl:value-of select="ArtifactName"/>
                </td>
                <td>
                  <xsl:value-of select="CreatorName"/>
                </td>
                <td>
                  <xsl:value-of select="Comment"/>
                </td>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="CreationDate" />
                  </xsl:call-template>
                </td>
                <td>
                  <xsl:value-of select="ArtifactStatusName"/>
                </td>
                <td>
                  <xsl:if test="ArtifactTypeId = 1">
                    RQ
                  </xsl:if>
                  <xsl:if test="ArtifactTypeId = 3">
                    IN
                  </xsl:if>
                  <xsl:if test="ArtifactTypeId = 5">
                    TR
                  </xsl:if>
                  <xsl:if test="ArtifactTypeId = 6">
                    TK
                  </xsl:if>
                  <xsl:if test="ArtifactTypeId = 7">
                    TS
                  </xsl:if>
                  <xsl:value-of select="ArtifactId"/>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="Discussions/Discussion">
          <div class="Title3">
            Comments:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Date</th>
              <th>Creator</th>
              <th>Comment</th>
              <th>Id</th>
            </tr>
            <xsl:for-each select="Discussions/Discussion">
              <tr>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="CreationDate" />
                  </xsl:call-template>
                </td>
                <td>
                  <xsl:value-of select="CreatorName"/>
                </td>
                <td>
                  <xsl:value-of select="Text" disable-output-escaping="yes"/>
                </td>
                <td>
                  <xsl:value-of select="DiscussionId"/>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="Attachments/Attachment">
          <div class="Title3">
            File Attachments:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Filename</th>
              <th>Description</th>
              <th>Author</th>
              <th>Date Uploaded</th>
            </tr>
            <xsl:for-each select="Attachments/Attachment">
              <tr>
                <td>
                  <xsl:value-of select="Filename"/>
                </td>
                <td>
                  <xsl:value-of select="Description"/>
                </td>
                <td>
                  <xsl:value-of select="AuthorName"/>
                </td>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="UploadDate" />
                  </xsl:call-template>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="SourceCodeRevisions/SourceCodeRevision">
          <div class="Title3">
            Source Code Revisions:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Name</th>
              <th>Message</th>
              <th>Author</th>
              <th>Date</th>
            </tr>
            <xsl:for-each select="SourceCodeRevisions/SourceCodeRevision">
              <tr>
                <td>
                  <xsl:value-of select="Name"/>
                </td>
                <td>
                  <xsl:value-of select="Message"/>
                </td>
                <td>
                  <xsl:value-of select="AuthorName"/>
                </td>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="UpdateDate" />
                  </xsl:call-template>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <xsl:if test="History/ArtifactHistory">
          <div class="Title3">
            Task Change History:
          </div>
          <table class="DataGrid">
            <tr>
              <th>Change Date</th>
              <th>Field Name</th>
              <th>Old Value</th>
              <th>New Value</th>
              <th>Changed By</th>
            </tr>
            <xsl:for-each select="History/ArtifactHistory">
              <tr>
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="ChangeDate" />
                  </xsl:call-template>
                </td>
                <td>
                  <xsl:value-of select="FieldName"/>
                </td>
                <td>
                  <xsl:value-of select="OldValue" disable-output-escaping="yes"/>
                </td>
                <td>
                  <xsl:value-of select="NewValue" disable-output-escaping="yes"/>
                </td>
                <td>
                  <xsl:value-of select="UserName"/>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <br/>
      </xsl:for-each>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-before(substring-after($datetime, ''T''), ''.'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year , '' '', $time)" />
  </xsl:template>
</xsl:stylesheet>', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (16, 4, N'ReleaseList', N'Release List', N'This section displays a simple table containing all the releases with a column for each of their standard and custom fields.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/ReleaseData">
    <table class="DataGrid" style="width:100%">
      <tr>
        <th colspan="10" style="text-align:center">
          General
        </th>
        <th colspan="5" style="text-align:center">
          Testing Information
        </th>
        <th colspan="7" style="text-align:center">
          Development Information
        </th>
        <th>
          <xsl:attribute name="colspan">
            <xsl:value-of select="count(Release[1]/CustomProperties/CustomProperty)"/>
          </xsl:attribute>
          Custom Properties
        </th>
      </tr>
      <tr>
        <th>Rel #</th>
        <th>Name</th>
        <th>Version Number</th>
        <th>Description</th>
        <th>Creator</th>
        <th>Creation Date</th>
        <th>Type</th>
        <th>Status</th>
        <th>Start Date</th>
        <th>End Date</th>
        <th># Failed</th>
        <th># Passed</th>
        <th># Blocked</th>
        <th># Caution</th>
        <th># Not Run</th>
        <th>Task Progress</th>
        <th>Planned Effort</th>
        <th>Available Effort</th>
        <th>Est. Effort</th>
        <th>Actual Effort</th>
        <th>Remaining Effort</th>
        <th>Projected Effort</th>
        <th># Resources</th>
        <th>Non-Working Days</th>
        <xsl:for-each select="Release[1]/CustomProperties/CustomProperty">
          <th>
            <xsl:value-of select="Alias"/>
          </th>
        </xsl:for-each>
      </tr>
      <xsl:for-each select="Release">
        <tr>
          <td>
            <xsl:value-of select="ReleaseId"/>
          </td>
          <td>
            <xsl:attribute name="style">
              padding-left: <xsl:value-of select="string-length(IndentLevel)*2"/>px;
            </xsl:attribute>
            <xsl:if test="IsIterationOrPhase=''False''">
              <b>
                <xsl:value-of select="Name"/>
              </b>
            </xsl:if>
            <xsl:if test="IsIterationOrPhase=''True''">
              <xsl:value-of select="Name"/>
            </xsl:if>
          </td>
          <td>
            <xsl:value-of select="VersionNumber"/>
          </td>
          <td>
            <xsl:value-of select="Description" disable-output-escaping="yes"/>
          </td>
          <td>
            <xsl:value-of select="CreatorName"/>
          </td>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="CreationDate" />
            </xsl:call-template>
          </td>
          <td>
            <xsl:value-of select="ReleaseTypeName"/>
          </td>
          <td>
            <xsl:value-of select="ReleaseStatusName"/>
          </td>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="StartDate" />
            </xsl:call-template>
          </td>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="EndDate" />
            </xsl:call-template>
          </td>
          <td>
            <xsl:value-of select="CountFailed"/>
          </td>
          <td>
            <xsl:value-of select="CountPassed"/>
          </td>
          <td>
            <xsl:value-of select="CountBlocked"/>
          </td>
          <td>
            <xsl:value-of select="CountCaution"/>
          </td>
          <td>
            <xsl:value-of select="CountNotRun"/>
          </td>
          <td>
            <xsl:value-of select="TaskCount"/><xsl:text> Tasks</xsl:text>
            <xsl:value-of select="TaskPercentOnTime"/><xsl:text>% On Schedule,</xsl:text>
            <xsl:value-of select="TaskPercentLateFinish"/><xsl:text>% Running Late,</xsl:text>
            <xsl:value-of select="TaskPercentNotStart"/><xsl:text>% Starting Late,</xsl:text>
            <xsl:value-of select="TaskPercentLateStart"/><xsl:text>% Not Started</xsl:text>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="PlannedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="AvailableEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="TaskEstimatedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="TaskActualEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="TaskRemainingEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="TaskProjectedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), ''h:'', format-number($minutes, ''00''), ''m'')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
            <xsl:value-of select="ResourceCount"/>
          </td>
          <td>
            <xsl:value-of select="DaysNonWorking"/>
          </td>

          <xsl:for-each select="CustomProperties/CustomProperty">
            <xsl:choose>
              <xsl:when test="Type=''Date''">
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="Value" />
                  </xsl:call-template>
                </td>
              </xsl:when>
              <xsl:otherwise>
                <td>
                  <xsl:value-of select="Value" disable-output-escaping="yes"/>
                </td>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>
  <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-before(substring-after($datetime, ''T''), ''.'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year , '' '', $time)" />
  </xsl:template>
</xsl:stylesheet>

', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (17, 4, N'ReleaseDetails', N'Release Details', N'This section displays all of the releases in a document format with embedded tables for each of the associated elements.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/ReleaseData">
    <xsl:for-each select="Release">
      <div class="Title2">
        <xsl:value-of select="VersionNumber"/> - <xsl:value-of select="Name"/>
      </div>
      <div class="Description">
        <xsl:value-of select="Description" disable-output-escaping="yes"/>
      </div>
      <table class="HiddenTable">
        <tr>
          <th>Version #:</th>
          <td>
            <xsl:value-of select="VersionNumber"/>
          </td>
          <th>Status:</th>
          <td>
            <xsl:value-of select="ReleaseStatusName"/>
          </td>
        </tr>
        <tr>
          <th>Owner:</th>
          <td>
            <xsl:value-of select="OwnerName"/>
          </td>
          <th>Type:</th>
          <td>
            <xsl:value-of select="ReleaseTypeName"/>
          </td>
        </tr>
        <tr>
          <th>Creator:</th>
          <td>
            <xsl:value-of select="CreatorName"/>
          </td>
          <th>Creation Date:</th>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="CreationDate" />
            </xsl:call-template>
          </td>
        </tr>
        <tr>
          <th>Start Date:</th>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="StartDate" />
            </xsl:call-template>
          </td>
          <th>Planned Effort:</th>
          <td>
			<xsl:variable name="durationInDays" select="PlannedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
        </tr>
        <tr>
          <th>End Date:</th>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="EndDate" />
            </xsl:call-template>
          </td>
          <th>Available Effort:</th>
          <td>
			<xsl:variable name="durationInDays" select="AvailableEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
        </tr>
        <tr>
          <th># Resources:</th>
          <td>
            <xsl:value-of select="ResourceCount"/>
          </td>
          <th>Task Est. Effort:</th>
          <td>
			<xsl:variable name="durationInDays" select="TaskEstimatedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
        </tr>
        <tr>
          <th>Non-Working Days:</th>
          <td>
            <xsl:value-of select="DaysNonWorking"/>
          </td>
          <th>Actual Effort:</th>
          <td>
			<xsl:variable name="durationInDays" select="TaskActualEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
        </tr>
        <tr>
          <th>Remaining Effort:</th>
          <td>
			<xsl:variable name="durationInDays" select="TaskRemainingEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <th>Projected Effort:</th>
          <td>
			<xsl:variable name="durationInDays" select="TaskProjectedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
       <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
        </tr>
        <xsl:for-each select="CustomProperties/CustomProperty">
          <tr>
            <th>
              <xsl:value-of select="Alias"/>:
            </th>
            <xsl:choose>
              <xsl:when test="Type=''Date''">
                <td class="Date" colspan="3">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="Value" />
                  </xsl:call-template>
                </td>
              </xsl:when>
              <xsl:otherwise>
                <td colspan="3">
                  <xsl:value-of select="Value" disable-output-escaping="yes"/>
                </td>
              </xsl:otherwise>
            </xsl:choose>
          </tr>
        </xsl:for-each>
      </table>
      <br />
      <xsl:if test="Requirements/Requirement">
        <div class="Title3">
          Requirements Added:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Req #</th>
            <th>Name</th>
            <th>Status</th>
            <th>Priority</th>
          </tr>
          <xsl:for-each select="Requirements/Requirement">
            <tr>
              <td>
                RQ<xsl:value-of select="RequirementId"/>
              </td>
              <td>
                <xsl:attribute name="style">
                  padding-left: <xsl:value-of select="string-length(IndentLevel)*2"/>px;
                </xsl:attribute>
                <xsl:if test="IsSummary=''True''">
                  <b>
                    <xsl:value-of select="Name"/>
                  </b>
                </xsl:if>
                <xsl:if test="IsSummary=''False''">
                  <xsl:value-of select="Name"/>
                </xsl:if>
              </td>
              <td>
                <xsl:value-of select="RequirementStatusName"/>
              </td>
              <td>
                <xsl:value-of select="ImportanceName"/>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <div class="Spacer"></div>
      <xsl:if test="TestCases/TestCase">
        <div class="Title3">
          Test Coverage:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Test #</th>
            <th>Name</th>
            <th>Status</th>
            <th>Last Execution Date</th>
          </tr>
          <xsl:for-each select="TestCases/TestCase">
            <tr>
              <td>
                TC<xsl:value-of select="TestCaseId"/>
              </td>
              <td>
                  <xsl:value-of select="Name"/>
              </td>
              <td>
                <xsl:value-of select="ExecutionStatusName"/>
              </td>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="ExecutionDate" />
                </xsl:call-template>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <xsl:if test="DetectedIncidents/Incident">
        <div class="Title3">
          Detected Incidents:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Inc #</th>
            <th>Type</th>
            <th>Status</th>
            <th>Priority</th>
            <th>Severity</th>
            <th>Name</th>
            <th>Owned By</th>
            <th>Detected On</th>
          </tr>
          <xsl:for-each select="DetectedIncidents/Incident">
            <tr>
              <td>
                IN<xsl:value-of select="IncidentId"/>
              </td>
              <td>
                <xsl:value-of select="IncidentTypeName"/>
              </td>
              <td>
                <xsl:value-of select="IncidentStatusName"/>
              </td>
              <td>
                <xsl:value-of select="PriorityName"/>
              </td>
              <td>
                <xsl:value-of select="SeverityName"/>
              </td>
              <td>
                <xsl:value-of select="Name"/>
              </td>
              <td>
                <xsl:value-of select="OwnerName"/>
              </td>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="CreationDate" />
                </xsl:call-template>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <xsl:if test="ResolvedIncidents/Incident">
        <div class="Title3">
          Resolved Incidents:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Inc #</th>
            <th>Type</th>
            <th>Status</th>
            <th>Priority</th>
            <th>Severity</th>
            <th>Name</th>
            <th>Owned By</th>
            <th>Detected On</th>
          </tr>
          <xsl:for-each select="ResolvedIncidents/Incident">
            <tr>
              <td>
                IN<xsl:value-of select="IncidentId"/>
              </td>
              <td>
                <xsl:value-of select="IncidentTypeName"/>
              </td>
              <td>
                <xsl:value-of select="IncidentStatusName"/>
              </td>
              <td>
                <xsl:value-of select="PriorityName"/>
              </td>
              <td>
                <xsl:value-of select="SeverityName"/>
              </td>
              <td>
                <xsl:value-of select="Name"/>
              </td>
              <td>
                <xsl:value-of select="OwnerName"/>
              </td>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="CreationDate" />
                </xsl:call-template>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <xsl:if test="VerifiedIncidents/Incident">
        <div class="Title3">
          Verified Incidents:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Inc #</th>
            <th>Type</th>
            <th>Status</th>
            <th>Priority</th>
            <th>Severity</th>
            <th>Name</th>
            <th>Owned By</th>
            <th>Detected On</th>
          </tr>
          <xsl:for-each select="VerifiedIncidents/Incident">
            <tr>
              <td>
                IN<xsl:value-of select="IncidentId"/>
              </td>
              <td>
                <xsl:value-of select="IncidentTypeName"/>
              </td>
              <td>
                <xsl:value-of select="IncidentStatusName"/>
              </td>
              <td>
                <xsl:value-of select="PriorityName"/>
              </td>
              <td>
                <xsl:value-of select="SeverityName"/>
              </td>
              <td>
                <xsl:value-of select="Name"/>
              </td>
              <td>
                <xsl:value-of select="OwnerName"/>
              </td>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="CreationDate" />
                </xsl:call-template>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <xsl:if test="Tasks/Task">
        <div class="Title3">
          Associated Tasks:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Task #</th>
            <th>Name</th>
            <th>Status</th>
            <th>Priority</th>
            <th>Owned By</th>
            <th>Start Date</th>
            <th>End Date</th>
            <th>% Completed</th>
            <th>Est. Effort</th>
            <th>Actual Effort</th>
          </tr>
          <xsl:for-each select="Tasks/Task">
            <tr>
              <td>
                TK<xsl:value-of select="TaskId"/>
              </td>
              <td>
                <xsl:value-of select="Name"/>
              </td>
              <td>
                <xsl:value-of select="TaskStatusName"/>
              </td>
              <td>
                <xsl:value-of select="TaskPriorityName"/>
              </td>
              <td>
                <xsl:value-of select="OwnerName"/>
              </td>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="StartDate" />
                </xsl:call-template>
              </td>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="EndDate" />
                </xsl:call-template>
              </td>
              <td>
                <xsl:value-of select="CompletionPercent"/> %
              </td>
              <td>
				<xsl:variable name="durationInDays" select="EstimatedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
              </td>
              <td>
				<xsl:variable name="durationInDays" select="ActualEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <xsl:if test="Discussions/Discussion">
        <div class="Title3">
          Comments:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Date</th>
            <th>Creator</th>
            <th>Comment</th>
            <th>Id</th>
          </tr>
          <xsl:for-each select="Discussions/Discussion">
            <tr>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="CreationDate" />
                </xsl:call-template>
              </td>
              <td>
                <xsl:value-of select="CreatorName"/>
              </td>
              <td>
                <xsl:value-of select="Text" disable-output-escaping="yes"/>
              </td>
              <td>
                <xsl:value-of select="DiscussionId"/>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <xsl:if test="TestRuns/TestRun">
        <div class="Title3">
          Test Runs:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Run #</th>
            <th>Name</th>
            <th>Test Case #</th>
            <th>Test Set</th>
            <th>Tester</th>
            <th>Status</th>
            <th>Est. Duration</th>
            <th>Actual Duration</th>
            <th>Execution Date</th>
          </tr>
          <xsl:for-each select="TestRuns/TestRun">
            <tr>
              <td>
                TR<xsl:value-of select="TestRunId"/>
              </td>
              <td>
                <xsl:value-of select="Name"/>
              </td>
              <td>
                <xsl:value-of select="TestCaseId"/>
              </td>
              <td>
                <xsl:value-of select="TestSetName"/>
              </td>
              <td>
                <xsl:value-of select="TesterName"/>
              </td>
              <td>
                <xsl:value-of select="ExecutionStatusName"/>
              </td>
              <td>
				<xsl:variable name="durationInDays" select="EstimatedDuration"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
       <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
              </td>
              <td>
				<xsl:variable name="durationInDays" select="ActualDuration"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '''')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
              </td>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="EndDate" />
                </xsl:call-template>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <xsl:if test="Attachments/Attachment">
        <div class="Title3">
          File Attachments:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Filename</th>
            <th>Description</th>
            <th>Author</th>
            <th>Date Uploaded</th>
          </tr>
          <xsl:for-each select="Attachments/Attachment">
            <tr>
              <td>
                <xsl:value-of select="Filename"/>
              </td>
              <td>
                <xsl:value-of select="Description"/>
              </td>
              <td>
                <xsl:value-of select="AuthorName"/>
              </td>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="UploadDate" />
                </xsl:call-template>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <xsl:if test="Builds/Build">
        <div class="Title3">
          Builds:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Name</th>
            <th>Description</th>
            <th>Status</th>
            <th>Date</th>
          </tr>
          <xsl:for-each select="Builds/Build">
            <tr>
              <td>
                <xsl:value-of select="Name"/>
              </td>
              <td>
                <xsl:value-of select="Description"/>
              </td>
              <td>
                <xsl:value-of select="BuildStatusName"/>
              </td>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="LastUpdateDate" />
                </xsl:call-template>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <xsl:if test="History/ArtifactHistory">
        <div class="Title3">
          Release/Iteration Change History:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Change Date</th>
            <th>Field Name</th>
            <th>Old Value</th>
            <th>New Value</th>
            <th>Changed By</th>
          </tr>
          <xsl:for-each select="History/ArtifactHistory">
            <tr>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="ChangeDate" />
                </xsl:call-template>
              </td>
              <td>
                <xsl:value-of select="FieldName"/>
              </td>
              <td>
                <xsl:value-of select="OldValue" disable-output-escaping="yes"/>
              </td>
              <td>
                <xsl:value-of select="NewValue" disable-output-escaping="yes"/>
              </td>
              <td>
                <xsl:value-of select="UserName"/>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <br/>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-before(substring-after($datetime, ''T''), ''.'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year , '' '', $time)" />
  </xsl:template>
</xsl:stylesheet>', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (18, 4, N'ReleasePlan', N'Release Plan', N'This section displays a table that contains all of the releases and iterations in the project together with the requirements, incidents and tasks arranged in a single GANTT view.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt">
  <xsl:template match="ReleaseData">
    <table class="DataGrid" style="width:100%">
      <tr>
        <th>ID #</th>
        <th colspan="2">Name</th>
        <th>Artifact</th>
        <th>Type</th>
        <th>Status</th>
        <th>Priority</th>
        <th>Owner</th>
        <th>Start Date</th>
        <th>End Date</th>
        <th>% Complete</th>
        <th>Plan Effort</th>
        <th>Est. Effort</th>
        <th>Actual Effort</th>
        <th>Rem. Effort</th>
        <th>Proj. Effort</th>
      </tr>
      <xsl:for-each select="Release">
        <tr>
          <xsl:attribute name="class">
            <xsl:if test="IsIterationOrPhase=''False''">
              BoldRow
            </xsl:if>
          </xsl:attribute>
          <td>
            RL<xsl:value-of select="ReleaseId"/>
          </td>
          <td colspan="2">
            <xsl:attribute name="style">
              padding-left: <xsl:value-of select="string-length(IndentLevel)*4"/>px;
            </xsl:attribute>
            <xsl:value-of select="VersionNumber"/> - <xsl:value-of select="Name"/>
          </td>
          <td>
            Release
          </td>
          <td>
            <xsl:value-of select="ReleaseTypeName"/>
          </td>
          <td>
            <xsl:value-of select="ReleaseStatusName"/>
          </td>
          <td></td>
          <td>
            <xsl:value-of select="OwnerName"/>
          </td>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="StartDate" />
            </xsl:call-template>
          </td>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="EndDate" />
            </xsl:call-template>
          </td>
          <td></td>
          <td>
			<xsl:variable name="durationInDays" select="PlannedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="TaskEstimatedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="TaskActualEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="TaskRemainingEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
          <td>
			<xsl:variable name="durationInDays" select="TaskProjectedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
          </td>
        </tr>
        <xsl:for-each select="Requirements/Requirement">
          <tr class="SmallRow">
            <td>
              RQ<xsl:value-of select="RequirementId"/>
            </td>
            <td style="width:50px"></td>
            <td>
              <xsl:value-of select="Name"/>
            </td>
            <td>
              Requirement
            </td>
            <td>
              <xsl:value-of select="RequirementTypeName"/>
            </td>
            <td>
              <xsl:value-of select="RequirementStatusName"/>
            </td>
            <td>
              <xsl:value-of select="ImportanceName"/>
            </td>
            <td>
              <xsl:value-of select="OwnerName"/>
            </td>
            <td>
            </td>
            <td>
            </td>
            <td>
            </td>
            <td>
			  <xsl:variable name="durationInDays" select="EstimatedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <td>
			  <xsl:variable name="durationInDays" select="TaskEstimatedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <td>
			  <xsl:variable name="durationInDays" select="TaskActualEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <td>
			  <xsl:variable name="durationInDays" select="TaskRemainingEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <td>
			  <xsl:variable name="durationInDays" select="TaskProjectedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
          </tr>
        </xsl:for-each>
        <xsl:for-each select="Tasks/Task">
          <tr class="SmallRow">
            <td>
              TK<xsl:value-of select="TaskId"/>
            </td>
            <td style="width:50px"></td>
            <td>
              <xsl:value-of select="Name"/>
            </td>
            <td>
              Task
            </td>
            <td>
              <xsl:value-of select="TaskTypeName"/>
            </td>
            <td>
              <xsl:value-of select="TaskStatusName"/>
            </td>
            <td>
              <xsl:value-of select="TaskPriorityName"/>
            </td>
            <td>
              <xsl:value-of select="OwnerName"/>
            </td>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="StartDate" />
              </xsl:call-template>
            </td>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="EndDate" />
              </xsl:call-template>
            </td>
            <td>
              <xsl:value-of select="CompletionPercent"/> %
            </td>
            <td></td>
            <td>
			  <xsl:variable name="durationInDays" select="EstimatedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <td>
			  <xsl:variable name="durationInDays" select="ActualEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <td>
			  <xsl:variable name="durationInDays" select="RemainingEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <td>
			  <xsl:variable name="durationInDays" select="ProjectedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
          </tr>
        </xsl:for-each>
        <xsl:for-each select="Incidents/Incident">
          <tr class="SmallItalicRow">
            <td>
              IN<xsl:value-of select="IncidentId"/>
            </td>
            <td style="width:50px"></td>
            <td>
              <xsl:value-of select="Name"/>
            </td>
            <td>
              Incident
            </td>
            <td>
              <xsl:value-of select="IncidentTypeName"/>
            </td>
            <td>
              <xsl:value-of select="IncidentStatusName"/>
            </td>
            <td>
              <xsl:value-of select="PriorityName"/>
            </td>
            <td>
              <xsl:value-of select="OwnerName"/>
            </td>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="StartDate" />
              </xsl:call-template>
            </td>
            <td class="Date">
              <xsl:call-template name="format-date">
                <xsl:with-param name="datetime" select="ClosedDate" />
              </xsl:call-template>
            </td>
            <td>
              <xsl:value-of select="CompletionPercent"/> %
            </td>
            <td></td>
            <td>
			  <xsl:variable name="durationInDays" select="EstimatedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <td>
			  <xsl:variable name="durationInDays" select="ActualEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <td>
			  <xsl:variable name="durationInDays" select="RemainingEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
            <td>
			  <xsl:variable name="durationInDays" select="ProjectedEffort"/>
    <xsl:choose>
	
      <xsl:when test="number($durationInDays) &gt;= 0">
        <xsl:variable name="totalMinutes" select="round($durationInDays * 60)"/>
        <xsl:variable name="hours" select="floor($durationInDays div 60)"/>
        <xsl:variable name="minutes" select="$durationInDays mod 60"/>
        <xsl:value-of select="concat(format-number($hours, ''00''), '':'', format-number($minutes, ''00''), '')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
            </td>
          </tr>
        </xsl:for-each>
      </xsl:for-each>
    </table>
  </xsl:template>
  <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-before(substring-after($datetime, ''T''), ''.'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year , '' '', $time)" />
  </xsl:template>
</xsl:stylesheet>
', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (19, 1, N'RequirementTrace', N'Requirement Traceability', N'This section displays a summary grid that shows the traceability from requirements > cases.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/RequirementData">
    <table class="DataGrid" style="width:100%">
      <tr>
        <th>Req #</th>
        <th>Name</th>
        <th>Type</th>
        <th>Status</th>
        <th>Release #</th>
        <th>Test Traceability</th>
        <th>Requirements Traceability</th>
      </tr>
      <xsl:for-each select="Requirement">
        <tr>
          <td>
            RQ<xsl:value-of select="RequirementId"/>
          </td>
          <td>
            <xsl:attribute name="style">
              padding-left: <xsl:value-of select="string-length(IndentLevel)*2"/>px;
            </xsl:attribute>
            <xsl:if test="IsSummary=''True''">
              <b>
                <xsl:value-of select="Name"/>
              </b>
            </xsl:if>
            <xsl:if test="IsSummary=''False''">
              <xsl:value-of select="Name"/>
            </xsl:if>
          </td>
          <td>
            <xsl:value-of select="RequirementTypeName"/>
          </td>
          <td>
            <xsl:value-of select="RequirementStatusName"/>
          </td>
          <td>
            <xsl:value-of select="ReleaseVersionNumber"/>
          </td>
          <td>
            <xsl:for-each select="TestCases/TestCase">
              <xsl:if test="position() > 1">
                , TC<xsl:value-of select="TestCaseId"/>
              </xsl:if>
              <xsl:if test="position() = 1">
                TC<xsl:value-of select="TestCaseId"/>
              </xsl:if>
            </xsl:for-each>
          </td>
          <td>
            <xsl:for-each select="Requirements/ArtifactLink">
              <xsl:if test="position() > 1">
                , RQ<xsl:value-of select="ArtifactId"/>
              </xsl:if>
              <xsl:if test="position() = 1">
                RQ<xsl:value-of select="ArtifactId"/>
              </xsl:if>
            </xsl:for-each>
          </td>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>
</xsl:stylesheet>

', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (20, 2, N'TestCaseTrace', N'Test Case Traceability', N'This section displays a summary grid that shows the traceability from test cases > requirements.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/TestCaseData">
    <table class="DataGrid" style="width:100%">
      <tr>
        <th>Test #</th>
        <th>Name</th>
        <th>Priority</th>
        <th>Requirements Traceability</th>
      </tr>
      <xsl:for-each select="TestCaseFolder">
        <tr>
          <td>
            <b>
              <xsl:value-of select="TestCaseFolderId"/>
            </b>
          </td>
          <td>
            <xsl:attribute name="style">
              padding-left: <xsl:value-of select="string-length(IndentLevel)*2"/>px;
            </xsl:attribute>
            <b>
              <xsl:value-of select="Name"/>
            </b>
          </td>
          <td></td>
          <td></td>
        </tr>
        <xsl:for-each select="TestCases/TestCase">
          <tr>
            <td>
              TC<xsl:value-of select="TestCaseId"/>
            </td>
            <td>
              <xsl:value-of select="Name"/>
            </td>
            <td>
              <xsl:value-of select="TestCasePriorityName"/>
            </td>
            <td>
              <xsl:for-each select="Requirements/Requirement">
                <xsl:if test="position() > 1">
                  , RQ<xsl:value-of select="RequirementId"/>
				</xsl:if>
                <xsl:if test="position() = 1">
                  RQ<xsl:value-of select="RequirementId"/>
				</xsl:if>
              </xsl:for-each>
            </td>
          </tr>
        </xsl:for-each>
      </xsl:for-each>
    </table>
  </xsl:template>
</xsl:stylesheet>

', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (21, 2, N'TestCaseTrace2', N'Test Case Traceability', N'This section displays a summary grid that shows the traceability from test cases > releases, incidents and test sets.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/TestCaseData">
    <table class="DataGrid" style="width:100%">
      <tr>
        <th>Test #</th>
        <th>Name</th>
        <th>Priority</th>
        <th>Releases</th>
        <th>Test Sets</th>
        <th>Incidents</th>
      </tr>
      <xsl:for-each select="TestCaseFolder">
        <tr>
          <td>
            <b>
              <xsl:value-of select="TestCaseFolderId"/>
            </b>
          </td>
          <td>
            <xsl:attribute name="style">
              padding-left: <xsl:value-of select="string-length(IndentLevel)*2"/>px;
            </xsl:attribute>
            <b>
              <xsl:value-of select="Name"/>
            </b>
          </td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
        </tr>
        <xsl:for-each select="TestCases/TestCase">
          <tr>
            <td>
              TC<xsl:value-of select="TestCaseId"/>
            </td>
            <td>
              <xsl:value-of select="Name"/>
            </td>
            <td>
              <xsl:value-of select="TestCasePriorityName"/>
            </td>
            <td>
              <xsl:for-each select="Releases/Release">
                <xsl:if test="position() > 1">
                  , RL<xsl:value-of select="ReleaseId"/>
				</xsl:if>
                <xsl:if test="position() = 1">
                  RL<xsl:value-of select="ReleaseId"/>
				</xsl:if>
              </xsl:for-each>
            </td>
            <td>
              <xsl:for-each select="TestSets/TestSet">
                <xsl:if test="position() > 1">
                  , TX<xsl:value-of select="TestSetId"/>
				</xsl:if>
                <xsl:if test="position() = 1">
                  TX<xsl:value-of select="TestSetId"/>
				</xsl:if>
              </xsl:for-each>
            </td>
            <td>
              <xsl:for-each select="Incidents/Incident">
                <xsl:if test="position() > 1">
                  , IN<xsl:value-of select="IncidentId"/>
				</xsl:if>
                <xsl:if test="position() = 1">
                  IN<xsl:value-of select="IncidentId"/>
				</xsl:if>
              </xsl:for-each>
            </td>
          </tr>
        </xsl:for-each>
      </xsl:for-each>
    </table>
  </xsl:template>
</xsl:stylesheet>

', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (22, 14, N'RiskList', N'Risk List', N'This section displays a simple table containing all the risks with a column for each of their standard and custom fields.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/RiskData">
    <table class="DataGrid" style="width:100%">
      <tr>
        <th>Risk #</th>
        <th>Name</th>
        <th>Description</th>
        <th>Probability</th>
        <th>Impact</th>
        <th>Exposure</th>
        <th>Type</th>
        <th>Status</th>
        <th>Release</th>
        <th>Component</th>
        <th>Created By</th>
        <th>Owned By</th>
        <th>Created On</th>
        <th>Last Modified</th>
        <th>Review Date</th>
        <th>Closed On</th>
        <xsl:for-each select="Risk[1]/CustomProperties/CustomProperty">
          <th>
            <xsl:value-of select="Alias"/>
          </th>
        </xsl:for-each>
      </tr>
      <xsl:for-each select="Risk">
        <tr>
          <td>
            <xsl:value-of select="RiskId"/>
          </td>
          <td>
            <xsl:value-of select="Name"/>
          </td>
          <td>
            <xsl:value-of select="Description" disable-output-escaping="yes"/>
          </td>
          <td>
            <xsl:value-of select="RiskProbabilityName"/>
          </td>
          <td>
            <xsl:value-of select="RiskImpactName"/>
          </td>
          <td>
            <xsl:value-of select="RiskExposure"/>
          </td>
          <td>
            <xsl:value-of select="RiskTypeName"/>
          </td>
          <td>
            <xsl:value-of select="RiskStatusName"/>
          </td>
          <td>
            <xsl:value-of select="ReleaseVersionNumber"/>
          </td>
          <td>
            <xsl:value-of select="ComponentName"/>
          </td>
          <td>
            <xsl:value-of select="CreatorName"/>
          </td>
          <td>
            <xsl:value-of select="OwnerName"/>
          </td>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="CreationDate" />
            </xsl:call-template>
          </td>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="LastUpdateDate" />
            </xsl:call-template>
          </td>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="ReviewDate" />
            </xsl:call-template>
          </td>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="ClosedDate" />
            </xsl:call-template>
          </td>
          <xsl:for-each select="CustomProperties/CustomProperty">
            <xsl:choose>
              <xsl:when test="Type=''Date''">
                <td class="Date">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="Value" />
                  </xsl:call-template>
                </td>
              </xsl:when>
              <xsl:otherwise>
                <td>
                  <xsl:value-of select="Value" disable-output-escaping="yes" />
                </td>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>
  <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-before(substring-after($datetime, ''T''), ''.'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year , '' '', $time)" />
  </xsl:template>
</xsl:stylesheet>

', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (23, 14, N'RiskDetails', N'Risk Details', N'This section displays all of the risks in a document format with embedded tables for each of the associated elements.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/RiskData">
    <xsl:for-each select="Risk">
      <div class="Title2">
        RK:<xsl:value-of select="RiskId"/> - <xsl:value-of select="Name"/>
      </div>
      <div class="Description">
        <xsl:value-of select="Description" disable-output-escaping="yes"/>
      </div>
      <table class="HiddenTable">
        <tr>
          <th>Type:</th>
          <td>
            <xsl:value-of select="RiskTypeName"/>
          </td>
          <th>Exposure:</th>
          <td>
            <xsl:value-of select="RiskExposure"/>
          </td>
        </tr>
        <tr>
          <th>Status:</th>
          <td>
            <xsl:value-of select="RiskStatusName"/>
          </td>
          <th>Probability:</th>
          <td>
            <xsl:value-of select="RiskProbabilityName"/>
          </td>
        </tr>
        <tr>
          <th>Component:</th>
          <td>
            <xsl:value-of select="ComponentName"/>
          </td>
          <th>Impact:</th>
          <td>
            <xsl:value-of select="RiskImpactName"/>
          </td>
        </tr>
        <tr>
          <th>Created By:</th>
          <td>
            <xsl:value-of select="CreatorName"/>
          </td>
          <th>Created On:</th>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="CreationDate" />
            </xsl:call-template>
          </td>
        </tr>
        <tr>
          <th>Owned By:</th>
          <td>
            <xsl:value-of select="OwnerName"/>
          </td>
          <th>Last Modified:</th>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="LastUpdateDate" />
            </xsl:call-template>
          </td>
        </tr>
        <tr>
          <th>Release:</th>
          <td>
            <xsl:value-of select="ReleaseVersionNumber"/>
          </td>
          <th>Review Date:</th>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="ReviewDate" />
            </xsl:call-template>
          </td>
        </tr>
        <tr>
          <td colspan="2"></td>
          <th>Closed On:</th>
          <td class="Date">
            <xsl:call-template name="format-date">
              <xsl:with-param name="datetime" select="ClosedDate" />
            </xsl:call-template>
          </td>
        </tr>
        <xsl:for-each select="CustomProperties/CustomProperty">
          <tr>
            <th>
              <xsl:value-of select="Alias"/>:
            </th>
            <xsl:choose>
              <xsl:when test="Type=''Date''">
                <td class="Date" colspan="3">
                  <xsl:call-template name="format-date">
                    <xsl:with-param name="datetime" select="Value" />
                  </xsl:call-template>
                </td>
              </xsl:when>
              <xsl:otherwise>
                <td colspan="3">
                  <xsl:value-of select="Value" disable-output-escaping="yes"/>
                </td>
              </xsl:otherwise>
            </xsl:choose>
          </tr>
        </xsl:for-each>
      </table>
      <xsl:if test="RiskDiscussions/RiskDiscussion">
        <div class="Title3">
          Comments:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Date</th>
            <th>Creator</th>
            <th>Comment</th>
            <th>Id</th>
          </tr>
          <xsl:for-each select="RiskDiscussions/RiskDiscussion">
            <tr>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="CreationDate" />
                </xsl:call-template>
              </td>
              <td>
                <xsl:value-of select="CreatorName"/>
              </td>
              <td>
                <xsl:value-of select="Text" disable-output-escaping="yes"/>
              </td>
              <td>
                <xsl:value-of select="DiscussionId"/>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <xsl:if test="Mitigations/Mitigation">
        <div class="Title3">
          Mitigations:
        </div>
        <table class="DataGrid">
          <tr>
            <th>#</th>
            <th>Description</th>
            <th>Review Date</th>
          </tr>
          <xsl:for-each select="Mitigations/Mitigation">
            <tr>
              <td>
                <xsl:value-of select="position()"/>
              </td>
              <td>
                <xsl:value-of select="Description" disable-output-escaping="yes"/>
              </td>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="ReviewDate" />
                </xsl:call-template>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <xsl:if test="Tasks/Task">
        <div class="Title4">
          Tasks:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Task #</th>
            <th>Name</th>
            <th>Status</th>
            <th>Type</th>
            <th>Priority</th>
            <th>Owned By</th>
            <th>Start Date</th>
            <th>End Date</th>
            <th>% Completed</th>
          </tr>
          <xsl:for-each select="Tasks/Task">
            <tr>
              <td>
                TK<xsl:value-of select="TaskId"/>
              </td>
              <td>
                <xsl:value-of select="Name"/>
              </td>
              <td>
                <xsl:value-of select="TaskStatusName"/>
              </td>
              <td>
                <xsl:value-of select="TaskTypeName"/>
              </td>
              <td>
                <xsl:value-of select="TaskPriorityName"/>
              </td>
              <td>
                <xsl:value-of select="OwnerName"/>
              </td>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="StartDate" />
                </xsl:call-template>
              </td>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="EndDate" />
                </xsl:call-template>
              </td>
              <td>
                <xsl:value-of select="CompletionPercent"/> %
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <xsl:if test="Attachments/Attachment">
        <div class="Title3">
          File Attachments:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Filename</th>
            <th>Description</th>
            <th>Author</th>
            <th>Date Uploaded</th>
          </tr>
          <xsl:for-each select="Attachments/Attachment">
            <tr>
              <td>
                <xsl:value-of select="Filename"/>
              </td>
              <td>
                <xsl:value-of select="Description"/>
              </td>
              <td>
                <xsl:value-of select="AuthorName"/>
              </td>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="UploadDate" />
                </xsl:call-template>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <xsl:if test="History/ArtifactHistory">
        <div class="Title3">
          Risk Change History:
        </div>
        <table class="DataGrid">
          <tr>
            <th>Change Date</th>
            <th>Field Name</th>
            <th>Old Value</th>
            <th>New Value</th>
            <th>Changed By</th>
          </tr>
          <xsl:for-each select="History/ArtifactHistory">
            <tr>
              <td class="Date">
                <xsl:call-template name="format-date">
                  <xsl:with-param name="datetime" select="ChangeDate" />
                </xsl:call-template>
              </td>
              <td>
                <xsl:value-of select="FieldName"/>
              </td>
              <td>
                <xsl:value-of select="OldValue" disable-output-escaping="yes"/>
              </td>
              <td>
                <xsl:value-of select="NewValue" disable-output-escaping="yes"/>
              </td>
              <td>
                <xsl:value-of select="UserName"/>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </xsl:if>
      <br/>
    </xsl:for-each>
  </xsl:template>
  <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-before(substring-after($datetime, ''T''), ''.'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year , '' '', $time)" />
  </xsl:template>
</xsl:stylesheet>

', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (24, 16, N'ProjectAuditTrail', N'Project Audit Trail', N'This section displays a history grid that shows the histroy from Product History Changes.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
    <xsl:template match="/ProductHistoryData">
        <table class="DataGrid" style="width:100%">
            <tr>
				<th>Change ID</th>
				<th>Project Name</th> 
				<th>Record Id</th>	
				<th>Field/Record Name</th>
				<th>Artifact Type</th>				
				<th>Change Date</th>
				<th>Time</th>
                <th>Time Zone</th>
				<th>Changed By</th> 
				<th>Action</th>
				<th>Before Value</th>
				<th>After Value</th>				 
				<th>Signed</th>
				<th>Name of the signer</th>
				<th>Meaning of the signature</th>										
            </tr>
            <xsl:for-each select="ProductHistory">
                <tr>
                    <td>
                        <xsl:value-of select="ChangeSetId"/>
                    </td>
					<td>
                        <xsl:value-of select="ProjectName" />
                    </td> 	
					<td>
                        <xsl:value-of select="FieldId"/>
                    </td>
					<td>
                        <xsl:value-of select="FieldName"/>
                    </td>
					<td>
                        <xsl:value-of select="ArtifactTypeName"/>
                    </td>
					<td class="Date">
						<xsl:call-template name="format-date">
						<xsl:with-param name="datetime" select="ChangeDate" />
						</xsl:call-template>
					</td>
					<td>
						<xsl:call-template name="format-time">
						<xsl:with-param name="time" select="Time" />
						</xsl:call-template>
					</td>
					<td>
					  UTC
                    </td>
					<td>
                        <xsl:value-of select="UserName" />
                    </td>
					<td>
                        <xsl:value-of select="ChangeTypeName"/>
                    </td> 					
					<td>
                        <xsl:value-of select="OldValue"/>
                    </td>
					<td>
                        <xsl:value-of select="NewValue"/>
                    </td>					
					<td>
                        <xsl:value-of select="SignedValue"/>
                    </td>
					<td>
					  <xsl:value-of select="NameOfSigner"/>
                    </td> 
					<td>
					  <xsl:value-of select="Meaning"/>
                    </td> 
									
                </tr>
            </xsl:for-each>
        </table>
    </xsl:template>
   <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-after($datetime, ''T'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year)" />
  </xsl:template>
  <xsl:template name="format-time">
    <xsl:param name="time"/>
	<xsl:variable name="time-ampm" select="substring-after($time, '''')"/>
      <xsl:variable name="time1" select="substring-before($time-ampm, '' '')"/>
      <xsl:variable name="ampm" select="substring-after($time-ampm, '' '')"/>
      <xsl:variable name="h" select="substring-before($time1, '':'')"/>
      <xsl:variable name="m-s" select="substring-after($time1, '':'')"/>
      <xsl:variable name="m" select="substring-before($m-s, '':'')"/>
      <xsl:variable name="s" select="substring-after($m-s, '':'')"/>

      <xsl:variable name="hh">
        <xsl:choose>
          <xsl:when test="$ampm = ''PM''">
            <xsl:value-of select="format-number($h + 12, ''00'')"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="format-number($h, ''00'')"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>     
      <xsl:value-of select="concat($time-ampm,'';'')"/>

  </xsl:template>
</xsl:stylesheet>', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (25, 19, N'AllHistoryList', N'Audit Trail', N'This section displays a history grid that shows the histroy from Product History Changes.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
    <xsl:template match="/ProductHistoryData">
        <table class="DataGrid" style="width:100%">
            <tr>
				<th>Change ID</th>
				<th>Project Name</th> 
				<th>Record Id</th>	
				<th>Field/Record Name</th>
				<th>Artifact Type</th>				
				<th>Change Date</th>
				<th>Time</th>
                <th>Time Zone</th>
				<th>Changed By</th> 
				<th>Action</th>
				<th>Before Value</th>
				<th>After Value</th>				 
				<th>Signed</th>
				<th>Name of the signer</th>
				<th>Meaning of the signature</th>										
            </tr>
            <xsl:for-each select="ProductHistory">
                <tr>
                    <td>
                        <xsl:value-of select="ChangeSetId"/>
                    </td>
					<td>
                        <xsl:value-of select="ProjectName" />
                    </td> 	
					<td>
                        <xsl:value-of select="FieldId"/>
                    </td>
					<td>
                        <xsl:value-of select="FieldName"/>
                    </td>
					<td>
                        <xsl:value-of select="ArtifactTypeName"/>
                    </td>
					<td class="Date">
						<xsl:call-template name="format-date">
						<xsl:with-param name="datetime" select="ChangeDate" />
						</xsl:call-template>
					</td>
					<td>
						<xsl:call-template name="format-time">
						<xsl:with-param name="time" select="Time" />
						</xsl:call-template>
					</td>
					<td>
					  UTC
                    </td>
					<td>
                        <xsl:value-of select="UserName" />
                    </td>
					<td>
                        <xsl:value-of select="ChangeTypeName"/>
                    </td> 					
					<td>
                        <xsl:value-of select="OldValue"/>
                    </td>
					<td>
                        <xsl:value-of select="NewValue"/>
                    </td>					
					<td>
                        <xsl:value-of select="SignedValue"/>
                    </td>
					<td>
					  <xsl:value-of select="NameOfSigner"/>
                    </td> 
					<td>
					  <xsl:value-of select="Meaning"/>
                    </td> 
									
                </tr>
            </xsl:for-each>
        </table>
    </xsl:template>
   <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-after($datetime, ''T'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year)" />
  </xsl:template>
  <xsl:template name="format-time">
    <xsl:param name="time"/>
	<xsl:variable name="time-ampm" select="substring-after($time, '''')"/>
      <xsl:variable name="time1" select="substring-before($time-ampm, '' '')"/>
      <xsl:variable name="ampm" select="substring-after($time-ampm, '' '')"/>
      <xsl:variable name="h" select="substring-before($time1, '':'')"/>
      <xsl:variable name="m-s" select="substring-after($time1, '':'')"/>
      <xsl:variable name="m" select="substring-before($m-s, '':'')"/>
      <xsl:variable name="s" select="substring-after($m-s, '':'')"/>

      <xsl:variable name="hh">
        <xsl:choose>
          <xsl:when test="$ampm = ''PM''">
            <xsl:value-of select="format-number($h + 12, ''00'')"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="format-number($h, ''00'')"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>     
      <xsl:value-of select="concat($time-ampm,'';'')"/>

  </xsl:template>
</xsl:stylesheet>', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (26, 20, N'AdminAuditTrail', N'Admin Audit Trail ', N'This section displays a history grid that shows the histroy from Admin History Changes.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
    <xsl:template match="/ProductHistoryData">
        <table class="DataGrid" style="width:100%">
            <tr>
				<th>Change ID</th>
				<th>Record Id</th>	
				<th>Field/Record Name</th>				
				<th>Change Date</th>
				<th>Time</th>
                <th>Time Zone</th>
				<th>Changed By</th> 
				<th>Action</th>
				<th>Action Description</th>
				<th>Admin Section Name</th>
				<th>Before Value</th>
				<th>After Value</th>									
            </tr>
            <xsl:for-each select="ProductHistory">
                <tr>
                    <td>
                        <xsl:value-of select="ChangeSetId"/>
                    </td>
					<td>
                        <xsl:value-of select="FieldId"/>
                    </td>
					<td>
                        <xsl:value-of select="FieldName"/>
                    </td>
					<td class="Date">
						<xsl:call-template name="format-date">
						<xsl:with-param name="datetime" select="ChangeDate" />
						</xsl:call-template>
					</td>
					<td>
						<xsl:call-template name="format-time">
						<xsl:with-param name="time" select="Time" />
						</xsl:call-template>
					</td>
					<td>
					  UTC
                    </td>
					<td>
                        <xsl:value-of select="UserName" />
                    </td>
					<td>
                        <xsl:value-of select="ChangeTypeName"/>
                    </td> 
					<td>
                        <xsl:value-of select="ActionDescription"/>
                    </td> 
					<td>
                        <xsl:value-of select="AdminSectionName"/>
                    </td>					
					<td>
                        <xsl:value-of select="OldValue"/>
                    </td>
					<td>
                        <xsl:value-of select="NewValue"/>
                    </td>
									
                </tr>
            </xsl:for-each>
        </table>
    </xsl:template>
   <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-after($datetime, ''T'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year)" />
  </xsl:template>
  <xsl:template name="format-time">
    <xsl:param name="time"/>
	<xsl:variable name="time-ampm" select="substring-after($time, '''')"/>
      <xsl:variable name="time1" select="substring-before($time-ampm, '' '')"/>
      <xsl:variable name="ampm" select="substring-after($time-ampm, '' '')"/>
      <xsl:variable name="h" select="substring-before($time1, '':'')"/>
      <xsl:variable name="m-s" select="substring-after($time1, '':'')"/>
      <xsl:variable name="m" select="substring-before($m-s, '':'')"/>
      <xsl:variable name="s" select="substring-after($m-s, '':'')"/>

      <xsl:variable name="hh">
        <xsl:choose>
          <xsl:when test="$ampm = ''PM''">
            <xsl:value-of select="format-number($h + 12, ''00'')"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="format-number($h, ''00'')"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>     
      <xsl:value-of select="concat($time-ampm,'';'')"/>

  </xsl:template>
</xsl:stylesheet>', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (27, 22, N'UserAuditTrail', N'User Audit Trail', N'This section displays a history grid that shows the histroy from User History Changes.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
	<xsl:template match="/UserHistoryData">
		<table class="DataGrid" style="width:100%">
			<tr>
				<th>Change ID</th>
				<th>Change Date</th>
				<th>Time</th>
				<th>Time Zone</th>
				<th>Changed By</th>
				<th>Action</th>
				<th>Event Description</th>
				<th>Property Name</th>
				<th>Before Value</th>
				<th>After Value</th>
			</tr>
			<xsl:for-each select="UserHistory">
				<tr>
					<td>
						<xsl:value-of select="ChangeSetId"/>
					</td>
					<td class="Date">
						<xsl:call-template name="format-date">
							<xsl:with-param name="datetime" select="ChangeDate" />
						</xsl:call-template>
					</td>
					<td>
						<xsl:call-template name="format-time">
							<xsl:with-param name="time" select="Time" />
						</xsl:call-template>
					</td>
					<td>         UTC                      </td>
					<td>
						<xsl:value-of select="UpdatedUserName" />
					</td>
					<td>
						<xsl:value-of select="ChangeTypeName"/>
					</td>
					<td>
						<xsl:value-of select="FieldName"/>
					</td>
					<td>
						<xsl:value-of select="PropertyName"/>
					</td>
					<td>
						<xsl:value-of select="OldValue"/>
					</td>
					<td>
						<xsl:value-of select="NewValue"/>
					</td>
				</tr>
			</xsl:for-each>
		</table>
	</xsl:template>
	<xsl:template name="format-date">
		<xsl:param name="datetime"/>
		<xsl:variable name="date" select="substring-before($datetime, ''T'')" />
		<xsl:variable name="year" select="substring-before($date, ''-'')" />
		<xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
		<xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
		<xsl:variable name="time" select="substring-after($datetime, ''T'')" />
		<xsl:variable name="monthname">
			<xsl:choose>
				<xsl:when test="$month=''01''">
					<xsl:value-of select="''Jan''"/>
				</xsl:when>
				<xsl:when test="$month=''02''">
					<xsl:value-of select="''Feb''"/>
				</xsl:when>
				<xsl:when test="$month=''03''">
					<xsl:value-of select="''Mar''"/>
				</xsl:when>
				<xsl:when test="$month=''04''">
					<xsl:value-of select="''Apr''"/>
				</xsl:when>
				<xsl:when test="$month=''05''">
					<xsl:value-of select="''May''"/>
				</xsl:when>
				<xsl:when test="$month=''06''">
					<xsl:value-of select="''Jun''"/>
				</xsl:when>
				<xsl:when test="$month=''07''">
					<xsl:value-of select="''Jul''"/>
				</xsl:when>
				<xsl:when test="$month=''08''">
					<xsl:value-of select="''Aug''"/>
				</xsl:when>
				<xsl:when test="$month=''09''">
					<xsl:value-of select="''Sep''"/>
				</xsl:when>
				<xsl:when test="$month=''10''">
					<xsl:value-of select="''Oct''"/>
				</xsl:when>
				<xsl:when test="$month=''11''">
					<xsl:value-of select="''Nov''"/>
				</xsl:when>
				<xsl:when test="$month=''12''">
					<xsl:value-of select="''Dec''"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="''''" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year)" />
	</xsl:template>
	<xsl:template name="format-time">
		<xsl:param name="time"/>
		<xsl:variable name="time-ampm" select="substring-after($time, '''')"/>
		<xsl:variable name="time1" select="substring-before($time-ampm, '' '')"/>
		<xsl:variable name="ampm" select="substring-after($time-ampm, '' '')"/>
		<xsl:variable name="h" select="substring-before($time1, '':'')"/>
		<xsl:variable name="m-s" select="substring-after($time1, '':'')"/>
		<xsl:variable name="m" select="substring-before($m-s, '':'')"/>
		<xsl:variable name="s" select="substring-after($m-s, '':'')"/>
		<xsl:variable name="hh">
			<xsl:choose>
				<xsl:when test="$ampm = ''PM''">
					<xsl:value-of select="format-number($h + 12, ''00'')"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="format-number($h, ''00'')"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:value-of select="concat($time-ampm,'';'')"/>
	</xsl:template>
</xsl:stylesheet>', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (28, 34, N'SystemUsageReport', N'System Usage Report', N'This section displays a Systenm Usage Report.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
	
	<xsl:template match="/SystemUsageData">
		<table class="DataGrid" style="width:100%">
			<tr>
				<th>Month Name</th>
				<th># of Active Accounts</th>
				<th>Active User %</th>
				<th>Avg No. of Connection PerDay</th>
				<th>Avg No. of Connection PerWeek</th>
				<th>Avg No. of Connection PerMonth</th>
				<th>Avg Conn Time PerDay</th>
				<th>Avg Conn Time PerWeek</th>
				<th>Avg Conn Time PerMonth</th>
			</tr>
			<xsl:for-each select="SystemUsage">
				<tr>
					<td>
						<xsl:value-of select="MonthName"/>
					</td>
					<td>
						<xsl:value-of select="ActiveAccount" />
					</td>
					<td>
						<xsl:value-of select="ActiveUserPercentage"/>
					</td>
					<td>
						<xsl:value-of select="AvgNoOfConnPerDay"/>
					</td>
					<td>
						<xsl:value-of select="AvgNoOfConnPerWeek"/>
					</td>
					<td>
						<xsl:value-of select="AvgNoOfConnPerMonth"/>
					</td>
					<td>
						<!-- Process AvgConnTimePerDay -->
						<xsl:call-template name="process-time">
							<xsl:with-param name="time" select="AvgConnTimePerDay"/>
						</xsl:call-template>
					</td>
					<td>
						<xsl:call-template name="process-time">
							<xsl:with-param name="time" select="AvgConnTimePerWeek"/>
						</xsl:call-template>
					</td>
					<td>
						<xsl:call-template name="process-time">
							<xsl:with-param name="time" select="AvgConnTimePerMonth"/>
						</xsl:call-template>
					</td>
				</tr>
			</xsl:for-each>
		</table>
	</xsl:template>

	<!-- Template to process time -->
	<xsl:template name="process-time">
		<xsl:param name="time" />
		
		<!-- Debugging: output the raw time value -->
		<xsl:value-of select="$time"/>
		<xsl:text> ; </xsl:text>

		<!-- Check if the time is in the "hh:mm" format -->
		<xsl:variable name="hoursString" select="substring-before($time, '':'')"/>
		<xsl:variable name="minutesString" select="substring-after($time, '':'')"/>
		
		<xsl:choose>
			<xsl:when test="string-length($hoursString) = 2 and string-length($minutesString) = 2">
				<!-- If the time is already in hh:mm format, just output it as is -->
				<xsl:value-of select="$time"/>
			</xsl:when>
			
			
		</xsl:choose>
	</xsl:template>
    
</xsl:stylesheet>', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (29, 14, N'RiskSummary', N'Risk Summary', N'This section displays all of the risks summary in a document format with embedded tables for each of the associated elements.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:template match="/RiskSummaryData">
    <table class="DataGrid" style="width:100%">
      <tr>
				<th></th>
				<th>Certain</th>
				<th>Likely</th>
				<th>Possible</th>
				<th>Unlikely</th>
				<th>Rare</th>				
			</tr>
			<tr>
				<td>Catastrophic</td>
				<td></td>
				<td></td>
				<td></td>
				<td></td>
				<td></td>				
			</tr>
			<tr>
				<td>Critical</td>
				<td></td>
				<td></td>
				<td></td>
				<td></td>
				<td></td>				
			</tr>
			<tr>
				<td>Serious</td>
				<td></td>
				<td></td>
				<td></td>
				<td></td>
				<td></td>				
			</tr>
			<tr>
				<td>Marginal</td>
				<td></td>
				<td></td>
				<td></td>
				<td></td>
				<td></td>
			</tr>
			<tr>
				<td>Negligible</td>
				<td></td>
				<td></td>
				<td></td>
				<td></td>
				<td></td>
			</tr>
      
    </table>
  </xsl:template>
</xsl:stylesheet>

', 0)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (31, 49, N'ProjectAuditTrail', N'Project Audit Trail Report', N'This section displays a history grid that shows the histroy from Product History Changes.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
    <xsl:template match="/ProductHistoryData">
        <table class="DataGrid" style="width:100%">
            <tr>
				<th>Change ID</th>
				<th>Project Name</th> 
				<th>Record Id</th>	
				<th>Field/Record Name</th>
				<th>Record Type</th>				
				<th>Change Date</th>
				<th>Time</th>
                <th>Time Zone</th>
				<th>Changed By</th> 
				<th>Action</th>
				<th>Before Value</th>
				<th>After Value</th>				 
				<th>Signed</th>
				<th>Name of the signer</th>
				<th>Meaning of the signature</th>										
            </tr>
            <xsl:for-each select="ProductHistory">
                <tr>
                    <td>
                        <xsl:value-of select="ChangeSetId"/>
                    </td>
					<td>
                        <xsl:value-of select="ProjectName" />
                    </td> 	
					<td>
                        <xsl:value-of select="FieldId"/>
                    </td>
					<td>
                        <xsl:value-of select="FieldName"/>
                    </td>
					<td>
                        <xsl:value-of select="ArtifactTypeName"/>
                    </td>
					<td class="Date">
						<xsl:call-template name="format-date">
						<xsl:with-param name="datetime" select="ChangeDate" />
						</xsl:call-template>
					</td>
					<td>
						<xsl:call-template name="format-time">
						<xsl:with-param name="time" select="Time" />
						</xsl:call-template>
					</td>
					<td>
					  UTC
                    </td>
					<td>
                        <xsl:value-of select="UserName" />
                    </td>
					<td>
                        <xsl:value-of select="ChangeTypeName"/>
                    </td> 					
					<td>
                        <xsl:value-of select="OldValue"/>
                    </td>
					<td>
                        <xsl:value-of select="NewValue"/>
                    </td>					
					<td>
                        <xsl:value-of select="SignedValue"/>
                    </td>
					<td>
					  <xsl:value-of select="NameOfSigner"/>
                    </td> 
					<td>
					  <xsl:value-of select="Meaning"/>
                    </td> 
									
                </tr>
            </xsl:for-each>
        </table>
    </xsl:template>
   <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-after($datetime, ''T'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year)" />
  </xsl:template>
  <xsl:template name="format-time">
    <xsl:param name="time"/>
	<xsl:variable name="time-ampm" select="substring-after($time, '''')"/>
      <xsl:variable name="time1" select="substring-before($time-ampm, '' '')"/>
      <xsl:variable name="ampm" select="substring-after($time-ampm, '' '')"/>
      <xsl:variable name="h" select="substring-before($time1, '':'')"/>
      <xsl:variable name="m-s" select="substring-after($time1, '':'')"/>
      <xsl:variable name="m" select="substring-before($m-s, '':'')"/>
      <xsl:variable name="s" select="substring-after($m-s, '':'')"/>

      <xsl:variable name="hh">
        <xsl:choose>
          <xsl:when test="$ampm = ''PM''">
            <xsl:value-of select="format-number($h + 12, ''00'')"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="format-number($h, ''00'')"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>     
      <xsl:value-of select="concat($time-ampm,'';'')"/>

  </xsl:template>
</xsl:stylesheet>', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (32, 50, N'AuditTrailReport', N'Audit Trail Report', N'This section displays a history grid that shows the histroy from Product History Changes.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
    <xsl:template match="/ProductHistoryData">
        <table class="DataGrid" style="width:100%">
            <tr>
				<th>Change ID</th>
				<th>Project Name</th> 
				<th>Record Id</th>	
				<th>Field/Record Name</th>
				<th>Record Type</th>				
				<th>Change Date</th>
				<th>Time</th>
                <th>Time Zone</th>
				<th>Changed By</th> 
				<th>Action</th>
				<th>Before Value</th>
				<th>After Value</th>				 
				<th>Signed</th>
				<th>Name of the signer</th>
				<th>Meaning of the signature</th>										
            </tr>
            <xsl:for-each select="ProductHistory">
                <tr>
                    <td>
                        <xsl:value-of select="ChangeSetId"/>
                    </td>
					<td>
                        <xsl:value-of select="ProjectName" />
                    </td> 	
					<td>
                        <xsl:value-of select="FieldId"/>
                    </td>
					<td>
                        <xsl:value-of select="FieldName"/>
                    </td>
					<td>
                        <xsl:value-of select="ArtifactTypeName"/>
                    </td>
					<td class="Date">
						<xsl:call-template name="format-date">
						<xsl:with-param name="datetime" select="ChangeDate" />
						</xsl:call-template>
					</td>
					<td>
						<xsl:call-template name="format-time">
						<xsl:with-param name="time" select="Time" />
						</xsl:call-template>
					</td>
					<td>
					  UTC
                    </td>
					<td>
                        <xsl:value-of select="UserName" />
                    </td>
					<td>
                        <xsl:value-of select="ChangeTypeName"/>
                    </td> 					
					<td>
                        <xsl:value-of select="OldValue"/>
                    </td>
					<td>
                        <xsl:value-of select="NewValue"/>
                    </td>					
					<td>
                        <xsl:value-of select="SignedValue"/>
                    </td>
					<td>
					  <xsl:value-of select="NameOfSigner"/>
                    </td> 
					<td>
					  <xsl:value-of select="Meaning"/>
                    </td> 
									
                </tr>
            </xsl:for-each>
        </table>
    </xsl:template>
   <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-after($datetime, ''T'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year)" />
  </xsl:template>
  <xsl:template name="format-time">
    <xsl:param name="time"/>
	<xsl:variable name="time-ampm" select="substring-after($time, '''')"/>
      <xsl:variable name="time1" select="substring-before($time-ampm, '' '')"/>
      <xsl:variable name="ampm" select="substring-after($time-ampm, '' '')"/>
      <xsl:variable name="h" select="substring-before($time1, '':'')"/>
      <xsl:variable name="m-s" select="substring-after($time1, '':'')"/>
      <xsl:variable name="m" select="substring-before($m-s, '':'')"/>
      <xsl:variable name="s" select="substring-after($m-s, '':'')"/>

      <xsl:variable name="hh">
        <xsl:choose>
          <xsl:when test="$ampm = ''PM''">
            <xsl:value-of select="format-number($h + 12, ''00'')"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="format-number($h, ''00'')"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>     
      <xsl:value-of select="concat($time-ampm,'';'')"/>

  </xsl:template>
</xsl:stylesheet>', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (33, 51, N'AdminAudit', N'Admin Audit Trail Report', N'This section displays a history grid that shows the histroy from Admin History Changes.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
    <xsl:template match="/ProductHistoryData">
        <table class="DataGrid" style="width:100%">
            <tr>
				<th>Change ID</th>
				<th>Record Id</th>	
				<th>Field/Record Name</th>				
				<th>Change Date</th>
				<th>Time</th>
                <th>Time Zone</th>
				<th>Changed By</th> 
				<th>Action</th>
				<th>Action Description</th>
				<th>Admin Section Name</th>
				<th>Before Value</th>
				<th>After Value</th>									
            </tr>
            <xsl:for-each select="ProductHistory">
                <tr>
                    <td>
                        <xsl:value-of select="ChangeSetId"/>
                    </td>
					<td>
                        <xsl:value-of select="FieldId"/>
                    </td>
					<td>
                        <xsl:value-of select="FieldName"/>
                    </td>
					<td class="Date">
						<xsl:call-template name="format-date">
						<xsl:with-param name="datetime" select="ChangeDate" />
						</xsl:call-template>
					</td>
					<td>
						<xsl:call-template name="format-time">
						<xsl:with-param name="time" select="Time" />
						</xsl:call-template>
					</td>
					<td>
					  UTC
                    </td>
					<td>
                        <xsl:value-of select="UserName" />
                    </td>
					<td>
                        <xsl:value-of select="ChangeTypeName"/>
                    </td> 
					<td>
                        <xsl:value-of select="ActionDescription"/>
                    </td> 
					<td>
                        <xsl:value-of select="AdminSectionName"/>
                    </td>					
					<td>
                        <xsl:value-of select="OldValue"/>
                    </td>
					<td>
                        <xsl:value-of select="NewValue"/>
                    </td>
									
                </tr>
            </xsl:for-each>
        </table>
    </xsl:template>
   <xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-after($datetime, ''T'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year)" />
  </xsl:template>
  <xsl:template name="format-time">
    <xsl:param name="time"/>
	<xsl:variable name="time-ampm" select="substring-after($time, '''')"/>
      <xsl:variable name="time1" select="substring-before($time-ampm, '' '')"/>
      <xsl:variable name="ampm" select="substring-after($time-ampm, '' '')"/>
      <xsl:variable name="h" select="substring-before($time1, '':'')"/>
      <xsl:variable name="m-s" select="substring-after($time1, '':'')"/>
      <xsl:variable name="m" select="substring-before($m-s, '':'')"/>
      <xsl:variable name="s" select="substring-after($m-s, '':'')"/>

      <xsl:variable name="hh">
        <xsl:choose>
          <xsl:when test="$ampm = ''PM''">
            <xsl:value-of select="format-number($h + 12, ''00'')"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="format-number($h, ''00'')"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>     
      <xsl:value-of select="concat($time-ampm,'';'')"/>

  </xsl:template>
</xsl:stylesheet>', 1)
INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) VALUES (34, 52, N'UserAuditTrailReport', N'User Audit Trail Report', N'This section displays a history grid that shows the histroy from User History Changes.', N'<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
	<xsl:template match="/UserHistoryData">
		<table class="DataGrid" style="width:100%">
			<tr>
				<th>Change ID</th>
				<th>Change Date</th>
				<th>Time</th>
				<th>Time Zone</th>
				<th>Changed By</th>
				<th>Action</th>
				<th>Event Description</th>
				<th>Property Name</th>
				<th>Before Value</th>
				<th>After Value</th>
			</tr>
			<xsl:for-each select="UserHistory">
				<tr>
					<td>
						<xsl:value-of select="ChangeSetId"/>
					</td>
					<td class="Date">
						<xsl:call-template name="format-date">
							<xsl:with-param name="datetime" select="ChangeDate" />
						</xsl:call-template>
					</td>
					<td>
						<xsl:call-template name="format-time">
							<xsl:with-param name="time" select="Time" />
						</xsl:call-template>
					</td>
					<td>         UTC                      </td>
					<td>
						<xsl:value-of select="UpdatedUserName" />
					</td>
					<td>
						<xsl:value-of select="ChangeTypeName"/>
					</td>
					<td>
						<xsl:value-of select="FieldName"/>
					</td>
					<td>
						<xsl:value-of select="PropertyName"/>
					</td>
					<td>
						<xsl:value-of select="OldValue"/>
					</td>
					<td>
						<xsl:value-of select="NewValue"/>
					</td>
				</tr>
			</xsl:for-each>
		</table>
	</xsl:template>
	<xsl:template name="format-date">
		<xsl:param name="datetime"/>
		<xsl:variable name="date" select="substring-before($datetime, ''T'')" />
		<xsl:variable name="year" select="substring-before($date, ''-'')" />
		<xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
		<xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
		<xsl:variable name="time" select="substring-after($datetime, ''T'')" />
		<xsl:variable name="monthname">
			<xsl:choose>
				<xsl:when test="$month=''01''">
					<xsl:value-of select="''Jan''"/>
				</xsl:when>
				<xsl:when test="$month=''02''">
					<xsl:value-of select="''Feb''"/>
				</xsl:when>
				<xsl:when test="$month=''03''">
					<xsl:value-of select="''Mar''"/>
				</xsl:when>
				<xsl:when test="$month=''04''">
					<xsl:value-of select="''Apr''"/>
				</xsl:when>
				<xsl:when test="$month=''05''">
					<xsl:value-of select="''May''"/>
				</xsl:when>
				<xsl:when test="$month=''06''">
					<xsl:value-of select="''Jun''"/>
				</xsl:when>
				<xsl:when test="$month=''07''">
					<xsl:value-of select="''Jul''"/>
				</xsl:when>
				<xsl:when test="$month=''08''">
					<xsl:value-of select="''Aug''"/>
				</xsl:when>
				<xsl:when test="$month=''09''">
					<xsl:value-of select="''Sep''"/>
				</xsl:when>
				<xsl:when test="$month=''10''">
					<xsl:value-of select="''Oct''"/>
				</xsl:when>
				<xsl:when test="$month=''11''">
					<xsl:value-of select="''Nov''"/>
				</xsl:when>
				<xsl:when test="$month=''12''">
					<xsl:value-of select="''Dec''"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="''''" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year)" />
	</xsl:template>
	<xsl:template name="format-time">
		<xsl:param name="time"/>
		<xsl:variable name="time-ampm" select="substring-after($time, '''')"/>
		<xsl:variable name="time1" select="substring-before($time-ampm, '' '')"/>
		<xsl:variable name="ampm" select="substring-after($time-ampm, '' '')"/>
		<xsl:variable name="h" select="substring-before($time1, '':'')"/>
		<xsl:variable name="m-s" select="substring-after($time1, '':'')"/>
		<xsl:variable name="m" select="substring-before($m-s, '':'')"/>
		<xsl:variable name="s" select="substring-after($m-s, '':'')"/>
		<xsl:variable name="hh">
			<xsl:choose>
				<xsl:when test="$ampm = ''PM''">
					<xsl:value-of select="format-number($h + 12, ''00'')"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="format-number($h, ''00'')"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:value-of select="concat($time-ampm,'';'')"/>
	</xsl:template>
</xsl:stylesheet>', 1)


INSERT [dbo].[TST_REPORT_SECTION] ([REPORT_SECTION_ID], [ARTIFACT_TYPE_ID], [TOKEN], [NAME], [DESCRIPTION], [DEFAULT_TEMPLATE], [IS_ACTIVE]) 
VALUES (54, 49, N'OldProjectAuditTrail', N'Old Project Audit Trail Report', 
N'This section displays a history grid that shows the histroy from Product old History Changes.',
 N'<?xml version="1.0" encoding="utf-8"?>  <xsl:stylesheet version="1.0"       
 xmlns:xsl="http://www.w3.org/1999/XSL/Transform"      
 xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">      
 <xsl:template match="/ProductHistoryData">          
 <table class="DataGrid" style="width:100%">              
 <tr>      
 <th>Change ID</th>      
 <th>Change Date</th>      
 <th>Changed By</th>            
 <th>Artifact Type</th>          
 <th>ArtifactId</th>      
 <th>ArtifactTypeName</th>                  
 <th>Time Zone</th>             
 <th>Change Type</th>                 
 <th>Signed</th>                              
 </tr>              
 <xsl:for-each select="ProductHistory">                  
 <tr>                      
 <td>                          
 <xsl:value-of select="ChangeSetId"/>                      
 </td>        
 <td class="Date">        
 <xsl:call-template name="format-date">        
 <xsl:with-param name="datetime" select="ChangeDate" />        
 </xsl:call-template>       
 </td>       
 <td>                          
 <xsl:value-of select="UserName" />                      
 </td>              
 <td>                          
 <xsl:value-of select="ArtifactTypeName"/>                      
 </td>       
 <td>                          
 <xsl:value-of select="ArtifactId"/>                      
 </td>       
 <td>                          
 <xsl:value-of select="ArtifactDesc"/>                      
 </td>       
 <td>         
 UTC                      
 </td>              
 <td>                          
 <xsl:value-of select="ChangeTypeName"/>                      
 </td>                         
 <td>                          
 <xsl:value-of select="SignedValue"/>                      
 </td>                                    
 </tr>              
 </xsl:for-each>          
 </table>      
 </xsl:template>     
<xsl:template name="format-date">
    <xsl:param name="datetime"/>
    <xsl:variable name="date" select="substring-before($datetime, ''T'')" />
    <xsl:variable name="year" select="substring-before($date, ''-'')" />
    <xsl:variable name="month" select="substring-before(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="day" select="substring-after(substring-after($date, ''-''), ''-'')" />
    <xsl:variable name="time" select="substring-after($datetime, ''T'')" />
    <xsl:variable name="monthname">
      <xsl:choose>
        <xsl:when test="$month=''01''">
          <xsl:value-of select="''Jan''"/>
        </xsl:when>
        <xsl:when test="$month=''02''">
          <xsl:value-of select="''Feb''"/>
        </xsl:when>
        <xsl:when test="$month=''03''">
          <xsl:value-of select="''Mar''"/>
        </xsl:when>
        <xsl:when test="$month=''04''">
          <xsl:value-of select="''Apr''"/>
        </xsl:when>
        <xsl:when test="$month=''05''">
          <xsl:value-of select="''May''"/>
        </xsl:when>
        <xsl:when test="$month=''06''">
          <xsl:value-of select="''Jun''"/>
        </xsl:when>
        <xsl:when test="$month=''07''">
          <xsl:value-of select="''Jul''"/>
        </xsl:when>
        <xsl:when test="$month=''08''">
          <xsl:value-of select="''Aug''"/>
        </xsl:when>
        <xsl:when test="$month=''09''">
          <xsl:value-of select="''Sep''"/>
        </xsl:when>
        <xsl:when test="$month=''10''">
          <xsl:value-of select="''Oct''"/>
        </xsl:when>
        <xsl:when test="$month=''11''">
          <xsl:value-of select="''Nov''"/>
        </xsl:when>
        <xsl:when test="$month=''12''">
          <xsl:value-of select="''Dec''"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="''''" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="concat($day, ''-'' ,$monthname, ''-'', $year)" />
  </xsl:template>
  <xsl:template name="format-time">
    <xsl:param name="time"/>
	<xsl:variable name="time-ampm" select="substring-after($time, '''')"/>
      <xsl:variable name="time1" select="substring-before($time-ampm, '' '')"/>
      <xsl:variable name="ampm" select="substring-after($time-ampm, '' '')"/>
      <xsl:variable name="h" select="substring-before($time1, '':'')"/>
      <xsl:variable name="m-s" select="substring-after($time1, '':'')"/>
      <xsl:variable name="m" select="substring-before($m-s, '':'')"/>
      <xsl:variable name="s" select="substring-after($m-s, '':'')"/>

      <xsl:variable name="hh">
        <xsl:choose>
          <xsl:when test="$ampm = ''PM''">
            <xsl:value-of select="format-number($h + 12, ''00'')"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="format-number($h, ''00'')"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>     
      <xsl:value-of select="concat($time-ampm,'';'')"/>

  </xsl:template>
</xsl:stylesheet>', 1)

SET IDENTITY_INSERT [dbo].[TST_REPORT_SECTION] OFF
