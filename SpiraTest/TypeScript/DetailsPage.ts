//external dependendencies (js libraries)
declare var $: any;
declare var $find: any;

//interfaces
interface ITstucPanel
{
    set_artifactTypeId(value: number): void;
    set_artifactId(value: number): void;
    get_artifactId(): number;
    load_data(filters: any, loadNow: boolean): void;
    check_hasData(callback: any): void;
    closeAddAssocationPanel(): void;
    update_permissions(): void;
}

//inflectra services
declare var ArtifactEmail_isUserSubscribed: any;
declare var globalFunctions: any;
declare var Inflectra: any;
declare var SpiraContext: any;
declare var tstucMessageManager: any;

//inflectra objects
declare var ajxFormManager_id: string;
declare var btnSave_id: string;
declare var btnSubscribe_id: string;
declare var lstComments_id: string;
declare var urlTemplate_projectHome: string;
declare var txtName_id: string;
declare var tabControl_id: string;
declare var navigationBar_id: string;
declare var hdnPlaceholderId_id: string;
declare var btnNewComment_id: string;
declare var btnEmail_id: string;

//Other Panels
declare var tstucAttachmentPanel: ITstucPanel;
declare var tstucHistoryPanel: ITstucPanel;
declare var tstucBaselinePanel: ITstucPanel;
declare var tstuc_cplMainContent_tstAssociationPanel: ITstucPanel;
declare var tstucTestRunPanel: ITstucPanel;
declare var tstucTestSetPanel: ITstucPanel;
declare var tstucIncidentPanel: ITstucPanel;
declare var tstuc_cplMainContent_tstCoveragePanel: ITstucPanel;
declare var tstucTaskListPanel: ITstucPanel;
declare var tstuc_cplMainContent_tstReleaseMappingPanel: ITstucPanel;
declare var tstuc_cplMainContent_tstTestCaseMapping: ITstucPanel;
declare var tstucRequirementTaskPanel: ITstucPanel;
declare var tstucDiagramPanel: ITstucPanel;

//Panel IDs
declare var pnlAttachments_id: string;
declare var pnlHistory_id: string;
declare var pnlAssociations_id: string;
declare var pnlTestRuns_id: string;
declare var pnlTestSets_id: string;
declare var pnlIncidents_id: string;
declare var pnlTasks_id: string;
declare var pnlCoverage_id: string;
declare var pnlReleases_id: string;
declare var pnlTestCases_id: string
declare var pnlReqsTasks_id: string;
declare var pnlDiagram_id: string;
declare var pnlBaseline_id: string;

//External functions
declare function updatePageContent(): void;

//Used by CKEDITOR
declare var CKEDITOR: any;
class PageProps
{
    loadedOK: boolean;
    screenshotUploadUrl: string;
    associationWithoutPanel: boolean;
    isTestExecution: boolean;
    includeStepHistory: boolean;
}

var pageProps = new PageProps();

//Base Urls
declare var urlTemplate_artifactListUrl: string;
declare var urlTemplate_artifactRedirectUrl: string;
declare var urlTemplate_screenshot: string;
declare var urlTemplate_artifactNew: string;

//for form layout
const u_params = {
    width_lg: 992,
    width_md: 768,
    width_sm: 554,
    names: {
        base: 'width_',
        lg: 'width_lg',
        md: 'width_md',
        sm: 'width_sm',
        xs: 'width_xs'
    },
};
//for resizable title input box
let artifactNameResizeContainer,
    artifactNameResizeChecker,
    artifactNameResizeField;

let initialLoad: boolean = true;

function artifactNameAutoResize()
{
    if (artifactNameResizeField)
    {
        //Cannot use innerHTML because of XSS vulnerabilities
        $(artifactNameResizeChecker).text(artifactNameResizeField.value + '\n');
    }
};

/**
 ** event handlers
 **/

$(function ()
{
    //update layout on page load
    uWrapper_onResize();

    //retrieve user settings for collapsible panels
    if (SpiraContext.HasCollapsiblePanels)
    {
        Inflectra.SpiraTest.Web.Services.Ajax.GlobalService.CollapsiblePanel_RetrieveStateAll(SpiraContext.pageId, retrieveCollapsiblePanelStates_Success, retrieveCollapsiblePanelStates_Success);
    }

    //for images that have theme icons they need to be set dynamically to add the base url to the img src. 
    function setThemeIcons() {
        var iconList: NodeListOf<Element> = document.querySelectorAll("img[data-themeIcon]");
        if (iconList.length) {
            for (let icon of iconList as any) {
                icon.src = SpiraContext.BaseThemeUrl + "Images/" + icon.dataset.themeicon;
            }
        } 
    }
    setThemeIcons();

    function retrieveCollapsiblePanelStates_Success(data)
    {
        //filter the result to just those groups that should be expanded
        var collapsedPanels = data.filter(function (item)
        {
            return item.value === "Y" // 'Y' represents a true to the setting IsCollapsed
        }).map(function (item)
        {
            return item.key;
        })
        //get the list of expand/collapse field groups 
        $('[data-collapsible]').each(function (group)
        {
            if (collapsedPanels.indexOf($(this).attr("id")) >= 0)
            {
                var headerElement = $(this).find(".u-box_header");
                manageFieldGroupExpandCollapse(headerElement);
            }
        })
    }

    function retrieveCollapsiblePanelStates_Failure(err)
    {
        //fail quietly
        console.log(err);
    }

    //Manage open and close of field groups
    $('.u-box_header').on('click', function (e)
    {
        e.preventDefault();
        //update the ui to expand/collapse the field group
        manageFieldGroupExpandCollapse(this);
    });

    //Hide any field groups that are completely empty
    //Only required on page load
    hideEmptyFieldGroups();

    //Hide email buttons if disabled
    if (!SpiraContext.IsEmailEnabled)
    {
        $('[data-requires-email="true"]').hide();
    }
});
$(window).on("resize", function ()
{
    //update layout on page resize
    uWrapper_onResize();
});
//... and on sidebar resize function above is also called - from relevant js file

