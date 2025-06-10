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
declare var txtRiskId_id: string;
declare var lblMessage_id: string;
declare var grdMitigations_id: string;
declare var lnkStepInsert_id: string;
declare var lnkStepDelete_id: string;
declare var lnkStepCopy_id: string;

//URL Templates
declare var urlTemplate_artifactRedirectUrl: string;

function riskDetails_ajxWorkflowOperations_operationExecuted(transitionId, isStatusOpen)
{
	riskDetails_update_closed_date(isStatusOpen);
}
function riskDetails_ajxFormManager_operationReverted(statusId, isStatusOpen)
{
	riskDetails_update_closed_date(isStatusOpen);
}
function riskDetails_update_closed_date(isStatusOpen)
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

//Redirects to a different risk
function riskDetails_btnFind_click()
{
    //Get the new id
    var newArtifactId = parseInt($('#' + txtRiskId_id).val());
    if (newArtifactId && newArtifactId != NaN && newArtifactId > 0)
    {
        //Make sure it exists before we redirect to it
        Inflectra.SpiraTest.Web.Services.Ajax.RisksService.Risk_CheckExists(SpiraContext.ProjectId, newArtifactId, riskDetails_btnFind_click_success, riskDetails_btnFind_click_failure, newArtifactId);
    }
}
function riskDetails_btnFind_click_success(exists, newArtifactId)
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
            var newRiskUrl = urlTemplate_artifactRedirectUrl.replace(globalFunctions.artifactIdToken, newArtifactId.toString());
            history.pushState(null, null, newRiskUrl);
        }
    }
    else
    {
        alert(resx.RiskDetails_RiskNotFound);
    }
}
function riskDetails_btnFind_click_failure(ex)
{
    var msg = $get(lblMessage_id);
    globalFunctions.display_error(msg, ex);
}

//Updates any page specific content
var riskDetails_riskId = -1;
//@ts-ignore
function updatePageContent(): void {
    //See if we have a new or existing risk
    if (SpiraContext.Mode == 'new') {
        //Hide the mitigations
        $('#divMitigationsPanel').hide();
    }
    else {
        //Show the mitigations
        $('#divMitigationsPanel').show();

        //see if we have permissions to edit mitigations - required due to modify owned case
        var isDisabled = false;
        var ajxFormManager = $find(ajxFormManager_id);
        var isCreatorOrOwner = ajxFormManager.get_isArtifactCreatorOrOwner();
        var authorizedViewTS = globalFunctions.isAuthorized(globalFunctions.permissionEnum.Modify, globalFunctions.artifactTypeEnum.risk);
        if (authorizedViewTS == globalFunctions.authorizationStateEnum.prohibited) {
            isDisabled = true;
        }
        if (authorizedViewTS == globalFunctions.authorizationStateEnum.limited && !isCreatorOrOwner) {
            isDisabled = true;
        }

        // enable/disable the buttons at the top of the mitigations grid
        if (isDisabled) {
            document.getElementById(lnkStepInsert_id).setAttribute("disabled", "disabled");
            document.getElementById(lnkStepDelete_id).setAttribute("disabled", "disabled");
            document.getElementById(lnkStepCopy_id).setAttribute("disabled", "disabled");

        }
        else {
            document.getElementById(lnkStepInsert_id).removeAttribute("disabled");
            document.getElementById(lnkStepDelete_id).removeAttribute("disabled");
            document.getElementById(lnkStepCopy_id).removeAttribute("disabled");
        }

        //See if the artifact id has changed
        var grdMitigations = $find(grdMitigations_id);


        if (riskDetails_riskId != SpiraContext.ArtifactId) {
            // enable/disable editing of the grid itself
            grdMitigations.set_allowEdit(!isDisabled);

            //Load the scenario steps
            var filters = {};
            filters[globalFunctions.keyPrefix + 'RiskId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
            grdMitigations.set_standardFilters(filters);
            grdMitigations.load_data();
            riskDetails_riskId = SpiraContext.ArtifactId;
        }
    }
}

//Set placeholder on form manager, for new items
$(document).ready(function ()
{
    if (SpiraContext.PlaceholderId && SpiraContext.PlaceholderId > 0)
    {
        $('#' + hdnPlaceholderId_id).val(SpiraContext.PlaceholderId);
    }
});
