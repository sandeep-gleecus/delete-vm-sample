<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SourceCodeCommits.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.SourceCodeCommits" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<a 
    aria-label="More Information About This Item"
    class="u-help" 
    id="help-SourceCodeCommits"  
    role="button" 
    tabindex="0" 
    >
    <i class="fas fa-info-circle fa-lg" aria-hidden="true"></i>
</a>

<asp:PlaceHolder runat="server" ID="plcBranchInfo">
    <div class="btn-group" role="group">
        <span class="label-addon">
            <asp:Localize Text="<%$Resources:Main,SourceCodeList_CurrentBranch %>" runat="server" />: 
        </span>
		<tstsc:DropMenu 
            id="mnuBranches" 
            runat="server" 
            GlyphIconCssClass="mr3 fas fa-code-branch" 
            ClientScriptMethod="void(0)" 
            ButtonTextSelectsItem="true"
            />
    </div>
</asp:PlaceHolder>

<div id="noSourceCodeProvidersLoaded" class="alert alert-warning" style="display:none"> 
    <span class="fas fa-info-circle"></span>
    <asp:Localize runat="server" Text="<%$Resources:Main,SourceCodeCommits_PleaseEnableSourceCode %>" /> <tstsc:HyperLinkEx runat="server" Text="<%$Resources:Main,GlobalNavigation_Administration %>" ID="lnkAdministration" />
</div>

<div class="u-chart db" id="c3_sourceCodeCommits">
</div>

