﻿module FresnelApp {

    export class ToolboxController {
        public classHierarchy: ClassItem[];

        static $inject = [
            '$rootScope',
            '$scope',
            'fresnelService',
            'requestBuilder',
            'appService'];

        constructor(
            $rootScope: ng.IRootScopeService,
            $scope: IToolboxControllerScope,
            fresnelService: IFresnelService,
            requestBuilder: RequestBuilder,
            appService: AppService) {

            $scope.loadClassHierarchy = function () {
                var promise = fresnelService.getClassHierarchy();

                promise.then((promiseResult) => {
                    var response = promiseResult.data;

                    appService.identityMap.merge(response.Modifications);
                    $rootScope.$broadcast(UiEventType.MessagesReceived, response.Messages);

                    this.classHierarchy = promiseResult.data;
                });
            }

            $scope.create = function (fullyQualifiedName: string) {
                var promise = fresnelService.createObject(fullyQualifiedName);

                promise.then((promiseResult) => {
                    var response = promiseResult.data;

                    appService.identityMap.merge(response.Modifications);
                    $rootScope.$broadcast(UiEventType.MessagesReceived, response.Messages);

                    if (response.Passed) {
                        var newObject = response.NewObject;
                        appService.identityMap.addObject(newObject);
                        $rootScope.$broadcast(UiEventType.ExplorerOpen, newObject);
                    }
                });
            }

            $scope.searchObjects = function (fullyQualifiedName: string) {
                var request = requestBuilder.buildSearchObjectsRequest(fullyQualifiedName);
                var promise = fresnelService.searchObjects(request);

                promise.then((promiseResult) => {
                    var response = promiseResult.data;

                    appService.identityMap.merge(response.Modifications);
                    $rootScope.$broadcast(UiEventType.MessagesReceived, response.Messages);

                    if (response.Passed) {
                        response.Result.IsSearchResults = true;
                        response.Result.OriginalRequest = request;

                        appService.identityMap.addObject(response.Result);
                        $rootScope.$broadcast(UiEventType.ExplorerOpen, response.Result);
                    }
                });
            }

            // This will run when the page loads:
            angular.element(document).ready(function () {
                $scope.loadClassHierarchy();
            });

        }

    }
}