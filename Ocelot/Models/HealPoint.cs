using StudioElevenLib.Level5.Binary.Collections;
using System.Collections.Generic;

namespace Ocelot.Models
{
    public class HealArea
    {
        public string PtreType { get; set; }

        public string HealAreaName { get; set; }

        public Position3D Position { get; set; }

        public HealArea(PtreeNode ptreeNode)
        {
            InitializeFromNode(ptreeNode);
        }

        private void InitializeFromNode(PtreeNode ptreeNode)
        {
            if (ptreeNode == null) return;

            PtreeNode basePtreeNode = ptreeNode.FindByHeader("FP");

            if (basePtreeNode != null)
            {
                // Value
                PtreType = basePtreeNode.GetValue<string>(0);
                HealAreaName = basePtreeNode.GetValue<string>(1);

                if (basePtreeNode.FindByHeader("POS") != null)
                {
                    Position = new Position3D(basePtreeNode, "POS");
                }
            }
        }
    }

    public class Healpoint
    {
        public string HealpointName { get; set; }

        public string MapID { get; set; }

        public List<HealArea> HealAreas;

        public Healpoint(PtreeNode ptreeNode)
        {
            InitializeFromNode(ptreeNode);
        }

        private void InitializeFromNode(PtreeNode ptreeNode)
        {
            if (ptreeNode == null) return;

            PtreeNode basePtreeNode = ptreeNode.FindByHeader("HEALPOINT");

            if (basePtreeNode != null)
            {
                // Value
                HealpointName = basePtreeNode.GetValue<string>(0);
                MapID = basePtreeNode.GetValue<string>(1);

                HealAreas = new List<HealArea>();

                foreach (PtreeNode childPtreeNode in ptreeNode.Children)
                {
                    if (childPtreeNode.Header == "FP")
                    {
                        HealAreas.Add(new HealArea(childPtreeNode));
                    }
                }
            }
        }
    }
}
