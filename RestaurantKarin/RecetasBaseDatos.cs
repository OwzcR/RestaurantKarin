using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Globalization;

namespace RestaurantKarin
{
    /// <summary>
    /// Una fila de ingrediente de una receta (tabla receta_linea).
    /// </summary>
    public sealed class RecetaLinea
    {
        public int IdLinea { get; set; }
        public string Insumo { get; set; } = "";
        public decimal Cantidad { get; set; }
        public string Unidad { get; set; } = "";
        public decimal CostoTotal { get; set; }
    }

    /// <summary>
    /// Cabecera de receta + lista de líneas cargada desde SQLite.
    /// </summary>
    public sealed class Receta
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public decimal Porciones { get; set; } = 1;
        public decimal CostoPorPorcion { get; set; }
        public List<RecetaLinea> Lineas { get; set; } = new List<RecetaLinea>();

        public Receta CopiaParaEdicion()
        {
            var n = new Receta
            {
                Id = Id,
                Nombre = Nombre,
                Descripcion = Descripcion,
                Porciones = Porciones,
                CostoPorPorcion = CostoPorPorcion
            };
            foreach (var l in Lineas)
            {
                n.Lineas.Add(new RecetaLinea
                {
                    IdLinea = l.IdLinea,
                    Insumo = l.Insumo,
                    Cantidad = l.Cantidad,
                    Unidad = l.Unidad,
                    CostoTotal = l.CostoTotal
                });
            }
            return n;
        }
    }

    /// <summary>
    /// Todo el acceso SQL del módulo recetas. Los formularios solo llaman estos métodos.
    /// Cadena: misma entrada "KarinDB" que FormLogin (App.config).
    /// </summary>
    public static class RecetasBaseDatos
    {
        private static string Cadena()
        {
            var cs = ConfigurationManager.ConnectionStrings["KarinDB"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
                return "Data Source=karin_pos.db;Version=3;";
            if (!cs.Contains("Version=", StringComparison.OrdinalIgnoreCase))
                return cs.Trim().TrimEnd(';') + ";Version=3;";
            return cs;
        }

        private static void ActivarClavesForaneas(SQLiteConnection con)
        {
            using var cmd = new SQLiteCommand("PRAGMA foreign_keys = ON;", con);
            cmd.ExecuteNonQuery();
        }

        /// <summary>Lista para la columna izquierda (sin cargar líneas; más rápido).</summary>
        public static List<Receta> ListarSoloCabeceras()
        {
            var lista = new List<Receta>();
            using var con = new SQLiteConnection(Cadena());
            con.Open();
            ActivarClavesForaneas(con);
            const string q = @"SELECT id_receta, nombre, descripcion, porciones, costo_por_porcion
                                 FROM receta ORDER BY nombre COLLATE NOCASE;";
            using var cmd = new SQLiteCommand(q, con);
            using var lector = cmd.ExecuteReader();
            while (lector.Read())
            {
                lista.Add(new Receta
                {
                    Id = lector.GetInt32(0),
                    Nombre = lector.IsDBNull(1) ? "" : lector.GetString(1),
                    Descripcion = lector.IsDBNull(2) ? "" : lector.GetString(2),
                    Porciones = Convert.ToDecimal(lector.GetValue(3), CultureInfo.InvariantCulture),
                    CostoPorPorcion = Convert.ToDecimal(lector.GetValue(4), CultureInfo.InvariantCulture)
                });
            }
            return lista;
        }

        public static Receta? ObtenerPorId(int idReceta)
        {
            using var con = new SQLiteConnection(Cadena());
            con.Open();
            ActivarClavesForaneas(con);
            Receta? r = null;
            using (var cmd = new SQLiteCommand(
                       @"SELECT id_receta, nombre, descripcion, porciones, costo_por_porcion FROM receta WHERE id_receta = @id;", con))
            {
                cmd.Parameters.AddWithValue("@id", idReceta);
                using var lector = cmd.ExecuteReader();
                if (!lector.Read()) return null;
                r = new Receta
                {
                    Id = lector.GetInt32(0),
                    Nombre = lector.IsDBNull(1) ? "" : lector.GetString(1),
                    Descripcion = lector.IsDBNull(2) ? "" : lector.GetString(2),
                    Porciones = Convert.ToDecimal(lector.GetValue(3), CultureInfo.InvariantCulture),
                    CostoPorPorcion = Convert.ToDecimal(lector.GetValue(4), CultureInfo.InvariantCulture)
                };
            }
            using (var cmdL = new SQLiteCommand(
                       @"SELECT id_linea, insumo, cantidad, unidad, costo_total FROM receta_linea WHERE id_receta = @id ORDER BY id_linea;", con))
            {
                cmdL.Parameters.AddWithValue("@id", idReceta);
                using var lector = cmdL.ExecuteReader();
                while (lector.Read())
                {
                    r.Lineas.Add(new RecetaLinea
                    {
                        IdLinea = lector.GetInt32(0),
                        Insumo = lector.IsDBNull(1) ? "" : lector.GetString(1),
                        Cantidad = Convert.ToDecimal(lector.GetValue(2), CultureInfo.InvariantCulture),
                        Unidad = lector.IsDBNull(3) ? "" : lector.GetString(3),
                        CostoTotal = Convert.ToDecimal(lector.GetValue(4), CultureInfo.InvariantCulture)
                    });
                }
            }
            return r;
        }

        /// <summary>Inserta o actualiza cabecera y reemplaza todas las líneas.</summary>
        public static void Guardar(Receta r)
        {
            using var con = new SQLiteConnection(Cadena());
            con.Open();
            ActivarClavesForaneas(con);
            using var tx = con.BeginTransaction();
            try
            {
                if (r.Id <= 0)
                {
                    using var ins = new SQLiteCommand(
                        @"INSERT INTO receta (nombre, descripcion, porciones, costo_por_porcion)
                          VALUES (@n, @d, @p, @c);", con, tx);
                    ins.Parameters.AddWithValue("@n", r.Nombre);
                    ins.Parameters.AddWithValue("@d", (object?)r.Descripcion ?? DBNull.Value);
                    ins.Parameters.AddWithValue("@p", r.Porciones);
                    ins.Parameters.AddWithValue("@c", r.CostoPorPorcion);
                    ins.ExecuteNonQuery();
                    using var idCmd = new SQLiteCommand("SELECT last_insert_rowid();", con, tx);
                    r.Id = Convert.ToInt32((long)idCmd.ExecuteScalar()!, CultureInfo.InvariantCulture);
                }
                else
                {
                    using var upd = new SQLiteCommand(
                        @"UPDATE receta SET nombre=@n, descripcion=@d, porciones=@p, costo_por_porcion=@c WHERE id_receta=@id;", con, tx);
                    upd.Parameters.AddWithValue("@n", r.Nombre);
                    upd.Parameters.AddWithValue("@d", (object?)r.Descripcion ?? DBNull.Value);
                    upd.Parameters.AddWithValue("@p", r.Porciones);
                    upd.Parameters.AddWithValue("@c", r.CostoPorPorcion);
                    upd.Parameters.AddWithValue("@id", r.Id);
                    upd.ExecuteNonQuery();
                    using var del = new SQLiteCommand("DELETE FROM receta_linea WHERE id_receta = @id;", con, tx);
                    del.Parameters.AddWithValue("@id", r.Id);
                    del.ExecuteNonQuery();
                }

                foreach (var l in r.Lineas)
                {
                    using var il = new SQLiteCommand(
                        @"INSERT INTO receta_linea (id_receta, insumo, cantidad, unidad, costo_total)
                          VALUES (@rid, @ins, @cant, @u, @cost);", con, tx);
                    il.Parameters.AddWithValue("@rid", r.Id);
                    il.Parameters.AddWithValue("@ins", l.Insumo);
                    il.Parameters.AddWithValue("@cant", l.Cantidad);
                    il.Parameters.AddWithValue("@u", l.Unidad);
                    il.Parameters.AddWithValue("@cost", l.CostoTotal);
                    il.ExecuteNonQuery();
                }
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public static void Eliminar(int idReceta)
        {
            using var con = new SQLiteConnection(Cadena());
            con.Open();
            ActivarClavesForaneas(con);
            using var cmd = new SQLiteCommand("DELETE FROM receta WHERE id_receta = @id;", con);
            cmd.Parameters.AddWithValue("@id", idReceta);
            cmd.ExecuteNonQuery();
        }

        /// <summary>Si la tabla está vacía, inserta ejemplos (solo la primera vez).</summary>
        public static void SembrarEjemplosSiVacio()
        {
            using var con = new SQLiteConnection(Cadena());
            con.Open();
            ActivarClavesForaneas(con);
            using var c0 = new SQLiteCommand("SELECT COUNT(*) FROM receta;", con);
            var n = Convert.ToInt32((long)c0.ExecuteScalar()!, CultureInfo.InvariantCulture);
            if (n > 0) return;

            void InsertarReceta(string nombre, string desc, decimal porc, decimal cpp, params (string ins, decimal cant, string unid, decimal costo)[] lineas)
            {
                var rec = new Receta { Nombre = nombre, Descripcion = desc, Porciones = porc, CostoPorPorcion = cpp };
                foreach (var t in lineas)
                    rec.Lineas.Add(new RecetaLinea { Insumo = t.ins, Cantidad = t.cant, Unidad = t.unid, CostoTotal = t.costo });
                Guardar(rec);
            }

            InsertarReceta("Hamburguesa Clasica",
                "Hamburguesa clásica con carne brioche, queso americano, lechuga y pan artesanal.",
                1, 80.00m,
                ("Carne Brioche", 200, "gramos", 0),
                ("Queso Americano", 2, "rebanada", 0),
                ("Lechuga Rabenta", 30, "gramos", 0),
                ("Pan de Hamburguesa", 2, "rebanada", 0));

            InsertarReceta("Torta de pierna", "Torta tradicional de pierna de cerdo con condimentos.", 1, 45.00m,
                ("Pierna de cerdo", 150, "gramos", 0),
                ("Bolillo", 1, "pieza", 0));

            InsertarReceta("Bistek a la plancha", "Corte de res a la plancha con guarnición.", 1, 95.00m,
                ("Bistek de res", 180, "gramos", 0));

            InsertarReceta("Tostada de pollo", "Tostadas con preparado de pollo.", 2, 35.00m,
                ("Tortilla tostada", 3, "pieza", 0),
                ("Pollo deshebrado", 120, "gramos", 0));
        }
    }
}
