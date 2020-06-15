﻿using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;

namespace game_stuff
{
    public class BulletConfig
    {
        private TwoDVector LocalAim;

        public int LastTick;
        public Dictionary<BodySize, BulletBox> RawBulletBoxes;
        private IAntiActBuffConfig SuccessActBuffConfigToOpponent;

        private IAntiActBuffConfig FailActBuffConfigToSelf;
        private DamageBuffConfig[] DamageBuffConfigs;
        private int PauseToCaster;
        private int PauseToOpponent;
        private int LifeTime;
        private TwoDVector Speed;

        public ObjType ObjType;
        public int ResId;

        public Bullet GenBullet(TwoDPoint casterPos, TwoDVector casterAim, ref CharacterInBattle caster, int tough)
        {
            var fixedAim = casterAim.ClockwiseTurn(LocalAim);

            var dictionary = RawBulletBoxes.ToDictionary(pair => pair.Key,
                pair => pair.Value.GenBulletboxByRawBox(casterPos, fixedAim));


            return new Bullet(casterPos, fixedAim, dictionary, ref caster,
                SuccessActBuffConfigToOpponent,
                FailActBuffConfigToSelf, PauseToCaster, PauseToOpponent, DamageBuffConfigs, ObjType, tough, LastTick,
                ResId);
        }
    }
}