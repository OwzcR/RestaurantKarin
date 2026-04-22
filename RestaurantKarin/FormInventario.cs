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
        // Colores y configuración visual
        private Color colorVerdeBorde = Color.FromArgb(88, 160, 166);
        private Color colorTablaFondo = Color.FromArgb(220, 230, 235);
        private Color colorAzulBtn = Color.FromArgb(26, 90, 122);
        private ListView lista;

        public FormInventario()
        {
            InitializeComponent();
            SetupUI();
            CargarDatosTabla();
        }

        private void SetupUI()
        {
            // Configuración del Formulario Base
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(29, 53, 87);
            this.Padding = new Padding(40);

            // 1. TARJETA BLANCA PRINCIPAL
            Panel cardPrincipal = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(25)
            };
            this.Controls.Add(cardPrincipal);
            RedondearControl(cardPrincipal, 30);

            // --- 2. PANEL SUPERIOR (BUSCADOR) ---
            Panel pnlSuperior = new Panel { Dock = DockStyle.Top, Height = 65 };
            Panel pnlBusqueda = new Panel
            {
                Size = new Size(500, 40),
                Location = new Point(0, 10),
                BackColor = Color.FromArgb(240, 244, 248)
            };
            TextBox txtBusqueda = new TextBox
            {
                Text = " BUSCAR INSUMO...",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Gray,
                BorderStyle = BorderStyle.None,
                BackColor = pnlBusqueda.BackColor,
                Location = new Point(15, 10),
                Width = 450
            };
            pnlBusqueda.Controls.Add(txtBusqueda);
            pnlSuperior.Controls.Add(pnlBusqueda);
            RedondearControl(pnlBusqueda, 20);

            // --- 3. PANEL INFERIOR (BOTONES DE ACCIÓN) ---
            TableLayoutPanel pnlBotones = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 85,
                ColumnCount = 4,
                Padding = new Padding(0, 15, 0, 0)
            };
            pnlBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            pnlBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            pnlBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            pnlBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            Button btnAgregar = CrearBotonAccion("➕ AGREGAR\nINSUMO", colorVerdeBorde);
            btnAgregar.Click += (s, e) => {
                using (FormAgregarInsumo frm = new FormAgregarInsumo())
                {
                    frm.StartPosition = FormStartPosition.CenterParent;
                    if (frm.ShowDialog(this) == DialogResult.OK) CargarDatosTabla();
                }
            };

            Button btnEditar = CrearBotonAccion("📝 EDITAR\nINSUMO", colorAzulBtn);
            btnEditar.Click += (s, e) => {
                if (lista.SelectedItems.Count > 0)
                {
                    ListViewItem item = lista.SelectedItems[0];
                    using (FormEditarInsumo frmEditar = new FormEditarInsumo())
                    {
                        frmEditar.StartPosition = FormStartPosition.CenterParent;
                        frmEditar.CargarDatosParaEdicion(
                            item.SubItems[0].Text, item.SubItems[1].Text, item.SubItems[2].Text,
                            item.SubItems[3].Text, item.SubItems[4].Text, item.SubItems[5].Text, item.SubItems[6].Text);
                        if (frmEditar.ShowDialog(this) == DialogResult.OK) CargarDatosTabla();
                    }
                }
                else MessageBox.Show("Selecciona un insumo primero.");
            };

            Button btnEntrada = CrearBotonAccion("🗃 ENTRADA\nINSUMOS", colorAzulBtn);
            btnEntrada.Click += (s, e) => {
                if (lista.SelectedItems.Count > 0)
                {
                    ListViewItem item = lista.SelectedItems[0];
                    using (FormEntradaInsumos frmEntrada = new FormEntradaInsumos())
                    {
                        frmEntrada.CargarDatos(item.SubItems[0].Text, item.SubItems[1].Text, item.SubItems[3].Text);
                        if (frmEntrada.ShowDialog(this) == DialogResult.OK) CargarDatosTabla();
                    }
                }
                else MessageBox.Show("Selecciona un insumo.");
            };

            Button btnEliminar = CrearBotonAccion("🗑 ELIMINAR\nINSUMO", Color.FromArgb(239, 83, 80));
            btnEliminar.Click += (s, e) => {
                if (lista.SelectedItems.Count > 0) AccionEliminar(lista.SelectedItems[0]);
            };

            pnlBotones.Controls.Add(btnAgregar, 0, 0);
            pnlBotones.Controls.Add(btnEditar, 1, 0);
            pnlBotones.Controls.Add(btnEntrada, 2, 0);
            pnlBotones.Controls.Add(btnEliminar, 3, 0);

            // --- 4. TABLA (LISTVIEW) ---
            lista = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                MultiSelect = false,
                BorderStyle = BorderStyle.None,
                BackColor = colorTablaFondo,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                OwnerDraw = true
            };

            lista.Columns.Add("ID", 50);
            lista.Columns.Add("Insumo", 250);
            lista.Columns.Add("Stock Actual", 130);
            lista.Columns.Add("Unidad", 100);
            lista.Columns.Add("Stock Mínimo", 130);
            lista.Columns.Add("Última Entrada", 160);
            lista.Columns.Add("Costo", 120);

            lista.DrawColumnHeader += (s, e) => {
                e.Graphics.FillRectangle(Brushes.White, e.Bounds);
                e.Graphics.DrawRectangle(new Pen(colorVerdeBorde, 3), e.Bounds);
                TextRenderer.DrawText(e.Graphics, e.Header.Text, lista.Font, e.Bounds, Color.FromArgb(26, 75, 80), TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
            };
            lista.DrawSubItem += (s, e) => {
                e.Graphics.DrawRectangle(new Pen(colorVerdeBorde, 1), e.Bounds);
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, lista.Font, e.Bounds, Color.Black, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            };

            // AGREGAR AL CARD EN ORDEN ESPECÍFICO PARA EL DOCKING
            cardPrincipal.Controls.Add(lista);       
            cardPrincipal.Controls.Add(pnlSuperior); 
            cardPrincipal.Controls.Add(pnlBotones);  

            lista.BringToFront(); 
        }


        public void CargarDatosTabla()
        {
            if (lista == null) return;
            lista.Items.Clear();

            // Dato de prueba
            ListViewItem prueba = new ListViewItem("0");
            prueba.SubItems.Add("TOMATE (PRUEBA)");
            prueba.SubItems.Add("10"); prueba.SubItems.Add("Kg");
            prueba.SubItems.Add("2"); prueba.SubItems.Add(DateTime.Now.ToString("dd/MM/yyyy"));
            prueba.SubItems.Add("$25.00");
            lista.Items.Add(prueba);

            try
            {
                string cadena = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                using (SQLiteConnection conn = new SQLiteConnection(cadena))
                {
                    conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM Insumos", conn))
                    {
                        using (SQLiteDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
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
            }
            catch (Exception) { }
        }

        private void AccionEliminar(ListViewItem item)
        {
            string id = item.SubItems[0].Text;
            if (id == "0") { MessageBox.Show("No puedes eliminar el dato de prueba."); return; }

            if (MessageBox.Show($"¿Eliminar '{item.SubItems[1].Text}'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    string cadena = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                    using (SQLiteConnection con = new SQLiteConnection(cadena))
                    {
                        con.Open();
                        using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Insumos WHERE id_insumo = @id", con))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    CargarDatosTabla();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private Button CrearBotonAccion(string texto, Color color)
        {
            Button btn = new Button
            {
                Text = texto,
                Dock = DockStyle.Fill,
                Margin = new Padding(8, 0, 8, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn; 
        }

        private void RedondearControl(Control c, int radio)
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddArc(0, 0, radio, radio, 180, 90);
            gp.AddArc(c.Width - radio, 0, radio, radio, 270, 90);
            gp.AddArc(c.Width - radio, c.Height - radio, radio, radio, 0, 90);
            gp.AddArc(0, c.Height - radio, radio, radio, 90, 90);
            c.Region = new Region(gp);
            c.Resize += (s, e) => RedondearControl(c, radio);
        }
    }
}