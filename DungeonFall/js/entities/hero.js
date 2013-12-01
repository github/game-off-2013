/// <reference path="../../lib/melonJS-0.9.10.js" />


/*------------------- 
a player entity
-------------------------------- */
game.Hero = me.ObjectEntity.extend({

    DUNGEON_WIDTH: 35,
    DUNGEON_HEIGHT: 19,
    DiscoveredTiles: new Array(35),
    target: new me.Vector2d(0, 0),
    walkTween: null,
    walkSpeed: 3,
    isTravelling: false,
    isResting: false,
    isFollowingPath: false,
    currentPath: null,
    currentPathStep: 0,
    stairsFound: null,
    isEntering: true,

    isInCombat: false,
    attackCooldown: 0,
    attackCooldownTarget: 2000,

    statsTick: 0,
    statsTickTarget: 1000,

    Level: 1,
    XP: 0,
    XPTNL: 50,
    LastXPTNL: 0,
    DRMin: 0,
    DRMax: 3,
    DRBase:3,
    SRMin: 0,
    SRMax: 2,
    SRBase: 2,
    HPMax: 10,
    HP: 10,

    isGameOver: false,
    gameOverTime: 0,

    Items: new Array(8),

    ItemNames: [ "a better helm", "a better chestplate", "better leggings", "better bracers", "better gloves", "better boots", "a better sword", "a health potion" ],

    init: function (x, y, settings) {
        // call the constructor
        settings.spritewidth = 32;
        settings.spriteheight = 32;

        this.name = "hero";

        x = -32;
        //y = 288 + 16;
        this.target = new me.Vector2d(x + (32*3), y);

        this.offset = new me.Vector2d(16, 16);

        this.parent(x, y, settings);

        this.renderable.anim = {};
        this.renderable.addAnimation("walk", [0]);
        this.renderable.setCurrentAnimation("walk");

        this.setVelocity(5, 5);
        this.setFriction(0, 0);
        this.setMaxVelocity(5, 5);
      
        this.isPersistent = true;

        this.gravity = 0;
      
        this.collidable = false;

        this.alwaysUpdate = true;

        this.isTravelling = true;
        this.isResting = false;
        this.isFollowingPath = false;
        this.isInCombat = false;
        this.HP = this.HPMax;
        this.isEntering = true;
        this.stairsFound = null;
        

        for (var x = 0; x < this.DUNGEON_WIDTH; x++) {
            this.DiscoveredTiles[x] = new Array(this.DUNGEON_HEIGHT);
            for (var y = 0; y < this.DUNGEON_HEIGHT; y++) {
                this.DiscoveredTiles[x][y] = -1;
            }
        }

        if (game.Level == 1) { // First level intialization
            for (var i = 0; i < 8; i++) this.Items[i] = 0;
            this.XPTNL = 50;
        }

        this.z = 4;

        this.isGameOver = false;

        game.HUD.addLine("Hero has arrived on dungeon floor "+ game.Level);
    },

    scanX: 0,
    scanY: 0,
    //lastKnownGoodX: 0,
    //lastKnownGoodY: 0,
    update: function () {
        var dungeon = me.game.world.getEntityByProp("name", "dungeon")[0];
        var mobs = me.game.world.getEntityByProp("name", "mob");

        if (!dungeon) return false;

        var tx = Math.floor(this.pos.x / 32);
        var ty = Math.floor(this.pos.y / 32);

      

        //if (tx > 0) this.lastKnownGoodX = tx;
        //if (ty > 0) this.lastKnownGoodY = ty;

        // Attempt to keep the hero in check on Chrome!
        //if (!this.isEntering) {
        //    if (Math.abs(this.target.x - this.pos.x) > 32 || Math.abs(this.target.y - this.pos.y) > 32) {
        //        this.walkTween.stop();
        //        this.isTravelling = false;
        //        this.pos.x = this.lastKnownGoodX;
        //        this.pos.y = this.lastKnownGoodY;
        //        this.target.x = this.pos.x;
        //        this.target.y = this.pos.y;
        //    }
        //}

        // re-calc stats
        this.DRMax = this.DRBase;
        this.SRMax = this.SRBase;
        for (var i = 0; i < 6; i++) this.SRMax += this.Items[i];
        this.DRMax += this.Items[6];
        this.attackCooldownTarget = 2000 - (this.Items[6] * 50);
        /////

        if (this.isGameOver) {
            if (me.timer.getTime() > this.gameOverTime + 3000) {
                me.game.viewport.fadeIn("#000000", 1000, this.gameOver.bind(this));
            }
            return true;
        }

        if (this.isTravelling) {
            if (this.target.x > this.pos.x) this.pos.x+=this.walkSpeed;
            if (this.target.x < this.pos.x) this.pos.x -= this.walkSpeed;
            if (this.target.y > this.pos.y) this.pos.y += this.walkSpeed;
            if (this.target.y < this.pos.y) this.pos.y -= this.walkSpeed;
            if (this.pos.x < this.target.x + this.walkSpeed && this.pos.x > this.target.x - this.walkSpeed && this.pos.y < this.target.y + this.walkSpeed && this.pos.y > this.target.y - this.walkSpeed) this.targetReached();
        }

        this.isInCombat = false;
        for (var i = 0; i < mobs.length; i++) {
            var mob = mobs[i];
            var mx = Math.floor(mob.pos.x / 32);
            var my = Math.floor(mob.pos.y / 32);
            if (((mx == tx - 1 || mx == tx + 1) && my == ty) ||
                ((my == ty - 1 || my == ty + 1) && mx == tx)) {
                this.isInCombat = true;
                this.statsTick = me.timer.getTime() + 2000;
                this.attack(mob);
            }
        }

        if (this.isInCombat && this.HP < (this.HPMax * 0.8) && this.Items[7] > 0) {
            this.Items[7]--;
            this.HP = this.HPMax;
            game.HUD.addLine("Hero drinks a health potion!");
        }

        if (this.HP <= 0) {
            this.HP = 0;
            this.isGameOver = true;
            this.gameOverTime = me.timer.getTime();
            game.HUD.addFloatyText(new me.Vector2d((this.pos.x - 50) + Math.floor(Math.random() * 16), this.pos.y - 16), "Dedz :(", "red", 3);
            game.HUD.addLine("");
            game.HUD.addLine("Hero dies!");
            game.HUD.addLine("--- Game Over ---");
        }

        if (!this.isInCombat) {
            //Tick health if not fighting
            if (me.timer.getTime() > this.statsTick + this.statsTickTarget) {
                this.statsTick = me.timer.getTime();
                if (this.HP < this.HPMax) {
                    this.HP+= (this.HPMax/20);
                    game.HUD.addFloatyText(new me.Vector2d((this.pos.x + 3) + Math.floor(Math.random() * 16), this.pos.y), (this.HPMax / 20), "green");
                    

                }
            }

            if (this.HP <= (this.HPMax / 2)) {
                if (!this.isResting) {
                    this.isResting = true;
                    game.HUD.addLine("Hero is resting...");
                }
            }
            else if (this.isResting) {
                game.HUD.addLine("Hero stops resting");
                this.isResting = false;
            }
        }

        if (this.HP > this.HPMax) this.HP = this.HPMax;

        for (var x = 0; x < this.DUNGEON_WIDTH; x++) {
            for (var y = 0; y < this.DUNGEON_HEIGHT; y++) {
                if (this.DiscoveredTiles[x][y] == -1) {
                    if (dungeon.Tiles[x][y] == -1 || (dungeon.Tiles[x][y] >= PieceHelper.MIN_WALL_TILE && dungeon.Tiles[x][y] <= PieceHelper.MAX_WALL_TILE))
                        this.DiscoveredTiles[x][y] = -1;
                    else {
                        this.DiscoveredTiles[x][y] = 0;
                        
                    }
                }
            }
        }

        for (var x = tx-1; x <= tx+1; x++) {
            for (var y = ty - 1; y <= ty + 1; y++) {
                if (x >= 1 && y >= 1 && x <= this.DUNGEON_WIDTH - 1 && y <= this.DUNGEON_HEIGHT - 2) {
                    if (this.DiscoveredTiles[x][y] != 1) this.XP += (game.Level / 5);
                    this.DiscoveredTiles[x][y] = 1;
                    if (dungeon.Tiles[x][y] == PieceHelper.STAIRS_TILE) {
                        if (this.stairsFound == null) {
                            game.HUD.addLine("Hero found stairs to floor " + (game.Level + 1));
                            game.HUD.addLine("...and will remember their location");
                        }
                        this.stairsFound = new me.Vector2d(x, y);
                        dungeon.stairsOK = true;
                    }
                }
            }
        }

        for (var i = 0; i < me.game.world.getEntityByProp("name", "chest").length; i++) {
            var chest = me.game.world.getEntityByProp("name", "chest")[i];
            var cx = Math.floor(chest.pos.x / 32);
            var cy = Math.floor(chest.pos.y / 32);
            if (tx == cx && ty == cy && !chest.isOpen) {
                chest.open();
                if (chest.Type == 0) {
                    game.HUD.addLine("Hero opened a chest");
                    var ran = Math.floor(Math.random() * 6);
                    this.Items[ran]++;
                    game.HUD.addLine("...and found " + this.ItemNames[ran] + "!");
                }
                if (chest.Type == 1) {
                    game.HUD.addLine("Hero found a weapon rack");
                    this.Items[6]++;
                    game.HUD.addLine("...and took " + this.ItemNames[6] + "!");
                }
                if (chest.Type == 2) {
                    this.Items[7]++;
                    game.HUD.addLine("Hero found " + this.ItemNames[7] + "!");
                }
            }
        }

        for (this.scanY = 1; this.scanY < this.DUNGEON_HEIGHT - 2; this.scanY++) {
            if (dungeon.Tiles[this.scanX][this.scanY] == PieceHelper.STAIRS_TILE && this.stairsFound==null) {
                var path = dungeon.findPath(dungeon.pathGrid, tx, ty, this.scanX, this.scanY);
                if (path.length > 0) {
                    dungeon.stairsOK = true;
                    if(!this.isTravelling && !this.isFollowingPath && !this.isResting) this.explore(this.scanX, this.scanY, path);
                } else {
                    dungeon.stairsOK = false;
                }
            }
        }

        if (!this.isInCombat && !this.isResting) {

            if (!this.isTravelling && !this.isFollowingPath) {
                for (var i = 0; i < me.game.world.getEntityByProp("name", "chest").length; i++) {
                    var chest = me.game.world.getEntityByProp("name", "chest")[i];
                    if (!chest.isOpen) {
                        var cx = Math.floor(chest.pos.x / 32);
                        var cy = Math.floor(chest.pos.y / 32);
                        var path = dungeon.findPath(dungeon.pathGridMobs, tx, ty, cx, cy);
                        if (path.length > 0) {
                            this.explore(cx, cy, path);
                        }
                    }
                }
            }

            if (!this.isTravelling && !this.isFollowingPath && this.HP >= this.HPMax * 0.8) {
                for (var i = 0; i < me.game.world.getEntityByProp("name", "mob").length; i++) {
                    var mob = me.game.world.getEntityByProp("name", "mob")[i];
                    if (mob.Level<=this.Level) {
                        var cx = Math.floor(mob.pos.x / 32);
                        var cy = Math.floor(mob.pos.y / 32);
                        var path = dungeon.findPath(dungeon.pathGrid, tx, ty, cx, cy);
                        if (path.length > 0) {
                            this.explore(cx, cy, path);
                        }
                    }
                }
                for (var i = 0; i < me.game.world.getEntityByProp("name", "mob").length; i++) {
                    var mob = me.game.world.getEntityByProp("name", "mob")[i];
                    if (mob.Level > this.Level && this.HP == this.HPMax) {
                        var cx = Math.floor(mob.pos.x / 32);
                        var cy = Math.floor(mob.pos.y / 32);
                        var path = dungeon.findPath(dungeon.pathGrid, tx, ty, cx, cy);
                        if (path.length > 0) {
                            this.explore(cx, cy, path);
                        }
                    }
                }
            }

            for (this.scanY = 1; this.scanY < this.DUNGEON_HEIGHT - 2; this.scanY++) {
                if (!this.isTravelling && !this.isFollowingPath) {
                    if (this.DiscoveredTiles[this.scanX][this.scanY] == 0) {
                        var path = dungeon.findPath(dungeon.pathGridMobs, tx, ty, this.scanX, this.scanY);
                        if (path.length > 0) {
                            this.explore(tx, ty, path);
                        }
                    }
                }


               
            }

            if (this.stairsFound != null) {
                if (!this.isTravelling && !this.isFollowingPath && dungeon.isComplete && dungeon.stairsOK && me.game.world.getEntityByProp("name", "mob").length == 0) {
                    var path = dungeon.findPath(dungeon.pathGrid, tx, ty, this.stairsFound.x, this.stairsFound.y);
                    if (path.length > 0) this.explore(tx, ty, path);
                }

                
            } else {
                if (!this.isTravelling && !this.isFollowingPath && dungeon.isComplete && me.game.world.getEntityByProp("name", "mob").length == 0) {
                    if (!dungeon.stairsOK) {
                        game.HUD.addLine("");
                        game.HUD.addLine("Hero is trapped!");
                        game.HUD.addLine("--- Game Over ---");
                        game.HUD.addFloatyText(new me.Vector2d((this.pos.x - 50) + Math.floor(Math.random() * 16), this.pos.y - 16), "Trapped!", "red", 3);
                        this.isGameOver = true;
                        this.gameOverTime = me.timer.getTime();
                    }
                }
            }

        }

        if (this.stairsFound != null && !this.isFollowingPath && dungeon.isComplete && tx == this.stairsFound.x && ty == this.stairsFound.y && me.game.world.getEntityByProp("name", "mob").length == 0 && !this.isTravelling) {
            me.game.viewport.fadeIn("#000000", 1000, this.nextLevel.bind(this));
            this.updateMovement();
            this.parent();
            return true;
        }
        
        this.scanX++;
        if (this.scanX == this.DUNGEON_WIDTH - 1) {
            this.scanX = 1;
        }
       

        this.updateMovement();

        this.parent();
       
        if (this.XP >= this.XPTNL) {
            this.Level++;
            this.LastXPTNL = this.XPTNL;
            this.XP -= this.XPTNL;
            this.XPTNL = 50 + (60 * this.Level);
            this.HPMax += this.Level;
            this.HP = this.HPMax;
            game.HUD.addFloatyText(new me.Vector2d((this.pos.x - 50) + Math.floor(Math.random() * 16), this.pos.y - 16), "Level Up!", "gold", 2);
            game.HUD.addLine("Hero is now level " + this.Level + "!");
        }

        return true;
    },

    attack: function(mob) {
        if (me.timer.getTime() < this.attackCooldown + this.attackCooldownTarget) return;

        this.attackCooldown = me.timer.getTime();

        mob.attackedBy(this);
    },

    attackedBy: function (attacker) {
        if (this.isGameOver) return;

        if (!this.isInCombat) {
            this.attackCooldown = me.timer.getTime() + (Math.random() * 2000);
            game.HUD.addFloatyText(new me.Vector2d((this.pos.x - 40) + Math.floor(Math.random() * 16), this.pos.y - 16), "Stunned!", "red");
        }
        this.isInCombat = true;

        var dam = attacker.DRMin + Math.floor(Math.random() * ((attacker.DRMax + 1) - attacker.DRMin));
        var sav = this.SRMin + Math.floor(Math.random() * ((this.SRMax + 1) - this.SRMin));

        var totaldam = dam - sav;

        var report = attacker.mobName + " attacks Hero";

        if (dam == 0) {
            report += " but misses!";
            game.HUD.addFloatyText(new me.Vector2d((attacker.pos.x - 20) + Math.floor(Math.random() * 16), attacker.pos.y), "Miss!", "white");
        }
        else {
            if (totaldam > 0) {
                report += " and hits for " + totaldam;
                game.HUD.addFloatyText(new me.Vector2d(this.pos.x + 3 + Math.floor(Math.random() * 16), this.pos.y), totaldam, "red");
                this.HP -= totaldam;
            }
            else {
                report = "Hero defends " + attacker.mobName + "'s attack!";
                game.HUD.addFloatyText(new me.Vector2d((this.pos.x - 30) + Math.floor(Math.random() * 16), this.pos.y), "Defend!", "green");

            }
        }

        game.HUD.addLine(report);
    },

    explore: function (tx, ty, path) {
        if (this.isInCombat) return;
        this.currentPath = path;
        this.currentPathStep = 0;
        this.target = new me.Vector2d((this.currentPath[this.currentPathStep].x * 32), (this.currentPath[this.currentPathStep].y * 32));
        //this.walkTween = new me.Tween(this.pos).to(this.target, 200).onComplete(this.targetReached.bind(this));
        //this.walkTween.easing(me.Tween.Easing.Linear.None);
        //this.walkTween.start();
        this.isTravelling = true;
        this.isFollowingPath = true;
        if (path.length > 10) game.HUD.addLine("Hero is exploring...");
    },

    targetReached: function () {
        var dungeon = me.game.world.getEntityByProp("name", "dungeon")[0];

        this.pos.x = this.target.x;
        this.pos.y = this.target.y;

        var tx = Math.floor(this.pos.x / 32);
        var ty = Math.floor(this.pos.y / 32);

        if (this.isEntering) {
            this.isEntering = false;
            dungeon.Tiles[0][9] = 1;
            dungeon.rebuild();
            me.game.viewport.shake(10, 500);
        }

        if (this.isInCombat) this.isFollowingPath = false;

        this.isTravelling = false;
        if (this.isFollowingPath) {
            if (this.currentPathStep < this.currentPath.length - 1) {
                var path = dungeon.findPath(dungeon.pathGrid, tx, ty, this.currentPath[this.currentPath.length - 1].x, this.currentPath[this.currentPath.length - 1].y);
                if (path.length > 0) {
                    this.currentPathStep++;
                    this.target = new me.Vector2d((this.currentPath[this.currentPathStep].x * 32), (this.currentPath[this.currentPathStep].y * 32));
                    //this.walkTween = new me.Tween(this.pos).to(this.target, 200).onComplete(this.targetReached.bind(this));
                    //this.walkTween.easing(me.Tween.Easing.Linear.None);
                    //this.walkTween.start();
                    this.isTravelling = true;
                } else {
                    this.isFollowingPath = false;
                }
            }
            else {
                this.isFollowingPath = false;
            }
        }
    },

    die: function () {
        if (!this.dying) {
            this.dying = true;
            this.renderable.setCurrentAnimation("die");
            this.renderable.setAnimationFrame(0);
            me.audio.play("tilly_die", false, null, 1);

            game.data.lives--;

            this.deathTimer = 100;

        }
    },

    reset: function () {

    },

    nextLevel: function () {
        game.Level++;
        this.pos = new me.Vector2d(-32, 9 * 32);
        this.target = new me.Vector2d(this.pos.x + (32 * 3), this.pos.y);
        this.init(this.pos.x, this.pos.y, { name: "hero", image: "hero" });
        var ftc = me.game.world.getEntityByProp("name", "FloatyTextContainer")[0];
        ftc.clear();
        me.game.reset();
        me.levelDirector.loadLevel("basedungeon");
        me.game.world.addChild(new game.Dungeon());
        me.game.world.addChild(new game.FallingPiece());
        me.game.viewport.fadeOut("#000000", 1000);
        
    },

    gameOver: function () {
        this.Level= 1;
        this.XP= 0;
        this.XPTNL= 50;
        this.LastXPTNL= 0;
        this.DRMin= 0;
        this.DRMax= 3;
        this.DRBase=3;
        this.SRMin= 0;
        this.SRMax= 2;
        this.SRBase= 2;
        this.HPMax= 10;
        this.HP= 10;
        game.Level = 1;
        for (var i = 0; i < 7; i++) game.HUD.addLine("");
        game.HUD.addLine("Welcome to DungeonFall");
        game.HUD.addLine("By Gareth Williams");
        game.HUD.addLine("");
        game.HUD.addLine("Arrows - Move/Rotate Piece");
        game.HUD.addLine("Z - Fast Drop");
        game.HUD.addLine("X - Instant Drop");
        game.HUD.addLine("");
        this.pos = new me.Vector2d(-32, 9 * 32);
        this.target = new me.Vector2d(this.pos.x + (32 * 3), this.pos.y);
        this.init(this.pos.x, this.pos.y, { name: "hero", image: "hero" });
        var ftc = me.game.world.getEntityByProp("name", "FloatyTextContainer")[0];
        ftc.clear();
        me.game.reset();
        me.levelDirector.loadLevel("basedungeon");
        me.game.world.addChild(new game.Dungeon());
        me.game.world.addChild(new game.FallingPiece());
        me.game.viewport.fadeOut("#000000", 1000);
    }

});