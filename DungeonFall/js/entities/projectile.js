/// <reference path="../../lib/melonJS-0.9.9.js" />

/*----------------
Projectile entity
------------------------ */
game.ProjectileEntity = me.ObjectEntity.extend({

    // extending the init function is not mandatory
    // unless you need to add some extra initialization
    init: function (x, y, v, settings) {
        // call the parent constructor
        this.parent(x, y, settings);


        //this.setMaxVelocity(vel.x, 5);
        this.setVelocity(Math.abs(v.x), 2);
        this.vel.x = v.x;
        this.gravity = 0.06;

        this.updateColRect(5, 20, 6, 20);

        this.collidable = true;;
        this.z = 6;
        this.alwaysUpdate = true;

    },


    update: function () {
        if (!this.inViewport) me.game.remove(this);

        if (this.vel.x < 0) this.flipX(true); else this.flipX(false);
        this.renderable.angle += 0.1;

        var res = this.updateMovement();

        if (res.y > 0) this.vel.y = -this.vel.y;
        if (res.x < 0 || res.x > 0) me.game.remove(this);

        res = me.game.collide(this);

        this.parent();
        return true;
    },

    // this function is called by the engine, when
    // an object is touched by something (here collected)
    onCollision: function (res, obj) {
      
        //if (res.y < 0) {
        //    //var sweet = me.entityPool.newInstanceOf("SweetEntity", this.pos.x, this.pos.y-64, { image: "sweets", spritewidth: 48, width: 64, height: 64, name: "SweetEntity", z: 6 });
        //    var obj;
            
        //    if (this.isJack) {
        //        obj = new game.JackEntity(this.pos.x, this.pos.y, { image: "jack", spritewidth: 30, spriteheight: 32, z: 0 });
        //    }
        //    else {
        //        obj = new game.SweetEntity(this.pos.x, this.pos.y, { image: "sweets", spritewidth: 48, spriteheight: 48, z: 0 });
        //    }
        //    obj.z = 3;
        //    me.game.add(obj);
            
        //    //me.game.sort();

        //    var tween = new me.Tween(obj.pos).to({ y: this.pos.y - (32 + (obj.renderable.height/2)) }, 500).onComplete((function () { obj.z = 3; }).bind(obj));
        //    tween.easing(me.Tween.Easing.Quadratic.Out);
        //    tween.start();

        //    me.game.remove(this);
        //}
    },


});
