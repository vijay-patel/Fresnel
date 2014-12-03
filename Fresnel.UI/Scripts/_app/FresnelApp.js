var FresnelApp;
(function (FresnelApp) {
    var ExplorerController = (function () {
        function ExplorerController($scope) {
            $scope.message = { title: "Goodbye!!" };
        }
        return ExplorerController;
    })();
    FresnelApp.ExplorerController = ExplorerController;
})(FresnelApp || (FresnelApp = {}));
var FresnelApp;
(function (FresnelApp) {
    var ToolboxController = (function () {
        function ToolboxController($scope, $http) {
            $scope.message = { title: "Hello World!!" };
            $scope.loadClassHierarchy = function () {
                var _this = this;
                var uri = "api/Toolbox/GetClassHierarchy";
                $http.get(uri).success(function (data, status) { return _this.classHierarchy = data; });
            };
            $scope.create = function (fullyQualifiedName) {
                var uri = "api/Toolbox/Create";
                var arg = "=" + fullyQualifiedName;
                var config = {
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' }
                };
                $http.post(uri, arg, config).success(function (data, status) { return $scope.newInstance = data; });
            };
        }
        return ToolboxController;
    })();
    FresnelApp.ToolboxController = ToolboxController;
})(FresnelApp || (FresnelApp = {}));
var FresnelApp;
(function (FresnelApp) {
    angular.module("fresnelApp", ["pageslide-directive"]).controller("toolboxController", FresnelApp.ToolboxController).controller("explorerController", FresnelApp.ExplorerController);
})(FresnelApp || (FresnelApp = {}));
