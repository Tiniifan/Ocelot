namespace Ocelot.Models
{
    public class TBoxConfig
    {
        public int TBoxID { get; set; }
        public float LocationX { get; set; }
        public float LocationZ { get; set; }
        public float LocationY { get; set; }
        public float Rotation { get; set; }
        public int ItemIDLight { get; set; }
        public int TBoxFlag { get; set; }
        public TBoxType TboxType { get; set; }
        public int ItemIDShadow { get; set; }

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
    }
}
