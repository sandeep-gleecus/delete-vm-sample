// A '.tsx' file enables JSX support in the TypeScript compiler, 
// for more information see the following page on the TypeScript wiki:
// https://github.com/Microsoft/TypeScript/wiki/JSX

//external dependendencies (js libraries)
declare var React: any;			//React
declare var ReactDOM: any;		//React
declare var $: any;
declare var $find: any;

//inflectra services
declare var globalFunctions: any;
declare var SpiraContext: any;
declare var Inflectra: any;

var resx = Inflectra.SpiraTest.Web.GlobalResources;

class OnBoarding extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);
        this.state = {
            seenAppIntro: this.props.seenAppIntro,
            seenUpdate: this.props.seenUpdate,
            showTour: false,
            tour: null,
            tourStepTitles: [],
            tourIndex: 0

        }
        this.handleClose = this.handleClose.bind(this);
        this.tourStart = this.tourStart.bind(this);
        this.tourStop = this.tourStop.bind(this);
        this.tourStopAndClosePanel = this.tourStopAndClosePanel.bind(this);
        this.tourChangeIndex = this.tourChangeIndex.bind(this);
    }

    //functions
    handleClose(e) {
        //Unmount react component to allow proper reinitialisation
        globalFunctions.dlgGlobalDynamicClear();
    }

    tourStopAndClosePanel(tourName) {
        this.tourStop(tourName);
        this.handleClose(null)
    }
    tourStop(tourName) {
        if (tourName) {
            //set user setting for guided tour as seen (only make network request if have not already seen that tour)
            if (!this.props.toursSeen[tourName]) {
                Inflectra.SpiraTest.Web.Services.Ajax.GlobalService.UserSettings_GuidedTour_SetSeen(tourName);
            }
            //update state
            this.setState({
                showTour: false,
                seenAppIntro: tourName == "appIntro" ? true : this.state.seenAppIntro,
                seenUpdate: tourName == this.props.updateTourName ? true : this.state.seenUpdate,
                tourIndex: 0
            })
        }
    }
    tourStart(tourName) {
        const newTour = this.tourGet(tourName);
        this.setState({
            showTour: true,
            tour: newTour,
            tourStepTitles: newTour && newTour.steps.length ? newTour.steps.map(x => x.title) : []
        });
    }

    tourGet(name) {
        const tourIndex = this.props.tours.map(x => x.name).indexOf(name);
        if (tourIndex >= 0) {
            const newTour = this.props.tours[tourIndex];
            // remove admin entries if user is not an admin
            const userIsAdmin = !globalFunctions.objectIsEmpty(SpiraContext.Navigation.adminNavigation)
            const newTourFilterAdmin = userIsAdmin ? newTour : this.tourStripAdminOnlySteps(newTour);

            // remove any links to previous tours if those tours have been seen
            const newTourFilterAdminAndTours = this.tourStripSeenTours(newTourFilterAdmin);
            return newTourFilterAdminAndTours;
        } else {
            return null;
        }
    }
    tourStripAdminOnlySteps(tour) {
        const filteredSteps = tour.steps.filter(x => !x.isAdmin);
        let newTour = tour;
        newTour.steps = filteredSteps;
        return newTour;
    }
    tourStripSeenTours(tour) {
        const tourLinkToTours = tour.steps.filter(x => x.linkToTour);
        // if the tour has links to other tours then filter out any that we've seen
        if (tourLinkToTours) {
            const filteredSteps = tour.steps.filter(x => {
                // include all steps that aren't links to tours
                if (!x.linkToTour) {
                    return x;
                // include all steps to other tours that have not been seen
                } else if (!this.props.toursSeen[x.linkToTour]) {
                    return x;
                }
            })
            let newTour = tour;
            newTour.steps = filteredSteps;
            return newTour;
        } else {
            return tour;
        }
    }

    // moves the tour from one step to the next - it also handles moving to a different tour as of 6.1
    // param: newIndex: the new index of the tour to move to
    tourChangeIndex(newIndex) {
        if (newIndex >= 0 && newIndex < this.state.tour.steps.length && newIndex !== this.state.tourIndex) {
            // check to see if the new step we would move to contains a link to another tour
            const newIndexLinkToTour = this.state.tour.steps[newIndex].linkToTour;
            // ...if it does, then we need to see if that tour exists
            if (newIndexLinkToTour) {
                const newTour = this.props.tours.filter(x => x.name == newIndexLinkToTour);
                if (newTour && newTour.length) {
                    // if the new tour exists then we move to it - stopping this tour and starting the new one.
                    this.tourStop(this.state.tour.name);
                    this.tourStart(newIndexLinkToTour);
                }
            // if this is a normal tour step then just move to it
            } else {
                this.setState({ tourIndex: newIndex })
            }
        }
    }

    render() {
        const { seenAppIntro, seenUpdate, showTour, tour, tourIndex, tourStepTitles } = this.state;
        const shouldShowUpdate = !seenUpdate && this.tourGet(this.props.updateTourName);

        return (
            <div className="br3-bottom fixed top-nav x-center bg-white z-1000 w960 mvw-100 py4 px5 px4-xs ov-y-auto left-25 b-light-gray bb bl br shadow-b-mid-gray mvh-insideHead min-h-insideHead-xs vw-100-sm">
                <span
                    onClick={this.tourStopAndClosePanel.bind(null, tour && tour.name ? tour.name : null)}
                    className="fas fa-times pointer fr o-70"
                ></span>

                {!showTour ?
                    <React.Fragment>
                        <div className="df py5 py4-xs">
                            <img
                                alt={SpiraContext.ProductType}
                                className="w7 h7 w6-xs h6-xs mr4 dib"
                                src={SpiraContext.BaseThemeUrl + "Images/product-" + SpiraContext.ProductType + ".svg"}
                            />
                            <div className="dib">
                                <h2 className="fs-h2 fs-h3-xs my0 dib">
                                    {resx.Onboarding_WelcomeToProduct.replace("{0}", SpiraContext.ProductType)}
                                </h2>
                                <p className="fs-125 fs-110-xs">
                                    {!seenAppIntro ? resx.Onboarding_Welcome_Message : shouldShowUpdate ? resx.Onboarding_UpdateIntro_Message : resx.Onboarding_SelectATour}
                                </p>
                            </div>
                        </div>

                        {!seenAppIntro || shouldShowUpdate ?
                            <React.Fragment>
                                <div className="btn-group">
                                    <button
                                        className="btn btn-primary px5 py3"
                                        onClick={this.tourStart.bind(null, !seenAppIntro ? "appIntro" : this.props.updateTourName )}
                                        type="button"
                                    >
                                        {resx.Onboarding_YesPlease}
                                    </button>
                                    <button
                                        className="btn btn-default bg-white px4 py3"
                                        onClick={this.tourStopAndClosePanel.bind(null, !seenAppIntro ? "appIntro" : this.props.updateTourName )}
                                        type="button"
                                    >
                                        {resx.Onboarding_NoThanks}
                                    </button>
                                </div>
                                <a
                                    href={null}
                                    className="pl4 pointer"
                                    onClick={this.tourStop.bind(null, !seenAppIntro ? "appIntro" : this.props.updateTourName)}
                                    >
                                    {resx.Onboarding_ShowAllTours}
                                </a>
                            </React.Fragment>
                            :
                            <div className="df flex-wrap">
                                {this.props.tours.map((tour, index) =>
                                    <div
                                        key={index}
                                        className={"px5 px3-xs py3 mb3 df items-center pointer bg-off-white-hover transition-all yolk-hover br3 w-50 w-100-xs"}
                                        onClick={this.tourStart.bind(null, tour.name)}
                                        >
                                        {tour.image &&
                                            <img
                                                className="w6 h6 mr4"
                                                src={SpiraContext.BaseThemeUrl + "Images/" + tour.image}
                                            />
                                        }
                                        <span>{tour.title}</span>
                                    </div>
                                )}
                                <div className="w-100">
                                    <button
                                        className="btn btn-default bg-white px4 py3"
                                        onClick={this.handleClose}
                                        type="button"
                                        >
                                        {resx.Global_Cancel}
                                    </button>
                                </div>
                            </div>
                        }
                    </React.Fragment>
                    : null
                }

                {showTour && tour ? 
                    <React.Fragment>
                        <button
                            type="button"
                            onClick={this.tourStop.bind(null, tour.name)}
                            className="u-btn-minimal pa3 ml3"
                            >
                            <i class="fas fa-home"></i>
                        </button>
                        <button
                            type="button"
                            onClick={this.tourChangeIndex.bind(null, tourIndex - 1 >= 0 ? tourIndex - 1 : null)}
                            disabled={tourIndex - 1 < 0}
                            className="u-btn-minimal pa3 ml3"
                            >
                            <i class="fas fa-arrow-left"></i>
                        </button>
                        <button
                            type="button"
                            onClick={this.tourChangeIndex.bind(null, tourIndex + 1 < tour.steps.length ? tourIndex + 1 : null)}
                            disabled={tourIndex + 1 >= tour.steps.length}
                            className="u-btn-minimal pa3 ml3"
                            >
                            <i class="fas fa-arrow-right"></i>
                        </button>
                        <h2 className="px4">{tour.title}</h2>
                        <div className="df mw800 mx-auto">
                            <ol className="ma0 pa0 dn-xs">
                                {tourStepTitles.map((x, index) =>
                                    <li
                                        key={index}
                                        className={"px5 py3 mb3 br3-left pointer lsn" + (tourIndex === index ? " bg-off-white" : "")}
                                        onClick={this.tourChangeIndex.bind(null, index)}
                                        >
                                        {x}
                                    </li>
                                )}
                            </ol>
                            <div className={"bg-off-white pa4 br3-right br3-bl" + (tourIndex !== 0 ? " br3-tl" : "")}>
                                <h4 className="dn-sm dn-md-up db-xs fs-150 fw-b pt4 my0">{tour.steps[tourIndex].title}</h4>
                                {tour.steps[tourIndex].description &&
                                    <p className="fs-125 fs-110-xs my4">
                                        {tour.steps[tourIndex].description}
                                        {tour.steps[tourIndex].link && 
                                            <React.Fragment>
                                                <br></br>
                                                <a href={tour.steps[tourIndex].link} target="_blank">
                                                    {resx.Onboarding_ReadMore}
                                                </a>
                                            </React.Fragment>
                                        }
                                    </p>
                                }
                                {tour.steps[tourIndex].image &&
                                    <img
                                        className="mh9 mw-100 pa4"
                                        src={SpiraContext.BaseThemeUrl + "Images/" + tour.steps[tourIndex].image}
                                       />
                                }
                                {tourIndex == tour.steps.length - 1 &&
                                    <div>
                                        <button
                                            type="button"
                                            onClick={this.tourStopAndClosePanel.bind(null, tour.name)}
                                            className="btn btn-primary mt4"
                                            >
                                            {resx.Global_Finish}
                                        </button>
                                    </div>
                                }
                            </div>
                        </div>
                    </React.Fragment>
                    : null
                }
            </div>
        )
    }
}
