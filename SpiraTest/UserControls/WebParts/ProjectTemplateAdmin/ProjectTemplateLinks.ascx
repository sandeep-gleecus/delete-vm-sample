<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProjectTemplateLinks.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectTemplateAdmin.ProjectTemplateLinks" %>
<div id="project-template-links-app"></div>
<script type="text/javascript">
    
    ReactDOM.render(React.createElement(AdminMenu, 
        { 
            currentLocation: null, 
            currentProjectId: SpiraContext.ProjectId, 
            currentProgramId: SpiraContext.ProjectGroupId, 
            currentTemplateId: SpiraContext.ProjectTemplateId,
            hideTitle: true,
            workspaceEnums: SpiraContext.WorkspaceEnums,
            workspaceMain: SpiraContext.Navigation.adminNavigation.template,
            workspaceMainType: 3, // template 
            wrapperId: "template-default-admin-nav", 
            sectionClasses: "", 
            sectionTitleClasses: "yolk mt0 mb3 bb b-yolk bw1 fs-h4", 
            sectionTitleLinkClasses: "yolk nav-text-hover transition-all tdn tdn-hover", 
            subSectionClasses: "u-box_group w-33 w-50-sm w-100-xs px4 pb4", 
            subSectionWrapperClasses: "df flex-wrap", 
            linkClasses: "transition-all tdn tdn-hover"
        }), document.getElementById('project-template-links-app')
        );

</script>
