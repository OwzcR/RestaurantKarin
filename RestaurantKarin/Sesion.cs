namespace RestaurantKarin
{
    public static class Sesion
    {
        public static string Nombre { get; set; }
        public static string Rol { get; set; }

        public static bool EsAdmin => Rol == "Admin";

        public static void Cerrar()
        {
            Nombre = "";
            Rol = "";
        }
    }
}