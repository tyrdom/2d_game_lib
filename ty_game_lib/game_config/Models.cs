using System;using System.Collections;using System.Collections.Generic;using System.Collections.Immutable;using System.ComponentModel;using Newtonsoft.Json;using Newtonsoft.Json.Converters;namespace game_config {[Serializable] public class bad_words:IGameConfig {  
 /// <summary> : </summary>
public int id { get; set; }  
 /// <summary> : </summary>
public string name { get; set; }  } [Serializable] public class base_attribute:IGameConfig {  
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
public uint ShieldDelayTime { get; set; }  
 /// <summary> : </summary>
public float ShieldChargeEffect { get; set; }  
 /// <summary> : </summary>
public float ShieldChargeExtra { get; set; }  
 /// <summary> : </summary>
public float ShieldRecover { get; set; }  
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
 /// <summary> : </summary>
public float ListenRange { get; set; }  } [Serializable] public class battle_bot:IGameConfig {  
 /// <summary> : </summary>
public int id { get; set; }  
 /// <summary> 说明: </summary>
public string info { get; set; }  
 /// <summary> : </summary>
public SimpleObj1[] ActWeight { get; set; }  
 /// <summary> : </summary>
public SimpleObj2 DoNotMinMaxTime { get; set; }  
 /// <summary> 反应提示时间: </summary>
public uint ActShowDelayTime { get; set; }  
 /// <summary> : </summary>
public int MaxCombo { get; set; }  } [Serializable] public class battle_npc:IGameConfig {  
 /// <summary> : </summary>
public int id { get; set; }  
 /// <summary> : </summary>
public string Name { get; set; }  
 /// <summary> 说明: </summary>
public string info { get; set; }  
 /// <summary> 体型: </summary>
[JsonConverter(typeof(StringEnumConverter))]public size BodyId { get; set; }  
 /// <summary> 基础属性id: </summary>
public int AttrId { get; set; }  
 /// <summary> 携带武器: </summary>
public string[] Weapons { get; set; }  
 /// <summary> 武器组上限: </summary>
public int MaxWeaponSlot { get; set; }  
 /// <summary> : </summary>
public string[] PropIds { get; set; }  
 /// <summary> : </summary>
public int PropPoint { get; set; }  
 /// <summary> 全体奖励: </summary>
public Gain[] AllDrops { get; set; }  
 /// <summary> 击杀奖励: </summary>
public Gain[] KillDrops { get; set; }  
 /// <summary> 座驾 空表示没有: </summary>
public string WithVehicleId { get; set; }  
 /// <summary> 掉落交互物类型: </summary>
[JsonConverter(typeof(StringEnumConverter))]public interactableType DropMapInteractableType { get; set; }  
 /// <summary> 掉落交互物类下Id范围: </summary>
public string[] DropMapInteractableId { get; set; }  
 /// <summary> 被动范围: </summary>
public string[] PassiveRange { get; set; }  
 /// <summary> 被动数量: </summary>
public int PassiveNum { get; set; }  
 /// <summary> : </summary>
public int botId { get; set; }  } [Serializable] public class body:IGameConfig {  
 /// <summary> id: </summary>
[JsonConverter(typeof(StringEnumConverter))]public size id { get; set; }  
 /// <summary> : </summary>
public float mass { get; set; }  
 /// <summary> : </summary>
public float rad { get; set; }  } [Serializable] public class bot_other_config:IGameConfig {  
 /// <summary> 识别序号: </summary>
public int id { get; set; }  
 /// <summary> 巡逻速度衰减到倍数: </summary>
public float PatrolSlowMulti { get; set; }  
 /// <summary> 对点达到距离: </summary>
public float CloseEnoughDistance { get; set; }  
 /// <summary> 巡逻范围比率: </summary>
public float PatrolMin { get; set; }  
 /// <summary> 巡逻范围比率: </summary>
public float PatrolMax { get; set; }  
 /// <summary> 最大锁定追踪距离: </summary>
public float MaxTraceDistance { get; set; }  
 /// <summary> 锁定追踪时间: </summary>
public uint LockTraceTickTime { get; set; }  } [Serializable] public class bullet:IGameConfig {  
 /// <summary> id: </summary>
[JsonConverter(typeof(StringEnumConverter))]public bullet_id id { get; set; }  
 /// <summary> 形状类型 1长方形 2圆形 3扇形 4线段 : </summary>
[JsonConverter(typeof(StringEnumConverter))]public raw_shape ShapeType { get; set; }  
 /// <summary> 发声音半径，负数为静音: </summary>
public float SoundWaveRad { get; set; }  
 /// <summary> 形状参数 1:矩形（长宽）2圆形半径 3扇形（上顶点坐标）4线段（起点00，终点x,y，正左方为前）: </summary>
public float[] ShapeParams { get; set; }  
 /// <summary> 本地位置: </summary>
public Point LocalPos { get; set; }  
 /// <summary> 命中音效: </summary>
public string HitSound { get; set; }  
 /// <summary> 本地旋转：度数:线段无效: </summary>
public int LocalRotate { get; set; }  
 /// <summary> 攻击成功给对手的硬直状态: </summary>
public Buff[] SuccessAntiActBuffConfigToOpponent { get; set; }  
 /// <summary> 最大目标数量，由近到远：0表示无限制: </summary>
public int MaxHitNum { get; set; }  
 /// <summary> 可以穿过子弹阻挡？: </summary>
public bool CanOverBulletBlock { get; set; }  
 /// <summary> j打击类型：range远程，失败触发吸收，melee近战，触发反击: </summary>
[JsonConverter(typeof(StringEnumConverter))]public hit_type HitType { get; set; }  
 /// <summary> 属于慢速攻击：慢速攻击会被不攻击反制:Tough=-1时自动计算并覆盖 </summary>
public bool IsHAtk { get; set; }  
 /// <summary> 近战攻击失败获得的硬直状态（远程为触发吸收机制，此字段无效）,每个类型角色可以不一样，找不到使用默认的： default：默认 small：小型 medium：中型 big：大型 : </summary>
public Buff[] FailActBuffConfigToSelf { get; set; }  
 /// <summary> 成功命中停顿帧数给自己: </summary>
public uint PauseToCaster { get; set; }  
 /// <summary> 成功命中停顿帧数给对手: </summary>
public uint PauseToOpponent { get; set; }  
 /// <summary> 目标类型 0:敌方 1己方: </summary>
[JsonConverter(typeof(StringEnumConverter))]public target_type TargetType { get; set; }  
 /// <summary> 攻击坚韧值:-1会按时间生成 </summary>
public int Tough { get; set; }  
 /// <summary> 保护值:-1会按时间生成 </summary>
public int ProtectValue { get; set; }  
 /// <summary> 攻击成功增加ammo: </summary>
public int SuccessAmmoAdd { get; set; }  
 /// <summary> 伤害倍率，0为使用释放时间加成倍率值: </summary>
public float DamageMulti { get; set; }  } [Serializable] public class caught_buff:IGameConfig {  
 /// <summary> id: </summary>
public string id { get; set; }  
 /// <summary> stun时间: </summary>
public uint LastTime { get; set; }  
 /// <summary> 抓取移动点，移动完抓取结束: </summary>
public SimpleObj3[] CatchKeyPoints { get; set; }  
 /// <summary> 同时触发的技能: </summary>
public string TrickSkill { get; set; }  } [Serializable] public class character:IGameConfig {  
 /// <summary> : </summary>
public int id { get; set; }  
 /// <summary> 说明: </summary>
public string info { get; set; }  
 /// <summary> 体型: </summary>
[JsonConverter(typeof(StringEnumConverter))]public size BodyId { get; set; }  
 /// <summary> 属性id: </summary>
public int AttrId { get; set; }  
 /// <summary> 武器: </summary>
public int[] Weapons { get; set; }  
 /// <summary> 最大携带数: </summary>
public int MaxWeaponSlot { get; set; }  } [Serializable] public class interaction:IGameConfig {  
 /// <summary> : </summary>
[JsonConverter(typeof(StringEnumConverter))]public interactionAct id { get; set; }  
 /// <summary> 基础坚韧: </summary>
public int BaseTough { get; set; }  
 /// <summary> 必须总时间: </summary>
public uint TotalTime { get; set; }  
 /// <summary> 音效: </summary>
public string LaunchSound { get; set; }  
 /// <summary> 延迟: </summary>
public float Delay { get; set; }  } [Serializable] public class item:IShowSchemeConfig,IGameConfig {  
 /// <summary> id: </summary>
[JsonConverter(typeof(StringEnumConverter))]public item_id id { get; set; }  
 /// <summary> 名字: </summary>
public string Name { get; set; }  
 /// <summary> : </summary>
public string Describe { get; set; }  
 /// <summary> 配置名字，another_name为关键字，道具配置依靠此翻译为道具货币id: </summary>
public string another_name { get; set; }  
 /// <summary> 图标: </summary>
public string icon { get; set; }  
 /// <summary> 类型: </summary>
[JsonConverter(typeof(StringEnumConverter))]public ItemType ItemType { get; set; }  
 /// <summary> 游戏用道具: </summary>
public bool IsPlayingItem { get; set; }  
 /// <summary> 结束清除: </summary>
public bool IsEndClear { get; set; }  
 /// <summary> : </summary>
public uint MaxStack { get; set; }  
 /// <summary> : </summary>
public int ShowType { get; set; }  } [Serializable] public class lock_area:IGameConfig {  
 /// <summary> id: </summary>
[JsonConverter(typeof(StringEnumConverter))]public lock_area_id id { get; set; }  
 /// <summary> 形状类型 1长方形 2圆形 3扇形 4线段 : </summary>
[JsonConverter(typeof(StringEnumConverter))]public raw_shape ShapeType { get; set; }  
 /// <summary> 形状参数: </summary>
public float[] ShapeParams { get; set; }  
 /// <summary> 本地位置: </summary>
public Point LocalPos { get; set; }  
 /// <summary> 本地旋转：度数: </summary>
public int LocalRotate { get; set; }  } [Serializable] public class map_raws:IGameConfig {  
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
public TelPoint[] TransPoint { get; set; }  } [Serializable] public class other_config:IGameConfig {  
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
public uint hit_wall_add_time_by_speed_param { get; set; }  
 /// <summary> : </summary>
public float hit_wall_dmg_param { get; set; }  
 /// <summary> : </summary>
public uint hit_wall_catch_time_param { get; set; }  
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
public uint interaction_act2_call_time { get; set; }  
 /// <summary> 标准保护值: </summary>
public int trick_protect_value { get; set; }  
 /// <summary> 保护时间: </summary>
public uint protect_time { get; set; }  
 /// <summary> 标准伤害保护倍数: </summary>
public int protect_standardMulti { get; set; }  
 /// <summary> 近战成功获得弹药默认计算倍数: </summary>
public int melee_ammo_gain_standard_multi { get; set; }  
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
public string RandShowItem { get; set; }  
 /// <summary> 游戏章节序列: </summary>
public int[] RogueChapters { get; set; }  
 /// <summary> rogue模式复活要求: </summary>
public Gain[] rogue_reborn_cost { get; set; }  
 /// <summary> rogue模式复活计时时间: </summary>
public uint rogueRebornCountDownTime { get; set; }  
 /// <summary> rogue模式过关计时时间: </summary>
public uint rogueChapterPassCountDownTime { get; set; }  
 /// <summary> : </summary>
public uint rogueGameCheckTickTime { get; set; }  
 /// <summary> 表格对应枚举类型: </summary>
[JsonConverter(typeof(StringEnumConverter))]public can_be_hit config_type_enum1 { get; set; }  
 /// <summary> 表格对应枚举类型: </summary>
[JsonConverter(typeof(StringEnumConverter))]public can_be_hit config_type_enum2 { get; set; }  
 /// <summary> 表格对应枚举类型: </summary>
[JsonConverter(typeof(StringEnumConverter))]public can_be_hit config_type_enum3 { get; set; }  
 /// <summary> 表格对应枚举类型: </summary>
[JsonConverter(typeof(StringEnumConverter))]public action_type config_type_enum4 { get; set; }  
 /// <summary> 表格对应枚举类型: </summary>
[JsonConverter(typeof(StringEnumConverter))]public action_type config_type_enum5 { get; set; }  
 /// <summary> 表格对应枚举类型: </summary>
[JsonConverter(typeof(StringEnumConverter))]public action_type config_type_enum6 { get; set; }  
 /// <summary> 表格对应枚举类型: </summary>
[JsonConverter(typeof(StringEnumConverter))]public hit_media config_type_enum7 { get; set; }  
 /// <summary> 表格对应枚举类型: </summary>
[JsonConverter(typeof(StringEnumConverter))]public hit_media config_type_enum8 { get; set; }  
 /// <summary> 表格对应枚举类型: </summary>
[JsonConverter(typeof(StringEnumConverter))]public hit_media config_type_enum9 { get; set; }  
 /// <summary> 默认取出技能: </summary>
public string default_take_out_skill { get; set; }  
 /// <summary> 传送后保护时间: </summary>
public uint teleport_protect_time { get; set; }  } [Serializable] public class passive:IShowSchemeConfig,IGameConfig {  
 /// <summary> : </summary>
[JsonConverter(typeof(StringEnumConverter))]public passive_id id { get; set; }  
 /// <summary> : </summary>
public string icon { get; set; }  
 /// <summary> 不可做商店招牌: </summary>
public bool CantTitle { get; set; }  
 /// <summary> 附带被动，附带的不再计算其附带: </summary>
public string[] AddOns { get; set; }  
 /// <summary> : </summary>
public string Name { get; set; }  
 /// <summary> : </summary>
public string Describe { get; set; }  
 /// <summary> : </summary>
public string info { get; set; }  
 /// <summary> : </summary>
[JsonConverter(typeof(StringEnumConverter))]public passive_type passive_effect_type { get; set; }  
 /// <summary> : </summary>
public Gain[] recycle_money { get; set; }  
 /// <summary> : </summary>
public float[] param_values { get; set; }  } [Serializable] public class play_buff:IGameConfig {  
 /// <summary> id: </summary>
public int id { get; set; }  
 /// <summary> : </summary>
public uint LastTime { get; set; }  
 /// <summary> : </summary>
[JsonConverter(typeof(StringEnumConverter))]public play_buff_effect_type EffectType { get; set; }  
 /// <summary> : </summary>
public float EffectValue { get; set; }  
 /// <summary> : </summary>
[JsonConverter(typeof(StringEnumConverter))]public stack_mode StackMode { get; set; }  
 /// <summary> : </summary>
public bool UseStack { get; set; }  } [Serializable] public class prop:IShowSchemeConfig,IGameConfig {  
 /// <summary> : </summary>
[JsonConverter(typeof(StringEnumConverter))]public prop_id id { get; set; }  
 /// <summary> : </summary>
public string Name { get; set; }  
 /// <summary> : </summary>
public string Describe { get; set; }  
 /// <summary> : </summary>
public string Info { get; set; }  
 /// <summary> : </summary>
public string icon { get; set; }  
 /// <summary> 动作编号: </summary>
public int CommonAct { get; set; }  
 /// <summary> 消费道具: </summary>
public int PropPointCost { get; set; }  
 /// <summary> 0 普通1 被控制: </summary>
public int UseCond { get; set; }  
 /// <summary> 基础坚韧: </summary>
public int BaseTough { get; set; }  
 /// <summary> : </summary>
public Dictionary<uint,Media> LaunchTimeToEffectM { get; set; }  
 /// <summary> 机器人使用条件: </summary>
[JsonConverter(typeof(StringEnumConverter))]public bot_use_cond BotUseCondType { get; set; }  
 /// <summary> 机器人使用条件参数: </summary>
public float[] BotUseCondParam { get; set; }  
 /// <summary> 使用必须时间: </summary>
public uint PropMustTime { get; set; }  
 /// <summary> 移动速度比率: </summary>
public float MoveSpeedMulti { get; set; }  
 /// <summary> 锁定方向: </summary>
public bool LockAim { get; set; }  
 /// <summary> 叠加运动开始时间,0为没有: </summary>
public uint MoveAddStartTime { get; set; }  
 /// <summary> 叠加强制运动速度，x 轴为正前方，米每秒: </summary>
public Point[] MoveAdds { get; set; }  } [Serializable] public class push_buff:IGameConfig {  
 /// <summary> id: </summary>
public string id { get; set; }  
 /// <summary> : </summary>
public uint LastTime { get; set; }  
 /// <summary> 0：无UpForce: </summary>
public int BuffType { get; set; }  
 /// <summary> : </summary>
public float PushForce { get; set; }  
 /// <summary> 1方向 2中心: </summary>
[JsonConverter(typeof(StringEnumConverter))]public PushType PushType { get; set; }  
 /// <summary> 1方向：修正的方向 2中心：修正的中心点 留空等于不修正默认: </summary>
public Point[] FixVector { get; set; }  
 /// <summary> : </summary>
public float UpForce { get; set; }  } [Serializable] public class radar_wave:IGameConfig {  
 /// <summary> id: </summary>
[JsonConverter(typeof(StringEnumConverter))]public radar_wave_id id { get; set; }  
 /// <summary> 形状类型 1长方形 2圆形 3扇形 4线段 : </summary>
[JsonConverter(typeof(StringEnumConverter))]public raw_shape ShapeType { get; set; }  
 /// <summary> 形状参数 1:矩形（长宽）2圆形半径 3扇形（上顶点坐标）4线段（长度）: </summary>
public float[] ShapeParams { get; set; }  
 /// <summary> 本地位置: </summary>
public Point LocalPos { get; set; }  
 /// <summary> 本地旋转：度数: </summary>
public int LocalRotate { get; set; }  } [Serializable] public class rogue_game_chapter:IGameConfig {  
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
public VendorSaleGroup[] SaleUnits { get; set; }  
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
public int SmallCreepNum { get; set; }  
 /// <summary> : </summary>
public int[] EliteRandomIn { get; set; }  
 /// <summary> : </summary>
public int SmallEliteNum { get; set; }  
 /// <summary> 小地图boss随机范围: </summary>
public int[] SmallBossCreepRandIn { get; set; }  
 /// <summary> : </summary>
public int[] BigCreepRandIn { get; set; }  
 /// <summary> : </summary>
public int BigCreepNum { get; set; }  
 /// <summary> : </summary>
public int[] BigEliteRandomIn { get; set; }  
 /// <summary> : </summary>
public int BigEliteNum { get; set; }  
 /// <summary> : </summary>
public int[] BigBossCreepRandIn { get; set; }  
 /// <summary> 额外被动数量: </summary>
public int ExtraPassiveNum { get; set; }  } [Serializable] public class sale_unit:IGameConfig {  
 /// <summary> : </summary>
public int id { get; set; }  
 /// <summary> 售卖类型: </summary>
[JsonConverter(typeof(StringEnumConverter))]public sale_type SaleType { get; set; }  
 /// <summary> 限制模式: </summary>
public bool IsLimitExcept { get; set; }  
 /// <summary> 限制范围: </summary>
public string[] LimitIdRange { get; set; }  
 /// <summary> 花费: </summary>
public Gain[] Cost { get; set; }  
 /// <summary> 即时随机: </summary>
public bool IsRandomSale { get; set; }  
 /// <summary> 数量: </summary>
public int Stack { get; set; }  } [Serializable] public class self_effect:IGameConfig {  
 /// <summary> id: </summary>
[JsonConverter(typeof(StringEnumConverter))]public self_id id { get; set; }  
 /// <summary> : </summary>
public float HealMulti { get; set; }  
 /// <summary> : </summary>
public float FixMulti { get; set; }  
 /// <summary> : </summary>
public float ShieldMulti { get; set; }  
 /// <summary> : </summary>
public float ReloadMulti { get; set; }  
 /// <summary> : </summary>
public int[] AddPlayBuffs { get; set; }  } [Serializable] public class show_text:IGameConfig {  
 /// <summary> id: </summary>
public string id { get; set; }  
 /// <summary> 配置名字，another_name为关键字，道具配置依靠此翻译为道具货币id: </summary>
public string chinese { get; set; }  } [Serializable] public class skill:IGameConfig {  
 /// <summary> : </summary>
[JsonConverter(typeof(StringEnumConverter))]public skill_id id { get; set; }  
 /// <summary> 动作表现分类: </summary>
[JsonConverter(typeof(StringEnumConverter))]public op_Act OpAct { get; set; }  
 /// <summary> 属于的动作连击表现号: </summary>
public int ActStatus { get; set; }  
 /// <summary> 从属武器，通用为：unarmed: </summary>
[JsonConverter(typeof(StringEnumConverter))]public weapon_id FromWeaponId { get; set; }  
 /// <summary> 消费弹药: </summary>
public int AmmoCost { get; set; }  
 /// <summary> 发动需要武器瞄准步数: </summary>
public int SnipeStepNeed { get; set; }  
 /// <summary> 敌方攻击失败时触发的技能: </summary>
public string EnemyFailTrickSkill { get; set; }  
 /// <summary> 基础坚韧:0值会根据子弹发射最快的自动计算: </summary>
public int BaseTough { get; set; }  
 /// <summary> 在第一帧选目标的区域，空代表没有，如果选定目标，技能产生的移动会向目标调整,抓取引发的技能无效，因为技能发动时距离已设定好: </summary>
public string LockArea { get; set; }  
 /// <summary> 帧时--发射的子弹Id，时间会自动转换，从1开始，配置0帧的技能会无效: </summary>
public Dictionary<uint,string[]> LaunchTimeToBullet { get; set; }  
 /// <summary> 从开始可以控制移动的时间，0表示不可控制: </summary>
public uint CanInputMove { get; set; }  
 /// <summary> 强制移动开始时间: </summary>
public uint MoveStartTime { get; set; }  
 /// <summary> 释放音效: </summary>
public string LaunchSound { get; set; }  
 /// <summary> 延迟: </summary>
public float LaunchSoundDelay { get; set; }  
 /// <summary> 运动每帧，角色0度默认为水平向右: </summary>
public Point[] Moves { get; set; }  
 /// <summary> 瞄准状态破坏Time：0代表不打消: </summary>
public uint BreakSnipeTime { get; set; }  
 /// <summary> 技能必须帧（技能在释放完这些帧之前不能取消）: </summary>
public uint SkillMustTime { get; set; }  
 /// <summary> 下个技能可输入帧: </summary>
public uint ComboInputStartTime { get; set; }  
 /// <summary> 技能完整帧数: </summary>
public uint SkillMaxTime { get; set; }  
 /// <summary> 下个技能状态值: </summary>
public int NextCombo { get; set; }  } [Serializable] public class skill_group:IGameConfig {  
 /// <summary> : </summary>
public string id { get; set; }  
 /// <summary> 操作1：动作状态-技能: </summary>
public Dictionary<int,string> Op1 { get; set; }  
 /// <summary> 操作2：动作状态-技能: </summary>
public Dictionary<int,string> Op2 { get; set; }  
 /// <summary> 操作3：动作状态-技能: </summary>
public Dictionary<int,string> Op3 { get; set; }  
 /// <summary> Switch：动作状态-技能: </summary>
public Dictionary<int,string> Switch { get; set; }  } [Serializable] public class snipe:IGameConfig {  
 /// <summary> id: </summary>
public int id { get; set; }  
 /// <summary> 开镜音效: </summary>
public string OnSniperSound { get; set; }  
 /// <summary> 触发按住时间:0.1秒一个tick </summary>
public int TrickTick { get; set; }  
 /// <summary> 上限步数: </summary>
public int TotalStep { get; set; }  
 /// <summary> 打开速度: </summary>
public int OnTickSpeed { get; set; }  
 /// <summary> 关闭速度: </summary>
public int OffTickSpeed { get; set; }  
 /// <summary> 速度调整比率: </summary>
public float MoveMulti { get; set; }  } [Serializable] public class standard_level_up:IGameConfig {  
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
public Gain[] KillBonus { get; set; }  } [Serializable] public class summon:IGameConfig {  
 /// <summary> id: </summary>
[JsonConverter(typeof(StringEnumConverter))]public summon_id id { get; set; }  
 /// <summary> : </summary>
public string Setter { get; set; }  } [Serializable] public class talent:IGameConfig {  
 /// <summary> : </summary>
public int id { get; set; }  
 /// <summary> :被动技能Id </summary>
public string passive_id { get; set; }  
 /// <summary> : </summary>
public int activeLevel { get; set; }  
 /// <summary> : </summary>
public Cost[] activeCost { get; set; }  
 /// <summary> : </summary>
public int addLevel { get; set; }  
 /// <summary> : </summary>
public Cost addLevelBaseCost { get; set; }  
 /// <summary> : </summary>
public Cost addLevelAddCost { get; set; }  } [Serializable] public class trap:IGameConfig {  
 /// <summary> : </summary>
[JsonConverter(typeof(StringEnumConverter))]public trap_id id { get; set; }  
 /// <summary> 体型: </summary>
[JsonConverter(typeof(StringEnumConverter))]public size BodyId { get; set; }  
 /// <summary> 属性id: </summary>
public int AttrId { get; set; }  
 /// <summary> : </summary>
public string TextureName { get; set; }  
 /// <summary> : </summary>
public bool CanBeSee { get; set; }  
 /// <summary> 打击失败次数，如果打击失败则增加1:0代表没有 </summary>
public int FailChance { get; set; }  
 /// <summary> 侦察间隔时间:至少为2个tick这个是额外的 </summary>
public uint CallTrapRoundTime { get; set; }  
 /// <summary> :0代表永续 </summary>
public uint MaxLifeTime { get; set; }  
 /// <summary> 提示圈半径: </summary>
public float HintRad { get; set; }  
 /// <summary> :周期发射的Media </summary>
public Media TrapMedia { get; set; }  
 /// <summary> :通过TrapMedia触发的Media </summary>
public Media[] LauchMedia { get; set; }  
 /// <summary> :触发延迟，0保底1tick </summary>
public uint TrickDelayTime { get; set; }  
 /// <summary> :触发次数，如果 </summary>
public uint TrickStack { get; set; }  
 /// <summary> 伤害倍率:伤害倍率，这个与释放装置的角色攻击有关 </summary>
public float DamageMulti { get; set; }  } [Serializable] public class vehicle:IShowSchemeConfig,IGameConfig {  
 /// <summary> : </summary>
[JsonConverter(typeof(StringEnumConverter))]public vehicle_id id { get; set; }  
 /// <summary> : </summary>
public string icon { get; set; }  
 /// <summary> : </summary>
public string Name { get; set; }  
 /// <summary> : </summary>
public string Describe { get; set; }  
 /// <summary> 体型: </summary>
[JsonConverter(typeof(StringEnumConverter))]public size BodyId { get; set; }  
 /// <summary> 属性id: </summary>
public int AttrId { get; set; }  
 /// <summary> : </summary>
public uint DestoryDelayTime { get; set; }  
 /// <summary> : </summary>
public string DestoryBullet { get; set; }  
 /// <summary> : </summary>
public string[] Weapons { get; set; }  
 /// <summary> : </summary>
public int MaxWeaponSlot { get; set; }  
 /// <summary> : </summary>
public string OutActSkill { get; set; }  
 /// <summary> : </summary>
public Point VScope { get; set; }  } [Serializable] public class weapon:IShowSchemeConfig,IGameConfig {  
 /// <summary> id: </summary>
[JsonConverter(typeof(StringEnumConverter))]public weapon_id id { get; set; }  
 /// <summary> : </summary>
public string Name { get; set; }  
 /// <summary> : </summary>
public string Describe { get; set; }  
 /// <summary> : </summary>
public string icon { get; set; }  
 /// <summary> 近身保持距离，加上双方半径: </summary>
public float KeepDistance { get; set; }  
 /// <summary> : </summary>
public SimpleObj4[] BodySizeUseAndSnipeSpeedFix { get; set; }  
 /// <summary> : </summary>
public SimpleObj5[] BodySizeToBlockSkill { get; set; }  
 /// <summary> j机器人使用射程: </summary>
public float BotRange { get; set; }  
 /// <summary> 最大放大倍数: </summary>
public float MaxRangeMulti { get; set; }  
 /// <summary> 总节数: </summary>
public int ChangeRangeStep { get; set; }  
 /// <summary> 瞄准1id: </summary>
public int Snipe1 { get; set; }  
 /// <summary> 瞄准2id: </summary>
public int Snipe2 { get; set; }  
 /// <summary> : </summary>
public int Snipe3 { get; set; }  } [Serializable] public enum item_id{[Description("coin")]@coin,[Description("chip")]@chip,[Description("exp")]@exp,[Description("chip_2")]@chip_2,[Description("chip_4")]@chip_4,[Description("chip_3")]@chip_3,[Description("rand_show")]@rand_show,[Description("free_ticket")]@free_ticket}[Serializable] public enum ItemType{[Description("forShow")]@forShow,[Description("bag")]@bag,[Description("money")]@money,[Description("battle_exp")]@battle_exp}[Serializable] public enum can_be_hit{[Description("trap")]@trap,[Description("vehicle")]@vehicle,[Description("character")]@character}[Serializable] public enum action_type{[Description("skill")]@skill,[Description("prop")]@prop,[Description("interaction")]@interaction}[Serializable] public enum hit_media{[Description("lock_area")]@lock_area,[Description("radar_wave")]@radar_wave,[Description("bullet")]@bullet}[Serializable] public enum botOp{[Description("none")]@none,[Description("op2")]@op2,[Description("op1")]@op1}[Serializable] public enum size{[Description("default")]@default,[Description("tiny")]@tiny,[Description("medium")]@medium,[Description("small")]@small,[Description("big")]@big}[Serializable] public enum interactableType{[Description("weapon")]@weapon,[Description("vehicle")]@vehicle,[Description("default")]@default,[Description("prop")]@prop}[Serializable] public enum bullet_id{[Description("test_h_1_b_1")]@test_h_1_b_1,[Description("test_cb_l_3")]@test_cb_l_3,[Description("test_l_1_k_1")]@test_l_1_k_1,[Description("test_cb_h_1")]@test_cb_h_1,[Description("test_cb_l_2")]@test_cb_l_2,[Description("default_block_1")]@default_block_1,[Description("vehicle_boom")]@vehicle_boom,[Description("test_cb_t_2")]@test_cb_t_2,[Description("test_l_1_b_1")]@test_l_1_b_1,[Description("test_cb_t_1")]@test_cb_t_1,[Description("test_cb_l_1")]@test_cb_l_1,[Description("test_cb_h_3")]@test_cb_h_3,[Description("test_cb_t_3")]@test_cb_t_3,[Description("rocket_punch")]@rocket_punch,[Description("test_b_1_b_1")]@test_b_1_b_1,[Description("test_r_1_b_1")]@test_r_1_b_1,[Description("test_cb_h_2")]@test_cb_h_2,[Description("mine_atk")]@mine_atk}[Serializable] public enum raw_shape{[Description("round")]@round,[Description("sector")]@sector,[Description("rectangle")]@rectangle,[Description("line")]@line}[Serializable] public enum buff_type{[Description("caught_buff")]@caught_buff,[Description("push_buff")]@push_buff}[Serializable] public enum hit_type{[Description("range")]@range,[Description("melee")]@melee}[Serializable] public enum target_type{[Description("all_team")]@all_team,[Description("other_team")]@other_team}[Serializable] public enum interactionAct{[Description("kick_vehicle")]@kick_vehicle,[Description("get_info")]@get_info,[Description("pick_up_cage")]@pick_up_cage,[Description("apply")]@apply,[Description("recycle_cage")]@recycle_cage,[Description("get_in_vehicle")]@get_in_vehicle}[Serializable] public enum lock_area_id{[Description("test_h_1_lock")]@test_h_1_lock,[Description("test_l_1_lock")]@test_l_1_lock}[Serializable] public enum direction{[Description("East")]@East,[Description("West")]@West,[Description("South")]@South,[Description("North")]@North}[Serializable] public enum passive_id{[Description("shield_trans1")]@shield_trans1,[Description("light_armor1")]@light_armor1,[Description("protect_time_boost")]@protect_time_boost,[Description("ticket_add")]@ticket_add,[Description("back_stab")]@back_stab,[Description("main_atk")]@main_atk,[Description("sharded_atk")]@sharded_atk,[Description("on_break")]@on_break,[Description("reload_boost")]@reload_boost,[Description("shield_absorb")]@shield_absorb,[Description("heal_effect_boost")]@heal_effect_boost,[Description("light_armor2")]@light_armor2,[Description("absorb_up")]@absorb_up,[Description("shield_boost")]@shield_boost,[Description("charge_boost")]@charge_boost,[Description("shield_reg_boost")]@shield_reg_boost,[Description("recycle_boost")]@recycle_boost,[Description("ammo_absorb")]@ammo_absorb,[Description("fix_boost")]@fix_boost,[Description("prop_boost")]@prop_boost,[Description("max_ammo_boost")]@max_ammo_boost,[Description("light_armor")]@light_armor,[Description("energy_armor")]@energy_armor,[Description("emergency_shield")]@emergency_shield,[Description("tough_max")]@tough_max,[Description("trap_atk")]@trap_atk,[Description("hp_boost")]@hp_boost,[Description("shield_trans")]@shield_trans,[Description("armor_absorb")]@armor_absorb,[Description("speed_boost")]@speed_boost,[Description("shield_overload1")]@shield_overload1,[Description("vicious_armor1")]@vicious_armor1,[Description("sharded_atk_small")]@sharded_atk_small,[Description("shield_overload")]@shield_overload,[Description("vicious_armor")]@vicious_armor,[Description("emergency_armor")]@emergency_armor,[Description("armor_boost")]@armor_boost,[Description("sweep")]@sweep,[Description("revenge")]@revenge,[Description("defence_boost")]@defence_boost,[Description("trap_survive")]@trap_survive,[Description("hp_absorb")]@hp_absorb,[Description("other_to_armor")]@other_to_armor,[Description("other_to_shield")]@other_to_shield,[Description("life_power")]@life_power,[Description("other_to_hp")]@other_to_hp,[Description("main_atk_small")]@main_atk_small,[Description("protect_absorb")]@protect_absorb,[Description("back_stab_small")]@back_stab_small,[Description("ticket_add_5")]@ticket_add_5}[Serializable] public enum passive_type{[Description("Other")]@Other,[Description("SpecialDamageAdd")]@SpecialDamageAdd,[Description("Survive")]@Survive,[Description("Regen")]@Regen,[Description("TrapAbout")]@TrapAbout,[Description("AddItem")]@AddItem,[Description("TransRegeneration")]@TransRegeneration,[Description("HitWinBuff")]@HitWinBuff,[Description("Attack")]@Attack,[Description("TickAdd")]@TickAdd,[Description("AbsorbAdd")]@AbsorbAdd}[Serializable] public enum play_buff_effect_type{[Description("TakeDamageAdd")]@TakeDamageAdd,[Description("Break")]@Break,[Description("Tough")]@Tough,[Description("MakeDamageAdd")]@MakeDamageAdd}[Serializable] public enum stack_mode{[Description("Stack")]@Stack,[Description("OverWrite")]@OverWrite,[Description("Time")]@Time}[Serializable] public enum prop_id{[Description("battery")]@battery,[Description("trap")]@trap,[Description("ammos")]@ammos,[Description("repair")]@repair,[Description("heal")]@heal,[Description("all_reg")]@all_reg,[Description("rocket_boost")]@rocket_boost,[Description("alert")]@alert,[Description("radar")]@radar}[Serializable] public enum effect_media_type{[Description("self")]@self,[Description("radar_wave")]@radar_wave,[Description("summon")]@summon,[Description("bullet")]@bullet}[Serializable] public enum bot_use_cond{[Description("HpBlowPercent")]@HpBlowPercent,[Description("OnPatrolRandom")]@OnPatrolRandom,[Description("EnemyOnSight")]@EnemyOnSight,[Description("CantUse")]@CantUse,[Description("ShieldBlowPercent")]@ShieldBlowPercent,[Description("ArmorBlowPercent")]@ArmorBlowPercent}[Serializable] public enum PushType{[Description("Center")]@Center,[Description("Vector")]@Vector}[Serializable] public enum radar_wave_id{[Description("alert_trick_r")]@alert_trick_r,[Description("mine_trick_r")]@mine_trick_r,[Description("r1")]@r1}[Serializable] public enum sale_type{[Description("passive")]@passive,[Description("weapon")]@weapon,[Description("prop")]@prop}[Serializable] public enum self_id{[Description("c1")]@c1,[Description("f1")]@f1,[Description("h1")]@h1,[Description("r1")]@r1,[Description("a1")]@a1}[Serializable] public enum skill_id{[Description("cross_bow_block")]@cross_bow_block,[Description("test_l_1")]@test_l_1,[Description("test_cross_bow_switch")]@test_cross_bow_switch,[Description("test_cross_bow_l_1")]@test_cross_bow_l_1,[Description("test_bow_l_2")]@test_bow_l_2,[Description("test_h_1_caught")]@test_h_1_caught,[Description("test_cross_bow_h_1")]@test_cross_bow_h_1,[Description("test_l_2")]@test_l_2,[Description("gun_block")]@gun_block,[Description("bow_block")]@bow_block,[Description("test_s_1")]@test_s_1,[Description("test_l_3")]@test_l_3,[Description("test_bow_h_1")]@test_bow_h_1,[Description("test_cought_skill")]@test_cought_skill,[Description("test_r_3")]@test_r_3,[Description("default_take_out")]@default_take_out,[Description("test_r_s")]@test_r_s,[Description("test_bow_switch")]@test_bow_switch,[Description("sword_block")]@sword_block,[Description("test_h_1")]@test_h_1,[Description("test_h_3")]@test_h_3,[Description("out_vehicle")]@out_vehicle,[Description("test_bow_l_1")]@test_bow_l_1,[Description("test_r_2")]@test_r_2,[Description("test_r_1")]@test_r_1,[Description("test_h_2")]@test_h_2,[Description("test_cross_bow_h_trick")]@test_cross_bow_h_trick,[Description("test_bow_l_3")]@test_bow_l_3}[Serializable] public enum op_Act{[Description("Switch")]@Switch,[Description("CaughtTrick")]@CaughtTrick,[Description("BreakAway")]@BreakAway,[Description("Block")]@Block,[Description("Op1")]@Op1,[Description("TakeOut")]@TakeOut,[Description("Op2")]@Op2,[Description("ByTrick")]@ByTrick}[Serializable] public enum weapon_id{[Description("unarmed")]@unarmed,[Description("test_cross_bow")]@test_cross_bow,[Description("test_bow")]@test_bow,[Description("test_sword")]@test_sword,[Description("test_gun")]@test_gun}[Serializable] public enum summon_id{[Description("s_a_mine")]@s_a_mine,[Description("s_a_alert")]@s_a_alert}[Serializable] public enum trap_id{[Description("mine")]@mine,[Description("alert")]@alert}[Serializable] public enum vehicle_id{[Description("type_c")]@type_c,[Description("type_a")]@type_a,[Description("type_b")]@type_b}public static class ResNames { public static Dictionary < Type, string > NamesDictionary{get;} = new Dictionary < Type, string > { {typeof(bad_words), "bad_words_s.json"},{typeof(base_attribute), "base_attribute_s.json"},{typeof(battle_bot), "battle_bot_s.json"},{typeof(battle_npc), "battle_npc_s.json"},{typeof(body), "body_s.json"},{typeof(bot_other_config), "bot_other_config_s.json"},{typeof(bullet), "bullet_s.json"},{typeof(caught_buff), "caught_buff_s.json"},{typeof(character), "character_s.json"},{typeof(interaction), "interaction_s.json"},{typeof(item), "item_s.json"},{typeof(lock_area), "lock_area_s.json"},{typeof(map_raws), "map_raws_s.json"},{typeof(other_config), "other_config_s.json"},{typeof(passive), "passive_s.json"},{typeof(play_buff), "play_buff_s.json"},{typeof(prop), "prop_s.json"},{typeof(push_buff), "push_buff_s.json"},{typeof(radar_wave), "radar_wave_s.json"},{typeof(rogue_game_chapter), "rogue_game_chapter_s.json"},{typeof(sale_unit), "sale_unit_s.json"},{typeof(self_effect), "self_effect_s.json"},{typeof(show_text), "show_text_s.json"},{typeof(skill), "skill_s.json"},{typeof(skill_group), "skill_group_s.json"},{typeof(snipe), "snipe_s.json"},{typeof(standard_level_up), "standard_level_up_s.json"},{typeof(summon), "summon_s.json"},{typeof(talent), "talent_s.json"},{typeof(trap), "trap_s.json"},{typeof(vehicle), "vehicle_s.json"},{typeof(weapon), "weapon_s.json"}};public static string[] Names{get;} = { "bad_words","base_attribute","battle_bot","battle_npc","body","bot_other_config","bullet","caught_buff","character","interaction","item","lock_area","map_raws","other_config","passive","play_buff","prop","push_buff","radar_wave","rogue_game_chapter","sale_unit","self_effect","show_text","skill","skill_group","snipe","standard_level_up","summon","talent","trap","vehicle","weapon"};}[Serializable] public  class ConfigDictionaries { 
 /// <summary> 屏蔽词 </summary>
public  ImmutableDictionary < int,bad_words> bad_wordss { get; set; } 
 /// <summary> 基本属性 </summary>
public  ImmutableDictionary < int,base_attribute> base_attributes { get; set; } 
 /// <summary> 战斗NPC </summary>
public  ImmutableDictionary < int,battle_bot> battle_bots { get; set; } 
 /// <summary> 体型 </summary>
public  ImmutableDictionary < int,battle_npc> battle_npcs { get; set; } 
 /// <summary> 伤害子弹 </summary>
public  ImmutableDictionary < size,body> bodys { get; set; } 
 /// <summary> 抓取buff </summary>
public  ImmutableDictionary < int,bot_other_config> bot_other_configs { get; set; } 
 /// <summary> 角色 </summary>
public  ImmutableDictionary < bullet_id,bullet> bullets { get; set; } 
 /// <summary> 交互行动 </summary>
public  ImmutableDictionary < string,caught_buff> caught_buffs { get; set; } 
 /// <summary> 物品 </summary>
public  ImmutableDictionary < int,character> characters { get; set; } 
 /// <summary> 锁定媒介 </summary>
public  ImmutableDictionary < interactionAct,interaction> interactions { get; set; } 
 /// <summary> 地图 </summary>
public  ImmutableDictionary < item_id,item> items { get; set; } 
 /// <summary> 其他 </summary>
public  ImmutableDictionary < lock_area_id,lock_area> lock_areas { get; set; } 
 /// <summary> 被动属性 </summary>
public  ImmutableDictionary < int,map_raws> map_rawss { get; set; } 
 /// <summary> 普通buff </summary>
public  ImmutableDictionary < int,other_config> other_configs { get; set; } 
 /// <summary> 道具技能 </summary>
public  ImmutableDictionary < passive_id,passive> passives { get; set; } 
 /// <summary> 推动buff </summary>
public  ImmutableDictionary < int,play_buff> play_buffs { get; set; } 
 /// <summary> 雷达波媒介 </summary>
public  ImmutableDictionary < prop_id,prop> props { get; set; } 
 /// <summary> rogue游戏章节 </summary>
public  ImmutableDictionary < string,push_buff> push_buffs { get; set; } 
 /// <summary> 自作用媒介 </summary>
public  ImmutableDictionary < radar_wave_id,radar_wave> radar_waves { get; set; } 
 /// <summary> 字符串表 </summary>
public  ImmutableDictionary < int,rogue_game_chapter> rogue_game_chapters { get; set; } 
 /// <summary> 技能 </summary>
public  ImmutableDictionary < int,sale_unit> sale_units { get; set; } 
 /// <summary> 技能组 </summary>
public  ImmutableDictionary < self_id,self_effect> self_effects { get; set; } 
 /// <summary> 瞄准 </summary>
public  ImmutableDictionary < string,show_text> show_texts { get; set; } 
 /// <summary> 标准升级 </summary>
public  ImmutableDictionary < skill_id,skill> skills { get; set; } 
 /// <summary> 召唤媒介 </summary>
public  ImmutableDictionary < string,skill_group> skill_groups { get; set; } 
 /// <summary> 天赋 </summary>
public  ImmutableDictionary < int,snipe> snipes { get; set; } 
 /// <summary> 陷阱 </summary>
public  ImmutableDictionary < int,standard_level_up> standard_level_ups { get; set; } 
 /// <summary> 载具 </summary>
public  ImmutableDictionary < summon_id,summon> summons { get; set; } 
 /// <summary> 武器 </summary>
public  ImmutableDictionary < int,talent> talents { get; set; } 
 /// <summary> None </summary>
public  ImmutableDictionary < trap_id,trap> traps { get; set; } 
 /// <summary> None </summary>
public  ImmutableDictionary < vehicle_id,vehicle> vehicles { get; set; } 
 /// <summary> None </summary>
public  ImmutableDictionary < weapon_id,weapon> weapons { get; set; }public  IDictionary[] all_Immutable_dictionary  {get;set;}
#if NETCOREAPP

        public ConfigDictionaries()
        {
            LoadAllByDll();


            all_Immutable_dictionary = new IDictionary[]
            {
               bad_wordss,base_attributes,battle_bots,battle_npcs,bodys,bot_other_configs,bullets,caught_buffs,characters,interactions,items,lock_areas,map_rawss,other_configs,passives,play_buffs,props,push_buffs,radar_waves,rogue_game_chapters,sale_units,self_effects,show_texts,skills,skill_groups,snipes,standard_level_ups,summons,talents,traps,vehicles,weapons
            };
        }
#endif

        public ConfigDictionaries(string jsonPath = "")
        {
            LoadAllByJson(jsonPath);


            all_Immutable_dictionary = new IDictionary[]
            {
               bad_wordss,base_attributes,battle_bots,battle_npcs,bodys,bot_other_configs,bullets,caught_buffs,characters,interactions,items,lock_areas,map_rawss,other_configs,passives,play_buffs,props,push_buffs,radar_waves,rogue_game_chapters,sale_units,self_effects,show_texts,skills,skill_groups,snipes,standard_level_ups,summons,talents,traps,vehicles,weapons
            };
        }

        public ConfigDictionaries(Dictionary<string, string> nameToJsonString)
        {
            LoadAllByJsonString(nameToJsonString);

            all_Immutable_dictionary = new IDictionary[]
            {
                bad_wordss,base_attributes,battle_bots,battle_npcs,bodys,bot_other_configs,bullets,caught_buffs,characters,interactions,items,lock_areas,map_rawss,other_configs,passives,play_buffs,props,push_buffs,radar_waves,rogue_game_chapters,sale_units,self_effects,show_texts,skills,skill_groups,snipes,standard_level_ups,summons,talents,traps,vehicles,weapons
            };
        }

#if NETCOREAPP
public  void LoadAllByDll(){bad_wordss = GameConfigTools.GenConfigDict <int,bad_words> ();base_attributes = GameConfigTools.GenConfigDict <int,base_attribute> ();battle_bots = GameConfigTools.GenConfigDict <int,battle_bot> ();battle_npcs = GameConfigTools.GenConfigDict <int,battle_npc> ();bodys = GameConfigTools.GenConfigDict <size,body> ();bot_other_configs = GameConfigTools.GenConfigDict <int,bot_other_config> ();bullets = GameConfigTools.GenConfigDict <bullet_id,bullet> ();caught_buffs = GameConfigTools.GenConfigDict <string,caught_buff> ();characters = GameConfigTools.GenConfigDict <int,character> ();interactions = GameConfigTools.GenConfigDict <interactionAct,interaction> ();items = GameConfigTools.GenConfigDict <item_id,item> ();lock_areas = GameConfigTools.GenConfigDict <lock_area_id,lock_area> ();map_rawss = GameConfigTools.GenConfigDict <int,map_raws> ();other_configs = GameConfigTools.GenConfigDict <int,other_config> ();passives = GameConfigTools.GenConfigDict <passive_id,passive> ();play_buffs = GameConfigTools.GenConfigDict <int,play_buff> ();props = GameConfigTools.GenConfigDict <prop_id,prop> ();push_buffs = GameConfigTools.GenConfigDict <string,push_buff> ();radar_waves = GameConfigTools.GenConfigDict <radar_wave_id,radar_wave> ();rogue_game_chapters = GameConfigTools.GenConfigDict <int,rogue_game_chapter> ();sale_units = GameConfigTools.GenConfigDict <int,sale_unit> ();self_effects = GameConfigTools.GenConfigDict <self_id,self_effect> ();show_texts = GameConfigTools.GenConfigDict <string,show_text> ();skills = GameConfigTools.GenConfigDict <skill_id,skill> ();skill_groups = GameConfigTools.GenConfigDict <string,skill_group> ();snipes = GameConfigTools.GenConfigDict <int,snipe> ();standard_level_ups = GameConfigTools.GenConfigDict <int,standard_level_up> ();summons = GameConfigTools.GenConfigDict <summon_id,summon> ();talents = GameConfigTools.GenConfigDict <int,talent> ();traps = GameConfigTools.GenConfigDict <trap_id,trap> ();vehicles = GameConfigTools.GenConfigDict <vehicle_id,vehicle> ();weapons = GameConfigTools.GenConfigDict <weapon_id,weapon> ();}
#endif
public  void LoadAllByJson(string path = ""){bad_wordss = GameConfigTools.GenConfigDictByJsonFile <int,bad_words> (path);base_attributes = GameConfigTools.GenConfigDictByJsonFile <int,base_attribute> (path);battle_bots = GameConfigTools.GenConfigDictByJsonFile <int,battle_bot> (path);battle_npcs = GameConfigTools.GenConfigDictByJsonFile <int,battle_npc> (path);bodys = GameConfigTools.GenConfigDictByJsonFile <size,body> (path);bot_other_configs = GameConfigTools.GenConfigDictByJsonFile <int,bot_other_config> (path);bullets = GameConfigTools.GenConfigDictByJsonFile <bullet_id,bullet> (path);caught_buffs = GameConfigTools.GenConfigDictByJsonFile <string,caught_buff> (path);characters = GameConfigTools.GenConfigDictByJsonFile <int,character> (path);interactions = GameConfigTools.GenConfigDictByJsonFile <interactionAct,interaction> (path);items = GameConfigTools.GenConfigDictByJsonFile <item_id,item> (path);lock_areas = GameConfigTools.GenConfigDictByJsonFile <lock_area_id,lock_area> (path);map_rawss = GameConfigTools.GenConfigDictByJsonFile <int,map_raws> (path);other_configs = GameConfigTools.GenConfigDictByJsonFile <int,other_config> (path);passives = GameConfigTools.GenConfigDictByJsonFile <passive_id,passive> (path);play_buffs = GameConfigTools.GenConfigDictByJsonFile <int,play_buff> (path);props = GameConfigTools.GenConfigDictByJsonFile <prop_id,prop> (path);push_buffs = GameConfigTools.GenConfigDictByJsonFile <string,push_buff> (path);radar_waves = GameConfigTools.GenConfigDictByJsonFile <radar_wave_id,radar_wave> (path);rogue_game_chapters = GameConfigTools.GenConfigDictByJsonFile <int,rogue_game_chapter> (path);sale_units = GameConfigTools.GenConfigDictByJsonFile <int,sale_unit> (path);self_effects = GameConfigTools.GenConfigDictByJsonFile <self_id,self_effect> (path);show_texts = GameConfigTools.GenConfigDictByJsonFile <string,show_text> (path);skills = GameConfigTools.GenConfigDictByJsonFile <skill_id,skill> (path);skill_groups = GameConfigTools.GenConfigDictByJsonFile <string,skill_group> (path);snipes = GameConfigTools.GenConfigDictByJsonFile <int,snipe> (path);standard_level_ups = GameConfigTools.GenConfigDictByJsonFile <int,standard_level_up> (path);summons = GameConfigTools.GenConfigDictByJsonFile <summon_id,summon> (path);talents = GameConfigTools.GenConfigDictByJsonFile <int,talent> (path);traps = GameConfigTools.GenConfigDictByJsonFile <trap_id,trap> (path);vehicles = GameConfigTools.GenConfigDictByJsonFile <vehicle_id,vehicle> (path);weapons = GameConfigTools.GenConfigDictByJsonFile <weapon_id,weapon> (path);}public void LoadAllByJsonString(Dictionary<string,string> nameToJsonString ){bad_wordss = GameConfigTools.GenConfigDictByJsonString <int,bad_words> (nameToJsonString["bad_words"] );base_attributes = GameConfigTools.GenConfigDictByJsonString <int,base_attribute> (nameToJsonString["base_attribute"] );battle_bots = GameConfigTools.GenConfigDictByJsonString <int,battle_bot> (nameToJsonString["battle_bot"] );battle_npcs = GameConfigTools.GenConfigDictByJsonString <int,battle_npc> (nameToJsonString["battle_npc"] );bodys = GameConfigTools.GenConfigDictByJsonString <size,body> (nameToJsonString["body"] );bot_other_configs = GameConfigTools.GenConfigDictByJsonString <int,bot_other_config> (nameToJsonString["bot_other_config"] );bullets = GameConfigTools.GenConfigDictByJsonString <bullet_id,bullet> (nameToJsonString["bullet"] );caught_buffs = GameConfigTools.GenConfigDictByJsonString <string,caught_buff> (nameToJsonString["caught_buff"] );characters = GameConfigTools.GenConfigDictByJsonString <int,character> (nameToJsonString["character"] );interactions = GameConfigTools.GenConfigDictByJsonString <interactionAct,interaction> (nameToJsonString["interaction"] );items = GameConfigTools.GenConfigDictByJsonString <item_id,item> (nameToJsonString["item"] );lock_areas = GameConfigTools.GenConfigDictByJsonString <lock_area_id,lock_area> (nameToJsonString["lock_area"] );map_rawss = GameConfigTools.GenConfigDictByJsonString <int,map_raws> (nameToJsonString["map_raws"] );other_configs = GameConfigTools.GenConfigDictByJsonString <int,other_config> (nameToJsonString["other_config"] );passives = GameConfigTools.GenConfigDictByJsonString <passive_id,passive> (nameToJsonString["passive"] );play_buffs = GameConfigTools.GenConfigDictByJsonString <int,play_buff> (nameToJsonString["play_buff"] );props = GameConfigTools.GenConfigDictByJsonString <prop_id,prop> (nameToJsonString["prop"] );push_buffs = GameConfigTools.GenConfigDictByJsonString <string,push_buff> (nameToJsonString["push_buff"] );radar_waves = GameConfigTools.GenConfigDictByJsonString <radar_wave_id,radar_wave> (nameToJsonString["radar_wave"] );rogue_game_chapters = GameConfigTools.GenConfigDictByJsonString <int,rogue_game_chapter> (nameToJsonString["rogue_game_chapter"] );sale_units = GameConfigTools.GenConfigDictByJsonString <int,sale_unit> (nameToJsonString["sale_unit"] );self_effects = GameConfigTools.GenConfigDictByJsonString <self_id,self_effect> (nameToJsonString["self_effect"] );show_texts = GameConfigTools.GenConfigDictByJsonString <string,show_text> (nameToJsonString["show_text"] );skills = GameConfigTools.GenConfigDictByJsonString <skill_id,skill> (nameToJsonString["skill"] );skill_groups = GameConfigTools.GenConfigDictByJsonString <string,skill_group> (nameToJsonString["skill_group"] );snipes = GameConfigTools.GenConfigDictByJsonString <int,snipe> (nameToJsonString["snipe"] );standard_level_ups = GameConfigTools.GenConfigDictByJsonString <int,standard_level_up> (nameToJsonString["standard_level_up"] );summons = GameConfigTools.GenConfigDictByJsonString <summon_id,summon> (nameToJsonString["summon"] );talents = GameConfigTools.GenConfigDictByJsonString <int,talent> (nameToJsonString["talent"] );traps = GameConfigTools.GenConfigDictByJsonString <trap_id,trap> (nameToJsonString["trap"] );vehicles = GameConfigTools.GenConfigDictByJsonString <vehicle_id,vehicle> (nameToJsonString["vehicle"] );weapons = GameConfigTools.GenConfigDictByJsonString <weapon_id,weapon> (nameToJsonString["weapon"] );}}[Serializable] public class SimpleObj1:IGameConfig { public int weight { get; set; } [JsonConverter(typeof(StringEnumConverter))]public botOp op { get; set; }  } [Serializable] public class SimpleObj2:IGameConfig { public uint item1 { get; set; } public uint item2 { get; set; }  } [Serializable] public class Gain:IGameConfig { public string item { get; set; } public int num { get; set; }  } [Serializable] public class Point:IGameConfig { public float x { get; set; } public float y { get; set; }  } [Serializable] public class Buff:IGameConfig { [JsonConverter(typeof(StringEnumConverter))]public size size { get; set; } [JsonConverter(typeof(StringEnumConverter))]public buff_type buff_type { get; set; } public string buff_id { get; set; }  } [Serializable] public class SimpleObj3:IGameConfig { public uint key_time { get; set; } public Point key_point { get; set; }  } [Serializable] public class TelPoint:IGameConfig { [JsonConverter(typeof(StringEnumConverter))]public direction Direction { get; set; } public Point[] Teleport { get; set; }  } [Serializable] public class Media:IGameConfig { [JsonConverter(typeof(StringEnumConverter))]public effect_media_type media_type { get; set; } public string e_id { get; set; }  } [Serializable] public class VendorSaleGroup:IGameConfig { public int Num { get; set; } public int[] Range { get; set; }  } [Serializable] public class Cost:IGameConfig { public string item { get; set; } public int num { get; set; } public int first_use_pay { get; set; }  } [Serializable] public class SimpleObj4:IGameConfig { [JsonConverter(typeof(StringEnumConverter))]public size body { get; set; } public float snipe_speed_fix { get; set; } public string skill_group { get; set; }  } [Serializable] public class SimpleObj5:IGameConfig { [JsonConverter(typeof(StringEnumConverter))]public size body { get; set; } public string blockSkill { get; set; }  } }