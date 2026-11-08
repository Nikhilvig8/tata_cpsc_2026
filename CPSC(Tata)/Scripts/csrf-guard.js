/*
 * Checkmarx "CSRF" (CWE-352) remediation - client half.
 *
 * Every page rendered through _Layout (and the few stand-alone pages that post) emits one
 * @Html.AntiForgeryToken() hidden input. This script makes sure that token travels with every
 * state-changing request, so the server-side AntiForgeryOnPostAttribute (global filter) and the
 * explicit [ValidateAntiForgeryToken] attributes can reject forged cross-site requests:
 *
 *  - HTML forms (method="post"): a hidden __RequestVerificationToken field is injected into every
 *    POST form on the page - at load, into forms added later (MutationObserver), on submit, and
 *    before programmatic form.submit() calls (which never fire a submit event).
 *  - XMLHttpRequest (covers every jQuery $.ajax/$.post call, regardless of which of the page's
 *    jQuery copies issued it): for same-origin non-GET requests the token is added as a request
 *    header AND, for url-encoded/FormData bodies, as a body field - the built-in
 *    [ValidateAntiForgeryToken] only reads Request.Form.
 *
 * Kept dependency-free (no jQuery) and loaded as an external file so it satisfies the CSP.
 */
(function () {
    'use strict';

    var FIELD = '__RequestVerificationToken';
    var HEADER = 'RequestVerificationToken';

    function getToken() {
        var el = document.querySelector('input[name="' + FIELD + '"]');
        return el ? el.value : null;
    }

    function isSameOrigin(url) {
        try {
            var a = document.createElement('a');
            a.href = url;
            return !a.host || a.host === window.location.host;
        } catch (e) {
            return false;
        }
    }

    function ensureFormToken(form) {
        if (!form || !form.getAttribute) return;
        var method = (form.getAttribute('method') || 'get').toLowerCase();
        if (method !== 'post') return;
        var action = form.getAttribute('action') || window.location.href;
        if (!isSameOrigin(action)) return;
        var token = getToken();
        if (!token) return;
        var existing = form.querySelector('input[name="' + FIELD + '"]');
        if (existing) {
            if (!existing.value) existing.value = token;
            return;
        }
        var input = document.createElement('input');
        input.type = 'hidden';
        input.name = FIELD;
        input.value = token;
        form.appendChild(input);
    }

    function scanForms(root) {
        var forms = (root || document).querySelectorAll ? (root || document).querySelectorAll('form') : [];
        for (var i = 0; i < forms.length; i++) ensureFormToken(forms[i]);
    }

    // Forms present at load and added later.
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () { scanForms(document); });
    } else {
        scanForms(document);
    }
    if (window.MutationObserver) {
        new MutationObserver(function (mutations) {
            for (var i = 0; i < mutations.length; i++) {
                var added = mutations[i].addedNodes;
                for (var j = 0; j < added.length; j++) {
                    var node = added[j];
                    if (node.nodeType !== 1) continue;
                    if (node.tagName === 'FORM') ensureFormToken(node);
                    else scanForms(node);
                }
            }
        }).observe(document.documentElement, { childList: true, subtree: true });
    }

    // User-initiated submits (capture phase, before any page handler).
    document.addEventListener('submit', function (e) { ensureFormToken(e.target); }, true);

    // Programmatic submits: form.submit() bypasses the submit event entirely.
    var nativeSubmit = HTMLFormElement.prototype.submit;
    HTMLFormElement.prototype.submit = function () {
        ensureFormToken(this);
        return nativeSubmit.apply(this, arguments);
    };

    // XMLHttpRequest (jQuery AJAX).
    var nativeOpen = XMLHttpRequest.prototype.open;
    var nativeSend = XMLHttpRequest.prototype.send;
    var nativeSetHeader = XMLHttpRequest.prototype.setRequestHeader;

    XMLHttpRequest.prototype.open = function (method, url) {
        this.__csrfMethod = (method || 'GET').toUpperCase();
        this.__csrfSameOrigin = isSameOrigin(url);
        this.__csrfContentType = null;
        return nativeOpen.apply(this, arguments);
    };

    XMLHttpRequest.prototype.setRequestHeader = function (name, value) {
        if (name && name.toLowerCase() === 'content-type') this.__csrfContentType = String(value).toLowerCase();
        return nativeSetHeader.apply(this, arguments);
    };

    XMLHttpRequest.prototype.send = function (body) {
        var m = this.__csrfMethod;
        if (this.__csrfSameOrigin && m && m !== 'GET' && m !== 'HEAD' && m !== 'OPTIONS') {
            var token = getToken();
            if (token) {
                try { nativeSetHeader.call(this, HEADER, token); } catch (e) { /* already sent */ }
                if (window.FormData && body instanceof FormData) {
                    if (!body.has || !body.has(FIELD)) body.append(FIELD, token);
                } else if (typeof body === 'string' || body === null || body === undefined) {
                    var ct = this.__csrfContentType;
                    if (!ct) {
                        // No explicit type: browser will send text/plain unless we set the form type.
                        nativeSetHeader.call(this, 'Content-Type', 'application/x-www-form-urlencoded; charset=UTF-8');
                        ct = 'application/x-www-form-urlencoded';
                    }
                    if (ct.indexOf('application/x-www-form-urlencoded') === 0) {
                        var str = body ? String(body) : '';
                        if (str.indexOf(FIELD + '=') === -1) {
                            body = (str ? str + '&' : '') + FIELD + '=' + encodeURIComponent(token);
                        }
                    }
                }
            }
        }
        return nativeSend.call(this, body);
    };
})();
