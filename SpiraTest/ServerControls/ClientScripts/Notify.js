/*
 * Author: Alex Gibson
 * https://github.com/alexgibson/notify.js
 * License: MIT license
 */

const N = window.Notification;

function isFunction(item) {
    return typeof item === 'function';
}

function Notify(title, options) {

    if (typeof title !== 'string') {
        throw new Error('Notify(): first arg (title) must be a string.');
    }

    if (options && typeof options !== 'object') {
        throw new Error('Notify(): second arg (options) must be an object.');
    }

    if (!options)
    {
        options = {
            notifyShow: null,
            notifyClose: null,
            notifyClick: null,
            notifyError: null,
            closeOnClick: false,
            timeout: null
        };
    }

    this.title = title;
    this.options = options;
    this.permission = null;
    this.closeOnClick = options.closeOnClick;
    this.timeout = options.timeout;

    //callback when notification is displayed
    if (options.notifyShow && isFunction(options.notifyShow))
    {
        this.onShowCallback = options.notifyShow;
    }

    //callback when notification is closed
    if (options.notifyClose && isFunction(options.notifyClose))
    {
        this.onCloseCallback = options.notifyClose;
    }

    //callback when notification is clicked
    if (options.notifyClick && isFunction(options.notifyClick))
    {
        this.onClickCallback = options.notifyClick;
    }

    //callback when notification throws error
    if (options.notifyError && isFunction(options.notifyError))
    {
        this.onErrorCallback = options.notifyError;
    }
}

// returns true if the browser supports Web Notifications
// https://developers.google.com/web/updates/2015/05/Notifying-you-of-notificiation-changes
// @param {perm} for test purposes only
Notify.isSupported = function(perm) {
    if (!N || !N.requestPermission) {
        return false;
    }

    if (perm === 'granted' || N.permission === 'granted') {
        throw new Error('You must only call this before calling Notification.requestPermission(), otherwise this feature detect would trigger an actual notification!');
    }

    try {
        new N('');
    } catch (e) {
        if (e.name === 'TypeError') {
            return false;
        }
    }
    return true;
};

// true if the permission is not granted
Notify.needsPermission = (N && N.permission && N.permission === 'granted') ? false : true;

// asks the user for permission to display notifications.  Then calls the callback functions is supplied.
Notify.requestPermission = function(onPermissionGrantedCallback, onPermissionDeniedCallback) {
    N.requestPermission(function(perm) {
        switch (perm) {
        case 'granted':
            Notify.needsPermission = false;
            if (isFunction(onPermissionGrantedCallback)) {
                onPermissionGrantedCallback();
            }
            break;
        case 'denied':
            Notify.needsPermission = true;
            if (isFunction(onPermissionDeniedCallback)) {
                onPermissionDeniedCallback();
            }
            break;
        }
    });
};


Notify.prototype.show = function() {
    this.myNotify = new N(this.title, this.options);

    if (!this.options.requireInteraction && this.timeout && !isNaN(this.timeout)) {
        setTimeout(this.close.bind(this), this.timeout * 1000);
    }

    this.myNotify.addEventListener('show', this, false);
    this.myNotify.addEventListener('error', this, false);
    this.myNotify.addEventListener('close', this, false);
    this.myNotify.addEventListener('click', this, false);
};

Notify.prototype.onShowNotification = function(e) {
    if (this.onShowCallback) {
        this.onShowCallback(e);
    }
};

Notify.prototype.onCloseNotification = function(e) {
    if (this.onCloseCallback) {
        this.onCloseCallback(e);
    }
    this.destroy();
};

Notify.prototype.onClickNotification = function(e) {
    if (this.onClickCallback) {
        this.onClickCallback(e);
    }

    if (this.closeOnClick) {
        this.close();
    }
};

Notify.prototype.onErrorNotification = function(e) {
    if (this.onErrorCallback) {
        this.onErrorCallback(e);
    }
    this.destroy();
};

Notify.prototype.destroy = function() {
    this.myNotify.removeEventListener('show', this, false);
    this.myNotify.removeEventListener('error', this, false);
    this.myNotify.removeEventListener('close', this, false);
    this.myNotify.removeEventListener('click', this, false);
};

Notify.prototype.close = function() {
    this.myNotify.close();
};

Notify.prototype.handleEvent = function(e) {
    switch (e.type) {
    case 'show':
        this.onShowNotification(e);
        break;
    case 'close':
        this.onCloseNotification(e);
        break;
    case 'click':
        this.onClickNotification(e);
        break;
    case 'error':
        this.onErrorNotification(e);
        break;
    }
};

