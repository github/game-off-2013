define([], function() {
  function WaterTool(context) {
    this.use = function() {
    me.event.publish('/tools/raiseWater');
    };
  }

  return WaterTool;
});