using System;
using System.Collections.Generic;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public readonly struct CharTickMsg : ISeeTickMsg
    {
        public CharTickMsg(int getId, List<ICharEvent> characterStatusCharEvents)
        {
            Gid = getId;
            CharEvents = characterStatusCharEvents;
        }

        public int Gid { get; }

        public List<ICharEvent> CharEvents { get; }

        public override string ToString()
        {
            var pos = CharEvents.OfType<PosChange>().FirstOrDefault()?.Pos;
            var aim = CharEvents.OfType<AimChange>().FirstOrDefault()?.Aim;
            var sightR = CharEvents.OfType<SightRChange>().FirstOrDefault()?.SightR;
            var hpChange = CharEvents.OfType<HpChange>().FirstOrDefault()?.NowHp;
            var amc = CharEvents.OfType<ArmorChange>().FirstOrDefault()?.NowArmor;
            var sc = CharEvents.OfType<ShieldChange>().FirstOrDefault()?.NowShield;
            var hdv = CharEvents.OfType<HitMark>().FirstOrDefault()?.HitDirV;
            var getS = CharEvents.OfType<GetStunBuff>().FirstOrDefault()?.RestTick;
            var startAct = CharEvents.OfType<StartAct>().FirstOrDefault();
            return
                $"gid: {Gid} Pos: {pos} Aim {aim} SightR {sightR} \n Hp:{hpChange} Armor:{amc} Shield:{sc}\n" +
                $" is on hit::{hdv} , is get stun :: {getS},skill act {startAct?.TypeEnum} launch {startAct?.IntId} ";
        }
    }
}