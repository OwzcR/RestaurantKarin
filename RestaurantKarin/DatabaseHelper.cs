using System;
using System.Data.SQLite;
using System.IO;

namespace RestaurantKarin
{
    public static class DatabaseHelper
    {
        public static void InicializarBaseDeDatos()
        {
            string nombreArchivo = "karin_pos.db";

            if (!File.Exists(nombreArchivo))
            {
                SQLiteConnection.CreateFile(nombreArchivo);

                using (var conexion = new SQLiteConnection($"Data Source={nombreArchivo};Version=3;"))
                {
                    conexion.Open();

                    string script = @"
                        CREATE TABLE usuario (
                            id_usuario      INTEGER PRIMARY KEY AUTOINCREMENT,
                            nombre          TEXT    NOT NULL,
                            rol             TEXT    NOT NULL,
                            pin_acceso      TEXT    NOT NULL UNIQUE,
                            fecha_registro  DATETIME DEFAULT CURRENT_TIMESTAMP,
                            estado          INTEGER  DEFAULT 1,
                            permisos        TEXT     DEFAULT 'Pedidos,Cuentas,Inventario,Recetas,Reportes'
                        );

                        CREATE TABLE mesa (
                            id_mesa       INTEGER PRIMARY KEY AUTOINCREMENT,
                            numero_mesa   INTEGER NOT NULL UNIQUE,
                            capacidad     INTEGER,
                            estado        TEXT DEFAULT 'Libre'
                        );

                        CREATE TABLE categoria (
                            id_categoria INTEGER PRIMARY KEY AUTOINCREMENT,
                            nombre       TEXT NOT NULL,
                            descripcion  TEXT
                        );

                        CREATE TABLE producto (
                            id_producto   INTEGER PRIMARY KEY AUTOINCREMENT,
                            nombre        TEXT NOT NULL,
                            descripcion   TEXT,
                            precio        REAL NOT NULL,
                            id_categoria  INTEGER,
                            disponibilidad INTEGER DEFAULT 1,
                            FOREIGN KEY (id_categoria) REFERENCES categoria(id_categoria)
                        );

                        CREATE TABLE cuenta (
                            id_cuenta             INTEGER PRIMARY KEY AUTOINCREMENT,
                            id_mesa               INTEGER,
                            id_usuario_apertura   INTEGER NOT NULL,
                            fecha_apertura        DATETIME DEFAULT CURRENT_TIMESTAMP,
                            fecha_cierre          DATETIME,
                            estado_cuenta         TEXT DEFAULT 'Abierta',
                            tipo_pedido           TEXT DEFAULT 'Local',
                            cargo_servicio_extra  REAL DEFAULT 0.00,
                            subtotal              REAL DEFAULT 0.00,
                            total                 REAL DEFAULT 0.00,
                            FOREIGN KEY (id_mesa) REFERENCES mesa(id_mesa),
                            FOREIGN KEY (id_usuario_apertura) REFERENCES usuario(id_usuario)
                        );

                        CREATE TABLE detalle_cuenta (
                            id_detalle          INTEGER PRIMARY KEY AUTOINCREMENT,
                            id_cuenta           INTEGER NOT NULL,
                            id_producto         INTEGER NOT NULL,
                            cantidad            INTEGER NOT NULL,
                            precio_unitario     REAL NOT NULL,
                            subtotal            REAL NOT NULL,
                            notas               TEXT,
                            estado_preparacion  TEXT DEFAULT 'Pendiente',
                            FOREIGN KEY (id_cuenta) REFERENCES cuenta(id_cuenta),
                            FOREIGN KEY (id_producto) REFERENCES producto(id_producto)
                        );

                        CREATE TABLE receta (
                            id_receta          INTEGER PRIMARY KEY AUTOINCREMENT,
                            nombre             TEXT NOT NULL,
                            descripcion        TEXT,
                            porciones          REAL NOT NULL DEFAULT 1,
                            costo_por_porcion  REAL NOT NULL DEFAULT 0
                        );

                        CREATE TABLE receta_linea (
                            id_linea    INTEGER PRIMARY KEY AUTOINCREMENT,
                            id_receta   INTEGER NOT NULL,
                            insumo      TEXT NOT NULL,
                            cantidad    REAL NOT NULL,
                            unidad      TEXT NOT NULL,
                            costo_total REAL NOT NULL DEFAULT 0,
                            FOREIGN KEY (id_receta) REFERENCES receta(id_receta) ON DELETE CASCADE
                        );

                        INSERT INTO usuario (nombre, rol, pin_acceso, permisos)
                        VALUES ('Dueño Karin', 'Admin', '1234', 'Pedidos,Cuentas,Inventario,Recetas,Reportes');

                        INSERT INTO usuario (nombre, rol, pin_acceso, permisos)
                        VALUES ('Mesero Estrella', 'Mesero', '5678', 'Pedidos,Cuentas');

                        INSERT INTO mesa (numero_mesa, capacidad) VALUES (1, 4), (2, 4), (3, 6);

                        INSERT INTO categoria (nombre, descripcion) VALUES ('Mariscos Frescos', 'Ceviches y más');

                        INSERT INTO producto (nombre, descripcion, precio, id_categoria) VALUES
                        ('Ceviche Mixto', 'Orden de ceviche', 180.00, 1),
                        ('Limonada', 'Jarra', 80.00, 1);
                    ";

                    using (var comando = new SQLiteCommand(script, conexion))
                        comando.ExecuteNonQuery();
                }
            }

            AsegurarTablasRecetas();
            AsegurarColumnaPermisos();
        }

