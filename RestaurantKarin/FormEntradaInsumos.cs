using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Configuration;
using System.Drawing.Drawing2D;

namespace RestaurantKarin
{
    public partial class FormEntradaInsumos : Form
    {
        
        private string idInsumo;
        private Label lblNombreInsumo;
        private TextBox txtUnidad, txtCantidad, txtCosto;

        
        private Color colorAzulBtn = Color.FromArgb(14, 77, 110);
        private Color colorGrisFondo = Color.FromArgb(230, 233, 235);
        private Color colorBtnTeal = Color.FromArgb(0, 151, 167);

        public FormEntradaInsumos()
        {
            InitializeComponent();
            SetupDiseno();
        }

        private void SetupDiseno()
        {
            
            this.Text = "Entrada de Inventario - Restaurant Karin";
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.Size = new Size(550, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.MinimumSize = new Size(500, 480);

            // --- HEADER ---
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = colorAzulBtn };
            Label lblTit = new Label
            {
                Text = "📥 ENTRADA DE INSUMOS",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlHeader.Controls.Add(lblTit);
            this.Controls.Add(pnlHeader);

            // --- CUERPO (Contenedor Principal) ---
            Panel pnlCuerpo = new Panel
            {
                Size = new Size(480, 260),
                Location = new Point(25, 80),
                BackColor = colorGrisFondo,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(pnlCuerpo);
            RedondearControl(pnlCuerpo, 15);

            Label lblL = new Label
            {
                Text = "INSUMO SELECCIONADO:",
                Location = new Point(20, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 64, 64)
            };
            pnlCuerpo.Controls.Add(lblL);

            lblNombreInsumo = new Label
            {
                Text = "CARGANDO...",
                Location = new Point(20, 35),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = colorAzulBtn
            };
            pnlCuerpo.Controls.Add(lblNombreInsumo);

            
            txtCantidad = CrearCampo(pnlCuerpo, "Cantidad a Ingresar:", 85, 20, 200);
            txtCantidad.KeyPress += SoloNumeros_KeyPress;

            txtUnidad = CrearCampo(pnlCuerpo, "Unidad de Medida:", 85, 250, 200);
            txtUnidad.ReadOnly = true;
            txtUnidad.BackColor = Color.White;

            txtCosto = CrearCampo(pnlCuerpo, "Nuevo Costo Unitario ($):", 165, 20, 200);
            txtCosto.KeyPress += SoloNumeros_KeyPress;

            // --- BOTONES ---
            Button btnCargar = new Button
            {
                Text = "CARGAR INVENTARIO",
                Size = new Size(210, 50),
                Location = new Point(300, 370),
                BackColor = colorBtnTeal,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnCargar.FlatAppearance.BorderSize = 0;
            btnCargar.Click += BtnCargar_Click;
            this.Controls.Add(btnCargar);
            RedondearControl(btnCargar, 20);

            Button btnCan = new Button
            {
                Text = "CANCELAR",
                Size = new Size(150, 50),
                Location = new Point(25, 370),
                BackColor = Color.FromArgb(180, 180, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnCan.FlatAppearance.BorderSize = 0;
            btnCan.Click += (s, e) => this.Close();
            this.Controls.Add(btnCan);
            RedondearControl(btnCan, 20);
        }

        private TextBox CrearCampo(Panel p, string titulo, int y, int x, int ancho)
        {
            Label lbl = new Label { Text = titulo, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(64, 64, 64) };
            p.Controls.Add(lbl);

            Panel pnlTxt = new Panel { Size = new Size(ancho, 35), Location = new Point(x, y + 22), BackColor = Color.White };
            p.Controls.Add(pnlTxt);
            RedondearControl(pnlTxt, 12);

            TextBox t = new TextBox { Size = new Size(ancho - 20, 20), Location = new Point(10, 8), BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 10) };
            pnlTxt.Controls.Add(t);
            return t;
        }

        private void SoloNumeros_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.')) e.Handled = true;
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1)) e.Handled = true;
        }

        public void CargarDatos(string id, string nombre, string unidad)
        {
            this.idInsumo = id;
            lblNombreInsumo.Text = nombre.ToUpper();
            txtUnidad.Text = unidad;
        }

        private void BtnCargar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCantidad.Text) || txtCantidad.Text == "0")
            {
                MostrarMensaje("Cantidad inválida ❌", Color.FromArgb(239, 83, 80));
                return;
            }

            try
            {
                string cadena = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                using (SQLiteConnection conn = new SQLiteConnection(cadena))
                {
                    conn.Open();
                    string query = "UPDATE Insumos SET StockActual = StockActual + @cant, Costo = @costo, FechaEntrada = @fecha WHERE id_insumo = @id";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@cant", double.Parse(txtCantidad.Text));
                        cmd.Parameters.AddWithValue("@costo", string.IsNullOrWhiteSpace(txtCosto.Text) ? 0 : double.Parse(txtCosto.Text));
                        cmd.Parameters.AddWithValue("@fecha", DateTime.Now.ToString("dd/MM/yyyy"));
                        cmd.Parameters.AddWithValue("@id", idInsumo);
                        cmd.ExecuteNonQuery();
                    }
                }
                MostrarMensaje("Actualizado con éxito ✅", Color.FromArgb(44, 160, 44));
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex) { MostrarMensaje("Error ❌", Color.FromArgb(239, 83, 80)); }
        }

        private void RedondearControl(Control c, int r)
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddArc(0, 0, r, r, 180, 90);
            gp.AddArc(c.Width - r, 0, r, r, 270, 90);
            gp.AddArc(c.Width - r, c.Height - r, r, r, 0, 90);
            gp.AddArc(0, c.Height - r, r, r, 90, 90);
            c.Region = new Region(gp);
        }

        private void MostrarMensaje(string mensaje, Color color)
        {
            Form toast = new Form { FormBorderStyle = FormBorderStyle.None, StartPosition = FormStartPosition.Manual, Size = new Size(250, 45), BackColor = color, Opacity = 0.95, TopMost = true, ShowInTaskbar = false };
            toast.Location = new Point(this.Left + (this.Width / 2) - 125, this.Top + this.Height - 80);
            Label lbl = new Label { Text = mensaje, ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            toast.Controls.Add(lbl);
            toast.Show(this);

            System.Windows.Forms.Timer tCierre = new System.Windows.Forms.Timer { Interval = 2000 };
            tCierre.Tick += (s, e) => { tCierre.Stop(); toast.Close(); tCierre.Dispose(); };
            tCierre.Start();
        }
    }
}