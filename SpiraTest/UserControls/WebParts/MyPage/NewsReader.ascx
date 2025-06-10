<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NewsReader.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.NewsReader" %>

<ul id="<%=this.UniqueID + "_tblNewsFeed"%>" class="w-100 pa0 df flex-wrap" style="display: none;" data-bind="foreach: $root, visible: true">
    <!-- ko ifnot: isInflectra -->
        <li class="lsn pa0 mb3 w-33">
            <a 
                class="mr3 tdn tdn-hover transition-all br3 bg-off-white bg-near-white-hover pa3 df justify-between"
                target="_blank" 
                data-bind="attr: { href: url },
                event: {
                    mouseover: function (dataItem, evt) {<%=this.UniqueID%>_page.displayTooltip(dataItem);},
                    mouseout: function () {<%=this.UniqueID%>_page.hideTooltip();} 
                }"
                >
                <div class="dif mr2 to-ellipsis">
                    <span data-bind="text: publishDate"></span>:
                    <span class="mx2" data-bind="text: headline().substring(0,<%=this.UniqueID%>_page.get_headlineLength())"></span>
                </div>
                <span class="fas fa-external-link-alt" aria-label="Opens in New Window"></span>
            
            </a>
        </li>
    <!-- /ko -->
    <!-- ko if: isInflectra -->
        <li class="lsn u-card u-card-hl">
            <div class="u-card-hl-bar" data-bind="attr: { 'data-cardtype': category }"></div>
            <a 
                class="u-card-hl-content"
                target="_blank" 
                data-bind="attr: { href: url },
                event: {
                    mouseover: function (dataItem, evt) {<%=this.UniqueID%>_page.displayTooltip(dataItem);},
                    mouseout: function () {<%=this.UniqueID%>_page.hideTooltip();} 
                }"
                >
                <h3 class="u-card-hl-title" data-bind="text: headline().substring(0,<%=this.UniqueID%>_page.get_headlineLength())"></h3>
            </a>
        </li>
    <!-- /ko -->
</ul>
<tstsc:MessageBox ID="localMessages" runat="server" SkinID="MessageBox"/>
<tstsc:LabelEx runat="server" ID="lblDefaultUrl" SkinID="DefaultRssUrl" />
<tstsc:ScriptManagerProxyEx runat="server" ID="ajxScriptManager">
    <Services>
        <asp:ServiceReference Path="~/Services/Ajax/NewsReaderService.svc" />
    </Services>
</tstsc:ScriptManagerProxyEx>
    <script type="text/javascript">       
        /* The Page Class */
        Type.registerNamespace('Inflectra.SpiraTest.Web.UserControls');
        Inflectra.SpiraTest.Web.UserControls.<%=this.UniqueID%> = function ()
        {
            this._userId = <%=UserId%>;
            this._projectId = <%=ProjectId%>;
            this._startingIndex = 0;
            this._rowCount = <%=rowsToDisplay%>;
            this._headlineLength = <%=HeadlineLength %>;
            this._url = '<%=FeedUrl %>';
            this._newsViewModel = null;

            Inflectra.SpiraTest.Web.UserControls.<%=this.UniqueID%>.initializeBase(this);
        }
        Inflectra.SpiraTest.Web.UserControls.<%=this.UniqueID%>.prototype =
        {
            /* Constructors */
            initialize: function ()
            {
                Inflectra.SpiraTest.Web.UserControls.<%=this.UniqueID%>.callBaseMethod(this, 'initialize');
            },
            dispose: function ()
            {
                delete this._dialog;
                Inflectra.SpiraTest.Web.UserControls.<%=this.UniqueID%>.callBaseMethod(this, 'dispose');
            },

            /* Properties */
            get_headlineLength: function()
            {
                return this._headlineLength;
            },

            get_url: function()
            {
                return this._url;
            },
            set_url: function(value)
            {
                this._url = value;
            },

            /* Public Methods */
            load_newsItems: function()
            {
                //Retrieve the news items
                globalFunctions.display_spinner();
                Inflectra.SpiraTest.Web.Services.Ajax.NewsReaderService.RetrieveFeed(this._url, this._startingIndex, this._rowCount, Function.createDelegate(this, this.load_newsItems_success), Function.createDelegate(this, this.load_newsItems_failure));
            },
            load_newsItems_success: function(data, evt)
            {
                globalFunctions.hide_spinner();


                var mapping = {
                    create: function (options) {
                        //customize at the root level.  
                        var innerModel = ko.mapping.fromJS(options.data);
                        //Add bool flag if the URL is about inflectra to the model
                        var inflectraFeedRegex = /www.inflectra.com/g;
                        var isInflectra = inflectraFeedRegex.test(options.data.url);
                        innerModel.isInflectra = isInflectra;
                        return innerModel;
                    }
                }

                //Databind
                if (this._newsViewModel)
                {
                    ko.mapping.fromJS(data, this._newsViewModel);
                }
                else
                {
                    this._newsViewModel = ko.mapping.fromJS(data, mapping);
                    ko.applyBindings(this._newsViewModel, $get('<%=this.UniqueID + "_tblNewsFeed"%>'));
                }
            },
            load_newsItems_failure: function(exception)
            {
                globalFunctions.hide_spinner();
                globalFunctions.display_error_message($get('<%=localMessages.ClientID%>'), exception.get_message());
            },

            displayTooltip: function(dataItem)
            {
                var tooltip = '<u>' + dataItem.headline() + '</u><br />' + dataItem.description();
                ddrivetip(tooltip);
            },
            hideTooltip: function(dataItem)
            {
                hideddrivetip();
            }
        }

        //Create the page class and load once page ready
        var <%=this.UniqueID%>_page = $create(Inflectra.SpiraTest.Web.UserControls.<%=this.UniqueID%>);
        $(document).ready(function() {
            <%=this.UniqueID%>_page.load_newsItems();
        });
    </script>

