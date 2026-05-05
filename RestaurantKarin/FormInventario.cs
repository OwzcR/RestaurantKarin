using System;
using System.Configuration;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public partial class FormInventario : UserControl
    {
        // ── Paleta ────────────────────────────────────────────────────────────
        private static readonly Color ColHeader  = Color.FromArgb(14,  77, 110);
        private static readonly Color ColBorde   = Color.FromArgb(0,  151, 167);
        private static readonly Color ColRowBg   = Color.White;
        private static readonly Color ColRowSel  = Color.FromArgb(185, 235, 242);
        private static readonly Color ColBtnTeal = Color.FromArgb(0,  151, 167);
        private static readonly Color ColBtnNavy = Color.FromArgb(23,  91, 122);

        private static readonly Font FontHdr  = new Font("Segoe UI", 10, FontStyle.Bold);
        private static readonly Font FontName = new Font("Segoe UI", 10, FontStyle.Bold);
        private static readonly Font FontData = new Font("Segoe UI", 10);

        private ListViewEx _lista       = null!;
        private TextBox    _txtBusqueda = null!;
        private Button     _btnSel      = null!;

        public FormInventario()
        {
            InitializeComponent();
            SetupUI();
            CargarDatosTabla();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Construcción de la UI
        // ══════════════════════════════════════════════════════════════════════
        private void SetupUI()
        {
            BackColor = Color.Transparent;
            Dock      = DockStyle.Fill;
            Padding   = new Padding(28, 18, 28, 18);   // muestra el fondo alrededor

            // ── Barra de búsqueda ─────────────────────────────────────────────
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.Transparent };

            var pill = new Panel { BackColor = Color.FromArgb(228, 235, 240), Height = 36, Location = new Point(0, 6) };
            ApplyRound(pill, 18);

            pnlTop.Resize += (_, _) =>
            {
                pill.Width = pnlTop.Width;
                ApplyRound(pill, 18);
                if (_btnSel      != null) _btnSel.Location      = new Point(pill.Width - 126, 3);
                if (_txtBusqueda != null) _txtBusqueda.Width    = Math.Max(60, pill.Width - 126 - 218 - 6);
            };

            new Label
            {
                Text = "🔍", Font = new Font("Segoe UI", 10),
                AutoSize = false, Size = new Size(32, 36), Location = new Point(6, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent, ForeColor = Color.FromArgb(50, 50, 50),
                Parent = pill
            };
            new Label
            {
                Text = "BUSCAR ID PRODUCTO :",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                AutoSize = false, Size = new Size(178, 36), Location = new Point(38, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent, ForeColor = Color.FromArgb(30, 30, 30),
                Parent = pill
            };

            _txtBusqueda = new TextBox
            {
                Font = new Font("Segoe UI", 9), BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(228, 235, 240), ForeColor = Color.FromArgb(25, 25, 25),
                Size = new Size(150, 22), Location = new Point(218, 8), Parent = pill
            };
            _txtBusqueda.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) BuscarPorId(_txtBusqueda.Text.Trim()); };

            _btnSel = new Button
            {
                Text = "SELECCIONAR",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.White, BackColor = ColHeader,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
                Size = new Size(118, 30), Location = new Point(380, 3), Parent = pill
            };
            _btnSel.FlatAppearance.BorderSize = 0;
            ApplyRoundOnResize(_btnSel, 8);
            _btnSel.Click += (_, _) => BuscarPorId(_txtBusqueda.Text.Trim());

            pnlTop.Controls.Add(pill);

            // ── Botones de acción ─────────────────────────────────────────────
            var pnlBotones = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom, Height = 60,
                ColumnCount = 4, Margin = Padding.Empty,
                Padding = new Padding(0, 2, 0, 2),
                BackColor = Color.Transparent
            };
            for (int i = 0; i < 4; i++)
                pnlBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            var bAgregar  = ActionPanel("➕", "AGREGAR\nINSUMO",     ColBtnTeal);
            var bEditar   = ActionPanel("✏",  "EDITAR\nINSUMO",      ColBtnNavy);
            var bEntrada  = ActionPanel("🗄",  "ENTRADA\nDE INSUMOS", ColBtnTeal);
            var bEliminar = ActionPanel("🗑",  "ELIMINAR\nINSUMO",    ColBtnNavy);

            bAgregar.Margin  = new Padding(0, 0, 3, 0);
            bEditar.Margin   = new Padding(3, 0, 3, 0);
            bEntrada.Margin  = new Padding(3, 0, 3, 0);
            bEliminar.Margin = new Padding(3, 0, 0, 0);

            ApplyRoundOnResize(bAgregar,  10);
            ApplyRoundOnResize(bEditar,   10);
            ApplyRoundOnResize(bEntrada,  10);
            ApplyRoundOnResize(bEliminar, 10);

            Wire(bAgregar,  (_, _) => DoAgregar());
            Wire(bEditar,   (_, _) => DoEditar());
            Wire(bEntrada,  (_, _) => DoEntrada());
            Wire(bEliminar, (_, _) => DoEliminar());

            pnlBotones.Controls.Add(bAgregar,  0, 0);
            pnlBotones.Controls.Add(bEditar,   1, 0);
            pnlBotones.Controls.Add(bEntrada,  2, 0);
            pnlBotones.Controls.Add(bEliminar, 3, 0);

            // ── Tabla con esquinas redondeadas ────────────────────────────────
            var pnlGapTop  = new Panel { Dock = DockStyle.Top,    Height = 8, BackColor = Color.Transparent };
            var pnlGapBtns = new Panel { Dock = DockStyle.Bottom, Height = 8, BackColor = Color.Transparent };
            var pnlLista   = new Panel { Dock = DockStyle.Fill,              BackColor = Color.White };
            pnlLista.Resize += (_, _) =>
            {
                if (pnlLista.Width > 20 && pnlLista.Height > 20)
                    ApplyRound(pnlLista, 12);
            };

            _lista = new ListViewEx
            {
                Dock = DockStyle.Fill, View = View.Details,
                FullRowSelect = true, MultiSelect = false,
                BorderStyle = BorderStyle.None, BackColor = Color.White,
                Font = FontData, OwnerDraw = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable
            };

            _lista.SmallImageList = new ImageList { ImageSize = new Size(1, 38) };

            _lista.Columns.Add("Insumo",        240);
            _lista.Columns.Add("Stock Actual",  115);
            _lista.Columns.Add("Unidad",         78);
            _lista.Columns.Add("Stock Mínimo",  115);
            _lista.Columns.Add("Ultima Entrada",145);
            _lista.Columns.Add("Costo Unitario",130);

            _lista.SizeChanged          += (_, _) => AjustarColumnas();
            _lista.ItemSelectionChanged += (_, _) => _lista.Invalidate();

            _lista.DrawColumnHeader += DrawHeader;
            _lista.DrawItem         += DrawRow;
            _lista.DrawSubItem      += DrawCell;

            pnlLista.Controls.Add(_lista);

            // Orden: Fill → Bottom (de abajo a arriba) → Top (de abajo a arriba)
            Controls.Add(pnlLista);
            Controls.Add(pnlBotones);
            Controls.Add(pnlGapBtns);
            Controls.Add(pnlGapTop);
            Controls.Add(pnlTop);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  OwnerDraw handlers
        // ══════════════════════════════════════════════════════════════════════
        private static void DrawHeader(object? s, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var br = new SolidBrush(ColHeader);
            e.Graphics.FillRectangle(br, e.Bounds);
            using var sep = new Pen(Color.FromArgb(0, 171, 187), 1);
            e.Graphics.DrawLine(sep, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom);
            using var bot = new Pen(ColBorde, 2);
            e.Graphics.DrawLine(bot, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);

            var flags = e.ColumnIndex == 0
                ? TextFormatFlags.VerticalCenter | TextFormatFlags.Left
                : TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
            var tr = e.ColumnIndex == 0
                ? new Rectangle(e.Bounds.X + 12, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height)
                : e.Bounds;
            TextRenderer.DrawText(e.Graphics, e.Header.Text, FontHdr, tr, Color.White, flags);
        }

        private static void DrawRow(object? s, DrawListViewItemEventArgs e)
        {
            Color bg = e.Item.Selected ? ColRowSel : ColRowBg;
            using var br = new SolidBrush(bg);
            e.Graphics.FillRectangle(br, e.Bounds);
        }

        private static void DrawCell(object? s, DrawListViewSubItemEventArgs e)
        {
            // Relleno de fondo de celda — necesario para que el hover no borre el renglón
            Color bg = e.Item.Selected ? ColRowSel : ColRowBg;
            using (var brBg = new SolidBrush(bg))
                e.Graphics.FillRectangle(brBg, e.Bounds);

            using (var hLine = new Pen(ColBorde, 1))
                e.Graphics.DrawLine(hLine, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            using (var vLine = new Pen(ColBorde, 1))
                e.Graphics.DrawLine(vLine, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom);

            var font  = e.ColumnIndex == 0 ? FontName : FontData;
            var flags = e.ColumnIndex == 0
                ? TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.WordBreak
                : TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
            var tr = new Rectangle(e.Bounds.X + 10, e.Bounds.Y + 2, e.Bounds.Width - 14, e.Bounds.Height - 4);
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, font, tr, Color.FromArgb(25, 25, 25), flags);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Acciones de botones
        // ══════════════════════════════════════════════════════════════════════
        private void DoAgregar()
        {
            using var frm = new FormAgregarInsumo { StartPosition = FormStartPosition.CenterParent };
            if (frm.ShowDialog(FindForm()) == DialogResult.OK) CargarDatosTabla();
        }

        private void DoEditar()
        {
            if (_lista.SelectedItems.Count == 0) { MessageBox.Show("Selecciona un insumo primero."); return; }
            var it = _lista.SelectedItems[0];
            using var frm = new FormEditarInsumo { StartPosition = FormStartPosition.CenterParent };
            frm.CargarDatosParaEdicion(
                it.Tag!.ToString()!,
                it.SubItems[0].Text, it.SubItems[1].Text, it.SubItems[2].Text,
                it.SubItems[3].Text, it.SubItems[4].Text, it.SubItems[5].Text);
            if (frm.ShowDialog(FindForm()) == DialogResult.OK) CargarDatosTabla();
        }

        private void DoEntrada()
        {
            if (_lista.SelectedItems.Count == 0) { MessageBox.Show("Selecciona un insumo."); return; }
            var it = _lista.SelectedItems[0];
            using var frm = new FormEntradaInsumos { StartPosition = FormStartPosition.CenterParent };
            frm.CargarDatos(it.Tag!.ToString()!, it.SubItems[0].Text, it.SubItems[2].Text);
            if (frm.ShowDialog(FindForm()) == DialogResult.OK) CargarDatosTabla();
        }

        private void DoEliminar()
        {
            if (_lista.SelectedItems.Count == 0) { MessageBox.Show("Selecciona un insumo."); return; }
            EliminarInsumo(_lista.SelectedItems[0]);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Datos
        // ══════════════════════════════════════════════════════════════════════
        public void CargarDatosTabla()
        {
            if (_lista == null) return;
            _lista.Items.Clear();
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                using var conn = new SQLiteConnection(cs);
                conn.Open();
                using var cmd = new SQLiteCommand("SELECT * FROM Insumos ORDER BY Nombre", conn);
                using var dr  = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var item = new ListViewItem(dr["Nombre"].ToString()) { Tag = dr["id_insumo"].ToString() };
                    item.SubItems.Add(dr["StockActual"].ToString());
                    item.SubItems.Add(dr["Unidad"].ToString());
                    item.SubItems.Add(dr["StockMinimo"].ToString());
                    item.SubItems.Add(dr["FechaEntrada"].ToString());
                    item.SubItems.Add("$" + dr["Costo"].ToString());
                    _lista.Items.Add(item);
                }
            }
            catch { }
            AjustarColumnas();
        }

        private void BuscarPorId(string texto)
        {
            if (string.IsNullOrEmpty(texto)) { CargarDatosTabla(); return; }
            _lista.Items.Clear();
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                using var conn = new SQLiteConnection(cs);
                conn.Open();
                using var cmd = new SQLiteCommand(
                    "SELECT * FROM Insumos WHERE id_insumo = @id OR Nombre LIKE @n ORDER BY Nombre", conn);
                cmd.Parameters.AddWithValue("@id", texto);
                cmd.Parameters.AddWithValue("@n",  "%" + texto + "%");
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var item = new ListViewItem(dr["Nombre"].ToString()) { Tag = dr["id_insumo"].ToString() };
                    item.SubItems.Add(dr["StockActual"].ToString());
                    item.SubItems.Add(dr["Unidad"].ToString());
                    item.SubItems.Add(dr["StockMinimo"].ToString());
                    item.SubItems.Add(dr["FechaEntrada"].ToString());
                    item.SubItems.Add("$" + dr["Costo"].ToString());
                    _lista.Items.Add(item);
                }
            }
            catch { }
            AjustarColumnas();
        }

        private void EliminarInsumo(ListViewItem item)
        {
            if (MessageBox.Show($"¿Eliminar '{item.SubItems[0].Text}'?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                using var con = new SQLiteConnection(cs);
                con.Open();
                using var cmd = new SQLiteCommand("DELETE FROM Insumos WHERE id_insumo = @id", con);
                cmd.Parameters.AddWithValue("@id", item.Tag?.ToString() ?? "");
                cmd.ExecuteNonQuery();
                CargarDatosTabla();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void AjustarColumnas()
        {
            if (_lista == null || _lista.Width <= 10) return;
            int avail = _lista.Width - SystemInformation.VerticalScrollBarWidth - 2;
            int fijo  = 115 + 78 + 115 + 145 + 130;
            _lista.Columns[0].Width = Math.Max(140, avail - fijo);
            _lista.Columns[1].Width = 115;
            _lista.Columns[2].Width = 78;
            _lista.Columns[3].Width = 115;
            _lista.Columns[4].Width = 145;
            _lista.Columns[5].Width = 130;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Helpers visuales
        // ══════════════════════════════════════════════════════════════════════
        private static Panel ActionPanel(string icon, string text, Color bg)
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Fill, BackColor = bg,
                Cursor = Cursors.Hand, Margin = new Padding(0)
            };

            var lblIco = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 15),
                ForeColor = Color.White, BackColor = Color.Transparent,
                AutoSize = false, Dock = DockStyle.Left, Width = 50,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblTxt = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White, BackColor = Color.Transparent,
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(2, 0, 0, 0)
            };

            pnl.Controls.Add(lblTxt);
            pnl.Controls.Add(lblIco);

            Color hover = ControlPaint.Dark(bg, 0.10f);
            Action<bool> setHover = on => pnl.BackColor = on ? hover : bg;
            pnl.MouseEnter    += (_, _) => setHover(true);
            pnl.MouseLeave    += (_, _) => setHover(false);
            lblIco.MouseEnter += (_, _) => setHover(true);
            lblIco.MouseLeave += (_, _) => setHover(false);
            lblTxt.MouseEnter += (_, _) => setHover(true);
            lblTxt.MouseLeave += (_, _) => setHover(false);

            return pnl;
        }

        private static void Wire(Panel pnl, EventHandler handler)
        {
            pnl.Click += handler;
            foreach (Control child in pnl.Controls)
                child.Click += handler;
        }

        private static void ApplyRound(Control c, int r)
        {
            if (c.Width <= 0 || c.Height <= 0) return;
            c.Region = new Region(RoundPath(new Rectangle(0, 0, c.Width, c.Height), r));
        }

        private static void ApplyRoundOnResize(Control c, int r)
        {
            c.Resize += (_, _) => ApplyRound(c, r);
            ApplyRound(c, r);
        }

        private static GraphicsPath RoundPath(Rectangle rc, int r)
        {
            int d  = r * 2;
            var gp = new GraphicsPath();
            gp.AddArc(rc.Left,      rc.Top,        d, d, 180, 90);
            gp.AddArc(rc.Right - d, rc.Top,        d, d, 270, 90);
            gp.AddArc(rc.Right - d, rc.Bottom - d, d, d, 0,   90);
            gp.AddArc(rc.Left,      rc.Bottom - d, d, d, 90,  90);
            gp.CloseFigure();
            return gp;
        }

        // ListView con doble buffer nativo.
        // En hover, Windows solo invalida la primera columna (item text), por lo que
        // DrawSubItem no se dispara para las otras columnas y el sistema las borra.
        // OnMouseMove fuerza una invalidación de ancho completo para cada fila,
        // asegurando que DrawSubItem reciba las 6 columnas en el update region.
        private class ListViewEx : ListView
        {
            private int _hotIndex = -1;

            public ListViewEx()
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.AllPaintingInWmPaint, true);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == 0x0014) { m.Result = IntPtr.Zero; return; } // WM_ERASEBKGND
                base.WndProc(ref m);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                int idx = HitTest(e.X, e.Y).Item?.Index ?? -1;
                if (idx == _hotIndex) return;
                RedrawRow(_hotIndex);
                _hotIndex = idx;
                RedrawRow(_hotIndex);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                RedrawRow(_hotIndex);
                _hotIndex = -1;
            }

            private void RedrawRow(int idx)
            {
                if (idx < 0 || idx >= Items.Count) return;
                var b = Items[idx].Bounds;
                Invalidate(new Rectangle(0, b.Y, Width, b.Height));
            }
        }
    }
}
