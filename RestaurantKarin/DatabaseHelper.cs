using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace RestaurantKarin
{
    public static class DatabaseHelper
    {
        private static readonly string NombreArchivo = "karin_pos.db";
        private static readonly string ConnectionString = $"Data Source={NombreArchivo};Version=3;";

        /// <summary>
        /// Obtiene la cadena de conexión a la base de datos SQLite.
        /// </summary>
        public static string GetConnectionString() => ConnectionString;

        public static void InicializarBaseDeDatos()
        {
            string nombreArchivo = NombreArchivo;

            if (!File.Exists(nombreArchivo))
            {
                SQLiteConnection.CreateFile(nombreArchivo);

                using (var conexion = new SQLiteConnection(ConnectionString))
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

                        CREATE TABLE tabla_unidades (
                            id_unidad INTEGER PRIMARY KEY AUTOINCREMENT,
                            nombre TEXT NOT NULL UNIQUE,
                            abreviatura TEXT NOT NULL UNIQUE,
                            tipo TEXT NOT NULL,
                            descripcion TEXT
                        );

                        CREATE TABLE Insumos (
                            id_insumo INTEGER PRIMARY KEY AUTOINCREMENT,
                            Nombre TEXT NOT NULL UNIQUE,
                            StockActual REAL DEFAULT 0.00,
                            id_unidad INTEGER DEFAULT 1,
                            Unidad TEXT DEFAULT 'gramos',
                            StockMinimo REAL DEFAULT 0.00,
                            FechaEntrada TEXT DEFAULT '',
                            Costo REAL DEFAULT 0.00,
                            FOREIGN KEY (id_unidad) REFERENCES tabla_unidades(id_unidad)
                        );

                        INSERT INTO tabla_unidades (nombre, abreviatura, tipo, descripcion) VALUES
                        ('Kilogramo', 'kg', 'Peso', 'Unidad de peso'),
                        ('Gramo', 'g', 'Peso', 'Unidad de peso'),
                        ('Miligramo', 'mg', 'Peso', 'Unidad de peso'),
                        ('Onza', 'oz', 'Peso', 'Unidad de peso'),
                        ('Libra', 'Lb', 'Peso', 'Unidad de peso'),
                        ('Litro', 'Lts', 'Volumen', 'Unidad de volumen'),
                        ('Mililitro', 'mL', 'Volumen', 'Unidad de volumen'),
                        ('Taza', 'cup', 'Volumen', 'Unidad de volumen (aproximadamente 240 mL)'),
                        ('Cucharada', 'tbsp', 'Volumen', 'Unidad de volumen (aproximadamente 15 mL)'),
                        ('Cucharadita', 'tsp', 'Volumen', 'Unidad de volumen (aproximadamente 5 mL)'),
                        ('Pieza', 'pz', 'Cantidad', 'Unidad individual'),
                        ('Docena', 'doc', 'Cantidad', 'Conjunto de 12 piezas'),
                        ('Piezas', 'pcs', 'Cantidad', 'Múltiples unidades individuales'),
                        ('Rebanada', 'rbd', 'Cantidad', 'Porción de algo cortado'),
                        ('Pizca', 'piz', 'Volumen', 'Pequeña cantidad de condimento');

                        INSERT INTO Insumos (Nombre, StockActual, id_unidad, Unidad, StockMinimo, FechaEntrada, Costo) VALUES
                        ('Aceite vegetal', 0, 7, 'mililitros', 0, '', 0.03),
                        ('Bistek de res', 0, 2, 'gramos', 0, '', 0.65),
                        ('Bolillo', 0, 11, 'pieza', 0, '', 2.00),
                        ('Carne Brioche', 0, 2, 'gramos', 0, '', 0.45),
                        ('Cebolla', 0, 2, 'gramos', 0, '', 0.02),
                        ('Chile serrano', 0, 11, 'pieza', 0, '', 0.50),
                        ('Cilantro', 0, 2, 'gramos', 0, '', 0.03),
                        ('Jitomate', 0, 11, 'pieza', 0, '', 2.00),
                        ('Lechuga Rabenta', 0, 2, 'gramos', 0, '', 0.05),
                        ('Limón', 0, 11, 'pieza', 0, '', 0.80),
                        ('Pan de Hamburguesa', 0, 14, 'rebanada', 0, '', 3.00),
                        ('Pierna de cerdo', 0, 2, 'gramos', 0, '', 0.30),
                        ('Pollo deshebrado', 0, 2, 'gramos', 0, '', 0.25),
                        ('Queso Americano', 0, 14, 'rebanada', 0, '', 2.50),
                        ('Sal', 0, 15, 'pizca', 0, '', 0.01),
                        ('Tortilla tostada', 0, 11, 'pieza', 0, '', 1.50);

                        INSERT INTO usuario (nombre, rol, pin_acceso, permisos)
                        VALUES ('Dueño Karin', 'Admin', '1234', 'Pedidos,Cuentas,Inventario,Recetas,Reportes');

                        INSERT INTO usuario (nombre, rol, pin_acceso, permisos)
                        VALUES ('Mesero Estrella', 'Mesero', '5678', 'Pedidos,Cuentas');
                        INSERT INTO mesa (numero_mesa, capacidad) VALUES (1, 4), (2, 4), (3, 6), (4, 4), (5, 4), (6, 6), (7, 2), (8, 6);

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
            AsegurarTablaUnidades();
            AsegurarColumnaPermisos();
            AsegurarMesasIniciales();
        }

        public static void AsegurarMesasIniciales()
        {
            if (!File.Exists("karin_pos.db")) return;
            using var con = new SQLiteConnection("Data Source=karin_pos.db;Version=3;");
            con.Open();
            using var cmd = new SQLiteCommand(@"
                INSERT OR IGNORE INTO mesa (numero_mesa, capacidad)
                VALUES (4, 4), (5, 4), (6, 6), (7, 2), (8, 6);", con);
            cmd.ExecuteNonQuery();
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

        /// <summary>
        /// Asegura que la tabla de unidades exista en la base de datos.
        /// Para bases de datos existentes, agrega la tabla y sus datos iniciales.
        /// </summary>
        public static void AsegurarTablaUnidades()
        {
            string nombreArchivo = "karin_pos.db";
            if (!File.Exists(nombreArchivo)) return;

            using (var conexion = new SQLiteConnection($"Data Source={nombreArchivo};Version=3;"))
            {
                conexion.Open();
                using (var pragma = new SQLiteCommand("PRAGMA foreign_keys = ON;", conexion))
                    pragma.ExecuteNonQuery();

                // Crear tabla si no existe
                string sqlCreateTable = @"
CREATE TABLE IF NOT EXISTS tabla_unidades (
    id_unidad INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre TEXT NOT NULL UNIQUE,
    abreviatura TEXT NOT NULL UNIQUE,
    tipo TEXT NOT NULL,
    descripcion TEXT
);";
                using (var cmd = new SQLiteCommand(sqlCreateTable, conexion))
                    cmd.ExecuteNonQuery();

                // Insertar o ignorar unidades iniciales para bases ya existentes
                string sqlInsertUnidades = @"
INSERT OR IGNORE INTO tabla_unidades (nombre, abreviatura, tipo, descripcion) VALUES
('Kilogramo', 'kg', 'Peso', 'Unidad de peso'),
('Gramo', 'g', 'Peso', 'Unidad de peso'),
('Miligramo', 'mg', 'Peso', 'Unidad de peso'),
('Onza', 'oz', 'Peso', 'Unidad de peso'),
('Libra', 'Lb', 'Peso', 'Unidad de peso'),
('Litro', 'Lts', 'Volumen', 'Unidad de volumen'),
('Mililitro', 'mL', 'Volumen', 'Unidad de volumen'),
('Taza', 'cup', 'Volumen', 'Unidad de volumen (aproximadamente 240 mL)'),
('Cucharada', 'tbsp', 'Volumen', 'Unidad de volumen (aproximadamente 15 mL)'),
('Cucharadita', 'tsp', 'Volumen', 'Unidad de volumen (aproximadamente 5 mL)'),
('Pieza', 'pz', 'Cantidad', 'Unidad individual'),
('Docena', 'doc', 'Cantidad', 'Conjunto de 12 piezas'),
('Piezas', 'pcs', 'Cantidad', 'Múltiples unidades individuales'),
('Rebanada', 'rbd', 'Cantidad', 'Porción de algo cortado'),
('Pizca', 'piz', 'Volumen', 'Pequeña cantidad de condimento');";
                using (var cmd2 = new SQLiteCommand(sqlInsertUnidades, conexion))
                    cmd2.ExecuteNonQuery();

                // Agregar columna id_unidad a Insumos si no existe
                try
                {
                    using (var cmd = new SQLiteCommand(
                        "ALTER TABLE Insumos ADD COLUMN id_unidad INTEGER DEFAULT 2;", conexion))
                        cmd.ExecuteNonQuery();
                }
                catch { /* columna ya existe, ignorar */ }
            }
        }

        public static void AsegurarTablaMovimientosInventario()
        {
            if (!File.Exists("karin_pos.db")) return;
            using var con = new SQLiteConnection(ConnectionString);
            con.Open();
            using var cmd = new SQLiteCommand(@"
                CREATE TABLE IF NOT EXISTS inventario_movimientos (
                    id_movimiento INTEGER PRIMARY KEY AUTOINCREMENT,
                    id_cuenta     INTEGER,
                    insumo        TEXT NOT NULL,
                    cantidad      REAL NOT NULL,
                    unidad        TEXT NOT NULL,
                    fecha         DATETIME DEFAULT CURRENT_TIMESTAMP
                );", con);
            cmd.ExecuteNonQuery();
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
CREATE TABLE IF NOT EXISTS tabla_unidades (
    id_unidad INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre TEXT NOT NULL UNIQUE,
    abreviatura TEXT NOT NULL UNIQUE,
    tipo TEXT NOT NULL,
    descripcion TEXT
);
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
);
CREATE TABLE IF NOT EXISTS Insumos (
    id_insumo INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT NOT NULL UNIQUE,
    StockActual REAL DEFAULT 0.00,
    id_unidad INTEGER DEFAULT 2,
    Unidad TEXT DEFAULT 'gramos',
    StockMinimo REAL DEFAULT 0.00,
    FechaEntrada TEXT DEFAULT '',
    Costo REAL DEFAULT 0.00,
    FOREIGN KEY (id_unidad) REFERENCES tabla_unidades(id_unidad)
);";
                using (var comando = new SQLiteCommand(sql, conexion))
                    comando.ExecuteNonQuery();

                // Agrega columnas faltantes en bases con esquema anterior
                foreach (var alter in new[]
                {
                    "ALTER TABLE Insumos ADD COLUMN StockActual REAL DEFAULT 0.00;",
                    "ALTER TABLE Insumos ADD COLUMN StockMinimo REAL DEFAULT 0.00;",
                    "ALTER TABLE Insumos ADD COLUMN FechaEntrada TEXT DEFAULT '';"
                })
                {
                    try { using var a = new SQLiteCommand(alter, conexion); a.ExecuteNonQuery(); }
                    catch { }
                }
            }

            RecetasBaseDatos.SembrarEjemplosSiVacio();
            SembrarInsumosSiVacio();
            AsegurarTablaMovimientosInventario();
        }

        private static void SembrarInsumosSiVacio()
        {
            string nombreArchivo = "karin_pos.db";
            if (!File.Exists(nombreArchivo)) return;

            using var con = new SQLiteConnection($"Data Source={nombreArchivo};Version=3;");
            con.Open();
            using var c0 = new SQLiteCommand("SELECT COUNT(*) FROM Insumos;", con);
            if (Convert.ToInt32((long)c0.ExecuteScalar()!) > 0) return;

            const string insert = @"
INSERT OR IGNORE INTO Insumos (Nombre, StockActual, id_unidad, Unidad, StockMinimo, FechaEntrada, Costo) VALUES
('Aceite vegetal', 0, 7, 'mililitros', 0, '', 0.03),
('Bistek de res', 0, 2, 'gramos', 0, '', 0.65),
('Bolillo', 0, 11, 'pieza', 0, '', 2.00),
('Carne Brioche', 0, 2, 'gramos', 0, '', 0.45),
('Cebolla', 0, 2, 'gramos', 0, '', 0.02),
('Chile serrano', 0, 11, 'pieza', 0, '', 0.50),
('Cilantro', 0, 2, 'gramos', 0, '', 0.03),
('Jitomate', 0, 11, 'pieza', 0, '', 2.00),
('Lechuga Rabenta', 0, 2, 'gramos', 0, '', 0.05),
('Limón', 0, 11, 'pieza', 0, '', 0.80),
('Pan de Hamburguesa', 0, 14, 'rebanada', 0, '', 3.00),
('Pierna de cerdo', 0, 2, 'gramos', 0, '', 0.30),
('Pollo deshebrado', 0, 2, 'gramos', 0, '', 0.25),
('Queso Americano', 0, 14, 'rebanada', 0, '', 2.50),
('Sal', 0, 15, 'pizca', 0, '', 0.01),
('Tortilla tostada', 0, 11, 'pieza', 0, '', 1.50);";
            using var cmd = new SQLiteCommand(insert, con);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Obtiene el reporte de ventas en un rango de fechas.
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio del reporte</param>
        /// <param name="fechaFin">Fecha de fin del reporte</param>
        /// <returns>DataTable con ventas totales, cantidad de órdenes, etc.</returns>
        public static System.Data.DataTable ObtenerReporteVentas(DateTime fechaInicio, DateTime fechaFin)
        {
            var dt = new System.Data.DataTable();
            try
            {
                using (var con = new SQLiteConnection(ConnectionString))
                {
                    con.Open();
                    string sql = @"
                        SELECT 
                            COALESCE(DATE(c.fecha_apertura), @inicio) as Fecha,
                            COUNT(DISTINCT c.id_cuenta) as CantidadOrdenes,
                            ROUND(COALESCE(SUM(CASE WHEN c.estado_cuenta='Cerrada' THEN c.total ELSE 0 END), 0), 2) as VentasCompletadas,
                            ROUND(COALESCE(SUM(CASE WHEN c.estado_cuenta='Abierta' THEN c.total ELSE 0 END), 0), 2) as VentasPendientes,
                            ROUND(COALESCE(SUM(c.total), 0), 2) as VentasTotal
                        FROM cuenta c
                        WHERE DATE(c.fecha_apertura) BETWEEN @inicio AND @fin
                        GROUP BY DATE(c.fecha_apertura)
                        ORDER BY c.fecha_apertura DESC";

                    using (var cmd = new SQLiteCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@inicio", fechaInicio.Date);
                        cmd.Parameters.AddWithValue("@fin", fechaFin.Date);

                        using (var adapter = new System.Data.SQLite.SQLiteDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error al obtener reporte de ventas: {ex.Message}", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            return dt;
        }

        /// <summary>
        /// Obtiene el reporte de productos más vendidos en un rango de fechas.
        /// </summary>
        public static System.Data.DataTable ObtenerReporteProductosMasVendidos(DateTime fechaInicio, DateTime fechaFin)
        {
            var dt = new System.Data.DataTable();
            try
            {
                using (var con = new SQLiteConnection(ConnectionString))
                {
                    con.Open();
                    string sql = @"
                        SELECT 
                            p.nombre as Producto,
                            COALESCE(cat.nombre, 'Sin Categoría') as Categoria,
                            SUM(dc.cantidad) as CantidadVendida,
                            ROUND(SUM(dc.subtotal), 2) as IngresoTotal,
                            ROUND(AVG(dc.precio_unitario), 2) as PrecioPromedio
                        FROM detalle_cuenta dc
                        INNER JOIN producto p ON dc.id_producto = p.id_producto
                        LEFT JOIN categoria cat ON p.id_categoria = cat.id_categoria
                        INNER JOIN cuenta c ON dc.id_cuenta = c.id_cuenta
                        WHERE DATE(c.fecha_apertura) BETWEEN @inicio AND @fin
                              AND c.estado_cuenta = 'Cerrada'
                        GROUP BY p.id_producto
                        ORDER BY CantidadVendida DESC
                        LIMIT 10";

                    using (var cmd = new SQLiteCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@inicio", fechaInicio.Date);
                        cmd.Parameters.AddWithValue("@fin", fechaFin.Date);

                        using (var adapter = new System.Data.SQLite.SQLiteDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error al obtener productos más vendidos: {ex.Message}", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            return dt;
        }

        /// <summary>
        /// Obtiene el reporte de ingresos por empleado en un rango de fechas.
        /// </summary>
        public static System.Data.DataTable ObtenerReporteIngresosPorEmpleado(DateTime fechaInicio, DateTime fechaFin)
        {
            var dt = new System.Data.DataTable();
            try
            {
                using (var con = new SQLiteConnection(ConnectionString))
                {
                    con.Open();
                    string sql = @"
                        SELECT 
                            u.nombre as Empleado,
                            u.rol as Rol,
                            COUNT(DISTINCT c.id_cuenta) as CantidadOrdenes,
                            ROUND(COALESCE(SUM(c.total), 0), 2) as IngresoGenerado
                        FROM cuenta c
                        INNER JOIN usuario u ON c.id_usuario_apertura = u.id_usuario
                        WHERE DATE(c.fecha_apertura) BETWEEN @inicio AND @fin
                              AND c.estado_cuenta = 'Cerrada'
                        GROUP BY u.id_usuario
                        ORDER BY IngresoGenerado DESC";

                    using (var cmd = new SQLiteCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@inicio", fechaInicio.Date);
                        cmd.Parameters.AddWithValue("@fin", fechaFin.Date);

                        using (var adapter = new System.Data.SQLite.SQLiteDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error al obtener ingresos por empleado: {ex.Message}", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            return dt;
        }

        /// <summary>
        /// Obtiene el reporte de consumo de inventario en un rango de fechas.
        /// Consulta la tabla inventario_movimientos para mostrar lo consumido en el período.
        /// </summary>
        public static System.Data.DataTable ObtenerReporteConsumoInventario(DateTime fechaInicio, DateTime fechaFin)
        {
            var dt = new System.Data.DataTable();
            try
            {
                using var con = new SQLiteConnection(ConnectionString);
                con.Open();
                string sql = @"
                    SELECT
                        m.insumo  AS Insumo,
                        m.unidad  AS Unidad,
                        ROUND(SUM(m.cantidad), 4)                              AS Consumido,
                        ROUND(SUM(m.cantidad * COALESCE(i.Costo, 0)), 2)       AS CostoTotal,
                        COALESCE(i.StockActual, 0)                             AS StockActual
                    FROM inventario_movimientos m
                    LEFT JOIN Insumos i ON i.Nombre = m.insumo COLLATE NOCASE
                    WHERE DATE(m.fecha) BETWEEN @inicio AND @fin
                    GROUP BY m.insumo, m.unidad
                    ORDER BY Consumido DESC
                    LIMIT 10;";

                using var cmd = new SQLiteCommand(sql, con);
                cmd.Parameters.AddWithValue("@inicio", fechaInicio.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@fin",    fechaFin.ToString("yyyy-MM-dd"));
                using var adapter = new System.Data.SQLite.SQLiteDataAdapter(cmd);
                adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error al obtener consumo de inventario: {ex.Message}", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            return dt;
        }

        /// <summary>
        /// Valida la conexión a la base de datos.
        /// </summary>
        /// <returns>true si la conexión es exitosa</returns>
        public static bool ValidarConexion()
        {
            try
            {
                using (var con = new SQLiteConnection(ConnectionString))
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand("SELECT 1", con))
                    {
                        cmd.ExecuteScalar();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Inserta datos de prueba para demostración de reportes.
        /// Solo se ejecuta si la tabla está vacía.
        /// </summary>
        public static void SembrarDatosPrueba()
        {
            string nombreArchivo = NombreArchivo;
            if (!File.Exists(nombreArchivo)) return;

            using (var con = new SQLiteConnection(ConnectionString))
            {
                con.Open();

                // Verificar si ya hay datos
                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM cuenta;", con))
                {
                    object result = cmd.ExecuteScalar();
                    long count = result != null ? Convert.ToInt64(result) : 0L;
                    if (count > 0)
                        return;  // Ya hay datos
                }

                try
                {
                    // Insertar usuario de prueba (solo si no existe)
                    using (var cmd = new SQLiteCommand(
                        "INSERT OR IGNORE INTO usuario (nombre, rol, pin_acceso, estado, permisos) VALUES ('Admin Prueba', 'Admin', '1234', 1, 'Pedidos,Cuentas,Inventario,Recetas,Reportes')", con))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Insertar categorías
                    using (var cmd = new SQLiteCommand(
                        "INSERT INTO categoria (nombre, descripcion) VALUES ('Bebidas', 'Bebidas varias'), ('Platos Principales', 'Platos principales'), ('Postres', 'Postres y dulces')", con))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Insertar productos
                    using (var cmd = new SQLiteCommand(@"
                        INSERT INTO producto (nombre, descripcion, precio, id_categoria, disponibilidad) VALUES 
                        ('Agua Fresca', 'Agua de diferentes sabores', 25.00, 1, 1),
                        ('Refresco', 'Refrescos variados', 30.00, 1, 1),
                        ('Tacos', 'Tacos al pastor', 120.00, 2, 1),
                        ('Carne Asada', 'Carne asada con tortillas', 150.00, 2, 1),
                        ('Ensalada', 'Ensalada fresca', 80.00, 2, 1),
                        ('Flan', 'Flan casero', 45.00, 3, 1)", con))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Insertar mesa
                    using (var cmd = new SQLiteCommand(
                        "INSERT INTO mesa (numero_mesa, capacidad, estado) VALUES (1, 4, 'Ocupada')", con))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Insertar cuentas con datos de prueba (últimos 7 días)
                    DateTime hoy = DateTime.Now;
                    for (int i = 0; i < 7; i++)
                    {
                        DateTime fecha = hoy.AddDays(-i);
                        decimal total = 250 + (i * 50);
                        decimal subtotal = total * 0.8m;

                        string sql = $@"
                            INSERT INTO cuenta (id_mesa, id_usuario_apertura, fecha_apertura, fecha_cierre, 
                                              estado_cuenta, tipo_pedido, subtotal, total) 
                            VALUES (1, 1, '{fecha:yyyy-MM-dd HH:mm:ss}', '{fecha.AddHours(2):yyyy-MM-dd HH:mm:ss}', 
                                   'Cerrada', 'Local', {subtotal}, {total})";

                        using (var cmd = new SQLiteCommand(sql, con))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Insertar detalles de cuenta
                    using (var cmd = new SQLiteCommand(@"
                        INSERT INTO detalle_cuenta (id_cuenta, id_producto, cantidad, precio_unitario, subtotal, estado_preparacion) 
                        SELECT 1, 1, 2, 25.00, 50.00, 'Completado'
                        UNION ALL
                        SELECT 1, 3, 3, 120.00, 360.00, 'Completado'
                        UNION ALL
                        SELECT 2, 2, 1, 30.00, 30.00, 'Completado'
                        UNION ALL
                        SELECT 3, 4, 2, 150.00, 300.00, 'Completado'", con))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    System.Windows.Forms.MessageBox.Show(
                        "✅ Datos de prueba insertados correctamente.\nAhora puedes ver los reportes con datos de ejemplo.",
                        "Datos de Prueba", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show($"Error al insertar datos de prueba: {ex.Message}", "Error", 
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
        }
    }
}