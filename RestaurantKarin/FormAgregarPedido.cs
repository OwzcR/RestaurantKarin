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
        private ComboBox      cmbCategoria     = null!;

        // ── Paleta ────────────────────────────────────────────────────────────
        private static readonly Color BgDark     = Color.FromArgb(10, 35, 70);
        private static readonly Color CardBg     = Color.FromArgb(20, 58, 110);
        private static readonly Color ItemBg     = Color.FromArgb(30, 75, 135);
        private static readonly Color BtnQuitar  = Color.FromArgb(0, 35, 55);    // #002337
        private static readonly Color BtnConfirm = Color.FromArgb(0, 151, 167);  // #0097A7
        private static readonly Color BtnAgregar = Color.FromArgb(0, 151, 167);  // mismo teal

        private const string Conn = "Data Source=karin_pos.db;Version=3;";

        // ─────────────────────────────────────────────────────────────────────
        public FormAgregarPedido(int idMesa, int idCuenta)
        {
            _idMesa   = idMesa;
            _idCuenta = idCuenta;
            BuildUI();
            CargarProductos();
            CargarCategoriasCombo();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  UI
        // ─────────────────────────────────────────────────────────────────────
        private void BuildUI()
        {
            Text            = $"Agregar Pedido — Mesa {_idMesa}";
            Size            = new Size(860, 590);
            MinimumSize     = new Size(860, 590);
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = BgDark;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;

            // ── Título ────────────────────────────────────────────────────────
            var lblTitulo = new Label
            {
                Text      = $"🛒  Pedido — Mesa {_idMesa}",
                Font      = new Font("Sansation", 14, FontStyle.Bold),
                ForeColor = Color.White,
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
                Size      = new Size(390, 490),
                BackColor = CardBg
            };
            ApplyRound(panelIzq, 10);

            var lblCat = MakeLbl("Categoría :", 12, 12);
            cmbCategoria = new ComboBox
            {
                Location      = new Point(12, 32),
                Size          = new Size(366, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Sansation", 9),
                BackColor     = ItemBg,
                ForeColor     = Color.White,
                FlatStyle     = FlatStyle.Flat
            };
            cmbCategoria.SelectedIndexChanged += (_, _) => FiltrarPorCategoria();

            var lblProd = MakeLbl("Productos :", 12, 64);
            lstProductos = new ListBox
            {
                Location      = new Point(12, 84),
                Size          = new Size(366, 190),
                BackColor     = ItemBg,
                ForeColor     = Color.White,
                Font          = new Font("Sansation", 10),
                BorderStyle   = BorderStyle.None,
                SelectionMode = SelectionMode.One
            };
            lstProductos.SelectedIndexChanged += OnProductoSeleccionado;

            lblNombreProducto = new Label
            {
                Text      = "Selecciona un producto",
                Font      = new Font("Sansation", 10, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(12, 284),
                BackColor = Color.Transparent
            };

            lblPrecioProducto = new Label
            {
                Text      = "",
                Font      = new Font("Sansation", 9),
                ForeColor = Color.FromArgb(160, 220, 230),
                AutoSize  = true,
                Location  = new Point(12, 308),
                BackColor = Color.Transparent
            };

            var lblCant = MakeLbl("Cantidad :", 12, 340);
            numCantidad = new NumericUpDown
            {
                Location  = new Point(12, 360),
                Size      = new Size(80, 28),
                Minimum   = 1,
                Maximum   = 99,
                Value     = 1,
                Font      = new Font("Sansation", 10),
                BackColor = ItemBg,
                ForeColor = Color.White
            };

            var lblNota = MakeLbl("Notas (opcional) :", 110, 340);
            txtNotas = new TextBox
            {
                Location        = new Point(110, 360),
                Size            = new Size(268, 28),
                Font            = new Font("Sansation", 9),
                BackColor       = ItemBg,
                ForeColor       = Color.White,
                BorderStyle     = BorderStyle.FixedSingle,
                PlaceholderText = "Sin cebolla, extra salsa..."
            };

            var btnAgregar = MakeBtn("➕  Agregar al carrito", BtnAgregar,
                                     new Point(12, 410), new Size(366, 40));
            btnAgregar.Click += OnAgregarAlCarrito;

            panelIzq.Controls.AddRange(new Control[]
            {
                lblCat, cmbCategoria,
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
                Location  = new Point(416, 50),
                Size      = new Size(418, 490),
                BackColor = CardBg
            };
            ApplyRound(panelDer, 10);

            var lblCarritoTit = new Label
            {
                Text      = "Carrito de pedido :",
                Font      = new Font("Sansation", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(200, 220, 255),
                AutoSize  = true,
                Location  = new Point(12, 12),
                BackColor = Color.Transparent
            };

            lstCarrito = new ListBox
            {
                Location      = new Point(12, 36),
                Size          = new Size(394, 288),
                BackColor     = ItemBg,
                ForeColor     = Color.White,
                Font          = new Font("Sansation", 9),
                BorderStyle   = BorderStyle.None,
                SelectionMode = SelectionMode.One
            };

            var btnQuitar = MakeBtn("🗑  Quitar seleccionado", BtnQuitar,
                                    new Point(12, 334), new Size(394, 38));
            btnQuitar.Click += OnQuitarDelCarrito;

            lblTotal = new Label
            {
                Text      = "Total : $0.00",
                Font      = new Font("Sansation", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 220, 200),
                AutoSize  = false,
                Size      = new Size(394, 30),
                Location  = new Point(12, 382),
                BackColor = Color.Transparent
            };

            var btnConfirmar = MakeBtn("✅  Confirmar Pedido", BtnConfirm,
                                       new Point(12, 420), new Size(394, 42));
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
                        Id       = Convert.ToInt32(r["id_receta"]),
                        Nombre   = r["nombre"].ToString()!,
                        Precio   = Convert.ToDecimal(r["costo_por_porcion"]),
                        Categoria = "Recetas"
                    };
                    _productos.Add(p);
                    lstProductos.Items.Add($"{p.Nombre}  —  {p.Precio:C}");
                }
            }
            catch { /* DB no disponible: lista queda vacía */ }
        }

        private void CargarCategoriasCombo()
        {
            cmbCategoria.Items.Clear();
            cmbCategoria.Items.Add("Todas");
            cmbCategoria.SelectedIndex = 0;
        }

        private void FiltrarPorCategoria() => CargarProductos();

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
            ForeColor = Color.FromArgb(190, 215, 255),
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
            return btn;
        }

        private static void ApplyRound(Control ctrl, int r)
        {
            ctrl.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = new GraphicsPath();
                var rect = new Rectangle(0, 0, ctrl.Width - 1, ctrl.Height - 1);
                path.AddArc(rect.Left,              rect.Top,             r * 2, r * 2, 180, 90);
                path.AddArc(rect.Right  - r * 2,    rect.Top,             r * 2, r * 2, 270, 90);
                path.AddArc(rect.Right  - r * 2,    rect.Bottom - r * 2,  r * 2, r * 2, 0,   90);
                path.AddArc(rect.Left,              rect.Bottom - r * 2,  r * 2, r * 2, 90,  90);
                path.CloseFigure();
                using var fill = new SolidBrush(ctrl.BackColor);
                e.Graphics.FillPath(fill, path);
            };
        }
    }
}
