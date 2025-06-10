//import * as React from 'react';
//import * as ReactDOM from 'react-dom';

/// <reference path="rct_comp_avatarIcon.tsx" />
/// <reference path="rct_comp_dropdown.tsx" />
/// <reference path="rct_comp_errorBoundary.tsx" />
/// <reference path="rct_comp_adminMenu.tsx" />

//external dependendencies (js libraries)
declare var React: any;
declare var ReactDOM: any;
declare var $: any;
declare var $find: any;
declare var Mousetrap: any;

//inflectra services
declare var globalFunctions: any;
declare var SpiraContext: any;
declare var Inflectra: any;
declare var pageCallbacks: any;





class GlobalNavigation extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);

        this.enum = {
            workspaceAdminClasses: "fa fa-cog mx2 fs-66 o-50 v-super",
            urlType: {
                workspace: "workspaceUrl",
                artifact: "artifactUrl"
            },
            searchStorage: "searchHistory",
            searchHistoryMax: 10,
        }

        this.supportsCssVars = typeof CSS != "undefined" && CSS.supports('color', 'var(--fake-var)');

        this.state = {
            workspaceDefaultText: resx.Global_ChooseWorkspace,
            workspaceList: [],
            workspaceListIsOpen: false,
            workspaceHidden: [],
            workspaceOnDashboard: this.workspaceOnDashboard(SpiraContext.Navigation.currentLocation),
            workspaceCurrent: "",
            workspaceCurrentId: "",
            workspaceLoading: false,

            artifactList: [],
            artifactListIsOpen: false,
            artifactHidden: [],
            artifactParent: {},

            userIcon: {},
            userList: [],
            userColorScheme: "auto",

            myPageLoading: false,

            reportsButton: null,
            reportsLoading: false,

            searchHistory: [],
            searchTerm: "",
            searchIsOpen: false,

            adminMenuShow: false,
            adminMenuPosition: "global-nav",

            spinnerShow: 0, // show only when int is greater than 0
            currentLocation: SpiraContext.Navigation.currentLocation,

            navMobileIsOpen: false,
        }

        this.workspaceDrop = React.createRef();
        this.artifactDrop = React.createRef();
        this.userDrop = React.createRef();

        // register and bind functions
        this.workspaceCreateList = this.workspaceCreateList.bind(this);
        this.workspaceSetExpanded = this.workspaceSetExpanded.bind(this);
        this.artifactListCallback = this.artifactListCallback.bind(this);
        this.artifactCreateTree = this.artifactCreateTree.bind(this);
        this.artifactsSetExpanded = this.artifactsSetExpanded.bind(this);
        this.userIcon = this.userIcon.bind(this);
        this.userCreateList = this.userCreateList.bind(this);
        this.userSetColorScheme = this.userSetColorScheme.bind(this);
        this.userSetColorSchemeAuto = this.userSetColorSchemeAuto.bind(this);
        this.userToggleColorScheme = this.userToggleColorScheme.bind(this);
        this.expandingToggle = this.expandingToggle.bind(this);
        this.setListInfo = this.setListInfo.bind(this);
        this.handleOpenLink = this.handleOpenLink.bind(this);

        // admin menu
        this.adminToggleMenu = this.adminToggleMenu.bind(this);
        this.adminMenuOnBlur = this.adminMenuOnBlur.bind(this);
        this.adminMenuClose = this.adminMenuClose.bind(this);

        // for handling search
        this.searchInput = React.createRef();
        this.searchToggleOpen = this.searchToggleOpen.bind(this);
        this.searchInit = this.searchInit.bind(this);
        this.searchKeyPress = this.searchKeyPress.bind(this);
        this.searchTermUpdate = this.searchTermUpdate.bind(this);

        // managing nav in general / on mobile
        this.wholeNavMobileToggle = this.wholeNavMobileToggle.bind(this);
        this.updateDropStatus = this.updateDropStatus.bind(this);

        // keyboard shortcut list of terms
        // code taken from kronodesk 3.0 which has fully react and auto generated documentation. Here, we only use part of this setup but I tried to keep it compatible in case we want to migrate to that model in the future
        this.kbCodes = {
            codes: {
                showGlossary: { term: "?" },
                [globalFunctions.artifactTypeEnum.requirement]: { term: "n r q" },
                [globalFunctions.artifactTypeEnum.planningBoard]: { term: "n p b" },
                [globalFunctions.artifactTypeEnum.programPlanningBoard]: { term: "n p b" },
                [globalFunctions.artifactTypeEnum.release]: { term: "n r l" },
                [globalFunctions.artifactTypeEnum.programReleases]: { term: "n r l" },
                [globalFunctions.artifactTypeEnum.document]: { term: "n d c" },
                [globalFunctions.artifactTypeEnum.testCase]: { term: "n t c" },
                [globalFunctions.artifactTypeEnum.testSet]: { term: "n t s" },
                [globalFunctions.artifactTypeEnum.testRun]: { term: "n t r" },
                [globalFunctions.artifactTypeEnum.automationHost]: { term: "n a h" },
                [globalFunctions.artifactTypeEnum.testConfigurationSet]: { term: "n t n" },
                [globalFunctions.artifactTypeEnum.incident]: { term: "n i n" },
                [globalFunctions.artifactTypeEnum.programIncidents]: { term: "n i n" },
                [globalFunctions.artifactTypeEnum.task]: { term: "n t k" },
                [globalFunctions.artifactTypeEnum.risk]: { term: "n r k" },
                [globalFunctions.artifactTypeEnum.resource]: { term: "n u s" },
                [globalFunctions.artifactTypeEnum.sourceCode]: { term: "n s c" },
                [globalFunctions.artifactTypeEnum.sourceCodeRevisions]: { term: "n c o" },
                [globalFunctions.artifactTypeEnum.pullRequests]: { term: "n p r" },
            }
        }
    }










    componentWillMount() {
        this.workspaceSet();
        this.workspaceCreateList(this.workspaceSetExpanded);
        this.artifactCreateTree(this.artifactListCallback);
        this.setState({ userIcon: this.userIcon() });
        this.userCreateList();
        this.userColorSchemeGetLocal();
        this.searchGetLocalTerms();

        this.onboardingShow(true);
        this.onboardingRegisterPage();
    }










    // create the list of workspace for the dropdown by combining programs and projects
    // param: option callback function that runs after state has been set
    // return: update state
    workspaceCreateList(callback?) {
        const { portfolios, programs, programIdsByOwner, projects, projectIdsByOwner, templates } = this.props.data;

        let list = [];
        let programsToUse = programs; // we create this because we may have to edit the array below
        let portfoliosToUse = portfolios && portfolios.length ? portfolios : []; // portfolios array may be missing so make sure it is here

        // access to the entprise menu/page(s) is limited to those using the right version of Spira (where portfolios are enabled) and where the user has the required specific permission
        if (this.props.isPortfolioEnabled && this.props.isEnterpriseViewer) {
            const enterpriseMenuItem = this.workspaceCreateEnterpriseItem(SpiraContext.IsSystemAdmin);
            list.push(enterpriseMenuItem);
        }


        var portfolioItems = null;
        // create the portfolio workspace items if we are in SpiraPlan
        if (this.props.isPortfolioEnabled) {
            // check first if there are any programs that don't have a portfolio - orphans
            const orphanPrograms = programs.filter(x => !x.portfolioId).map(x => x.id);
            //if we have orphan programs...
            if (orphanPrograms && orphanPrograms.length) {
                // create a fake portfolio for them and...
                portfoliosToUse.push({
                    id: -1,
                    name: resx.GlobalNavigation_DefaultPortfolio,
                    programIds: orphanPrograms
                });
                // update the program object of any orphans so that their portfolioId matches that of the fake portfolio
                programsToUse = programs.map(x => {
                    if (orphanPrograms.indexOf(x.id) >= 0) {
                        x.portfolioId = -1;
                    }
                    return x;
                });
            }
            portfolioItems = portfoliosToUse
                .sort((a, b) => a.name.toLowerCase() > b.name.toLowerCase() ? 1 : -1)
                .map(pf => {
                    // we have to add all the children - for programs and projects to make the expand/collapse work properly
                    const programsInPortfolio = programsToUse.filter(x => x.portfolioId === pf.id);
                    const programIdsInPortfolio = programsInPortfolio.map(x => x.id);

                    const projectListIdsInPortfolio = projects
                        .filter(x => programIdsInPortfolio.indexOf(x.programId) >= 0)
                        .map(x => this.props.workspaceEnums.product + "-" + x.id)
                    const programListIdsInPortfolio = programsInPortfolio.map(x => this.props.workspaceEnums.program + "-" + x.id);

                    const children = projectListIdsInPortfolio.concat(...programListIdsInPortfolio);
                    return this.workspaceCreatePortfolioItem(pf, children)
                });
        }
        // create the program workspace items
        const programItems = programsToUse
            .sort((a, b) => a.name.toLowerCase() > b.name.toLowerCase() ? 1 : -1)
            .map(pg => {
                const children = projects
                    .filter(x => x.programId === pg.id)
                    .map(x => this.props.workspaceEnums.product + "-" + x.id)
                return this.workspaceCreateProgramItem(pg, programIdsByOwner, children, this.props.isPortfolioEnabled)
            });

        // create the project workspace items
        const projectItems = projects
            .sort((a, b) => a.name.toLowerCase() > b.name.toLowerCase() ? 1 : -1)
            .map(p => this.workspaceCreateProjectItem(p, projectIdsByOwner, this.props.isPortfolioEnabled));

        if (this.props.isPortfolioEnabled) {
            // add the portfolios, programs, and projects in order to the main workspace list
            for (var i = 0; i < portfolioItems.length; i++) {
                const pf = portfolioItems[i];
                list.push(pf);
                // get relevant project items for this program
                const programsInPortfolio = programItems.filter(pg => pf.children && pf.children.indexOf(pg.listId) >= 0);
                for (var j = 0; j < programsInPortfolio.length; j++) {
                    const pg = programsInPortfolio[j];
                    list.push(pg);
                    // get relevant project items for this program
                    const children = projectItems.filter(p => pg.children && pg.children.indexOf(p.listId) >= 0);
                    list = list.concat(...children);
                }
            }
        } else {
            // add the programs and projects in order to the main workspace list
            for (var i = 0; i < programItems.length; i++) {
                const pg = programItems[i];
                // get relevant project items for this program
                const children = projectItems.filter(p => pg.children && pg.children.indexOf(p.listId) >= 0);
                list.push(pg);
                list = list.concat(...children);
            }
        }

        // add templates if user had admin access to any templates
        if (templates && templates.length) {
            // first add a top level template menu item to list the templates inside
            list.push(this.workspaceCreateTemplateWrapper(templates));

            // create template object for each template
            const templatesItems = templates
                .sort((a, b) => a.name.toLowerCase() > b.name.toLowerCase() ? 1 : -1)
                .map(t => this.workspaceCreateTemplateItem(t));
            // add the templates to the workspace array
            list = list.concat(...templatesItems);
        }

        // update state
        this.setState({ workspaceList: list },
            () => callback && callback()
        );
    }

    // wrapper function that runs expanding toggle for workspaces - used to easily add as callback on creation of workspace list
    workspaceSetExpanded() {
        this.expandingToggle(
            JSON.parse(localStorage.getItem("workspaceHidden")),
            true,
            "workspaceList",
            "workspaceHidden"
        )
    }

    // determine if we should navigate to the dashboard page of the current workspace (if not we navigate to the artifact / other page)
    // param: location int enum
    // return: true if the workspace navigation should go to the dashboard of that workspace
    workspaceOnDashboard(location) {
        return location === globalFunctions.artifactTypeEnum.projectHome || location === globalFunctions.artifactTypeEnum.programHome;
    }

    // set the initial workspace code (workspace type code - workspace id)
    // if there is no current project or group, it means this is a users first log on so don't try and set state
    workspaceSet() {
        let prefix = this.props.workspaceType;
        let suffix = null;

        switch (this.props.workspaceType) {
            case this.props.workspaceEnums.program:
                suffix = SpiraContext.ProjectGroupId;
                break;
            case this.props.workspaceEnums.product:
                suffix = SpiraContext.ProjectId;
                break;
            case this.props.workspaceEnums.template:
                suffix = SpiraContext.ProjectTemplateId;
                break;
            case this.props.workspaceEnums.portfolio:
                suffix = SpiraContext.PortfolioId;
                break;
            case this.props.workspaceEnums.enterprise:
            default:
                suffix = 1;

                // there is a special case of 6.5 for system admin: the workspace enum is the same for enterprise and system admin
                // when a system admin is on a system admin page AND they can view the enterprise pages, the workspace dropdown will tell them they are on the "enterprise" view. 
                // this could be misleading
                if (this.props.data.currentLocation == globalFunctions.artifactTypeEnum.administration) {
                    prefix = this.props.data.currentLocation;
                    //set the default dropdown text to show they are in system admin
                    this.setState({ workspaceDefaultText: resx.Global_SystemAdministration })
                }
                break;
        }
        if (prefix && suffix) {
            this.setState({
                workspaceCurrent: `${prefix}-${suffix}`,
                workspaceCurrentId: suffix
            });
        }
    }

    // if we can return the new specific workspace url, otherwise return nothing
    workspaceUrl() {
        if (this.state.workspaceCurrent && this.state.workspaceList.length) {
            const filteredWorkspaces = this.state.workspaceList.filter(x => x.listId == this.state.workspaceCurrent);
            if (filteredWorkspaces.length) {
                return filteredWorkspaces[0].workspaceUrl;
            } else {
                return null;
            }
        } else {
            return null;
        }
    }

    // returns the name of the image to use for each specific workspace
    workspaceIconName(workspaceEnum) {
        if (workspaceEnum) {
            switch (workspaceEnum) {
                case this.props.workspaceEnums.enterprise:
                    return "org-Enterprise.svg";
                case this.props.workspaceEnums.portfolio:
                    return "org-Portfolio.svg";
                case this.props.workspaceEnums.template:
                    return "org-Template-outline.svg";
                case this.props.workspaceEnums.program:
                    return "org-Program-outline.svg";
                case this.props.workspaceEnums.product:
                default:
                    return "org-Project-outline.svg";
            }
        }
    }

    // returns the object for the system item that gets added at the top of the workspace menu
    // param: bool of true if user is a system admin
	workspaceCreateEnterpriseItem(isAdmin) {
        return {
            ariaLabel: "Global Navigation Enterprise View",
            domIdPrefix: "globalNav_workspaceDropdown_enterprise_", //combined with the listId field on render
            imageClasses: "w5 h5 pr2",
            imageUrl: SpiraContext.BaseThemeUrl + "Images/org-Enterprise.svg",
            isEnabled: true,
            linkClasses: "nav-drop-menu-item fs-110",
            listId: this.props.workspaceEnums.enterprise + "-1",
            name: resx.Global_Enterprise,
            secondaryGlyph: isAdmin ? this.enum.workspaceAdminClasses : null,
            title: resx.GlobalNavigation_TooltipEnterprise,
            type: this.props.workspaceEnums.enterprise,
            url: globalFunctions.replaceBaseUrl("~/Enterprise/Default.aspx"),
            workspaceUrl: this.props.data.adminUrl
        }
    }

    // returns the object for the system item that gets added at the top of the workspace menu
    // param: bool of true if user is a system admin
    workspaceCreatePortfolioItem(pf, children) {
        const isPortfolioOwner = this.props.isPortfolioViewer || false;
        const isOnAdminPage = this.state.currentLocation == globalFunctions.artifactTypeEnum.administration;
        const urlToUse = this.state.workspaceOnDashboard || (isOnAdminPage && !isPortfolioOwner) ? this.enum.urlType.workspace : this.enum.urlType.artifact;
        return {

            ariaLabel: "PF:" + pf.id + " - " + pf.name,
            children: children && children.length ? children : null,
            hideChildren: false,
            id: pf.id,
            imageClasses: "w5 h5 pr2",
            imageUrl: SpiraContext.BaseThemeUrl + "Images/org-Portfolio.svg",
            isEnabled: isPortfolioOwner,
            linkClasses: "nav-drop-menu-item fs-110",
            listId: this.props.workspaceEnums.portfolio + "-" + pf.id,
            domIdPrefix: "globalNav_workspaceDropdown_portfolio_", //combined with the listId field on render
            name: pf.name,
            title: (pf.description ? "(" : "") + "PF:" + pf.id + (isPortfolioOwner ? " - " + resx.Global_Owner : "") + (pf.description ? ") " + pf.description : ""),
            type: this.props.workspaceEnums.portfolio,
            url: globalFunctions.replaceBaseUrl(pf[urlToUse]),
            workspaceUrl: globalFunctions.replaceBaseUrl(pf.workspaceUrl)
        }
    }


    // returns an amended workspace object for passed in program
    // param: pg is the original program object
    // param: array of ints programIdsByOwner is all programs the user is an owner of
    // param: urlToUse string based on what part of the system we are in
    // param: children array of child objects attached to the program (ie projects)
    // return: updated program object
    workspaceCreateProgramItem(pg, programIdsByOwner, children, isPortfolioEnabled) {
        const isProgramOwner = programIdsByOwner && programIdsByOwner.length ? globalFunctions.findItemInArray(programIdsByOwner, pg.id) : false;
        const isOnAdminPage = this.state.currentLocation == globalFunctions.artifactTypeEnum.administration;
        const urlToUse = this.state.workspaceOnDashboard || (isOnAdminPage && !isProgramOwner) ? this.enum.urlType.workspace : this.enum.urlType.artifact;
        return {
            ariaLabel: "PG:" + pg.id + " - " + pg.name,
            children: children && children.length ? children : null,
            hideChildren: false,
            id: pg.id,
            imageClasses: "w5 h5 pr2",
            imageUrl: SpiraContext.BaseThemeUrl + "Images/org-Program-outline.svg",
            indentLevel: isPortfolioEnabled ? 1 : 0,
            isEnabled: pg.isEnabled,
            linkClasses: "nav-drop-menu-item fs-110",
            listId: this.props.workspaceEnums.program + "-" + pg.id,
            domIdPrefix: "globalNav_workspaceDropdown_program_", //combined with the listId field on render
            name: pg.name,
            secondaryGlyph: isProgramOwner ? this.enum.workspaceAdminClasses : null,
            title: "PG:" + pg.id + (isProgramOwner ? " - " + resx.Global_Owner : ""),
            type: this.props.workspaceEnums.program,
            url: globalFunctions.replaceBaseUrl(pg[urlToUse]),
            workspaceUrl: globalFunctions.replaceBaseUrl(pg.workspaceUrl),
        }
    }


    // returns an amended workspace object for passed in project
    // param: p is the original project object
    // param: array of ints projectIdsByOwner is all projects the user is an owner of
    // return: updated project object
    workspaceCreateProjectItem(p, projectIdsByOwner, isPortfolioEnabled) {
        const isProjectOwner = projectIdsByOwner && projectIdsByOwner.length ? globalFunctions.findItemInArray(projectIdsByOwner, p.id) : false;
        const isOnAdminPage = this.state.currentLocation == globalFunctions.artifactTypeEnum.administration;
        const urlToUse = this.state.workspaceOnDashboard || (isOnAdminPage && !isProjectOwner) ? this.enum.urlType.workspace : this.enum.urlType.artifact;
        return {
            ariaLabel: "PR:" + p.id + " - " + p.name,
            id: p.id,
            imageClasses: "w4 h4 pr2",
            imageUrl: SpiraContext.BaseThemeUrl + "Images/org-Project-outline.svg",
            indentLevel: isPortfolioEnabled ? 2 : 1,
            isEnabled: p.isEnabled,
            linkClasses: "nav-drop-menu-item",
            listId: this.props.workspaceEnums.product + "-" + p.id,
            domIdPrefix: "globalNav_workspaceDropdown_product_", //combined with the listId field on render
            name: p.name,
            secondaryGlyph: isProjectOwner ? this.enum.workspaceAdminClasses : null,
            title: (p.description ? "(" : "") + "PR:" + p.id + (isProjectOwner ? " - " + resx.Global_Owner : "") + (p.description ? ") " + p.description : ""),
            type: this.props.workspaceEnums.product,
            url: globalFunctions.replaceBaseUrl(p[urlToUse]),
            workspaceUrl: globalFunctions.replaceBaseUrl(p.workspaceUrl),
        }
    }


    // returns the parent item for the templates to be children of
    // param: array of templates to derive their ids from (for collapsing)
    workspaceCreateTemplateWrapper(templates) {
        return {
            children: templates.map(x => this.props.workspaceEnums.template + "-" + x.id),
            imageClasses: "w5 h5 pr2",
            imageUrl: SpiraContext.BaseThemeUrl + "Images/org-Template-outline.svg",
            isEnabled: true,
            linkClasses: "nav-drop-menu-item fs-110",
            listId: this.props.workspaceEnums.template + "-" + 0,
            domIdPrefix: "globalNav_workspaceDropdown_template_", //combined with the listId field on render
            hideChildren: false,
            name: resx.Global_Templates,
            type: this.props.workspaceEnums.template,
        };
    }

    // returns an amended workspace object for passed in template
    // param: tp is the original template object
    // return: updated program object
    workspaceCreateTemplateItem(pt) {
        return {
            ariaLabel: "PT:" + pt.id + " - " + pt.name,
            id: pt.id,
            indentLevel: 1,
            imageClasses: "w4 h4 pr2",
            imageUrl: SpiraContext.BaseThemeUrl + "Images/org-Template-outline.svg",
            isEnabled: true,
            linkClasses: "nav-drop-menu-item",
            listId: this.props.workspaceEnums.template + "-" + pt.id,
            domIdPrefix: "globalNav_workspaceDropdown_template_", //combined with the listId field on render
            name: pt.name,
            secondaryGlyph: this.enum.workspaceAdminClasses,
            title: (pt.description ? "(" : "") + "PT:" + pt.id + (pt.description ? ") " + pt.description : ""),
            type: this.props.workspaceEnums.template,
            url: SpiraContext.BaseUrl + "pt/" + pt.id + "/Administration/Default.aspx",
            workspaceUrl: SpiraContext.BaseUrl + "pt/" + pt.id + "/Administration/Default.aspx",
        }
    }

    workspaceTemplateControlsProduct() {
        if (this.props.workspaceType == this.props.workspaceEnums.template) {
            const currentTemplate = this.props.data.templates.filter(x => x.id == SpiraContext.ProjectTemplateId);
            if (currentTemplate && currentTemplate.length) {
                const templateProductMatch = currentTemplate[0].projects && currentTemplate[0].projects.length ? currentTemplate[0].projects.filter(x => x.id == SpiraContext.ProjectId) : false;
                return templateProductMatch && templateProductMatch.length ? true : false;
            } else {
                return true;
            }
        } else {
            return true;
        }
    }

    // creates a hierarchical list of artifacts available for the current workspace / user
    // return: updates state
    artifactCreateTree(callback?) {

        const nodes = JSON.parse(JSON.stringify(SpiraContext.Navigation.nodeTree.children)),
            artifactTypeCode = this.props.workspaceType == this.props.workspaceEnums.program ? globalFunctions.artifactTypeEnum.programHome : globalFunctions.artifactTypeEnum.projectHome;

        let artifactParentNode = nodes.filter(x => x.id == artifactTypeCode)[0];
        let artifactList = [];

        if (artifactParentNode && artifactParentNode.children && artifactParentNode.children.length) {
            for (var i = 0; i < artifactParentNode.children.length; i++) {
                const art = artifactParentNode.children[i];
                // skip over items that should not be in the artifact dropdown
                if (art.id == globalFunctions.artifactTypeEnum.reports) {
                    const button = {
                        id: art.id,
                        url: this.replaceWorkplaceTokenUrl(art.url),
                        name: art.name
                    };
                    this.setState({ reportsButton: button });
                    // each artifact is part of a group so the first item in the group should always be a parent
                    // if we have a parent create it and its children
                } else if (art.children && art.children.length) {
                    const children = art.children.map(child => this.artifactCreateItem(child, true, true, false));
                    const parent = this.artifactCreateItem(art, false, false, true);
                    if (children.length) {
                        parent.children = children.map(x => x.listId);
                        parent.hideChildren = false;
                    }
                    artifactList.push(parent);
                    artifactList.push(...children);
                    // otherwise we have an orphan artifact
                    // if it has an id then it is an artifact and we show it
                    // if it does not have an id then it is a parent with no children (that the user can see) so we hide it
                } else if (art.id) {
                    artifactList.push(this.artifactCreateItem(art, true, false, false));
                }
            };

            artifactParentNode.children = artifactList;

            this.setState({
                artifactParent: artifactParentNode,
                artifactList: artifactList
            },
                () => callback && callback()
            );
        }
    }

    // creates a single artifact item for use in the artifact dropdown
    // param: art object for a specific artifact (from site map)
    // param: hasIcon bool for whether there is a specific icon to show for this artifact
    // param: isIndented bool - if it is not a root item then it needs to be indented
    artifactCreateItem(art, hasIcon, isIndented, isParent) {
        const artifactObject = art.id ? globalFunctions.getArtifactTypes(art.id)[0] : null;
        let item = {
            ariaLabel: art.name,
            children: null,
            hideChildren: false,
            imageUrl: hasIcon && artifactObject ? SpiraContext.BaseThemeUrl + artifactObject.image : null,
            imageClasses: "w4 h4 mr3",
            linkClasses: "nav-drop-menu-item" + (isParent ? " fs-125" : ""),
            id: art.id,
            name: art.name,
            url: isParent ? null : this.replaceWorkplaceTokenUrl(art.url),
            listId: isParent ? art.name : art.id,
            domIdPrefix: "globalNav_artifactDropdown_", //combined with the listId field on render
            isEnabled: true,
            title: artifactObject && resx["GlobalNavigation_Tooltip" + artifactObject.image.replace("Images/artifact-", "").replace(".svg", "")]
        }
        return item;
    }

    // wrapper function that runs expanding toggle for artifacts - used to easily add as callback on creation of workspace list
    artifactsSetExpanded() {
        this.expandingToggle(
            JSON.parse(localStorage.getItem("artifactHidden")),
            true,
            "artifactList",
            "artifactHidden"
        )
    }

    // handler function for running after the artifact list has been created
    artifactListCallback() {
        this.artifactsSetExpanded();
        this.keyboardShortcutsInit();
    }

    // checks to see if the artifact list contains a specific artifact type
    // param: int of the artifact enum to search for
    // return: bool of true if present, false if not present
    artifactListGetItem(artifactId) {
        let match = false;
        if (typeof artifactId != "undefined" || artifactId !== 0) {
            match = this.state.artifactList.map(x => x.id).indexOf(artifactId) >= 0;
        }
        return match;
    }

    // checks whether to show the artifacts menu or not
    artifactShowDropdown() {
        const hasArtifacts = this.state.artifactList && this.state.artifactList.length,
            notEnterpriseView = this.props.workspaceType != this.props.workspaceEnums.enterprise,
            notPortfolioView = this.props.workspaceType != this.props.workspaceEnums.portfolio,
            productMatchesTemplate = this.workspaceTemplateControlsProduct();
        
        return hasArtifacts && notEnterpriseView && notPortfolioView && productMatchesTemplate;
    }

    // creates a user object for the icon component to use
    userIcon() {
        const userObj = SpiraContext.Navigation.user;
        return {
            name: userObj.fullName,
            nameAsIcon: userObj.firstName && userObj.lastName ? userObj.firstName[0].toUpperCase() + userObj.lastName[0].toUpperCase() : "?",
            hasIcon: userObj.hasIcon || false,
            avatarUrl: userObj.hasIcon ? userObj.avatarUrl : null
        }
    }

    // create the list of items to display in the user dropdown - each of these items is created bespoke inside this function
    // param: option callback function that runs after state has been set
    // return: update state
    userCreateList() {
        const icon = this.userIcon();
        const userProfileName = <div className="df">
            <AvatarIcon
                hasIcon={icon.hasIcon}
                icon={icon.avatarUrl}
                iconSize={6}
                name={icon.name}
                nameAsIcon={icon.nameAsIcon}
                wrapperClasses="lh-initial mr3"
            />
            <div>
                <p className="ma0 pa0 nowrap fs-125">{SpiraContext.Navigation.user.fullName}</p>
                <p className="ma0 pa0"
                    title={SpiraContext.IsSystemAdmin ? "" : SpiraContext.ProjectRole.Description}
                    >
                    {SpiraContext.IsSystemAdmin ? "Product Owner" : SpiraContext.ProjectRole.Name}
                </p>
            </div>
        </div>;

        const userProfile = {
            ariaLabel: "Global Navigation My Profile",
            domIdPrefix: "globalNav_userDropdown_profile_", //combined with the listId field on render
            isEnabled: true,
            linkClasses: "nav-drop-menu-item",
            listId: globalFunctions.artifactTypeEnum.myProfile.toString(), // listIds are expected to be strings so the dropdown can work properly
            name: userProfileName,
            showTitleInButton: true,
            title: resx.GlobalNavigation_MyProfile,
            url: globalFunctions.replaceBaseUrl(SpiraContext.Navigation.myProfileUrl)
        };
        const timecard = SpiraContext.Navigation.myTimecardUrl && {
            ariaLabel: "Global Navigation My Timecard",
            domIdPrefix: "globalNav_userDropdown_timecard_", //combined with the listId field on render
            glyph: "fas fa-stopwatch pr3",
            isEnabled: true,
            name: resx.GlobalNavigation_MyTimecard,
            linkClasses: "nav-drop-menu-item",
            listId: globalFunctions.artifactTypeEnum.myTimecard.toString(), // listIds are expected to be strings so the dropdown can work properly
            title: resx.GlobalNavigation_MyTimecard,
            url: globalFunctions.replaceBaseUrl(SpiraContext.Navigation.myTimecardUrl)
        };
        const help = {
            ariaLabel: "Global Navigation Documentation",
            domIdPrefix: "globalNav_userDropdown_help_", //combined with the listId field on render
            glyph: "fas fa-book-open pr3",
            isEnabled: true,
            name: resx.Global_Documentation,
            openPlainHtml: true,
            linkClasses: "nav-drop-menu-item",
            listId: "help", // text listId used here as this does not have an enum specified
            dontShowLoader: true,
            url: (SpiraContext.Navigation.helpUrl ? SpiraContext.Navigation.helpUrl : null),
            target: "_blank"
        };
        const keyboard = {
            ariaLabel: "Global Navigation Open Keyboard Shortcuts Help",
            domIdPrefix: "globalNav_userDropdown_shortcuts_", //combined with the listId field on render
            glyph: "far fa-keyboard pr3",
            isEnabled: true,
            name: resx.GlobalNavigation_KeyboardShortcuts,
            linkClasses: "nav-drop-menu-item",
            listId: "keyboard", // text listId used here as this does not have an enum specified
            dontShowLoader: true,
            url: "javascript:ucGlobalNavigation_displayShortcuts()"
        };
        const onboarding = {
            ariaLabel: "Global Navigation Show Onboarding Tours",
            domIdPrefix: "globalNav_userDropdown_tours_", //combined with the listId field on render
            dontShowLoader: true,
            glyph: "far fa-eye pr3",
            isEnabled: true,
            linkClasses: "nav-drop-menu-item",
            listId: "onboarding", // text listId used here as this does not have an enum specified
            name: resx.GlobalNavigation_ShowOnboardingTours,
            url: "javascript:window.rct_comp_globalNav.onboardingShow(false)"
        };
        const colorScheme = {
            ariaLabel: "Global Navigation Change Color Scheme",
            domIdPrefix: "globalNav_userDropdown_colorsSCheme_", //combined with the listId field on render
            dontShowLoader: true,
            glyph: this.state.userColorScheme == "dark" ? "fas fa-moon pr3" : this.state.userColorScheme == "light" ? "fas fa-sun pr3" : "fas fa-magic pr3",
            id: "default",
            isEnabled: true,
            linkClasses: "nav-drop-menu-item",
            listId: "default", // text listId used here as this does not have an enum specified
            name: this.state.userColorScheme == "dark" ? resx.GlobalNavigation_ColorScheme_Dark : this.state.userColorScheme == "light" ? resx.GlobalNavigation_ColorScheme_Light : resx.GlobalNavigation_ColorScheme_Auto,
            title: this.state.userColorScheme == "dark" ? resx.GlobalNavigation_ColorScheme_DarkTitle : this.state.userColorScheme == "light" ? resx.GlobalNavigation_ColorScheme_LightTitle : resx.GlobalNavigation_ColorScheme_AutoTitle,
            url: "javascript:window.rct_comp_globalNav.userToggleColorScheme()"
        };
        const logout = {
            ariaLabel: "Global Navigation Logout",
            domIdPrefix: "globalNav_userDropdown_", //combined with the listId field on render
            glyph: "fas fa-sign-out-alt pr3",
            isEnabled: true,
            linkClasses: "nav-drop-menu-item",
            listId: "logout", // text listId used here as this does not have an enum specified
            name: resx.AjaxFormManager_Logout,
            url: globalFunctions.replaceBaseUrl("~/Logout.aspx")
        };

        let list = [];
        list.push(userProfile);
        if (timecard) list.push(timecard);
        if (SpiraContext.Navigation.helpUrl) list.push(help);
        if (SpiraContext.ThemeName === "InflectraTheme") list.push(onboarding);

        list.push(keyboard);
        if (this.supportsCssVars) list.push(colorScheme);
        list.push(logout);

        this.setState({ userList: list });
    }


    // get color mode setting from local storage and update state and dom as needed
    // only take any action if the browser supports css variables
    userColorSchemeGetLocal() {
        if (this.supportsCssVars) {
            const localScheme = document.body.dataset.colorscheme;

            // auto is the default on page load, so apply the theme if the theme is not auto
            if (localScheme && localScheme != "auto") {
                this.setState(
                    { userColorScheme: localScheme },
                    // we need to refresh the user list so that the color mode item has the correct label;
                    this.userCreateList
                );
                this.userSetColorScheme(localScheme);
            } else {
                this.userSetColorScheme("auto");
            }
        }
    };

    // logic checks to work out which theme to use in the DOM - and then apply it
    // param: string of the theme to set things too
    userSetColorScheme(newColorScheme) {
        const systemScheme = window.matchMedia('(prefers-color-scheme: dark)');
        const isSystemSchemeDark = systemScheme.matches;
        let colorSchemeToUse = "";

        // we first work out the right theme to use. If the user is using AUTO, then we use the operating system setting. Otherwise, go with the local storage selection
        // handle add/remove of event listeners as needed too
        if (newColorScheme == "auto") {
            colorSchemeToUse = isSystemSchemeDark ? "dark" : "light";
            systemScheme.addListener(this.userSetColorSchemeAuto);
        } else {
            systemScheme.removeListener(this.userSetColorSchemeAuto);
            colorSchemeToUse = newColorScheme;
        }

        // now add the relevant information to DOM
        document.body.dataset.colorscheme = colorSchemeToUse;
        const companyLogo = document.getElementById("footerCompanyLogo") as HTMLImageElement;
        if (companyLogo) {
            if (colorSchemeToUse == "dark") {
                companyLogo.src = companyLogo.src.replace("CompanyLogo.svg", "CompanyLogo_Light.svg");
            } else {
                companyLogo.src = companyLogo.src.replace("CompanyLogo_Light.svg", "CompanyLogo.svg");
            } 
        }

        // need to do the same for CKEDITOR iframes
        this.ckeditorSetColorScheme(colorSchemeToUse, null);
    }

    // apply the auto theme to the DOM - used for when we are on auto mode to make add/remove of event listener simpler
    userSetColorSchemeAuto() {
        if (this.state.userColorScheme = "auto") this.userSetColorScheme("auto");
    }

    // toggle the theme to dark mode or light mode based on user action
    userToggleColorScheme() {
        let newColorScheme = "";
        switch (this.state.userColorScheme) {
            case "auto":
                newColorScheme = "dark";
                break;
            case "dark":
                newColorScheme = "light";
                break;
            case "light":
            default:
                newColorScheme = "auto";
                break;
        }
        // update state - used for showing the correct text on the button
        this.setState({ userColorScheme: newColorScheme });
        this.userCreateList();
        // save it to local storage for next time the page is loaded
        //localStorage.setItem("spira-color-scheme", newColorScheme);
        Inflectra.SpiraTest.Web.Services.Ajax.GlobalService.UserSettings_ColorMode_Set(newColorScheme);

        // update the DOM
        this.userSetColorScheme(newColorScheme);
    }

    // helper function that performs required checks on the ckeditors and sets them to the right color mode
    // because as of 6.1 they are in iframes this process is annoyingly convoluted
    // param: colorSchemeToUse string ("light", "dark", "auto")
    // param: optional bool that can be run to not check for ckeditor - used on doc list page where there's an iframe but no ckeditor
    ckeditorSetColorScheme(colorSchemeToUse, noCkeditorChecks) {
        if (colorSchemeToUse == "dark") {
            const ckeditorExists = typeof CKEDITOR != "undefined" && CKEDITOR.status == 'loaded' && CKEDITOR.instances;
            // on first page load we need to check that ckeditor is present and then wait for the iframes in them to be ready - these happen at different times
            if (noCkeditorChecks || ckeditorExists) {
                if (noCkeditorChecks || $("iframe.cke_wysiwyg_frame").contents().length > 0) {
                    // if the iframes exist then we can apply the dark class
                    $("iframe.cke_wysiwyg_frame, iframe.inflectra-iframe").contents().find("body").attr("data-colorscheme", "dark");
                } else {
                    // if iframes are not yet rendered, try a number of times - if still not there we stop trying
                    var counter = 0;
                    var ckEditorCheckStatus = setInterval(() => {
                        counter++;
                        if ($("iframe.cke_wysiwyg_frame").contents().length > 0) {
                            // the iframes should be ready, but we still have to use a setTimeout - 
                            setTimeout(() => $("iframe.cke_wysiwyg_frame, iframe.inflectra-iframe").contents().find("body").attr("data-colorscheme", "dark"), 500);
                            clearInterval(ckEditorCheckStatus);
                        } else if (counter == 10) {
                            clearInterval(ckEditorCheckStatus);
                        }
                    }, 500);
                }
            }
            // this code will only run when user changes color mode during page use so no checks for presence are required
        } else {
            $("iframe.cke_wysiwyg_frame, iframe.inflectra-iframe").contents().find("body").attr("data-colorscheme", "auto");
        }
    }



    /**
      * ==============
      * ADMINISTRATION
      * ==============
      */

    // toggles the opening and closing of the on page admin menu
    adminToggleMenu(location) {
        if (this.props.data.adminUrl) {
            if (!this.state.adminMenuShow) {
                this.adminMenuOpen(location)
            } else {
                this.adminMenuClose();
            }

        }
    }

    adminMenuOpen(location) {
        if (this.props.data.adminUrl) {
            Mousetrap.bind("escape", (e) => this.adminMenuClose());

            // close other nav dropdowns if we are opening the admin dropdown
            this.workspaceDrop.current && this.workspaceDrop.current.menuClose();
            this.artifactDrop.current && this.artifactDrop.current.menuClose();
            this.userDrop.current && this.userDrop.current.menuClose();

            // now open the admin menu
            this.setState({
                adminMenuShow: true,
                adminMenuPosition: location && location == "sidebar" ? "sidebar" : "global-nav"
            });
        }
    }

    adminMenuClose() {
        this.setState({ adminMenuShow: false });
        Mousetrap.unbind("escape");
    }

    adminMenuOnBlur(e) {
        const target = e.currentTarget;
        //setTimeout used so that as the event happens the correct element will be focused to make sure close event occurs only when it should
        setTimeout(() => {
            //check to see if the clicked item is contained within the element being closed on blur
            if (!target.contains(document.activeElement)) {
                this.adminMenuClose();
            }
        }, 0);
    }

    // return the correct part of the admin menu to display right at the top
    // this is can be product/template/program. 
    // the special case is if on a template, but the last project you went to was a product that uses the current template and you are an admin of that product
    adminSetMainData(workspaceType) {
        const { adminNavigation } = this.props.data;
        // first find if we have a match for the current type
        let mainData = null;
        switch (workspaceType) {
            case this.props.workspaceEnums.product:
                mainData = adminNavigation.project || null;
                break;
            case this.props.workspaceEnums.template:
                const showProject = this.templateAndProjectMatch() && adminNavigation.project;
                const showTemplate = !showProject && adminNavigation.template;
                mainData = showProject ? adminNavigation.project : showTemplate ? adminNavigation.template : null;
                break;
            case this.props.workspaceEnums.program:
                mainData = adminNavigation.program || null;
                break;
        }
        return mainData;
    }
    // return the correct type of the admin menu to display right at the top
    adminSetMainType(workspaceType) {
        const { adminNavigation } = this.props.data;
        // first find if we have a match for the current type
        let mainType = workspaceType;
        if (workspaceType == this.props.workspaceEnums.template && adminNavigation.project && this.templateAndProjectMatch()) {
            mainType = this.props.workspaceEnums.product;
        }
        return mainType;
    }
    // return the correct part of the admin menu to display second from the top
    // this is either the template admin or nothing
    // the template menu is only shown here if the workspace type is product and we also have a template menu
    adminSetSecondaryData(workspaceType) {
        const { adminNavigation } = this.props.data;
        // first find if we have a match for the current type
        let secondaryData = null;
        switch (workspaceType) {
            case this.props.workspaceEnums.program:
                secondaryData = null;
                break;
            case this.props.workspaceEnums.product:
            case this.props.workspaceEnums.template:
                secondaryData = adminNavigation.project && adminNavigation.template && this.templateAndProjectMatch() ? adminNavigation.template : null;
                break;
        }
        return secondaryData;
    }
    adminSetSecondaryType(workspaceType) {
        const { adminNavigation } = this.props.data;
        // first find if we have a match for the current type
        let secondaryType = workspaceType;
        if (workspaceType == this.props.workspaceEnums.product && adminNavigation.template && this.templateAndProjectMatch()) {
            secondaryType = this.props.workspaceEnums.template;
        }
        return secondaryType;
    }

    // utility function that checks whether the current project uses the current template
    templateAndProjectMatch(templateId = SpiraContext.ProjectTemplateId, projectId = SpiraContext.ProjectId) {
        const { templates } = this.props.data;
        const currentTemplate = templates.filter(x => x.id === templateId);
        let templateMatchesCurrentProject = false;
        if (currentTemplate && currentTemplate.length) {
            const templatesAttachedToCurrentProject = currentTemplate[0].projects.filter(x => x.id == projectId);
            templateMatchesCurrentProject = templatesAttachedToCurrentProject && templatesAttachedToCurrentProject.length ? true : false;
        }
        return templateMatchesCurrentProject;
    }




    // handle a click on any expand/collapse buttons in the workspace (programs and projects) dropdown
    // param: parentIds - array of strings of the unique id being toggled
    // param: newStatus bool of hide status - true = hide
    // param: stateName - string of the name of the object in state which stores the data
    // param: stateHiddenName - string of the name of the object in state which stores the array of parent Ids whose children should be hidden
    // return: updates state with the new expanded status, and calls function to update local storage
    expandingToggle(parentIds, newStatus, stateName, stateHiddenName) {
        const arrayCopy = this.state[stateName] ? JSON.parse(JSON.stringify(this.state[stateName])) : null;
        // only do function if we have arrays and other key params
        if (parentIds && Array.isArray(parentIds) && parentIds.length && Array.isArray(arrayCopy) && stateName && stateHiddenName) {
            // first get an array of arrays of children
            const childrenArrays = arrayCopy.filter(x => parentIds.indexOf(x.listId) >= 0).map(y => y.children);
            // combine these into a single array of ids
            let childrenIds = [];
            for (var i = 0; i < childrenArrays.length; i++) {
                childrenIds.push(...childrenArrays[i]);
            }
            // we can now navigate over the state array and set the new status for both the parents and children in a single loop
            // note that programs can be both parents and children
            const arrayToggled = arrayCopy.map(item => {
                if (parentIds.indexOf(item.listId) >= 0) {
                    item.hideChildren = newStatus;
                }
                if (childrenIds.indexOf(item.listId) >= 0) {
                    item.hide = newStatus;
                    if (item.children) {
                        item.hideChildren = newStatus;
                    }
                }
                return item;
            });
            // set state using the newly updated array
            this.setState({ [stateName]: arrayToggled });
            // and update the the parent ids list - for local storage and state
            this.setListInfo(parentIds, stateHiddenName);
        }
    }

    // parses an array of values to update state and local storage - adding/removing from the final array if currently absent/present
    // param: parentIds - array of strings of the unique id being toggled
    // param: stateListName = string of the state list to update
    // return: updates state with the new expanded status, and updates local storage
    setListInfo(parentIds, stateListName) {
        if (parentIds && Array.isArray(parentIds) && parentIds.length && this.state[stateListName]) {
            const arrayCopy = JSON.parse(JSON.stringify(this.state[stateListName]));

            for (var i = 0; i < parentIds.length; i++) {
                const arrIndex = arrayCopy.indexOf(parentIds[i]);
                if (arrIndex >= 0) {
                    arrayCopy.splice(arrIndex, 1);
                } else {
                    arrayCopy.push(parentIds[i]);
                }
            };

            this.setState({ [stateListName]: arrayCopy });
            localStorage.setItem(stateListName, JSON.stringify(arrayCopy));
        }
    }

    // converts a project / program tokenized url with the correct project / program id
    // param: string url
    // return: new url
    replaceWorkplaceTokenUrl(url) {
        const token = this.props.workspaceType == this.props.workspaceEnums.program ? "{projGroupId}" : "{projId}";
        const workspaceId = this.props.workspaceType == this.props.workspaceEnums.program ? SpiraContext.ProjectGroupId : SpiraContext.ProjectId
        return url ? url.replace(token, workspaceId) : url;
    }

    // gets any existing search terms stored in local storage and saves them to state
    searchGetLocalTerms() {
        const localTerms = JSON.parse(localStorage.getItem(this.enum.searchStorage));
        if (localTerms && localTerms.length > 0) this.setState({ searchHistory: localTerms })
    }

    // toggles opening the search input field and sets focus after animation
    searchToggleOpen(e?) {
        const newValue = e && e.target ? e.target.checked : !this.state.searchIsOpen;
        this.setState({ searchIsOpen: newValue }, () => {
            if (this.state.searchIsOpen) setTimeout(() => { this.searchInput.current.focus() }, 500);
        });
    }

    // we sometimes need to know if buttons are missing from the main nav ul (eg reporting and artifact buttons)
    // return: int of how many buttons are missing 
    mainNavHiddenButtons() {
        let missingNavMenus = 0;
        if (!this.state.artifactList || !this.state.artifactList.length) missingNavMenus++;
        if (!this.state.reportsButton || !this.state.reportsButton.url) missingNavMenus++;
        return missingNavMenus;
    }

    // if buttons are missing from the main nav then the search button moves over to the left
    // but when search input is shown we need the search button to be in the same place as normal (ie to the right)
    // return: string of bespoke classes that specify the offset to the right when in search mode
    searchButtonGetOffsetClasses() {
        const offset = this.mainNavHiddenButtons();
        let classes = "";
        if (offset == 1) {
            classes = "nav-top-set-ml1-on-search";
        } else if (offset == 2) {
            classes = "nav-top-set-ml2-on-search";
        }
        return classes;
    }

    // updates search term in state from input - initiates search if ENTER is pressed
    searchTermUpdate(e) {
        this.setState({ searchTerm: e.target.value });
    }

    // detects special key presses on search box 
    searchKeyPress(e) {
        // on enter initiate the search
        if (e.keyCode === 13) {
            this.searchInit(globalFunctions.trim(this.state.searchTerm));
            // stop the event bubble
            e.preventDefault();
            e.stopPropagation();
            // on escape close the form
        } else if (e.keyCode === 27) {
            this.setState({ searchIsOpen: false });
        }
    }

    searchInit(searchTerm) {
        // Get the text from the search box
        const searchService = Inflectra.SpiraTest.Web.Services.Ajax.SearchService,
            errorMessageControlId = $find('<%=divMessageBox.ClientID %>');

        // Trim the keywords
        const keywords = globalFunctions.trim(searchTerm);
        if (keywords && keywords != '') {
            // Set the error box
            globalSearch.msgBoxId = errorMessageControlId;

            // Send the current project ID to the global search function
            globalSearch.currentProjectId = SpiraContext.ProjectId;

            // close the search input
            this.setState({ searchIsOpen: false });

            // send params to the global search service
            globalSearch.searchInit(keywords, searchService);

            // save the search history to state local storage
            if (this.state.searchHistory.indexOf(keywords) < 0) {
                const newHistory = [keywords, ...this.state.searchHistory];
                // make sure the search history doesn't get too long
                if (newHistory.length > this.enum.searchHistoryMax) {
                    newHistory.slice(0, this.enum.searchHistoryMax - 1);
                }
                this.setState({ searchHistory: newHistory });
                localStorage.setItem(this.enum.searchStorage, JSON.stringify(newHistory));
            }
        }
    }

    // handler function called when the mobile nav button to open and close the menu is clicked
    // return: updates state here as well as of specific dropdowns (via refs to call those child components here)
    wholeNavMobileToggle(e) {
        // if we are closing the nav, close any open dropdowns
        if (!e.target.checked) {
            this.workspaceDrop && this.workspaceDrop.current.menuClose();
            this.artifactDrop && this.artifactDrop.current.menuClose();
            this.userDrop && this.userDrop.current.menuClose();
            !globalFunctions.objectIsEmpty(this.props.data.adminNavigation) && this.adminMenuClose();
        }
        this.setState({ navMobileIsOpen: e.target.checked });
    }

    // used to access dropdowns and toggle them open - used by keyboard shortcuts
    // we close the admin first and do the toggle last just to make sure that any other keyboard unbindings have taken place before the new menu might be opened
    workspaceDropToggleOpen() {
        if (this.workspaceDrop.current) {
            this.adminMenuClose();
            this.artifactDrop.current && this.artifactDrop.current.menuClose();
            this.userDrop.current && this.userDrop.current.menuClose();
            this.workspaceDrop.current.menuToggle();
        }
    }
    artifactDropToggleOpen() {
        if (this.artifactDrop.current) {
            this.adminMenuClose();
            this.userDrop.current && this.userDrop.current.menuClose();
            this.workspaceDrop.current && this.workspaceDrop.current.menuClose();
            this.artifactDrop.current.menuToggle();
        }
    }
    userDropToggleOpen() {
        if (this.userDrop.current) {
            this.adminMenuClose();
            this.workspaceDrop.current && this.workspaceDrop.current.menuClose();
            this.artifactDrop.current && this.artifactDrop.current.menuClose();
            this.userDrop.current.menuToggle();
        }
    }


    // functions to control the global spinner state
    spinnerShow() {
        const newSpinner = this.state.spinnerShow + 1;
        this.setState({ spinnerShow: newSpinner });
    }
    spinnerHide() {
        const newSpinner = this.state.spinnerShow > 0 ? this.state.spinnerShow - 1 : 0;
        this.setState({ spinnerShow: newSpinner });
    }

    // function to show spinner on individual links
    handleOpenLink(name, url, evt) {
        // open link in new tab if ctrl+click / middle click
        if (evt.ctrlKey || evt.which === 2) {
            window.open(url, '_blank');
        } else {
            window.location.assign(url);
            this.setState({ [name]: true });
        }
    }

    // sets the status of a dropdown - as passed by the dropdown child component
    // this is useful when the parent needs to keep track of this state
    // eg. when in mobile mode to alter the z-index to ensure the dropdown menu is fully visible
    updateDropStatus(dataName, newStatus) {
        if (dataName) this.setState({ [dataName + "IsOpen"]: newStatus });
    }

    // opening the onboarding component
    onboardingShow(onlyOpenIfHasUnseen) {
        if (SpiraContext.ThemeName === "InflectraTheme") {
            const seenAppIntro = SpiraContext.GuidedToursSeen.guidedToursNavigationBarSeen || SpiraContext.GuidedToursSeen.appIntro || false; // "guidedToursNavigationBarSeen" was used in v5
            const seenUpdate = SpiraContext.GuidedToursSeen["update-" + SpiraContext.VersionNumber] ? true : SpiraContext.GuidedTours.filter(x => x.name == ["update-" + SpiraContext.VersionNumber]).length == 0;
            let shouldShowUpdate = false;
            if (seenAppIntro && !seenUpdate) {
                const now = new Date(),
                    year = now.getFullYear(),
                    month = (now.getMonth() + 1) < 10 ? "0" + (now.getMonth() + 1) : (now.getMonth() + 1),
                    day = now.getDate() < 10 ? "0" + now.getDate() : now.getDate(),
                    todayIsoDate = year + "-" + month + "-" + day;
                shouldShowUpdate = new Date(seenAppIntro) < new Date(todayIsoDate);
            }
            // if we are passing in a setting to only open tours if we have unseen ones, only open it if we have no tours listed out from the server
            if ((onlyOpenIfHasUnseen && (!seenAppIntro || shouldShowUpdate)) || !onlyOpenIfHasUnseen) {
                // clear out whatever was in the dialog before, if anything
                globalFunctions.dlgGlobalDynamicClear();

                ReactDOM.render(
                    <OnBoarding
                        tours={SpiraContext.GuidedTours || null}
                        toursSeen={SpiraContext.GuidedToursSeen || null}
                        seenAppIntro={seenAppIntro}
                        seenUpdate={!shouldShowUpdate}
                        updateTourName={"update-" + SpiraContext.VersionNumber}
                    />,
                    document.getElementById('dlgGlobalDynamic')
                );
            }
        }
    }

    // if a user has not visited this general artifact before then register their first visit to the tours collection
    // this happens automatically on each load of the global nav and allows us to not overload users with tours they don't need
    onboardingRegisterPage() {
        if (SpiraContext.ThemeName === "InflectraTheme") {
            const pageName = "navigationLink_" + this.state.currentLocation;
            if (!SpiraContext.GuidedToursSeen[pageName]) {
                Inflectra.SpiraTest.Web.Services.Ajax.GlobalService.UserSettings_GuidedTour_SetSeen(pageName);
            }

            //now update the page count tracker
            const pageCount = "navigationCount_" + this.state.currentLocation;
            Inflectra.SpiraTest.Web.Services.Ajax.GlobalService.UserSettings_GuidedTour_SetNavigationLinkCount(pageCount);
        }
    }



    /**
     * ==================
     * KEYBOARD SHORTCUTS
     * ==================
     */

    keyboardShortcutsInit() {
        if (!this.kbCodes && !this.kbCodes.codes) return;

        let kbCodes = this.kbCodes.codes;

        // KEYBOARD GLOSSARY POPUP - show hide
        Mousetrap.bind(kbCodes.showGlossary.term, (e) => {
            this.keyboardTogglePopup();
        });

        // NAVIGATE TO PAGES
        // my page
        Mousetrap.bind('n 1', (e) => window.location.href = globalFunctions.replaceBaseUrl(SpiraContext.Navigation.myPageUrl));
        // workspace dashboard
        const workspaceUrl = this.workspaceUrl();
        if (workspaceUrl) Mousetrap.bind('n 2', (e) => window.location.href = workspaceUrl);
        // admin
        if (this.props.data.adminUrl) Mousetrap.bind('n a d', (e) => window.location.href = this.props.data.adminUrl);
        // timecard
        if (this.props.data.myTimecardUrl) Mousetrap.bind('n t i', (e) => window.location.href = globalFunctions.replaceBaseUrl(this.props.data.myTimecardUrl));

        // NAVIGATE TO ARTIFACTS
        for (var i = 0; i < this.state.artifactList.length; i++) {
            const art = this.state.artifactList[i];
            if (art.id && art.url && kbCodes[art.id]) Mousetrap.bind(kbCodes[art.id].term, () => window.location.href = art.url);
        }

        // reporting
        if (this.state.reportsButton && this.state.reportsButton.url) {
            Mousetrap.bind('n r p', (e) => window.location.href = this.state.reportsButton.url);
        }
        // user profile
        Mousetrap.bind('n 0', (e) => window.location.href = globalFunctions.replaceBaseUrl(SpiraContext.Navigation.myProfileUrl));

        // global actions
        // search
        if (!this.state.searchIsOpen) {
            Mousetrap.bind('shift+s', (e) => this.searchToggleOpen());
        }
        // new incident
        if (SpiraContext.ProjectId && SpiraContext.BaseUrl) {
            Mousetrap.bind('shift+i', (e) =>
                window.location.href = SpiraContext.BaseUrl + SpiraContext.ProjectId + '/Incident/New.aspx'
            )
        }

        // opening dropdowns
        Mousetrap.bind('mod+alt+w', (e) => this.workspaceDropToggleOpen());
        Mousetrap.bind('mod+alt+a', (e) => this.artifactDropToggleOpen());
        Mousetrap.bind('mod+alt+u', (e) => this.userDropToggleOpen());

        // opening admin menu
        if (!globalFunctions.objectIsEmpty(this.props.data.adminNavigation)) {
            Mousetrap.bind('shift+a', (e) => this.adminToggleMenu(null));
        }
    }

    keyboardTogglePopup() {
        $('#global-nav-keyboard-shortcuts').modal();
    }






    render() {
        // sets const from state to make referencing them simpler
        const {
            adminShow,
            artifactList,
            artifactParent,
            currentLocation,
            reportsButton,
            searchHistory,
            searchTerm,
            spinnerShow,
            userIcon,
            userList,
            workspaceCurrent,
            workspaceList
        } = this.state;

        const loaderIcon = <div className="lds-ellipsis"><div /><div /><div /><div /></div>;
        const productIcon = <img
            alt={this.props.product}
            className="nav-home-logo mh-100 mw5" // mh-100 is used to make sure the icon scales properly in small device mode
			src={this.props.baseThemeUrl + "Images/org-Program-outline.svg"}
        />;
        const workspaceTitle = artifactParent.name;
        const workspaceIcon = <img
            alt={workspaceTitle}
            className="nav-home-logo mh5 mw5 pa1 pl0-sm pr3-sm"
			src={this.props.baseThemeUrl + "Images/org-Program-outline.svg"}
        />;

        const isMyPage = currentLocation === globalFunctions.artifactTypeEnum.myPage,
            isWorkspaceHome =
                currentLocation === globalFunctions.artifactTypeEnum.projectHome ||
                currentLocation === globalFunctions.artifactTypeEnum.programHome ||
                currentLocation === globalFunctions.artifactTypeEnum.administration && this.props.workspaceType === this.props.workspaceEnums.enterprise,
            isArtifact = artifactList.filter(x => x.id).map(x => x.id).indexOf(currentLocation.toString()) >= 0,
            isReporting = currentLocation === globalFunctions.artifactTypeEnum.reports,
            isUserPage = userList.map(x => x.listId).indexOf(currentLocation.toString()) >= 0,
            isAdministration = currentLocation === globalFunctions.artifactTypeEnum.administration;

        const searchHistoryListItems = (searchHistory && searchHistory.length > 0) ?
            searchHistory.map((searchTerm, i) =>
                <li className="lsn my2 mx3 pa0" key={i}>
                    <a
                        className="db tdn tdn-hover mid-gray mid-gray-hover py3 br2 bg-vlight-gray-hover pl6 pr4 pointer"
                        onClick={this.searchInit.bind(null, searchTerm)}
                    >
                        {searchTerm}
                    </a>
                </li>
            )
            : <li className="lsn my2 mx3 pa0 o-70 pl6 pr4">{resx.Global_None}</li>;

        // set the max height dynamically based on what is shown or hidden in this part of the menu - used for the mobile version of the nav
        const mainListClasses = "fw2 cf nav-top-list nav-top-list-" + (5 - this.mainNavHiddenButtons()) + (this.state.workspaceListIsOpen || this.state.artifactListIsOpen ? " z-3" : " z-2")

        return (
            <nav
				className="nav-top nav-fixed mw-100 z-999"
				id="nav-top"
				role="navigation"
				style={{ backgroundColor: SpiraContext.UnsupportedBrowser ? "#e63d3d" : ""}}
                >
                {spinnerShow > 0 &&
                    <div
                        id="globalNav_spinner" className={"absolute h-nav-top w-auto px4 top0 bg-yolk z-3 df items-center right0-sm" + (!globalFunctions.objectIsEmpty(this.props.data.adminNavigation) ? " right8" : " right4")}>
                        <div className="lds-ellipsis mr5"><div /><div /><div /><div /></div>
                        {resx.GlobalFunctions_SpinnerText}
                    </div>
                }
                <input
                    checked={this.state.searchIsOpen}
                    className="dn"
                    id="nav-search-check"
                    onChange={this.searchToggleOpen}
                    type="checkbox"
                />
                <input
                    type="checkbox"
                    className="dn"
                    id="nav-menu-sm-check"
                    onChange={this.wholeNavMobileToggle}
                />


                <ul className="fw2 cf nav-top-head-sm" role="navigation">
                    <li className="nav-top-li absolute">
                        <label className="nav-top-btn no-indicator ma0" for="nav-menu-sm-check" id="globalNav_btn-show-list" href="products">
                            <i className="nav-hamburger" />
                        </label>
                    </li>
                    
                </ul>
                <ul
                    className={mainListClasses}
                    role="navigation"
                >
                    
                    {this.workspaceUrl() &&
                        <li className="nav-top-li w6 w-100-xs shadow-r-sm-tiber" id="global-navigation-workspace-home">
                            <a
                                aria-label="Global Navigation Workspace Home"
                                className={"nav-top-btn tdn vvlight-gray df py2 nav-home-link h-nav-top w-100" + (isWorkspaceHome ? " bg-nav-bg-highlight-subtle" : "")}
                                href={this.workspaceUrl()}
                                id="globalNav_workspaceHome"
                                onClick={this.handleOpenLink.bind(this, "workspaceLoading", this.workspaceUrl())}
                                title={resx.GlobalNavigation_TooltipWorkspaceHome}
                                >
                                {this.state.workspaceLoading ? loaderIcon : workspaceIcon}
                                <span className="dn-md-up">{workspaceTitle}</span>
                            </a>
                        </li>
                    }
                    <li className="nav-top-li nav-hide-on-search nav-top-set-width shadow-r-sm-tiber" id="global-navigation-workspace-li">
                        <ErrorBoundary>
                            <RctDrop
                                ariaLabelWrapper="Global Navigation Open Workspace Menu"
                                boxClasses="w-100"
                                buttonClasses="nav-top-btn"
                                dataName="workspaceList"
                                dataHiddenName="workspaceHidden"
                                defaultText={workspaceList.length ? this.state.workspaceDefaultText : resx.NavigationBar_NoWorkspaceMembership}
                                defaultTitle={resx.GlobalNavigation_TooltipWorkspace}
                                domId="globalNav_workspaceDropdown"
                                expanderClasses="nav-text yolk-hover"
                                expanderClickManager={this.expandingToggle}
                                menuToggleCallback={this.updateDropStatus}
                                items={workspaceList}
                                menuClasses="nav-drop-menu shadow-b-mid-gray"
                                ref={this.workspaceDrop}
                                selected={workspaceCurrent}
                                showLoaderOnClick
                            />
                        </ErrorBoundary>
                    </li>
                    {this.artifactShowDropdown() ?
                        <ErrorBoundary>
                            <li className="nav-top-li nav-hide-on-search nav-top-set-width shadow-r-sm-tiber" id="global-navigation-artifact-li">
                                <RctDrop
                                    ariaLabelWrapper="Global Navigation Open Artifact Menu"
                                    boxClasses="w-100"
                                    buttonClasses={"nav-top-btn" + (isArtifact ? " bg-nav-bg-highlight-subtle" : "")}
                                    dataName="artifactList"
                                    dataHiddenName="artifactHidden"
                                    defaultText={resx.Global_Artifacts}
                                    defaultTitle={resx.GlobalNavigation_TooltipArtifacts}
                                    domId="globalNav_artifactDropdown"
                                    expanderClasses="nav-text yolk-hover fs-h5 mr2"
                                    expanderClickManager={this.expandingToggle}
                                    items={artifactList}
                                    menuClasses="nav-drop-menu shadow-b-mid-gray"
                                    menuToggleCallback={this.updateDropStatus}
                                    ref={this.artifactDrop}
                                    selected={currentLocation}
                                    showLoaderOnClick
                                    showImageInButton
                                />
                            </li >
                        </ErrorBoundary>
                        : null
                    }
                    {(reportsButton && reportsButton.url && this.props.workspaceType != this.props.workspaceEnums.enterprise) ?
                        <li className="nav-top-li nav-hide-on-search nav-top-set-width shadow-r-sm-tiber" id="global-navigation-reporting-li">
                            <a
                                aria-label="Global Navigation Reporting"
                                className={"nav-top-btn" + (isReporting ? " bg-nav-bg-highlight-subtle" : "")}
                                href={reportsButton.url}
                                id="globalNav_reporting"
                                onClick={this.handleOpenLink.bind(this, "reportsLoading", reportsButton.url)}
                                title={resx.GlobalNavigation_TooltipReporting}
                            >
                                {this.state.reportsLoading ? loaderIcon : reportsButton.name}
                            </a>
                        </li >
                        : null
                    }
                    <li className="nav-top-li nav-show-on-search shadow-r-sm-tiber" id="global-navigation-search-box-li">
                        <aside className="nav-search-box">
                            <button
                                aria-label="Global Navigation Search"
                                aria-hidden="true"
                                className="nav-search-go"
                                id="globalNav_search_button"
                                onClick={this.searchInit.bind(null, globalFunctions.trim(this.state.searchTerm))}
                                title={resx.Global_Search}
                                type="button"
                            >
                                <i className="nav-search-icon"></i>
                            </button>
                            <input
                                className="nav-search-input"
                                id="globalNav_search_input"
                                maxlength="255"
                                onChange={this.searchTermUpdate}
                                onKeyDown={this.searchKeyPress}
                                placeholder={resx.Global_Search}
                                ref={this.searchInput}
                                title={resx.GlobalNavigation_SearchTooltip}
                                type="text"
                                value={searchTerm}
                            />
                            <aside className="nav-quick-links bg-white py4 mt2" aria-label="Global Navigation Recent Search Box">
                                <h3 className="my3 mx5 pl3 fs-h6 gray">{resx.NavigationBar_RecentSearches}</h3>
                                <ul className="ma0 pa0 fs-h6">
                                    {searchHistoryListItems}
                                </ul>
                            </aside>
                        </aside>
                    </li >
                    <li
                        className={"nav-top-li w6 items-center dn-sm h-nav-top shadow-r-sm-tiber " + this.searchButtonGetOffsetClasses()}
                        id="global-navigation-search-btn-li"
                        >
                        <label
                            className="nav-top-btn nav-search-label pa0 w5 h5 pointer mx-auto db"
                            for="nav-search-check"
                            id="btn-global-search"
                            title={resx.Global_Search}
                        >
                            <i className="nav-search-icon" />
                        </label>
					</li>
				</ul>
                <ul className="fw2 cf nav-top-list-user z-2" role="navigation">
                    <li className="nav-top-li shadow-l-sm-tiber" id="global-navigation-user-li">
                        <ErrorBoundary>
                            <RctDrop
                                ariaLabelWrapper="Global Navigation Open User Menu"
                                boxClasses="w-100"
                                buttonClasses={"nav-top-btn py0 min-w6" + (isUserPage ? " bg-nav-bg-highlight-subtle" : "")}
                                defaultText={<span>
                                    <AvatarIcon
                                        hasIcon={userIcon.hasIcon}
                                        icon={userIcon.avatarUrl}
                                        name={userIcon.name}
                                        nameAsIcon={userIcon.nameAsIcon}
                                        onClick={this.userMenuToggle}
                                        wrapperClasses="lh-initial di"
                                    />
                                </span>}
                                domId="globalNav_userDropdown"
                                expanderClasses="nav-text yolk-hover fs-h5 mr2"
                                expanderClickManager={this.expandingToggle}
                                isMobileMenu={this.state.navMobileIsOpen}
                                items={userList}
                                menuClasses="nav-drop-menu left-auto right0 shadow-b-mid-gray"
                                menuToggleCallback={this.updateDropStatus}
                                ref={this.userDrop}
                                selected={currentLocation}
                                showLoaderOnClick
                                showDefaultAndSelected
                                showDefaultOnRight
                                extraDefaultClasses="dn-md-up"
                                extraDefaultText={userIcon.name}
                            />
                        </ErrorBoundary>
                    </li>
                    {!globalFunctions.objectIsEmpty(this.props.data.adminNavigation) &&
                        <li
                            className="nav-top-li w6 shadow-l-sm-tiber"
                            onBlur={this.adminMenuOnBlur}
                            id="global-navigation-admin-li"
                            >
                            <button
                                aria-label="Global Navigation Open Admin Menu"
                                className={"nav-top-btn tdn df py2 nav-home-link h-nav-top w-100" + (isAdministration ? " bg-nav-bg-highlight-subtle" : "")}
                                id="globalNav_adminMenu_open"
                                onClick={this.adminToggleMenu}
                                title={resx.Global_Administration}
                                type="button"
                                >
                                <img
                                    className="mh5 mw5 pr3-sm"
                                    src={this.props.baseThemeUrl + "Images/org-Administration.svg"}
                                    />
                                <span className="dn-md-up">{resx.Global_Administration}</span>
                            </button>
                            {this.state.adminMenuShow && !globalFunctions.objectIsEmpty(this.props.data.adminNavigation) ?
                                <aside
                                    className={"fixed mh-belowHeader ov-y-auto top-nav mw-100 w960 pa4 br3 br0-top bg-nav-bg filter-blur-bg" + (this.state.adminMenuPosition == "sidebar" ? " left2 br0-left" : " right0 br0-right")}
                                    id="globalNav_adminAside"
                                    tabindex="0"
                                    >
                                    <ErrorBoundary>
                                        <AdminMenu
                                            currentLocation={currentLocation}
                                            currentProjectId={SpiraContext.ProjectId}
                                            currentProgramId={SpiraContext.ProjectGroupId}
                                            currentTemplateId={SpiraContext.ProjectTemplateId}
                                            system={this.props.data.adminNavigation.system}
                                            workspaceMain={this.adminSetMainData(this.props.workspaceType)}
                                            workspaceMainType={this.adminSetMainType(this.props.workspaceType)}
                                            workspaceSecondary={this.adminSetSecondaryData(this.props.workspaceType)}
                                            workspaceSecondaryType={this.adminSetSecondaryType(this.props.workspaceType)}
                                            workspaceEnums={this.props.workspaceEnums}

                                            isSystemAdmin={this.props.isSysAdmin}
											isReportAdmin={this.props.isReportAdmin}
                                            wrapperId="global-admin-nav"
                                            sectionClasses=""
                                            sectionTitleClasses="yolk mt0 mb3 bb b-yolk bw1 fs-h4"
                                            sectionTitleLinkClasses="yolk nav-text-hover transition-all tdn tdn-hover"
                                            subSectionClasses="u-box_group w-33 w-50-sm w-100-xs px4 mb4"
                                            subSectionWrapperClasses="df flex-wrap"
                                            linkClasses="nav-text nav-top-active-hover transition-all tdn tdn-hover"
                                        />
                                    </ErrorBoundary>
                                </aside>
                                : null
                            }
                        </li>
                    }
                </ul >
                <label className="modal-overlay modal-full-belowHeader bg-darken-60" for="nav-search-check" tabindex="-1"></label>
            </nav >
        )
    }
}





