/*! Copyright 2014-2015 Tacklr, LLC */
/* tslint:disable:max-line-length */
/* tslint:disable:comment-format */
/* tslint:disable:quotemark */
/// <reference path="typescript/jquery-fix.d.ts" />
/// <reference path="typescript/bootstrap-fix.d.ts" />
/// <reference path="typescript/knockout-fix.d.ts" />
/// <reference path="typescript/toastr-fix.d.ts" />


;
(function (window, $, ko, toastr, Prism, undefined) {
    //#region Initalize
    window.CM = window.CM || {};
    var CM = window.CM;

    var docReady = $.Deferred();
    $(docReady.resolve);

    var checkedState = {};

    toastr.options = {
        closeButton: true,
        closeHtml: '<button type="button"><span class="fa fa-times fa-fw"></span></button>',
        //debug: false,//default
        //positionClass: 'toast-top-right',//default
        //onclick: null,//default
        //showDuration: 300,//default
        //hideDuration: 1000,//default
        //timeOut: 5000,//default
        //extendedTimeOut: 1000,//default
        //showEasing: 'swing',//default
        hideEasing: 'linear'
    };

    //#endregion Initalize
    //#region Properties
    var canResize = 'resize' in window.document.body.style;
    var cacheData = {};
    var delimiter = '/';

    //From System.Web.Caching.CacheItemPriority (unlikely to change, probably doesn't need to be dynamic.)
    var cachePriority = {
        '1': 'Low',
        '2': 'Below Normal',
        '3': 'Normal',
        '4': 'Above Normal',
        '5': 'High',
        '6': 'Not Removable'
    };

    //ko.bindingHandlers.stopBinding = {
    //    init: function () {
    //        return { controlsDescendantBindings: true };
    //    }
    //};
    //#endregion Properties
    //#region Templates
    //We would want the details info for these to start with.
    //ko.templates['CacheListTemplate'] =
    //    '<tr>' +
    //        '<td>' +
    //            '<span class="btn-group">' +
    //                '<button type="button" title="Delete Entry" class="btn btn-xs btn-danger" data-bind="click: CM.DeleteNode(Key)"><span class="fa fa-lg fa-trash-o"></span></button>' +
    //                '<button type="button" title="View Entry Details" class="btn btn-xs btn-info" data-bind="click: CM.EntryDetails(Key)"><span class="fa fa-lg fa-info-circle"></span></button>' +
    //            '</span>' +
    //        '</td>' +
    //        '<td data-bind="text: Key"></td>' +
    //        '<td data-bind="text: Type"></td>' +
    //        //other info
    //    '</tr>';
    ko.templates['CacheTreeTemplate'] = '<!-- ko if: !$data.isEmpty() -->' + '<ul>' + '<!-- ko foreach: CM.Sort(CM.ObjectAsArray($data.Children), CM.SortCacheKey) -->' + '<li>' + '<button type="button" title="Delete Prefix" class="btn btn-xs btn-link" data-bind="click: CM.DeleteNode(Key, true)"><span class="fa fa-lg fa-trash-o"></span></button>' + '<input type="checkbox" class="expand" data-bind="checked: ob_Checked, attr: { id: Id }, click: CM.BranchExpand" />' + '<label data-bind="attr: { for: Id }"><span data-bind="text: Text"></span> <span class="delimiter" data-bind="text: $root.Delimiter"></span></label>' + '<!-- ko template: { name: "CacheTreeTemplate", data: $data } --><!-- /ko -->' + '</li>' + '<!-- /ko -->' + '<!-- ko foreach: CM.Sort($data.Values, CM.SortCacheKey) -->' + '<li>' + '<button type="button" title="Delete Entry" class="btn btn-xs btn-link" data-bind="click: CM.DeleteNode(Key)"><span class="fa fa-lg fa-trash-o"></span></button>' + '<button type="button" title="View Entry Details" class="btn btn-xs btn-link" data-bind="click: CM.EntryDetails(Key)"><span class="fa fa-lg fa-info-circle"></span></button>' + '<span data-bind="text: Text, attr: { title: Key }"></span>' + '</li>' + '<!-- /ko -->' + '</ul>' + '<!-- /ko -->';

    ko.templates['EntryDetailsTemplate'] = '<div class="modal-header" tabindex="-1">' + '<button type="button" class="close" data-dismiss="modal" aria-hidden="true"><span class="fa fa-times fa-fw"></span></button>' + '<h4 class="modal-title" id="modal-title">Entry Details</h4>' + '</div>' + '<div class="modal-body">' + '<div class="row">' + '<div class="col-xs-12">' + '<div class="panel panel-default">' + '<div class="table-responsive">' + '<table class="table table-bordered table-condensed">' + '<tbody>' + '<tr>' + '<th class="col-fit text-right">Key</th>' + '<td class="break-all" data-bind="text: Key"></td>' + '</tr>' + '<tr>' + '<th class="col-fit text-right">Type</th>' + '<td class="break-all" data-bind="text: Type"></td>' + '</tr>' + '<tr>' + '<th class="col-fit text-right">Priority</th>' + '<td data-bind="text: Priority"></td>' + '</tr>' + '<tr>' + '<th class="col-fit text-right">Created</th>' + '<td data-bind="text: Created"></td>' + '</tr>' + '<tr>' + '<th class="col-fit text-right">Absolute Expiration</th>' + '<td data-bind="text: AbsoluteExpiration"></td>' + '</tr>' + '<tr>' + '<th class="col-fit text-right">Sliding Expiration</th>' + '<td data-bind="text: SlidingExpiration"></td>' + '</tr>' + '</tbody>' + '</table>' + '</div>' + '</div>' + '</div>' + '</div>' + '<div class="row">' + '<div class="col-xs-12">' + (!canResize ? '<textarea class="serialized-data" data-bind="text: Value" wrap="off" readonly></textarea>' : '<pre class="serialized-data" data-bind="html: ValueHtml"></pre>') + '</div>' + '</div>' + '</div>' + '<div class="modal-footer">' + '<button type="button" class="btn btn-danger pull-left" data-bind="click: CM.DeleteNode(Key)">Delete</button>' + '<button type="button" class="btn btn-default" data-dismiss="modal">Close</button>' + '</div>';

    //ko.templates['SerializeNodeTemplate'] =
    //    '<div class="modal-header" tabindex="-1">' +
    //        '<button type="button" class="close" data-dismiss="modal" aria-hidden="true"><span class="fa fa-times fa-fw"></span></button>' +//styled ×?
    //        '<h4 class="modal-title" id="modal-title">Serialized Data</h4>' +
    //    '</div>' +
    //    '<div class="modal-body">' +
    //        '<textarea class="serialized-data" data-bind="text: Values" wrap="off" readonly></textarea>' +
    //    '</div>' +
    //    '<div class="modal-footer">' +
    //        //'<a href="#" class="btn btn-default">Format</a>' +//format, download, other buttons/actions?
    //        '<button type="button" class="btn btn-default" data-dismiss="modal">Close</button>' +
    //    '</div>';
    //#endregion Templates
    //#region Classes
    var CacheNode = function (key, text, ob_Checked, id) {
        this.Key = key;
        this.Text = text;
        this.Children = {}; //Nodes
        this.Values = []; //Values
        this.Id = id;
        this.ob_Checked = ob_Checked;
        this.isEmpty = function () {
            return this.Values.length === 0 && $.isEmptyObject(this.Children);
        };
    };

    var CacheValue = function (cache, text, id) {
        this.Key = cache.Key;
        this.Text = text;
        this.Type = cache.Type;
        this.Id = id;
    };

    //#endregion Classes
    //#region Public Methods
    CM.ClearCache = function () {
        if (confirm('Are you sure you want to clear the cache?')) {
            API.Post('/clear').done(function (data) {
                cacheData.ob_Entries([]);
                cacheData.ob_Count(0);
                toastr.success('Cache has been cleared.');
            });
        }
    };

    CM.PageCache = function () {
        //Modal?
        var url = prompt('Enter the relative url (e.g. /foo/bar) to remove all output cache entries.');
        if (url.indexOf('/') === 0) {
            API.Post('/page', { data: { Url: url } }).done(function (data) {
                cacheData.ob_Entries([]);
                toastr.success('Output cache cleared.');
            });
        } else {
            toastr.error('Url must start with a "/".');
        }
    };

    CM.SortCacheKey = function (node1, node2) {
        var key1 = node1.Key;
        var key2 = node2.Key;

        //What order do we want?
        return (key1 < key2) ? -1 : (key1 > key2) ? 1 : 0;
    };

    CM.FindCacheKey = function (key, op_prefix) {
        op_prefix = op_prefix || false;
        return function (node) {
            if (op_prefix) {
                return node.Key.indexOf(key) === 0;
            }
            return node.Key === key;
        };
    };

    CM.CopyrightYear = function () {
        return new Date().getUTCFullYear();
    };

    CM.Sort = function (array, op_sorter) {
        if ($.isFunction(op_sorter)) {
            return array.sort(op_sorter);
        }
        return array.sort();
    };

    CM.Refresh = function () {
        $('.refresh-loading').removeClass('hidden');

        API.Get('/cache').done(function (data) {
            cacheData.ob_Count(data.Count);
            cacheData.ob_Entries(data.Entries);
        });
    };

    CM.AfterRenderDetailView = function () {
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
            //TODO: Combine these calls, I don't want to seperate ones.
            if (!op_prefix && (!cacheData.ConfirmDeleteKey || confirm('Are you sure you want to delete this cache value?\n\n' + key))) {
                API.Post('/delete', { data: { Key: key } }).done(function (data) {
                    cacheData.ob_Count(data.Count);
                    cacheData.ob_Entries.remove(CM.FindCacheKey(key));
                });
            } else if (op_prefix && (!cacheData.ConfirmDeletePrefix || confirm('Are you sure you want to delete everything with this prefix?\n\n' + key))) {
                API.Post('/delete', { data: { Key: key, Prefix: true } }).done(function (data) {
                    cacheData.ob_Count(data.Count);
                    cacheData.ob_Entries.remove(CM.FindCacheKey(key, true));
                });
            }
        };
    };

    //Store the response data against the key object for later? or JIT every time?
    CM.EntryDetails = function (key) {
        return function () {
            API.Get('/details', { data: { Key: key } }).done(function (data) {
                //better data transformer? viewmodel constructor?
                //data.Value = '{"Alerts":[{"AlertId":16,"Headline":"AlertWire Launching Soon!","Message":"We are happy to announce the imminent launch of AlertWire, the easiest way to get the word out to your web sites\' visitors.","Url":"http://www.alertwi.re/","UrlText":"Click Here to Learn More","BackgroundColor":"D0E0E3","IconColor":"FF0000","TextColor":"000000","Icon":"alert-system-i-spam","Closing":1,"Method":0},{"AlertId":26,"Headline":"HONESTY IN PET FOOD.","Message":"Purina believes that honesty is the most important ingredient in the relationship between pet owners and pet food manufacturers. Please visit www.petfoodhonesty.com to learn more about actions we are taking to stop false advertising aimed at pet owners.","Url":"http://www.petfoodhonesty.com","UrlText":"Click Here to Learn More","BackgroundColor":"85C569","IconColor":"F1C232","TextColor":"000000","Icon":"alert-system-i-search","Closing":1,"Method":0}],"CssNamespace":"alert-system","CssUrl":"http://api.dev.noticegiver.com/Core/core-wip.min.css?_=D251D0443E84D050CD56F43363DD0D3785FA7F7E","Preview":false,"Audit":false,"Success":true,"Errors":{},"Message":null}';
                if (data.ValueError) {
                    data.Value = data.ValueError || 'Unknown error serializing data.';
                } else {
                    data.Value = JSON.stringify(JSON.parse(data.Value), null, 4); //eww, but the only way we can catch serialization errors without killing the wholer response is to serialize on the server. Make space count configurable?
                    data.ValueHtml = Prism.highlight(data.Value, Prism.languages.json);
                }

                data.Priority = cachePriority[data.Priority] || 'Unknown';

                //moment.js? config setting? change to date format at binding level?
                data.AbsoluteExpiration = data.AbsoluteExpiration === null ? 'None' : new Date(data.AbsoluteExpiration).toLocaleString();
                data.Created = new Date(data.Created).toLocaleString();
                data.SlidingExpiration = data.SlidingExpiration === null ? 'None' : (data.SlidingExpiration / 1000) + " Seconds"; //Need better timespan formatting.

                //data.act_DataBlur = function (model, e) {
                //    var $this = $(e.target);
                //    var $sibling = $this.siblings('pre').first();
                //    if ($sibling.length > 0) {
                //        $this.hide();
                //        $sibling.show();
                //    }
                //};
                //data.act_DataFocus = function (model, e) {
                //    var $this = $(e.target);
                //    var $sibling = $this.siblings('textarea').first();
                //    if ($sibling.length > 0) {
                //        $this.hide();
                //        $sibling.show().focus();
                //    }
                //};
                //show serialized data
                //Make seperate modal methods? right now this the only usage.
                var modal = Modal.Generate('EntryDetailsTemplate');
                ko.applyBindings($.extend({}, data), modal[0]);
                modal.modal('show');
            });
        };
    };

    //CM.SerializeNode = function (key, op_prefix) {
    //    op_prefix = op_prefix || false;
    //    return function () {
    //        API.Get('/info', { data: { Key: key, Prefix: op_prefix } })
    //        .done(function (data): void {
    //            data.Values = JSON.stringify(data.Values, null, 4);
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

        for (var child in object) {
            /* tslint:enable:typedef */
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
                if (!current.ob_Checked()) {
                    current.ob_Checked(true);
                }
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
    var Modal = {};

    Modal.Generate = function (template, options) {
        //Deferred?
        var defaults = {
            Size: '',
            Animate: true
        };
        var opts = $.extend({}, defaults, options);

        //Right now we only allow one modal to be open by closing any previous. Should we allow stacking? Should we fail the new one?
        var $existing = $('#modal-container');
        if ($existing.hasClass('in')) {
            $existing.modal('hide');
        } else {
            $existing.remove();
        }

        var $modalContainer = $('<div id="modal-container" class="modal ' + (opts.Animate ? 'fade' : '') + '" tabindex="-1" role="dialog" aria-hidden="true" aria-labelledby="modal-title">' + '<div class="modal-dialog' + (opts.Size ? ' ' + opts.Size : '') + '">' + '<div class="modal-content" data-bind="template: { name: \'' + template + '\' }"></div>' + '</div>' + '</div>').one('hidden.bs.modal', function () {
            $(this).remove();
        });
        return $modalContainer;
    };

    var Ajax = {};

    var handleDataError = function (response) {
        var message = response.Message || "An unknown error has occured.";

        //messageHandler(message);//toastr?
        toastr.error(message);
    };

    var handleFailError = function (jqXHR, textStatus, errorThrown) {
        var response = jqXHR.responseJSON || {};
        var message = response.Message || errorThrown || "An unknown error has occured.";

        //messageHandler(message);//toastr?
        toastr.error(message);
    };

    var nonce = function () {
        /* tslint:disable:no-bitwise */
        return parseInt(new Date().getTime().toString() + ((Math.random() * 1e5) | 0), 10).toString(36);
        /* tslint:enable:no-bitwise */
    };

    Ajax.Post = function (url, options) {
        var opts = $.extend({}, { type: 'POST' }, options);
        return Ajax.Request(url, opts);
    };

    var promiseCache = {};
    var getVerificationToken = function (url, op_timeout) {
        var promise = promiseCache[url];
        if (promise) {
            return promise;
        }

        var delay = op_timeout || 30 * 1000;
        var src = url + (url.indexOf('?') > -1 ? "&" : "?") + "_=" + nonce();
        var dfd = $.Deferred();

        if ($.active++ === 0) {
            $.event.trigger('ajaxStart');
        }

        var $iframe = $('<iframe src="' + src + '" style="height: 0; width: 0; border: 0;">').on('load', function () {
            var token = $(this).contents().find('#VerificationToken').val();
            if (token) {
                Ajax.VerificationTokens[url] = token;
                dfd.resolve(token);
            } else {
                dfd.reject();
            }

            $iframe.remove();
        }).appendTo('body');

        setTimeout(function () {
            if (dfd.state() === 'pending') {
                dfd.reject(); //timeout
                $iframe.remove();
            }
        }, delay);

        promise = promiseCache[url] = dfd.promise().fail(function () {
            promiseCache[url] = null;
            toastr.error('Error loading verification token.');
        }).always(function () {
            if (!(--$.active)) {
                $.event.trigger('ajaxStop');
            }
        });

        return promise;
    };

    //Really messy chaining of deferreds but there is no way to block.
    Ajax.Post = function (url, options) {
        var dfd = $.Deferred();

        getVerificationToken('VerificationToken').always(function (token) {
            var opts = $.extend({}, { type: 'POST', headers: { 'X-CSRF-Token': token } }, options);
            Ajax.Request(url, opts).done(function () {
                dfd.resolveWith(this, arguments); //proper context?
            }).fail(function () {
                dfd.rejectWith(this, arguments); //proper context?
            });
        });

        return dfd.promise();
    };

    Ajax.Get = function (url, options) {
        var opts = $.extend({}, { type: 'GET' }, options);
        return Ajax.Request(url, opts);
    };

    var busyCounter = 0;
    Ajax.Request = function (url, options) {
        var dfd = $.Deferred();

        var defaults = {
            type: 'POST',
            dataType: 'json',
            data: null
        };

        var opts = $.extend({}, defaults, options);

        return $.ajax(url, opts).done(function (response) {
            if (response.Success) {
                dfd.resolveWith(this, arguments); //proper context?
            } else {
                handleDataError(response);
                dfd.rejectWith(this, arguments); //proper context?
            }
        }).fail(function (jqXHR, textStatus, errorThrown) {
            handleFailError(jqXHR, textStatus, errorThrown); //Use status code specific errors when applicable?
            dfd.rejectWith(this, arguments); //proper context?
        });
    };

    var API = {};

    API.Get = function (url, options) {
        return Ajax.Get('api/v1' + url, options);
    };

    API.Post = function (url, options) {
        return Ajax.Post('api/v1' + url, options);
    };

    //Add loader bar like on AlertWire?
    $(document).ajaxStart(function () {
        $('html').addClass('busy');
        //NProgress.start();
    }).ajaxStop(function () {
        $('html').removeClass('busy');
        //NProgress.done();
    });

    //$('#collapseOne, #collapseTwo').on('show.bs.collapse', function () {
    //    API.Get('/cache')//'refresh' url? include freemem/other stats?
    //    .done(function (data) {
    //        cacheData.ob_Entries(data.Entries);
    //    });
    //    return false;
    //});
    //Really messy chaining of deferreds but there is no way to block.
    //Ajax.Post = function (url, options) {
    //    var dfd = $.Deferred();
    //
    //    getVerificationToken('/VerificationToken')
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
    //#region Start
    $.when(API.Get('/combined'), docReady).done(function (combined) {
        //var params = parseQuery(true);//save in local storage instead?
        var data = combined[0];

        //TODO: Escape non-printable character?
        //data.Delimiter = params['delimiter'] || data.Delimiter || delimiter;
        //Better way to do these transforms?
        data.MemoryLimit = data.MemoryLimit === null ? 'Unknown' : data.MemoryLimit === -1 ? 'Unlimited' : data.MemoryLimit;
        data.MemoryLimitPercent = data.MemoryLimitPercent === null ? 'Unknown' : data.MemoryLimitPercent;
        data.ob_Count = ko.observable(data.Count);
        data.ob_Delimiter = ko.observable(data.Delimiter);
        data.ob_DetailView = ko.observable(data.DetailView);
        data.ob_Entries = ko.observableArray(data.Entries);
        data.ob_EntryTree = ko.computed(function () {
            //Can we template directly off the array somehow instead of building our tree here?
            //TODO: need to somehow keep checked state
            var CacheRoot = {
                Children: {},
                Values: [],
                isEmpty: function () {
                    return this.Values.length === 0 && $.isEmptyObject(this.Children);
                }
            };

            var id_i = 0;
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
                        var currentKey = currentKeyParts.join(delimiter) + delimiter;

                        //Can this be done more cleanly?
                        if (checkedState[currentKey] === undefined) {
                            checkedState[currentKey] = ko.observable(false);
                        }

                        current.Children[keyPart] = new CacheNode(currentKey, keyPart, checkedState[currentKey], 'item-' + id_i++); //need to get the subkey up to this point
                    }
                    current = current.Children[keyPart];
                }
                current.Values.push(new CacheValue(cache, keyParts.pop(), 'item-' + id_i++)); //TODO: Prevent duplicates
            });

            return CacheRoot;
        });

        var deferred = data.DetailView === 'Defer';
        if (deferred) {
            $('#collapse-tree').one('show.bs.collapse', function () {
                //just trigger refresh button?
                $('.refresh-loading').removeClass('hidden');

                API.Get('/cache').done(function (response) {
                    data.ob_Entries(response.Entries);
                });
            });
        }

        ko.applyBindings(data); //Tree parts lose open state on delete, need to save the state somehow.
        cacheData = data;

        //window.DERP = data;
        //Clear loading indiciator
        $('.content-loading').fadeOut(function () {
            $(this).remove();
        });
    });
    //#endregion Start
}(window, window.jQuery, window.ko, window.toastr, window.Prism));
