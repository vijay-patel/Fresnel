var FresnelApp;
(function (FresnelApp) {
    // Taken from http://stackoverflow.com/a/25391043/80369
    function DisableAnchorDirective() {
        return {
            compile: function (tElement, tAttrs, transclude) {
                //Disable ngClick
                tAttrs["ngClick"] = ("ng-click", "!(" + tAttrs["aDisabled"] + ") && (" + tAttrs["ngClick"] + ")");
                //Toggle "disabled" to class when aDisabled becomes true
                return function (scope, iElement, iAttrs) {
                    scope.$watch(iAttrs["aDisabled"], function (newValue) {
                        if (newValue !== undefined) {
                            iElement.toggleClass("disabled", newValue);
                        }
                    });
                    //Disable href on click
                    iElement.on("click", function (e) {
                        if (scope.$eval(iAttrs["aDisabled"])) {
                            e.preventDefault();
                        }
                    });
                };
            }
        };
    }
    FresnelApp.DisableAnchorDirective = DisableAnchorDirective;
})(FresnelApp || (FresnelApp = {}));
//# sourceMappingURL=DisableAnchorDirective.js.map