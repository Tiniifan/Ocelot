namespace Ocelot.Models
{
    public class NPCBase
    {
        public int ID { get; set; }
        public int ModelHead { get; set; }
        public int Type { get; set; } // NPCType
        public int Unk1 { get; set; }
        public int UniformNumber { get; set; }
        public int BootsNumber { get; set; }
        public int GloveNumber { get; set; }
        public int Icon { get; set; } // IconType

        public NPCBase Clone()
        {
            return new NPCBase
            {
                ID = this.ID,
                ModelHead = this.ModelHead,
                Type = this.Type,
                Unk1 = this.Unk1,
                UniformNumber = this.UniformNumber,
                BootsNumber = this.BootsNumber,
                GloveNumber = this.GloveNumber,
                Icon = this.Icon
            };
        }
    }

    public class NPCPreset
    {
        public int NPCBaseID { get; set; }
        public int NPCAppearStartIndex { get; set; }
        public int NPCAppearCount { get; set; }

        public NPCPreset Clone()
        {
            return new NPCPreset
            {
                NPCBaseID = this.NPCBaseID,
                NPCAppearStartIndex = this.NPCAppearStartIndex,
                NPCAppearCount = this.NPCAppearCount
            };
        }
    }

    public class NPCAppear
    {
        public float LocationX { get; set; }
        public float LocationZ { get; set; }
        public float LocationY { get; set; }
        public int Unk1 { get; set; }
        public int Unk2 { get; set; }
        public float Rotation { get; set; }
        public string StandAnimation { get; set; }
        public int LookAtThePlayer { get; set; } // 2 = true, other = false
        public string TalkAnimation { get; set; }
        public string UnkAnimation { get; set; }
        public int Unk7 { get; set; }
        public object PhaseAppear { get; set; }
        public int Unk8 { get; set; }

        public NPCAppear Clone()
        {
            return new NPCAppear
            {
                LocationX = this.LocationX,
                LocationZ = this.LocationZ,
                LocationY = this.LocationY,
                Unk1 = this.Unk1,
                Unk2 = this.Unk2,
                Rotation = this.Rotation,
                StandAnimation = this.StandAnimation,
                TalkAnimation = this.TalkAnimation,
                LookAtThePlayer = this.LookAtThePlayer,
                UnkAnimation = this.UnkAnimation,
                Unk7 = this.Unk7,
                PhaseAppear = this.PhaseAppear,
                Unk8 = this.Unk8
            };
        }
    }

    public class NPCTalkInfo
    {
        public int NPCBaseID { get; set; }
        public int TalkConfigStartIndex { get; set; }
        public int TalkConfigCount { get; set; }

        public NPCTalkInfo Clone()
        {
            return new NPCTalkInfo
            {
                NPCBaseID = this.NPCBaseID,
                TalkConfigStartIndex = this.TalkConfigStartIndex,
                TalkConfigCount = this.TalkConfigCount
            };
        }
    }

    public class NPCTalkConfig
    {
        public int EventType { get; set; }
        public object EventValue { get; set; }
        public object EventCondition { get; set; }
        public int AutoTurn { get; set; } // 2 = true, other = false

        public NPCTalkConfig Clone()
        {
            return new NPCTalkConfig
            {
                EventType = this.EventType,
                EventValue = this.EventValue,
                EventCondition = this.EventCondition,
                AutoTurn = this.AutoTurn
            };
        }
    }
}