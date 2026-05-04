using System.Collections.Generic;

namespace RestaurantKarin
{
    public static class Sesion
    {
        public static string Nombre { get; set; }
        public static string Rol { get; set; }
        public static string Permisos { get; set; } = "";

        public static bool EsAdmin => Rol == "Admin";

        public static bool TienePermiso(string modulo)
        {
            if (EsAdmin) return true;
            if (string.IsNullOrEmpty(Permisos)) return false;
            return Permisos.Contains(modulo);
        }

        public static void Cerrar()
        {
            Nombre = "";
            Rol = "";
            Permisos = "";
        }
    }
}