(function( window, document, undefined ) {
  'use strict';

  function Level() {}

  Level.prototype.fromJSON = function() {};

  function Entity () {}

  Entity.prototype.update = function() {};

  Entity.prototype.draw = function( ctx ) {
    ctx.fillStyle = 'black';
    ctx.fillRect( 0, 0, 100, 100 );
  };

  function Player() {}

  Player.prototype.update = function() {};

  function Game() {
    this.prevTime = Date.now();
    this.currTime = this.prevTime;

    this.running = true;

    this.canvas = document.createElement( 'canvas' );
    this.ctx    = this.canvas.getContext( '2d' );

    this.WIDTH  = 640;
    this.HEIGHT = 480;

    this.canvas.width  = this.WIDTH;
    this.canvas.height = this.HEIGHT;

    this.entities = [];
    this.level = null;
  }

  Game.instance = null;

  Game.prototype.update = function() {
    this.entities.forEach(function( entity ) {
      entity.update();
    });
  };

  Game.prototype.draw =function() {
    var ctx = this.ctx;

    ctx.clearRect( 0, 0, ctx.canvas.width, ctx.canvas.height );
    this.entities.forEach(function( entity ) {
      entity.draw( ctx );
    });
  };

  Game.prototype.tick = function() {
    this.update();
    this.draw();
  };

  (function() {
    var game = Game.instance = new Game();
    game.entities.push( new Entity() );
    game.tick();

    document.body.appendChild( game.canvas );
  }) ();
}) ( window, document );
