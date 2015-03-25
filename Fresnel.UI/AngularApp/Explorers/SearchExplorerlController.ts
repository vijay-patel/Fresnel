﻿module FresnelApp {

    // Used to control the interactions within an open Search form
    export class SearchExplorerController {

        static $inject = [
            '$rootScope',
            '$scope',
            'fresnelService',
            'searchService',
            'smartTablePredicateService',
            'appService',
            'blockUI'];

        private scope: ISearchScope;

        constructor(
            $rootScope: ng.IRootScopeService,
            $scope: ISearchScope,
            fresnelService: IFresnelService,
            searchService: SearchService,
            smartTablePredicateService: SmartTablePredicateService,
            appService: AppService,
            blockUI: any) {

            this.scope = $scope;

            $scope.results = <SearchResultsVM>$scope.explorer.__meta;
            $scope.request = $scope.results.OriginalRequest;
            $scope.results.AllowMultiSelect = false;
            $scope.searchAction = function () {
                return fresnelService.searchObjects(<SearchObjectsRequest>$scope.request);
            };

            // This allows Smart-Table to handle the st-safe-src properly:
            $scope.results.DisplayItems = [].concat($scope.results.Items);

            $scope.openNewExplorer = function (obj: ObjectVM) {
                searchService.openNewExplorer(obj, $rootScope, $scope.explorer);
            }

            $scope.stTablePipe = function (tableState) {
                // Sorting on an Object/Collection property doesn't make sense, so don't allow it:
                var sortProperty = smartTablePredicateService.getSortProperty(tableState, $scope.results.ElementProperties);
                if (sortProperty == null || !sortProperty.IsNonReference)
                    return;

                // Changing the order means we start from the beginning again:
                $scope.request.PageNumber = 1;
                $scope.request.OrderBy = sortProperty.InternalName;
                $scope.request.IsDescendingOrder = tableState.sort.reverse;

                blockUI.start("Sorting by " + sortProperty.Name + "...");

                $scope.searchAction().then((promiseResult) => {
                    var response = promiseResult.data;
                    var newSearchResults: SearchResultsVM = response.Result;

                    // Ensure that we re-use any objects that are already cached:
                    var bindableItems = searchService.mergeSearchResults(newSearchResults);

                    // This allows Smart-Table to handle the st-safe-src properly:
                    $scope.results.Items = bindableItems;
                    $scope.results.DisplayItems = [].concat(bindableItems);

                    tableState.pagination.numberOfPages = 50;
                })
                    .finally(() => {
                    blockUI.stop();
                });
            };

            $scope.loadNextPage = function () {
                searchService.loadNextPage($scope.request, $scope.results, $scope.searchAction);
            }

        }


    }
}