//get the list of followers for this artifact - handle opening of the follower list on success
document.addEventListener("DOMContentLoaded", function ()
{
    //for managing the auto resizing textarea name field of the artifact
    artifactNameResizeContainer = document.querySelector('.textarea-resize_container');

    //only manage it when there is a confirmed resize container on the page
    if (artifactNameResizeContainer)
    {
        artifactNameResizeChecker = artifactNameResizeContainer.querySelector('.textarea-resize_checker');
        artifactNameResizeField = artifactNameResizeContainer.querySelector('.textarea-resize_field');

        artifactNameAutoResize();
        if (artifactNameResizeField)
        {
            artifactNameResizeField.addEventListener('input', artifactNameAutoResize);
            //disable user typing an enter character into the field
            artifactNameResizeField.addEventListener('keypress', function (event) {
                if (event.keyCode == 13) event.preventDefault();
            });
        }
    }
});

/**
 ** functions
 **/

//update layout function
function uWrapper_onResize()
{

    // you can have wrappers that you don't want to be resized - eg on dialogs. 
    // so only change layout classes where they already exist
    // we need to iterate over wrapper with a width class so each is dealt with separately - as they could be currently hidden, or in differently sized containers
    $(`.u-wrapper.${u_params.names.lg}, .u-wrapper.${u_params.names.md}, .u-wrapper.${u_params.names.sm}, .u-wrapper.${u_params.names.xs}`).each(function resizeWrapper()
    {

        var wrapperWidth = $(this).width(),
            originalWidthClass = $(this).hasClass(u_params.names.lg) ? u_params.names.lg :
                $(this).hasClass(u_params.names.md) ? u_params.names.md :
                    $(this).hasClass(u_params.names.sm) ? u_params.names.sm :
                        $(this).hasClass(u_params.names.xs) ? u_params.names.xs : null,
            widthClass = '';

        //assign the correct width bracket
        if (wrapperWidth >= u_params.width_lg)
        {
            widthClass = u_params.names.lg;
        } else if (wrapperWidth >= u_params.width_md)
        {
            widthClass = u_params.names.md;
        } else if (wrapperWidth >= u_params.width_sm)
        {
            widthClass = u_params.names.sm;
        } else
        {
            widthClass = u_params.names.xs;
        }

        //set width classes on wrappers if width bracket has changed
        if (widthClass !== originalWidthClass)
        {
            var allWidthClasses = `${u_params.names.lg} ${u_params.names.md} ${u_params.names.sm} ${u_params.names.xs}`;
            $(this).removeClass(allWidthClasses).addClass(widthClass);
        }
    })
};

//update the ui to expand/collapse the field group
//param: el: the clicked element (this) that is inside of the group to be expanded or collapsed - ie a child of the parent wrapper
function manageFieldGroupExpandCollapse(el)
{
    var oldState = $(el).attr("aria-expanded"),
        newState = oldState === "true" ? false : true,
        isCollapsed = !newState;
    $(el).attr("aria-expanded", newState.toString());
    $(el).find('.u-anim_open-close').toggleClass('is-open o-1-always');

    //now handle updating the user setting on the server
    var groupParentId = $(el).closest('[data-collapsible="true"]').attr('id');
    Inflectra.SpiraTest.Web.Services.Ajax.GlobalService.CollapsiblePanel_UpdateState(SpiraContext.pageId, groupParentId, isCollapsed, updatePanelState_Success, updatePanelState_Failure);

    function updatePanelState_Success(data)
    {
        // no action required
    }
    function updatePanelState_Failure(err)
    {
        // fail quietly
    }
};

//some field groupings can be empty - they have no built in props and there could be zero custom props for that section
//where that's the case we should just hide the entire section - including the header, to avoid confusion to users
function hideEmptyFieldGroups()
{
    //first get all the fieldgroups that use lists of fields
    $(".u-box_group .u-box_list")
        //then find only those that do not have any li elements as children
        .filter(function ()
        {
            return !($(this).children().is('li'));
        })
        //we need to hide the box_group itself - that has the header block in it
        .parent()
        .hide();
};

//make sure all groups with data are shown - if moving from a state where 
function showNonEmptyFieldGroups()
{

    //first get all the fieldgroups that use lists of fields
    $(".u-box_group .u-box_list")
        //then find only those that do not have any li elements as children
        .filter(function ()
        {
            return ($(this).children().is('li'));
        })
        .parent()
        .show();
};

//open followers box
function loadFollowersBox(projectId, artifactTypeId, artifactId)
{
    //send the id of the dropmenu subscribe button to the component
    followers.dropMenu = btnSubscribe_id;
    followers.getFollowersList(projectId, artifactTypeId, artifactId);
};

/**
 ** functions
 **/

