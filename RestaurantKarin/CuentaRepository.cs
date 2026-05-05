using System;
using System.Data.SQLite;

namespace RestaurantKarin
{
    public static class CuentaRepository
    {
        private const string Conn = "Data Source=karin_pos.db;Version=3;";

        public static int AbrirCuenta(int idMesa, int idUsuario)
        {
            using var con = new SQLiteConnection(Conn);
            con.Open();

            using var cmd = new SQLiteCommand(@"
                INSERT INTO cuenta (id_mesa, id_usuario_apertura, estado_cuenta, tipo_pedido)
                VALUES (@mesa, @usuario, 'Abierta', 'Local');
                SELECT last_insert_rowid();", con);

            cmd.Parameters.AddWithValue("@mesa", idMesa);
            cmd.Parameters.AddWithValue("@usuario", idUsuario);

            long idCuenta = (long)cmd.ExecuteScalar();
            MesaRepository.MarcarOcupada(idMesa);
            return (int)idCuenta;
        }

        public static void CerrarCuenta(int idMesa)
        {
            using var con = new SQLiteConnection(Conn);
            con.Open();

            using var cmd = new SQLiteCommand(@"
                UPDATE cuenta
                SET estado_cuenta = 'Cerrada',
                    fecha_cierre  = CURRENT_TIMESTAMP
                WHERE id_mesa       = @mesa
                  AND estado_cuenta = 'Abierta';", con);

            cmd.Parameters.AddWithValue("@mesa", idMesa);
            cmd.ExecuteNonQuery();
            MesaRepository.MarcarLibre(idMesa);
        }

        public static void CerrarCuentaPorId(int idCuenta)
        {
            using var con = new SQLiteConnection(Conn);
            con.Open();

            int idMesa = -1;
            using (var cmd = new SQLiteCommand(
                "SELECT id_mesa FROM cuenta WHERE id_cuenta = @id;", con))
            {
                cmd.Parameters.AddWithValue("@id", idCuenta);
                var r = cmd.ExecuteScalar();
                if (r != null && r != DBNull.Value) idMesa = Convert.ToInt32(r);
            }

            using (var cmd = new SQLiteCommand(@"
                UPDATE cuenta
                SET estado_cuenta = 'Cerrada',
                    fecha_cierre  = CURRENT_TIMESTAMP
                WHERE id_cuenta = @id;", con))
            {
                cmd.Parameters.AddWithValue("@id", idCuenta);
                cmd.ExecuteNonQuery();
            }

            if (idMesa > 0) MesaRepository.MarcarLibre(idMesa);
        }

        public static void ActualizarTotales(int idCuenta, decimal subtotal, decimal total)
        {
            using var con = new SQLiteConnection(Conn);
            con.Open();

            using var cmd = new SQLiteCommand(@"
                UPDATE cuenta
                SET subtotal = @sub,
                    total    = @tot
                WHERE id_cuenta = @id;", con);

            cmd.Parameters.AddWithValue("@sub", subtotal);
            cmd.Parameters.AddWithValue("@tot", total);
            cmd.Parameters.AddWithValue("@id", idCuenta);
            cmd.ExecuteNonQuery();
        }


        public static int ObtenerIdCuentaAbierta(int idMesa)
        {
            using var con = new SQLiteConnection(Conn);
            con.Open();

            using var cmd = new SQLiteCommand(@"
        SELECT id_cuenta FROM cuenta
        WHERE id_mesa      = @mesa
          AND estado_cuenta = 'Abierta'
        LIMIT 1;", con);

            cmd.Parameters.AddWithValue("@mesa", idMesa);
            var result = cmd.ExecuteScalar();

            return result == null || result == DBNull.Value ? -1 : Convert.ToInt32(result);
        }
    }
}