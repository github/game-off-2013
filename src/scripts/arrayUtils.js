define('arrayUtils', function() {
    'use strict';

    var getRandomElement = function getRandomElement(arr) {
        return arr[Math.floor(Math.random() * arr.length)];
    };

    var addIfNotPresent = function addIfNotPresent(arr, elem) {
        if (arr.indexOf(elem) === -1) {
            arr.push(elem);
            return true;
        }
        return false;
    };

    return {
        getRandomElement: getRandomElement,
        addIfNotPresent: addIfNotPresent
    };
});
