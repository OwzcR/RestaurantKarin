using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace RestaurantKarin
{
    public static class MesaRepository
    {
        private const string Conn = "Data Source=karin_pos.db;Version=3;";

        public static List<MesaModel> GetAll()
        {
            var lista = new List<MesaModel>();

            using var con = new SQLiteConnection(Conn);
            con.Open();

            string sql = @"
                SELECT
                    m.id_mesa,
                    m.numero_mesa,
                    m.capacidad,
                    m.estado,
                    c.subtotal,
                    c.fecha_apertura
                FROM mesa m
                LEFT JOIN cuenta c
                    ON c.id_mesa = m.id_mesa
                    AND c.estado_cuenta = 'Abierta'
                ORDER BY m.numero_mesa;";

            using var cmd = new SQLiteCommand(sql, con);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                bool activa = reader["estado"].ToString() == "Ocupada";

                lista.Add(new MesaModel
                {
                    Id = Convert.ToInt32(reader["id_mesa"]),
                    Nombre = $"MESA : {reader["numero_mesa"]}",
                    CapacidadMax = reader["capacidad"] == DBNull.Value
                                    ? 0 : Convert.ToInt32(reader["capacidad"]),
                    Activa = activa,
                    Cuenta = activa && reader["subtotal"] != DBNull.Value
                                    ? Convert.ToDecimal(reader["subtotal"]) : 0,
                    HoraLlegada = activa && reader["fecha_apertura"] != DBNull.Value
                                    ? DateTime.SpecifyKind(Convert.ToDateTime(reader["fecha_apertura"]), DateTimeKind.Utc).ToLocalTime()
                                    : DateTime.Now,
                    Personas = 1,
                    PropinaPercent = 10
                });
            }

            return lista;
        }

        public static void MarcarOcupada(int idMesa)
        {
            using var con = new SQLiteConnection(Conn);
            con.Open();
            using var cmd = new SQLiteCommand(
                "UPDATE mesa SET estado = 'Ocupada' WHERE id_mesa = @id;", con);
            cmd.Parameters.AddWithValue("@id", idMesa);
            cmd.ExecuteNonQuery();
        }

        public static void MarcarLibre(int idMesa)
        {
            using var con = new SQLiteConnection(Conn);
            con.Open();
            using var cmd = new SQLiteCommand(
                "UPDATE mesa SET estado = 'Libre' WHERE id_mesa = @id;", con);
            cmd.Parameters.AddWithValue("@id", idMesa);
            cmd.ExecuteNonQuery();
        }
    }
}