define([], function() {
  /**
   * Simple buoyant object.
   * Don't expect it to follow the laws of physics. How much it floats
   * can be controlled by the floatingFactor setting.
   */
  var Buoyant = me.ObjectEntity.extend({
    init: function(x, y, settings) {
      this.parent(x, y, settings);
      this.alwaysUpdate = true;
      this.floatingFactor = settings.floatingFactor || 1;
      this.z = 1001;
    },
    update: function() {
      var water = me.state.current().water;

      if (water.isOver(this)) {
        var submerged = water.submerged(this);
        this.vel.y = -this.gravity * submerged * this.floatingFactor;
      }

      this.updateMovement();
      this.parent();

      return true;
    }
  });

  return Buoyant;
});