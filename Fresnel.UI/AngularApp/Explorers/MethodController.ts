﻿/// <reference path="../../scripts/typings/angular-ui-bootstrap/angular-ui-bootstrap.d.ts" />

module FresnelApp {

    export class MethodController {

        static $inject = [
            '$rootScope',
            '$scope',
            'fresnelService',
            'appService',
            'explorerService',
            'requestBuilder',
            'explorer',
            'method'];

        constructor(
            $rootScope: ng.IRootScopeService,
            $scope: IMethodControllerScope,
            fresnelService: IFresnelService,
            appService: AppService,
            explorerService: ExplorerService,
            requestBuilder: RequestBuilder,
            explorer: Explorer,
            method: MethodVM) {

            $scope.explorer = explorer;
            $scope.method = method;

            method.ParametersSetByUser = [];

            $scope.invoke = function (method: MethodVM) {
                var request = requestBuilder.buildMethodInvokeRequest(method);
                var promise = fresnelService.invokeMethod(request);

                promise.then((promiseResult) => {
                    var response = promiseResult.data;
                    method.Error = response.Passed ? "" : response.Messages[0].Text;

                    appService.identityMap.merge(response.Modifications);
                    $rootScope.$broadcast("messagesReceived", response.Messages);

                    if (response.ResultObject) {
                        $rootScope.$broadcast("openNewExplorer", response.ResultObject);
                    }
                });
            }

            $scope.setProperty = function (param: ParameterVM) {
                // Find the parameter, and set it's value:
                // If it's an Object, set the ReferenceValueID
                if (!param.IsNonReference) {
                    param.State.ReferenceValueID = param.State.Value.ID;
                }

                var matches = $.grep(method.ParametersSetByUser, function (e) {
                    return e == param;
                });
                if (matches.length == 0) {
                    method.ParametersSetByUser.push(param);
                }
            }

            $scope.setBitwiseEnumProperty = function (param: ParameterVM, enumValue: number) {
                param.State.Value = param.State.Value ^ enumValue;
                $scope.setProperty(param);
            }

            $scope.cancel = function () {
                //modalInstance.dismiss();
            }

        }

    }
}
