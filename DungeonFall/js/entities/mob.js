/// <reference path="../../lib/melonJS-0.9.10.js" />

game.MobList = ["a Spider"];
game.Mob = me.ObjectEntity.extend({

    Type: 0,

    DUNGEON_WIDTH: 35,
    DUNGEON_HEIGHT: 19,
    target: new me.Vector2d(0, 0),
    walkTween: null,
    isTravelling: false,
    isFollowingPath: false,
    currentPath: null,
    currentPathStep: 0,

    isInCombat: false,
    attackCooldown: 0,
    attackCooldownTarget: 2000,

    Level: 1,
    DRMin: 0,
    DRMax: 1,
    SRMin: 0,
    SRMax: 1,
    HP: 1,

    init: function (x, y, settings) {
        // call the constructor
        settings.spritewidth = 32;
        settings.spriteheight = 32;
        

        this.Type = settings.type;
        settings.image = "mob" + this.Type;

        //x = -32;
        //y = 288 + 16;
        this.target = new me.Vector2d(x, y);

        //this.offset = new me.Vector2d(16, 16);

        this.combatOffsetPos = new me.Vector2d(0, 0);

        this.parent(x, y, settings);

        this.renderable.anim = {};
        this.renderable.addAnimation("walk", [0]);
        this.renderable.setCurrentAnimation("walk");

        this.setVelocity(5, 5);
        this.setFriction(0, 0);
        this.setMaxVelocity(5, 5);
        
        this.gravity = 0;
      
        this.collidable = false;

        this.name = "mob";
        
        this.mobName = game.MobList[this.Type];

        this.alwaysUpdate = true;

        this.z = 3;

        // Distribute stats
        var points = this.Level * 3;
        for (var i = 0; i < points; i++) {
            var r = Math.floor(Math.random() * 3);
            switch (r) {
                case 0:
                    this.HP++;
                    break;
                case 1:
                    this.DRMax++;
                    break;
                case 2:
                    this.SRMax++;
                    break;
            }
        }
        this.DRMin = this.Level - 1;
        this.SRMin = this.Level - 1;


        //this.walkTween = new me.Tween(this.pos).to(this.target, 100).onComplete(this.targetReached.bind(this));
        //this.walkTween.easing(me.Tween.Easing.Linear.None);
        //this.walkTween.start();
        //this.isTravelling = true;
    },

    scanX: 0,
    scanY: 0,
    update: function () {
        var dungeon = me.game.world.getEntityByProp("name", "dungeon")[0];
        var hero = me.game.world.getEntityByProp("name", "hero")[0];

        var tx = Math.floor(this.pos.x / 32);
        var ty = Math.floor(this.pos.y / 32);

        var hx = Math.floor(hero.pos.x / 32);
        var hy = Math.floor(hero.pos.y / 32);

        if (((hx == tx - 1 || hx == tx + 1) && hy == ty) ||
            ((hy == ty - 1 || hy == ty + 1) && hx == tx)) {
            this.isInCombat = true;
            this.attack();
        } else this.isInCombat = false;

        if (!this.isInCombat && !this.isTravelling) {
            //this.currentPath = path;
            //this.currentPathStep = 0;
            if (Math.floor(Math.random() * 10) == 0) {
                var dir = Math.floor(Math.random() * 4);
                switch (dir) {
                    case 0:
                        if ((dungeon.pathGridMobs[tx][ty-1] == 1)
                            && !(hx==tx && hy==ty-1)) this.target = new me.Vector2d(this.pos.x, this.pos.y - 32);
                        break;
                    case 1:
                        if ((dungeon.pathGridMobs[tx + 1][ty] == 1)
                            && !(hx == tx + 1 && hy == ty)) this.target = new me.Vector2d(this.pos.x + 32, this.pos.y);
                        break;
                    case 2:
                        if ((dungeon.pathGridMobs[tx][ty+1] == 1)
                            && !(hx == tx && hy == ty + 1)) this.target = new me.Vector2d(this.pos.x, this.pos.y + 32);
                        break;
                    case 3:
                        if ((dungeon.pathGridMobs[tx - 1][ty] == 1)
                            && !(hx == tx - 1 && hy == ty)) this.target = new me.Vector2d(this.pos.x - 32, this.pos.y);
                        break;
                }

                if (this.target != this.pos) {
                    this.walkTween = new me.Tween(this.pos).to(this.target, 200).onComplete(this.targetReached.bind(this));
                    this.walkTween.easing(me.Tween.Easing.Linear.None);
                    this.walkTween.start();
                    this.isTravelling = true;
                } else this.isTravelling = false;
                //this.target = new me.Vector2d((this.currentPath[this.currentPathStep].x * 32), (this.currentPath[this.currentPathStep].y * 32));
                
                //this.isFollowingPath = true;
            }
        }

        this.updateMovement();

        this.parent();
       
        if (dungeon.Tiles[tx][ty] >= PieceHelper.MIN_WALL_TILE && dungeon.Tiles[tx][ty] <= PieceHelper.MAX_WALL_TILE) me.game.remove(this);

        if (this.HP <= 0) {
            this.die();
            hero.XP += this.Level * 10;
            game.HUD.addFloatyText(new me.Vector2d(hero.pos.x + 3 + Math.floor(Math.random() * 16), hero.pos.y), (this.Level * 10) + "XP", "blue");
        }

        return true;
    },

    attack: function() {
        var hero = me.game.world.getEntityByProp("name", "hero")[0];

        if (me.timer.getTime() < this.attackCooldown + this.attackCooldownTarget) return;

        this.attackCooldown = me.timer.getTime();

        hero.attackedBy(this);
    },

    attackedBy: function (attacker) {

        if (!this.isInCombat) {
            this.attackCooldown = me.timer.getTime() + (Math.random() * 2000);
            game.HUD.addFloatyText(new me.Vector2d((this.pos.x - 40) + Math.floor(Math.random() * 16), this.pos.y - 16), "Stunned!", "red");
        }
        this.isInCombat = true;

        var dam = attacker.DRMin + Math.floor(Math.random() * ((attacker.DRMax+1) - attacker.DRMin));
        var sav = this.SRMin + Math.floor(Math.random() * ((this.SRMax + 1) - this.SRMin));

        var totaldam = dam - sav;

        var report = "Hero attacks " + this.mobName;

        if (dam == 0) {
            report += " but misses!";
            game.HUD.addFloatyText(new me.Vector2d(attacker.pos.x + 3 + Math.floor(Math.random() * 16), attacker.pos.y), "Miss!", "white");
        }
        else {
            if (totaldam > 0) {
                report += " and hits for " + totaldam;
                game.HUD.addFloatyText(new me.Vector2d(this.pos.x + 3 + Math.floor(Math.random() * 16), this.pos.y), totaldam, "red");
                this.HP -= totaldam;
            }
            else {
                report = this.mobName + " defends the Hero's attack!";
                game.HUD.addFloatyText(new me.Vector2d(this.pos.x + 3 + Math.floor(Math.random() * 16), this.pos.y), "Defend!", "green");

            }
        }

        game.HUD.addLine(report);

        
    },

    targetReached: function() {
        this.isTravelling = false;
        if (this.isFollowingPath) {
            if (this.currentPathStep < this.currentPath.length - 1) {
                this.currentPathStep++;
                this.target = new me.Vector2d((this.currentPath[this.currentPathStep].x * 32), (this.currentPath[this.currentPathStep].y * 32));
                this.walkTween = new me.Tween(this.pos).to(this.target, 100).onComplete(this.targetReached.bind(this));
                this.walkTween.easing(me.Tween.Easing.Linear.None);
                this.walkTween.start();
                this.isTravelling = true;
            }
            else {
                this.isFollowingPath = false;
            }
        }
    },

    die: function () {
        game.HUD.addLine(this.mobName + " dies!");
        me.game.remove(this);
    },

    reset: function () {

        this.init(this.spawnPosition.x, this.spawnPosition.y, { image: "girl" });
    }

});