function ajxFormManager_dataSaved(operation, newArtifactId)
{
    var ajxFormManager = $find(ajxFormManager_id);
    if (operation == 'close')
    {
        ajxFormManager.set_suppressErrors(true);
        window.location.href = urlTemplate_artifactListUrl;
    }
    if (operation == 'new')
    {
        //See if this artifact creates the item as part of the new process (all but incidents do this)
        if (newArtifactId)
        {
            //Redirect to the new item using live loading
            ajxFormManager.set_suppressErrors(true);
            ajxFormManager.set_primaryKey(newArtifactId, true);
            ajxFormManager.load_data();

            //Also need to rewrite the URL to match
            if (history && history.pushState)
            {
                //We set the URL of the page to match the item we're loading
                var href = urlTemplate_artifactRedirectUrl.replace(globalFunctions.artifactIdToken, '' + newArtifactId);
                history.pushState(newArtifactId, null, href);

                //TODO: Add code to handle browser forward/back buttons like in the navigation bar
            }
        }
        else
        {
            //This is the incident case where we use placeholders
            ajxFormManager.set_suppressErrors(true);
            SpiraContext.Mode = 'new';
            ajxFormManager.set_primaryKey(null, true);
            ajxFormManager.load_data();
            //Also need to rewrite the URL to match
            if (history && history.pushState)
            {
                //We set the URL of the page to match the item we're loading
                var href = urlTemplate_artifactNew;
                history.pushState(null, null, href);
            }
            //Make sure we have a placeholder, then set on the form manager
            if (!SpiraContext.PlaceholderId)
            {
                Inflectra.SpiraTest.Web.Services.Ajax.PlaceHolderService.PlaceHolder_GetNextId(SpiraContext.ProjectId, function (placeholderId)
                {
                    SpiraContext.PlaceholderId = placeholderId;
                    $('#' + hdnPlaceholderId_id).val(SpiraContext.PlaceholderId);
                });
            }
            else
            {
                $('#' + hdnPlaceholderId_id).val(SpiraContext.PlaceholderId);
            }
        }
    }
    if (operation == 'redirect' && newArtifactId)
    {
        ajxFormManager.set_suppressErrors(true);
        SpiraContext.Mode = 'update';
        SpiraContext.ArtifactId = newArtifactId;
        ajxFormManager.set_primaryKey(newArtifactId, true);
        ajxFormManager.load_data();
        //Also need to rewrite the URL to match
        if (history && history.pushState)
        {
            //We set the URL of the page to match the item we're loading
            var newIncidentUrl = urlTemplate_artifactRedirectUrl.replace(globalFunctions.artifactIdToken, newArtifactId);
            history.pushState(null, null, newIncidentUrl);
        }
    }
}

function ajxFormManager_loaded(dontClearMessages)
{
    var ajxFormManager = $find(ajxFormManager_id);

    //Check for artifact not found case
    if (ajxFormManager.get_dataItem() == null)
    {
        ajxFormManager.set_suppressErrors(true);
        window.location.href = urlTemplate_projectHome + "?errorMessage=" + resx.ArtifactDetails_ArtifactNotFound;
        return;
    }
    SpiraContext.ArtifactId = ajxFormManager.get_primaryKey();
    SpiraContext.IsArtifactCreatorOrOwner = ajxFormManager.get_isArtifactCreatorOrOwner();

    //refresh the auto resize text area
    artifactNameAutoResize();

    //Show the correct toolbars
    showHideToolbars();

    //Configure the RTE screenshots to point to the correct URL
    var url;
    var isAuthorizedToAttachDocuments = false;
    if (SpiraContext.Mode == 'new')
    {
        url = urlTemplate_screenshot.replace('{0}', SpiraContext.PlaceholderId);
        url = url.replace('{1}', SpiraContext.PlaceholderArtifactTypeId);

    }
    else
    {
        url = urlTemplate_screenshot.replace('{0}', SpiraContext.ArtifactId);
        url = url.replace('{1}', SpiraContext.ArtifactTypeId);
    }
    //Set the page props since we modified CKEDITOR to look for this (the config value is not always read correctly)
    if (typeof (CKEDITOR) != 'undefined' && CKEDITOR)
    {
        pageProps.screenshotUploadUrl = url;
        for (var instanceName in CKEDITOR.instances)
        {
            CKEDITOR.instances[instanceName].config.uploadUrl = pageProps.screenshotUploadUrl;
        }
    }
    if (SpiraContext.Mode == 'update') {
        //Load the comments list
        if (typeof (lstComments_id) !== 'undefined') {
            var lstComments = $find(lstComments_id);
            lstComments.set_artifactId(SpiraContext.ArtifactId);
            lstComments.load_data(dontClearMessages);
        }

        //See if we're the owner/author and update permissions on various tabs
        var isAuthorizedToModify = false;
        var authorizedState = globalFunctions.isAuthorized(globalFunctions.permissionEnum.Modify, SpiraContext.ArtifactTypeId);
        if (authorizedState != globalFunctions.authorizationStateEnum.prohibited) {
            var isCreatorOrOwner = ajxFormManager.get_isArtifactCreatorOrOwner();
            if (authorizedState == globalFunctions.authorizationStateEnum.authorized || isCreatorOrOwner) {
                isAuthorizedToModify = true;
            }
        }
        if (typeof (btnSave_id) !== 'undefined') {
            $find(btnSave_id).set_authorized(isAuthorizedToModify);
        }

        //Check to see if we are subscribed
        if (typeof (btnSubscribe_id) !== 'undefined' && btnSubscribe_id) {
            ArtifactEmail_isUserSubscribed(SpiraContext.ProjectId, SpiraContext.ArtifactTypeId, SpiraContext.ArtifactId, $find(btnSubscribe_id));

            //update the followers box
            loadFollowersBox(SpiraContext.ProjectId, SpiraContext.ArtifactTypeId, SpiraContext.ArtifactId);
        }
    }
    else if (SpiraContext.Mode == 'new')
    {
        //Make sure to unload the comments list - used in case of save and new for Risks and Incidents
        //Load the comments list
        if (typeof (lstComments_id) !== 'undefined') {
            var lstComments = $find(lstComments_id);
            lstComments.set_artifactId(SpiraContext.ArtifactId);
            lstComments.retrieve_success(null)
        }
    }

    //Update the other tabs
    updateOtherTabs();

    //Reload the nav bar if not initial load
    if (!initialLoad && typeof (navigationBar_id) != 'undefined' && navigationBar_id)
    {
        var navigationBar = $find(navigationBar_id);
        navigationBar.set_selectedItemId(SpiraContext.ArtifactId);
        navigationBar.refresh_data(dontClearMessages);
    }
    else
    {
        initialLoad = false;
    }

    //Update any page specific content not part of the form manager
    if (typeof (updatePageContent) != 'undefined')
    {
        updatePageContent();
    }

    //If we have an instant messenger manager, register the artifact type/id
    if (typeof (tstucMessageManager) != 'undefined' && tstucMessageManager)
    {
        tstucMessageManager.set_artifactId(SpiraContext.ArtifactId);
    }

    //Finally record this artifact in user settings to track recent artifacts (fail quietly)
    if (SpiraContext.ProjectId && SpiraContext.ArtifactTypeId && SpiraContext.ArtifactId) {
        Inflectra.SpiraTest.Web.Services.Ajax.GlobalService.UserSettings_UpdateRecentArtifact(SpiraContext.ProjectId, SpiraContext.ArtifactTypeId, SpiraContext.ArtifactId);
    }
};

