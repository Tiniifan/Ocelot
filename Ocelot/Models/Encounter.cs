using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocelot.Models
{
    public class EncountInfo
    {
        public int ChasePlayer { get; set; } // 2 = true, other = false
        public int TeamID { get; set; }
        public int Unk2 { get; set; }
        public float LocationX { get; set; }
        public float LocationZ { get; set; }
        public float LocationY { get; set; }
        public int Unk6 { get; set; }
        public int Unk7 { get; set; }
        public float Rotation { get; set; }
        public int TextIDAsk { get; set; }
        public int TextIDWait { get; set; }
        public string Condition { get; set; }
        public int PlayerID1 { get; set; }
        public int PlayerID2 { get; set; }
        public int PlayerID3 { get; set; }
        public int PlayerID4 { get; set; }
        public int PlayerID5 { get; set; }
        public int Uniform { get; set; }
        public int Boots { get; set; }
        public int TextIDLost { get; set; }
        public int TextIDWin { get; set; }
        public int Unk21 { get; set; }

        public bool IsChasingPlayer => ChasePlayer == 2;

        //public Position4D Position
        //{
        //    get => new Position4D(LocationX, LocationZ, LocationY, Rotation);
        //    set
        //    {
        //        LocationX = value.LocationX;
        //        LocationZ = value.LocationZ;
        //        LocationY = value.LocationY;
        //        Rotation = value.Rotation;
        //    }
        //}

        public List<int> PlayerIDs => new List<int> { PlayerID1, PlayerID2, PlayerID3, PlayerID4, PlayerID5 };
    }
}
