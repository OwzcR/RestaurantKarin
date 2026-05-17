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
        private static readonly Color ColHeader = Color.FromArgb(14, 77, 110);
        private static readonly Color ColBorde = Color.FromArgb(0, 151, 167);
        private static readonly Color ColRowBg = Color.White;
        private static readonly Color ColRowSel = Color.FromArgb(185, 235, 242);
        private static readonly Color ColBtnTeal = Color.FromArgb(0, 151, 167);
        private static readonly Color ColBtnNavy = Color.FromArgb(23, 91, 122);
        private static readonly Color ColSearchBg = Color.FromArgb(240, 244, 247);

        private ListViewEx _lista;
        private TextBox _txtBusqueda;

        public FormInventario()
        {
            InitializeComponent();
            SetupUI();
            CargarDatosTabla();
        }

        private void SetupUI()
        {
            this.BackColor = Color.Transparent;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(15, 10, 15, 10);
            
            // --- 1. BUSCADOR (TOP) ---
            Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 60 };
            Panel pill = new Panel { BackColor = ColSearchBg, Width = 380, Height = 36, Location = new Point(0, 10) };
            ApplyRound(pill, 18);

            _txtBusqueda = new TextBox
            {
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.None,
                BackColor = ColSearchBg,
                Location = new Point(15, 10),
                Width = 250,
                Text = "Escribe aquí...",
                ForeColor = Color.Gray
            };

            _txtBusqueda.Enter += (s, e) => { if (_txtBusqueda.Text == "Escribe aquí...") { _txtBusqueda.Text = ""; _txtBusqueda.ForeColor = Color.Black; } };
            _txtBusqueda.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(_txtBusqueda.Text)) { _txtBusqueda.Text = "Escribe aquí..."; _txtBusqueda.ForeColor = Color.Gray; } };

            Button btnBusca = new Button
            {
                Text = "BUSCAR",
                BackColor = ColHeader,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Right,
                Width = 90,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBusca.FlatAppearance.BorderSize = 0;
            btnBusca.Click += (s, e) => BuscarPorTexto(_txtBusqueda.Text);

            pill.Controls.Add(_txtBusqueda);
            pill.Controls.Add(btnBusca);
            pnlTop.Controls.Add(pill);

            // --- 2. CONTENEDOR MAESTRO ---
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            // Los botones ocupan el 12% para verse más pequeños
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 88f));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 12f));

            // --- 3. LA TABLA ---
            _lista = new ListViewEx
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                BorderStyle = BorderStyle.None,
                OwnerDraw = true,
                Font = new Font("Segoe UI", 9)
            };
            ConfigurarColumnas();
            _lista.SizeChanged += (s, e) => AjustarAnchoColumnas();

            Panel pnlTablaWrap = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, 0, 15) };
            ApplyRound(pnlTablaWrap, 12);
            pnlTablaWrap.Controls.Add(_lista);

            mainLayout.Controls.Add(pnlTablaWrap, 0, 0);

            // --- 4. BOTONES ---
            TableLayoutPanel pnlBotones = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                Margin = new Padding(0, 0, 0, 10)
            };
            for (int i = 0; i < 4; i++) pnlBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            pnlBotones.Controls.Add(CreateActionButton("➕", "Agregar Insumo", ColBtnTeal, (s, e) => DoAgregar()), 0, 0);
            pnlBotones.Controls.Add(CreateActionButton("✏", "Editar Insumo", ColBtnNavy, (s, e) => DoEditar()), 1, 0);
            pnlBotones.Controls.Add(CreateActionButton("🗄", "Entrada Insumos", ColBtnTeal, (s, e) => DoEntrada()), 2, 0);
            pnlBotones.Controls.Add(CreateActionButton("🗑", "Eliminar Insumo", ColBtnNavy, (s, e) => DoEliminar()), 3, 0);

            mainLayout.Controls.Add(pnlBotones, 0, 1);

            this.Controls.Add(mainLayout);
            this.Controls.Add(pnlTop);
        }

        private void ConfigurarColumnas()
        {
            _lista.Columns.Add("Insumo", 200);
            _lista.Columns.Add("Stock Actual", 100);
            _lista.Columns.Add("Unidad", 100);
            _lista.Columns.Add("Stock Mínimo", 100);
            _lista.Columns.Add("Ultima Entrada", 130);
            _lista.Columns.Add("Costo Unitario", -2);

            _lista.DrawColumnHeader += (s, e) => {
                e.Graphics.FillRectangle(new SolidBrush(ColHeader), e.Bounds);
                TextRenderer.DrawText(e.Graphics, e.Header.Text, new Font("Segoe UI", 9, FontStyle.Bold), e.Bounds, Color.White, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
            };

            _lista.DrawSubItem += (s, e) => {
                Color bg = e.Item.Selected ? ColRowSel : ColRowBg;
                e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
                using (Pen p = new Pen(ColBorde)) e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.Item.Font, e.Bounds, Color.Black, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
            };
        }

        private void AjustarAnchoColumnas()
        {
            if (_lista.Width <= 0) return;
            int w = _lista.Width - 30;
            _lista.Columns[0].Width = (int)(w * 0.30);
            _lista.Columns[1].Width = (int)(w * 0.12);
            _lista.Columns[2].Width = (int)(w * 0.12);
            _lista.Columns[3].Width = (int)(w * 0.12);
            _lista.Columns[4].Width = (int)(w * 0.17);
            _lista.Columns[5].Width = -2;
        }

        private Panel CreateActionButton(string icon, string text, Color bg, EventHandler click)
        {
            Panel p = new Panel { Dock = DockStyle.Fill, BackColor = bg, Margin = new Padding(8), Cursor = Cursors.Hand };
            Label l = new Label
            {
                Text = icon + " " + text,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold), // Letra más grande
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            l.Click += click;
            p.Click += click;
            p.Controls.Add(l);

            ApplyRound(p, 10);
            return p;
        }

        public void CargarDatosTabla()
        {
            _lista.Items.Clear();
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                using var conn = new SQLiteConnection(cs);
                conn.Open();
                using var cmd = new SQLiteCommand("SELECT * FROM Insumos ORDER BY Nombre", conn);
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var it = new ListViewItem(dr["Nombre"].ToString()) { Tag = dr["id_insumo"].ToString() };
                    it.SubItems.Add(dr["StockActual"].ToString());
                    it.SubItems.Add(dr["Unidad"].ToString());
                    it.SubItems.Add(dr["StockMinimo"].ToString());
                    it.SubItems.Add(dr["FechaEntrada"].ToString());
                    it.SubItems.Add("$" + dr["Costo"].ToString());
                    _lista.Items.Add(it);
                }
            }
            catch { }
            AjustarAnchoColumnas();
        }

        private void BuscarPorTexto(string t)
        {
            if (string.IsNullOrWhiteSpace(t) || t == "Escribe aquí...") { CargarDatosTabla(); return; }
            _lista.Items.Clear();
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                using var conn = new SQLiteConnection(cs);
                conn.Open();
                using var cmd = new SQLiteCommand("SELECT * FROM Insumos WHERE Nombre LIKE @n OR id_insumo LIKE @n", conn);
                cmd.Parameters.AddWithValue("@n", "%" + t + "%");
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var it = new ListViewItem(dr["Nombre"].ToString()) { Tag = dr["id_insumo"].ToString() };
                    it.SubItems.Add(dr["StockActual"].ToString());
                    it.SubItems.Add(dr["Unidad"].ToString());
                    it.SubItems.Add(dr["StockMinimo"].ToString());
                    it.SubItems.Add(dr["FechaEntrada"].ToString());
                    it.SubItems.Add("$" + dr["Costo"].ToString());
                    _lista.Items.Add(it);
                }
            }
            catch { }
            AjustarAnchoColumnas();
        }

        private void DoAgregar() { using var f = new FormAgregarInsumo(); if (f.ShowDialog() == DialogResult.OK) CargarDatosTabla(); }

        private void DoEditar()
        {
            if (_lista.SelectedItems.Count == 0)
            {
                MessageBox.Show("Por favor, selecciona un insumo de la lista para editar.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var it = _lista.SelectedItems[0];
            using var f = new FormEditarInsumo();
            f.CargarDatosParaEdicion(it.Tag.ToString(), it.SubItems[0].Text, it.SubItems[1].Text, it.SubItems[2].Text, it.SubItems[3].Text, it.SubItems[4].Text, it.SubItems[5].Text.Replace("$", ""));
            if (f.ShowDialog() == DialogResult.OK) CargarDatosTabla();
        }

        private void DoEntrada()
        {
            if (_lista.SelectedItems.Count == 0)
            {
                MessageBox.Show("Por favor, selecciona un insumo para registrar una entrada.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var it = _lista.SelectedItems[0];
            using var f = new FormEntradaInsumos();
            f.CargarDatos(it.Tag.ToString(), it.SubItems[0].Text, it.SubItems[2].Text);
            if (f.ShowDialog() == DialogResult.OK) CargarDatosTabla();
        }

        private void DoEliminar()
        {
            if (_lista.SelectedItems.Count == 0)
            {
                MessageBox.Show("Por favor, selecciona el insumo que deseas eliminar.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var it = _lista.SelectedItems[0];

            // --- CORRECCIÓN AQUÍ ---
            using var f = new FormEliminarInsumo();

            // Usamos un bloque try-catch o simplemente verificamos si el método existe.
            // Si el método en tu FormEliminarInsumo tiene otro nombre, cámbialo aquí.
            try
            {
                // Intenta pasar el ID y Nombre. Si no tienes este método en el Form, comenta la siguiente línea.
                f.CargarDatos(it.Tag.ToString(), it.Text);
            }
            catch
            {
                /* El método no existe o tiene otra firma */
            }

            if (f.ShowDialog() == DialogResult.OK)
            {
                CargarDatosTabla();
            }
        }

        private void ApplyRound(Control c, int r)
        {
            c.Paint += (s, e) => {
                GraphicsPath gp = new GraphicsPath();
                gp.AddArc(0, 0, r * 2, r * 2, 180, 90);
                gp.AddArc(c.Width - r * 2, 0, r * 2, r * 2, 270, 90);
                gp.AddArc(c.Width - r * 2, c.Height - r * 2, r * 2, r * 2, 0, 90);
                gp.AddArc(0, c.Height - r * 2, r * 2, r * 2, 90, 90);
                c.Region = new Region(gp);
            };
        }

        private class ListViewEx : ListView { public ListViewEx() { DoubleBuffered = true; } }
    }
}