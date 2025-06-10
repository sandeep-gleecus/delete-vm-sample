/// <reference path="rct_comp_avatarIcon.tsx" />

//external dependendencies (js libraries)
declare var React: any;
declare var ReactDOM: any;
declare var $: any;
declare var $find: any;

//inflectra services
declare var globalFunctions: any;
declare var SpiraContext: any;
declare var Inflectra: any;
declare var g_userOnlineStatusManager: any;
declare var pnlAddFollower_updateFollowers_success: any;
declare var ArtifactEmail_subscribeChange_UpdateDropMenu: any;

// set up global calls for aspx page and react components to reference
interface followers {
    domContainerId: string;
    dropMenu: string;
    followerData: any[];
    followerMeta: followerMeta;
    reactInitiallyRendered: boolean;
}
interface followerMeta {
    projectId: number;
    artifactTypeId: number;
    artifactId: number;
}


let followers = {
    domContainerId: 'followersListBox',
    dropMenu: '',
    followerData: new Array,
    followerMeta: {
        projectId: 0,
        artifactTypeId: 0,
        artifactId: 0,
    },
    reactInitiallyRendered: false,
    getFollowersList: function (projectId, artifactTypeId, artifactId) {
        followers.followerMeta.projectId = projectId;
        followers.followerMeta.artifactTypeId = artifactTypeId;
        followers.followerMeta.artifactId = artifactId;
        Inflectra.SpiraTest.Web.Services.Ajax.NotificationService.Notification_FollowersOfArtifact(
            projectId,
            artifactTypeId,
            artifactId,
            followers.getFollowersList_success,
            followers.getFollowersList_failure
        )
    },
    getFollowersList_success: function (res) {
        if (res && res.length > 0) {
            //save the respsonse data 
            followers.followerData = res;
        }

        if (followers.reactInitiallyRendered) {
            pnlAddFollower_updateFollowers_success(res)
        } else {
            //render react
            ReactDOM.render(
                <FollowerBox
                    projectId={SpiraContext.ProjectId}
                    artifactTypeId={SpiraContext.ArtifactTypeId}
                    currentUserId={SpiraContext.CurrentUserId}
                    dropMenu={followers.dropMenu}
                    />,
                document.getElementById(followers.domContainerId)
            )
            //set flag to track if react has been initialized to true
            followers.reactInitiallyRendered = true;
        }
    },

    getFollowersList_failure: function () {
        //fail quietly
    },

    //empty function in global namespace, which gets set inside react
    seeWhoIsOnline: function (onlineUsers) {
    }
};

