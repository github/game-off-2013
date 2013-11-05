requirejs.config({
  paths: {
    'phaser': 'lib/phaser',
    'minpubsub': 'lib/minpubsub'
  },
  shim: {
    'phaser': {
      exports: 'Phaser'
    }
  }
});

require(['context', 'minpubsub'], function(Context, MinPubSub) {
  (new Context).init();
});