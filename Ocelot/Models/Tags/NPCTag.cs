namespace Ocelot.Models.Tags
{
    public class NPCTagData
    {
        public string Type { get; set; }
        public NPCBase Data { get; set; }
    }

    public class TalksCategoryTagData
    {
        public string Type { get; set; }
        public NPCBase NPC { get; set; }
    }

    public class NPCTalkTagData
    {
        public string Type { get; set; }
        public NPCBase NPC { get; set; }
        public NPCTalkConfig Talk { get; set; }
        public int Index { get; set; }
    }

    public class AppearsCategoryTagData
    {
        public string Type { get; set; }
        public NPCBase NPC { get; set; }
    }

    public class NPCAppearTagData
    {
        public string Type { get; set; }
        public NPCBase NPC { get; set; }
        public NPCAppear Appear { get; set; }
        public int Index { get; set; }
    }
}
