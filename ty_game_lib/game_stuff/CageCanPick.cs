using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using collision_and_rigid;
using game_config;

namespace game_stuff
{
    public class CageCanPick : IMapInteractable
    {
        private CageCanPick(Interaction charActOne, Interaction charActTwo, Round canInterActiveRound, Zone zone)
        {
            CharActOne = charActOne;
            CharActOne.InMapInteractable.InWhichMapInteractive = this;
            CharActTwo = charActTwo;
            CharActTwo.InMapInteractable.InWhichMapInteractive = this;
            CanInterActiveRound = canInterActiveRound;
            Zone = zone;
            NowInterCharacterBody = null;
            LocateRecord = new Queue<Quad>();
        }


        public static Interaction GenInteractionByConfig(ICanPutInMapInteractable canPutInMapInteractable,
            interaction interaction,
            MapInteract mapInteract)
        {
            return new Interaction(interaction.BaseTough,
                CommonConfig.GetTickByTime(interaction.TotalTime),
                canPutInMapInteractable, null, mapInteract);
        }

        public CageCanPick(ICanPutInMapInteractable canPutInMapInteractable, TwoDPoint pos)
        {
            var configsInteraction = CommonConfig.Configs.interactions;

            var roundP = canPutInMapInteractable switch
            {
                Prop _ => new Round(pos, LocalConfig.PropR),
                Weapon _ => new Round(pos, LocalConfig.WeaponR),
                PassiveTrait _ => new Round(pos, LocalConfig.PassiveR),
                _ => throw new ArgumentOutOfRangeException(nameof(canPutInMapInteractable))
            };
            var interaction1 = configsInteraction[interactionAct.pick_up_cage];
            Interaction interaction =
                GenInteractionByConfig(canPutInMapInteractable, interaction1, MapInteract.PickCall);
            var interaction2 = configsInteraction[interactionAct.recycle_cage];
            Interaction interaction22 =
                GenInteractionByConfig(canPutInMapInteractable, interaction2, MapInteract.RecycleCall);

            var zonesP = roundP.GetZones();
            Zone = zonesP;
            CanInterActiveRound = roundP;
            CharActTwo = interaction22;
            CharActOne = interaction;
            CharActOne.InMapInteractable.InWhichMapInteractive = this;
            CharActTwo.InMapInteractable.InWhichMapInteractive = this;
            NowInterCharacterBody = null;
            LocateRecord = new Queue<Quad>();
        }


        public void ReLocate(TwoDPoint pos)
        {
            MapInteractableDefault.ReLocate(pos, this);
        }

        public void StartActOneBySomeBody(CharacterBody characterBody)
        {
            MapInteractableDefault.StartActOneBySomeBody(characterBody, this);
        }

        public void StartActTwoBySomeBody(CharacterBody characterBody)
        {
            MapInteractableDefault.StartActTwoBySomeBody(characterBody, this);
        }

        public CharacterBody? NowInterCharacterBody { get; set; }
        public Interaction CharActOne { get; }
        public Interaction CharActTwo { get; }

        public void RecordQuad(int qI)
        {
            MapInteractableDefault.RecardQuad(qI, this);
        }

        public bool IsCage()
        {
            return true;
        }

        public bool IsVehicle()
        {
            return false;
        }

        public bool IsSale()
        {
            return false;
        }

        public Queue<Quad> LocateRecord { get; set; }


        public Interaction? GetActOne(CharacterStatus characterStatus)
        {
            return CharActOne.InMapInteractable.CanInterActOneBy(characterStatus) ? CharActOne : null;
        }

        public Interaction? GetActTwo(CharacterStatus characterStatus)
        {
            return CharActTwo.InMapInteractable.CanInterActTwoBy(characterStatus) ? CharActTwo : null;
        }

        public Round CanInterActiveRound { get; }

        public void WriteQuadRecord(Quad quad)
        {
            MapInteractableDefault.WriteQuadRecord(quad, this);
        }

        public Quad? GetNextQuad()
        {
            return MapInteractableDefault.GetNextQuad(this);
        }

        public IShape GetShape()
        {
            return CanInterActiveRound;
        }


        public bool CanInteractive(TwoDPoint pos)
        {
            return Zone.IncludePt(pos) && CanInterActiveRound.Include(pos) && NowInterCharacterBody == null;
        }

        public Zone Zone { get; set; }

        public IEnumerable<(int, IAaBbBox)> SplitByQuads(float horizon, float vertical)
        {
            return MapInteractableDefault.SplitByQuads(horizon, vertical, this);
        }

        public TwoDPoint GetAnchor()
        {
            return CanInterActiveRound.O;
        }

        public ISeeTickMsg GenTickMsg(int? gid = null)
        {
            return CharActOne.InMapInteractable switch
            {
                PassiveTrait passiveTrait => new CageTickMsg(ContainType.PassiveC, passiveTrait.PassId, GetAnchor()),
                Prop prop => new CageTickMsg(ContainType.PropC, prop.PId, GetAnchor()),
                Weapon weapon => new CageTickMsg(ContainType.WeaponC, weapon.WId, GetAnchor()),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public enum ContainType
    {
        PassiveC,
        PropC,
        WeaponC,
        VehicleC,
        GameItemC,
    }

    public class CageTickMsg : ISeeTickMsg
    {
        public CageTickMsg(ContainType containType, int id, TwoDPoint pos)
        {
            ContainType = containType;
            Id = id;
            Pos = pos;
        }

        public TwoDPoint Pos { get; }
        public ContainType ContainType { get; }

        public int Id { get; }
    }
}