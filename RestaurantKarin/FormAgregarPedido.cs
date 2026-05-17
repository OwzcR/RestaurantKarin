using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public class ProductoItem
    {
        public int     Id        { get; set; }
        public string  Nombre    { get; set; } = "";
        public decimal Precio    { get; set; }
        public string  Categoria { get; set; } = "";
        public override string ToString() => Nombre;
    }

    public class DetallePedido
    {
        public int     IdProducto    { get; set; }
        public string  Nombre        { get; set; } = "";
        public int     Cantidad      { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }

    public class FormAgregarPedido : Form
    {
        // ── Datos ─────────────────────────────────────────────────────────────
        private readonly int _idMesa;
        private readonly int _idCuenta;
        private readonly List<DetallePedido> _carrito  = new();
        private List<ProductoItem>           _productos = new();

        // ── Controles ─────────────────────────────────────────────────────────
        private ListBox       lstProductos     = null!;
        private ListBox       lstCarrito       = null!;
        private Label         lblTotal         = null!;
        private NumericUpDown numCantidad      = null!;
        private Label         lblNombreProducto = null!;
        private Label         lblPrecioProducto = null!;
        private TextBox       txtNotas         = null!;
        private TextBox       txtBuscar        = null!;
        private Label         lblSinResultados = null!;

        // ── Paleta ────────────────────────────────────────────────────────────
        private static readonly Color BgDark     = Color.FromArgb(217, 217, 217);
        private static readonly Color CardBg     = Color.FromArgb(232, 232, 232);
        private static readonly Color ItemBg     = Color.White;
        private static readonly Color TextDark   = Color.FromArgb(20, 20, 20);
        private static readonly Color BtnQuitar  = Color.FromArgb(128, 128, 128);
        private static readonly Color BtnConfirm = Color.FromArgb(14, 77, 120);
        private static readonly Color BtnAgregar = Color.FromArgb(14, 77, 120);

        private const string Conn = "Data Source=karin_pos.db;Version=3;";

        // ─────────────────────────────────────────────────────────────────────
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        public FormAgregarPedido(int idMesa, int idCuenta)
        {
            _idMesa   = idMesa;
            _idCuenta = idCuenta;
            BuildUI();
            CargarProductos();

            HandleCreated += (_, _) =>
            {
                int pref = 2; // DWMWCP_ROUND — esquinas redondeadas nativas Win11
                DwmSetWindowAttribute(Handle, 33, ref pref, 4);
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        //  UI
        // ─────────────────────────────────────────────────────────────────────
        private void BuildUI()
        {
            Text            = $"Agregar Pedido — Mesa {_idMesa}";
            Size            = new Size(920, 590);
            MinimumSize     = new Size(920, 590);
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = BgDark;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;

            // ── Título ────────────────────────────────────────────────────────
            var lblTitulo = new Label
            {
                Text      = $"🛒  Pedido — Mesa {_idMesa}",
                Font      = new Font("Sansation", 14, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize  = true,
                Location  = new Point(16, 14),
                BackColor = Color.Transparent
            };

            // ══════════════════════════════════════════════════════════════════
            //  PANEL IZQUIERDO — Catálogo de recetas
            // ══════════════════════════════════════════════════════════════════
            var panelIzq = new Panel
            {
                Location  = new Point(12, 50),
                Size      = new Size(450, 490),
                BackColor = CardBg
            };
            ApplyRound(panelIzq, 18);

            var lblCat = MakeLbl("Buscar producto :", 12, 12);
            txtBuscar = new TextBox
            {
                Location        = new Point(12, 32),
                Size            = new Size(426, 26),
                Font            = new Font("Sansation", 9),
                BackColor       = ItemBg,
                ForeColor       = TextDark,
                BorderStyle     = BorderStyle.None,
                PlaceholderText = "🔍  Escribe el nombre del platillo..."
            };
            txtBuscar.TextChanged += (_, _) => FiltrarProductos(txtBuscar.Text);

            lblSinResultados = new Label
            {
                Text      = "⚠  Sin resultados para esa búsqueda.",
                Font      = new Font("Sansation", 9, FontStyle.Italic),
                ForeColor = Color.FromArgb(180, 0, 0),
                AutoSize  = true,
                Location  = new Point(12, 62),
                BackColor = Color.Transparent,
                Visible   = false
            };

            var lblProd = MakeLbl("Productos :", 12, 64);
            lstProductos = new ListBox
            {
                Location      = new Point(12, 84),
                Size          = new Size(426, 190),
                BackColor     = ItemBg,
                ForeColor     = TextDark,
                Font          = new Font("Sansation", 10),
                BorderStyle   = BorderStyle.None,
                SelectionMode = SelectionMode.One
            };
            lstProductos.SelectedIndexChanged += OnProductoSeleccionado;
            ApplyRoundCtrl(lstProductos, 8);

            lblNombreProducto = new Label
            {
                Text      = "Selecciona un producto",
                Font      = new Font("Sansation", 10, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize  = true,
                Location  = new Point(12, 284),
                BackColor = Color.Transparent
            };

            lblPrecioProducto = new Label
            {
                Text      = "",
                Font      = new Font("Sansation", 9),
                ForeColor = Color.FromArgb(60, 60, 60),
                AutoSize  = true,
                Location  = new Point(12, 308),
                BackColor = Color.Transparent
            };

            var lblCant = MakeLbl("Cantidad :", 12, 340);
            numCantidad = new NumericUpDown
            {
                Location    = new Point(12, 360),
                Size        = new Size(80, 28),
                Minimum     = 1,
                Maximum     = 99,
                Value       = 1,
                Font        = new Font("Sansation", 10),
                BackColor   = ItemBg,
                ForeColor   = TextDark,
                BorderStyle = BorderStyle.None
            };

            var lblNota = MakeLbl("Notas (opcional) :", 110, 340);
            txtNotas = new TextBox
            {
                Location        = new Point(110, 360),
                Size            = new Size(328, 28),
                Font            = new Font("Sansation", 9),
                BackColor       = ItemBg,
                ForeColor       = TextDark,
                BorderStyle     = BorderStyle.None,
                PlaceholderText = "Sin cebolla, extra salsa..."
            };

            var btnAgregar = MakeBtn("➕  Agregar al carrito", BtnAgregar,
                                     new Point(12, 410), new Size(426, 40));
            btnAgregar.Click += OnAgregarAlCarrito;

            panelIzq.Controls.AddRange(new Control[]
            {
                lblCat, txtBuscar, lblSinResultados,
                lblProd, lstProductos,
                lblNombreProducto, lblPrecioProducto,
                lblCant, numCantidad,
                lblNota, txtNotas,
                btnAgregar
            });

            // ══════════════════════════════════════════════════════════════════
            //  PANEL DERECHO — Carrito
            // ══════════════════════════════════════════════════════════════════
            var panelDer = new Panel
            {
                Location  = new Point(476, 50),
                Size      = new Size(432, 490),
                BackColor = CardBg
            };
            ApplyRound(panelDer, 18);

            var lblCarritoTit = new Label
            {
                Text      = "Carrito de pedido :",
                Font      = new Font("Sansation", 11, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize  = true,
                Location  = new Point(12, 12),
                BackColor = Color.Transparent
            };

            lstCarrito = new ListBox
            {
                Location      = new Point(12, 36),
                Size          = new Size(408, 288),
                BackColor     = ItemBg,
                ForeColor     = TextDark,
                Font          = new Font("Sansation", 9),
                BorderStyle   = BorderStyle.None,
                SelectionMode = SelectionMode.One
            };

            ApplyRoundCtrl(lstCarrito, 8);

            var btnQuitar = MakeBtn("🗑  Quitar seleccionado", BtnQuitar,
                                    new Point(12, 334), new Size(408, 38));
            btnQuitar.Click += OnQuitarDelCarrito;

            lblTotal = new Label
            {
                Text      = "Total : $0.00",
                Font      = new Font("Sansation", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(14, 77, 120),
                AutoSize  = false,
                Size      = new Size(408, 30),
                Location  = new Point(12, 382),
                BackColor = Color.Transparent
            };

            var btnConfirmar = MakeBtn("✅  Confirmar Pedido", BtnConfirm,
                                       new Point(12, 420), new Size(408, 42));
            btnConfirmar.Click += OnConfirmarPedido;

            panelDer.Controls.AddRange(new Control[]
            {
                lblCarritoTit, lstCarrito,
                btnQuitar, lblTotal, btnConfirmar
            });

            // ── Ensamblar ─────────────────────────────────────────────────────
            Controls.AddRange(new Control[] { lblTitulo, panelIzq, panelDer });
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Carga de datos — fuente: tabla receta
        // ─────────────────────────────────────────────────────────────────────
        private void CargarProductos(string? filtro = null)
        {
            _productos.Clear();
            lstProductos.Items.Clear();

            try
            {
                using var con = new SQLiteConnection(Conn);
                con.Open();

                const string sql = @"
                    SELECT id_receta, nombre, costo_por_porcion
                    FROM   receta
                    ORDER  BY nombre COLLATE NOCASE;";

                using var cmd = new SQLiteCommand(sql, con);
                using var r   = cmd.ExecuteReader();
                while (r.Read())
                {
                    var p = new ProductoItem
                    {
                        Id        = Convert.ToInt32(r["id_receta"]),
                        Nombre    = r["nombre"].ToString()!,
                        Precio    = Convert.ToDecimal(r["costo_por_porcion"]),
                        Categoria = "Recetas"
                    };

                    if (string.IsNullOrWhiteSpace(filtro) ||
                        p.Nombre.Contains(filtro.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        _productos.Add(p);
                        lstProductos.Items.Add($"{p.Nombre}  —  {p.Precio:C}");
                    }
                }
            }
            catch { /* DB no disponible: lista queda vacía */ }
        }

        private void FiltrarProductos(string query)
        {
            CargarProductos(query);

            bool sinResultados = _productos.Count == 0 && !string.IsNullOrWhiteSpace(query);
            lblSinResultados.Visible = sinResultados;
            txtBuscar.BackColor      = sinResultados ? Color.FromArgb(255, 220, 220) : ItemBg;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Eventos
        // ─────────────────────────────────────────────────────────────────────
        private void OnProductoSeleccionado(object? sender, EventArgs e)
        {
            int idx = lstProductos.SelectedIndex;
            if (idx < 0 || idx >= _productos.Count) return;

            var p = _productos[idx];
            lblNombreProducto.Text = p.Nombre;
            lblPrecioProducto.Text = $"Precio unitario : {p.Precio:C}  |  Categoría : {p.Categoria}";
        }

        private void OnAgregarAlCarrito(object? sender, EventArgs e)
        {
            int idx = lstProductos.SelectedIndex;
            if (idx < 0 || idx >= _productos.Count)
            {
                MessageBox.Show("Selecciona un producto primero.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var prod  = _productos[idx];
            int cant  = (int)numCantidad.Value;
            string nota = txtNotas.Text.Trim();

            var existente = _carrito.Find(d => d.IdProducto == prod.Id);
            if (existente != null)
                existente.Cantidad += cant;
            else
                _carrito.Add(new DetallePedido
                {
                    IdProducto     = prod.Id,
                    Nombre         = prod.Nombre + (nota != "" ? $" ({nota})" : ""),
                    Cantidad       = cant,
                    PrecioUnitario = prod.Precio
                });

            RefreshCarrito();
            numCantidad.Value = 1;
            txtNotas.Clear();
        }

        private void OnQuitarDelCarrito(object? sender, EventArgs e)
        {
            int idx = lstCarrito.SelectedIndex;
            if (idx < 0 || idx >= _carrito.Count) return;
            _carrito.RemoveAt(idx);
            RefreshCarrito();
        }

        private void OnConfirmarPedido(object? sender, EventArgs e)
        {
            if (_carrito.Count == 0)
            {
                MessageBox.Show("El carrito está vacío.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                GuardarEnBD();
                MessageBox.Show($"✅ Pedido guardado correctamente.\nTotal : {TotalCarrito():C}",
                    "Pedido Confirmado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el pedido:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Guardar en BD
        // ─────────────────────────────────────────────────────────────────────
        private void GuardarEnBD()
        {
            using var con  = new SQLiteConnection(Conn);
            con.Open();
            using var tran = con.BeginTransaction();

            try
            {
                foreach (var d in _carrito)
                {
                    // Obtiene o crea la entrada en 'producto' para satisfacer la FK
                    string nombreBase = d.Nombre.Contains('(')
                        ? d.Nombre[..d.Nombre.IndexOf('(')].Trim()
                        : d.Nombre;
                    int idProducto = SyncProducto(nombreBase, d.PrecioUnitario, con, tran);

                    using var cmd = new SQLiteCommand(@"
                        INSERT INTO detalle_cuenta
                            (id_cuenta, id_producto, cantidad, precio_unitario, subtotal, notas, estado_preparacion)
                        VALUES
                            (@cuenta, @prod, @cant, @precio, @sub, @notas, 'Pendiente');",
                        con, tran);

                    cmd.Parameters.AddWithValue("@cuenta", _idCuenta);
                    cmd.Parameters.AddWithValue("@prod",   idProducto);
                    cmd.Parameters.AddWithValue("@cant",   d.Cantidad);
                    cmd.Parameters.AddWithValue("@precio", d.PrecioUnitario);
                    cmd.Parameters.AddWithValue("@sub",    d.Subtotal);
                    cmd.Parameters.AddWithValue("@notas",  d.Nombre.Contains('(') ? d.Nombre : "");
                    cmd.ExecuteNonQuery();

                    DescontarInventario(nombreBase, d.Cantidad, _idCuenta, con, tran);
                }

                decimal nuevoSubtotal;
                using (var cmdSum = new SQLiteCommand(
                    "SELECT COALESCE(SUM(subtotal),0) FROM detalle_cuenta WHERE id_cuenta = @id;",
                    con, tran))
                {
                    cmdSum.Parameters.AddWithValue("@id", _idCuenta);
                    nuevoSubtotal = Convert.ToDecimal(cmdSum.ExecuteScalar());
                }

                using (var cmdUpd = new SQLiteCommand(@"
                    UPDATE cuenta
                    SET subtotal = @sub, total = @tot
                    WHERE id_cuenta = @id;", con, tran))
                {
                    cmdUpd.Parameters.AddWithValue("@sub", nuevoSubtotal);
                    cmdUpd.Parameters.AddWithValue("@tot", nuevoSubtotal);
                    cmdUpd.Parameters.AddWithValue("@id",  _idCuenta);
                    cmdUpd.ExecuteNonQuery();
                }

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        // Descuenta los insumos de inventario según las líneas de la receta y registra el movimiento.
        // porciones: cuántas porciones se vendieron en este ítem del carrito.
        private static void DescontarInventario(string nombreReceta, int porciones, int idCuenta,
                                                SQLiteConnection con, SQLiteTransaction tran)
        {
            // Obtener id_receta y porciones_receta
            int     idReceta       = -1;
            decimal porcionesTotal = 1m;

            using (var cmd = new SQLiteCommand(
                "SELECT id_receta, porciones FROM receta WHERE nombre = @n COLLATE NOCASE LIMIT 1;",
                con, tran))
            {
                cmd.Parameters.AddWithValue("@n", nombreReceta);
                using var r = cmd.ExecuteReader();
                if (!r.Read()) return;
                idReceta       = Convert.ToInt32(r["id_receta"]);
                porcionesTotal = Convert.ToDecimal(r["porciones"]);
                if (porcionesTotal <= 0m) porcionesTotal = 1m;
            }

            // Leer líneas de ingredientes
            var lineas = new List<(string Insumo, decimal Cantidad, string Unidad)>();
            using (var cmd = new SQLiteCommand(
                "SELECT insumo, cantidad, unidad FROM receta_linea WHERE id_receta = @id;",
                con, tran))
            {
                cmd.Parameters.AddWithValue("@id", idReceta);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                    lineas.Add((r["insumo"].ToString()!, Convert.ToDecimal(r["cantidad"]), r["unidad"].ToString()!));
            }

            foreach (var (insumo, cantidadReceta, unidadReceta) in lineas)
            {
                // Cantidad a descontar: proporción por porción × porciones vendidas
                decimal totalConsumo = cantidadReceta / porcionesTotal * porciones;

                // Leer unidad del insumo en inventario
                string unidadInsumo;
                using (var cmd = new SQLiteCommand(
                    "SELECT Unidad FROM Insumos WHERE Nombre = @n COLLATE NOCASE LIMIT 1;",
                    con, tran))
                {
                    cmd.Parameters.AddWithValue("@n", insumo);
                    var result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value) continue;
                    unidadInsumo = result.ToString()!;
                }

                // Convertir si las unidades difieren
                if (!string.Equals(unidadReceta.Trim(), unidadInsumo.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    decimal? convertido = UnitConverter.Convert(totalConsumo, unidadReceta, unidadInsumo);
                    if (convertido == null) continue; // unidades incompatibles, omitir
                    totalConsumo = convertido.Value;
                }

                // Descontar del stock (mínimo 0)
                using (var upd = new SQLiteCommand(@"
                    UPDATE Insumos
                    SET StockActual = MAX(0, StockActual - @qty)
                    WHERE Nombre = @n COLLATE NOCASE;", con, tran))
                {
                    upd.Parameters.AddWithValue("@qty", totalConsumo);
                    upd.Parameters.AddWithValue("@n",   insumo);
                    upd.ExecuteNonQuery();
                }

                // Registrar movimiento para reportes
                using var mov = new SQLiteCommand(@"
                    INSERT INTO inventario_movimientos (id_cuenta, insumo, cantidad, unidad, fecha)
                    VALUES (@cuenta, @insumo, @qty, @unidad, CURRENT_TIMESTAMP);", con, tran);
                mov.Parameters.AddWithValue("@cuenta", idCuenta);
                mov.Parameters.AddWithValue("@insumo", insumo);
                mov.Parameters.AddWithValue("@qty",    totalConsumo);
                mov.Parameters.AddWithValue("@unidad", unidadInsumo);
                mov.ExecuteNonQuery();
            }
        }

        // Busca en 'producto' por nombre; si no existe lo crea y devuelve su id.
        private static int SyncProducto(string nombre, decimal precio,
                                         SQLiteConnection con, SQLiteTransaction tran)
        {
            using (var sel = new SQLiteCommand(
                "SELECT id_producto FROM producto WHERE nombre = @n LIMIT 1;", con, tran))
            {
                sel.Parameters.AddWithValue("@n", nombre);
                var found = sel.ExecuteScalar();
                if (found != null && found != DBNull.Value)
                    return Convert.ToInt32(found);
            }

            using (var ins = new SQLiteCommand(
                "INSERT INTO producto (nombre, precio, disponibilidad) VALUES (@n, @p, 1);",
                con, tran))
            {
                ins.Parameters.AddWithValue("@n", nombre);
                ins.Parameters.AddWithValue("@p", precio);
                ins.ExecuteNonQuery();
            }

            using var last = new SQLiteCommand("SELECT last_insert_rowid();", con, tran);
            return Convert.ToInt32((long)last.ExecuteScalar()!);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Helpers UI
        // ─────────────────────────────────────────────────────────────────────
        private void RefreshCarrito()
        {
            lstCarrito.Items.Clear();
            foreach (var d in _carrito)
                lstCarrito.Items.Add($"x{d.Cantidad}  {d.Nombre}  —  {d.Subtotal:C}");

            lblTotal.Text = $"Total : {TotalCarrito():C}";
        }

        private decimal TotalCarrito()
        {
            decimal t = 0;
            foreach (var d in _carrito) t += d.Subtotal;
            return t;
        }

        private static Label MakeLbl(string text, int x, int y) => new()
        {
            Text      = text,
            Font      = new Font("Sansation", 9),
            ForeColor = Color.FromArgb(20, 20, 20),
            AutoSize  = true,
            Location  = new Point(x, y),
            BackColor = Color.Transparent
        };

        private static Button MakeBtn(string text, Color bg, Point loc, Size size)
        {
            var btn = new Button
            {
                Text      = text,
                Font      = new Font("Sansation", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = bg,
                FlatStyle = FlatStyle.Flat,
                Location  = loc,
                Size      = size,
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            ApplyRoundBtn(btn, 14);
            return btn;
        }

        private static void ApplyRoundBtn(Button btn, int radius) =>
            ApplyRoundCtrl(btn, radius);

        private static GraphicsPath MakeRoundPath(Rectangle rect, int r)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.Left,           rect.Top,            r * 2, r * 2, 180, 90);
            path.AddArc(rect.Right - r * 2,  rect.Top,            r * 2, r * 2, 270, 90);
            path.AddArc(rect.Right - r * 2,  rect.Bottom - r * 2, r * 2, r * 2,   0, 90);
            path.AddArc(rect.Left,           rect.Bottom - r * 2, r * 2, r * 2,  90, 90);
            path.CloseFigure();
            return path;
        }

        private static void ApplyRound(Control ctrl, int r)
        {
            void ApplyRegion()
            {
                if (ctrl.Width <= 0 || ctrl.Height <= 0) return;
                using var path = MakeRoundPath(new Rectangle(0, 0, ctrl.Width, ctrl.Height), r);
                ctrl.Region = new Region(path);
            }
            ApplyRegion();
            ctrl.Resize += (_, _) => ApplyRegion();
            ctrl.Paint  += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = MakeRoundPath(new Rectangle(0, 0, ctrl.Width - 1, ctrl.Height - 1), r);
                using var fill = new SolidBrush(ctrl.BackColor);
                e.Graphics.FillPath(fill, path);
            };
        }

        private static void ApplyRoundCtrl(Control ctrl, int radius)
        {
            void Apply()
            {
                if (ctrl.Width <= 0 || ctrl.Height <= 0) return;
                using var path = MakeRoundPath(new Rectangle(0, 0, ctrl.Width, ctrl.Height), radius);
                ctrl.Region = new Region(path);
            }
            Apply();
            ctrl.Resize += (_, _) => Apply();
        }
    }
}
