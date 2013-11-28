define('availableFacilities', ['underscore'], function(_) {
    'use strict';

    var facilities = [
        {
            name: 'Farm',
            landCost: 2,
            buildDuration: 1,
            buildDelta: {
                energy: -16,
                pollution: 32,
                food: 0
            },
            normalDelta: {
                energy: -2,
                pollution: 16,
                food: 32
            }
        },
        {
            name: 'Coal Power Plant',
            landCost: 4,
            buildDuration: 3,
            buildDelta: {
                energy: -16,
                pollution: 80,
                food: 0
            },
            normalDelta: {
                energy: 128,
                pollution: 64,
                food: 0
            }
        }
    ];

    return _.object(_.map(facilities, function(item) {
        return [item.name, item]
    }));
});