function showHideToolbars()
{
    if (SpiraContext.Mode == 'new')
    {
        $('#plcNewArtifactSubmit').show();
        $('#plcToolbar').hide();

        //make sure the overview tab is selected
        var tabParent = $find(tabControl_id),
            overviewTab = tabParent.get_tabPage('tabOverview');
        tabParent._onClick(tabParent, { thisRef: tabParent, tabPage: overviewTab });

        //Hide the History and Associations tabs
        let tabAssociations = $find(tabControl_id).get_tabPage('tabAssociations');
        if (tabAssociations)
        {
            tabAssociations.hide()
        };
        //Hide the comments setion
        let commentSection = document.getElementById("form-group_comments");
        commentSection && $(commentSection).hide();

        $find(tabControl_id).get_tabPage('tabHistory').hide();
        if (typeof (btnNewComment_id) != 'undefined' && btnNewComment_id)
        {
            $('#' + btnNewComment_id).hide();
        }
    }
    else
    {
        $('#plcNewArtifactSubmit').hide();
        $('#plcToolbar').show();
        if (typeof (btnEmail_id) != 'undefined' && btnEmail_id)
        {
            if (SpiraContext.EmailEnabled)
            {
                $('#' + btnEmail_id).show();
            }
            else
            {
                $('#' + btnEmail_id).hide();
            }
        }

        //Show the History and Associations tabs
        let tabAssociations = typeof tabControl_id != "undefined" ? $find(tabControl_id).get_tabPage('tabAssociations') : null;
        if (tabAssociations)
        {
            tabAssociations.show();
        }
        let tabHistory = typeof tabControl_id != "undefined" ? $find(tabControl_id).get_tabPage('tabHistory') : null;
        if (tabHistory)
        {
            tabHistory.show();
        }

        //Show/hide the comments section
        if (typeof (btnNewComment_id) != 'undefined' && btnNewComment_id)
        {
            let commentSection = document.getElementById("form-group_comments");
            commentSection && $(commentSection).show();
            if (SpiraContext.CanUserAddCommentToArtifacts)
            {
                $('#' + btnNewComment_id).show();
            }
            else
            {
                $('#' + btnNewComment_id).hide();
            }
        }
    }
}

