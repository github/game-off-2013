define([], function() {
  var Water = me.ObjectEntity.extend({
    init: function(x, y, settings) {
      this.parent(x, y, settings);
      this.z = 1000;
    },

    update: function() {
    }
  });

  return Water;
});