class FollowerBox extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);
        this.state = {
            artifactId: SpiraContext.ArtifactId,
            canUnsubscribe: this.props.dropMenu ? $find(this.props.dropMenu)._authorizedDropMenuItems : false,
            followers: followers.followerData.length > 0 ? followers.followerData : [],
            lastRemovedUserId: -1,
            maxToShow: 100,
            imEnabled: typeof g_userOnlineStatusManager != "undefined",
            numberAboveFold: 5,
            usersOnline: typeof g_userOnlineStatusManager != "undefined" ? g_userOnlineStatusManager.get_onlineUsers() : null, //only check online status if instant messenger enabled
            viewUpToMax: false
        }

        //register functions
        this.toggleShowAll = this.toggleShowAll.bind(this);
        this.sendMessageClick = this.sendMessageClick.bind(this);
        this.removeUserAsFollower = this.removeUserAsFollower.bind(this);
        this.updateListOfFollowers = this.updateListOfFollowers.bind(this);
        this.updateListOfFollowers_success = this.updateListOfFollowers_success.bind(this);
    }
    componentWillMount () {
        let self = this;
        //initialized the fx to set the state for the list of online users
        followers.seeWhoIsOnline = function () {
            self.setState({ usersOnline: typeof g_userOnlineStatusManager != "undefined" ? g_userOnlineStatusManager.get_onlineUsers() : null });
        }
        //register a callback function to update online status whenever messageGetInfo polls the server (if IM enabled)
        if(this.state.imEnabled) g_userOnlineStatusManager.register_callback(followers.seeWhoIsOnline);

        //change the below function to updating state with the latest list of followers 
        pnlAddFollower_updateFollowers_success = function (data) {
            //sets new follower data
            self.setState({
                followers: data,
                artifactId: SpiraContext.ArtifactId
            });
        };
    }

    //toggles whether to show only above the line followers or up to the max allowed for display
    toggleShowAll () {
        this.setState({ viewUpToMax: !this.state.viewUpToMax });
    }

    //trigger sending a message to the chosen user
    sendMessageClick (userId) {
        if (this.state.imEnabled) g_userOnlineStatusManager.sendMessageToSpecifiedUser(userId);
    }

    removeUserAsFollower (userId) {
        this.setState({ lastRemovedUserId: userId });

        Inflectra.SpiraTest.Web.Services.Ajax.NotificationService.Notification_UnsubscribeSelectedUserFromArtifact(
            this.props.projectId,
            this.props.artifactTypeId,
            this.state.artifactId,
            userId,
            this.updateListOfFollowers,
            this.removeUserAsFollower_failure
        )
    }
    updateListOfFollowers () {
        //get the updated list of following users
        Inflectra.SpiraTest.Web.Services.Ajax.NotificationService.Notification_FollowersOfArtifact(
            this.props.projectId,
            this.props.artifactTypeId,
            this.state.artifactId,
            this.updateListOfFollowers_success,
            this.updateListOfFollowers_failure
        )

        //handle the case where user unsubscribes themselves using the UI here, instead of dropmenu
        if (this.props.currentUserId == this.state.lastRemovedUserId) {
            ArtifactEmail_subscribeChange_UpdateDropMenu($find(this.props.dropMenu));
        }
    }
    updateListOfFollowers_success (data) {
        //sets new follower data
        this.setState({ followers: data });
    }
    updateListOfFollowers_failure () {
        //do nothing
    }
    removeUserAsFollower_failure () {
        //do nothing
    }

    render () {
        const self = this,
            moreThanMaxFollowers = this.state.followers.length > this.state.maxToShow,
            maxNumberToShow = moreThanMaxFollowers ? this.state.maxToShow : this.state.followers.length,
            numberBelowFold = maxNumberToShow > this.state.numberAboveFold ? maxNumberToShow - this.state.numberAboveFold : 0,
            followers = this.state.followers.map(function (item) {
                item.nameAsIcon = item.firstName && item.lastName ? item.firstName[0].toUpperCase() + item.lastName[0].toUpperCase() : "?";
                item.isOnline = self.state.imEnabled ? self.state.usersOnline.indexOf(item.userId) >= 0 : false;
                item.avatarUrl = `${SpiraContext.BaseUrl}UserAvatar.ashx?userId=${item.userId}`
                item.index = self.state.followers.indexOf(item);
                item.showIcon = item.index < self.state.maxToShow && (item.index < self.state.numberAboveFold || self.state.viewUpToMax);
                return item;
            }),
            avatarNodes = followers.filter(item => item.showIcon).map(function (item) {
                return (
                    <AvatarIcon
                        hasIcon={item.hasIcon}
                        icon={item.avatarUrl}
                        key={item.userId}
                        name={item.fullName}
                        nameAsIcon={item.nameAsIcon}
                        showIcon={item.showIcon}
                        userId={item.userId}

                        // card specific props
                        canUnsubscribe={self.state.canUnsubscribe || false}
                        cardWidth="256" /*this is the width used to calculate how wide the popup card will be and therefore it's relative position*/
                        department={item.department}
                        isFollower={true} //by default all on this list are followers, so set as true
                        isOnline={item.isOnline}
                        removeUserAsFollower={self.removeUserAsFollower}
                        sendMessageClick={self.sendMessageClick}
                        showCard={true}
                        />
                )
            });
        let belowFoldText = this.state.viewUpToMax ? resx.Global_ShowLess : resx.Global_ShowAll,
            belowFoldIcon = this.state.viewUpToMax ? "fas fa-angle-up pl3 u-mini-bounce_up-hover" : "fa fa-angle-down pl3 u-mini-bounce_down-hover",
            numberCurrentlyShowing = this.state.followers.filter(item => item.showIcon === true).length;

        if (this.state.followers.length) {
            return (
                <div>
                    <div className="clearfix mb3 lh-initial">
                        <span className="mr2">
                            {resx.Global_Followers}
                        </span>
                        <span className="gray fs-90">
                            ({numberCurrentlyShowing < maxNumberToShow ? numberCurrentlyShowing + " / " : ""}{maxNumberToShow}{moreThanMaxFollowers ? "+" : null})
                        </span>
                        {numberBelowFold ?
                            <div
                                className="silver pointer fr"
                                onClick={this.toggleShowAll}
                                >
                                {belowFoldText}
                                   <span className={belowFoldIcon} />
                            </div>
                            : null
                        }
                    </div>
                    {avatarNodes}
                </div>
            )
        } else {
            return null
        }
    }

}

