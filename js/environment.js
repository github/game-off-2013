define([], function() {
  function Environment() {
    var _this = this;

    this.waterLevel = 100;
    this.yearsLeft = 10000;

    subscribe('/tools/raiseWater', function() {
      _this.waterLevel += 1; // TODO: This could be received as a parameter
    });
  }

  return Environment;
});
