/*
 * Bootstrap 3 -> Bootstrap 5 compatibility layer (JavaScript)
 * ------------------------------------------------------------
 * Load AFTER bootstrap.bundle.min.js. Keeps legacy Bootstrap 3 markup and theme (Metronic)
 * scripts working on Bootstrap 5.3:
 *
 *  1. Bootstrap 5 only reads data-bs-* attributes. Every legacy data-toggle / data-target /
 *     data-dismiss / data-placement / ... attribute gets a data-bs-* twin - for markup present at
 *     load, markup inserted later (MutationObserver) and right before a jQuery plugin call.
 *  2. Dropdowns keep Bootstrap 3's CSS positioning (data-bs-display="static") instead of Popper.
 *  3. Tabs: Bootstrap 3 marked the active <li>; Bootstrap 5 marks the toggle itself. Both are
 *     kept in sync so the theme CSS (li.active > a) and the Bootstrap 5 tab logic both work.
 *  4. Bootstrap 3 jQuery method names that Bootstrap 5 renamed ('destroy' -> 'dispose').
 *  5. Collapse/fade state classes: ".in" (BS3) is mirrored to ".show" (BS5) at load.
 */
(function () {
    'use strict';

    var ATTRS = ['toggle', 'target', 'dismiss', 'placement', 'parent', 'content', 'container',
        'html', 'trigger', 'backdrop', 'keyboard', 'delay', 'animation', 'offset', 'spy', 'ride',
        'slide', 'interval', 'boundary', 'title', 'original-title', 'template', 'selector', 'viewport'];
    var SELECTOR = ATTRS.map(function (a) { return '[data-' + a + ']'; }).join(',');

    function mirror(el) {
        if (!el || el.nodeType !== 1) return;
        for (var i = 0; i < ATTRS.length; i++) {
            var legacy = 'data-' + ATTRS[i];
            if (el.hasAttribute(legacy)) {
                var modern = 'data-bs-' + ATTRS[i];
                if (!el.hasAttribute(modern)) el.setAttribute(modern, el.getAttribute(legacy));
            }
        }
        if (el.getAttribute('data-bs-toggle') === 'dropdown' && !el.hasAttribute('data-bs-display')) {
            el.setAttribute('data-bs-display', 'static');
        }
    }

    function mirrorTree(root) {
        if (!root || !root.querySelectorAll) return;
        if (root.matches && root.matches(SELECTOR)) mirror(root);
        var list = root.querySelectorAll(SELECTOR);
        for (var i = 0; i < list.length; i++) mirror(list[i]);
    }

    function legacyStateClasses(root) {
        var scope = root && root.querySelectorAll ? root : document;
        // ".collapse.in" / ".fade.in" (BS3) -> also ".show" (BS5)
        var shown = scope.querySelectorAll('.collapse.in, .fade.in');
        for (var i = 0; i < shown.length; i++) shown[i].classList.add('show');
        // <li class="active"><a data-toggle="tab"> -> toggle itself must be .active for BS5
        var tabs = scope.querySelectorAll('li.active > [data-toggle="tab"], li.active > [data-toggle="pill"], li.active > [data-bs-toggle="tab"], li.active > [data-bs-toggle="pill"]');
        for (var j = 0; j < tabs.length; j++) tabs[j].classList.add('active');
    }

    function init() {
        mirrorTree(document.documentElement);
        legacyStateClasses(document);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    if (window.MutationObserver) {
        new MutationObserver(function (mutations) {
            for (var i = 0; i < mutations.length; i++) {
                var m = mutations[i];
                if (m.type === 'attributes') { mirror(m.target); continue; }
                for (var j = 0; j < m.addedNodes.length; j++) {
                    var n = m.addedNodes[j];
                    if (n.nodeType === 1) { mirrorTree(n); legacyStateClasses(n); }
                }
            }
        }).observe(document.documentElement, {
            childList: true, subtree: true, attributes: true,
            attributeFilter: ATTRS.map(function (a) { return 'data-' + a; })
        });
    }

    // Tabs: keep the BS3 "li.active" convention in sync with Bootstrap 5.
    document.addEventListener('shown.bs.tab', function (e) {
        var link = e.target;
        var li = link && link.parentNode;
        if (li && li.tagName === 'LI') {
            var siblings = li.parentNode ? li.parentNode.children : [];
            for (var i = 0; i < siblings.length; i++) siblings[i].classList.remove('active');
            li.classList.add('active');
        }
        if (e.relatedTarget) e.relatedTarget.classList.remove('active');
    });

    // Dropdowns: BS3 toggled ".open" on the parent; the theme CSS still keys off it.
    document.addEventListener('show.bs.dropdown', function (e) {
        var p = e.target && e.target.parentNode;
        if (p && p.classList) p.classList.add('open');
    });
    document.addEventListener('hide.bs.dropdown', function (e) {
        var p = e.target && e.target.parentNode;
        if (p && p.classList) p.classList.remove('open');
    });

    // jQuery plugin shims (Bootstrap 5 registers $.fn.modal/tooltip/... when jQuery is present).
    function patchJQuery() {
        var $ = window.jQuery;
        if (!$ || !$.fn || $.fn.__bs3CompatPatched) return;
        ['tooltip', 'popover', 'modal', 'collapse', 'dropdown', 'tab', 'alert', 'button'].forEach(function (name) {
            var original = $.fn[name];
            if (typeof original !== 'function') return;
            var wrapped = function (option) {
                this.each(function () { mirrorTree(this); });
                if (option === 'destroy') option = 'dispose';
                if (option === 'fixTitle') return this;
                var args = Array.prototype.slice.call(arguments);
                args[0] = option;
                return original.apply(this, args);
            };
            for (var key in original) {
                if (Object.prototype.hasOwnProperty.call(original, key)) wrapped[key] = original[key];
            }
            $.fn[name] = wrapped;
        });
        $.fn.__bs3CompatPatched = true;
    }
    // Bootstrap 5 attaches its jQuery plugins on DOMContentLoaded; patch right after that.
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () { setTimeout(patchJQuery, 0); patchJQuery(); });
    } else {
        patchJQuery();
    }
})();
