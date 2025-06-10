//Drop Button component
interface Assn_DropButtonProps {
    selected: string; //used to set the current selected item
    enabled: boolean; // determines whether 
    itemClickFunction: any; //function to call when an item in the dropdown list is clicked
    items: Array<any>; //the list of values to show in the dropdown along with their associated IDs
    onExpandCollapseFolderBtnItemClick: any; //if the dropdown items are hierarchically nested, this is the function to call on one of the expand/collapse buttons being clicked
    boxClasses: string; // for the div wrapper around the entire control
    buttonClasses: string; // for the button
    menuClasses: string; // for the dropdown menu
    liClasses: string; // general styling of each li wrapper for the items in the dropdown list
    itemClasses: string; // styling for the inner tag inside each li
}
class RctDrop extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);

        this.state = {
            menuIsOpen: false,
            boxClasses: "drop-box" + (this.props.boxClasses ? " " + this.props.boxClasses : ""),
            buttonClasses: "",
            buttonText: "",
            hasItems: false, //should always have one item - the "Select from dropdown" entry
            menuClasses: this.props.menuClasses ? this.props.menuClasses : "u-drop-menu mh9 ov-y-auto",
            itemClasses: this.props.itemClasses ? this.props.itemClasses : "",
            liClasses: "u-drop-item " + (this.props.liClasses ? this.props.liClasses : ""),
            selected: this.props.selected,
            selectedItem: "",
            focused: this.props.selected,
            itemListIds: this.props.items && this.props.items.length ? this.props.items.filter(x => !x.hide).map(x => x.listId) : null,
            showLoader: false
        }

        this.setButtonClasses = this.setButtonClasses.bind(this);
        this.menuToggle = this.menuToggle.bind(this);
        this.menuClose = this.menuClose.bind(this);
        this.menuOnBlur = this.menuOnBlur.bind(this);
        this.itemOnClick = this.itemOnClick.bind(this);
        this.itemExpanderClick = this.itemExpanderClick.bind(this);
    }

    componentDidMount() {
        this.bindKeyboardShortcuts();
        this.setButtonClasses();
        this.setState({
            buttonText: this.buttonText(),
            hasItems: this.hasItems(),
            selectedItem: this.selectedItem()
        })
    }

    hasItems() {
        return this.props.items && this.props.items.length > 0 && this.props.items[0] != undefined;
    }

    setButtonClasses() {
        const classes = "drop-toggle" + (this.hasItems() ? "" : " is-empty") + (this.props.buttonClasses ? " " + this.props.buttonClasses : " u-btn");
        this.setState({ buttonClasses: classes })
    }

    selectedItem() {
        return this.hasItems() ?
            this.props.items.filter(item => item.listId == this.props.selected)[0] || this.props.items[0].listId
            : null;
    }

    buttonText() {
        // first get the currently selected item
        const selectedItem = this.selectedItem();
        const wrapperClasses = "mw-100 ov-hidden to-ellipsis";

        let selectedText = "",
            defaultText = "",
            extraDefaultText = "";

        // first we create the parts of the button text
        if (selectedItem && selectedItem.name) {
            const selectedImage = this.props.showImageInButton && selectedItem.imageUrl ? <img
                src={selectedItem.imageUrl}
                className={selectedItem.imageClasses || ""}
            /> : null;
            const selectedTextActual = selectedItem.showTitleInButton ? selectedItem.title : selectedItem.name;
            const selectedGlyph = selectedItem.secondaryGlyph && <i className={selectedItem.secondaryGlyph} />;

            selectedText = <span>{selectedImage}{selectedTextActual}{selectedGlyph}</span>
        }
        if (this.props.defaultText && (this.props.showDefaultAndSelected || this.props.alwaysShowDefault || !selectedText)) {
            defaultText = this.props.defaultText;
        }
        if (!selectedText && this.props.extraDefaultText && (this.props.showDefaultAndSelected || this.props.alwaysShowDefault)) {
            extraDefaultText = <span className={this.props.extraDefaultClasses}>{this.props.extraDefaultText}</span>
        }

        // then we get them in the right order
        const showDefaultOnRight = this.props.showDefaultOnRight;
        const firstItem = showDefaultOnRight ? selectedText : defaultText,
            secondItem = extraDefaultText,
            thirdItem = showDefaultOnRight ? defaultText : selectedText;

        return <span className={wrapperClasses}>
            {firstItem}
            <span className="ml2">{secondItem}</span>
            <span className="ml2">{thirdItem}</span>
        </span>;
    }

    bindKeyboardShortcuts() {
        Mousetrap.bind("down", (e) => {
            e.target.focus();
            e.preventDefault();
            this.focusedMove("down");
        });
        Mousetrap.bind("up", (e) => {
            e.target.focus();
            e.preventDefault();
            this.focusedMove("up");
        });
        Mousetrap.bind("right", (e) => {
            e.preventDefault();
            this.itemExpanderClick(this.state.focused, false, null);
        });
        Mousetrap.bind("left", (e) => {
            e.preventDefault();
            this.itemExpanderClick(this.state.focused, true, null);
        });
        Mousetrap.bind("escape", (e) => this.menuClose());
        Mousetrap.bind("enter", (e) => this.itemOnClick(this.state.focused, e));
    }
    unbindKeyboardShortcuts() {
        Mousetrap.unbind('down');
        Mousetrap.unbind('up');
        Mousetrap.unbind('right');
        Mousetrap.unbind('left');
    }

    // utility function to give you the next or previous item in an array - ie if you are at index 2 it returns the item at index 1 or 3 (based on direction)
    // param: item - the starting element in the array - eg string or int
    // param: array - the array to check
    // param: bool of moveUp - is true if moving up in the array (ie toward the start of the array)
    // return: object with the new item and info about whether we are at the start or end of the array
    moveInArray(item, array, moveUp) {
        const index = array.indexOf(String(item));
        // move forward or backward in the array if we can
        const shift = (moveUp && index > 0) ? -1 : (!moveUp && array.length > index + 1) ? 1 : 0;
        const newIndex = index === -1 ? 0 : index + shift;
        return {
            item: array[newIndex],
            atStart: newIndex === 0,
            atEnd: newIndex === array.length - 1
        }
    }

    // finds the next item in the list
    // param: string of the direction - "up" or "down" - if absent then the function defaults to moving 'down'
    // return: string of the new item, if there is one - otherwise defaults to current focused item
    focusedMove(direction) {
        let newFocus = this.state.selected;
        const isMoveUp = direction == "up";
        const listIds = this.props.items.filter(x => !x.hide).map(x => x.listId);

        // only take action if we have items
        if (listIds.length) {
            // move forward or backward in the array if we can
            newFocus = this.moveInArray(this.state.focused, listIds, isMoveUp).item;
        }
        this.setState({ focused: newFocus });
    }

    // toggles whether the entire menu opens or closes
    menuToggle(e?) {
        if (e) e.stopPropagation();
        // if we have a passed in function from the parent to call when the dropdown is opened/closed we call it now
        this.props.menuToggleCallback && this.props.menuToggleCallback(this.props.dataName, !this.state.menuIsOpen)

        this.state.menuIsOpen ? this.unbindKeyboardShortcuts() : this.bindKeyboardShortcuts();
        this.setState({ menuIsOpen: !this.state.menuIsOpen })
    }

    // closes the entire menu
    menuClose() {
        // if we have a passed in function from the parent to call when the dropdown is opened/closed we call it now
        this.props.menuToggleCallback && this.props.menuToggleCallback(this.props.dataName, false)

        this.setState({ menuIsOpen: false });
        this.unbindKeyboardShortcuts();
    }

    // handles the menu onblur synthetic event - closes the menu under certain circumstances
    menuOnBlur(e) {
        const target = e.currentTarget;

        //setTimeout used so that as the event happens the correct element will be focused to make sure close event occurs only when it should
        setTimeout(() => {
            //check to see if the clicked item is contained within the element being closed on blur
            if (!target.contains(document.activeElement)) {
                this.menuClose();
                this.unbindKeyboardShortcuts();
            }
        }, 0);
    }

    //manage the clicking of an item in the dropdown - logic and functionality effectively passed to parent component to manage main state
    itemOnClick(listId, e?) {
        if (e) e.preventDefault();

        // first find the item matching the listId in the array of items
        const item = this.props.items.filter(x => x.listId === listId)[0];
        if (item && item.isEnabled) {
            //call external function to change main state - if one has been defined
            if (this.props.itemClickManager) {
                this.props.itemClickManager(listId);
                if (this.props.showLoaderOnClick && !item.dontShowLoader) { this.setState({ showLoader: true }) };
                //close the dropdown
                this.menuClose();
                // otherwise try and navigate to the url
            } else if (item.url) {
                // if ctrl + click / middle-click then open in new window
                if (e.ctrlKey || e.which === 2) {
                    window.open(item.url, '_blank');
                } else {
                    window.location.assign(item.url);
                    if (this.props.showLoaderOnClick && !item.dontShowLoader) { this.setState({ showLoader: true }) };
                    //close the dropdown
                    this.menuClose();
                }
                // if the item is enabled and is a parent but does not have a url make it like it is the expander
            } else if (item.children) {
                this.itemExpanderClick(listId, null, null);
            }
            // if the item is disabled and is a parent make it like it is the expander
        } else if (item && item.children) {
            this.itemExpanderClick(listId, null, null);
        }
    }

    //handle a click on any expand/collapse buttons in the dropdown
    itemExpanderClick(parentListId, status, event?) {
        if (event) {
            event.preventDefault();
            event.stopPropagation();
        }

        // check parentListId is a parent
        const parent = this.props.items.filter(x => x.listId === parentListId)[0];
        const hasChildren = parent && parent.children && parent.children.length > 0;
        if (hasChildren) {
            const newHideStatus = status == null ? !parent.hideChildren : status;
            this.props.expanderClickManager([parentListId], newHideStatus, this.props.dataName, this.props.dataHiddenName);
        }
    }

    render() {
        const {
            boxClasses,
            buttonClasses,
            buttonText,
            hasItems,
            itemClasses,
            menuClasses,
            menuIsOpen,
            selectedItem,
            showLoader
        } = this.state;

        const itemNodes = hasItems ?
            this.props.items.filter(x => !x.hide).map((item, index) => {
                const selectedClass = this.state.selected == item.listId ? " is-selected" : "",
                    focusedClass = this.state.focused == item.listId ? " is-focused" : "",
                    enabledClass = item.isEnabled ? "" : " cursor-default";

                const linkClasses = (item.linkClasses ? item.linkClasses : "u-drop-item") + selectedClass + enabledClass + focusedClass,
                    expanderClasses = this.props.expanderClasses + (" w4 mln4 pointer fas fa-fw" + (item.hideChildren ? " fa-caret-right" : " fa-caret-down"));

                return (
                    <a
                        aria-label={item.ariaLabel}
                        className={linkClasses}
                        href={item.isEnabled ? item.url || "" : ""}
                        id={item.domIdPrefix && item.listId ? item.domIdPrefix + item.listId : ""}
                        key={item.listId}
                        target={item.target || ""}
                        onClick={item.openPlainHtml ? null : this.itemOnClick.bind(null, item.listId)}
                        title={item.title}
                        >
                        {item.indentLevel ? <span style={{ paddingLeft: (item.indentLevel + "rem") }} /> : null}
                        {item.children &&
                            <span
                                className={expanderClasses}
                                onClick={this.itemExpanderClick.bind(null, item.listId, null)}
                                />
                        }
                        {item.imageUrl &&
                            <img
                                src={item.imageUrl}
                                className={item.imageClasses || ""}
                                />
                        }
                        {item.glyph && <i aria-hidden="true" className={item.glyph} />}
                        {item.name}
                        {item.secondaryGlyph && <i aria-hidden="true" className={item.secondaryGlyph} />}
                    </a>
                )
            })
            : null;

        return (
            <div
                className={boxClasses}
                id={this.props.wrapperId || ""}
                onBlur={this.menuOnBlur}
                >
                <button
                    aria-expanded={menuIsOpen ? "true" : "false"}
                    aria-haspopup="true"
                    aria-label={this.props.ariaLabelWrapper}
                    className={buttonClasses + (menuIsOpen ? " is-open" : "") + (showLoader ? " is-empty" : "")}
                    id={this.props.domId || "" }
                    onClick={this.menuToggle}
                    title={selectedItem && selectedItem.title || this.props.defaultTitle || ""}
                    type="button"
                    >
                    {this.state.showLoader ?
                        <div className="lds-ellipsis"><div /><div /><div /><div /></div>
                        : buttonText
                    }
                </button>
                {hasItems && menuIsOpen &&
                    <div
                        className={menuClasses}
                        role="menu"
                        tabindex="0"
                        >
                        {itemNodes}
                    </div>
                }
            </div>
        )
    }
}
