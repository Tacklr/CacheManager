//TODO: Test for large trees
//integrate https://square.github.io/crossfilter/ or similar for working with large data? not sure how tree generation/reading would work with it.
//TODO:
//what about empties , e.g. blah//blah
//or keys that end with the delimiter?
//Keep open state on data change/reload?
//TODO: Put wrappers around ajax so we can do things like error handling, busy indicator (tokens?)
/*!
 * Copyright 2014 Tacklr
 */
; (function (base, $, undefined) {
    //#region Initalize

    base.CM = base.CM || {};
    var CM = base.CM;

    var docReady = $.Deferred();
    $(docReady.resolve);

    var checkedState = {};

    $.when($.get('api/v1/combined'), docReady)
    .done(function (combined) {
        //var params = parseQuery(true);//save in local storage instead?
        var data = combined[0];

        //TODO: Escape non-printable character?
        //data.Delimiter = params['delimiter'] || data.Delimiter || delimiter;

        //Better way to do these transforms?
        data.MemoryLimit = data.MemoryLimit === null ? 'Unknown' : data.MemoryLimit === -1 ? 'Unlimited' : data.MemoryLimit;
        data.MemoryLimitPercent = data.MemoryLimitPercent === null ? 'Unknown' : data.MemoryLimitPercent;
        data.ob_Delimiter = ko.observable(data.Delimiter);
        data.ob_Entries = ko.observableArray(data.Entries);
        data.ob_EntryTree = ko.computed(function () {
            //Can we template directly off the array somehow instead of building our tree here?
            //TODO: need to somehow keep checked state
            var CacheRoot = {
                Children: {},
                Values: []
            };

            var id_i = 0;//need hash function or something
            var delimiter = data.ob_Delimiter();
            //Build our tree, any advantage to doing it server side?
            $.each(data.ob_Entries(), function (i, cache) {
                var key = cache.Key;
                var keyParts = key.split(delimiter || null).reverse();
                var current = CacheRoot;
                var currentKeyParts = [];
                while (keyParts.length > 1) {
                    var keyPart = keyParts.pop();
                    currentKeyParts.push(keyPart);
                    if (current.Children[keyPart] === undefined) {
                        var currentKey = currentKeyParts.join(delimiter) + delimiter;//Tacking the delimiter on the end should also prevent collisions for different delimiters on checked state (I think). (actually this may not work if set to no delimiter in certain cases (key ending with delimiter))
                        //Can this be done more cleanly?
                        if (checkedState[currentKey] === undefined)
                            checkedState[currentKey] = ko.observable(false);

                        current.Children[keyPart] = new CacheNode(currentKey, keyPart, checkedState[currentKey], 'item-' + id_i++);//need to get the subkey up to this point
                    }
                    current = current.Children[keyPart];
                }
                current.Values.push(new CacheValue(cache, keyParts.pop(), 'item-' + id_i++));//TODO: Prevent duplicates
            });

            return CacheRoot;
        });

        ko.applyBindings(data);//Tree parts lose open state on delete, need to save the state somehow.
        cacheData = data;

        //window.DERP = data;

        //Clear loading indiciator
        $('.content-loading').fadeOut(function () {
            $(this).remove();
        });
    });

    //#endregion Initalize

    //#region Properties

    var cacheData = {};
    var delimiter = '/';//because we are using it later, we must always have one. Apparenly we can even have null keys, so use \x00 if we don't want a delimiter? I don't know if a c# key can contain a null.

    //From System.Web.Caching.CacheItemPriority (unlikely to change, probably doesn't need to be dynamic.)
    var cachePriority = {
        '1': 'Low',
        '2': 'BelowNormal',
        '3': 'Normal',//Also 'Default'
        '4': 'AboveNormal',
        '5': 'High',
        '6': 'NotRemovable'
    };

    ko.bindingHandlers.stopBinding = {
        init: function () {
            return { controlsDescendantBindings: true };
        }
    };

    //#endregion Properties

    //#region Templates

    //We would want the details info for these to start with.
    //ko.templates['CacheListTemplate'] = [
    //    ''
    //].join('');

    ko.templates['CacheTreeTemplate'] = [
        '<ul>',
            '<!-- ko foreach: CM.Sort(CM.ObjectAsArray($data.Children), CM.SortCacheKey) -->',//eww
            '<li>',
                '<button type="button" title="Delete Prefix" class="btn btn-xs btn-link" data-bind="click: CM.DeleteNode(Key, true)"><span class="fa fa-lg fa-trash-o"></span></button>',
                //'<button type="button" title="Serialize Prefix" class="btn btn-xs btn-link" data-bind="click: CM.SerializeNode(Key, true)"><span class="fa fa-lg fa-code"></span></button>',
                '<input type="checkbox" class="expand" data-bind="checked: ob_Checked, attr: { id: Id }, click: CM.BranchExpand" />',
                '<label data-bind="attr: { for: Id }"><span data-bind="text: Text"></span> <span class="delimiter" data-bind="text: $root.Delimiter"></span></label>',
                '<!-- ko template: { name: \'CacheTreeTemplate\', data: $data } --><!-- /ko -->',
            '</li>',
            '<!-- /ko -->',
            '<!-- ko foreach: CM.Sort($data.Values, CM.SortCacheKey) -->',//eww
            '<li>',//Make delete button last tab index?
                '<button type="button" title="Delete Entry" class="btn btn-xs btn-link" data-bind="click: CM.DeleteNode(Key)"><span class="fa fa-lg fa-trash-o"></span></button>',
                '<button type="button" title="View Entry Details" class="btn btn-xs btn-link" data-bind="click: CM.EntryDetails(Key)"><span class="fa fa-lg fa-info-circle"></span></button>',
                '<span data-bind="text: Text, attr: { title: Key }"></span>',// <span class="text-muted">(<span data-bind="text: Type"></span>)</span>
            '</li>',
            '<!-- /ko -->',
        '</ul>'
    ].join('');

    ko.templates['EntryDetailsTemplate'] = [
        '<div class="modal-header" tabindex="-1">',
            '<button type="button" class="close" data-dismiss="modal" aria-hidden="true"><span class="fa fa-times fa-fw"></span></button>',//styled ×?
            '<h4 class="modal-title" id="modal-title">Entry Details</h4>',
        '</div>',
        '<div class="modal-body">',








            '<textarea class="serialized-data" data-bind="text: Value" wrap="off" readonly></textarea>',
        '</div>',
        '<div class="modal-footer">',
            '<button type="button" class="btn btn-danger pull-left" data-bind="click: CM.DeleteNode(Key)">Delete</button>',//format, download, other buttons/actions?
            '<button type="button" class="btn btn-default" data-dismiss="modal">Close</button>',
        '</div>',
    ].join('');

    //ko.templates['SerializeNodeTemplate'] = [
    //    '<div class="modal-header" tabindex="-1">',
    //        '<button type="button" class="close" data-dismiss="modal" aria-hidden="true"><span class="fa fa-times fa-fw"></span></button>',//styled ×?
    //        '<h4 class="modal-title" id="modal-title">Serialized Data</h4>',
    //    '</div>',
    //    '<div class="modal-body">',
    //        '<textarea class="serialized-data" data-bind="text: Values" wrap="off" readonly></textarea>',
    //    '</div>',
    //    '<div class="modal-footer">',
    //        //'<a href="#" class="btn btn-default">Format</a>',//format, download, other buttons/actions?
    //        '<button type="button" class="btn btn-default" data-dismiss="modal">Close</button>',
    //    '</div>',
    //].join('');

    //#endregion Templates

    //#region Classes

    var CacheNode = function (key, text, ob_Checked, id) {//, ob_checked
        this.Key = key;
        this.Text = text;
        this.Children = {};//Nodes
        this.Values = [];//Values
        this.Id = id;
        this.ob_Checked = ob_Checked;//ko.observable(false);
        //this.ob_Checked = ko.computed({
        //    read: function () {
        //        return ob_checked();
        //    },
        //    write: function (value) {
        //        ob_checked(value);
        //    }
        //});
    };

    var CacheValue = function (cache, text, id) {
        this.Key = cache.Key;
        this.Text = text;
        this.Type = cache.Type;
        this.Id = id;
    };

    //#endregion Classes

    //#region Public Methods

    CM.SortCacheKey = function (node1, node2) {
        var key1 = node1.Key;
        var key2 = node2.Key;

        //What order do we want?
        if (key1 < key2) return -1;
        if (key1 > key2) return 1;
        return 0;
    };

    CM.FindCacheKey = function (key, op_prefix) {
        op_prefix = op_prefix || false;
        return function (node) {
            if (op_prefix)
                return node.Key.indexOf(key) === 0;
            return node.Key === key;
        };
    };

    CM.CopyrightYear = function () {
        return new Date().getUTCFullYear();
    };

    CM.Sort = function (array, op_sorter) {
        if ($.isFunction(op_sorter))
            return array.sort(op_sorter);
        return array.sort();
    };

    CM.Refresh = function () {
        $('.refresh-loading').removeClass('hidden');

        Ajax.Get('api/v1/cache')//'refresh' url? include freemem/other stats?
        .done(function (data) {
            cacheData.ob_Entries(data.Entries);
        });

        //TODO: Inline refresh (observables?)
        //document.location.reload();
    };

    CM.AfterRenderCacheTree = function () {//elements
        $('.refresh-loading').addClass('hidden');
        //other stuff?
    };

    CM.SetDelimiter = function (data) {
        //delimiter will already be set via two way input binding, this would also be caused by a refresh (use same button text?)
        //trigger re-draw of tree
        cacheData.ob_Delimiter(data.Delimiter);
    };

    CM.EnterDelimiter = function (data, e) {
        if (e.which === 13) {
            cacheData.ob_Delimiter(data.Delimiter);
        }
        return true;
    };

    CM.DeleteNode = function (key, op_prefix) {
        op_prefix = op_prefix || false;

        //Need to somehow keep checked state on redraw
        //Not sure how much I like spawning a new function for each one. Make each level a seperate observable? eww
        return function () {
            if (!op_prefix && (!cacheData.ConfirmDeleteKey || confirm('Are you sure you want to delete this cache value?\n\nKey: ' + key))) {
                Ajax.Post('api/v1/delete', { data: { Key: key } })
                .done(function (data) {
                    cacheData.ob_Entries.remove(CM.FindCacheKey(key))
                });
            } else if (op_prefix && (!cacheData.ConfirmDeletePrefix || confirm('Are you sure you want to delete everything with this prefix?\n\nPrefix: ' + key))) {
                Ajax.Post('api/v1/delete', { data: { Key: key, Prefix: true } })
                .done(function (data) {
                    cacheData.ob_Entries.remove(CM.FindCacheKey(key, true))
                });
            }
        };
    };

    //Store the response data against the key object for later? or JIT every time?
    CM.EntryDetails = function (key) {
        return function () {
            Ajax.Get('api/v1/details', { data: { Key: key } })
            .done(function (data) {
                data.Value = data.Value === 'undefined' ? 'Error serializing data.' : JSON.stringify(JSON.parse(data.Value), null, '    ');//eww, but the only way we can catch serialization errors without killing the wholer response is to serialize on the server.
                data.Priority = cachePriority[data.Priority] || 'Unknown';
                //moment.js? change to date format at binding level?
                //this.AbsoluteExpiration = cache.AbsoluteExpiration;
                //this.Created = cache.Created;
                //this.SlidingExpiration = cache.SlidingExpiration;

                //show serialized data
                //Make seperate modal methods? right now this the only usage.
                var $container = $('#modal-container');
                var $content = $container.find('.modal-content').first();
                var content = $content[0];
                ko.cleanNode(content);
                ko.applyBindings($.extend({}, data, { Template: 'EntryDetailsTemplate' }), content);
                $container.modal('show');
            });
        };
    };

    //CM.SerializeNode = function (key, op_prefix) {
    //    op_prefix = op_prefix || false;

    //    return function () {
    //        Ajax.Get('api/v1/info', { data: { Key: key, Prefix: op_prefix } })
    //        .done(function (data) {
    //            data.Values = JSON.stringify(data.Values, null, '    ');

    //            //show serialized data
    //            //Make seperate modal methods? right now this the only usage.
    //            var $container = $('#modal-container');
    //            var $content = $container.find('.modal-content').first();
    //            var content = $content[0];
    //            ko.cleanNode(content);
    //            ko.applyBindings($.extend({}, data, { Template: 'SerializeNodeTemplate' }), content);
    //            $container.modal('show');
    //        });
    //    };
    //};

    CM.ObjectAsArray = function (object) {
        var properties = [];

        for (child in object) {
            if (object.hasOwnProperty(child)) {
                properties.push(object[child]);
            }
        }

        return properties;
    };

    CM.BranchExpand = function (branch) {
        if (branch.ob_Checked() && cacheData.ExpandSingleBranches) {
            var current = branch;
            var children = Object.keys(current.Children);
            while (children.length === 1) {
                current = current.Children[children[0]];
                children = Object.keys(current.Children);
                if (!current.ob_Checked())
                    current.ob_Checked(true);
            }
        }
        return true;
    };

    //String.prototype.getHashCode = function () {
    //    //From Java (apparently), find better one?
    //    var hash = 0;
    //    if (this.length == 0) return hash;
    //    for (i = 0; i < this.length; i++) {
    //        char = this.charCodeAt(i);
    //        hash = ((hash << 5) - hash) + char;
    //        hash = hash & hash; // Convert to 32bit integer
    //    }
    //    return hash;
    //}

    //$.wait = function (ms) {
    //    var dfd = $.Deferred();
    //    setTimeout(function () { dfd.resolve(); }, ms);
    //    return dfd;
    //};

    //#endregion Public Methods

    //#region Private Methods

    var Ajax = {};

    var handleDataError = function (response) {//, textStatus, jqXHR) {
        var message = response.Message || "An unknown error has occured."
        //messageHandler(message);//toastr?
        console.log(message);
    };

    var handleFailError = function (jqXHR, textStatus, errorThrown) {
        var response = jqXHR.responseJSON || {};
        var message = response.Message || errorThrown || "An unknown error has occured."
        //messageHandler(message);//toastr?
        console.log(message);
    };

    Ajax.Post = function (url, options) {
        var opts = $.extend({}, { type: 'POST' }, options);
        return Ajax.Request(url, opts);
    };

    Ajax.Get = function (url, options) {
        var opts = $.extend({}, { type: 'GET' }, options);
        return Ajax.Request(url, opts);
    };

    var busyCounter = 0;
    Ajax.Request = function (url, options) {
        var dfd = $.Deferred();
        var busyClass = 'busy-' + busyCounter++;

        var defaults = {
            type: 'POST',
            dataType: 'json',
            data: null,
            traditional: true,// lets us post arrays as foo=1&foo=2 instead of foo[]=1&foo[]=2, which makes MVC happier
            //contentType: "application/x-www-form-urlencoded; charset=UTF-8",

            //messageHandler: function (message) { /*if (console && console.log) console.log(message);*/ Alerts.Messaging.Error(message); },
            //statusCode: {
            //    //Can we insert the options object into the jqXHR response somehow so we don't have to do this?
            //    400: function (jqXHR, textStatus, errorThrown) { handleServerError(jqXHR, textStatus, errorThrown, this.messageHandler, options.validator); },
            //    401: function (jqXHR, textStatus, errorThrown) { handleServerAuthorization(jqXHR, textStatus, errorThrown, this.messageHandler); },
            //    500: function (jqXHR, textStatus, errorThrown) { handleServerException(jqXHR, textStatus, errorThrown, this.messageHandler); }
            //},
            //handle client side error?
        };

        var opts = $.extend({}, defaults, options);

        $('html').addClass(busyClass);
        return $.ajax(url, opts)
        .done(function (response) {
            if (response.Success) {
                dfd.resolveWith(this, arguments);//proper context?
            } else {
                handleDataError(response);
                dfd.rejectWith(this, arguments);//proper context?
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            handleFailError(jqXHR, textStatus, errorThrown);//Use status code specific errors when applicable?
            dfd.rejectWith(this, arguments);//proper context?
        })
        .always(function () {
            $('html').removeClass(busyClass);
        });

        return dfd;
    };

    //Really messy chaining of deferreds but there is no way to block.
    //Ajax.Post = function (url, options) {
    //    var dfd = $.Deferred();
    //
    //    Ajax.GetVerificationToken('/VerificationToken')
    //    .always(function (token) {
    //        var opts = $.extend({}, { type: 'POST', headers: { 'VerificationToken': token } }, options);
    //        Ajax.Request(url, opts)
    //        .done(function () {
    //            dfd.resolveWith(this, arguments);//proper context?
    //        })
    //        .fail(function () {
    //            dfd.rejectWith(this, arguments);//proper context?
    //        });
    //    });
    //    //.fail(function () {
    //    //    dfd.rejectWith();//proper context and args?
    //    //});
    //
    //    return dfd.promise();
    //
    //    //var opts = $.extend({}, { type: 'POST', headers: { 'VerificationToken': token } }, options);
    //    //return Ajax.Request(url, opts);
    //};

    //var parseQuery = function (op_lowercase) {
    //    op_lowercase = op_lowercase || false;
    //    var query = document.location.search.substr(1);
    //    var parts = query.split('&');
    //    var parameters = {};
    //    for (var i = 0; i < parts.length; i++) {
    //        //If the value contains ='s, e.g. base64, we want to keep them.
    //        var param = parts[i].split('=');
    //        var key = param.shift();
    //        var value = param.join('=');

    //        if (op_lowercase)
    //            key = key.toLowerCase();

    //        parameters[key] = value;
    //    }
    //    return parameters;
    //};

    //#endregion Private Methods
}(window, window.jQuery));