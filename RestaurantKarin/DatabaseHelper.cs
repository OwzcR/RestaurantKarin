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

            // Si el archivo no existe, lo crea y arma las tablas
            if (!File.Exists(nombreArchivo))
            {
                SQLiteConnection.CreateFile(nombreArchivo);

                using (var conexion = new SQLiteConnection($"Data Source={nombreArchivo};Version=3;"))
                {
                    conexion.Open();

                    string script = @"
                        -- 1. Usuarios
                        CREATE TABLE usuario (
                            id_usuario INTEGER PRIMARY KEY AUTOINCREMENT,
                            nombre TEXT NOT NULL,
                            rol TEXT NOT NULL,
                            pin_acceso TEXT NOT NULL UNIQUE,
                            fecha_registro DATETIME DEFAULT CURRENT_TIMESTAMP,
                            estado INTEGER DEFAULT 1
                        );

                        -- 2. Mesas
                        CREATE TABLE mesa (
                            id_mesa INTEGER PRIMARY KEY AUTOINCREMENT,
                            numero_mesa INTEGER NOT NULL UNIQUE,
                            capacidad INTEGER,
                            estado TEXT DEFAULT 'Libre'
                        );

                        -- 3. Categorías
                        CREATE TABLE categoria (
                            id_categoria INTEGER PRIMARY KEY AUTOINCREMENT,
                            nombre TEXT NOT NULL,
                            descripcion TEXT
                        );

                        -- 4. Productos
                        CREATE TABLE producto (
                            id_producto INTEGER PRIMARY KEY AUTOINCREMENT,
                            nombre TEXT NOT NULL,
                            descripcion TEXT,
                            precio REAL NOT NULL,
                            id_categoria INTEGER,
                            disponibilidad INTEGER DEFAULT 1,
                            FOREIGN KEY (id_categoria) REFERENCES categoria(id_categoria)
                        );

                        -- 5. Cuentas
                        CREATE TABLE cuenta (
                            id_cuenta INTEGER PRIMARY KEY AUTOINCREMENT,
                            id_mesa INTEGER,
                            id_usuario_apertura INTEGER NOT NULL,
                            fecha_apertura DATETIME DEFAULT CURRENT_TIMESTAMP,
                            fecha_cierre DATETIME,
                            estado_cuenta TEXT DEFAULT 'Abierta',
                            tipo_pedido TEXT DEFAULT 'Local',
                            cargo_servicio_extra REAL DEFAULT 0.00,
                            subtotal REAL DEFAULT 0.00,
                            total REAL DEFAULT 0.00,
                            FOREIGN KEY (id_mesa) REFERENCES mesa(id_mesa),
                            FOREIGN KEY (id_usuario_apertura) REFERENCES usuario(id_usuario)
                        );

                        -- 6. Detalle Cuenta
                        CREATE TABLE detalle_cuenta (
                            id_detalle INTEGER PRIMARY KEY AUTOINCREMENT,
                            id_cuenta INTEGER NOT NULL,
                            id_producto INTEGER NOT NULL,
                            cantidad INTEGER NOT NULL,
                            precio_unitario REAL NOT NULL,
                            subtotal REAL NOT NULL,
                            notas TEXT,
                            estado_preparacion TEXT DEFAULT 'Pendiente',
                            FOREIGN KEY (id_cuenta) REFERENCES cuenta(id_cuenta),
                            FOREIGN KEY (id_producto) REFERENCES producto(id_producto)
                        );

                        -- DATOS DE PRUEBA (Para que puedan iniciar sesión)
                        INSERT INTO usuario (nombre, rol, pin_acceso) VALUES ('Dueño Karin', 'Admin', '1234');
                        INSERT INTO usuario (nombre, rol, pin_acceso) VALUES ('Mesero Estrella', 'Mesero', '5678');
                        
                        INSERT INTO mesa (numero_mesa, capacidad) VALUES (1, 4), (2, 4), (3, 6);
                        
                        INSERT INTO categoria (nombre, descripcion) VALUES ('Mariscos Frescos', 'Ceviches y más');
                        
                        INSERT INTO producto (nombre, descripcion, precio, id_categoria) VALUES 
                        ('Ceviche Mixto', 'Orden de ceviche', 180.00, 1),
                        ('Limonada', 'Jarra', 80.00, 1);
                    ";

                    using (var comando = new SQLiteCommand(script, conexion))
                    {
                        comando.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}