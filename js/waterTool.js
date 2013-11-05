define([], function() {
  function WaterTool(context) {
    this.use = function() {
      publish('/tools/raiseWater');
    };
  }

  return WaterTool;
});