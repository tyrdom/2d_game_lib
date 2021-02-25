using System;using System.Collections;using System.Collections.Generic;using System.Collections.Immutable;using Newtonsoft.Json;using Newtonsoft.Json.Converters;namespace game_config {[Serializable] public class skill:IGameConfig { public string id { get; set; } public int AmmoCost { get; set; } public int SnipeStepNeed { get; set; } public int BaseTough { get; set; } public string LockArea { get; set; } public Dictionary<float,string> LaunchTimeToBullet { get; set; } public float CanInputMove { get; set; } public float MoveStartTime { get; set; } public Point[] Moves { get; set; } public float BreakSnipeTime { get; set; } public float SkillMustTime { get; set; } public float ComboInputStartTime { get; set; } public float SkillMaxTime { get; set; } public int NextCombo { get; set; }  } [Serializable] public class bad_words:IGameConfig { public int id { get; set; } public string name { get; set; }  } [Serializable] public class caught_buff:IGameConfig { public string id { get; set; } public float LastTime { get; set; } public SimpleObj1[] CatchKeyPoints { get; set; } public string TrickSkill { get; set; }  } [Serializable] public class prop:IGameConfig { public int id { get; set; } public string Info { get; set; } public int PropPointCost { get; set; } public int UseCond { get; set; } public int BaseTough { get; set; } public Dictionary<float,Media> LaunchTimeToEffectM { get; set; } [JsonConverter(typeof(StringEnumConverter))]public bot_use_cond BotUseCondType { get; set; } public float[] BotUseCondParam { get; set; } public float PropMustTime { get; set; } public float MoveSpeedMulti { get; set; } public bool LockAim { get; set; } public float MoveAddStartTime { get; set; } public Point[] MoveAdds { get; set; }  } [Serializable] public class talent:IGameConfig { public int id { get; set; } public int passive_id { get; set; } public int activeLevel { get; set; } public Cost[] activeCost { get; set; } public int addLevel { get; set; } public Cost addLevelBaseCost { get; set; } public Cost addLevelAddCost { get; set; }  } [Serializable] public class other_config:IGameConfig { public int id { get; set; } public float max_hegiht { get; set; } public float g_acc { get; set; } public float friction { get; set; } public int tough_grow { get; set; } public int mid_tough { get; set; } public int weapon_num { get; set; } public float two_s_to_see_pertick { get; set; } public float two_s_to_see_pertick_medium_vehicle { get; set; } public int qspace_max_per_level { get; set; } public float hit_wall_add_time_by_speed_param { get; set; } public float hit_wall_dmg_param { get; set; } public float hit_wall_catch_time_param { get; set; } public float hit_wall_catch_dmg_param { get; set; } public float sight_length { get; set; } public float sight_width { get; set; } public string common_fail_antibuff { get; set; } public float tick_time { get; set; } public float interaction_act2_call_time { get; set; } public int trick_protect_value { get; set; } public float protect_time { get; set; } public int standard_max_prop_stack { get; set; } public int standard_recycle_prop_stack { get; set; } public float prop_R { get; set; } public float weapon_R { get; set; } public float pass_R { get; set; } public float saleBox_R { get; set; } public int up_trap_max { get; set; } public float rogue_reborn_limit_time { get; set; } public float ShardedAttackMulti { get; set; } public float DecreaseMinCos { get; set; } public float NormalSpeedMinCos { get; set; } public float MoveDecreaseMinMulti { get; set; } public int atkPassBuffId { get; set; } public int defPassBuffId { get; set; } public int[] RogueChapters { get; set; } public Gain[] rogue_reborn_cost { get; set; } public float rogueRebornCountDownTime { get; set; } public float rogueGameCheckTickTime { get; set; }  } [Serializable] public class trap:IGameConfig { public string id { get; set; } [JsonConverter(typeof(StringEnumConverter))]public size BodyId { get; set; } public int AttrId { get; set; } public bool CanBeSee { get; set; } public int FailChance { get; set; } public float CallTrapRoundTime { get; set; } public float MaxLifeTime { get; set; } public Media TrapMedia { get; set; } public Media[] LauchMedia { get; set; } public float TrickDelayTime { get; set; } public uint TrickStack { get; set; } public float DamageMulti { get; set; }  } [Serializable] public class radar_wave:IGameConfig { public string id { get; set; } [JsonConverter(typeof(StringEnumConverter))]public raw_shape ShapeType { get; set; } public float[] ShapeParams { get; set; } public Point LocalPos { get; set; } public int LocalRotate { get; set; }  } [Serializable] public class standard_level_up:IGameConfig { public int id { get; set; } public int next_exp { get; set; } public float RebornTime { get; set; } public int[] UpPassiveGet { get; set; } public Gain[] FastRebornCost { get; set; } public Gain[] TeamBonus { get; set; } public Gain[] KillBonus { get; set; }  } [Serializable] public class snipe:IGameConfig { public int id { get; set; } public int TrickTick { get; set; } public int TotalStep { get; set; } public int OnTickSpeed { get; set; } public int OffTickSpeed { get; set; } public float MoveMulti { get; set; }  } [Serializable] public class body:IGameConfig { [JsonConverter(typeof(StringEnumConverter))]public size id { get; set; } public float mass { get; set; } public float rad { get; set; }  } [Serializable] public class skill_group:IGameConfig { public string id { get; set; } public Dictionary<int,string> Op1 { get; set; } public Dictionary<int,string> Op2 { get; set; } public Dictionary<int,string> Op3 { get; set; } public Dictionary<int,string> Switch { get; set; }  } [Serializable] public class interaction:IGameConfig { [JsonConverter(typeof(StringEnumConverter))]public interactionAct id { get; set; } public int BaseTough { get; set; } public float TotalTime { get; set; }  } [Serializable] public class show_text:IGameConfig { public string id { get; set; } public string chinese { get; set; }  } [Serializable] public class bullet:IGameConfig { public string id { get; set; } [JsonConverter(typeof(StringEnumConverter))]public raw_shape ShapeType { get; set; } public float[] ShapeParams { get; set; } public Point LocalPos { get; set; } public int LocalRotate { get; set; } public Buff[] SuccessAntiActBuffConfigToOpponent { get; set; } public bool CanOverBulletBlock { get; set; } [JsonConverter(typeof(StringEnumConverter))]public hit_type HitType { get; set; } public Buff[] FailActBuffConfigToSelf { get; set; } public int PauseToCaster { get; set; } public int PauseToOpponent { get; set; } [JsonConverter(typeof(StringEnumConverter))]public target_type TargetType { get; set; } public int Tough { get; set; } public bool IsHAtk { get; set; } public int ProtectValue { get; set; } public int SuccessAmmoAdd { get; set; } public float DamageMulti { get; set; }  } [Serializable] public class character:IGameConfig { public int id { get; set; } public string info { get; set; } [JsonConverter(typeof(StringEnumConverter))]public size BodyId { get; set; } public int AttrId { get; set; } public int[] Weapons { get; set; } public int MaxWeaponSlot { get; set; }  } [Serializable] public class push_buff:IGameConfig { public string id { get; set; } public float LastTime { get; set; } public int BuffType { get; set; } public float PushForce { get; set; } [JsonConverter(typeof(StringEnumConverter))]public PushType PushType { get; set; } public Point[] FixVector { get; set; } public float UpForce { get; set; }  } [Serializable] public class map_raws:IGameConfig { public int id { get; set; } public string info { get; set; } public Point[][] WalkRawMap { get; set; } public Point[][] SightRawMap { get; set; } public Point[][] BulletRawMap { get; set; } public Point[][] StartPoints { get; set; } public TelPoint[] TransPoint { get; set; }  } [Serializable] public class summon:IGameConfig { public string id { get; set; } public string Setter { get; set; }  } [Serializable] public class rogue_game_chapter:IGameConfig { public int id { get; set; } public int[] SmallMapResRandIn { get; set; } public int[] StartRandIn { get; set; } public int[] BigMapResRandIn { get; set; } public int[] BossMapRandIn { get; set; } public int[] HangarMapResRandIn { get; set; } public int[] VendorMapResRandIn { get; set; } public int SmallMap { get; set; } public int BigMap { get; set; } public int VendorMap { get; set; } public int VendorMapStart { get; set; } public int VendorMapRange { get; set; } public int HangarMap { get; set; } public int HangarMapStart { get; set; } public int HangarMapRange { get; set; } public bool StartWithBig { get; set; } public bool EndWithBig { get; set; } public int[] CreepRandIn { get; set; } public int[] EliteRandomIn { get; set; } public int SmallCreepNum { get; set; } public int SmallEliteNum { get; set; } public int[] SmallBossCreepRandIn { get; set; } public int BigCreepNum { get; set; } public int BigEliteNum { get; set; } public int[] BigBossCreepRandIn { get; set; }  } [Serializable] public class battle_npc:IGameConfig { public int id { get; set; } public string info { get; set; } [JsonConverter(typeof(StringEnumConverter))]public size BodyId { get; set; } public int AttrId { get; set; } public int[] Weapons { get; set; } public int MaxWeaponSlot { get; set; } public int[] PropIds { get; set; } public int PropPoint { get; set; } public Gain[] AllDrops { get; set; } public Gain[] KillDrops { get; set; } public int WithVehicleId { get; set; } [JsonConverter(typeof(StringEnumConverter))]public interactableType DropMapInteractableType { get; set; } public int DropMapInteractableId { get; set; } public int[] PassiveRange { get; set; } public int PassiveNum { get; set; } public SimpleObj2[] ActWeight { get; set; } public SimpleObj3 DoNotMinMaxTime { get; set; } public float ActShowDelayTime { get; set; } public int MaxCombo { get; set; }  } [Serializable] public class vehicle:IGameConfig { public int id { get; set; } [JsonConverter(typeof(StringEnumConverter))]public size BodyId { get; set; } public int AttrId { get; set; } public float DestoryDelayTime { get; set; } public string DestoryBullet { get; set; } public int[] Weapons { get; set; } public int MaxWeaponSlot { get; set; } public string OutActSkill { get; set; } public Point VScope { get; set; }  } [Serializable] public class play_buff:IGameConfig { public int id { get; set; } public float LastTime { get; set; } [JsonConverter(typeof(StringEnumConverter))]public play_buff_effect_type EffectType { get; set; } public float EffectValue { get; set; } [JsonConverter(typeof(StringEnumConverter))]public stack_mode StackMode { get; set; } public bool UseStack { get; set; }  } [Serializable] public class self_effect:IGameConfig { public string id { get; set; } public float HealMulti { get; set; } public float FixMulti { get; set; } public float ShieldMulti { get; set; } public float ReloadMulti { get; set; } public int[] AddPlayBuffs { get; set; }  } [Serializable] public class lock_area:IGameConfig { public string id { get; set; } [JsonConverter(typeof(StringEnumConverter))]public raw_shape ShapeType { get; set; } public float[] ShapeParams { get; set; } public Point LocalPos { get; set; } public int LocalRotate { get; set; }  } [Serializable] public class item:IGameConfig { public int id { get; set; } public string Name { get; set; } public string another_name { get; set; } public string Icon { get; set; } [JsonConverter(typeof(StringEnumConverter))]public ItemType ItemType { get; set; } public bool IsPlayingItem { get; set; } public bool IsEndClear { get; set; } public uint MaxStack { get; set; } public int ShowType { get; set; }  } [Serializable] public class weapon:IGameConfig { public int id { get; set; } public SimpleObj4[] BodySizeUseAndSnipeSpeedFix { get; set; } public float BotRange { get; set; } public float MaxRangeMulti { get; set; } public int ChangeRangeStep { get; set; } public int Snipe1 { get; set; } public int Snipe2 { get; set; } public int Snipe3 { get; set; }  } [Serializable] public class passive:IGameConfig { public int id { get; set; } public string info { get; set; } [JsonConverter(typeof(StringEnumConverter))]public passive_type passive_effect_type { get; set; } public Gain[] recycle_money { get; set; } public float[] param_values { get; set; }  } [Serializable] public class base_attribute:IGameConfig { public int id { get; set; } public string info { get; set; } public float MoveMaxSpeed { get; set; } public float MoveAddSpeed { get; set; } public float MoveMinSpeed { get; set; } public float RecycleMulti { get; set; } public uint Atk { get; set; } public uint ShardedNum { get; set; } public int MaxAmmo { get; set; } public float ReloadMulti { get; set; } public float BackStabAdd { get; set; } public uint MaxHP { get; set; } public float HealEffect { get; set; } public uint MaxArmor { get; set; } public uint ArmorDefence { get; set; } public float ArmorFixEffect { get; set; } public uint MaxShield { get; set; } public float ShieldDelayTime { get; set; } public float ShieldChargeEffect { get; set; } public uint ShieldRecover { get; set; } public uint ShieldInstability { get; set; } public uint MaxTrapNum { get; set; } public float TrapAtkMulti { get; set; } public float TrapSurvivalMulti { get; set; } public float HPAbsorb { get; set; } public float ArmorAbsorb { get; set; } public float ShieldAbsorb { get; set; } public float AmmoAbsorb { get; set; } public float ProtectAbsorb { get; set; }  } [Serializable] public enum ItemType{@battle_exp,@money,@bag}[Serializable] public enum effect_media_type{@summon,@radar_wave,@bullet,@self}[Serializable] public enum bot_use_cond{@CantUse,@ArmorBlowPercent,@ShieldBlowPercent,@HpBlowPercent,@OnPatrolRandom,@EnemyOnSight}[Serializable] public enum size{@medium,@default,@small,@big,@tiny}[Serializable] public enum raw_shape{@sector,@rectangle,@round}[Serializable] public enum interactionAct{@get_in_vehicle,@kick_vehicle,@pick_up_cage,@get_info,@recycle_cage,@apply}[Serializable] public enum buff_type{@push_buff,@caught_buff}[Serializable] public enum hit_type{@range,@melee}[Serializable] public enum target_type{@other_team,@all_team}[Serializable] public enum PushType{@Center,@Vector}[Serializable] public enum direction{@North,@West,@South,@East}[Serializable] public enum interactableType{@weapon,@prop,@vehicle}[Serializable] public enum botOp{@none,@op2,@op1}[Serializable] public enum play_buff_effect_type{@TakeDamageAdd,@Break,@MakeDamageAdd,@Tough}[Serializable] public enum stack_mode{@Stack,@OverWrite,@Time}[Serializable] public enum passive_type{@AbsorbAdd,@Survive,@Attack,@TrapAbout,@HitWinBuff,@AddItem,@TickAdd,@Regen,@Other}public static class ResNames { public static Dictionary < Type, string > NamesDictionary{get;} = new Dictionary < Type, string > { {typeof(skill), "skill_s.json"},{typeof(bad_words), "bad_words_s.json"},{typeof(caught_buff), "caught_buff_s.json"},{typeof(prop), "prop_s.json"},{typeof(talent), "talent_s.json"},{typeof(other_config), "other_config_s.json"},{typeof(trap), "trap_s.json"},{typeof(radar_wave), "radar_wave_s.json"},{typeof(standard_level_up), "standard_level_up_s.json"},{typeof(snipe), "snipe_s.json"},{typeof(body), "body_s.json"},{typeof(skill_group), "skill_group_s.json"},{typeof(interaction), "interaction_s.json"},{typeof(show_text), "show_text_s.json"},{typeof(bullet), "bullet_s.json"},{typeof(character), "character_s.json"},{typeof(push_buff), "push_buff_s.json"},{typeof(map_raws), "map_raws_s.json"},{typeof(summon), "summon_s.json"},{typeof(rogue_game_chapter), "rogue_game_chapter_s.json"},{typeof(battle_npc), "battle_npc_s.json"},{typeof(vehicle), "vehicle_s.json"},{typeof(play_buff), "play_buff_s.json"},{typeof(self_effect), "self_effect_s.json"},{typeof(lock_area), "lock_area_s.json"},{typeof(item), "item_s.json"},{typeof(weapon), "weapon_s.json"},{typeof(passive), "passive_s.json"},{typeof(base_attribute), "base_attribute_s.json"}};public static string[] Names{get;} = { "skill","bad_words","caught_buff","prop","talent","other_config","trap","radar_wave","standard_level_up","snipe","body","skill_group","interaction","show_text","bullet","character","push_buff","map_raws","summon","rogue_game_chapter","battle_npc","vehicle","play_buff","self_effect","lock_area","item","weapon","passive","base_attribute"};}[Serializable] public  class ConfigDictionaries {public  ImmutableDictionary < string,skill> skills { get; set; }public  ImmutableDictionary < int,bad_words> bad_wordss { get; set; }public  ImmutableDictionary < string,caught_buff> caught_buffs { get; set; }public  ImmutableDictionary < int,prop> props { get; set; }public  ImmutableDictionary < int,talent> talents { get; set; }public  ImmutableDictionary < int,other_config> other_configs { get; set; }public  ImmutableDictionary < string,trap> traps { get; set; }public  ImmutableDictionary < string,radar_wave> radar_waves { get; set; }public  ImmutableDictionary < int,standard_level_up> standard_level_ups { get; set; }public  ImmutableDictionary < int,snipe> snipes { get; set; }public  ImmutableDictionary < size,body> bodys { get; set; }public  ImmutableDictionary < string,skill_group> skill_groups { get; set; }public  ImmutableDictionary < interactionAct,interaction> interactions { get; set; }public  ImmutableDictionary < string,show_text> show_texts { get; set; }public  ImmutableDictionary < string,bullet> bullets { get; set; }public  ImmutableDictionary < int,character> characters { get; set; }public  ImmutableDictionary < string,push_buff> push_buffs { get; set; }public  ImmutableDictionary < int,map_raws> map_rawss { get; set; }public  ImmutableDictionary < string,summon> summons { get; set; }public  ImmutableDictionary < int,rogue_game_chapter> rogue_game_chapters { get; set; }public  ImmutableDictionary < int,battle_npc> battle_npcs { get; set; }public  ImmutableDictionary < int,vehicle> vehicles { get; set; }public  ImmutableDictionary < int,play_buff> play_buffs { get; set; }public  ImmutableDictionary < string,self_effect> self_effects { get; set; }public  ImmutableDictionary < string,lock_area> lock_areas { get; set; }public  ImmutableDictionary < int,item> items { get; set; }public  ImmutableDictionary < int,weapon> weapons { get; set; }public  ImmutableDictionary < int,passive> passives { get; set; }public  ImmutableDictionary < int,base_attribute> base_attributes { get; set; }public  IDictionary[] all_Immutable_dictionary  {get;set;}
#if NETCOREAPP

        public ConfigDictionaries()
        {
            LoadAllByDll();


            all_Immutable_dictionary = new IDictionary[]
            {
               skills,bad_wordss,caught_buffs,props,talents,other_configs,traps,radar_waves,standard_level_ups,snipes,bodys,skill_groups,interactions,show_texts,bullets,characters,push_buffs,map_rawss,summons,rogue_game_chapters,battle_npcs,vehicles,play_buffs,self_effects,lock_areas,items,weapons,passives,base_attributes
            };
        }
#endif

        public ConfigDictionaries(string jsonPath = "")
        {
            LoadAllByJson(jsonPath);


            all_Immutable_dictionary = new IDictionary[]
            {
               skills,bad_wordss,caught_buffs,props,talents,other_configs,traps,radar_waves,standard_level_ups,snipes,bodys,skill_groups,interactions,show_texts,bullets,characters,push_buffs,map_rawss,summons,rogue_game_chapters,battle_npcs,vehicles,play_buffs,self_effects,lock_areas,items,weapons,passives,base_attributes
            };
        }

        public ConfigDictionaries(Dictionary<string, string> nameToJsonString)
        {
            LoadAllByJsonString(nameToJsonString);

            all_Immutable_dictionary = new IDictionary[]
            {
                skills,bad_wordss,caught_buffs,props,talents,other_configs,traps,radar_waves,standard_level_ups,snipes,bodys,skill_groups,interactions,show_texts,bullets,characters,push_buffs,map_rawss,summons,rogue_game_chapters,battle_npcs,vehicles,play_buffs,self_effects,lock_areas,items,weapons,passives,base_attributes
            };
        }

#if NETCOREAPP
public  void LoadAllByDll(){skills = GameConfigTools.GenConfigDict <string,skill> ();bad_wordss = GameConfigTools.GenConfigDict <int,bad_words> ();caught_buffs = GameConfigTools.GenConfigDict <string,caught_buff> ();props = GameConfigTools.GenConfigDict <int,prop> ();talents = GameConfigTools.GenConfigDict <int,talent> ();other_configs = GameConfigTools.GenConfigDict <int,other_config> ();traps = GameConfigTools.GenConfigDict <string,trap> ();radar_waves = GameConfigTools.GenConfigDict <string,radar_wave> ();standard_level_ups = GameConfigTools.GenConfigDict <int,standard_level_up> ();snipes = GameConfigTools.GenConfigDict <int,snipe> ();bodys = GameConfigTools.GenConfigDict <size,body> ();skill_groups = GameConfigTools.GenConfigDict <string,skill_group> ();interactions = GameConfigTools.GenConfigDict <interactionAct,interaction> ();show_texts = GameConfigTools.GenConfigDict <string,show_text> ();bullets = GameConfigTools.GenConfigDict <string,bullet> ();characters = GameConfigTools.GenConfigDict <int,character> ();push_buffs = GameConfigTools.GenConfigDict <string,push_buff> ();map_rawss = GameConfigTools.GenConfigDict <int,map_raws> ();summons = GameConfigTools.GenConfigDict <string,summon> ();rogue_game_chapters = GameConfigTools.GenConfigDict <int,rogue_game_chapter> ();battle_npcs = GameConfigTools.GenConfigDict <int,battle_npc> ();vehicles = GameConfigTools.GenConfigDict <int,vehicle> ();play_buffs = GameConfigTools.GenConfigDict <int,play_buff> ();self_effects = GameConfigTools.GenConfigDict <string,self_effect> ();lock_areas = GameConfigTools.GenConfigDict <string,lock_area> ();items = GameConfigTools.GenConfigDict <int,item> ();weapons = GameConfigTools.GenConfigDict <int,weapon> ();passives = GameConfigTools.GenConfigDict <int,passive> ();base_attributes = GameConfigTools.GenConfigDict <int,base_attribute> ();}
#endif
public  void LoadAllByJson(string path = ""){skills = GameConfigTools.GenConfigDictByJsonFile <string,skill> (path);bad_wordss = GameConfigTools.GenConfigDictByJsonFile <int,bad_words> (path);caught_buffs = GameConfigTools.GenConfigDictByJsonFile <string,caught_buff> (path);props = GameConfigTools.GenConfigDictByJsonFile <int,prop> (path);talents = GameConfigTools.GenConfigDictByJsonFile <int,talent> (path);other_configs = GameConfigTools.GenConfigDictByJsonFile <int,other_config> (path);traps = GameConfigTools.GenConfigDictByJsonFile <string,trap> (path);radar_waves = GameConfigTools.GenConfigDictByJsonFile <string,radar_wave> (path);standard_level_ups = GameConfigTools.GenConfigDictByJsonFile <int,standard_level_up> (path);snipes = GameConfigTools.GenConfigDictByJsonFile <int,snipe> (path);bodys = GameConfigTools.GenConfigDictByJsonFile <size,body> (path);skill_groups = GameConfigTools.GenConfigDictByJsonFile <string,skill_group> (path);interactions = GameConfigTools.GenConfigDictByJsonFile <interactionAct,interaction> (path);show_texts = GameConfigTools.GenConfigDictByJsonFile <string,show_text> (path);bullets = GameConfigTools.GenConfigDictByJsonFile <string,bullet> (path);characters = GameConfigTools.GenConfigDictByJsonFile <int,character> (path);push_buffs = GameConfigTools.GenConfigDictByJsonFile <string,push_buff> (path);map_rawss = GameConfigTools.GenConfigDictByJsonFile <int,map_raws> (path);summons = GameConfigTools.GenConfigDictByJsonFile <string,summon> (path);rogue_game_chapters = GameConfigTools.GenConfigDictByJsonFile <int,rogue_game_chapter> (path);battle_npcs = GameConfigTools.GenConfigDictByJsonFile <int,battle_npc> (path);vehicles = GameConfigTools.GenConfigDictByJsonFile <int,vehicle> (path);play_buffs = GameConfigTools.GenConfigDictByJsonFile <int,play_buff> (path);self_effects = GameConfigTools.GenConfigDictByJsonFile <string,self_effect> (path);lock_areas = GameConfigTools.GenConfigDictByJsonFile <string,lock_area> (path);items = GameConfigTools.GenConfigDictByJsonFile <int,item> (path);weapons = GameConfigTools.GenConfigDictByJsonFile <int,weapon> (path);passives = GameConfigTools.GenConfigDictByJsonFile <int,passive> (path);base_attributes = GameConfigTools.GenConfigDictByJsonFile <int,base_attribute> (path);}public void LoadAllByJsonString(Dictionary<string,string> nameToJsonString ){skills = GameConfigTools.GenConfigDictByJsonString <string,skill> (nameToJsonString["skill"] );bad_wordss = GameConfigTools.GenConfigDictByJsonString <int,bad_words> (nameToJsonString["bad_words"] );caught_buffs = GameConfigTools.GenConfigDictByJsonString <string,caught_buff> (nameToJsonString["caught_buff"] );props = GameConfigTools.GenConfigDictByJsonString <int,prop> (nameToJsonString["prop"] );talents = GameConfigTools.GenConfigDictByJsonString <int,talent> (nameToJsonString["talent"] );other_configs = GameConfigTools.GenConfigDictByJsonString <int,other_config> (nameToJsonString["other_config"] );traps = GameConfigTools.GenConfigDictByJsonString <string,trap> (nameToJsonString["trap"] );radar_waves = GameConfigTools.GenConfigDictByJsonString <string,radar_wave> (nameToJsonString["radar_wave"] );standard_level_ups = GameConfigTools.GenConfigDictByJsonString <int,standard_level_up> (nameToJsonString["standard_level_up"] );snipes = GameConfigTools.GenConfigDictByJsonString <int,snipe> (nameToJsonString["snipe"] );bodys = GameConfigTools.GenConfigDictByJsonString <size,body> (nameToJsonString["body"] );skill_groups = GameConfigTools.GenConfigDictByJsonString <string,skill_group> (nameToJsonString["skill_group"] );interactions = GameConfigTools.GenConfigDictByJsonString <interactionAct,interaction> (nameToJsonString["interaction"] );show_texts = GameConfigTools.GenConfigDictByJsonString <string,show_text> (nameToJsonString["show_text"] );bullets = GameConfigTools.GenConfigDictByJsonString <string,bullet> (nameToJsonString["bullet"] );characters = GameConfigTools.GenConfigDictByJsonString <int,character> (nameToJsonString["character"] );push_buffs = GameConfigTools.GenConfigDictByJsonString <string,push_buff> (nameToJsonString["push_buff"] );map_rawss = GameConfigTools.GenConfigDictByJsonString <int,map_raws> (nameToJsonString["map_raws"] );summons = GameConfigTools.GenConfigDictByJsonString <string,summon> (nameToJsonString["summon"] );rogue_game_chapters = GameConfigTools.GenConfigDictByJsonString <int,rogue_game_chapter> (nameToJsonString["rogue_game_chapter"] );battle_npcs = GameConfigTools.GenConfigDictByJsonString <int,battle_npc> (nameToJsonString["battle_npc"] );vehicles = GameConfigTools.GenConfigDictByJsonString <int,vehicle> (nameToJsonString["vehicle"] );play_buffs = GameConfigTools.GenConfigDictByJsonString <int,play_buff> (nameToJsonString["play_buff"] );self_effects = GameConfigTools.GenConfigDictByJsonString <string,self_effect> (nameToJsonString["self_effect"] );lock_areas = GameConfigTools.GenConfigDictByJsonString <string,lock_area> (nameToJsonString["lock_area"] );items = GameConfigTools.GenConfigDictByJsonString <int,item> (nameToJsonString["item"] );weapons = GameConfigTools.GenConfigDictByJsonString <int,weapon> (nameToJsonString["weapon"] );passives = GameConfigTools.GenConfigDictByJsonString <int,passive> (nameToJsonString["passive"] );base_attributes = GameConfigTools.GenConfigDictByJsonString <int,base_attribute> (nameToJsonString["base_attribute"] );}}[Serializable] public class Point:IGameConfig { public float x { get; set; } public float y { get; set; }  } [Serializable] public class SimpleObj1:IGameConfig { public float key_time { get; set; } public Point key_point { get; set; }  } [Serializable] public class Media:IGameConfig { [JsonConverter(typeof(StringEnumConverter))]public effect_media_type media_type { get; set; } public string e_id { get; set; }  } [Serializable] public class Cost:IGameConfig { public int item { get; set; } public int num { get; set; } public int first_use_pay { get; set; }  } [Serializable] public class Gain:IGameConfig { public int item { get; set; } public int num { get; set; }  } [Serializable] public class Buff:IGameConfig { [JsonConverter(typeof(StringEnumConverter))]public size size { get; set; } [JsonConverter(typeof(StringEnumConverter))]public buff_type buff_type { get; set; } public string buff_id { get; set; }  } [Serializable] public class TelPoint:IGameConfig { [JsonConverter(typeof(StringEnumConverter))]public direction Direction { get; set; } public Point[] Teleport { get; set; }  } [Serializable] public class SimpleObj2:IGameConfig { public int weight { get; set; } [JsonConverter(typeof(StringEnumConverter))]public botOp op { get; set; }  } [Serializable] public class SimpleObj3:IGameConfig { public float item1 { get; set; } public float item2 { get; set; }  } [Serializable] public class SimpleObj4:IGameConfig { [JsonConverter(typeof(StringEnumConverter))]public size body { get; set; } public float snipe_speed_fix { get; set; } public string skill_group { get; set; }  } }