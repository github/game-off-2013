/// <reference path="../../lib/melonJS-0.9.9.js" />

/*----------------
Block entity
------------------------ */
game.BlockEntity = me.ObjectEntity.extend({

    // extending the init function is not mandatory
    // unless you need to add some extra initialization
    init: function (x, y, settings) {
        // call the parent constructor
        this.parent(x, y, settings);

        this.updateColRect(28, 8, 0, 75);

        this.isJack = settings.isjack;
        this.enemyType = settings.enemy;

        this.enemySheet = settings.enemysheet;
        this.enemyWidth = settings.enemywidth;
        this.enemyHeight = settings.enemyheight;
        this.enemyWalkFrames = settings.enemywalkframes;
        this.enemyDieFrames = settings.enemydieframes;
        this.enemyHP = settings.enemyhp;

        this.collidable = true;
    },


    update: function () {
        this.parent();


        return true;
    },

    // this function is called by the engine, when
    // an object is touched by something (here collected)
    onCollision: function (res, obj) {
      
        if (res.y < 0) {
            //var sweet = me.entityPool.newInstanceOf("SweetEntity", this.pos.x, this.pos.y-64, { image: "sweets", spritewidth: 48, width: 64, height: 64, name: "SweetEntity", z: 6 });
            var obj;
            
            if (this.isJack) {
                for (var i = 0; i < 3; i++) {
                    obj = new game.JackEntity(this.pos.x, this.pos.y, { image: "jack", spritewidth: 30, spriteheight: 32, z: 0 });
                    var tween = new me.Tween(obj.pos).to({ x: obj.pos.x-20 + (i*20), y: this.pos.y - (32 + (obj.renderable.height / 2)) + (i==1?-20:0) }, 500).onComplete((function () { obj.z = 4; }).bind(obj));
                    tween.easing(me.Tween.Easing.Quadratic.Out);
                    tween.start();
                    obj.z = 4;
                    me.game.add(obj);
                }
            } else if (this.enemySheet) {
                obj = new game.WalkingEnemy(this.pos.x, this.pos.y, { image: this.enemySheet, spritewidth: this.enemyWidth, spriteheight: this.enemyHeight, z: 0, walkframes: this.enemyWalkFrames, dieframes: this.enemyDieFrames, hp: this.enemyHP });
                var tween = new me.Tween(obj.pos).to({ y: this.pos.y - (50 + (obj.renderable.height / 2)) }, 500).onComplete((function () { obj.z = 4; obj.collidable = true; obj.spawning = false; obj.walkLeft = false; }).bind(obj));
                tween.easing(me.Tween.Easing.Quadratic.Out);
                tween.start();
                obj.z = 4;
                obj.collidable = false;
                obj.spawning = true;
                me.game.add(obj);
            } else {
                obj = new game.SweetEntity(this.pos.x, this.pos.y, { image: "sweets", spritewidth: 48, spriteheight: 48, z: 0 });
                var tween = new me.Tween(obj.pos).to({ y: this.pos.y - (32 + (obj.renderable.height / 2)) }, 500).onComplete((function () { obj.z = 4; }).bind(obj));
                tween.easing(me.Tween.Easing.Quadratic.Out);
                tween.start();
                obj.z = 4;
                me.game.add(obj);
            }
            
            
            //me.game.sort();

            

            me.game.remove(this);
        }
    },


});
