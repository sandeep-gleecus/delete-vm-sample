///// <reference path="typings/react.d.ts"/>
///// <reference path="typings/react-dom.d.ts"/>
//import * as React from 'react';
//import * as ReactDOM from 'react-dom';

//external dependendencies (js libraries)
declare var React: any;
declare var ReactDOM: any;
declare var $: any;

//inflectra services
declare var resx: any;
declare var globalFunctions: any;

//on click of global search button
var globalSearch = {
    msgBoxId: '' as string,
    searchKeywords: '' as string,
    currentProjectId: 0 as Number,
    searchResultData: [],
    searchInit: function (keywords, searchService) {
        
        globalSearch.searchKeywords = keywords;
        var pageIndex = 0,
            pageSize = 250,
            msgBoxId = this.msgBoxId;
        globalFunctions.display_spinner();
        searchService.RetrieveResults(
            keywords,
            pageIndex,
            pageSize,
            globalSearch.searchSuccess,
            globalSearch.searchFailure
        );
    },

    searchSuccess: function (res: any) {
        globalFunctions.hide_spinner();
        globalSearch.searchResultData = res.Values;
        //first unmount existing react component to make sure it is cleared
        globalFunctions.dlgGlobalDynamicClear();
        //next initiliaze the react component fresh
        globalSearch.showResults();
    },

    searchFailure: function (error) {
        globalFunctions.hide_spinner();
        //Display validation exceptions in a friendly manner
        var messageBox = document.getElementById(globalSearch.msgBoxId);
        globalFunctions.display_error(messageBox, error);
    },
    showResults: function () {
        ReactDOM.render(
            <SearchBox/>,
            document.getElementById('dlgGlobalDynamic')
        );
    }
};