// this try function wrapper makes sure that react render is initiated as soon as possible - ie as soon as the target element has rendered
// note that just calling the reactdom.render function on script load is too soon: the dom element is not present
function navLoadTry() {
    // check that the SpiraContext object is fully written out to the page
    if (typeof SpiraContext == 'undefined' || typeof SpiraContext.Navigation == 'undefined') {
        setTimeout(() => navLoadTry(), 100);
        // make sure that the element exists
    } else if (!document.getElementById('global-navigation')) {
        window.requestAnimationFrame(navLoadTry);
        // only then can we render
    } else {
        ReactDOM.render(
            <GlobalNavigation
                data={SpiraContext.Navigation}
                baseThemeUrl={SpiraContext.BaseThemeUrl}
                baseUrl={SpiraContext.BaseUrl}
                isGroupAdmin={SpiraContext.IsGroupAdmin}
                isPortfolioEnabled={SpiraContext.Features.portfolios}
                isPortfolioViewer={SpiraContext.IsPortfolioAdmin}
                isEnterpriseViewer={SpiraContext.IsPortfolioAdmin} // as of 6.5 this permission is identical to that of viewing portfolios, but that may change in the future so keeping them separate
                isProjectAdmin={SpiraContext.IsProjectAdmin}
				isSysAdmin={SpiraContext.IsSystemAdmin}
				isReportAdmin={SpiraContext.isReportAdmin}
                projectGroupId={SpiraContext.ProjectGroupId}
                projectId={SpiraContext.ProjectId}
                product={SpiraContext.ProductType}
                ref={(rct_comp_globalNav) => { (window as any).rct_comp_globalNav = rct_comp_globalNav }}
                workspaceEnums={SpiraContext.WorkspaceEnums}
                workspaceType={SpiraContext.WorkspaceType}
            />,
            document.getElementById('global-navigation')
        );
    }
};
navLoadTry();
