require.config({
    'paths': {
        'jquery': '//ajax.googleapis.com/ajax/libs/jquery/2.0.0/jquery.min',
        'd3': '../lib/d3.v3.min',
        'geodesic': '../lib/geodesic',
        'underscore': '//cdnjs.cloudflare.com/ajax/libs/underscore.js/1.5.2/underscore-min'
    },

    shim: {
        'd3': {
            exports: 'd3'
        },
        'geodesic': ['d3']
    }
});

require(['jquery', 'game', 'gameStateUpdater', 'grid', 'globe', 'terrain', 'facilityList'],
        function($, Game, GameStateUpdater, grid, globe, terrainFactory, facilityList) {
            'use strict';

            var initialGameState = {
                year: 0,
                seaLevel: 0,
                pollution: 0,
                agricultureLevel: 5,
                population: 7000,
                food: 0,
                deathsFromStarvation: 0
            };

            var mapElement = document.getElementById('map');

            var n = 13;

            var cells = grid.generate(n);
            var terrain = terrainFactory.generate(cells, 0.5);

            var map = globe.create(mapElement, cells);
            var gameStateUpdater = new GameStateUpdater(terrain, facilityList);
            var game = new Game(initialGameState, gameStateUpdater);

            refreshDisplay();

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
                document.getElementById('remainingLand').value = terrain.calculateRemainingLandArea();
                document.getElementById('population').value = game.state.population;
                document.getElementById('food').value = game.state.food;
                document.getElementById('pollution').value = game.state.pollution;
                document.getElementById('agricultureLevel').value = game.state.agricultureLevel;
                document.getElementById('deathsFromStarvation').value = game.state.deathsFromStarvation;
                map.redraw();
            }
        });