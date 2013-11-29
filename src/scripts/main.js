require.config({
    'paths': {
        'jquery': '//ajax.googleapis.com/ajax/libs/jquery/2.0.0/jquery.min',
        'd3': '../lib/d3.v3.min',
        'geodesic': '../lib/geodesic',
        'underscore': '//cdnjs.cloudflare.com/ajax/libs/underscore.js/1.5.2/underscore-min'
    },

    shim: {
        'underscore': {
            exports: '_'
        },
        'd3': {
            exports: 'd3'
        },
        'geodesic': ['d3']
    }
});

require(['jquery', 'game', 'gameStateUpdater', 'grid', 'globe', 'terrain', 'facilityList'],
        function($, Game, GameStateUpdater, grid, globe, terrainFactory, FacilityList) {
            'use strict';

            var initialGameState = {
                tick: 0,
                seaLevel: 0,
                buildableLandArea: map.calculateRemainingLandArea(),
                pollution: 0,
                food: 200,
                population: 7000
            };

            var mapElement = document.getElementById('map');

            var n = 13;

            var cells = grid.generate(n);
            var terrain = terrainFactory.generate(cells, 0.5);

            var map = globe.create(mapElement, cells);
            var facilityList = new FacilityList();
            var gameStateUpdater = new GameStateUpdater(terrain, facilityList);
            var game = new Game(initialGameState, gameStateUpdater);

            refreshDisplay();

            $('#nextTurnButton').click(function() {
                game.update();
                refreshDisplay();
                if (game.state.population === 0) {
                    $('#nextTurnButton').prop('disabled', 'disabled');
                }
            });

            function refreshDisplay() {
                document.getElementById('tick').value = game.state.tick;
                document.getElementById('seaLevel').value = game.state.seaLevel;
<<<<<<< HEAD
                document.getElementById('remainingLand').value = terrain.calculateRemainingLandArea();
=======
                document.getElementById('buildableLand').value = game.state.buildableLandArea;
>>>>>>> mock facilityList and get unit tests passing
                document.getElementById('population').value = game.state.population;
                document.getElementById('food').value = game.state.food;
                document.getElementById('pollution').value = game.state.pollution;
<<<<<<< HEAD
                document.getElementById('deathsFromStarvation').value = game.state.deathsFromStarvation;
                map.redraw();
=======
>>>>>>> update food, starving people if necessary
            }
        });