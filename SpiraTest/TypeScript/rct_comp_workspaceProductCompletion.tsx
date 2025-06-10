declare var React: any;
declare var ReactDOM: any;
declare var c3: any;
declare var gantt: any;

class WorkspaceProductCompletion extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);

        this.lateScheduleBar = SpiraContext.dashboard.scheduleBars.filter(x => x.key == SpiraContext.dashboard.schedule.late.enum);

        // the items immediately set below could be passed in as props as they do not change during use, but it seemed simpler to have them only set once, rather than in all user controls
        this.state = {
            childWorkspaces: null, // the workspaces that the widget displays - they are the children of the workspace of the dashboard as a whole (eg products if on the program dashboard)
            daysDoneAsCount: null, // array of arrays of all datapoints (as days) across all categories for c3 chart 
            daysDoneAsPercent: null, // array of arrays of all datapoints (as %age) across all categories for c3 chart 
            daysDoneColorLate: this.lateScheduleBar && this.lateScheduleBar.length ? this.lateScheduleBar[0].color : null, // the late color which is needed for a color calculation in render
            daysDoneColors: SpiraContext.dashboard.scheduleBars.reduce((a, b) => { a[b.label] = b.color; return a; }, {}), // array of the colors used for this chart
            daysDoneGroups: [SpiraContext.dashboard.scheduleBars.map(x => x.label)], // array of arrays of name strings for each category (late, done, etc) - used for data matching, legend, tooltips
            daysDoneLabels: null, // the names of the child workspaces
            daysDoneToDisplay: null, // this switches between rqsDoneAsPercent and rqsDoneAsCount based on user toggle
            isDisplayingPercent: true, // toggle flag for the entire chart - true = show percent, false = show numbers
            rqsDoneAsCount: null, // array of arrays of all datapoints (as requirement numbers) across all categories for c3 chart 
            rqsDoneAsPercent: null, // array of arrays of all datapoints (as %age) across all categories for c3 chart 
            rqsDoneColors: SpiraContext.dashboard.completionBars.reduce((a, b) => { a[b.label] = b.color; return a; }, {}), // array of the colors used for this chart
            rqsDoneGroups: [SpiraContext.dashboard.completionBars.map(x => x.label)], // array of arrays of name strings for each category (in progress, remaining) - used for data matching, legend, tooltips
            rqsDoneLabels: null, // the names of the child workspaces (identical to that for days)
            rqsDoneToDisplay: null, // this switches between rqsDoneAsPercent and rqsDoneAsCount based on user toggle
        };

        this.daysDoneSet = this.daysDoneSet.bind(this);
        this.toggleDisplayType = this.toggleDisplayType.bind(this);

        // some of the schedule objects on dashboard.master have info used to calculate if an inprogress workspace is ahead/behind/on schedule. These are filtered here so does not have to be recalculated in relevant functions
        // not in state as only used by internal functions
        this.inProgressScheduleBars = SpiraContext.dashboard.scheduleBars.filter(x => x.lowerThreshold != undefined);
    }

    componentDidMount() {
        // the child workspace data is the first thing set
        // at the end of this function are callbacks to dependent functions for setting the actual chart data for rq completion and schedule status
        this.childWorkspaceSetOverdueData();
    }

    // go through all workspaces and determine if each item is overdue. If any child is overdue, then the parent must be overdue too
    childWorkspaceSetOverdueData() {
        const today = new Date();

        let updatedSprints: Array<any>,
            updatedReleases: Array<any>,
            updatedProducts: Array<any>,
            updatedPrograms: Array<any>,
            updatedPortfolios: Array<any>,
            childWorkspaces: Array<any>;

        // do lowest level first then feed that data into the next level up (if it exists), and so on - to make sure we have accurate overdue data
        if (this.props.sprints && this.props.sprints.length) {
            updatedSprints = this.props.sprints.map(r => this.overdueWorkspaceSet(r, today, null));
        }
        if (this.props.releases && this.props.releases.length) {
            updatedReleases = this.props.releases.map(r => {
                const children = updatedSprints && updatedSprints.length ? updatedSprints.filter(x => x.parentId == r.workspaceId) : null;
                return this.overdueWorkspaceSet(r, today, children)
            });
        }
        if (this.props.products && this.props.products.length) {
            updatedProducts = this.props.products.map(pr => {
                const children = updatedReleases && updatedReleases.length ? updatedReleases.filter(x => x.parentId == pr.workspaceId) : null;
                return this.overdueWorkspaceSet(pr, today, children)
            });
        }
        if (this.props.programs && this.props.programs.length) {
            updatedPrograms = this.props.programs.map(pg => {
                const children = updatedProducts && updatedProducts.length ? updatedProducts.filter(x => x.parentId == pg.workspaceId) : null;
                return this.overdueWorkspaceSet(pg, today, children)
            });
        }
        if (this.props.portfolios && this.props.portfolios.length) {
            updatedPortfolios = this.props.portfolios.map(pf => {
                const children = updatedPrograms && updatedPrograms.length ? updatedPrograms.filter(x => x.parentId == pf.workspaceId) : null;
                return this.overdueWorkspaceSet(pf, today, children)
            });
        }

        // now we need find the specific workspace array that we need for the widget
        switch (this.props.workspaceType) {
            case this.props.workspaceEnums.enterprise:
                childWorkspaces = updatedPortfolios;
                break;
            case this.props.workspaceEnums.portfolio:
                childWorkspaces = updatedPrograms;
                break;
            case this.props.workspaceEnums.program:
                childWorkspaces = updatedProducts;
                break;
            case this.props.workspaceEnums.product:
                childWorkspaces = updatedReleases;
                break;
        }

        // should we show sprints? Only if: we are on the product dashboard; the display sprints flag is passed in; and we have sprints to show
        if (this.props.showSprints && this.props.workspaceType === this.props.workspaceEnums.product && updatedSprints && updatedSprints.length) {
            childWorkspaces = [];
            let residualSprints = updatedSprints;
            // go through each release (if we have any) and add its sprints directly after it in the array
            if (updatedReleases && updatedReleases.length) {
                updatedReleases.forEach(r => {
                    // first look for matching children
                    const children = residualSprints.filter(x => x.parentId == r.workspaceId);
                    // then add items to the array in the correct order
                    childWorkspaces.push(r);
                    if (children && children.length) {
                        // remove any sprints already matched - to make the next iteration faster, and so we know which sprints are orphaned
                        residualSprints = residualSprints.filter(x => x.parentId != r.workspaceId)
                        childWorkspaces.push(...children);
                    }
                });
            }
            // if there is a case where a sprint is orphaned / belongs to the main release being displayed for, add it to the end of the list
            if (residualSprints && residualSprints.length) {
                childWorkspaces.push(...residualSprints);
            }
        }

        // if we are only showing workspaces that have requirements filter the array now
        if (this.props.hideEmptyWorkspaces && childWorkspaces && childWorkspaces.length > 0) {
            childWorkspaces = childWorkspaces.filter(x => x.requirementsAll > 0);
        }

        // if we are only showing workspaces that have real ids (>0) filter the array now - used to filter out the "No Portfolio" which has an id of -1.
        if (this.props.hideEmptyVirtualWorkspaces && childWorkspaces && childWorkspaces.length > 0) {
            // include all workspaces with positive ids and only those with negative ids that also have requirements in them
            childWorkspaces = childWorkspaces.filter(x => x.workspaceId > 0 || (x.requirementsAll > 0 && x.workspaceId < 0));
        }

        const workspaceLabels = childWorkspaces && childWorkspaces.length ? childWorkspaces.map(x => x.workspaceName) : [];

        // update state with the workspace information
        this.setState({
            childWorkspaces: childWorkspaces,
            daysDoneLabels: workspaceLabels,
            rqsDoneLabels: workspaceLabels

        }, () => {
            // the above data is required for the rest of the widget to function, so they are run as callbacks - ie after setState has finished
            this.rqsDoneSet();
            this.daysDoneSet();
        });

    }

    // works out whether an item is overdue - used for items that should have children (ie products, programs, portfolios)
    // param: item - object of the individual item
    // param: today - date 
    // param: children - array of item objects (nullable)
    overdueWorkspaceSet(item, today, children) {
        // set initual variables
        let newItem = { ...item }; // create a copy of the original workspace item passed in
        const endDate = globalFunctions.parseJsonDate(item.endDate),
            startDate = globalFunctions.parseJsonDate(item.startDate),
            daysAllocated = Math.floor((endDate - startDate) / (1000 * 3600 * 24)),
            childOverdue = children ? children.filter(c => c.parentId == item.workspaceId).filter(c => c.status == SpiraContext.dashboard.schedule.late.enum) : null;

        // these variables are set here initially and calculated below, based on which state the item is in
        let status = "notSet";
        let daysPercentComplete = 0;

        // we need to determine the correct state of a workspace based off of the inputs - then assign its status and daysPercentComplete
        // workspace = not started if its start date is in the future (its days percent complete remains 0)
        if (startDate > today) {
            status = SpiraContext.dashboard.schedule.notStarted.enum;

        // workspace = complete if its rq completion is at 100 - days percent complete is also 100
        } else if (item.percentComplete == 100) {
            daysPercentComplete = 100;
            status = SpiraContext.dashboard.schedule.complete.enum;

        // workspace = late if either any of its descendants are late OR if it is late (ie its end date is in the past - implicitly to get here rq completion is under 100% which is also required)
        } else if ((childOverdue && childOverdue.length) || endDate < today ? true : false) {
            const timeOverdue = today - endDate;
            // if the item itself is late, then days percent complete is the time from start to today as a proportion of the original planned length
            if (timeOverdue > 0) {
                const daysOverdue = Math.floor(timeOverdue / (1000 * 3600 * 24));
                daysPercentComplete = Math.floor((daysAllocated + daysOverdue) / daysAllocated * 100);
            // if only a child is late, then we calculate the days percent complete in the same way as for in progress workspaces
            } else {
                const daysSinceStart = Math.floor((today - startDate) / (1000 * 3600 * 24));
                daysPercentComplete = Math.floor(daysSinceStart / daysAllocated * 100);
            }
            status = SpiraContext.dashboard.schedule.late.enum;

        // in all other cases, the workspace is in progress. Days percent complete is the time between today and the start date as %age
        // the status is more complicated here as it can be either ahead/behind/on schedule. 
        } else {
            const daysSinceStart = Math.floor((today - startDate) / (1000 * 3600 * 24));
            daysPercentComplete = Math.floor(daysSinceStart / daysAllocated * 100);

            // the in progress status is based on the percentage gap between the schedule and requirement completion percentages
            // the data for the bands that determine which status to used is stored globally - and originally comes from dashboard.master
            const rqsVsDays = item.percentComplete - daysPercentComplete;
            const thresholds = this.getHighestScheduleMatch(this.inProgressScheduleBars, rqsVsDays, "lowerThreshold")
            status = thresholds && thresholds.key ? thresholds.key : null;
        }
        // add the required fields to the workspace item and then return it
        newItem.status = status;
        newItem.daysAllocated = daysAllocated;
        newItem.daysPercentComplete = daysPercentComplete;
        return newItem;
    }

    // returns an object of required result for the legend - the obj where the specified key has the lowest value in the legends array but it still greater than the passed in value
    // param: legends = array of objects
    // param: value = number to check against
    // param: key = key in the object to check against the value
    getHighestScheduleMatch(legends, value, key) {
        // first find all matches where the key is greater than the value, then return the object with the lowest key
        const legendsArray = legends && legends.length ? legends.filter(x => x[key] < value) : null;
        const highestLegend = legendsArray && legendsArray.length ? legendsArray.reduce((previous, current) => (previous[key] > current[key]) ? previous : current) : null;

        return highestLegend || null;
    }

    // this has to be run after the overdue data has all been set in state and has to be run against state
    // creates the array or arrays for both chart display types (numbers and percent)
    // each main array contains one array for each chart category (eg late). That array has its label/name, then one value per workspace. If the value should not be rendered, null is passed in, otherwise the correct number
    // example array for percent (total can exceed 100 for late items): [ ["on schedule",75,null,42], ["completed",null,100,null], ["remaining",25,null,58] ]
    // takes the child workspaces as input
    // output = updating state with the array of arrays for c3 to render
    daysDoneSet() {
        // small helper function to take a percentage complete and the total number and turn it into a number that are complete (eg 52% of 10 spits out 5)
        const percentToNumbers = (percent, number) => Math.round(percent * number / 100);

        // only carry out function if workspaces are present in state
        if (this.state.childWorkspaces && this.state.childWorkspaces.length) {
            // create the percent arrays - one sub array for each category / group to render in the chart
            const outputPercent = [
                // the complete array is special as we set it to 100% both if the status of the workspace is complete OR late (and has a percent complete of > 100% - this means the item itself is late, not just one of its children)
                // we use this array for late items so that we can show more clearly that the workspace itself is running late
                [SpiraContext.dashboard.schedule.complete.label, ...this.state.childWorkspaces.map(x => {
                    if (x.status == SpiraContext.dashboard.schedule.complete.enum) {
                        return x.daysPercentComplete >= 100 ? 100 : x.daysPercentComplete;
                    } else if (x.status == SpiraContext.dashboard.schedule.late.enum && x.daysPercentComplete > 100) {
                        return 100;
                    } else {
                        return null;
                    }
                })],
                // for each of the categories below, add the percent number if the status matches the one for this category, otherwise add a null
                [SpiraContext.dashboard.schedule.aheadOfSchedule.label, ...this.state.childWorkspaces.map(x =>
                    x.status == SpiraContext.dashboard.schedule.aheadOfSchedule.enum ? x.daysPercentComplete : null
                )],
                [SpiraContext.dashboard.schedule.onSchedule.label, ...this.state.childWorkspaces.map(x =>
                    x.status == SpiraContext.dashboard.schedule.onSchedule.enum ? x.daysPercentComplete : null
                )],
                [SpiraContext.dashboard.schedule.behindSchedule.label, ...this.state.childWorkspaces.map(x =>
                    x.status == SpiraContext.dashboard.schedule.behindSchedule.enum ? x.daysPercentComplete : null
                )],
                [SpiraContext.dashboard.schedule.late.label, ...this.state.childWorkspaces.map(x => {
                    if (x.status == SpiraContext.dashboard.schedule.late.enum) {
                        // a workspace will be less than 100% done but still late when it is shown as late because a child of it is late - this line handles that case
                        return x.daysPercentComplete > 100 ? x.daysPercentComplete - 100 : x.daysPercentComplete;
                    } else {
                        return null;
                    }
                })],
                // for remaining, we need to check that percent complete is less than 100 (ie it is not complete or the workspace itself is not late). The %age complete is the inverse of the actual % complete
                [SpiraContext.dashboard.schedule.remaining.label, ...this.state.childWorkspaces.map(x => 
                    x.daysPercentComplete >= 100  ? null : 100 - x.daysPercentComplete
                )],
            ];

            // create the number of days arrays - one sub array for each category / group to render in the chart
            const outputNumbers = [
                // the complete array is special as we set it to the total daysAllocated both if the status of the workspace is complete OR late (and has a percent complete of > 100% - this means the item itself is late, not just one of its children)
                // we use this array for late items so that we can show more clearly that the workspace itself is running late
                [SpiraContext.dashboard.schedule.complete.label, ...this.state.childWorkspaces.map(x => {
                    if (x.status == SpiraContext.dashboard.schedule.complete.enum) {
                        return x.daysPercentComplete >= 100 ? x.daysAllocated : x.daysPercentComplete;
                    } else if (x.status == SpiraContext.dashboard.schedule.late.enum && x.daysPercentComplete > 100) {
                        return x.daysAllocated;
                    } else {
                        return null;
                    }
                })],
                // for each of the categories below, add the days since the workspace started till now if the status matches the one for this category, otherwise add a null
                [SpiraContext.dashboard.schedule.aheadOfSchedule.label, ...this.state.childWorkspaces.map(x =>
                    x.status == SpiraContext.dashboard.schedule.aheadOfSchedule.enum ? percentToNumbers(x.daysPercentComplete, x.daysAllocated) : null
                )],
                [SpiraContext.dashboard.schedule.onSchedule.label, ...this.state.childWorkspaces.map(x =>
                    x.status == SpiraContext.dashboard.schedule.onSchedule.enum ? percentToNumbers(x.daysPercentComplete, x.daysAllocated) : null
                )],
                [SpiraContext.dashboard.schedule.behindSchedule.label, ...this.state.childWorkspaces.map(x =>
                    x.status == SpiraContext.dashboard.schedule.behindSchedule.enum ? percentToNumbers(x.daysPercentComplete, x.daysAllocated) : null
                )],
                [SpiraContext.dashboard.schedule.late.label, ...this.state.childWorkspaces.map(x => {
                    if (x.status == SpiraContext.dashboard.schedule.late.enum) {
                        // a workspace will be less than 100% done but still late when it is shown as late because a child of it is late - this line handles that case
                        return x.daysPercentComplete > 100 ? percentToNumbers((x.daysPercentComplete - 100), x.daysAllocated) : percentToNumbers(x.daysPercentComplete, x.daysAllocated);
                    } else {
                        return null;
                    }
                })],
                // for remaining, we need to check that percent complete is less than 100 (ie it is not complete or the workspace itself is not late). The output is the inverse of the actual days complete complete
                [SpiraContext.dashboard.schedule.remaining.label, ...this.state.childWorkspaces.map(x =>
                    x.daysPercentComplete >= 100 ? null : x.daysAllocated - percentToNumbers(x.daysPercentComplete, x.daysAllocated)
                )],
            ];

            // finally update state
            this.setState({
                daysDoneAsPercent: outputPercent,
                daysDoneAsCount: outputNumbers,
                daysDoneToDisplay: this.state.daysDoneToDisplay ? this.state.daysDoneToDisplay : outputPercent,
            })
        }
    }

    // Creates the array datasets for the chart of #RQ completion - for numbers and %ages
    // It gets the array for the child workspaces of the current workspace (if present) to create the complete/incomplete arrays for the chart
    // each main array contains one array for each chart category (eg in progress). That array has its label/name, then one value per workspace. If the value should not be rendered, null is passed in, otherwise the correct number
    // example array for percent (total does not exceed 100): [ ["inProgress",75,null,42], ["completed",null,100,null], ["remaining",25,null,58] ]
    // takes the child workspaces as input
    // output = updating state with the array of arrays for c3 to render
    rqsDoneSet() {
        // small helper function to take a percentage complete and the total number and turn it into a number that are complete (eg 52% of 10 spits out 5)
        const percentToNumbers = (percent, number) => Math.round(percent * number / 100);

        // only carry out function if workspaces are present in state
        if (this.state.childWorkspaces && this.state.childWorkspaces.length) {
            // for each of the categories, for each workspace set it to null if it should not be rendered or its number
            // set inProgress to the actual number/percent - if workspace is not complete
            // set complete to 100 if workspace is complete
            // set remaining to all minus actual, if the workspace is not complete
            const outputPercent = [
                [SpiraContext.dashboard.completions.inProgress.label, ...this.state.childWorkspaces.map(x => x.percentComplete < 100 ? x.percentComplete : null)],
                [SpiraContext.dashboard.completions.complete.label, ...this.state.childWorkspaces.map(x => x.percentComplete == 100 ? 100 : null)],
                [SpiraContext.dashboard.completions.remaining.label, ...this.state.childWorkspaces.map(x => x.percentComplete == 100 ? null : 100 - x.percentComplete)],
            ];
            // repeat the above for numbers as for percent
            const outputNumbers = [
                [SpiraContext.dashboard.completions.inProgress.label, ...this.state.childWorkspaces.map(x => x.percentComplete < 100 ? percentToNumbers(x.percentComplete, x.requirementsAll) : null)],
                [SpiraContext.dashboard.completions.complete.label, ...this.state.childWorkspaces.map(x => x.percentComplete == 100 ? percentToNumbers(100, x.requirementsAll) : null)],
                [SpiraContext.dashboard.completions.remaining.label, ...this.state.childWorkspaces.map(x => x.percentComplete == 100 ? null : x.requirementsAll - percentToNumbers(x.percentComplete, x.requirementsAll))],
            ];

            // finally update state
            this.setState({
                rqsDoneAsPercent: outputPercent,
                rqsDoneAsCount: outputNumbers,
                rqsDoneToDisplay: this.state.rqsDoneToDisplay ? this.state.rqsDoneToDisplay : outputPercent,
            })
        }
    }

    // toggles the display from showing percentages (where every bar has the same total) to actual numbers (to compare relative sizes)
    toggleDisplayType() {
        this.setState({
            isDisplayingPercent: !this.state.isDisplayingPercent,
            rqsDoneToDisplay: !this.state.isDisplayingPercent ? this.state.rqsDoneAsPercent : this.state.rqsDoneAsCount,
            daysDoneToDisplay: !this.state.isDisplayingPercent ? this.state.daysDoneAsPercent : this.state.daysDoneAsCount
        });
    }

    render() {
        return (
            <React.Fragment>
                {this.state.childWorkspaces && this.state.childWorkspaces.length > 0 ? 
                    <div className="dib v-top mb5 mx0 w-100">
                        <label className="fw-b mr2">
                            {resx.Global_Displaying}
                        </label>
                        <button
                            className="btn btn-default"
                            onClick={this.toggleDisplayType}
                            type="button"
                            >
                            {this.state.isDisplayingPercent ? resx.Dashboard_Completion_ShowAsPercent : resx.Dashboard_Completion_ShowAsNumbers }
                        </button>

                        <div className="flex flex-wrap mt3">
                            <div className="w-60 mt3">
                                <h4 className="fw-b tc my0">{resx.Dashboard_Completion_ScheduleTitle}</h4>
                                <ReactC3Chart
                                    axis={{
                                        rotated: true,
                                        y: { show: false },
                                        x: {
                                            show: true,
                                            type: 'category',
                                            categories: this.state.daysDoneLabels
                                        }
                                    }}
                                    bar={{ width: 32 }}
                                    data={{
                                        colors: this.state.daysDoneColors, // the default colors for each category/group
                                        // we have to actively set the remaining color to null here as it already has a color assigned to it by c3 that we need to 
                                        color: (color, d) => {
                                            // set the color to null (so that css handles it) for remaining items
                                            if (d.id && d.id == SpiraContext.dashboard.schedule.remaining.label) {
                                                return null;
                                                // for the complete category, we may need to change the color 
                                            } else if (d.id && d.id == SpiraContext.dashboard.schedule.complete.label) {
                                                // use a special color for late items
                                                // theoretically we should specify to do this only for items that are themselves late (ie not for items who have late children)
                                                // however we do not need to do this as the "complete" value will be null for items who have a late child
                                                if (this.state.childWorkspaces[d.index].status == SpiraContext.dashboard.schedule.late.enum) {
                                                    return this.state.daysDoneColorLate ? d3.rgb(this.state.daysDoneColorLate).darker(1) : color;
                                                } else {
                                                    return color;
                                                }
                                                // in all other cases use the color prescribed in data.colors above
                                            } else {
                                                return color;
                                            }
                                        },
                                        columns: this.state.daysDoneToDisplay,
                                        groups: this.state.daysDoneGroups,
                                        labels: {
                                            // show labels for all categories except remaining - which are never shown
                                            format: {
                                                // if an item is marked as not started we show a specific label explaining that
                                                [SpiraContext.dashboard.schedule.complete.label]: (v, id, i, j) => v > 0 ? v : "",
                                                [SpiraContext.dashboard.schedule.late.label]: (v, id, i, j) => v > 0 ? v : "",
                                                [SpiraContext.dashboard.schedule.aheadOfSchedule.label]: (v, id, i, j) => v > 0 ? v : "",
                                                [SpiraContext.dashboard.schedule.onSchedule.label]: (v, id, i, j) => {
                                                    if (v > 0) {
                                                        return v;
                                                    } else if (!this.state.childWorkspaces[i].daysPercentComplete) {
                                                        return SpiraContext.dashboard.schedule.notStarted.label;
                                                    } else {
                                                        return "";
                                                    }
                                                },
                                                [SpiraContext.dashboard.schedule.behindSchedule.label]: (v, id, i, j) => v > 0 ? v : ""
                                            }
                                        },
                                        order: null,
                                        type: "bar",
                                    }}
                                    dataSource={this.state.isDisplayingPercent}
                                    size={{
                                        height: this.state.daysDoneLabels ? (this.state.daysDoneLabels.length * 50) : 0
                                    }}
                                    legend={{
                                        show: false,
                                    }}
                                />

                                <div className="mt4 flex flex-wrap px5">
                                    {SpiraContext.dashboard.scheduleBars.filter(x => x.showInLegend).map(x => (
                                        <div
                                            className="mx2 fs-75 br3 px3 py2 mt3 mr3 white nowrap"
                                            style={{ backgroundColor: x.color }}
                                            title={x.tooltip}
                                            >
                                            {x.label}
                                        </div>
                                    ))}
                                </div>
                            </div>
                
                            <div className="w-40 mt3 pl2">
                                <h4 className="fw-b tc my0">{resx.Dashboard_Completion_RequirementsTitle}</h4>
                                <ReactC3Chart
                                    axis={{
                                        rotated: true,
                                        y: { show: false },
                                        x: {
                                            show: false,
                                            type: 'category',
                                            categories: this.state.rqsDoneLabels
                                        }
                                    }}
                                    bar={{ width: 32 }}
                                    data={{
                                        colors: this.state.rqsDoneColors,
                                        // we have to actively set the remaining color to null here as it already has a color assigned to it by c3 that we need to 
                                        color: (color, d) => {
                                            return d.id && d.id == SpiraContext.dashboard.completions.remaining.label ? null : color
                                        },
                                        columns: this.state.rqsDoneToDisplay,
                                        groups: this.state.rqsDoneGroups,
                                        labels: {
                                            // show labels for all categories except remaining - which are never shown
                                            format: {
                                                // for in progress, we show a special "none" label if there are no requirements in the workspace
                                                [SpiraContext.dashboard.completions.inProgress.label]: (v, id, i, j) => {
                                                    return this.state.childWorkspaces[i].requirementsAll ? v : resx.Global_None;
                                                },
                                                [SpiraContext.dashboard.completions.complete.label]: (v, id, i, j) => v > 0 ? v : "",
                                            },
                                        },
                                        order: null,
                                        type: "bar"
                                    }}
                                    dataSource={this.state.isDisplayingPercent}
                                    size={{
                                        height: this.state.rqsDoneLabels ? (this.state.rqsDoneLabels.length * 50) : 0
                                    }}
                                    legend={{
                                        show: false,
                                    }}
                                />


                                <div className="mt4 flex flex-wrap pr5">
                                    {SpiraContext.dashboard.completionBars.filter(x => x.gaugeThreshold !== undefined).map(x => (
                                        <div
                                            className="mx2 fs-75 br3 px3 py2 mt3 mr3 white nowrap"
                                            style={{ backgroundColor: x.color }}
                                            >
                                            {x.label}
                                        </div>
                                    ))}
                                </div>
                            </div>
                        </div>
                    </div>
                    :
                    <div
                        className="ma4 alert alert-info"
                        role="alert"
                        >
                        {resx.Global_NoDataToDisplay}
                    </div>
                }
            </React.Fragment>
        );
    }
}
