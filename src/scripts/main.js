require.config({
    'paths': {
        'jquery': '//ajax.googleapis.com/ajax/libs/jquery/2.0.0/jquery.min',
        'd3': '../lib/d3.v3.min',
        'geodesic': '../lib/geodesic'
    },

    shim: {
        'd3': {
            exports: 'd3'
        },
        'geodesic': ['d3']
    }
});

require(['jquery', 'game', 'gameStateUpdater', 'map', 'plateCareeProjection', 'globe'],
        function($, Game, GameStateUpdater, Map, plateCareeProjection) {
            'use strict';

            var EARTH_SURFACE_AREA = 510100000;

            var initialGameState = {
                year: 2013,
                seaLevel: 0,
                pollution: 0,
                agricultureLevel: 50,
                population: 7000000000,
                food: 0,
                deathsFromStarvation: 0
            };

            var mapElement = document.getElementById('map');
            var map = new Map('map.png', EARTH_SURFACE_AREA, plateCareeProjection, mapElement, onRender);
            var gameStateUpdater = new GameStateUpdater(map);
            var game = new Game(initialGameState, gameStateUpdater);

            function onRender() {
                refreshDisplay();
            }

            $('#nextTurnButton').click(function() {
                var agricultureIncrease = parseInt($('input[name=agricultureIncrease]:checked').val(), 10);

                game.update({agricultureIncrease: agricultureIncrease});
                refreshDisplay();
                if (game.state.population === 0) {
                    $('#nextTurnButton').prop('disabled', 'disabled');
                }
            });

            function refreshDisplay() {
                document.getElementById('year').value = game.state.year;
                document.getElementById('seaLevel').value = game.state.seaLevel;
                document.getElementById('remainingLand').value = map.calculateRemainingLandArea();
                document.getElementById('population').value = game.state.population;
                document.getElementById('food').value = game.state.food;
                document.getElementById('pollution').value = game.state.pollution;
                document.getElementById('agricultureLevel').value = game.state.agricultureLevel;
                document.getElementById('deathsFromStarvation').value = game.state.deathsFromStarvation;
            }
        });