class SearchBox extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);
        this.state = {
            data: globalSearch.searchResultData,
            keyword: globalSearch.searchKeywords,
            sortedByRank: true,
            artifactFilter: 0, // 0 represents all data
            projectFilter: 0,
            projectFilterList: [
                {
                    name: resx.Global_AllProjects,
                    id: 0,
                    count: globalSearch.searchResultData.length,
                    countFiltered: globalSearch.searchResultData.length
                },
                {
                    name: resx.Global_CurrentProject,
                    id: globalSearch.currentProjectId,
                    count: globalSearch.searchResultData.map(function (item) { return item.ProjectId }).filter(function (item) { return item === globalSearch.currentProjectId }).length,
                    countFiltered: 0
                }
            ],
            isOpen: false
        }

        // register functions
        this.sortListRank = this.sortListRank.bind(this);
        this.sortListDate = this.sortListDate.bind(this);
        this.filterArtifactListClick = this.filterArtifactListClick.bind(this);
        this.filterProjectListClick = this.filterProjectListClick.bind(this);
        this.closeClick = this.closeClick.bind(this);
    }

    componentWillMount() {
        this.setState({
            isOpen: true
        })
    }

    //custom event / click handlers
    sortListRank(e: any) {
        if (!this.state.sortedByRank) {
            this.setState({
                data: this.state.data.sort(function (a, b) {
                    return parseInt(b.Rank) - parseInt(a.Rank);
                }),
                sortedByRank: true
            })
        }
    }
    sortListDate(e) {
        if (this.state.sortedByRank) {
            this.setState({
                data: this.state.data.sort(function (a, b) {
                    return +new Date(b.LastUpdateDate) - +new Date(a.LastUpdateDate);
                }),
                sortedByRank: false
            })
        }
    }
    filterArtifactListClick (artifactFilter, isClickable, e) {
        if (isClickable && artifactFilter >= 0) {
            this.setState({
                artifactFilter: artifactFilter,
            });
        }
    }
    filterProjectListClick (projectFilter, isClickable, e) {
        if (isClickable && projectFilter >= 0) {
            this.setState({
                projectFilter: projectFilter,
            });
        }
    }
    closeClick (e) {
        //reset necessary react states
        this.setState({
            isOpen: e ? false : true
        });
        //reset globalSearch parameters
        globalSearch.searchKeywords = "";
        globalSearch.searchResultData.splice(0, globalSearch.searchResultData.length);
        //Unmount react component to allow proper reinitialisation
        globalFunctions.dlgGlobalDynamicClear();
    }

    render() {
        var resLength = this.state.data.length;
        var panelClasses = this.state.isOpen ? "u-popup u-popup_down w-75 is-open" : "u-popup u-popup_down w-75";
        var typesAll = { count: resLength, id: 0, name: resx.Global_AllArtifacts };
        var artifactFilter = this.state.artifactFilter,
            projectFilter = this.state.projectFilter;

        //generate list of relevant artifiacts, inc live update counts based on current project filter
        var returnedTypes = globalFunctions.listUniquesInArray(this.state.data, 'ArtifactTypeId'),
            returnedTypesCount = returnedTypes.length,
            returnedTypesLong = this.state.data.map(function (item) { return item.ArtifactTypeId }),
            returnedTypesFilteredByProject = this.state.data
                .filter(function (item) {
                    return projectFilter === 0 || projectFilter === item.ProjectId;
                })
                .map(function (item) { return item.ArtifactTypeId }),
            returnedTypesInfo = globalFunctions.filterArrayByList(globalFunctions.getArtifactTypes(), returnedTypes, 'id')
                .map(function (item) {
                    var count = 0,
                        countFiltered = 0;
                    for (var i = 0; i < resLength; i++) {
                        if (item.id === returnedTypesLong[i]) { count++ };
                        if (i < returnedTypesFilteredByProject.length) {
                            if (item.id === returnedTypesFilteredByProject[i]) { countFiltered++ };
                        }
                    }
                    item.count = count;
                    item.countFiltered = countFiltered;
                    return item;
                });
        returnedTypesInfo.unshift(typesAll);

        //generate project list for filters with live update counts based on current artifact filter
        var returnedTypesFilteredByArtifact = this.state.data
            .filter(function (item) {
                return artifactFilter === 0 || artifactFilter === item.ArtifactTypeId;
            })
            .map(function (item) { return item.ProjectId }),
            projectFilterList = this.state.projectFilterList.map(function (item) {
                if (item.id > 0) {
                    var countFiltered = 0;
                    for (var i = 0; i < returnedTypesFilteredByArtifact.length; i++) {
                        if (returnedTypesFilteredByArtifact[i] === item.id) {
                            countFiltered++;
                        }
                    }
                    item.countFiltered = countFiltered;  
                }
                return item;
            });

        //generate information about the currently selected filters
        var artifactFilterDetails = returnedTypesInfo.filter(function (item) {
            return item.id === artifactFilter;
        })[0];
        var projectFilterDetails = projectFilterList.filter(function (item) {
            return item.id === projectFilter;
        })[0];

        return (
            <div className={panelClasses}>
                <span onClick={this.closeClick} className="fas fa-times pointer fr pa4 mtn4 mrn4 fade70"></span>
                <Header 
                    keyword={this.state.keyword}
                    total={resLength}
                    sortedByRank={this.state.sortedByRank}
                    sortListRank={this.sortListRank}
                    sortListDate={this.sortListDate}
                />
                <FilterBox 
                    artifactFilter={this.state.artifactFilter}
                    artifactFilterDetails={artifactFilterDetails}
                    artifactsList={returnedTypesInfo}
                    filterArtifactListClick={this.filterArtifactListClick}
                    filterProjectListClick={this.filterProjectListClick}
                    projectFilter={this.state.projectFilter}
                    projectFilterDetails={projectFilterDetails}
                    projectFilterList={this.state.projectFilterList}
                />
                <ResultList 
                    data={this.state.data}
                    artifactFilter={this.state.artifactFilter}
                    projectFilter={this.state.projectFilter}
                />
            </div>
        )
    }
}

//Header component
interface HeaderProps {
    keyword: string;
    total: number;
    sortedByRank: boolean;
    sortListRank: boolean;
    sortListDate: boolean;
}
function Header(props) {
    var rankClass = props.sortedByRank ? 'btn btn-default active' : 'btn btn-default ',
        dateClass = props.sortedByRank ? 'btn btn-default ' : 'btn btn-default  active';
    return (
        <div>
            <h3 className="mt0 mr5 display-inline-block"><strong>"{props.keyword}"</strong>: {props.total} {resx.Global_Search_MatchingResults}</h3>
            <div className="display-inline-block">
                {resx.Global_Sort}: 
                <div className="btn-group mx4">
                    <a 
                        href="#" 
                        className={rankClass}
                        onClick={props.sortListRank}>{resx.Global_Search_SortByRelevance}</a>
                    <a 
                        href="#" 
                        className={dateClass}
                        onClick={props.sortListDate}>{resx.Global_Search_SortByDate}</a>
                </div>
            </div>
        </div>
    );
}

