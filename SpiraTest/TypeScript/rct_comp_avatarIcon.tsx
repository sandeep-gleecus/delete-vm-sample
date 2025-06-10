class AvatarIcon extends (React.Component as { new(props): any; }) {
    constructor(props) {
        super(props);
        /*extra state is declared for the card position because this needs to be dynamically calculated on mount based on position*/
        this.state = {
            cardPosition: "right"
        }
    }

    componentDidMount() {
        if (this.props.showCard) {
            const iconNode = ReactDOM.findDOMNode(this),
                spaceOnLeft = iconNode.offsetLeft,
                spaceOnRight = window.innerWidth - iconNode.offsetLeft,
                newCardPosition = this.setCardPosition(spaceOnLeft, spaceOnRight, this.props.cardWidth);
            this.setState({ cardPosition: newCardPosition });
        }
    }

    setCardPosition(left, right, cardWidth) {
        return right >= cardWidth ? "right" : left >= cardWidth ? "left" : "center";
    }

    render() {
        const iconSize = this.props.iconSize || 5,
            iconWidth = "w" + iconSize,
            iconHeight = "h" + iconSize;
        return (
            <div
                className={this.props.wrapperClasses ? this.props.wrapperClasses : "mr3 mb3 relative dib lh-initial"}
                onClick={this.props.onClick}
                >
                <div
                    className="dib relative o-1-sibling-hover u-slideIn-sibling-hover"
                    >
                    {this.props.hasIcon ?
                        <img
                            alt={this.props.name}
                            className={"br-100 h-auto " + iconWidth}
                            src={this.props.icon}
                            />
                        :
                        <div className={"br-100 dib v-mid bg-light-gray ov-hidden tc " + iconWidth + " " + iconHeight}>
                            <span className="fs-150 fs-110-sm white y-center dib lh0 fw-b">{this.props.nameAsIcon}</span>
                        </div>
                    }
                    {this.props.isOnline ?
                        <div className="h3 w3 br-100 absolute bottom0 right0 bg-green" />
                        : null
                    }
                </div>
                {this.props.showCard &&
                    <AvatarCard
                        cardPosition={this.state.cardPosition}
                        {...this.props}
                    />
                }
            </div>
        )
    }
}