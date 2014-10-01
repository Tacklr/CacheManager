//TODO: Add copyright
//TODO: Test for large trees
//integrate https://square.github.io/crossfilter/ or similar for working with large data? not sure how tree generation/reading would work with it.
//TODO:
//what about empties , e.g. blah//blah
//or keys that end with the delimiter?
//Keep open state on data change/reload?
//TODO: Put wrappers around ajax so we can do things like error handling, busy indicator (tokens?)
; (function (base, $, undefined) {
    //#region Initalize

    base.CM = base.CM || {};
    var CM = base.CM;

    var docReady = $.Deferred();
    $(docReady.resolve);

    $.when($.get('api/v1/combined'), docReady)
    .done(function (combined) {
        //var params = parseQuery(true);//save in local storage instead?
        var data = combined[0];

        //TODO: Escape non-printable character?
        //data.Delimiter = params['delimiter'] || data.Delimiter || delimiter;
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
            //Build our tree, any advantage to doing it server side?
            $.each(data.ob_Entries(), function (i, cache) {
                var key = cache.Key;
                var keyParts = key.split(data.ob_Delimiter() || null).reverse();
                var current = CacheRoot;
                var currentKey = [];
                while (keyParts.length > 1) {
                    var keyPart = keyParts.pop();
                    currentKey.push(keyPart);
                    if (current.Children[keyPart] === undefined)
                        current.Children[keyPart] = new CacheNode(currentKey.join(data.ob_Delimiter()) + data.ob_Delimiter(), keyPart, 'item-' + id_i++);//need to get the subkey up to this point
                    current = current.Children[keyPart];
                }
                current.Values.push(new CacheValue(key, keyParts.pop(), 'item-' + id_i++));//TODO: Prevent duplicates
            });

            return CacheRoot;
        });

        ko.applyBindings(data);//Tree parts lose open state on delete, need to save the state somehow.
        cacheData = data;

        //Clear loading indiciator
        $('.content-loading').fadeOut(function () {
            $(this).remove();
        });
    });

    //#endregion Initalize

    //#region Classes

    var CacheNode = function (key, text, id) {
        this.Key = key;
        this.Text = text;
        this.Children = {};//Nodes
        this.Values = [];//Values
        this.Id = id;
        this.ob_Checked = ko.observable(false);
    };

    var CacheValue = function (key, text, id) {
        this.Key = key;
        this.Text = text;
        this.Id = id;
    };

    //#endregion Classes

    //#region Properties

    var cacheData = {};
    var delimiter = '/';//because we are using it later, we must always have one. Apparenly we can even have null keys, so use \x00 if we don't want a delimiter? I don't know if a c# key can contain a null.

    //#endregion Properties

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

        $.get('api/v1/cache')//'refresh' url? include freemem/other stats?
        .done(function (data) {
            cacheData.ob_Entries(data.Entries);
        });

        //TODO: Inline refresh (observables?)
        //document.location.reload();
    };

    CM.ClearRefreshSpinner = function () {
        $('.refresh-loading').addClass('hidden');
    };

    CM.GoBack = function () {
        document.location.href = './';
        //try {
        //    history.go(-1);
        //}
        //catch (e) { }
        //$.wait(100).then(function () {
        //    document.location = './';
        //});
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
                $.post('api/v1/delete', { Key: key })
                .done(function (data) {
                    if (data.Success) {
                        //delete
                        cacheData.ob_Entries.remove(CM.FindCacheKey(key))
                    } else {
                        //error
                    }
                });
            } else if (op_prefix && (!cacheData.ConfirmDeletePrefix || confirm('Are you sure you want to delete everything with this prefix?\n\nPrefix: ' + key))) {
                $.post('api/v1/delete', { Key: key, Prefix: true })
                .done(function (data) {
                    if (data.Success) {
                        //delete
                        cacheData.ob_Entries.remove(CM.FindCacheKey(key, true))
                    } else {
                        //error
                    }
                });
            }
        };
    };

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