//This function handles tabs that are not specific to an individual page
function updateOtherTabs()
{
    //-- Attachments --
    if (typeof (tstucAttachmentPanel) != 'undefined' && tstucAttachmentPanel)
    {
        if (SpiraContext.Mode == 'new')
        {
            //Set the attachment panel to placeholder
            tstucAttachmentPanel.set_artifactId(SpiraContext.PlaceholderId);
            tstucAttachmentPanel.set_artifactTypeId(SpiraContext.PlaceholderArtifactTypeId);

            //Check to see if we have data and update permissions
            tstucAttachmentPanel.check_hasData(attachment_callback);

            //Reload the tab's data
            var filters = {};
            filters[globalFunctions.keyPrefix + 'ArtifactId'] = globalFunctions.serializeValueInt(SpiraContext.PlaceholderId);
            filters[globalFunctions.keyPrefix + 'ArtifactType'] = globalFunctions.serializeValueInt(SpiraContext.PlaceholderArtifactTypeId);
            tstucAttachmentPanel.load_data(filters, loadNow);
        }
        if (SpiraContext.Mode == 'update')
        {
            var artifactChanged = false;
            if (tstucAttachmentPanel.get_artifactId() != SpiraContext.ArtifactId)
            {
                artifactChanged = true;
            }
            tstucAttachmentPanel.set_artifactId(SpiraContext.ArtifactId);
            tstucAttachmentPanel.set_artifactTypeId(SpiraContext.ArtifactTypeId);

            //Check to see if we have data
            tstucAttachmentPanel.check_hasData(attachment_callback);

            if (artifactChanged)
            {
                //See if the tab is visible
                var loadNow = ($find(tabControl_id).get_selectedTabClientId() == pnlAttachments_id);

                //Reload the tab's data
                var filters = {};
                filters[globalFunctions.keyPrefix + 'ArtifactId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                filters[globalFunctions.keyPrefix + 'ArtifactType'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactTypeId);
                tstucAttachmentPanel.load_data(filters, loadNow);
            }
            else
            {
                //Update permissions only, without loading data
                tstucAttachmentPanel.update_permissions();
            }
        }
    }

    //-- History --
    if (typeof (tstucHistoryPanel) != 'undefined' && tstucHistoryPanel)
    {
        if (SpiraContext.Mode == 'update')
        {
            var artifactChanged = false;
            if (tstucHistoryPanel.get_artifactId() != SpiraContext.ArtifactId)
            {
                artifactChanged = true;
            }
            tstucHistoryPanel.set_artifactId(SpiraContext.ArtifactId);
            tstucHistoryPanel.set_artifactTypeId(SpiraContext.ArtifactTypeId);

            //Check to see if we have data
            tstucHistoryPanel.check_hasData(history_callback);

            if (artifactChanged)
            {
                //See if the tab is visible
                var loadNow = ($find(tabControl_id).get_selectedTabClientId() == pnlHistory_id);


                //Reload the tab's data
                var filters = {};
                filters[globalFunctions.keyPrefix + 'ArtifactId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                filters[globalFunctions.keyPrefix + 'ArtifactType'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactTypeId);
                filters[globalFunctions.keyPrefix + 'IsProjectAdmin'] = globalFunctions.serializeValueBool(SpiraContext.IsProjectAdmin);

                //See if we should include step data
                if (pageProps.includeStepHistory)
                {
                    filters[globalFunctions.keyPrefix + 'IncludeSteps'] = globalFunctions.serializeValueBool(true);
                }

                tstucHistoryPanel.load_data(filters, loadNow);
            }
        }
    }

	//-- Baseline --
	if (typeof (tstucBaselinePanel) != 'undefined' && tstucBaselinePanel) {
		if (SpiraContext.Mode == 'update') {
			var artifactChanged = false;
			if (tstucBaselinePanel.get_artifactId() != SpiraContext.ArtifactId) {
				artifactChanged = true;
			}
			tstucBaselinePanel.set_artifactId(SpiraContext.ArtifactId);
			tstucBaselinePanel.set_artifactTypeId(SpiraContext.ArtifactTypeId);

			//Check to see if we have data
			tstucBaselinePanel.check_hasData(baseline_callback);

			if (artifactChanged) {
				//See if the tab is visible
                var loadNow = ($find(tabControl_id).get_selectedTabClientId() == pnlBaseline_id);


				//Reload the tab's data
				var filters = {};
				filters[globalFunctions.keyPrefix + 'ArtifactId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
				filters[globalFunctions.keyPrefix + 'ArtifactType'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactTypeId);
				filters[globalFunctions.keyPrefix + 'IsProjectAdmin'] = globalFunctions.serializeValueBool(SpiraContext.IsProjectAdmin);
				tstucBaselinePanel.load_data(filters, loadNow);
			}
		}
	}

    //-- Coverage --
    if (typeof (tstuc_cplMainContent_tstCoveragePanel) != 'undefined' && tstuc_cplMainContent_tstCoveragePanel)
    {
        if (SpiraContext.Mode == 'update')
        {
            var artifactChanged = false;
            if (tstuc_cplMainContent_tstCoveragePanel.get_artifactId() != SpiraContext.ArtifactId)
            {
                artifactChanged = true;
            }
            tstuc_cplMainContent_tstCoveragePanel.set_artifactId(SpiraContext.ArtifactId);
            tstuc_cplMainContent_tstCoveragePanel.set_artifactTypeId(SpiraContext.ArtifactTypeId);

            //Check to see if we have data
            tstuc_cplMainContent_tstCoveragePanel.check_hasData(coverage_callback);

            if (artifactChanged)
            {
                //close the add new assocation panel
                tstuc_cplMainContent_tstCoveragePanel.closeAddAssocationPanel();

                //See if the tab is visible
                var loadNow = ($find(tabControl_id).get_selectedTabClientId() == pnlCoverage_id);

                //Reload the tab's data
                var filters = {};
                filters[globalFunctions.keyPrefix + 'ArtifactId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                filters[globalFunctions.keyPrefix + 'ArtifactType'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactTypeId);
                tstuc_cplMainContent_tstCoveragePanel.load_data(filters, loadNow);
            }
        }
    }

    //-- Test Case Mapping --
    if (typeof (tstuc_cplMainContent_tstTestCaseMapping) != 'undefined' && tstuc_cplMainContent_tstTestCaseMapping)
    {
        if (SpiraContext.Mode == 'update')
        {
            var artifactChanged = false;
            if (tstuc_cplMainContent_tstTestCaseMapping.get_artifactId() != SpiraContext.ArtifactId)
            {
                artifactChanged = true;
            }
            tstuc_cplMainContent_tstTestCaseMapping.set_artifactId(SpiraContext.ArtifactId);
            tstuc_cplMainContent_tstTestCaseMapping.set_artifactTypeId(SpiraContext.ArtifactTypeId);

            //Check to see if we have data
            tstuc_cplMainContent_tstTestCaseMapping.check_hasData(testCases_callback);

            if (artifactChanged)
            {
                //close the add new assocation panel
                tstuc_cplMainContent_tstTestCaseMapping.closeAddAssocationPanel();

                //See if the tab is visible
                var loadNow = ($find(tabControl_id).get_selectedTabClientId() == pnlTestCases_id);

                //Reload the tab's data
                var filters = {};
                filters[globalFunctions.keyPrefix + 'ArtifactId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                filters[globalFunctions.keyPrefix + 'ArtifactType'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactTypeId);
                tstuc_cplMainContent_tstTestCaseMapping.load_data(filters, loadNow);
            }
        }
    }

    //-- Release Mapping --
    if (typeof (tstuc_cplMainContent_tstReleaseMappingPanel) != 'undefined' && tstuc_cplMainContent_tstReleaseMappingPanel)
    {
        if (SpiraContext.Mode == 'update')
        {
            var artifactChanged = false;
            if (tstuc_cplMainContent_tstReleaseMappingPanel.get_artifactId() != SpiraContext.ArtifactId)
            {
                artifactChanged = true;
            }
            tstuc_cplMainContent_tstReleaseMappingPanel.set_artifactId(SpiraContext.ArtifactId);
            tstuc_cplMainContent_tstReleaseMappingPanel.set_artifactTypeId(SpiraContext.ArtifactTypeId);

            //Check to see if we have data
            tstuc_cplMainContent_tstReleaseMappingPanel.check_hasData(releases_callback);

            if (artifactChanged)
            {
                //close the add new assocation panel
                tstuc_cplMainContent_tstReleaseMappingPanel.closeAddAssocationPanel();

                //See if the tab is visible
                var loadNow = ($find(tabControl_id).get_selectedTabClientId() == pnlReleases_id);

                //Reload the tab's data
                var filters = {};
                filters[globalFunctions.keyPrefix + 'ArtifactId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                filters[globalFunctions.keyPrefix + 'ArtifactType'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactTypeId);
                tstuc_cplMainContent_tstReleaseMappingPanel.load_data(filters, loadNow);
            }
        }
    }

    //-- Diagram --
    if (typeof (tstucDiagramPanel) != 'undefined' && tstucDiagramPanel)
    {
        //See if we have any steps
        var ajxFormManager = $find(ajxFormManager_id);
        var hasSteps = false;
        var dataItem = ajxFormManager.get_dataItem();
        if (dataItem != null && dataItem.alternate)
        {
            hasSteps = true;
        }

        //If we have SpiraTest or the requirement has no steps, hide this panel
        if (SpiraContext.ProductType == 'SpiraTest' || !hasSteps) {
            var tabControl = $find(tabControl_id);
            tabControl.get_tabPage('tabDiagram').hide();
            //Make sure it's not the selected panel, if so switch to overview tab
            if (tabControl.get_selectedTabClientId() == pnlDiagram_id)
            {
                tabControl.set_selectedTabClientId(pnlOverview_id);
            }
        }
        else {
            $find(tabControl_id).get_tabPage('tabDiagram').show();
            var artifactChanged = false;
            if (tstucDiagramPanel.get_artifactId() != SpiraContext.ArtifactId) {
                artifactChanged = true;
            }
            tstucDiagramPanel.set_artifactId(SpiraContext.ArtifactId);

            //Check to see if we have data
            //Currently not supported for this tab
            //tstucTaskListPanel.check_hasData(tasks_callback);

            if (artifactChanged) {
                //See if the tab is visible
                var loadNow = ($find(tabControl_id).get_selectedTabClientId() == pnlDiagram_id);

                //Reload the tab's data
                tstucDiagramPanel.load_data(null, loadNow);
            }
        }
    }

    //-- Tasks --
    if (typeof (tstucTaskListPanel) != 'undefined' && tstucTaskListPanel)
    {
        if (SpiraContext.Mode == 'new')
        {
            //Hide the tasks tab
            $find(tabControl_id).get_tabPage('tabTasks').hide();
        }
        if (SpiraContext.Mode == 'update')
        {
            //Show the tasks tab
            $find(tabControl_id).get_tabPage('tabTasks').show();

            var artifactChanged = false;
            if (tstucTaskListPanel.get_artifactId() != SpiraContext.ArtifactId)
            {
                artifactChanged = true;
            }
            tstucTaskListPanel.set_artifactId(SpiraContext.ArtifactId);
            tstucTaskListPanel.set_artifactTypeId(SpiraContext.ArtifactTypeId);

            //Check to see if we have data
            tstucTaskListPanel.check_hasData(tasks_callback);

            if (artifactChanged)
            {
                //See if the tab is visible
                var loadNow = ($find(tabControl_id).get_selectedTabClientId() == pnlTasks_id);

                //Reload the tab's data
                var filters = {};
                //Requirement
                if (SpiraContext.ArtifactTypeId == globalFunctions.artifactTypeEnum.requirement)
                {
                    filters[globalFunctions.keyPrefix + 'RequirementId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                    tstucTaskListPanel.load_data(filters, loadNow);
                }
                //Risk
                if (SpiraContext.ArtifactTypeId == globalFunctions.artifactTypeEnum.risk) {
                    filters[globalFunctions.keyPrefix + 'RiskId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                    tstucTaskListPanel.load_data(filters, loadNow);
                }
            }
        }
    }

    //-- Requirement-Tasks --
    if (typeof (tstucRequirementTaskPanel) != 'undefined' && tstucRequirementTaskPanel)
    {
        if (SpiraContext.Mode == 'update')
        {
            var artifactChanged = false;
            if (tstucRequirementTaskPanel.get_artifactId() != SpiraContext.ArtifactId)
            {
                artifactChanged = true;
            }
            tstucRequirementTaskPanel.set_artifactId(SpiraContext.ArtifactId);
            tstucRequirementTaskPanel.set_artifactTypeId(SpiraContext.ArtifactTypeId);

            //Check to see if we have data
            tstucRequirementTaskPanel.check_hasData(requirementTasks_callback);

            if (artifactChanged)
            {
                //See if the tab is visible
                var loadNow = ($find(tabControl_id).get_selectedTabClientId() == pnlReqsTasks_id);

                //Reload the tab's data
                var filters = {};
                //Release
                if (SpiraContext.ArtifactTypeId == globalFunctions.artifactTypeEnum.release)
                {
                    filters[globalFunctions.keyPrefix + 'ReleaseId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                    tstucRequirementTaskPanel.load_data(filters, loadNow);
                }
            }
        }
    }

    //-- Associations --
    if (typeof tstuc_cplMainContent_tstAssociationPanel != 'undefined' && tstuc_cplMainContent_tstAssociationPanel)
    {
        if (SpiraContext.Mode == 'update')
        {
            var artifactChanged = false;
            if (tstuc_cplMainContent_tstAssociationPanel.get_artifactId() != SpiraContext.ArtifactId)
            {
                artifactChanged = true;
            }
            tstuc_cplMainContent_tstAssociationPanel.set_artifactId(SpiraContext.ArtifactId);
            tstuc_cplMainContent_tstAssociationPanel.set_artifactTypeId(SpiraContext.ArtifactTypeId);

            //Check to see if we have data
            tstuc_cplMainContent_tstAssociationPanel.check_hasData(associations_callback);

            if (artifactChanged)
            {
                //close the add new assocation panel
                tstuc_cplMainContent_tstAssociationPanel.closeAddAssocationPanel();

                //See if the tab is visible
                //If we don't have a panel id, it means it is on a page without a tab control so should always display (eg build details page)
                var loadNow = typeof pnlAssociations_id == 'undefined' || ($find(tabControl_id).get_selectedTabClientId() == pnlAssociations_id);

                //Reload the tab's data
                var filters = {};
                filters[globalFunctions.keyPrefix + 'ArtifactId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                filters[globalFunctions.keyPrefix + 'ArtifactType'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactTypeId);
                tstuc_cplMainContent_tstAssociationPanel.load_data(filters, loadNow);
            }
        }
    }

    //-- Test Runs --
    if (typeof (tstucTestRunPanel) != 'undefined' && tstucTestRunPanel)
    {
        if (SpiraContext.Mode == 'update')
        {
            var artifactChanged = false;
            if (tstucTestRunPanel.get_artifactId() != SpiraContext.ArtifactId)
            {
                artifactChanged = true;
            }
            tstucTestRunPanel.set_artifactId(SpiraContext.ArtifactId);
            tstucTestRunPanel.set_artifactTypeId(SpiraContext.ArtifactTypeId);

            //Check to see if we have data
            tstucTestRunPanel.check_hasData(testRuns_callback);

            if (artifactChanged)
            {
                //See if the tab is visible
                //If we don't have a panel id, it means it is on a page without a tab control so should always display (eg build details page)
                var loadNow = typeof tabControl_id != 'undefined' ? ($find(tabControl_id).get_selectedTabClientId() == pnlTestRuns_id) : true;

                //Reload the tab's data
                var filters = {};
                //Test Case
                if (SpiraContext.ArtifactTypeId == globalFunctions.artifactTypeEnum.testCase)
                {
                    filters[globalFunctions.keyPrefix + 'TestCaseId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                    tstucTestRunPanel.load_data(filters, loadNow);
                }
                //Test Set
                if (SpiraContext.ArtifactTypeId == globalFunctions.artifactTypeEnum.testSet)
                {
                    filters[globalFunctions.keyPrefix + 'TestSetId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                    tstucTestRunPanel.load_data(filters, loadNow);
                }
                //Release
                if (SpiraContext.ArtifactTypeId == globalFunctions.artifactTypeEnum.release)
                {
                    filters[globalFunctions.keyPrefix + 'ReleaseId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                    tstucTestRunPanel.load_data(filters, loadNow);
                }
                //Automation Host
                if (SpiraContext.ArtifactTypeId == globalFunctions.artifactTypeEnum.automationHost)
                {
                    filters[globalFunctions.keyPrefix + 'AutomationHostId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                    tstucTestRunPanel.load_data(filters, loadNow);
                }
                //Build
                if (SpiraContext.ArtifactTypeId == globalFunctions.artifactTypeEnum.build)
                {
                    filters[globalFunctions.keyPrefix + 'BuildId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                    tstucTestRunPanel.load_data(filters, loadNow);
                }
            }
        }
    }

    //-- Incidents --
    if (typeof (tstucIncidentPanel) != 'undefined' && tstucIncidentPanel)
    {
        if (SpiraContext.Mode == 'update')
        {
            var artifactChanged = false;
            if (tstucIncidentPanel.get_artifactId() != SpiraContext.ArtifactId)
            {
                artifactChanged = true;
            }
            tstucIncidentPanel.set_artifactId(SpiraContext.ArtifactId);
            tstucIncidentPanel.set_artifactTypeId(SpiraContext.ArtifactTypeId);

            //Check to see if we have data
            tstucIncidentPanel.check_hasData(incidents_callback);

            if (artifactChanged)
            {
                //See if the tab is visible
                //If we don't have a panel id, it means it is on a page without a tab control so should always display (eg build details page)
                var loadNow = typeof tabControl_id != 'undefined' ? ($find(tabControl_id).get_selectedTabClientId() == pnlIncidents_id) : true;

                //Reload the tab's data
                var filters = {};
                //Test Case
                if (SpiraContext.ArtifactTypeId == globalFunctions.artifactTypeEnum.testCase)
                {
                    filters[globalFunctions.keyPrefix + 'TestCaseId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                    tstucIncidentPanel.load_data(filters, loadNow);
                }
                //Test Set
                if (SpiraContext.ArtifactTypeId == globalFunctions.artifactTypeEnum.testSet)
                {
                    filters[globalFunctions.keyPrefix + 'TestSetId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                    tstucIncidentPanel.load_data(filters, loadNow);
                }
                //Test Run
                if (SpiraContext.ArtifactTypeId == globalFunctions.artifactTypeEnum.testRun)
                {
                    filters[globalFunctions.keyPrefix + 'TestRunId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                    tstucIncidentPanel.load_data(filters, loadNow);
                }
                //Test Step
                if (SpiraContext.ArtifactTypeId == globalFunctions.artifactTypeEnum.testStep)
                {
                    filters[globalFunctions.keyPrefix + 'TestStepId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                    tstucIncidentPanel.load_data(filters, loadNow);
                }
                //Build
                if (SpiraContext.ArtifactTypeId == globalFunctions.artifactTypeEnum.build)
                {
                    filters[globalFunctions.keyPrefix + 'BuildId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                    tstucIncidentPanel.load_data(filters, loadNow);
                }
            }
        }
    }

    //-- Test Sets --
    if (typeof (tstucTestSetPanel) != 'undefined' && tstucTestSetPanel)
    {
        if (SpiraContext.Mode == 'update')
        {
            var artifactChanged = false;
            if (tstucTestSetPanel.get_artifactId() != SpiraContext.ArtifactId)
            {
                artifactChanged = true;
            }
            tstucTestSetPanel.set_artifactId(SpiraContext.ArtifactId);
            tstucTestSetPanel.set_artifactTypeId(SpiraContext.ArtifactTypeId);

            //Check to see if we have data
            tstucTestSetPanel.check_hasData(testSets_callback);

            if (artifactChanged)
            {
                //See if the tab is visible
                var loadNow = ($find(tabControl_id).get_selectedTabClientId() == pnlTestSets_id);

                //Reload the tab's data
                var filters = {};
                //Test Case
                if (SpiraContext.ArtifactTypeId == globalFunctions.artifactTypeEnum.testCase)
                {
                    filters[globalFunctions.keyPrefix + 'TestCaseId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                    tstucTestSetPanel.load_data(filters, loadNow);
                }
                //Test Configuration Set
                if (SpiraContext.ArtifactTypeId == globalFunctions.artifactTypeEnum.testConfigurationSet)
                {
                    filters[globalFunctions.keyPrefix + 'TestConfigurationSetId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                    tstucTestSetPanel.load_data(filters, loadNow);
                }
            }
        }
    }
}

//The different tab callbacks
function attachment_callback(hasData)
{
    typeof tabControl_id != "undefined" && $find(tabControl_id).updateHasData('tabAttachments', hasData);
}
function history_callback(hasData)
{
    typeof tabControl_id != "undefined" && $find(tabControl_id).updateHasData('tabHistory', hasData);
}
function associations_callback(hasData)
{
    typeof tabControl_id != "undefined" && $find(tabControl_id).updateHasData('tabAssociations', hasData);
}
function testRuns_callback(hasData)
{
    if (typeof tabControl_id != "undefined") {
        $find(tabControl_id).updateHasData('tabTestRuns', hasData);
    // if no tab control, look for a panel control - and hide the panel if no data (eg. build details page)
    } else if (typeof pnlTestRuns_id != "undefined") {
        if (!hasData) {
            document.getElementById(pnlTestRuns_id).classList.add("dn");
        } else {
        document.getElementById(pnlTestRuns_id).classList.remove("dn");
        }
    }
}
function testSets_callback(hasData)
{
    typeof tabControl_id != "undefined" && $find(tabControl_id).updateHasData('tabTestSets', hasData);
}
function incidents_callback(hasData)
{
    if (typeof tabControl_id != "undefined") {
        $find(tabControl_id).updateHasData('tabIncidents', hasData);
    // if no tab control, look for a panel control - and hide the panel if no data (eg. build details page)
    } else if (typeof pnlIncidents_id != "undefined") {
        if (!hasData) {
            document.getElementById(pnlIncidents_id).classList.add("dn");
        } else {
            document.getElementById(pnlIncidents_id).classList.remove("dn");
        }
    }
}
function tasks_callback(hasData)
{
    typeof tabControl_id != "undefined" && $find(tabControl_id).updateHasData('tabTasks', hasData);
}
function coverage_callback(hasData)
{
    typeof tabControl_id != "undefined" && $find(tabControl_id).updateHasData('tabCoverage', hasData);
}
function releases_callback(hasData)
{
    typeof tabControl_id != "undefined" && $find(tabControl_id).updateHasData('tabReleases', hasData);
}
function testCases_callback(hasData)
{
    typeof tabControl_id != "undefined" &&  $find(tabControl_id).updateHasData('tabTestCases', hasData);
}
function requirementTasks_callback(hasData)
{
    typeof tabControl_id != "undefined" && $find(tabControl_id).updateHasData('tabTasks', hasData);
}
function baseline_callback(hasData) {
    typeof tabControl_id != "undefined" && $find(tabControl_id).updateHasData('tabBaseline', hasData);
}
