using System;

namespace RestaurantKarin
{
    public class Mesa
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public int Capacidad { get; set; }
        public bool Activa { get; set; }
        public int Personas { get; set; }
        public DateTime HoraLlegada { get; set; }
        public decimal Cuenta { get; set; }
        public decimal PropinaPercent { get; set; }
    }
}
