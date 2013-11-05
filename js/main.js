requirejs.config({
  paths: {
    'phaser': 'lib/phaser'
  },
  shim: {
    'phaser': {
      exports: 'Phaser'
    }
  }
});

require(['context'], function(Context) {
  (new Context).init();
});