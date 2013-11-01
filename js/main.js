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
