using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Configuration;

namespace RestaurantKarin
{
    public partial class FormInventario : Form
    {
        // Colores de la interfaz
        private Color colorVerdeBorde = Color.FromArgb(88, 160, 166);
        private Color colorTablaFondo = Color.FromArgb(220, 230, 235);
        private Color colorAzulBtn = Color.FromArgb(26, 90, 122);
        private Color colorSeleccion = Color.FromArgb(180, 220, 240); // Celeste de selección
        private ListView lista;
        private TextBox txtBusqueda;

        public FormInventario()
        {
            InitializeComponent();
            SetupUI();
            CargarDatosTabla();
        }

        private void SetupUI()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(29, 53, 87);
            this.Padding = new Padding(20, 20, 30, 20);

            // Contenedor Principal (Tarjeta Blanca)
            Panel cardPrincipal = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(20)
            };
            this.Controls.Add(cardPrincipal);

            cardPrincipal.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = CrearPathRedondeado(cardPrincipal.ClientRectangle, 30))
                {
                    e.Graphics.FillPath(Brushes.White, path);
                }
            };

            // --- PANEL SUPERIOR (Buscador y Filtros) ---
            Panel pnlSuperior = new Panel { Dock = DockStyle.Top, Height = 60 };
            Panel pnlBusqueda = new Panel
            {
                Size = new Size(620, 40),
                Location = new Point(0, 5),
                BackColor = Color.FromArgb(240, 244, 248)
            };

            pnlBusqueda.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = CrearPathRedondeado(pnlBusqueda.ClientRectangle, 20))
                {
                    e.Graphics.FillPath(new SolidBrush(pnlBusqueda.BackColor), path);
                }
            };

            txtBusqueda = new TextBox
            {
                Text = " BUSCAR INSUMO...",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Gray,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(240, 244, 248),
                Location = new Point(15, 12),
                Width = 330
            };

            txtBusqueda.Enter += (s, e) => {
                if (txtBusqueda.Text == " BUSCAR INSUMO...") { txtBusqueda.Text = ""; txtBusqueda.ForeColor = Color.Black; }
            };
            txtBusqueda.Leave += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtBusqueda.Text)) { txtBusqueda.Text = " BUSCAR INSUMO..."; txtBusqueda.ForeColor = Color.Gray; }
            };

            // Botón Buscar
            Button btnBuscar = new Button
            {
                Text = "BUSCAR",
                Size = new Size(100, 32),
                Location = new Point(360, 4),
                BackColor = colorAzulBtn,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBuscar.FlatAppearance.BorderSize = 0;
            btnBuscar.Click += (s, e) => EjecutarBusqueda();

            // Botón Ver Todo (Restaurar tabla)
            Button btnVerTodo = new Button
            {
                Text = "VER TODO",
                Size = new Size(100, 32),
                Location = new Point(470, 4),
                BackColor = Color.SlateGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnVerTodo.FlatAppearance.BorderSize = 0;
            btnVerTodo.Click += (s, e) => {
                txtBusqueda.Text = " BUSCAR INSUMO...";
                txtBusqueda.ForeColor = Color.Gray;
                CargarDatosTabla();
            };

            pnlBusqueda.Controls.Add(txtBusqueda);
            pnlBusqueda.Controls.Add(btnBuscar);
            pnlBusqueda.Controls.Add(btnVerTodo);
            pnlSuperior.Controls.Add(pnlBusqueda);

            // --- PANEL INFERIOR (Acciones) ---
            TableLayoutPanel pnlBotones = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                ColumnCount = 4,
                Padding = new Padding(0, 10, 0, 0)
            };
            pnlBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            pnlBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            pnlBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            pnlBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            Button btnAdd = CrearBotonAccion("➕ AGREGAR\nINSUMO", colorVerdeBorde);
            btnAdd.Click += (s, e) => {
                using (FormAgregarInsumo frm = new FormAgregarInsumo()) {
                    frm.StartPosition = FormStartPosition.CenterParent;
                    if (frm.ShowDialog(this) == DialogResult.OK) CargarDatosTabla();
                }
            };

            Button btnEdit = CrearBotonAccion("📝 EDITAR\nINSUMO", colorAzulBtn);
            btnEdit.Click += (s, e) => {
                if (lista.SelectedItems.Count > 0) {
                    var it = lista.SelectedItems[0];
                    using (FormEditarInsumo frm = new FormEditarInsumo()) {
                        frm.StartPosition = FormStartPosition.CenterParent;
                        frm.CargarDatosParaEdicion(it.Text, it.SubItems[1].Text, it.SubItems[2].Text, it.SubItems[3].Text, it.SubItems[4].Text, it.SubItems[5].Text, it.SubItems[6].Text);
                        if (frm.ShowDialog(this) == DialogResult.OK) CargarDatosTabla();
                    }
                } else MessageBox.Show("Selecciona un insumo para editar.");
            };

            Button btnEnt = CrearBotonAccion("🗃 ENTRADA\nINSUMOS", colorAzulBtn);
            btnEnt.Click += (s, e) => {
                if (lista.SelectedItems.Count > 0) {
                    var it = lista.SelectedItems[0];
                    using (FormEntradaInsumos frm = new FormEntradaInsumos()) {
                        frm.StartPosition = FormStartPosition.CenterParent;
                        frm.CargarDatos(it.Text, it.SubItems[1].Text, it.SubItems[3].Text);
                        if (frm.ShowDialog(this) == DialogResult.OK) CargarDatosTabla();
                    }
                } else MessageBox.Show("Selecciona un insumo para la entrada.");
            };

            Button btnDel = CrearBotonAccion("🗑 ELIMINAR\nINSUMO", Color.FromArgb(239, 83, 80));
            btnDel.Click += (s, e) => {
                if (lista.SelectedItems.Count > 0) AccionEliminar(lista.SelectedItems[0]);
                else MessageBox.Show("Selecciona un insumo.");
            };

            pnlBotones.Controls.Add(btnAdd, 0, 0);
            pnlBotones.Controls.Add(btnEdit, 1, 0);
            pnlBotones.Controls.Add(btnEnt, 2, 0);
            pnlBotones.Controls.Add(btnDel, 3, 0);

            // --- TABLA (ListView) ---
            lista = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                MultiSelect = false,
                BorderStyle = BorderStyle.None,
                BackColor = colorTablaFondo,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                OwnerDraw = true
            };

            lista.Columns.Add("ID", 60);
            lista.Columns.Add("Insumo", 300);
            lista.Columns.Add("Stock Actual", 120);
            lista.Columns.Add("Unidad", 100);
            lista.Columns.Add("Stock Mínimo", 120);
            lista.Columns.Add("Última Entrada", 150);
            lista.Columns.Add("Costo", 110);

            cardPrincipal.SizeChanged += (s, e) => AjustarColumnas();

            lista.DrawColumnHeader += (s, e) => {
                e.Graphics.FillRectangle(Brushes.White, e.Bounds);
                using (Pen p = new Pen(colorVerdeBorde, 2)) e.Graphics.DrawRectangle(p, e.Bounds);
                TextRenderer.DrawText(e.Graphics, e.Header.Text, lista.Font, e.Bounds, Color.FromArgb(26, 75, 80), TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
            };

            lista.DrawSubItem += (s, e) => {
                if (e.Item.Selected) {
                    e.Graphics.FillRectangle(new SolidBrush(colorSeleccion), e.Bounds);
                } else {
                    e.Graphics.FillRectangle(new SolidBrush(lista.BackColor), e.Bounds);
                }
                using (Pen p = new Pen(colorVerdeBorde, 1)) e.Graphics.DrawRectangle(p, e.Bounds);
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, lista.Font, e.Bounds, Color.Black, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            };

            cardPrincipal.Controls.Add(lista);
            cardPrincipal.Controls.Add(pnlSuperior);
            cardPrincipal.Controls.Add(pnlBotones);
            lista.BringToFront();
        }

        private GraphicsPath CrearPathRedondeado(Rectangle r, int radio)
        {
            GraphicsPath gp = new GraphicsPath();
            int d = radio * 2;
            if (d > r.Width) d = r.Width;
            if (d > r.Height) d = r.Height;
            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }

        private void AjustarColumnas()
        {
            if (lista == null || lista.Columns.Count < 2) return;
            int anchoFijo = 0;
            for (int i = 0; i < lista.Columns.Count; i++) if (i != 1) anchoFijo += lista.Columns[i].Width;
            int nuevoAncho = lista.Width - anchoFijo - 15;
            if (nuevoAncho > 100) lista.Columns[1].Width = nuevoAncho;
        }

        private void EjecutarBusqueda()
        {
            string criterio = txtBusqueda.Text.Trim();
            if (criterio == " BUSCAR INSUMO..." || string.IsNullOrEmpty(criterio)) { CargarDatosTabla(); return; }
            lista.Items.Clear();
            try {
                string cadena = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                using (SQLiteConnection conn = new SQLiteConnection(cadena)) {
                    conn.Open();
                    string query = "SELECT * FROM Insumos WHERE id_insumo = @c OR Nombre LIKE @p";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn)) {
                        cmd.Parameters.AddWithValue("@c", criterio);
                        cmd.Parameters.AddWithValue("@p", "%" + criterio + "%");
                        using (SQLiteDataReader dr = cmd.ExecuteReader()) {
                            while (dr.Read()) {
                                ListViewItem item = new ListViewItem(dr["id_insumo"].ToString());
                                item.SubItems.Add(dr["Nombre"].ToString());
                                item.SubItems.Add(dr["StockActual"].ToString());
                                item.SubItems.Add(dr["Unidad"].ToString());
                                item.SubItems.Add(dr["StockMinimo"].ToString());
                                item.SubItems.Add(dr["FechaEntrada"].ToString());
                                item.SubItems.Add("$" + dr["Costo"].ToString());
                                lista.Items.Add(item);
                            }
                        }
                    }
                }
            } catch (Exception ex) { MessageBox.Show("Error en búsqueda: " + ex.Message); }
        }

        public void CargarDatosTabla()
        {
            if (lista == null) return;
            lista.Items.Clear();
            try {
                string cadena = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                using (SQLiteConnection conn = new SQLiteConnection(cadena)) {
                    conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM Insumos", conn)) {
                        using (SQLiteDataReader dr = cmd.ExecuteReader()) {
                            while (dr.Read()) {
                                ListViewItem item = new ListViewItem(dr["id_insumo"].ToString());
                                item.SubItems.Add(dr["Nombre"].ToString());
                                item.SubItems.Add(dr["StockActual"].ToString());
                                item.SubItems.Add(dr["Unidad"].ToString());
                                item.SubItems.Add(dr["StockMinimo"].ToString());
                                item.SubItems.Add(dr["FechaEntrada"].ToString());
                                item.SubItems.Add("$" + dr["Costo"].ToString());
                                lista.Items.Add(item);
                            }
                        }
                    }
                }
            } catch (Exception) { }
            AjustarColumnas();
        }

        private void AccionEliminar(ListViewItem item)
        {
            if (MessageBox.Show($"¿Eliminar '{item.SubItems[1].Text}'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try {
                    string cadena = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                    using (SQLiteConnection con = new SQLiteConnection(cadena)) {
                        con.Open();
                        using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Insumos WHERE id_insumo = @id", con)) {
                            cmd.Parameters.AddWithValue("@id", item.SubItems[0].Text);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    CargarDatosTabla();
                } catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private Button CrearBotonAccion(string texto, Color color)
        {
            Button btn = new Button {
                Text = texto, Dock = DockStyle.Fill, Margin = new Padding(5),
                FlatStyle = FlatStyle.Flat, BackColor = color, ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}
