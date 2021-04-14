using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace game_config
{
    [Serializable]
    public class bad_words : IGameConfig
    {
        /// <summary> : </summary>
        public int id { get; set; }

        /// <summary> : </summary>
        public string name { get; set; }
    }

    [Serializable]
    public class base_attribute : IGameConfig
    {
        /// <summary> : </summary>
        public int id { get; set; }

        /// <summary> : </summary>
        public string info { get; set; }

        /// <summary> : </summary>
        public float MoveMaxSpeed { get; set; }

        /// <summary> : </summary>
        public float MoveAddSpeed { get; set; }

        /// <summary> : </summary>
        public float MoveMinSpeed { get; set; }

        /// <summary> : </summary>
        public float RecycleMulti { get; set; }

        /// <summary> : </summary>
        public uint Atk { get; set; }

        /// <summary> : </summary>
        public uint ShardedNum { get; set; }

        /// <summary> : </summary>
        public int MaxAmmo { get; set; }

        /// <summary> : </summary>
        public float ReloadMulti { get; set; }

        /// <summary> : </summary>
        public float BackStabAdd { get; set; }

        /// <summary> : </summary>
        public uint MaxHP { get; set; }

        /// <summary> : </summary>
        public float HealEffect { get; set; }

        /// <summary> : </summary>
        public uint MaxArmor { get; set; }

        /// <summary> : </summary>
        public uint ArmorDefence { get; set; }

        /// <summary> : </summary>
        public float ArmorFixEffect { get; set; }

        /// <summary> : </summary>
        public uint MaxShield { get; set; }

        /// <summary> : </summary>
        public float ShieldDelayTime { get; set; }

        /// <summary> : </summary>
        public float ShieldChargeEffect { get; set; }

        /// <summary> : </summary>
        public uint ShieldRecover { get; set; }

        /// <summary> : </summary>
        public uint ShieldInstability { get; set; }

        /// <summary> : </summary>
        public uint MaxTrapNum { get; set; }

        /// <summary> : </summary>
        public float TrapAtkMulti { get; set; }

        /// <summary> : </summary>
        public float TrapSurvivalMulti { get; set; }

        /// <summary> : </summary>
        public float HPAbsorb { get; set; }

        /// <summary> : </summary>
        public float ArmorAbsorb { get; set; }

        /// <summary> : </summary>
        public float ShieldAbsorb { get; set; }

        /// <summary> : </summary>
        public float AmmoAbsorb { get; set; }

        /// <summary> : </summary>
        public float ProtectAbsorb { get; set; }
    }

    [Serializable]
    public class battle_npc : IGameConfig
    {
        /// <summary> : </summary>
        public int id { get; set; }

        /// <summary> 说明: </summary>
        public string info { get; set; }

        /// <summary> 体型: </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public size BodyId { get; set; }

        /// <summary> 属性id: </summary>
        public int AttrId { get; set; }

        /// <summary> 携带武器: </summary>
        public int[] Weapons { get; set; }

        /// <summary> 武器组上限: </summary>
        public int MaxWeaponSlot { get; set; }

        /// <summary> : </summary>
        public int[] PropIds { get; set; }

        /// <summary> : </summary>
        public int PropPoint { get; set; }

        /// <summary> 全体奖励: </summary>
        public Gain[] AllDrops { get; set; }

        /// <summary> 击杀奖励: </summary>
        public Gain[] KillDrops { get; set; }

        /// <summary> 座驾 0表示没有: </summary>
        public int WithVehicleId { get; set; }

        /// <summary> 掉落交互物类型: </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public interactableType DropMapInteractableType { get; set; }

        /// <summary> 掉落交互物类型: </summary>
        public int DropMapInteractableId { get; set; }

        /// <summary> 被动范围: </summary>
        public int[] PassiveRange { get; set; }

        /// <summary> 被动数量: </summary>
        public int PassiveNum { get; set; }

        /// <summary> : </summary>
        public SimpleObj1[] ActWeight { get; set; }

        /// <summary> : </summary>
        public SimpleObj2 DoNotMinMaxTime { get; set; }

        /// <summary> 反应提示时间: </summary>
        public float ActShowDelayTime { get; set; }

        /// <summary> : </summary>
        public int MaxCombo { get; set; }
    }

    [Serializable]
    public class body : IGameConfig
    {
        /// <summary> id: </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public size id { get; set; }

        /// <summary> : </summary>
        public float mass { get; set; }

        /// <summary> : </summary>
        public float rad { get; set; }
    }

    [Serializable]
    public class bullet : IGameConfig
    {
        /// <summary> id: </summary>
        public string id { get; set; }

        /// <summary> 形状类型 1长方形 2圆形 3扇形 4线段 : </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public raw_shape ShapeType { get; set; }

        /// <summary> 形状参数 1:矩形（长宽）2圆形半径 3扇形（上顶点坐标）4线段（长度）: </summary>
        public float[] ShapeParams { get; set; }

        /// <summary> 本地位置: </summary>
        public Point LocalPos { get; set; }

        /// <summary> 本地旋转：度数: </summary>
        public int LocalRotate { get; set; }

        /// <summary> 攻击成功给对手的硬直状态: </summary>
        public Buff[] SuccessAntiActBuffConfigToOpponent { get; set; }

        /// <summary> : </summary>
        public bool CanOverBulletBlock { get; set; }

        /// <summary> j打击类型：range远程，失败触发吸收，melee近战，触发反击: </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public hit_type HitType { get; set; }

        /// <summary> 攻击失败获得的硬直状态,每个类型角色可以不一样，找不到使用默认的： 0：默认 1小型 2：中型 3：大型 : </summary>
        public Buff[] FailActBuffConfigToSelf { get; set; }

        /// <summary> 成功命中停顿帧数给自己: </summary>
        public int PauseToCaster { get; set; }

        /// <summary> 成功命中停顿帧数给对手: </summary>
        public int PauseToOpponent { get; set; }

        /// <summary> 目标类型 0:敌方 1己方: </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public target_type TargetType { get; set; }

        /// <summary> 攻击坚韧值:-1会按时间生成 </summary>
        public int Tough { get; set; }

        /// <summary> 属于慢速攻击：慢速攻击会被不攻击反制:Tough=-1时自动计算并覆盖 </summary>
        public bool IsHAtk { get; set; }

        /// <summary> 保护值: </summary>
        public int ProtectValue { get; set; }

        /// <summary> : </summary>
        public int SuccessAmmoAdd { get; set; }

        /// <summary> 0为默认值为释放时间加成: </summary>
        public float DamageMulti { get; set; }
    }

    [Serializable]
    public class caught_buff : IGameConfig
    {
        /// <summary> id: </summary>
        public string id { get; set; }

        /// <summary> stun时间: </summary>
        public float LastTime { get; set; }

        /// <summary> 抓取移动点，移动完抓取结束: </summary>
        public SimpleObj3[] CatchKeyPoints { get; set; }

        /// <summary> 同时触发的技能: </summary>
        public string TrickSkill { get; set; }
    }

    [Serializable]
    public class character : IGameConfig
    {
        /// <summary> : </summary>
        public int id { get; set; }

        /// <summary> 说明: </summary>
        public string info { get; set; }

        /// <summary> 体型: </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public size BodyId { get; set; }

        /// <summary> 属性id: </summary>
        public int AttrId { get; set; }

        /// <summary> 武器: </summary>
        public int[] Weapons { get; set; }

        /// <summary> 最大携带数: </summary>
        public int MaxWeaponSlot { get; set; }
    }

    [Serializable]
    public class interaction : IGameConfig
    {
        /// <summary> : </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public interactionAct id { get; set; }

        /// <summary> 基础坚韧: </summary>
        public int BaseTough { get; set; }

        /// <summary> 必须总时间: </summary>
        public float TotalTime { get; set; }
    }

    [Serializable]
    public class item : IGameConfig
    {
        /// <summary> id: </summary>
        public int id { get; set; }

        /// <summary> 名字: </summary>
        public string Name { get; set; }

        /// <summary> 配置名字，another_name为关键字，道具配置依靠此翻译为道具货币id: </summary>
        public string another_name { get; set; }

        /// <summary> 图标: </summary>
        public string Icon { get; set; }

        /// <summary> 类型: </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ItemType ItemType { get; set; }

        /// <summary> 游戏用道具: </summary>
        public bool IsPlayingItem { get; set; }

        /// <summary> 结束清除: </summary>
        public bool IsEndClear { get; set; }

        /// <summary> : </summary>
        public uint MaxStack { get; set; }

        /// <summary> : </summary>
        public int ShowType { get; set; }
    }

    [Serializable]
    public class lock_area : IGameConfig
    {
        /// <summary> id: </summary>
        public string id { get; set; }

        /// <summary> 形状类型 1长方形 2圆形 3扇形 4线段 : </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public raw_shape ShapeType { get; set; }

        /// <summary> 形状参数: </summary>
        public float[] ShapeParams { get; set; }

        /// <summary> 本地位置: </summary>
        public Point LocalPos { get; set; }

        /// <summary> 本地旋转：度数: </summary>
        public int LocalRotate { get; set; }
    }

    [Serializable]
    public class other_config : IGameConfig
    {
        /// <summary> 识别序号: </summary>
        public int id { get; set; }

        /// <summary> 最大高度: </summary>
        public float max_hegiht { get; set; }

        /// <summary> g加速度: </summary>
        public float g_acc { get; set; }

        /// <summary> : </summary>
        public float friction { get; set; }

        /// <summary> : </summary>
        public int tough_grow { get; set; }

        /// <summary> : </summary>
        public int mid_tough { get; set; }

        /// <summary> : </summary>
        public int weapon_num { get; set; }

        /// <summary> : </summary>
        public float two_s_to_see_pertick { get; set; }

        /// <summary> : </summary>
        public float two_s_to_see_pertick_medium_vehicle { get; set; }

        /// <summary> : </summary>
        public int qspace_max_per_level { get; set; }

        /// <summary> : </summary>
        public float hit_wall_add_time_by_speed_param { get; set; }

        /// <summary> : </summary>
        public float hit_wall_dmg_param { get; set; }

        /// <summary> : </summary>
        public float hit_wall_catch_time_param { get; set; }

        /// <summary> : </summary>
        public float hit_wall_catch_dmg_param { get; set; }

        /// <summary> 左上点长: </summary>
        public float sight_length { get; set; }

        /// <summary> 半宽: </summary>
        public float sight_width { get; set; }

        /// <summary> : </summary>
        public string common_fail_antibuff { get; set; }

        /// <summary> : </summary>
        public float tick_time { get; set; }

        /// <summary> : </summary>
        public float interaction_act2_call_time { get; set; }

        /// <summary> 标准保护值: </summary>
        public int trick_protect_value { get; set; }

        /// <summary> 保护时间: </summary>
        public float protect_time { get; set; }

        /// <summary> 标准最大道具数量: </summary>
        public int standard_max_prop_stack { get; set; }

        /// <summary> 标准回收道具数量: </summary>
        public int standard_recycle_prop_stack { get; set; }

        /// <summary> 道具半径: </summary>
        public float prop_R { get; set; }

        /// <summary> 武器半径: </summary>
        public float weapon_R { get; set; }

        /// <summary> 被动半径: </summary>
        public float pass_R { get; set; }

        /// <summary> 售卖盒半径: </summary>
        public float saleBox_R { get; set; }

        /// <summary> 至多陷阱数量: </summary>
        public int up_trap_max { get; set; }

        /// <summary> rogue模式复活限制时间: </summary>
        public float rogue_reborn_limit_time { get; set; }

        /// <summary> 碎片攻击倍数: </summary>
        public float ShardedAttackMulti { get; set; }

        /// <summary> 减速速度偏角最小Cos，小于减速到最低倍数: </summary>
        public float DecreaseMinCos { get; set; }

        /// <summary> 正常速度偏角最小Cos，小于开始减速倍数: </summary>
        public float NormalSpeedMinCos { get; set; }

        /// <summary> 减速到最小时的速度倍数: </summary>
        public float MoveDecreaseMinMulti { get; set; }

        /// <summary> : </summary>
        public int atkPassBuffId { get; set; }

        /// <summary> : </summary>
        public int defPassBuffId { get; set; }

        /// <summary> 传送相对角色增加半径: </summary>
        public float tele_gap { get; set; }

        /// <summary> : </summary>
        public int[] RogueChapters { get; set; }

        /// <summary> rogue模式复活要求: </summary>
        public Gain[] rogue_reborn_cost { get; set; }

        /// <summary> rogue模式复活计时时间: </summary>
        public float rogueRebornCountDownTime { get; set; }

        /// <summary> : </summary>
        public float rogueGameCheckTickTime { get; set; }
    }

    [Serializable]
    public class passive : IGameConfig
    {
        /// <summary> : </summary>
        public int id { get; set; }

        /// <summary> : </summary>
        public string info { get; set; }

        /// <summary> : </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public passive_type passive_effect_type { get; set; }

        /// <summary> : </summary>
        public Gain[] recycle_money { get; set; }

        /// <summary> : </summary>
        public float[] param_values { get; set; }
    }

    [Serializable]
    public class play_buff : IGameConfig
    {
        /// <summary> id: </summary>
        public int id { get; set; }

        /// <summary> : </summary>
        public float LastTime { get; set; }

        /// <summary> : </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public play_buff_effect_type EffectType { get; set; }

        /// <summary> : </summary>
        public float EffectValue { get; set; }

        /// <summary> : </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public stack_mode StackMode { get; set; }

        /// <summary> : </summary>
        public bool UseStack { get; set; }
    }

    [Serializable]
    public class prop : IGameConfig
    {
        /// <summary> : </summary>
        public int id { get; set; }

        /// <summary> : </summary>
        public string Info { get; set; }

        /// <summary> 消费道具: </summary>
        public int PropPointCost { get; set; }

        /// <summary> 0 普通1 被控制: </summary>
        public int UseCond { get; set; }

        /// <summary> 基础坚韧: </summary>
        public int BaseTough { get; set; }

        /// <summary> : </summary>
        public Dictionary<float, Media> LaunchTimeToEffectM { get; set; }

        /// <summary> : </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public bot_use_cond BotUseCondType { get; set; }

        /// <summary> : </summary>
        public float[] BotUseCondParam { get; set; }

        /// <summary> : </summary>
        public float PropMustTime { get; set; }

        /// <summary> 移动速度比率: </summary>
        public float MoveSpeedMulti { get; set; }

        /// <summary> 锁定方向: </summary>
        public bool LockAim { get; set; }

        /// <summary> 叠加运动开始时间,0为没有: </summary>
        public float MoveAddStartTime { get; set; }

        /// <summary> 叠加强制运动速度，x 轴为正前方，米每秒: </summary>
        public Point[] MoveAdds { get; set; }
    }

    [Serializable]
    public class push_buff : IGameConfig
    {
        /// <summary> id: </summary>
        public string id { get; set; }

        /// <summary> : </summary>
        public float LastTime { get; set; }

        /// <summary> 0：无UpForce: </summary>
        public int BuffType { get; set; }

        /// <summary> : </summary>
        public float PushForce { get; set; }

        /// <summary> 1方向 2中心: </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PushType PushType { get; set; }

        /// <summary> 1方向：修正的方向 2中心：修正的中心点 留空等于不修正默认: </summary>
        public Point[] FixVector { get; set; }

        /// <summary> : </summary>
        public float UpForce { get; set; }
    }

    [Serializable]
    public class radar_wave : IGameConfig
    {
        /// <summary> id: </summary>
        public string id { get; set; }

        /// <summary> 形状类型 1长方形 2圆形 3扇形 4线段 : </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public raw_shape ShapeType { get; set; }

        /// <summary> 形状参数 1:矩形（长宽）2圆形半径 3扇形（上顶点坐标）4线段（长度）: </summary>
        public float[] ShapeParams { get; set; }

        /// <summary> 本地位置: </summary>
        public Point LocalPos { get; set; }

        /// <summary> 本地旋转：度数: </summary>
        public int LocalRotate { get; set; }
    }

    [Serializable]
    public class rogue_game_chapter : IGameConfig
    {
        /// <summary> id: </summary>
        public int id { get; set; }

        /// <summary> : </summary>
        public int[] SmallMapResRandIn { get; set; }

        /// <summary> : </summary>
        public int[] StartRandIn { get; set; }

        /// <summary> : </summary>
        public int[] BigMapResRandIn { get; set; }

        /// <summary> : </summary>
        public int[] BossMapRandIn { get; set; }

        /// <summary> : </summary>
        public int[] HangarMapResRandIn { get; set; }

        /// <summary> : </summary>
        public int[] VendorMapResRandIn { get; set; }

        /// <summary> : </summary>
        public int SmallMap { get; set; }

        /// <summary> : </summary>
        public int BigMap { get; set; }

        /// <summary> : </summary>
        public int VendorMap { get; set; }

        /// <summary> : </summary>
        public int VendorMapStart { get; set; }

        /// <summary> : </summary>
        public int VendorMapRange { get; set; }

        /// <summary> : </summary>
        public int HangarMap { get; set; }

        /// <summary> : </summary>
        public int HangarMapStart { get; set; }

        /// <summary> : </summary>
        public int HangarMapRange { get; set; }

        /// <summary> : </summary>
        public bool StartWithBig { get; set; }

        /// <summary> : </summary>
        public bool EndWithBig { get; set; }

        /// <summary> : </summary>
        public int[] CreepRandIn { get; set; }

        /// <summary> : </summary>
        public int[] EliteRandomIn { get; set; }

        /// <summary> : </summary>
        public int SmallCreepNum { get; set; }

        /// <summary> : </summary>
        public int SmallEliteNum { get; set; }

        /// <summary> : </summary>
        public int[] SmallBossCreepRandIn { get; set; }

        /// <summary> : </summary>
        public int BigCreepNum { get; set; }

        /// <summary> : </summary>
        public int BigEliteNum { get; set; }

        /// <summary> : </summary>
        public int[] BigBossCreepRandIn { get; set; }
    }

    [Serializable]
    public class self_effect : IGameConfig
    {
        /// <summary> id: </summary>
        public string id { get; set; }

        /// <summary> : </summary>
        public float HealMulti { get; set; }

        /// <summary> : </summary>
        public float FixMulti { get; set; }

        /// <summary> : </summary>
        public float ShieldMulti { get; set; }

        /// <summary> : </summary>
        public float ReloadMulti { get; set; }

        /// <summary> : </summary>
        public int[] AddPlayBuffs { get; set; }
    }

    [Serializable]
    public class show_text : IGameConfig
    {
        /// <summary> id: </summary>
        public string id { get; set; }

        /// <summary> 配置名字，another_name为关键字，道具配置依靠此翻译为道具货币id: </summary>
        public string chinese { get; set; }
    }

    [Serializable]
    public class skill : IGameConfig
    {
        /// <summary> : </summary>
        public string id { get; set; }

        /// <summary> 消费弹药: </summary>
        public int AmmoCost { get; set; }

        /// <summary> 发动需要武器瞄准步数，: </summary>
        public int SnipeStepNeed { get; set; }

        /// <summary> 基础坚韧:0值会根据子弹发射最快的自动计算: </summary>
        public int BaseTough { get; set; }

        /// <summary> 在第一帧选目标的区域，空代表没有，如果选定目标，技能产生的移动会向目标调整,抓取引发的技能无效，因为技能发动时距离已设定好: </summary>
        public string LockArea { get; set; }

        /// <summary> 帧时--发射的子弹Id，时间会自动转换，从1开始，配置0帧的技能会无效: </summary>
        public Dictionary<float, string> LaunchTimeToBullet { get; set; }

        /// <summary> 从开始可以控制移动的时间，0表示不可控制: </summary>
        public float CanInputMove { get; set; }

        /// <summary> 强制移动开始时间: </summary>
        public float MoveStartTime { get; set; }

        /// <summary> 运动每帧，角色0度默认为水平向右，: </summary>
        public Point[] Moves { get; set; }

        /// <summary> 瞄准状态破坏Time：0代表不打消: </summary>
        public float BreakSnipeTime { get; set; }

        /// <summary> 技能必须帧（技能在释放完这些帧之前不能取消）: </summary>
        public float SkillMustTime { get; set; }

        /// <summary> 下个技能可输入帧: </summary>
        public float ComboInputStartTime { get; set; }

        /// <summary> 技能完整帧数: </summary>
        public float SkillMaxTime { get; set; }

        /// <summary> 下个技能状态值: </summary>
        public int NextCombo { get; set; }
    }

    [Serializable]
    public class skill_group : IGameConfig
    {
        /// <summary> : </summary>
        public string id { get; set; }

        /// <summary> 操作1：动作状态-技能: </summary>
        public Dictionary<int, string> Op1 { get; set; }

        /// <summary> 操作2：动作状态-技能: </summary>
        public Dictionary<int, string> Op2 { get; set; }

        /// <summary> 操作3：动作状态-技能: </summary>
        public Dictionary<int, string> Op3 { get; set; }

        /// <summary> Switch：动作状态-技能: </summary>
        public Dictionary<int, string> Switch { get; set; }
    }

    [Serializable]
    public class snipe : IGameConfig
    {
        /// <summary> id: </summary>
        public int id { get; set; }

        /// <summary> : </summary>
        public int TrickTick { get; set; }

        /// <summary> : </summary>
        public int TotalStep { get; set; }

        /// <summary> : </summary>
        public int OnTickSpeed { get; set; }

        /// <summary> : </summary>
        public int OffTickSpeed { get; set; }

        /// <summary> 原始值: </summary>
        public float MoveMulti { get; set; }
    }

    [Serializable]
    public class standard_level_up : IGameConfig
    {
        /// <summary> : </summary>
        public int id { get; set; }

        /// <summary> : </summary>
        public int next_exp { get; set; }

        /// <summary> : </summary>
        public float RebornTime { get; set; }

        /// <summary> : </summary>
        public int[] UpPassiveGet { get; set; }

        /// <summary> : </summary>
        public Gain[] FastRebornCost { get; set; }

        /// <summary> : </summary>
        public Gain[] TeamBonus { get; set; }

        /// <summary> : </summary>
        public Gain[] KillBonus { get; set; }
    }

    [Serializable]
    public class summon : IGameConfig
    {
        /// <summary> id: </summary>
        public string id { get; set; }

        /// <summary> : </summary>
        public string Setter { get; set; }
    }

    [Serializable]
    public class talent : IGameConfig
    {
        /// <summary> : </summary>
        public int id { get; set; }

        /// <summary> :被动技能Id </summary>
        public int passive_id { get; set; }

        /// <summary> : </summary>
        public int activeLevel { get; set; }

        /// <summary> : </summary>
        public Cost[] activeCost { get; set; }

        /// <summary> : </summary>
        public int addLevel { get; set; }

        /// <summary> : </summary>
        public Cost addLevelBaseCost { get; set; }

        /// <summary> : </summary>
        public Cost addLevelAddCost { get; set; }
    }

    [Serializable]
    public class trap : IGameConfig
    {
        /// <summary> : </summary>
        public string id { get; set; }

        /// <summary> 体型: </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public size BodyId { get; set; }

        /// <summary> 属性id: </summary>
        public int AttrId { get; set; }

        /// <summary> : </summary>
        public bool CanBeSee { get; set; }

        /// <summary> 打击失败次数，如果打击失败则增加1:0代表没有 </summary>
        public int FailChance { get; set; }

        /// <summary> :至少为2个tick这个是额外的 </summary>
        public float CallTrapRoundTime { get; set; }

        /// <summary> :0代表永续 </summary>
        public float MaxLifeTime { get; set; }

        /// <summary> :周期发射的Media </summary>
        public Media TrapMedia { get; set; }

        /// <summary> :通过TrapMedia触发的Media </summary>
        public Media[] LauchMedia { get; set; }

        /// <summary> :触发延迟，0保底1tick </summary>
        public float TrickDelayTime { get; set; }

        /// <summary> :触发次数，如果 </summary>
        public uint TrickStack { get; set; }

        /// <summary> 伤害倍率:伤害倍率，这个与释放装置的角色攻击有关 </summary>
        public float DamageMulti { get; set; }
    }

    [Serializable]
    public class vehicle : IGameConfig
    {
        /// <summary> : </summary>
        public int id { get; set; }

        /// <summary> 体型: </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public size BodyId { get; set; }

        /// <summary> 属性id: </summary>
        public int AttrId { get; set; }

        /// <summary> : </summary>
        public float DestoryDelayTime { get; set; }

        /// <summary> : </summary>
        public string DestoryBullet { get; set; }

        /// <summary> : </summary>
        public int[] Weapons { get; set; }

        /// <summary> : </summary>
        public int MaxWeaponSlot { get; set; }

        /// <summary> : </summary>
        public string OutActSkill { get; set; }

        /// <summary> : </summary>
        public Point VScope { get; set; }
    }

    [Serializable]
    public class weapon : IGameConfig
    {
        /// <summary> id: </summary>
        public int id { get; set; }

        /// <summary> : </summary>
        public SimpleObj4[] BodySizeUseAndSnipeSpeedFix { get; set; }

        /// <summary> j机器人使用射程: </summary>
        public float BotRange { get; set; }

        /// <summary> : </summary>
        public float MaxRangeMulti { get; set; }

        /// <summary> : </summary>
        public int ChangeRangeStep { get; set; }

        /// <summary> : </summary>
        public int Snipe1 { get; set; }

        /// <summary> : </summary>
        public int Snipe2 { get; set; }

        /// <summary> : </summary>
        public int Snipe3 { get; set; }
    }

    [Serializable]
    public class map_raws : IGameConfig
    {
        /// <summary> : </summary>
        public int id { get; set; }

        /// <summary> : </summary>
        public string info { get; set; }

        /// <summary> : </summary>
        public Point[][] WalkRawMap { get; set; }

        /// <summary> : </summary>
        public Point[][] SightRawMap { get; set; }

        /// <summary> : </summary>
        public Point[][] BulletRawMap { get; set; }

        /// <summary> : </summary>
        public Point[][] StartPoints { get; set; }

        /// <summary> : </summary>
        public TelPoint[] TransPoint { get; set; }
    }

    [Serializable]
    public enum ItemType
    {
        @bag,
        @battle_exp,
        @money
    }

    [Serializable]
    public enum size
    {
        @default,
        @medium,
        @tiny,
        @big,
        @small
    }

    [Serializable]
    public enum interactableType
    {
        @vehicle,
        @prop,
        @weapon
    }

    [Serializable]
    public enum botOp
    {
        @op1,
        @op2,
        @none
    }

    [Serializable]
    public enum raw_shape
    {
        @rectangle,
        @sector,
        @round
    }

    [Serializable]
    public enum buff_type
    {
        @caught_buff,
        @push_buff
    }

    [Serializable]
    public enum hit_type
    {
        @melee,
        @range
    }

    [Serializable]
    public enum target_type
    {
        @all_team,
        @other_team
    }

    [Serializable]
    public enum interactionAct
    {
        @get_in_vehicle,
        @pick_up_cage,
        @apply,
        @kick_vehicle,
        @get_info,
        @recycle_cage
    }

    [Serializable]
    public enum passive_type
    {
        @TickAdd,
        @TrapAbout,
        @Regen,
        @AddItem,
        @Attack,
        @Other,
        @Survive,
        @AbsorbAdd,
        @HitWinBuff
    }

    [Serializable]
    public enum play_buff_effect_type
    {
        @Tough,
        @TakeDamageAdd,
        @Break,
        @MakeDamageAdd
    }

    [Serializable]
    public enum stack_mode
    {
        @Stack,
        @OverWrite,
        @Time
    }

    [Serializable]
    public enum effect_media_type
    {
        @self,
        @radar_wave,
        @bullet,
        @summon
    }

    [Serializable]
    public enum bot_use_cond
    {
        @ArmorBlowPercent,
        @HpBlowPercent,
        @CantUse,
        @OnPatrolRandom,
        @EnemyOnSight,
        @ShieldBlowPercent
    }

    [Serializable]
    public enum PushType
    {
        @Center,
        @Vector
    }

    [Serializable]
    public enum direction
    {
        @West,
        @North,
        @East,
        @South
    }

    public static class ResNames
    {
        public static Dictionary<Type, string> NamesDictionary { get; } = new Dictionary<Type, string>
        {
            {typeof(bad_words), "bad_words_s.json"}, {typeof(base_attribute), "base_attribute_s.json"},
            {typeof(battle_npc), "battle_npc_s.json"}, {typeof(body), "body_s.json"}, {typeof(bullet), "bullet_s.json"},
            {typeof(caught_buff), "caught_buff_s.json"}, {typeof(character), "character_s.json"},
            {typeof(interaction), "interaction_s.json"}, {typeof(item), "item_s.json"},
            {typeof(lock_area), "lock_area_s.json"}, {typeof(other_config), "other_config_s.json"},
            {typeof(passive), "passive_s.json"}, {typeof(play_buff), "play_buff_s.json"}, {typeof(prop), "prop_s.json"},
            {typeof(push_buff), "push_buff_s.json"}, {typeof(radar_wave), "radar_wave_s.json"},
            {typeof(rogue_game_chapter), "rogue_game_chapter_s.json"}, {typeof(self_effect), "self_effect_s.json"},
            {typeof(show_text), "show_text_s.json"}, {typeof(skill), "skill_s.json"},
            {typeof(skill_group), "skill_group_s.json"}, {typeof(snipe), "snipe_s.json"},
            {typeof(standard_level_up), "standard_level_up_s.json"}, {typeof(summon), "summon_s.json"},
            {typeof(talent), "talent_s.json"}, {typeof(trap), "trap_s.json"}, {typeof(vehicle), "vehicle_s.json"},
            {typeof(weapon), "weapon_s.json"}, {typeof(map_raws), "map_raws_s.json"}
        };

        public static string[] Names { get; } =
        {
            "bad_words", "base_attribute", "battle_npc", "body", "bullet", "caught_buff", "character", "interaction",
            "item", "lock_area", "other_config", "passive", "play_buff", "prop", "push_buff", "radar_wave",
            "rogue_game_chapter", "self_effect", "show_text", "skill", "skill_group", "snipe", "standard_level_up",
            "summon", "talent", "trap", "vehicle", "weapon", "map_raws"
        };
    }

    [Serializable]
    public class ConfigDictionaries
    {
        /// <summary> 屏蔽词 </summary>
        public ImmutableDictionary<int, bad_words> bad_wordss { get; set; }

        /// <summary> 基本属性 </summary>
        public ImmutableDictionary<int, base_attribute> base_attributes { get; set; }

        /// <summary> 战斗NPC </summary>
        public ImmutableDictionary<int, battle_npc> battle_npcs { get; set; }

        /// <summary> 体型 </summary>
        public ImmutableDictionary<size, body> bodys { get; set; }

        /// <summary> 伤害子弹 </summary>
        public ImmutableDictionary<string, bullet> bullets { get; set; }

        /// <summary> 抓取buff </summary>
        public ImmutableDictionary<string, caught_buff> caught_buffs { get; set; }

        /// <summary> 角色 </summary>
        public ImmutableDictionary<int, character> characters { get; set; }

        /// <summary> 交互行动 </summary>
        public ImmutableDictionary<interactionAct, interaction> interactions { get; set; }

        /// <summary> 物品 </summary>
        public ImmutableDictionary<int, item> items { get; set; }

        /// <summary> 锁定媒介 </summary>
        public ImmutableDictionary<string, lock_area> lock_areas { get; set; }

        /// <summary> 其他 </summary>
        public ImmutableDictionary<int, other_config> other_configs { get; set; }

        /// <summary> 被动属性 </summary>
        public ImmutableDictionary<int, passive> passives { get; set; }

        /// <summary> 普通buff </summary>
        public ImmutableDictionary<int, play_buff> play_buffs { get; set; }

        /// <summary> 道具技能 </summary>
        public ImmutableDictionary<int, prop> props { get; set; }

        /// <summary> 推动buff </summary>
        public ImmutableDictionary<string, push_buff> push_buffs { get; set; }

        /// <summary> 雷达波媒介 </summary>
        public ImmutableDictionary<string, radar_wave> radar_waves { get; set; }

        /// <summary> rogue游戏章节 </summary>
        public ImmutableDictionary<int, rogue_game_chapter> rogue_game_chapters { get; set; }

        /// <summary> 自作用媒介 </summary>
        public ImmutableDictionary<string, self_effect> self_effects { get; set; }

        /// <summary> 字符串表 </summary>
        public ImmutableDictionary<string, show_text> show_texts { get; set; }

        /// <summary> 技能 </summary>
        public ImmutableDictionary<string, skill> skills { get; set; }

        /// <summary> 技能组 </summary>
        public ImmutableDictionary<string, skill_group> skill_groups { get; set; }

        /// <summary> 瞄准 </summary>
        public ImmutableDictionary<int, snipe> snipes { get; set; }

        /// <summary> 标准升级 </summary>
        public ImmutableDictionary<int, standard_level_up> standard_level_ups { get; set; }

        /// <summary> 召唤媒介 </summary>
        public ImmutableDictionary<string, summon> summons { get; set; }

        /// <summary> 天赋 </summary>
        public ImmutableDictionary<int, talent> talents { get; set; }

        /// <summary> 陷阱 </summary>
        public ImmutableDictionary<string, trap> traps { get; set; }

        /// <summary> 载具 </summary>
        public ImmutableDictionary<int, vehicle> vehicles { get; set; }

        /// <summary> 武器 </summary>
        public ImmutableDictionary<int, weapon> weapons { get; set; }

        /// <summary> 地图 </summary>
        public ImmutableDictionary<int, map_raws> map_rawss { get; set; }

        public IDictionary[] all_Immutable_dictionary { get; set; }
#if NETCOREAPP
        public ConfigDictionaries()
        {
            LoadAllByDll();


            all_Immutable_dictionary = new IDictionary[]
            {
                bad_wordss, base_attributes, battle_npcs, bodys, bullets, caught_buffs, characters, interactions, items,
                lock_areas, other_configs, passives, play_buffs, props, push_buffs, radar_waves, rogue_game_chapters,
                self_effects, show_texts, skills, skill_groups, snipes, standard_level_ups, summons, talents, traps,
                vehicles, weapons, map_rawss
            };
        }
#endif

        public ConfigDictionaries(string jsonPath = "")
        {
            LoadAllByJson(jsonPath);


            all_Immutable_dictionary = new IDictionary[]
            {
                bad_wordss, base_attributes, battle_npcs, bodys, bullets, caught_buffs, characters, interactions, items,
                lock_areas, other_configs, passives, play_buffs, props, push_buffs, radar_waves, rogue_game_chapters,
                self_effects, show_texts, skills, skill_groups, snipes, standard_level_ups, summons, talents, traps,
                vehicles, weapons, map_rawss
            };
        }

        public ConfigDictionaries(Dictionary<string, string> nameToJsonString)
        {
            LoadAllByJsonString(nameToJsonString);

            all_Immutable_dictionary = new IDictionary[]
            {
                bad_wordss, base_attributes, battle_npcs, bodys, bullets, caught_buffs, characters, interactions, items,
                lock_areas, other_configs, passives, play_buffs, props, push_buffs, radar_waves, rogue_game_chapters,
                self_effects, show_texts, skills, skill_groups, snipes, standard_level_ups, summons, talents, traps,
                vehicles, weapons, map_rawss
            };
        }

#if NETCOREAPP
        public void LoadAllByDll()
        {
            bad_wordss = GameConfigTools.GenConfigDict<int, bad_words>();
            base_attributes =
                GameConfigTools.GenConfigDict<int, base_attribute>();
            battle_npcs =
                GameConfigTools.GenConfigDict<int, battle_npc>();
            bodys = GameConfigTools.GenConfigDict<size, body>();
            bullets =
                GameConfigTools.GenConfigDict<string, bullet>();
            caught_buffs =
                GameConfigTools.GenConfigDict<string, caught_buff>();
            characters =
                GameConfigTools.GenConfigDict<int, character>();
            interactions =
                GameConfigTools.GenConfigDict<interactionAct, interaction>();
            items =
                GameConfigTools.GenConfigDict<int, item>();
            lock_areas =
                GameConfigTools.GenConfigDict<string, lock_area>();
            other_configs =
                GameConfigTools.GenConfigDict<int, other_config>();
            passives =
                GameConfigTools.GenConfigDict<int, passive>();
            play_buffs = GameConfigTools.GenConfigDict<int, play_buff>();
            props =
                GameConfigTools.GenConfigDict<int, prop>();
            push_buffs =
                GameConfigTools.GenConfigDict<string, push_buff>();
            radar_waves =
                GameConfigTools.GenConfigDict<string, radar_wave>();
            rogue_game_chapters =
                GameConfigTools.GenConfigDict<int, rogue_game_chapter>();
            self_effects =
                GameConfigTools.GenConfigDict<string, self_effect>();
            show_texts =
                GameConfigTools.GenConfigDict<string, show_text>();
            skills =
                GameConfigTools.GenConfigDict<string, skill>();
            skill_groups =
                GameConfigTools.GenConfigDict<string, skill_group>();
            snipes =
                GameConfigTools.GenConfigDict<int, snipe>();
            standard_level_ups =
                GameConfigTools.GenConfigDict<int, standard_level_up>();
            summons =
                GameConfigTools.GenConfigDict<string, summon>();
            talents = GameConfigTools.GenConfigDict<int, talent>();
            traps =
                GameConfigTools.GenConfigDict<string, trap>();
            vehicles = GameConfigTools.GenConfigDict<int, vehicle>();
            weapons =
                GameConfigTools.GenConfigDict<int, weapon>();
            map_rawss = GameConfigTools.GenConfigDict<int, map_raws>();
        }
#endif
        public void LoadAllByJson(string path = "")
        {
            bad_wordss = GameConfigTools.GenConfigDictByJsonFile<int, bad_words>(path);
            base_attributes = GameConfigTools.GenConfigDictByJsonFile<int, base_attribute>(path);
            battle_npcs = GameConfigTools.GenConfigDictByJsonFile<int, battle_npc>(path);
            bodys = GameConfigTools.GenConfigDictByJsonFile<size, body>(path);
            bullets = GameConfigTools.GenConfigDictByJsonFile<string, bullet>(path);
            caught_buffs = GameConfigTools.GenConfigDictByJsonFile<string, caught_buff>(path);
            characters = GameConfigTools.GenConfigDictByJsonFile<int, character>(path);
            interactions = GameConfigTools.GenConfigDictByJsonFile<interactionAct, interaction>(path);
            items = GameConfigTools.GenConfigDictByJsonFile<int, item>(path);
            lock_areas = GameConfigTools.GenConfigDictByJsonFile<string, lock_area>(path);
            other_configs = GameConfigTools.GenConfigDictByJsonFile<int, other_config>(path);
            passives = GameConfigTools.GenConfigDictByJsonFile<int, passive>(path);
            play_buffs = GameConfigTools.GenConfigDictByJsonFile<int, play_buff>(path);
            props = GameConfigTools.GenConfigDictByJsonFile<int, prop>(path);
            push_buffs = GameConfigTools.GenConfigDictByJsonFile<string, push_buff>(path);
            radar_waves = GameConfigTools.GenConfigDictByJsonFile<string, radar_wave>(path);
            rogue_game_chapters = GameConfigTools.GenConfigDictByJsonFile<int, rogue_game_chapter>(path);
            self_effects = GameConfigTools.GenConfigDictByJsonFile<string, self_effect>(path);
            show_texts = GameConfigTools.GenConfigDictByJsonFile<string, show_text>(path);
            skills = GameConfigTools.GenConfigDictByJsonFile<string, skill>(path);
            skill_groups = GameConfigTools.GenConfigDictByJsonFile<string, skill_group>(path);
            snipes = GameConfigTools.GenConfigDictByJsonFile<int, snipe>(path);
            standard_level_ups = GameConfigTools.GenConfigDictByJsonFile<int, standard_level_up>(path);
            summons = GameConfigTools.GenConfigDictByJsonFile<string, summon>(path);
            talents = GameConfigTools.GenConfigDictByJsonFile<int, talent>(path);
            traps = GameConfigTools.GenConfigDictByJsonFile<string, trap>(path);
            vehicles = GameConfigTools.GenConfigDictByJsonFile<int, vehicle>(path);
            weapons = GameConfigTools.GenConfigDictByJsonFile<int, weapon>(path);
            map_rawss = GameConfigTools.GenConfigDictByJsonFile<int, map_raws>(path);
        }

        public void LoadAllByJsonString(Dictionary<string, string> nameToJsonString)
        {
            bad_wordss = GameConfigTools.GenConfigDictByJsonString<int, bad_words>(nameToJsonString["bad_words"]);
            base_attributes =
                GameConfigTools.GenConfigDictByJsonString<int, base_attribute>(nameToJsonString["base_attribute"]);
            battle_npcs = GameConfigTools.GenConfigDictByJsonString<int, battle_npc>(nameToJsonString["battle_npc"]);
            bodys = GameConfigTools.GenConfigDictByJsonString<size, body>(nameToJsonString["body"]);
            bullets = GameConfigTools.GenConfigDictByJsonString<string, bullet>(nameToJsonString["bullet"]);
            caught_buffs =
                GameConfigTools.GenConfigDictByJsonString<string, caught_buff>(nameToJsonString["caught_buff"]);
            characters = GameConfigTools.GenConfigDictByJsonString<int, character>(nameToJsonString["character"]);
            interactions =
                GameConfigTools.GenConfigDictByJsonString<interactionAct, interaction>(nameToJsonString["interaction"]);
            items = GameConfigTools.GenConfigDictByJsonString<int, item>(nameToJsonString["item"]);
            lock_areas = GameConfigTools.GenConfigDictByJsonString<string, lock_area>(nameToJsonString["lock_area"]);
            other_configs =
                GameConfigTools.GenConfigDictByJsonString<int, other_config>(nameToJsonString["other_config"]);
            passives = GameConfigTools.GenConfigDictByJsonString<int, passive>(nameToJsonString["passive"]);
            play_buffs = GameConfigTools.GenConfigDictByJsonString<int, play_buff>(nameToJsonString["play_buff"]);
            props = GameConfigTools.GenConfigDictByJsonString<int, prop>(nameToJsonString["prop"]);
            push_buffs = GameConfigTools.GenConfigDictByJsonString<string, push_buff>(nameToJsonString["push_buff"]);
            radar_waves = GameConfigTools.GenConfigDictByJsonString<string, radar_wave>(nameToJsonString["radar_wave"]);
            rogue_game_chapters =
                GameConfigTools.GenConfigDictByJsonString<int, rogue_game_chapter>(
                    nameToJsonString["rogue_game_chapter"]);
            self_effects =
                GameConfigTools.GenConfigDictByJsonString<string, self_effect>(nameToJsonString["self_effect"]);
            show_texts = GameConfigTools.GenConfigDictByJsonString<string, show_text>(nameToJsonString["show_text"]);
            skills = GameConfigTools.GenConfigDictByJsonString<string, skill>(nameToJsonString["skill"]);
            skill_groups =
                GameConfigTools.GenConfigDictByJsonString<string, skill_group>(nameToJsonString["skill_group"]);
            snipes = GameConfigTools.GenConfigDictByJsonString<int, snipe>(nameToJsonString["snipe"]);
            standard_level_ups =
                GameConfigTools.GenConfigDictByJsonString<int, standard_level_up>(
                    nameToJsonString["standard_level_up"]);
            summons = GameConfigTools.GenConfigDictByJsonString<string, summon>(nameToJsonString["summon"]);
            talents = GameConfigTools.GenConfigDictByJsonString<int, talent>(nameToJsonString["talent"]);
            traps = GameConfigTools.GenConfigDictByJsonString<string, trap>(nameToJsonString["trap"]);
            vehicles = GameConfigTools.GenConfigDictByJsonString<int, vehicle>(nameToJsonString["vehicle"]);
            weapons = GameConfigTools.GenConfigDictByJsonString<int, weapon>(nameToJsonString["weapon"]);
            map_rawss = GameConfigTools.GenConfigDictByJsonString<int, map_raws>(nameToJsonString["map_raws"]);
        }
    }

    [Serializable]
    public class Gain : IGameConfig
    {
        public int item { get; set; }
        public int num { get; set; }
    }

    [Serializable]
    public class SimpleObj1 : IGameConfig
    {
        public int weight { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public botOp op { get; set; }
    }

    [Serializable]
    public class SimpleObj2 : IGameConfig
    {
        public float item1 { get; set; }
        public float item2 { get; set; }
    }

    [Serializable]
    public class Point : IGameConfig
    {
        public float x { get; set; }
        public float y { get; set; }
    }

    [Serializable]
    public class Buff : IGameConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public size size { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public buff_type buff_type { get; set; }

        public string buff_id { get; set; }
    }

    [Serializable]
    public class SimpleObj3 : IGameConfig
    {
        public float key_time { get; set; }
        public Point key_point { get; set; }
    }

    [Serializable]
    public class Media : IGameConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public effect_media_type media_type { get; set; }

        public string e_id { get; set; }
    }

    [Serializable]
    public class Cost : IGameConfig
    {
        public int item { get; set; }
        public int num { get; set; }
        public int first_use_pay { get; set; }
    }

    [Serializable]
    public class SimpleObj4 : IGameConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public size body { get; set; }

        public float snipe_speed_fix { get; set; }
        public string skill_group { get; set; }
    }

    [Serializable]
    public class TelPoint : IGameConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public direction Direction { get; set; }

        public Point[] Teleport { get; set; }
    }
}