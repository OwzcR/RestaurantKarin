using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public class ProductoItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public decimal Precio { get; set; }
        public string Categoria { get; set; } = "";
        public override string ToString() => Nombre;
    }

    public class DetallePedido
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }

    public class FormAgregarPedido : Form
    {
        // ── Datos ─────────────────────────────────────────────────────────────
        private readonly int _idMesa;
        private readonly int _idCuenta;
        private readonly List<DetallePedido> _carrito = new();
        private List<ProductoItem> _productos = new();

        // ── Controles ─────────────────────────────────────────────────────────
        private ListBox lstProductos = null!;
        private ListBox lstCarrito = null!;
        private Label lblTotal = null!;
        private NumericUpDown numCantidad = null!;
        private Label lblNombreProducto = null!;
        private Label lblPrecioProducto = null!;
        private TextBox txtNotas = null!;
        private ComboBox cmbCategoria = null!;

        // ── Colores ───────────────────────────────────────────────────────────
        private static readonly Color BgDark = Color.FromArgb(26, 58, 107);
        private static readonly Color AccentTeal = Color.FromArgb(0, 137, 123);
        private static readonly Color AccentRed = Color.FromArgb(183, 28, 28);
        private static readonly Color CardBg = Color.FromArgb(33, 72, 127);

        private const string Conn = "Data Source=karin_pos.db;Version=3;";

        // ─────────────────────────────────────────────────────────────────────
        public FormAgregarPedido(int idMesa, int idCuenta)
        {
            _idMesa = idMesa;
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
            Text = $"Agregar Pedido — Mesa {_idMesa}";
            Size = new Size(860, 580);
            MinimumSize = new Size(860, 580);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = BgDark;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            // ── Título ────────────────────────────────────────────────────────
            var lblTitulo = new Label
            {
                Text = $"🛒  Pedido — Mesa {_idMesa}",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(16, 14)
            };

            // ══════════════════════════════════════════════════════════════════
            //  PANEL IZQUIERDO — Catálogo de productos
            // ══════════════════════════════════════════════════════════════════
            var panelIzq = new Panel
            {
                Location = new Point(12, 50),
                Size = new Size(390, 460),
                BackColor = CardBg
            };
            ApplyRound(panelIzq, 10);

            var lblCat = MakeLbl("Categoría :", 12, 12);
            cmbCategoria = new ComboBox
            {
                Location = new Point(12, 34),
                Size = new Size(366, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(44, 90, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            cmbCategoria.SelectedIndexChanged += (_, _) => FiltrarPorCategoria();

            var lblProd = MakeLbl("Productos :", 12, 66);
            lstProductos = new ListBox
            {
                Location = new Point(12, 86),
                Size = new Size(366, 180),
                BackColor = Color.FromArgb(44, 90, 150),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.None,
                SelectionMode = SelectionMode.One
            };
            lstProductos.SelectedIndexChanged += OnProductoSeleccionado;

            // Info del producto seleccionado
            lblNombreProducto = MakeLbl("Selecciona un producto", 12, 278);
            lblNombreProducto.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblNombreProducto.ForeColor = Color.White;
            lblNombreProducto.AutoSize = true;

            lblPrecioProducto = MakeLbl("", 12, 300);
            lblPrecioProducto.Font = new Font("Segoe UI", 9);
            lblPrecioProducto.ForeColor = Color.FromArgb(180, 230, 230);
            lblPrecioProducto.AutoSize = true;

            var lblCant = MakeLbl("Cantidad :", 12, 330);
            numCantidad = new NumericUpDown
            {
                Location = new Point(12, 350),
                Size = new Size(80, 28),
                Minimum = 1,
                Maximum = 99,
                Value = 1,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(44, 90, 150),
                ForeColor = Color.White
            };

            var lblNota = MakeLbl("Notas (opcional) :", 110, 330);
            txtNotas = new TextBox
            {
                Location = new Point(110, 350),
                Size = new Size(268, 28),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(44, 90, 150),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Sin cebolla, extra salsa..."
            };

            var btnAgregar = MakeBtn("➕  Agregar al carrito", AccentTeal, new Point(12, 396), new Size(366, 38));
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
                Location = new Point(416, 50),
                Size = new Size(418, 460),
                BackColor = CardBg
            };
            ApplyRound(panelDer, 10);

            var lblCarritoTit = MakeLbl("Carrito de pedido :", 12, 12);
            lblCarritoTit.Font = new Font("Segoe UI", 11, FontStyle.Bold);

            lstCarrito = new ListBox
            {
                Location = new Point(12, 36),
                Size = new Size(394, 310),
                BackColor = Color.FromArgb(44, 90, 150),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.None,
                SelectionMode = SelectionMode.One
            };

            var btnQuitar = MakeBtn("🗑  Quitar seleccionado", AccentRed, new Point(12, 354), new Size(394, 34));
            btnQuitar.Click += OnQuitarDelCarrito;

            lblTotal = new Label
            {
                Text = "Total : $0.00",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 220, 180),
                AutoSize = true,
                Location = new Point(12, 396)
            };

            var btnConfirmar = MakeBtn("✅  Confirmar Pedido", Color.FromArgb(30, 120, 60),
                                       new Point(12, 418), new Size(394, 38));
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
        //  Carga de datos
        // ─────────────────────────────────────────────────────────────────────
        private void CargarProductos(string? categoria = null)
        {
            _productos.Clear();
            lstProductos.Items.Clear();

            using var con = new SQLiteConnection(Conn);
            con.Open();

            string sql = categoria == null || categoria == "Todas"
                ? @"SELECT p.id_producto, p.nombre, p.precio, c.nombre as cat
                    FROM producto p
                    LEFT JOIN categoria c ON c.id_categoria = p.id_categoria
                    WHERE p.disponibilidad = 1
                    ORDER BY c.nombre, p.nombre;"
                : @"SELECT p.id_producto, p.nombre, p.precio, c.nombre as cat
                    FROM producto p
                    LEFT JOIN categoria c ON c.id_categoria = p.id_categoria
                    WHERE p.disponibilidad = 1 AND c.nombre = @cat
                    ORDER BY p.nombre;";

            using var cmd = new SQLiteCommand(sql, con);
            if (categoria != null && categoria != "Todas")
                cmd.Parameters.AddWithValue("@cat", categoria);

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var p = new ProductoItem
                {
                    Id = Convert.ToInt32(r["id_producto"]),
                    Nombre = r["nombre"].ToString()!,
                    Precio = Convert.ToDecimal(r["precio"]),
                    Categoria = r["cat"]?.ToString() ?? ""
                };
                _productos.Add(p);
                lstProductos.Items.Add($"{p.Nombre}  —  {p.Precio:C}");
            }
        }

        private void CargarCategoriasCombo()
        {
            cmbCategoria.Items.Clear();
            cmbCategoria.Items.Add("Todas");

            using var con = new SQLiteConnection(Conn);
            con.Open();
            using var cmd = new SQLiteCommand("SELECT nombre FROM categoria ORDER BY nombre;", con);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                cmbCategoria.Items.Add(r["nombre"].ToString()!);

            cmbCategoria.SelectedIndex = 0;
        }

        private void FiltrarPorCategoria()
        {
            string? cat = cmbCategoria.SelectedItem?.ToString();
            CargarProductos(cat);
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

            var prod = _productos[idx];
            int cant = (int)numCantidad.Value;
            string nota = txtNotas.Text.Trim();

            // Si ya está en el carrito, aumenta cantidad
            var existente = _carrito.Find(d => d.IdProducto == prod.Id);
            if (existente != null)
            {
                existente.Cantidad += cant;
            }
            else
            {
                _carrito.Add(new DetallePedido
                {
                    IdProducto = prod.Id,
                    Nombre = prod.Nombre + (nota != "" ? $" ({nota})" : ""),
                    Cantidad = cant,
                    PrecioUnitario = prod.Precio
                });
            }

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
            using var con = new SQLiteConnection(Conn);
            con.Open();
            using var tran = con.BeginTransaction();

            try
            {
                // 1. Insertar cada línea en detalle_cuenta
                foreach (var d in _carrito)
                {
                    using var cmd = new SQLiteCommand(@"
                        INSERT INTO detalle_cuenta
                            (id_cuenta, id_producto, cantidad, precio_unitario, subtotal, notas, estado_preparacion)
                        VALUES
                            (@cuenta, @prod, @cant, @precio, @sub, @notas, 'Pendiente');",
                        con, tran);

                    cmd.Parameters.AddWithValue("@cuenta", _idCuenta);
                    cmd.Parameters.AddWithValue("@prod", d.IdProducto);
                    cmd.Parameters.AddWithValue("@cant", d.Cantidad);
                    cmd.Parameters.AddWithValue("@precio", d.PrecioUnitario);
                    cmd.Parameters.AddWithValue("@sub", d.Subtotal);
                    cmd.Parameters.AddWithValue("@notas", d.Nombre.Contains("(")
                                                            ? d.Nombre : "");
                    cmd.ExecuteNonQuery();
                }

                // 2. Recalcular subtotal de la cuenta sumando todos los detalles
                decimal nuevoSubtotal;
                using (var cmdSum = new SQLiteCommand(
                    "SELECT COALESCE(SUM(subtotal),0) FROM detalle_cuenta WHERE id_cuenta = @id;",
                    con, tran))
                {
                    cmdSum.Parameters.AddWithValue("@id", _idCuenta);
                    nuevoSubtotal = Convert.ToDecimal(cmdSum.ExecuteScalar());
                }

                // 3. Actualizar cuenta con nuevo subtotal y total
                using (var cmdUpd = new SQLiteCommand(@"
                    UPDATE cuenta
                    SET subtotal = @sub,
                        total    = @tot
                    WHERE id_cuenta = @id;", con, tran))
                {
                    cmdUpd.Parameters.AddWithValue("@sub", nuevoSubtotal);
                    cmdUpd.Parameters.AddWithValue("@tot", nuevoSubtotal); // sin cargo extra aún
                    cmdUpd.Parameters.AddWithValue("@id", _idCuenta);
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

        private decimal TotalCarrito() =>
            _carrito.Count == 0 ? 0 : _carrito.Sum(d => d.Subtotal);

        private static Label MakeLbl(string text, int x, int y) => new()
        {
            Text = text,
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(200, 220, 255),
            AutoSize = true,
            Location = new Point(x, y),
            BackColor = Color.Transparent
        };

        private static Button MakeBtn(string text, Color bg, Point loc, Size size)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = bg,
                FlatStyle = FlatStyle.Flat,
                Location = loc,
                Size = size,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private static void ApplyRound(Control ctrl, int r)
        {
            ctrl.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var path = new System.Drawing.Drawing2D.GraphicsPath();
                var rect = new Rectangle(0, 0, ctrl.Width - 1, ctrl.Height - 1);
                path.AddArc(rect.Left, rect.Top, r * 2, r * 2, 180, 90);
                path.AddArc(rect.Right - r * 2, rect.Top, r * 2, r * 2, 270, 90);
                path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
                path.AddArc(rect.Left, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
                path.CloseFigure();
                using var fill = new SolidBrush(ctrl.BackColor);
                e.Graphics.FillPath(fill, path);
            };
        }

        // Permite sumar una lista de decimales sin LINQ extra
        private static decimal Sum(List<DetallePedido> lista, Func<DetallePedido, decimal> sel)
        {
            decimal t = 0;
            foreach (var i in lista) t += sel(i);
            return t;
        }
    }
}