<div class="well" id="recent-revisions">
    <h3 class="mt0">
        <asp:Localize runat="server" Text="<%$Resources:ClientScript,SourceCode_RecentCommits %>" />
        <tstsc:HyperLinkEx ID="lnkViewRevisions" SkinID="ButtonDefault" runat="server" Text="<%$Resources:Main,Global_ViewAll %>" /><br />
    </h3>

    <div id="target-revisions">
        <asp:Localize runat="server" Text="<%$Resources:ClientScript,GlobalFunctions_TooltipLoading %>" />
    </div>
    <script id="template-revisions" type="x-tmpl-mustache">
        <span id="lstRevisions" class="db pl3 mb2 mt1">
            {{#.}}
            <span class="ws-nowrap dib px3 py2 br2 bg-vlight-gray-hover transition-all">
                <tstsc:ImageEx runat="server" CssClass="w4 h4" ImageUrl="Images/artifact-Revision.svg" />
                <a class="tdn py1" href="{{ customUrl }}" onmouseover="ddrivetip('<u>{{ Fields.Name.textValue }} - {{ Fields.UpdateDate.tooltip }}</u><br />{{ Fields.Message.textValue }}<br /><i>- {{ Fields.AuthorName.textValue }}</i>')" onmouseout="hideddrivetip()">
                    {{ Fields.Name.textValue }}</a>
                <small>
                (<span>{{ Fields.UpdateDate.textValue }}</span>)
                </small>
            </span>
            
            {{/.}}
        </span>
    </script>
</div>

<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
    <Scripts>
        <asp:ScriptReference Assembly="Web" Name="Inflectra.SpiraTest.Web.ClientScripts.mustache.js" />
    </Scripts>
    <Services>  
        <asp:ServiceReference Path="~/Services/Ajax/SourceCodeRevisionService.svc" />  
        <asp:ServiceReference Path="~/Services/Ajax/SourceCodeFileService.svc" />  
    </Services>  
</tstsc:ScriptManagerProxyEx>

<script type="text/javascript">
    // Manage info popup
    var resx = Inflectra.SpiraTest.Web.GlobalResources;
    $('#help-SourceCodeCommits').popover({
        content: resx.InAppHelp_Chart_SourceCodeCommits,
        placement: "left",
        trigger: "hover focus",
        delay: { "show": 400, "hide": 100 }
    });

    $(document).ready(function ()
    {
        //Change MustacheJS to escape single quotes
        Mustache.escape = function (string)
        {
            return string.replace('\'', '\\\'');
        };
    });

    //We need two event handles because the page can reload using an ASP.NET AJAX UpdatePanel
    document.addEventListener("DOMContentLoaded", function () { load_commitsGraph(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (sender, args) { load_commitsGraph(); });

    function load_commitsGraph()
    {
        //Load the count of commits by dates
        var dateRange = null; //Use last 90 days
        var dateFormatMonthFirst = <%=DateFormatMonthFirst.ToString().ToLowerInvariant()%>;
        Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionService.SourceCodeRevision_RetrieveCommitCounts(SpiraContext.ProjectId, dateRange, function (data)
        {
            //Success
            var cleanedData = prepDataForC3(data);

            var sourceCodeCommits = c3.generate({
                bindto: d3.select('#c3_sourceCodeCommits'),
                data: {
                    x: 'x',
                    columns: cleanedData.columns,
                    type: "bar",
                    colors: cleanedData.colors,
                    color: function (color, d) {
                        // d will be 'id' when called for legends
                        //make the bottom quartile lighter and the upper quartile darker
                        return d.value && d.value <= cleanedData.bottomQuartile ? d3.rgb(color).darker(1) : 
                            d.value >= cleanedData.topQuartile ? d3.rgb(color).brighter(1) : color;
                    }
                },
                bar: {
                    width: {
                        ratio: 0.8
                    }
                },
                grid: {
                    y: {
                        show: true
                    }
                },
                axis: {
                    x: {
                        type: 'timeseries',
                        tick: {
                            format: function (x) {
                                if (dateFormatMonthFirst)
                                    return (x.getMonth()+1) + '/' + x.getDate();
                                else
                                    return x.getDate() + '/' + (x.getMonth()+1);
                            }
                        }
                    }
                },
                legend: {
                    show: false
                }
            });
        }, function (ex)
        {
            //See if the error is due to source code not enabled
            errorType = ex.get_exceptionType();
            if (errorType == 'Provider Error')
            {
                $('#noSourceCodeProvidersLoaded').show();
                $('#c3_sourceCodeCommits').hide();
            }
            else
            {
                //Display error
                var messageBox = $get('<%=this.MessageBoxClientID%>');
                globalFunctions.display_error(messageBox, ex);
            }
        });

        //Load any recent revisions
        var branchPath = '<%=GlobalFunctions.JSEncode(CurrentBranchKey)%>';
        load_recent_revisions(branchPath);

        function prepDataForC3(data)
        {
            var res = new Object();
            var dataLength = data.length,
                dataValues = data.map(function (val) { return val.count }),
                minValue = Math.min.apply(null, dataValues),
                maxValue = Math.max.apply(null, dataValues),
                valueRange = maxValue - minValue;
            res.bottomQuartile = minValue + (valueRange / 4);
            res.topQuartile = maxValue - (valueRange / 4);

            if (data)
            {
                res.colors = {};
                res.colors[resx.SourceCode_Commits] = '#FB8532';
                res.columns = [['x'], [resx.SourceCode_Commits]];
                for (var i = 0; i < dataLength; i++)
                {
                    //Convert the WCF date
                    var date;
                    if (typeof data[i].interval.getMonth === 'function')
                    {
                        date = data[i].interval;
                    }
                    else
                    {
                        date = new Date(parseInt(data[i].interval.substr(6)));
                    }
                    res.columns[0].push(date);
                    res.columns[1].push(data[i].count);
                }
            }
            return res;
        }

    }

    function load_recent_revisions(branchPath)
    {
        Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionService.SourceCodeRevision_RetrieveRecent(SpiraContext.ProjectId, branchPath, 5, load_recent_revisions_success, load_recent_revisions_failure);
    }
    function load_recent_revisions_success(dataItems)
    {
        var template = document.getElementById('template-revisions').innerHTML;
        var rendered = Mustache.render(template, dataItems);
        document.getElementById('target-revisions').innerHTML = rendered;
    }
    function load_recent_revisions_failure(ex)
    {
        //See if the error is due to source code not enabled
        errorType = ex.get_exceptionType();
        if (errorType == 'Provider Error') {
            //Hide the entire recent commits box
            $('#recent-revisions').hide();
        }
        else {
            var lblMessage = $get('<%=this.Message.ClientID%>');
            globalFunctions.display_error(lblMessage, ex);
        }
    }

    function mnuBranches_click(branchPath)
    {
        //Set the new branch
        Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeFileService.SourceCode_SetSelectedBranch(SpiraContext.ProjectId, branchPath, mnuBranches_click_success, mnuBranches_click_failure, branchPath);
    }
    function mnuBranches_click_success(data, branchPath)
    {
        //Change the branch display
        var mnuBranches = $find('<%=this.mnuBranches.ClientID%>');
        mnuBranches.set_text(branchPath);
        mnuBranches.update_menu();
        mnuBranches.refreshItems(true);

        //Reload the grid and graph
        load_commitsGraph();
        load_recent_revisions(branchPath);
    }
    function mnuBranches_click_failure(ex) {
        var lblMessage = $get('<%=this.Message.ClientID%>');
        globalFunctions.display_error(lblMessage, ex);
    }
</script>
