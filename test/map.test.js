define(function (require) {
    'use strict';

    describe('map', function() {
        it('should be defined', function() {
            var map = require('map');
            expect(map.isDefined).toBe(true);
        });
    });
});