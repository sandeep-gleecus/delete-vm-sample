declare var React: any;
declare var ReactDOM: any;
declare var c3: any;
declare var gantt: any;

class WorkspaceSchedule extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);

        this.state = {
            ganttData: null,
            zoomToFit: (this.props.fullSize) ? false : true
        };
        this.DEFAULT_WORKSPACE_ID = -1;
        this.ROOT_ID = this.props.workspaceType + "-" + this.props.workspace.workspaceId;
    }

    componentDidMount() {
        this.ganttSet();
    }

    // utility function that goes through an object and returns the key that matches a value
    // param: object to search through
    // param: value to look for
    getKeyByValue(object, value) {
        for (var prop in object) {
            if (object.hasOwnProperty(prop)) {
                if (object[prop] === value)
                    return prop;
            }
        }
    }


    // returns the enum id of the workspace type that is the parent of the type being queried
    // param: enum of the workspace type you want to find the parent type of
    parentTypeIdGet(workspaceType) {
        switch (workspaceType) {
            case this.props.workspaceEnums.portfolio:
                return this.props.workspaceEnums.enterprise;
            case this.props.workspaceEnums.program:
                return this.props.workspaceEnums.portfolio;
            case this.props.workspaceEnums.product:
                return this.props.workspaceEnums.program;
            case this.props.workspaceEnums.release:
            case this.props.workspaceEnums.sprint:
                return this.props.workspaceEnums.product;
            case this.props.workspaceEnums.releaseTask:
                return this.props.workspaceEnums.release;
            case this.props.workspaceEnums.sprintTask:
                return this.props.workspaceEnums.sprint;
            default:
                return null;
        }
    }

    // takes the raw data and creates info needed to pass to the gantt component in the right format
    ganttSet() {
        // add all available data in props - filter out any items that do not have both a start and end date
        const updatedPortfolios = this.props.portfolios ? this.props.portfolios
            .filter(x => this.entryisValid(x))
            .map(x => this.ganttEntryCreate(x, this.props.workspaceEnums.portfolio)) : null;
        const updatedPrograms = this.props.programs ? this.props.programs
            .filter(x => this.entryisValid(x))
            .map(x => this.ganttEntryCreate(x, this.props.workspaceEnums.program)) : null;
        const updatedProducts = this.props.products ? this.props.products
            .filter(x => this.entryisValid(x))
            .map(x => this.ganttEntryCreate(x, this.props.workspaceEnums.product)) : null;
        const updatedReleases = this.props.releases ? this.props.releases
            .filter(x => this.entryisValid(x))
            .map(x => this.ganttEntryCreate(x, this.props.workspaceEnums.release)) : null;
        const updatedSprints = this.props.sprints ? this.props.sprints
            .filter(x => this.entryisValid(x))
            .map(x => this.ganttEntryCreate(x, this.props.workspaceEnums.sprint)) : null;
        const updatedSprintTasks = this.props.sprintTasks ? this.props.sprintTasks
            .filter(x => this.entryisValid(x))
            .map(x => this.ganttEntryCreate(x, this.props.workspaceEnums.sprintTask)) : null;
        const updatedReleaseTasks = this.props.releaseTasks ? this.props.releaseTasks
            .filter(x => this.entryisValid(x))
            .map(x => this.ganttEntryCreate(x, this.props.workspaceEnums.releaseTask)) : null;

        // create the array with all relevant data in it
        let output = [];
        if (updatedPortfolios) output.push(...updatedPortfolios);
        if (updatedPrograms) output.push(...updatedPrograms);
        if (updatedProducts) output.push(...updatedProducts);
        if (updatedReleases) output.push(...updatedReleases);
        if (updatedSprints) output.push(...updatedSprints);
        if (updatedSprintTasks) output.push(...updatedSprintTasks);
        if (updatedReleaseTasks) output.push(...updatedReleaseTasks);

        // make any final edits to the array
        const ganttData = {
            data: output.map(x => {
                // if a task entry has a parent set but that parent is not in the list and is not the root ID then we need to update the parent to the root id so that the entry will display
                const parentIsRoot = x.parent == this.ROOT_ID;
                const parentInList = output.map(y => y.id).indexOf(x.parent) >= 0;
                if (!parentIsRoot && !parentInList) {
                    x.parent = this.ROOT_ID;
                }
                return x;
            })
        };
        this.setState({ ganttData: ganttData });
    }

    // creates a single object to add to an array of gantt objects
    ganttEntryCreate(entry, workspaceEnum) {
        // if there is no parent sent on the entry then we add it to the default entry for that parent type 
        const parentIdToUse = typeof entry.parentId != "undefined" ? entry.parentId : this.DEFAULT_WORKSPACE_ID;
        // sprints are special: their normal parent is a release (which is actually the same artifact type though it is handled differently here because of how the data is sent and to make sure the right icons are displayed
        // so when a sprint has parentTypeToUse = true, then it's parent is actually a release (you can't have a sprint be a parent of another sprint)
        // when parentTypeToUse = false, then, as with releases, its parent is the product
        const parentTypeToUse = !entry.parentIsSameType ? this.parentTypeIdGet(workspaceEnum) : (workspaceEnum === this.props.workspaceEnums.sprint ? this.props.workspaceEnums.release : workspaceEnum);
        const workspaceEnumValue = this.getKeyByValue(this.props.workspaceEnums, workspaceEnum);

        // we get the workspace URL so that you can click through on the task to the item itself.
        // for non real workspaces (ie releases and tasks) we need to send in a product ID. We don't know this if on a program, portfolio, or enterprise
        // this means we can't set the product id fully programmatically. 
        // instead we set it in the props on the relevant user controls (those that are at the product level)
        const url = entry.workspaceId > 0 ? globalFunctions.getWorkspaceDefaultUrl(SpiraContext.BaseUrl, workspaceEnum, entry.workspaceId, this.props.productId || null) : "";
        return {
            end_date: globalFunctions.parseJsonDate(entry.endDate),
            iconClass: workspaceEnumValue,
            workspaceEnum: workspaceEnum,
            id: workspaceEnum + "-" + entry.workspaceId,
            parent: parentTypeToUse + "-" + parentIdToUse,
            progress: entry.percentComplete / 100,
            readonly: true,
            start_date: globalFunctions.parseJsonDate(entry.startDate),
            taskBgClass: "bg-" + workspaceEnumValue,
			text: url ? `<a class="transition-all" href="${url}">${globalFunctions.htmlEncode(entry.workspaceName)}</a>` : globalFunctions.htmlEncode(entry.workspaceName),
        }
    }

    // helper function to check validity of an entry that it will render correctly in the gantt chart - dates have to exist and be valid (ie later than 1/1/1970)
    entryisValid(entry) {
        if (entry) {
            return entry.startDate && globalFunctions.parseJsonDate(entry.startDate) > 0 && entry.endDate && globalFunctions.parseJsonDate(entry.endDate) > 0;
        } else {
            return false;
        }
    }

    render() {
        const heightClass = this.props.heightClass || "h10";
        return (
            <div className={`gantt-container ${heightClass}`}>
                {this.state.ganttData && this.state.ganttData.data && this.state.ganttData.data.length > 0 ?
                    <ReactGantt
                        tasks={this.state.ganttData}
                        isCustomTaskClass={this.props.isCustomTaskClass}
                        isLightboxEnabled={this.props.isLightboxEnabled || false}
                        config={{
                            columns: [
                                { name: "text", label: resx.Global_Workspaces, width: "*", tree: true },
                            ],
                            readonly: this.props.isEditable ? false : true,
                            root_id: this.ROOT_ID,
                            xml_date: "%Y-%m-%d %H:%i",
                        }}
                        templates={{
                            task_class: (start, end, task) => task.taskBgClass || ""
                        }}
                        zoomToFit={this.state.zoomToFit}
                    />
                    :
                    <div 
                        className="ma4 alert alert-info"
                        role="alert"
                        >
                        {resx.Global_NoDataToDisplay}
                    </div>
                }
            </div>
        );
    }
}
