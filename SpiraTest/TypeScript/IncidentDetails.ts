//external dependendencies (js libraries)
declare var $: any;
declare var $find: any;
declare var $get: any;
declare var Type: any;
declare var ko: any;
declare var AspNetAjax$Function: any;

//inflectra services
declare var globalFunctions: any;
declare var Inflectra: any;
declare var SpiraContext: any;

//Control IDs
declare var datClosedDate_id: string;
declare var txtIncidentId_id: string;
declare var lblMessage_id: string;

//URL Templates
declare var urlTemplate_artifactRedirectUrl: string;

function ddlResolvedRelease_selectedItemChanged(item)
{
	//Get the current state of the form manager
    var ajxFormManager = $find(ajxFormManager_id);
	var unsavedChanges = ajxFormManager.get_unsavedChanges();

	//Get the new releaseId
	if (item.get_value() == '')
	{
        var ddlBuild = $find(ddlBuild_id);
		ddlBuild.clearItems();
		ddlBuild.addItem('', '-- ' + resx.Global_None + ' --');
		ddlBuild.set_selectedItem('');
	}
	else
	{
        var releaseId = item.get_value();
		//Get the list of builds for this release from the web service
		Inflectra.SpiraTest.Web.Services.Ajax.BuildService.GetBuildsForRelease(
            SpiraContext.ProjectId, 
            releaseId, 
            ddlResolvedRelease_selectedItemChanged_success, 
            ddlResolvedRelease_selectedItemChanged_failure, 
            unsavedChanges
            );
		ajxFormManager.update_saveButtons(unsavedChanges);
	}
}
function ddlResolvedRelease_selectedItemChanged_success (data, unsavedChanges)
{
    //Clear values and databind
    var ddlBuild = $find(ddlBuild_id);
	ddlBuild.clearItems();
	ddlBuild.addItem('', '-- ' + resx.Global_None + ' --');
	if (data)
	{
		ddlBuild.set_dataSource(data);
		ddlBuild.dataBind();
        var ajxFormManager = $find(ajxFormManager_id);
		if (ajxFormManager && ajxFormManager.get_dataItem())
		{
			var dataItem = ajxFormManager.get_dataItem();
			if (dataItem.Fields.BuildId)
			{
				var buildId = dataItem.Fields.BuildId.intValue;
				if (buildId)
				{
				    ddlBuild.set_selectedItem(buildId);
				}
				else
				{
				    ddlBuild.set_selectedItem('');
				}
			}
			else
			{
				ddlBuild.set_selectedItem('');
			}
		}
		else
		{
			ddlBuild.set_selectedItem('');
		}
	}
}
function ddlResolvedRelease_selectedItemChanged_failure (error, unsavedChanges)
{
    var ddlBuild = $find(ddlBuild_id);
	ddlBuild.clearItems();
	ddlBuild.addItem('', '-- ' + resx.Global_None + ' --');
	ddlBuild.set_selectedItem('');

	//ignore error, just clear dropdown values
}
function ajxWorkflowOperations_operationExecuted(transitionId, isStatusOpen)
{
	update_closed_date(isStatusOpen);
}
function ajxFormManager_operationReverted(statusId, isStatusOpen)
{
	update_closed_date(isStatusOpen);
}
function update_closed_date(isStatusOpen)
{
	//If we have a closed status, fill in the Closed Date/Time, otherwise blank it out
    var closedDate = $find(datClosedDate_id);
	if (isStatusOpen)
	{
		closedDate.clearDatetime();
	}
	else
	{
		closedDate.selectCurrent();
	}
}

//Redirects to a different incident
function btnFind_click()
{
    //Get the new id
    var newArtifactId = parseInt($('#' + txtIncidentId_id).val());
    if (newArtifactId && newArtifactId != NaN && newArtifactId > 0)
    {
        //Make sure it exists before we redirect to it
        Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService.Incident_CheckExists(SpiraContext.ProjectId, newArtifactId, btnFind_click_success, btnFind_click_failure, newArtifactId);
    }
}
function btnFind_click_success(exists, newArtifactId)
{
    if (exists)
    {
        var ajxFormManager = $find(ajxFormManager_id);
        ajxFormManager.set_suppressErrors(true);
        SpiraContext.ArtifactId = newArtifactId;
        ajxFormManager.set_primaryKey(newArtifactId, true);
        ajxFormManager.load_data();
        //Also need to rewrite the URL to match
        if (history && history.pushState)
        {
            //We set the URL of the page to match the item we're loading
            var newIncidentUrl = urlTemplate_artifactRedirectUrl.replace(globalFunctions.artifactIdToken, newArtifactId.toString());
            history.pushState(null, null, newIncidentUrl);
        }
    }
    else
    {
        alert(resx.IncidentDetails_IncidentNotFound);
    }
}
function btnFind_click_failure(ex)
{
    var msg = $get(lblMessage_id);
    globalFunctions.display_error(msg, ex);
}

//Set placeholder on form manager, for new items
$(document).ready(function ()
{
    if (SpiraContext.PlaceholderId && SpiraContext.PlaceholderId > 0)
    {
        $('#' + hdnPlaceholderId_id).val(SpiraContext.PlaceholderId);
    }
});
