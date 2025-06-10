// renders a nested menu - ie administration

class AdminMenu extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);
        this.state = {
            //workspaceMain: null,
            //workspaceSecondary: null,
            //system: null,
            collapsedSubSections: []
        }

        this.toggleExpanded = this.toggleExpanded.bind(this);
    }



    componentWillMount() {
        this.getCollapsedSubSections(this.props.wrapperId);
    }



    // utility function to pass into a sort, that will sort based on the index property of an admin menu list
    compareByIndex(a, b) {
        return a.index < b.index ? -1 : a.index > b.index ? 1 : 0;
    }

    // utility function to pass into a sort, that will sort based on the name property of an admin menu list
    compareByName(a, b) {
        return a.name < b.name ? -1 : a.name > b.name ? 1 : 0;
    }

    // gets any existing searcg terms stored in local storage and saves them to state
    getCollapsedSubSections(wrapperId) {
        const localTerms = JSON.parse(localStorage.getItem(wrapperId + "-collapse"));
        if (localTerms && localTerms.length > 0) this.setState({ collapsedSubSections: localTerms });
    }

    // toggle the specific section to open or closed, update state, and update local storage
    toggleExpanded(id) {
        if (!id) return;
        let newList = this.state.collapsedSubSections;
        if (this.state.collapsedSubSections.length > 0 && globalFunctions.findItemInArray(this.state.collapsedSubSections, id)) {
            const position = this.state.collapsedSubSections.indexOf(id);
            newList.splice(position, 1);
        } else {
            newList.push(id);
        }
        this.setState({ collapsedSubSections: newList });
        localStorage.setItem(this.props.wrapperId + "-collapse", JSON.stringify(newList));
    }

    // utility function to create the correct href from a passed in link object
	createLinkUrl(link, section) {
		// createa the link suffix - if it already has aspx in it, don't add it at the end, otherwise add it at the end, otherwise set is as blank
		const linkSuffix = link && link.url ? (link.url.includes(".aspx") || link.urlIsFromRoot ? link.url : link.url + ".aspx" ) : "";
        let suffix;

        if (link.urlIsComplete) {
            return linkSuffix;
        }
        if (link.urlIsFromRoot) {
            suffix = linkSuffix;
        } else {
            switch(section) {
                case this.props.workspaceEnums.product:
                    let projectSuffixBase = this.props.currentProjectId + "/Administration/"
                    suffix = linkSuffix ? projectSuffixBase + linkSuffix : projectSuffixBase + "Default.aspx";
                    break;
                case this.props.workspaceEnums.program:
                    let programSuffixBase = "pg/" + this.props.currentProgramId + "/Administration/"
                    suffix = linkSuffix ? programSuffixBase + linkSuffix : programSuffixBase + "Default.aspx";
                    break;
                case this.props.workspaceEnums.template:
					let templateSuffixBase = "pt/" + this.props.currentTemplateId + "/Administration/";
                    suffix = linkSuffix ? templateSuffixBase + linkSuffix : templateSuffixBase + "Default.aspx";
                    break;
                case this.props.workspaceEnums.enterprise:
                default:
                    suffix = linkSuffix ? "Administration/" + linkSuffix : "Administration.aspx";
                    break;
            }

        }

        return SpiraContext.BaseUrl + suffix;
    }

    // used to create each specific section of the menu (eg system, project, program, template)
    createMenuSection(section, wrapperId, sectionType) {
        const wrapperIdBase = wrapperId + "-" + section.id;
        return (
            <div role="group" className={this.props.sectionClasses ? this.props.sectionClasses : ""}>
                {!this.props.hideTitle &&
                    <h3
                        className={this.props.sectionTitleClasses ? this.props.sectionTitleClasses : ""}
                        id={wrapperIdBase}
                    >
                        {(typeof section.url != "undefined") ?
                            <a
                                className={this.props.sectionTitleLinkClasses ? this.props.sectionTitleLinkClasses : ""}
                                href={this.createLinkUrl(section.url, sectionType)}
                            >
                                {section.name}
                            </a>
                            : <span>{section.name}</span>
                        }
                    </h3>
                }
                {section.links && this.createLinkList(section.links, wrapperIdBase)}
                {section.subSections && this.createSubSections(section.subSections, wrapperIdBase, sectionType)}
            </div>
        )
    }

    // creates a list of subsections within a main section (as well as all required links in each section)
	// handle permissions / access to each subsection - if required permissions provided 
    createSubSections(subSections, wrapperId, sectionType) {
		if (subSections) {
			// filter the subsections into those the user has access to
			const allowedsubSections = subSections.filter(subSection => {
				// if no array of permission types provided just display the subsection
				// otherwise check if the user has any of the required permissions
				if (!subSection.adminPermissions || globalFunctions.isAuthorizedAdminAccess(subSection.adminPermissions, globalFunctions.permissionEnum)) {
					return subSection;
				}
			});
			const subSectionItems = allowedsubSections.sort(this.compareByIndex).map(subSection => {
                subSection.expanded = true;
                const wrapperIdBase = subSection.id && wrapperId ? wrapperId + "-" + subSection.id : "";
                if (globalFunctions.findItemInArray(this.state.collapsedSubSections, wrapperIdBase)) {
                    subSection.expanded = false;
                }
                const subSectionWithUrls = subSection.links ? subSection.links.map(x => {
					x.finalUrl = this.createLinkUrl(x, sectionType);
                    return x;
                }) : null;

				const isCurrentArtifact = this.props.currentLocation && subSection.navigationId && this.props.currentLocation == subSection.navigationId,
					//check if any url in the section matches the current one
					currentUrlMatchesToSubSectionurl = subSectionWithUrls.map(x => x.finalUrl.replace(/s.aspx$/, "")).filter(x => window.location.href.indexOf(x) >= 0),
					isCurrentAdminSubsection = currentUrlMatchesToSubSectionurl.length ? true : false,
                    subSectionClasses = (this.props.subSectionClasses ? this.props.subSectionClasses : "") + (isCurrentArtifact || isCurrentAdminSubsection ? "  bg-nav-bg-highlight-subtle br3" : "");
                return (
                    <div
                        className={subSectionClasses}
                        role="group"
                        data-collapsible="true"
                        id={wrapperIdBase}
                        key={subSection.index}
                        >
                        <div
                            aria-expanded={subSection.expanded}
                            className="u-box_header fs-h6 orange bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                            onClick={this.toggleExpanded.bind(null, wrapperIdBase)}
                            >
                            <h4
                                className="fs-h6 mt0 mb2 ff-kanit di"
                                id={wrapperIdBase + "-heading"}
                                >
                                {subSection.name}
                            </h4>
                            <span className={"u-anim u-anim_open-close fr mr3 mt2" + (subSection.expanded ? " is-open" : " o-1-always")}></span>
                        </div>
                        {subSection.links && this.createLinkList(subSectionWithUrls, wrapperIdBase)}
                    </div>
                )
            });
            return (
                <div className={this.props.subSectionWrapperClasses ? this.props.subSectionWrapperClasses : ""}>
                    {subSectionItems}
                </div>
            )
        }
    }

    // creates a specific ul with li's for each of the links passed in
    createLinkList(links, wrapperId) {
        if (links) {
            const listItems = links.sort(this.compareByIndex).map(link =>
                <li key={link.index}>
                    <a 
                        className={this.props.linkClasses ? this.props.linkClasses : ""}
                        id={link.id && wrapperId ? wrapperId + "-" + link.id : ""}
                        href={link.finalUrl}
                        >
                        {link.name}
                    </a>
                </li>
            );
            return (
                <ul className="u-box_list">
                    {listItems}
                </ul>
            )
        }
    }

    render() {
        let workspaceMainMenu = this.props.workspaceMain && this.createMenuSection(this.props.workspaceMain, this.props.wrapperId, this.props.workspaceMainType);
        let workspaceSecondaryMenu = this.props.workspaceSecondary && this.createMenuSection(this.props.workspaceSecondary, this.props.wrapperId, this.props.workspaceSecondaryType);
        let systemMenu = this.props.system && this.createMenuSection(this.props.system, this.props.wrapperId, this.props.workspaceEnums.system)

        // finally render the full admin menu
        return (
            <div role="menu" aria-label="Administration Menu">
                {workspaceMainMenu}
                {workspaceSecondaryMenu}
                {systemMenu}
            </div>
        )
    }
}