        // Agrega columna permisos a DBs existentes que no la tienen
        public static void AsegurarColumnaPermisos()
        {
            string nombreArchivo = "karin_pos.db";
            if (!File.Exists(nombreArchivo)) return;

            using (var con = new SQLiteConnection($"Data Source={nombreArchivo};Version=3;"))
            {
                con.Open();
                try
                {
                    using (var cmd = new SQLiteCommand(
                        "ALTER TABLE usuario ADD COLUMN permisos TEXT DEFAULT 'Pedidos,Cuentas,Inventario,Recetas,Reportes'", con))
                        cmd.ExecuteNonQuery();
                }
                catch { /* columna ya existe, ignorar */ }

                // Admin siempre tiene todos los permisos
                using (var cmd = new SQLiteCommand(
                    "UPDATE usuario SET permisos='Pedidos,Cuentas,Inventario,Recetas,Reportes' WHERE rol='Admin' AND (permisos IS NULL OR permisos='')", con))
                    cmd.ExecuteNonQuery();
            }
        }

        public static void AsegurarTablasRecetas()
        {
            string nombreArchivo = "karin_pos.db";
            if (!File.Exists(nombreArchivo)) return;

            using (var conexion = new SQLiteConnection($"Data Source={nombreArchivo};Version=3;"))
            {
                conexion.Open();
                using (var pragma = new SQLiteCommand("PRAGMA foreign_keys = ON;", conexion))
                    pragma.ExecuteNonQuery();

                string sql = @"
CREATE TABLE IF NOT EXISTS receta (
    id_receta INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre TEXT NOT NULL,
    descripcion TEXT,
    porciones REAL NOT NULL DEFAULT 1,
    costo_por_porcion REAL NOT NULL DEFAULT 0
);
CREATE TABLE IF NOT EXISTS receta_linea (
    id_linea INTEGER PRIMARY KEY AUTOINCREMENT,
    id_receta INTEGER NOT NULL,
    insumo TEXT NOT NULL,
    cantidad REAL NOT NULL,
    unidad TEXT NOT NULL,
    costo_total REAL NOT NULL DEFAULT 0,
    FOREIGN KEY (id_receta) REFERENCES receta(id_receta) ON DELETE CASCADE
);";
                using (var comando = new SQLiteCommand(sql, conexion))
                    comando.ExecuteNonQuery();
            }

            RecetasBaseDatos.SembrarEjemplosSiVacio();
        }
    }
}