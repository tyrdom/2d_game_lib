using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class CharacterStatus : IMoveBattleAttrModel, IBattleUnitStatus
    {
        public CharacterBody CharacterBody;

        public int BaseAttrId { get; }
        public float MaxMoveSpeed { get; private set; }

        public float MinMoveSpeed { get; }

        public float AddMoveSpeed { get; private set; }

        //move status
        public float NowMoveSpeed { get; private set; }

        public int GId { get; }

        //Snipe Status

        private SnipeAction? SnipeOnCallAct { get; set; }

        private int SnipeCallStack { get; set; }

        private SnipeAction? NowOnSnipeAct { get; set; }

        private int NowSnipeStep { get; set; }

        public Scope GetStandardScope()
        {
            return NowVehicle?.StandardScope ?? CharacterBody.Sight.StandardScope;
        }

        public float ListenRange { get; }

        public float GetListenRange()
        {
            return NowVehicle?.ListenRange ?? ListenRange;
        }

        //Ammo
        public int NowAmmo { get; set; }

        private int GetAmmo()
        {
            return NowVehicle?.NowAmmo ?? NowAmmo;
        }

        public int MaxAmmo { get; set; }
        public IBattleUnitStatus? LockingWho { get; set; }

        public CharacterBody GetFinalCaster()
        {
            return CharacterBody;
        }


        public CharacterStatus? CatchingWho { get; set; }

        public int NowWeapon { get; private set; }

        private int MaxWeaponSlot { get; }

        public int GetNowMaxWeaponSlotNum()
        {
            return NowVehicle?.WeaponCarryMax ?? MaxWeaponSlot;
        }

        public Dictionary<int, Weapon> Weapons { get; }

        public Dictionary<int, Weapon> GetWeapons()
        {
            return NowVehicle == null ? Weapons : NowVehicle.Weapons;
        }
        //Skill Status

        public ICharAct? NowCastAct { get; private set; }

        private void SetAct(ICharAct charAct)
        {
            if (charAct is Skill skill)
            {
                CostAmmo(skill.AmmoCost);
            }
#if DEBUG
            Console.Out.WriteLine($"now set act {charAct.GetTypeEnum()} : {charAct.GetIntId()}");
#endif
            NowCastAct = charAct;
            NextSkill = null;
            var startAct = new StartAct(charAct.GetTypeEnum(), charAct.GetIntId(),
                charAct.NowOnTick);
            CharEvents.Add(startAct);
        }

        private void CostAmmo(int skillAmmoCost)
        {
            if (NowVehicle != null)
            {
                NowVehicle.NowAmmo -= skillAmmoCost;
            }
            else
            {
                NowAmmo -= skillAmmoCost;
            }
        }

        private NextSkill? NextSkill { get; set; }

        //Prop
        public Prop? Prop { get; private set; }

        public int NowPropPoint { get; private set; }

        public int MaxPropPoint { get; private set; }

        private Skill DefaultTakeOutWeapon { get; }

        //Vehicle
        public Vehicle? NowVehicle { get; private set; }

        //traps

        private uint MaxTrap { get; }

        public Queue<Trap> Traps { get; }

        public float TrapSurvivalMulti { get; set; }

        public float TrapAtkMulti { get; set; }

        //InterAct CallLong status

        private MapInteract? NowMapInteractive { get; set; }
        private uint NowCallLongStack { get; set; }

        private uint MaxCallLongStack { get; }

        //be hit status
        public int PauseTick { get; private set; }

        public void SetPauseTick(int tick)
        {
            if (tick <= 0) return;
            PauseTick = tick;
            var getPauseTick = new GetPauseTick(tick);
            CharEvents.Add(getPauseTick);
        }

        public IStunBuff? StunBuff { get; private set; }

        public void SetStunBuff(IStunBuff stunBuff)
        {
            if (StunBuff != null) stunBuff.RestTick = MathTools.Max(StunBuff.RestTick, stunBuff.RestTick);
            StunBuff = stunBuff;
            var getStunBuff = new GetStunBuff(StunBuff.RestTick);
            CharEvents.Add(getStunBuff);
        }

        //protect status
        private int NowProtectValue { get; set; }
        private int MaxProtectValue { get; }

        public float GetProtectRate()
        {
            var nowProtectValue = (float) NowProtectValue / MaxProtectValue;
            return nowProtectValue;
        }

        private float ProtectTickMultiAdd { get; set; }
        public int NowProtectTick { get; private set; }

        //Passive

        public Dictionary<passive_id, PassiveTrait> PassiveTraits { get; }

        //playBuff

        public Dictionary<TrickCond, HashSet<IPlayingBuff>> BuffTrick { get; }

        private Dictionary<play_buff_id, IPlayingBuff> PlayingBuffs { get; }

        // Status
        public AttackStatus AttackStatus { get; }
        public DamageMultiStatus DamageMultiStatus { get; }

        public StunFixStatus StunFixStatus { get; }

        public void AttackStatusRefresh(float[] atkAboutPassiveEffects)
        {
            BattleUnitMoverStandard.AtkStatusRefresh(atkAboutPassiveEffects, this);
        }

        private void RegenStatusRefresh(float[] regenAttrPassiveEffects)
        {
            BattleUnitMoverStandard.RegenStatusRefresh(regenAttrPassiveEffects, this);
        }

        public void OtherStatusRefresh(float[] otherAttrPassiveEffects)
        {
            BattleUnitMoverStandard.OtherStatusRefresh(otherAttrPassiveEffects, this);
        }


        public SurvivalStatus SurvivalStatus { get; }

        public SurvivalStatus GetNowSurvivalStatus()
        {
            return NowVehicle != null ? NowVehicle.SurvivalStatus : SurvivalStatus;
        }

        public RegenEffectStatus RegenEffectStatus { get; }

        public TransRegenEffectStatus TransRegenEffectStatus { get; }
        public AbsorbStatus AbsorbStatus { get; }

        //wealth about
        private float RecycleMulti { get; set; }
        public PlayingItemBag PlayingItemBag { get; }

        public IScoreData ScoreData { get; }


        // for_tick_msg
        public HashSet<ICharEvent> CharEvents { get; }
        private HashSet<BaseChangeMark> BaseChangeMarks { get; }


        public CharacterStatus(int gId, int baseAttrId, PlayingItemBag playingItemBag,
            Dictionary<passive_id, PassiveTrait>? passiveTraits = null, int? maxWeaponNum = null)
        {
            ScoreData = new CharKillData();
            var genBaseAttrById = GameTools.GenBaseAttrById(baseAttrId);
            SurvivalStatus = SurvivalStatus.GenByConfig(genBaseAttrById);
            AttackStatus = AttackStatus.GenByConfig(genBaseAttrById);
            RegenEffectStatus = RegenEffectStatus.GenBaseByAttr(genBaseAttrById);
            AbsorbStatus = AbsorbStatus.GenBaseByAttr(genBaseAttrById);
            ProtectTickMultiAdd = 0;
            TrapAtkMulti = genBaseAttrById.TrapAtkMulti;
            TrapSurvivalMulti = genBaseAttrById.TrapSurvivalMulti;
            MaxTrap = Math.Min(genBaseAttrById.MaxTrapNum, (uint) CommonConfig.OtherConfig.up_trap_max);
            Traps = new Queue<Trap>();

            CharacterBody = null!;
            MaxMoveSpeed = genBaseAttrById.MoveMaxSpeed;
            BuffTrick = new Dictionary<TrickCond, HashSet<IPlayingBuff>>();
            GId = gId;
            PauseTick = 0;
            LockingWho = null;
            CatchingWho = null;
            NowWeapon = 0;
            Weapons = new Dictionary<int, Weapon>();
            NowCastAct = null;
            NextSkill = null;
            StunBuff = null;
            PlayingBuffs = new Dictionary<play_buff_id, IPlayingBuff>();

            NowProtectTick = 0;
            AddMoveSpeed = genBaseAttrById.MoveAddSpeed;
            MinMoveSpeed = genBaseAttrById.MoveMinSpeed;
            MaxProtectValue = CommonConfig.OtherConfig.trick_protect_value;
            PassiveTraits = passiveTraits ?? new Dictionary<passive_id, PassiveTrait>();
            ListenRange = genBaseAttrById.ListenRange;
            BaseAttrId = baseAttrId;
            PlayingItemBag = playingItemBag;
            DamageMultiStatus = new DamageMultiStatus();
            DefaultTakeOutWeapon = Skill.GenSkillById(CommonConfig.OtherConfig.default_take_out_skill);
            NowMoveSpeed = 0f;
            CharEvents = new HashSet<ICharEvent>();
            BaseChangeMarks = new HashSet<BaseChangeMark>();
            StunFixStatus = new StunFixStatus();
            TransRegenEffectStatus = new TransRegenEffectStatus();
            ResetSnipe();
            Prop = null;
            NowPropPoint = 0;
            MaxPropPoint = CommonConfig.OtherConfig.standard_max_prop_stack;
            NowVehicle = null;
            NowAmmo = 0;
            MaxAmmo = genBaseAttrById.MaxAmmo;
            MaxCallLongStack = StuffLocalConfig.MaxCallActTwoTick;
            NowCallLongStack = 0;
            NowMapInteractive = null;
            MaxWeaponSlot = maxWeaponNum ?? CommonConfig.OtherConfig.weapon_num;
            RecycleMulti = genBaseAttrById.RecycleMulti;
            StartPassiveInitRefresh();
        }

        private void BuffsGoATick()
        {
            var playingBuffsValues = PlayingBuffs.Values.ToArray();
            foreach (IPlayingBuff playingBuff in playingBuffsValues)
            {
                playingBuff.GoATick();
                if (playingBuff.IsFinish())
                {
                    PlayingBuffs.Remove(playingBuff.BuffId);
                }
            }
        }

        private void StartPassiveInitRefresh()
        {
#if DEBUG
            Console.Out.WriteLine($"init passives {PassiveTraits.Count}");
#endif
            var groupBy = PassiveTraits.Values.GroupBy(x => x.PassiveTraitEffect.GetType());

            foreach (var passiveTraits in groupBy)
            {
                var firstOrDefault = passiveTraits.FirstOrDefault();

                if (firstOrDefault == null)
                {
                    continue;
                }

#if DEBUG
                var aggregate1 = passiveTraits.Aggregate("",
                    ((s, trait) => s + trait.PassiveTraitEffect.GetType() + ""));
                Console.Out.WriteLine(
                    $"to init passive id key: {firstOrDefault.PassId} enum {passiveTraits.Count()} types : {aggregate1}");
#endif
                var aggregate = passiveTraits.Aggregate(new float[] { },
                    (s, x) => s.Plus(x.PassiveTraitEffect.GenEffect(x.Level).GetVector()));
#if DEBUG
                Console.Out.WriteLine(
                    $"array:{aggregate.Length} type : {firstOrDefault.PassiveTraitEffect.GetType()}");
#endif
                RefreshStatusByAKindOfPass(firstOrDefault, aggregate);
            }
        }

        private void OpChangeAim(TwoDVector? aim)
        {
            if (aim != null)
            {
                BaseChangeMarks.Add(BaseChangeMark.AimC);
            }

            var twoSToSeePerTick =
                NowVehicle == null
                    ? CommonConfig.OtherConfig.two_s_to_see_pertick
                    : CommonConfig.OtherConfig.two_s_to_see_pertick_medium_vehicle;

            var nowScope = GetNowScope();
            var opChangeAim = CharacterBody.Sight.OpChangeAim(aim, nowScope, twoSToSeePerTick);
            if (opChangeAim)
            {
                BaseChangeMarks.Add(BaseChangeMark.NowRc);
            }
        }

        public void Reborn()
        {
            PauseTick = 0;
            LockingWho = null;
            CatchingWho = null;
            NowCastAct = null;
            NextSkill = null;
            StunBuff = null;
            PlayingBuffs.Clear();
            SurvivalStatus.Full();
            NowProtectTick = 30;
            NowMoveSpeed = 0f;

            ResetSnipe();
            Prop = null;
            NowPropPoint = 0;
            TempMsgClear();
        }

        public Scope? GetNowScope()
        {
            if (NowOnSnipeAct == null || NowSnipeStep < 0 || !GetWeapons().TryGetValue(NowWeapon, out var weapon))
                return NowVehicle?.StandardScope ?? null;
            var weaponZoomStepScope = weapon.ZoomStepScopes[NowSnipeStep];
#if DEBUG
            Console.Out.WriteLine($"weapon Now Scope theta {weaponZoomStepScope.Theta}");
#endif
            return weaponZoomStepScope;
        }


        private CharGoTickResult LoadSkill(TwoDVector? aim, Skill skill, out bool ok,
            TwoDVector? moveOp = null)
        {
            //装载技能时，重置速度和锁定角色
            ResetSpeed();
            LockingWho = null;

            OpChangeAim(aim);


            if (!skill.Launch(NowSnipeStep, GetAmmo(), out var isLowAmmo))
            {
                ok = false;
                if (!isLowAmmo) return new CharGoTickResult();
                var lowAmmo = new LowAmmo();

                CharEvents.Add(lowAmmo);
                return new CharGoTickResult();
            }

            ok = true;
            SetAct(skill);

            var (posMedia, canInputMove) = skill.GetSkillStart(GetPos(), GetAim());
            if (!canInputMove || moveOp == null)
            {
                return new CharGoTickResult(launchBullet: posMedia);
            }

            var f = GetNowSnipe()?.GetSpeedMulti(CharacterBody.GetSize());
            var fixMove
                = f == null
                    ? moveOp.Multi(GetStandardSpeed(moveOp))
                    : moveOp.Multi(GetStandardSpeed(moveOp))
                        .Multi(f.Value);

            return new CharGoTickResult(launchBullet: posMedia, move: fixMove);
        }

        private void ResetSpeed()
        {
            NowMoveSpeed = MinMoveSpeed;
        }


        public void ResetCastAct()
        {
            switch (NowCastAct)
            {
                case null:
                    break;
                case Interaction aInteraction:
                    if (aInteraction.InMapInteractable.InWhichMapInteractive != null)
                        aInteraction.InMapInteractable.InWhichMapInteractive.NowInterCharacterBody = null;
                    break;
                case Prop _:
                    break;
                case Skill _:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(NowCastAct));
            }

            NowCastAct = null;
            NextSkill = null;
            LockingWho = null;
        }


        private Snipe? GetNowSnipe()
        {
            return NowTempSnipe;
        }

        private void CallSnipe(SnipeAction snipeAction)
        {
            if (snipeAction != SnipeAction.SnipeOff)
            {
                if (NowOnSnipeAct != snipeAction)
                {
                    if (SnipeOnCallAct == snipeAction)
                    {
                        SnipeCallStack += 1;
                    }
                    else
                    {
                        SnipeOnCallAct = snipeAction;
                        SnipeCallStack = 1;
                    }
                }


                if (!GetWeapons().TryGetValue(NowWeapon, out var weapon) ||
                    !weapon.Snipes.TryGetValue(snipeAction, out var snipe))
                {
                    if (NowTempSnipe == null) return;
                    SetNowOnSnipAct(SnipeAction.SnipeOff);
                    OffSnipe(NowTempSnipe);
                    return;
                }

                if (snipe.TrickTick >= SnipeCallStack && SnipeOnCallAct != NowOnSnipeAct)
                {
                    return;
                }

                SnipeCallStack = 0;
                NowTempSnipe = snipe;
                SetNowOnSnipAct(snipeAction);
                OnSnipe(NowTempSnipe);
            }
            else
            {
                if (NowTempSnipe != null)
                {
                    SetNowOnSnipAct(snipeAction);

                    OffSnipe(NowTempSnipe);
                }
                else ResetSnipe();
            }
        }

        private void SetNowOnSnipAct(SnipeAction snipeAction)
        {
            NowOnSnipeAct = snipeAction;
            var tickSnipeActionLaunch = new TickSnipeActionLaunch(snipeAction);
            CharEvents.Add(tickSnipeActionLaunch);
        }

        private Snipe? NowTempSnipe { get; set; }


        private void GoSnipe()
        {
            if (NowTempSnipe == null) return;
            switch (NowOnSnipeAct)
            {
                case SnipeAction.SnipeOn1:
                    OnSnipe(NowTempSnipe);
                    break;

                case SnipeAction.SnipeOn2:
                    OnSnipe(NowTempSnipe);
                    break;

                case SnipeAction.SnipeOff:
                    OffSnipe(NowTempSnipe);
                    break;
                case SnipeAction.SnipeOn3:
                    OnSnipe(NowTempSnipe);
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //开镜
        private void OnSnipe(Snipe snipe)
        {
            var temp = NowSnipeStep;
            NowSnipeStep = NowSnipeStep < snipe.MaxStep
                ? Math.Min(NowSnipeStep + snipe.AddStepPerTick, snipe.MaxStep)
                : Math.Max(NowSnipeStep - snipe.OffStepPerTick, snipe.MaxStep);
            if (temp == NowSnipeStep) return;
            BaseChangeMarks.Add(BaseChangeMark.ThetaC);
        }

        //关镜
        private void OffSnipe(Snipe snipe)
        {
            var temp = NowSnipeStep;
            var snipeOffStepPerTick = NowSnipeStep - snipe.OffStepPerTick;

            if (snipeOffStepPerTick < 0)
            {
                ResetSnipe();
                return;
            }

            NowSnipeStep = snipeOffStepPerTick;

            if (temp == NowSnipeStep) return;
            BaseChangeMarks.Add(BaseChangeMark.ThetaC);
        }

        //重置
        public void ResetSnipe()
        {
            SnipeOnCallAct = null;

            SnipeCallStack = 0;
            NowTempSnipe = null;
            NowOnSnipeAct = null;

            if (NowSnipeStep == -1) return;
            NowSnipeStep = -1;
            // var tickSnipeActionLaunches = CharEvents.OfType<TickSnipeActionLaunch>();
            // CharEvents.ExceptWith(tickSnipeActionLaunches);
            CharEvents.Add(new TickSnipeActionLaunch(SnipeAction.SnipeOff));
#if DEBUG
            Console.Out.WriteLine("snipe off by reset");
#endif
            BaseChangeMarks.Add(BaseChangeMark.ThetaC);
        }

        // private IPosMedia? SelfEffectFilter(IEffectMedia? effectMedia)
        // {
        //     switch (effectMedia)
        //     {
        //         case null:
        //             return null;
        //         case IPosMedia posMedia:
        //             return posMedia;
        //         case SelfEffect selfEffect:
        //             HitBySelfEffect(selfEffect);
        //             return null;
        //         default:
        //             throw new ArgumentOutOfRangeException(nameof(effectMedia));
        //     }
        // }

        private void HitBySelfEffect(SelfEffect selfEffect)
        {
            PlayBuffStandard.AddBuffs(PlayingBuffs, selfEffect.PlayingBuffToAdd);


            var selfEffectRegenerationBase = selfEffect.RegenerationBase;
            if (selfEffectRegenerationBase == null) return;
            var effectRegenerationBase = selfEffectRegenerationBase.Value;
            if (NowVehicle == null)
            {
                SurvivalStatus.GetRegen(effectRegenerationBase, RegenEffectStatus);
                ReloadAmmo(effectRegenerationBase.ReloadMulti);
            }
            else
            {
                NowVehicle.SurvivalStatus.GetRegen(effectRegenerationBase, NowVehicle.RegenEffectStatus);
                NowVehicle.ReloadAmmo(effectRegenerationBase.ReloadMulti);
            }
        }

        public void ReloadAmmo(float reloadMulti)
        {
            BattleUnitMoverStandard.ReloadAmmo(this, reloadMulti);
        }

        private CharGoTickResult GoNowActATick(ICharAct charAct,
            TwoDVector? moveOp, TwoDVector? aim)
        {
            if (charAct is Prop {LockAim: false}) OpChangeAim(aim);

            var limitV = charAct switch
            {
                Interaction _ => null,
                Prop _ => GetAim().Multi(GetStandardSpeed(GetAim())),
                Skill _ => LockingWho == null
                    ? null
                    : TwoDVector.TwoDVectorByPt(GetPos(), LockingWho.GetPos())
                        .ClockwiseTurn(CharacterBody.Sight.Aim)
                        .AddX(-GetRr() - LockingWho.GetRr() - (GetWeapons().TryGetValue(NowWeapon, out var aWeapon)
                            ? aWeapon.KeepDistance
                            : 0f))
                        .MaxFixX(0), //在有锁定目标时，会根据与当前目标的向量调整，有一定程度防止穿模型
                _ => throw new ArgumentOutOfRangeException(nameof(charAct))
            };
            var f = GetNowSnipe()?.GetSpeedMulti(CharacterBody.GetSize());
            var fixMove = charAct switch
            {
                Interaction _ => null,
                Prop _ => moveOp?.Multi(GetStandardSpeed(moveOp)),
                Skill _ => f == null
                    ? moveOp?.Multi(GetStandardSpeed(moveOp))
                    : moveOp?.Multi(GetStandardSpeed(moveOp))
                        .Multi(f.Value),
                _ => throw new ArgumentOutOfRangeException(nameof(charAct))
            };


#if DEBUG
            var lockingWhoGId = LockingWho == null ? "null" : LockingWho.GetId().ToString();
            Console.Out.WriteLine($"skill lock {lockingWhoGId} limitV ::{limitV}");
#endif

            var (move, bullet, snipeOff, getThing, interactive) = charAct
                .GoATick(GetPos(), GetAim(), fixMove, limitV);
            if (snipeOff)
            {
#if DEBUG
                Console.Out.WriteLine("res");

#endif
                ResetSnipe();
            }

            var ieToHashSet = bullet.IeToHashSet();
            var selfEffects = ieToHashSet.OfType<SelfEffect>();
            foreach (var selfEffect in selfEffects)
            {
                HitBySelfEffect(selfEffect);
            }

            var selfEffectFilter = ieToHashSet.OfType<IPosMedia>().ToArray();
            // var selfEffectFilter = SelfEffectFilter(bullet);

            if (getThing == null)
            {
                return new CharGoTickResult(move: move, launchBullet: selfEffectFilter);
            }

            var actResult = getThing.ActWhichChar(this, interactive);

            // HashSet<IMapInteractable>? dropThings1DropSet = null;
            // TelePortMsg? t = null;
            var mapInteractive = getThing is IApplyUnit ? null : getThing.InWhichMapInteractive;

            // switch (actResult)
            // {
            //     case null:
            //         break;
            //     case DropThings dropThings1:
            //         dropThings1DropSet = dropThings1.DropSet.IeToHashSet();
            //         break;
            //     case TelePortMsg telePort:
            //         t = telePort;
            //         break;
            //     default:
            //         throw new ArgumentOutOfRangeException(nameof(actResult));
            // }


            return new CharGoTickResult(move: move, launchBullet: selfEffectFilter,
                getThing: mapInteractive, actResults: actResult);
        }


        internal void RecycleAProp(Prop prop, int mapMarkId = -1)
        {
            NowPropPoint = Math.Min(MaxPropPoint,
                (int) (NowPropPoint + prop.RecyclePropStack * (1 + GetRecycleMulti())));
            if (mapMarkId < 0) return;
            var removeMapMark = new RemoveMapMark(mapMarkId);
            CharEvents.Add(removeMapMark);
        }

        private float GetRecycleMulti()
        {
            return NowVehicle?.RecycleMulti ?? RecycleMulti;
        }

        public void SetPropPoint(int v)
        {
            NowPropPoint = v;
        }

        public IMapInteractable? PicAWeapon(Weapon weapon, int mapMarkId = -1)
        {
            var pickedBySomebody = weapon.PickedBySomebody(this);
            var pickWeapon = new PickWeapon(weapon.WId);
            CharEvents.Add(pickWeapon);
            if (mapMarkId < 0) return pickedBySomebody;
            var removeMapMark = new RemoveMapMark(mapMarkId);
            CharEvents.Add(removeMapMark);
            return pickedBySomebody;
        }


        private IEnumerable<IMapInteractable> DropWeapon(size bodySize)
        {
            return GameTools.DropWeapon(Weapons, bodySize, GetPos());
        }

        public IEnumerable<IMapInteractable> GetInAVehicle(Vehicle vehicle)
        {
            if (NowVehicle != null) throw new Exception("have in a vehicle");

            var mapIntractable = DropWeapon(vehicle.Size);
            NowVehicle = vehicle;
            vehicle.Sign(this);
            return mapIntractable;
        }

        public IMapInteractable? PickAProp(Prop prop, int mapMarkId = -1)
        {
            prop.Sign(this);
            var pickAProp = new PickAProp(prop.PId);
            CharEvents.Add(pickAProp);
            if (mapMarkId >= 0)
            {
                var removeMapMark = new RemoveMapMark(mapMarkId);
                CharEvents.Add(removeMapMark);
            }

            if (Prop != null)
            {
                var dropAsIMapInteractable = Prop.DropAsIMapInteractable(GetPos());
                Prop = prop;
                return dropAsIMapInteractable;
            }

            Prop = prop;
            return null;
        }

        private void TempMsgClear()
        {
#if DEBUG
            Console.Out.WriteLine("temp msg clear~~~");
#endif
            CharEvents.Clear();
            BaseChangeMarks.Clear();
            GetNowSurvivalStatus().SurvivalChangeMarks.Clear();
        }


        /// <summary>
        /// char main fuc 
        /// </summary>
        /// <param name="operate"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal CharGoTickResult
            CharGoTick(Operate? operate) //角色一个tick行为
        {
            //清理消息缓存
            TempMsgClear();
            //  
            BuffsGoATick();
            // 命中停帧 输入无效
            var b1 = PauseTick > 0;

            if (b1)
            {
                PauseTick -= 1;
                return new CharGoTickResult();
            }

            //  检查保护 进入保护
            if (NowProtectValue > MaxProtectValue)
            {
                NowProtectTick = (int) (StuffLocalConfig.ProtectTick * (1 + ProtectTickMultiAdd));
                NowProtectValue = 0;
                var inProtect = new InProtect(NowProtectTick);
                CharEvents.Add(inProtect);
            }

            //  被硬直状态 输入无效
            var dPoint = GetPos();
            if (StunBuff != null)
            {
#if DEBUG
                Console.Out.WriteLine(
                    $"{GId}  {StunBuff.GetType()} ::anti v::{StunBuff.GetItp().ToString()}::anti buff :: {StunBuff.RestTick}");
#endif
                var (twoDPoint, antiActBuff) = StunBuff.GoTickDrivePos(dPoint);
                StunBuff = antiActBuff;

#if DEBUG
                Console.Out.WriteLine(
                    $"{GId} {StunBuff?.GetType()}  ::IPt {twoDPoint.ToString()} ::anti buff :: {StunBuff?.RestTick}");
#endif
                return new CharGoTickResult(move: twoDPoint);
            }

//
            if (NowVehicle != null)
            {
                var (isBroken, destroyBullet, weapons) = NowVehicle.GoATickCheckSurvival();
                if (isBroken)
                {
                    if (operate?.SpecialAction == SpecialAction.OutVehicle)
                    {
                        return OutNowVehicle();
                    }

                    if (destroyBullet == null)
                        return new CharGoTickResult();


                    NowVehicle = null;
                    NowProtectTick = 0;

                    var mapInteractableS = weapons.Select(x => x.DropAsIMapInteractable(GetPos()));
                    var dropThings = new DropThings(mapInteractableS);
                    var dropThingsList = new[] {dropThings}.OfType<IActResult>().ToImmutableArray();
                    return new CharGoTickResult(launchBullet: new IPosMedia[] {destroyBullet},
                        actResults: dropThingsList);
                }
            }


            if (!SurvivalStatus.GoATickAndCheckAlive())
            {
                return new CharGoTickResult(false);
            }
#if DEBUG
            // if (GId == 1)
            // {
            //     Console.Out.WriteLine($"id::{GId} now sv{SurvivalStatus} {SurvivalStatus.GetHashCode()}");
            // }
#endif
            //
            if (NowProtectTick > 0)
            {
                NowProtectTick -= 1;
            }

            // 当前技能结束检查
            if (NowCastAct?.InWhichPeriod() == SkillPeriod.End) NowCastAct = null;

            // 当前技能的释放时候
            var opAction = operate?.GetAction();
            if (NowCastAct != null)
            {
                // 当前动作进行一个tick
                var operateAim = operate?.Aim ?? operate?.Move; // 检查下一个连续技能，如果有连续技能可以切换，则切换到下一个技能,NextSkill为null
                var operateMove = operate?.Move;
                var actNowActATick =
                    ActNowActATickOrCombo(operateAim, operateMove);

#if DEBUG
                Console.Out.WriteLine($"{GId} skill on {NowCastAct.NowOnTick}");
                Console.Out.WriteLine($"skill move {actNowActATick.Move}");
                var launchBullet = actNowActATick.LaunchBullet;
                if (launchBullet.Any())
                    foreach (var bMedia in launchBullet)
                    {
                        Console.Out.WriteLine(
                            $"launch IHitAble::{bMedia.GetType()}::{bMedia.Aim}||{bMedia.Pos}");
                    }

#endif


                //没有更多Act操作，则返回
                if (opAction == null) return actNowActATick;

                //有Act操作，则检测出下一个状态id

                var weaponSkillStatus = NowCastAct.ComboInputRes();


                if (weaponSkillStatus == null) return actNowActATick;
                // 状态可用，则执行连技操作
                var status = weaponSkillStatus.Value;
                var b = opAction == SkillAction.Switch;
                var toUse = NowWeapon;
                if (b)
                {
                    status = 0;
                    toUse = (toUse + 1) % GetWeapons().Count;
                    ResetSnipe();
                }

                var nowWeapon = GetWeapons().TryGetValue(toUse, out var weapon)
                    ? weapon
                    : GetWeapons().First().Value;

                if (!nowWeapon.SkillGroups.TryGetValue(CharacterBody.GetSize(), out var immutableDictionary) ||
                    !immutableDictionary.TryGetValue(opAction.Value, out var skills)
                ) return actNowActATick;
                if (!skills.TryGetValue(status, out var skill) && !skills.TryGetValue(-1, out skill))
                    return actNowActATick;
                switch (NowCastAct.InWhichPeriod())
                {
                    case SkillPeriod.Casting:
                        //释放中则进入预输入动作
                        if (NextSkill == null && skill != null)
                        {
                            NextSkill = new NextSkill(operateAim, skill, opAction.Value);
                        }

                        break;
                    case SkillPeriod.CanCombo:
                        var charGoTickResult = LoadSkill(operateAim, skill, out var ok, operateMove);
                        actNowActATick = charGoTickResult;
                        if (ok)
                        {
                            NowWeapon = toUse;
                            var switchWeapon = new SwitchWeapon(GetWeapons()[NowWeapon].WId);
                            CharEvents.Add(switchWeapon);
                        }

                        break;
                    case SkillPeriod.End:
                        NowWeapon = toUse;
                        var switchWeapon2 = new SwitchWeapon(GetWeapons()[NowWeapon].WId);
                        CharEvents.Add(switchWeapon2);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return actNowActATick;
            }

            // 没有技能在释放
            // 首先运行现有存在的瞄准
            GoSnipe();
            // 没有任何操作
            if (operate == null)
            {
                ResetSpeed();
                OpChangeAim(null);
                return new CharGoTickResult();
            }

            // 有各种操作

            // 与地图上物品互动
            var mapInteractive = operate.GetMapInteractive();

            if (mapInteractive != null)
            {
                switch (mapInteractive)
                {
                    case MapInteract.PickCall:
                        return new CharGoTickResult(mapInteractiveAbout: (MapInteract.PickCall,
                            CharacterBody));
                    case MapInteract.InVehicleCall:
                        return NowVehicle == null
                            ? new CharGoTickResult(mapInteractiveAbout: (MapInteract.InVehicleCall,
                                CharacterBody))
                            : new CharGoTickResult();
                    case MapInteract.GetInfoCall:
                        return new CharGoTickResult(mapInteractiveAbout: (MapInteract.GetInfoCall,
                            CharacterBody));
                    case MapInteract.RecycleCall:
                        return CallLongTouch(MapInteract.RecycleCall)
                            ? new CharGoTickResult(mapInteractiveAbout: (MapInteract.RecycleCall,
                                CharacterBody))
                            : new CharGoTickResult();
                    case MapInteract.KickVehicleCall:
                        return CallLongTouch(MapInteract.KickVehicleCall) && NowVehicle == null
                            ? new CharGoTickResult(mapInteractiveAbout: (MapInteract.KickVehicleCall,
                                CharacterBody))
                            : new CharGoTickResult();
                    case MapInteract.BuyOrApplyCall:

                        return CallLongTouch(MapInteract.BuyOrApplyCall)
                            ? new CharGoTickResult(mapInteractiveAbout: (MapInteract.BuyOrApplyCall,
                                CharacterBody))
                            : new CharGoTickResult();
                    case null:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            ResetLongInterAct();

            // 瞄准请求
            var snipeAction = operate.GetSnipe();
            if (snipeAction != null)
            {
                CallSnipe(snipeAction.Value);
            }

            // 转换视野方向

            OpChangeAim(operate.Aim);

#if DEBUG
            Console.Out.WriteLine($"is skill start? snipe {snipeAction} skill {opAction}");
#endif
            //启动技能
            if (opAction != null)
            {
#if DEBUG
                Console.Out.WriteLine($"skill start {opAction.ToString()}");
#endif

                // 非连击状态切换武器
                if (opAction == SkillAction.Switch)
                {
#if DEBUG
                    Console.Out.WriteLine($"Now Weapon {NowWeapon} switch to");
#endif
                    NowWeapon = (NowWeapon + 1) % GetWeapons().Count;
                    var switchWeapon = new SwitchWeapon(GetWeapons()[NowWeapon].WId);
                    CharEvents.Add(switchWeapon);

#if DEBUG
                    Console.Out.WriteLine($"Now Weapon {NowWeapon} in {GetWeapons().Count()}");
#endif
                    ResetSnipe();
                    LoadSkill(operate.Aim, DefaultTakeOutWeapon, out _, operate.Move);
                }
                // 发动当前武器技能组的起始技能0
                else
                {
                    if (!GetWeapons().TryGetValue(NowWeapon, out var weapon) ||
                        !weapon.SkillGroups.TryGetValue(CharacterBody.GetSize(), out var value1) ||
                        !value1.TryGetValue(opAction.Value, out var value) ||
                        !value.TryGetValue(0, out var skill)) return new CharGoTickResult();

                    return LoadSkill(null, skill, out _, operate.Move);
                }
            }


            var specialAction = operate.GetSpecialAction();
            if (specialAction != null)
            {
                switch (specialAction)
                {
                    case SpecialAction.UseProp:
                        if (Prop == null) return new CharGoTickResult();
                        if (Prop.Launch(NowPropPoint))
                        {
                            NowPropPoint -= Prop.PropPointCost;
                            SetAct(Prop);
                        }
                        else
                        {
                            var lowProp = new LowProp();
                            CharEvents.Add(lowProp);
                        }

                        break;
                    case SpecialAction.OutVehicle:
                        return OutNowVehicle();


                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            //其他移动操作
            var twoDVector = operate.GetMove();

            if (twoDVector == null)
            {
                ResetSpeed();
                return new CharGoTickResult();
            }

            // 加速跑步运动，有上限
            if (operate.Aim == null)
            {
                OpChangeAim(twoDVector);
            }

            NowMoveSpeed = GetStandardSpeed(twoDVector);
            var nowSnipe = GetNowSnipe();
            var multiSpeed = NowMoveSpeed * nowSnipe?.GetSpeedMulti(CharacterBody.GetSize()) ?? NowMoveSpeed;
            var dVector = twoDVector.Multi(multiSpeed);

            return new CharGoTickResult(move: dVector);
        }

        private CharGoTickResult ActNowActATickOrCombo(TwoDVector? operateAim, TwoDVector? operateMove)
        {
            if (NextSkill == null ||
                NowCastAct == null ||
                NowCastAct.InWhichPeriod() != SkillPeriod.CanCombo)
                return ActNowActATick(operateAim, operateMove);
            var twoDVector = NextSkill.Aim ?? null;
            var aim = operateAim ?? twoDVector;
            var valueSkill = NextSkill.Skill;
            var nextSkillOpAction = NextSkill.OpAction;
            var charGoTickResult = LoadSkill(aim, valueSkill, out var ok, operateMove);

            if (ok)
            {
                if (nextSkillOpAction is SkillAction.Switch)
                {
                    NowWeapon = (NowWeapon + 1) % GetWeapons().Count;
                    var switchWeapon = new SwitchWeapon(GetWeapons()[NowWeapon].WId);
                    CharEvents.Add(switchWeapon);
                }
            }

            NextSkill = null;
            return charGoTickResult;
        }

        private CharGoTickResult ActNowActATick(TwoDVector? operateAim, TwoDVector? operateMove)
        {
            return NowCastAct == null ? new CharGoTickResult() : GoNowActATick(NowCastAct, operateMove, operateAim);
        }

        private CharGoTickResult OutNowVehicle()
        {
            if (NowVehicle == null) return new CharGoTickResult();
            {
                NowVehicle.OutAct.Launch(0, 0, out _);
                SetAct(NowVehicle.OutAct);
                NowVehicle.WhoDriveOrCanDrive = null;

                var genIMapInteractable = NowVehicle.DropAsIMapInteractable(GetPos());
                NowVehicle = null;

                var immutableArray =
                    new[] {(IActResult) new DropThings(new[] {genIMapInteractable})}.ToImmutableArray();
                return new CharGoTickResult(actResults: immutableArray);
            }
        }

        private void ResetLongInterAct()
        {
            NowMapInteractive = null;
            NowCallLongStack = 0;
        }

        private bool CallLongTouch(MapInteract mapInteract)
        {
            var b1 = NowMapInteractive == mapInteract;
            if (b1)
            {
                NowCallLongStack++;
            }
            else
            {
                NowMapInteractive = mapInteract;
                NowCallLongStack = 0;
            }

            var b = NowCallLongStack > MaxCallLongStack;
            if (b)
            {
                ResetLongInterAct();
            }
#if DEBUG
            Console.Out.WriteLine($"calling~~{NowCallLongStack}");
            if (b)
            {
                Console.Out.WriteLine($"start inter app {b}");
            }

#endif
            return b;
        }

        private float GetStandardSpeed(TwoDVector move)
        {
            var dot = move.Dot(CharacterBody.Sight.Aim);
            var normalSpeedMinCos = MathTools.Max(0f, MathTools.Min(1f,
                (dot - CommonConfig.OtherConfig.DecreaseMinCos) /
                (CommonConfig.OtherConfig.NormalSpeedMinCos - CommonConfig.OtherConfig.DecreaseMinCos)
            ));
            var moveDecreaseMinMulti = CommonConfig.OtherConfig.MoveDecreaseMinMulti +
                                       (1f - CommonConfig.OtherConfig.MoveDecreaseMinMulti) * normalSpeedMinCos;
            var maxMoveSpeed = GetMaxMoveSpeed() * moveDecreaseMinMulti;
            var nowMoveSpeed = MathTools.Max(GetMinMoveSpeed(), MathTools.Min(maxMoveSpeed,
                NowMoveSpeed + GetAddMoveSpeed()));
            return nowMoveSpeed;
        }

        private float GetAddMoveSpeed()
        {
            return NowVehicle?.AddMoveSpeed ?? AddMoveSpeed;
        }

        private float GetMinMoveSpeed()
        {
            return NowVehicle?.MinMoveSpeed ?? MinMoveSpeed;
        }

        private float GetMaxMoveSpeed()
        {
            return NowVehicle?.MaxMoveSpeed ?? MaxMoveSpeed;
        }

        public TwoDPoint GetPos()
        {
            return CharacterBody.NowPos;
        }

        public int GetId()
        {
            return GId;
        }

        public void BaseBulletAtkOk(int pauseToCaster, int ammoAddWhenSuccess, IBattleUnitStatus targetCharacterStatus)
        {
            SetPauseTick(pauseToCaster);
#if DEBUG
            Console.Out.WriteLine($"bullet hit!! caster pause tick {PauseTick}");
#endif
            AddAmmo(ammoAddWhenSuccess);
            //如果没有锁定目标，则锁定当前命中的目标
            // LockingWho ??= targetCharacterStatus;
        }

        public (UnitType unitType, int gid) GetTypeAndId()
        {
            return CharacterBody.GetTypeAndId();
        }

        public TwoDVector GetAim()
        {
            return CharacterBody.Sight.Aim;
        }

        public Damage GenDamage(float damageMulti, bool b4)
        {
            var multi = GetBuffs<MakeDamageBuff>().GetDamageMulti();
            var f = GetNowProtectMulti();
            var totalMulti = DamageMultiStatus.GetTotalMulti(GetNowSurvivalStatus(), f);
            var onBreakMulti = DamageMultiStatus.OnBreakMulti;
            var attackStatus = NowVehicle?.AttackStatus ?? AttackStatus;
            NowProtectValue = 0;
            return attackStatus.GenDamage(damageMulti, b4, multi + totalMulti, onBreakMulti);
        }

        private float GetNowProtectMulti()
        {
            if (NowProtectTick > 0)
            {
                return 1f;
            }

            var nowProtectValue = (float) NowProtectValue / MaxProtectValue;
            return nowProtectValue;
        }

        public void LoadCatchTrickSkill(TwoDVector? aim, CatchStunBuffMaker catchAntiActBuffMaker)
        {
            LoadSkill(aim, catchAntiActBuffMaker.TrickSkill, out _);
            NextSkill = null;
        }

        public float GetRr()
        {
            return CharacterBody.GetRr();
        }

        public void AddAKillScore(CharacterBody characterBody)
        {
            ScoreData.AddScore();
        }

        public bool IsDeadOrCantDmg()
        {
            return SurvivalStatus.IsDead();
        }

        public StunFixStatus GetStunFixStatus()
        {
            return StunFixStatus;
        }

        private void AddProtect(int protectValueAdd)
        {
            NowProtectValue += protectValueAdd;
        }

        public void AddAmmo(int ammoAdd)
        {
            if (NowVehicle == null)
            {
                NowAmmo = Math.Min(MaxAmmo, NowAmmo + ammoAdd);
            }
            else
            {
                NowVehicle.AddAmmo(ammoAdd);
            }
        }


        public void PassiveEffectChangeOther(float[] otherAttrPassiveEffects,
            (int MaxAmmo, float MoveMaxSpeed, float MoveMinSpeed, float MoveAddSpeed, int StandardPropMaxStack, float
                RecycleMulti) otherBaseStatus)
        {
            var lossAmmo = MaxAmmo - NowAmmo;
            var (maxAmmo, moveMaxSpeed, _, moveAddSpeed, _, recycleMulti) = otherBaseStatus;
            MaxAmmo = (int) (maxAmmo * (1f + otherAttrPassiveEffects[0]));
            NowAmmo = Math.Max(0, MaxAmmo - lossAmmo);
            var max = otherAttrPassiveEffects[1];
            MaxMoveSpeed = moveMaxSpeed * (1f + max / (max + 1f));
            var add = otherAttrPassiveEffects[2];
            AddMoveSpeed = moveAddSpeed * (1f + add / (add + 1f));
            var lossP = MaxPropPoint - NowPropPoint;
            MaxPropPoint = (int) (CommonConfig.OtherConfig.standard_max_prop_stack * (1f + otherAttrPassiveEffects[3]));
            NowPropPoint = MaxPropPoint - lossP;
            RecycleMulti = recycleMulti * (1f + otherAttrPassiveEffects[4]);
        }


        public bool LoadInteraction(Interaction charAct)
        {
            var b = NowCastAct == null;
            if (!b) return b;
            charAct.Launch();
            SetAct(charAct);
            return b;
        }

        public void FullAmmo()
        {
            NowAmmo = MaxAmmo;
            NowVehicle?.FullAmmo();
        }

        public void AddPlayingBuff(IEnumerable<IPlayingBuff> playingBuffs)
        {
            PlayBuffStandard.AddBuffs(PlayingBuffs, playingBuffs);
        }

        public void AddAPlayingBuff(IPlayingBuff playingBuff)
        {
            PlayBuffStandard.AddABuff(PlayingBuffs, playingBuff);
        }

        public float[] GetPassiveEffects<T>() where T : IPassiveTraitEffect
        {
            var passiveTraits = PassiveTraits.Values.Where(x => x.PassiveTraitEffect is T);
            var enumerable = passiveTraits as PassiveTrait[] ?? passiveTraits.ToArray();
            if (!enumerable.Any()) return new float[] { };
            var passiveTraitEffects = enumerable.Select(x => x.PassiveTraitEffect.GenEffect(x.Level));
            var traitEffects = passiveTraitEffects.ToList();
            var r = traitEffects.Aggregate(new float[] { }, (s, x) => s.Plus(x.GetVector()));
            return r;
        }


        public void SurvivalStatusRefresh(float[] survivalAboutPassiveEffects)
        {
            BattleUnitMoverStandard.SurvivalStatusRefresh(survivalAboutPassiveEffects, this);
        }


        private void RefreshStatusByAKindOfPass(PassiveTrait passiveTrait, float[] vector)
        {
            if (!vector.Any())
            {
                return;
            }

            switch (passiveTrait.PassiveTraitEffect)
            {
                case OtherAttrPassiveEffect _:
                    OtherStatusRefresh(vector);
                    NowVehicle?.OtherStatusRefresh(vector);
                    break;
                case RegenPassiveEffect _:
                    RegenStatusRefresh(vector);
                    NowVehicle?.RegenStatusRefresh(vector);
                    break;
                case SpecialDamageAddEffect _:
                    SpecialDamageMultiStatusRefresh(vector);
                    break;
                case StunFixEffect _:
                    StunFixStatusRefresh(vector);
                    break;
                case AtkAboutPassiveEffect _:
                    AttackStatusRefresh(vector);
                    NowVehicle?.AttackStatusRefresh(vector);
                    break;
                case HitPass _:
                    HitBuffTrickRefresh(vector);
                    break;
                case SurvivalAboutPassiveEffect _:
                    SurvivalStatusRefresh(vector);
                    NowVehicle?.SurvivalStatusRefresh(vector);
                    break;
                case AddItem _:
                    var itemId = (int) vector[0];
                    var num = (int) vector[1];
                    var gameItem = new GameItem((item_id) itemId, num);
                    PickGameItem(gameItem);
                    break;
                case AbsorbAboutPassiveEffect _:
                    AbsorbStatusRefresh(vector);
                    NowVehicle?.AbsorbStatusRefresh(vector);
                    break;
                case TrapEffect _:
                    TrapAboutRefresh(vector);
                    NowVehicle?.TrapAboutRefresh(vector);
                    break;
                case TickAddEffect _:
                    TickAboutRefresh(vector);
                    break;
                case TransRegenerationEffect _:
                    TransRegenerationEffectRefresh(vector);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void StunFixStatusRefresh(float[] vector)
        {
            StunFixStatus.PassiveEffectChange(vector);
        }

        private void TransRegenerationEffectRefresh(float[] vector)
        {
            TransRegenEffectStatus.TransRegenerationEffectChange(vector);
        }

        private void SpecialDamageMultiStatusRefresh(float[] vector)
        {
            DamageMultiStatus.RefreshSurvivalDmgMulti(vector);
        }


        private void HitBuffTrickRefresh(float[] vector)
        {
            var passiveEffect = (int) vector[0];
            var genById = PlayBuffStandard.GenById(CommonConfig.OtherConfig.atkPassBuffId);
            genById.Stack = passiveEffect;
            BuffTrick[TrickCond.MyAtkOk] = new HashSet<IPlayingBuff> {genById};


            var passiveEffect2 = (int) vector[1];
            var genById2 = PlayBuffStandard.GenById(CommonConfig.OtherConfig.defPassBuffId);
            genById2.Stack = passiveEffect2;

            BuffTrick[TrickCond.OpponentAtkFail] = new HashSet<IPlayingBuff> {genById};
        }

        private void AbsorbStatusRefresh(float[] vector)
        {
            BattleUnitMoverStandard.AbsorbStatusRefresh(vector, this);
        }

        public void PickAPassive(PassiveTrait passiveTrait)
        {
            var passiveTraitPassId = passiveTrait.PassId;
            if (PassiveTraits.TryGetValue(passiveTraitPassId, out var trait))
            {
                trait.AddLevel(passiveTrait.Level);
            }
            else
            {
                PassiveTraits.Add(passiveTraitPassId, passiveTrait);
            }

            float[] v;
            if (passiveTrait.PassiveTraitEffect is AddItem addItem)
            {
                v = addItem.GetVector();
            }
            else
            {
                var passiveTraits =
                    PassiveTraits.Values.Where(x =>
                    {
                        var b = x.PassiveTraitEffect.GetType() == passiveTrait.PassiveTraitEffect.GetType();

                        return b;
                    });
                v = passiveTraits.Aggregate(new float[] { },
                    (s, x) => s.Plus(x.PassiveTraitEffect.GenEffect(x.Level).GetVector()));
            }
#if DEBUG
            Console.Out.WriteLine($"passive v is {v.Aggregate("", (s, x) => s + "|" + x)}");
#endif
            var level = PassiveTraits[passiveTraitPassId].Level;
            RefreshStatusByAKindOfPass(passiveTrait, v);
            var getPassive = new GetPassive(passiveTraitPassId, level);
            CharEvents.Add(getPassive);
        }

        private void TickAboutRefresh(IReadOnlyList<float> tickAdds)
        {
            ProtectTickMultiAdd = tickAdds[0];
        }

        private void TrapAboutRefresh(float[] trapAdd)
        {
            BattleUnitMoverStandard.TrapAboutRefresh(trapAdd, this);
        }


        public void RecyclePassive(PassiveTrait passiveTrait)
        {
            if (!CommonConfig.Configs.passives.TryGetValue(passiveTrait.PassId, out var passive)) return;
            var passiveRecycleMoney =
                passive.recycle_money.Select(x =>
                    GameItem.GenByConfigGain(new Gain {item = x.item, num = (int) (x.num * (1 + GetRecycleMulti()))}));
            foreach (var gameItem in passiveRecycleMoney)
            {
                PickGameItem(gameItem);
            }
        }

        private void PickGameItem(GameItem gameItem)
        {
            PlayingItemBag.Gain(gameItem);


            var itemChange = new ItemChange(new[] {gameItem.ItemId});
            CharEvents.Add(itemChange);
        }

        public void PickGameItem(ImmutableList<GameItem> gameItems)
        {
            PlayingItemBag.Gain(gameItems);
            var enumerable = gameItems.Select(x => x.ItemId);
            var itemChange = new ItemChange(enumerable.ToArray());
            CharEvents.Add(itemChange);
        }

        public void AbsorbRangeBullet(TwoDPoint pos, int protectValueAdd, IBattleUnitStatus bodyCaster,
            float damageMulti, bool back, bullet_id bulletId)
        {
            if (NowCastAct is Skill skill && bodyCaster is CharacterStatus characterStatus)
            {
                skill.GenAbsorbBuffs(characterStatus, this);
            }

            SetHitMark(TwoDVector.TwoDVectorByPt(GetPos(), pos), bulletId);
            var pa = GetProtectAbsorb();
            var valueAdd = (int) (protectValueAdd * (1 + pa));
            AddProtect(valueAdd);

            var genDamage = bodyCaster.GenDamage(damageMulti, back);
            var total = genDamage.MainDamage + genDamage.ShardedDamage * genDamage.ShardedNum;
            var times = genDamage.ShardedNum;

            var ammoAbsorb = GetAmmoAbsorb();
            AddAmmo((int) (total * ammoAbsorb));
            if (NowVehicle != null)
                NowVehicle.AbsorbDamage(total, times, genDamage.ShardedDamage);
            else
                AbsorbDamage(total, times, genDamage.ShardedDamage);
        }

        private float GetAmmoAbsorb()
        {
            return NowVehicle?.AbsorbStatus.AmmoAbs ?? AbsorbStatus.AmmoAbs;
        }

        private void AbsorbDamage(uint total, uint times, uint shardedDamage)
        {
            SurvivalStatus.AbsorbDamage(total, times, AbsorbStatus, shardedDamage);
        }

        public void DirectStunBuffChange(TwoDPoint pos)
        {
            ResetSpeed();
            ResetSnipe();
            ResetSight();
            ResetCastAct();
            SetHitMark(TwoDVector.TwoDVectorByPt(GetPos(), pos), bullet_id.mine_atk);
            if (CatchingWho == null) return;
            CatchingWho.SetStunBuff(StuffLocalConfig.OutCaught(this));
            CatchingWho = null;
        }

        public DmgShow? BaseBeHitByBulletChange(TwoDPoint pos, int protectValueAdd, IBattleUnitStatus bodyCaster,
            float damageMulti, bool back, bullet_id bulletId)
        {
            ResetSpeed();
            ResetSnipe();
            ResetSight();
            ResetCastAct();
            SetHitMark(TwoDVector.TwoDVectorByPt(GetPos(), pos), bulletId);


            if (CatchingWho != null)
            {
                CatchingWho.SetStunBuff(StuffLocalConfig.OutCaught(this));
                CatchingWho = null;
            }

            AddProtect(protectValueAdd);
            var takeDamage = TakeDamage(bodyCaster.GenDamage(damageMulti, back));
            var b = bodyCaster.GetFinalCaster().Team != CharacterBody.Team;
            if (takeDamage is {IsKill: true} && b)
            {
                bodyCaster.AddAKillScore(CharacterBody);
            }

            return takeDamage;
        }

        private void ResetSight()
        {
            CharacterBody.Sight.Reset();
            BaseChangeMarks.Add(BaseChangeMark.NowRc);
        }

        private float GetProtectAbsorb()
        {
            var absorbStatusProtectAbs = NowVehicle?.AbsorbStatus.ProtectAbs ?? AbsorbStatus.ProtectAbs;
            return absorbStatusProtectAbs;
        }

        public DmgShow? TakeDamage(Damage genDamage)
        {
            var damageMulti = MathTools.Max(0, 1 + GetBuffs<TakeDamageBuff>().GetDamageMulti());
            genDamage.GetOtherMulti(damageMulti);
            if (NowVehicle == null)
            {
                var isDead = SurvivalStatus.IsDead();
                SurvivalStatus.TakeDamageAndEtc(genDamage, TransRegenEffectStatus);

                var after = SurvivalStatus.IsDead();
#if DEBUG
                // Console.Out.WriteLine(
                //     $"{GId} take damage {genDamage.MainDamage}| {genDamage.ShardedDamage}~{genDamage.ShardedNum} Sv {SurvivalStatus} {SurvivalStatus.GetHashCode()}");
#endif
                return new DmgShow(!isDead && after, genDamage);
            }

            var nowVehicleSurvivalStatus = NowVehicle.SurvivalStatus;
            nowVehicleSurvivalStatus.TakeDamageAndEtc(genDamage, TransRegenEffectStatus);
            return new DmgShow(false, genDamage);
        }

        public bool CheckCanBeHit()
        {
            return !SurvivalStatus.IsDead() || StunBuff != null;
        }

        public void AddTrap(Trap genATrap)
        {
            if (Traps.Count < MaxTrap)
            {
                Traps.Enqueue(genATrap);

#if DEBUG
                Console.Out.WriteLine($"not over flow {Traps.Count}");
#endif
            }
            else
            {
                var trap = Traps.Dequeue();
                //NotOverFlow false 会在下一时刻回收
                trap.NotOverFlow = false;
                Traps.Enqueue(genATrap);
#if DEBUG
                Console.Out.WriteLine($" over flow {Traps.Count}");
#endif
            }
        }

        public void PassiveEffectChangeTrap(float[] trapAdd,
            (float TrapAtkMulti, float TrapSurvivalMulti) trapBaseAttr)
        {
            BattleUnitMoverStandard.PassiveEffectChangeTrap(trapAdd, trapBaseAttr, this);
        }


        public bool CheckBuff(play_buff_id id)
        {
            return PlayingBuffs.TryGetValue(id, out var playingBuff) && playingBuff.Stack > 0;
        }

        public IEnumerable<T> GetBuffs<T>() where T : IPlayingBuff
        {
            var ofType = PlayingBuffs.Values.OfType<T>().ToArray();
            foreach (var playingBuff in ofType)
            {
                playingBuff.UseBuff();
                if (playingBuff.Stack <= 0)
                {
                    PlayingBuffs.Remove(playingBuff.BuffId);
                }
            }

            return ofType;
        }


        public void UseBuff(play_buff_id atkPassBuffId)
        {
            if (PlayingBuffs.TryGetValue(atkPassBuffId, out var playingBuff))
            {
                playingBuff.Stack -= 1;
            }
        }

        public int GetNowSnipeStep()
        {
            return NowSnipeStep;
        }

        public void SetHitMark(TwoDVector twoDVectorByPt, bullet_id bulletId)
        {
#if DEBUG
            Console.Out.WriteLine($"gid:{GId} be hit form {twoDVectorByPt}");
#endif
            var hitMark = new HitMark(twoDVectorByPt, bulletId);
            CharEvents.Add(hitMark);
        }

        public void NewPt()
        {
            BaseChangeMarks.Add(BaseChangeMark.PosC);
        }

        public IEnumerable<ICharEvent> GenBaseChangeMarksEvents()
        {
            var genBaseChangeMarksEvents = BaseChangeMarks.Select(x =>
            {
                ICharEvent charEvent = x switch
                {
                    BaseChangeMark.PosC => new PosChange(CharacterBody.NowPos),
                    BaseChangeMark.AimC => new AimChange(CharacterBody.Sight.Aim),
                    BaseChangeMark.NowRc => new SightRChange(CharacterBody.Sight.NowR),
                    BaseChangeMark.ThetaC => new SightAngleChange(GetNowScope()?.Theta ?? GetStandardScope().Theta),
                    _ => throw new ArgumentOutOfRangeException(nameof(x), x, null)
                };
                return charEvent;
            });
            return genBaseChangeMarksEvents;
        }

        public bool PlayingItemBagCost(ISaleUnit saleUnit)
        {
            var saleUnitCost = saleUnit.Cost;
            var playingItemBagCost = PlayingItemBag.Cost(saleUnitCost);
            if (playingItemBagCost)
            {
                var gameItem = saleUnitCost.ItemId;
                var itemChange = new ItemChange(new[] {gameItem});
                CharEvents.Add(itemChange);
                return playingItemBagCost;
            }


            foreach (var saleUnitOrCost in saleUnit.OrCosts)
            {
                var cost = PlayingItemBag.Cost(saleUnitOrCost);
                if (!cost) continue;
                var num = saleUnitOrCost.ItemId;
                var itemChange = new ItemChange(new[] {num});
                CharEvents.Add(itemChange);
                return true;
            }

            return false;
        }

        public void ResetProtect()
        {
            NowProtectTick = 0;
        }

        public void MakeProtect(int tick)
        {
            NowProtectTick = tick;
        }

        public bool Hear(Bullet bullet, SightMap? map)
        {
            if (bullet.WaveCast < 0)
            {
                return false;
            }

            var twoDVectorLine = new TwoDVectorLine(GetPos(), bullet.Pos);
            var bulletWaveCast = GetListenRange() + bullet.WaveCast;
            var distance = twoDVectorLine.GetVector().SqNorm();
            if (distance > bulletWaveCast * bulletWaveCast)
            {
                return false;
            }

            var isBlockSightLine = map?.IsBlockSightLine(twoDVectorLine) ?? false;

            return !isBlockSightLine;
        }

        public void TrickBlockSkill(TwoDPoint pos)
        {
            if (NowCastAct != null)
            {
                return;
            }

            var weapon = GetWeapons()[NowWeapon];
            if (weapon.BlockSkills.TryGetValue(CharacterBody.GetSize(), out var skill))
            {
                var twoDVector = GetPos().GenVector(pos).GetUnit2();
                LoadSkill(twoDVector, skill, out _);
            }

            NextSkill = null;
        }

        public void NowSkillTryTrickSkill(TwoDPoint twoDPoint)
        {
            if (!(NowCastAct is Skill skill)) return;
            var skillEnemyFailTrickSkill = skill.EnemyFailTrickSkill;
            if (skillEnemyFailTrickSkill == null)
            {
                return;
            }

            var twoDVector = GetPos().GenVector(twoDPoint).GetUnit2();
            LoadSkill(twoDVector, skillEnemyFailTrickSkill, out _);
            NextSkill = null;
        }

        public void RecycleWeapon(int mapMarkId)
        {
            FullAmmo();
            if (mapMarkId < 0) return;
            var removeMapMark = new RemoveMapMark(mapMarkId);
            CharEvents.Add(removeMapMark);
        }

        public int GetNowTough()
        {
            var toughUpBuffs = GetBuffs<ToughUpBuff>().Sum(x => x.PlayBuffEffectValue);
            if (NowCastAct == null)
            {
                return 0 + toughUpBuffs;
            }

            var nowTough = NowCastAct.NowTough + toughUpBuffs;
            return nowTough;
        }
    }


    public enum TrickCond
    {
        MyAtkOk,
        OpponentAtkFail
    }
}