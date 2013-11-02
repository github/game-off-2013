requirejs.config({
  paths: {
    'phaser': '../../../js/lib/phaser'
  },
  shim: {
    'phaser': {
      exports: 'Phaser'
    }
  }
});

require(['game'], function(Game) {
  (new Game).run();
});