function AvatarCard(props){
    // First, work out the correct classes to make sure the card will be visible properly depending on screen space
	let cardClassesBase = "dib absolute br2 shadow-b-mid-gray mh8 bg-white z-110 bottom-100 mb4",
        cardClassesRight = "left0 u-triangle-bl-vlight-gray",
        cardClassesLeft = "right0 u-triangle-br-vlight-gray",
        cardClassesCenter = "u-triangle-b-vlight-gray",
        isCenter = props.cardPosition !== "right" && props.cardPosition !== "left",
        cardClasses = `${cardClassesBase} ${
            props.cardPosition === "right" ? cardClassesRight : 
            props.cardPosition === "left" ? cardClassesLeft : cardClassesCenter
        }`,
        statusClasses = `dib br2 px3 py1 tc mr2 silver ${props.isOnline ? "fw-b" : ""}`,
        messageClasses = `dib br2 silver px3 py1 tc ba b-light-gray ${props.isOnline ? "bg-white pointer" : "pointer"}`,
        cardStyle = {
            width: `${props.cardWidth}px`, /*note this value is styled as its value needs to be accessed by the JS */
            left: isCenter ? `calc(50% - ${(props.cardWidth / 2)}px` : ""
        };
    return (
        <div
            className={cardClasses}
            style={cardStyle}
            >
            <div className="db relative df items-start">
                {props.hasIcon ?
                    <img
                        src={props.icon}
                        alt={props.name}
                        className="w6 h-auto br2-br br2-tl fl"/>
                    :
                    <div className="w6 h6 br2-br br2-tl fl dit bg-vlight-gray ov-hidden tc">
                        <span className="fs-250 white y-center dib lh0 fw-b">{props.nameAsIcon}</span>
                    </div>
                }
                <div className="dt mx3 mt3">
                    <div className="dtc px2 w7">
                        <span className="fs-h4 fw-b db mb1">
                            {props.name}
                        </span>
                        <span className="fs-h5 silver">
                            {props.department}
                        </span>
                    </div>
                    {props.isFollower ?
                        <div
                            className={ `absolute top0 right0 px4 py3 silver ${props.canUnsubscribe ? " pointer" : ""}` }
                            data-placement="bottom"
                            data-toggle="tooltip" 
                            onClick={props.canUnsubscribe ? props.removeUserAsFollower.bind(null, props.userId) : null}
                            title={props.canUnsubscribe ? resx.Followers_UnfollowUser : null}
                            >
                            <span className="fas fa-star"></span>
                        </div>
                        :
                        <div
                            className={ `absolute top0 right0 px4 py3 silver ${props.canUnsubscribe ? " pointer" : ""}` }
                            >
                            <span className="far fa-star"></span>
                        </div>
                    }
                </div>
            </div>
            <div className="mt5 bg-vlight-gray pa3 tr br2-bottom">
                <div
                    className={messageClasses}
                    onClick={props.sendMessageClick.bind(null, props.userId) }
                    >
                    <span className="far fa-comment pr2"/>
                    {resx.ArtifactType_Message}
                </div>
            </div>
        </div>
    )
}