//FilterBox component
interface FilterBoxProps {
    artifactFilter: number;
    artifactFilterDetails: Object;
    artifactFilterListIsOpen: boolean;
    artifactsList: Object;
    filterArtifactListClick: any;
    filterProjectListClick: any;
    projectFilter: number;
    projectFilterDetails: Object;
    projectFilterList: Object;
    projectFilterListIsOpen: boolean;
}
function FilterBox(props) {
    var filterArtifactListClick = props.filterArtifactListClick,
        filterProjectListClick = props.filterProjectListClick,
        artifactFilter = props.artifactFilter,
        artifactFilterClass = props.artifactFilter != 0 ? "font-bold" : "",
        projectFilter = props.projectFilter,
        projectFilterClass = props.projectFilter != 0 ? "font-bold" : "";
    var filterArtifactNodes = props.artifactsList.map(function(item) {
        return (
            <li>
                <FilterItem
                clickFunction={filterArtifactListClick}
                countFiltered={item.countFiltered}
                countFull={item.count}
                filter={artifactFilter}
                id={item.id}
                key={item.id}
                name={item.name} />
            </li>
        )
    });
    var filterProjectNodes = props.projectFilterList.map(function (item) {
        return (
            <li>
                <FilterItem
                clickFunction={filterProjectListClick}
                countFiltered={item.countFiltered}
                countFull={item.count}
                filter={projectFilter}
                id={item.id}
                key={item.id}
                name={item.name} />
            </li>
        )
    });
    return (
        <div>{resx.Global_Filter}: 
            <div className="btn-group">
                <button type="button" className="btn btn-flat dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                {props.artifactFilterDetails.name} <span className="caret"></span>
                </button>
                <ul className="dropdown-menu">
                {filterArtifactNodes}
                </ul>
            </div>
            <div className="btn-group">
                <button type="button" className="btn btn-flat dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                {props.projectFilterDetails.name} <span className="caret"></span>
                </button>
                <ul className="dropdown-menu">
                {filterProjectNodes}
                </ul>
            </div>
        </div>
    );
}

//FilterItem component
interface FilterItemProps {
    classes: string;
    clickFunction: any;
    countFull: number;
    countFiltered: number;
    filter: number;
    id: number;
    name: string;
}
function FilterItem(props) {
    var isActive = props.filter === props.id,
        isNone = props.countFiltered === 0,
        isActiveClass = isActive ? " active" : "",
        isDisabled = isNone ? "disabled" : "",
        isClickable = !isActive && !isNone,
        showBadge = props.countFull ? true : false;
    var classes = (props.classes ? props.classes : "") + isActiveClass;
    return (
        <a
            href="#"
            className={classes}
            disabled={isDisabled}
            onClick={props.clickFunction.bind(null, props.id, isClickable) } >{props.name}
            {showBadge ?
                <span className="ml2 badge font-70">{props.countFull}</span>
                :
                null
            }
        </a>
    )
}

//ResultList component
interface ResultListProps {
    artifactFilter: number;
    data: Object;
}
function ResultList(props) {
    var artifactFilter = props.artifactFilter,
        projectFilter = props.projectFilter;
    var listNodes = props.data.map(function(item) {
        return (
            <ResultItem
                artifactFilter={artifactFilter}
                artifactTypeId={item.ArtifactTypeId}
                date={item.LastUpdateDate}
                description={item.Description}
                icon={item.Icon}
                key={item.Token}
                project={item.ProjectName}
                projectFilter={projectFilter}
                projectId={item.ProjectId}
                rank={item.Rank}
                title={item.Title}
                token={item.Token}
                type={item.IconAlt}
                url={item.Url}
            />
        )
    });
    return (
        <div className="scrollbox overflow-y mb4 py3 globalSearch-scrollbox">
            {listNodes}
        </div>
    );
}

//ResultItem component
interface ResultItemProps {
    artifactFilter: number;
    artifactTypeId: number;
    date: Date;
    description: string;
    icon: string;
    key: number;
    project: string;
    projectId: number;
    projectFilter: number;
    rank: number;
    title: string;
    token: string;
    type: string;
    url: string;

}
function ResultItem(props) {
    var showArtifact = props.artifactFilter === 0 || props.artifactFilter === props.artifactTypeId,
        showProject = props.projectFilter === 0 || props.projectFilter === props.projectId,
        classes = showArtifact && showProject ? "result-item" : "result-item hide";
    return (
        <div className={classes}>
            <p className="mb0">
                <a href={props.url} className="link-no-decoration"><strong>{props.token}</strong> - {props.title}</a>
                <span className="badge bg-palegray ml4"> {props.project}</span>
            </p>
            <p className="mb0">{props.description}</p>
            <p className="small fade70">{props.date}</p>
        </div>
    )
}
