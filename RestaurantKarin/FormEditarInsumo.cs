using System;
using System.Configuration;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public partial class FormEditarInsumo : Form
    {
        private Color colorAzulBtn  = Color.FromArgb(26, 90, 122);
        private Color colorGrisFondo = Color.FromArgb(230, 233, 235);

        private string idInsumo = "";
        private TextBox txtNombre, txtMinimo, txtActual, txtCosto, txtUnidad;
        private DateTimePicker dtpFecha;

        public FormEditarInsumo()
        {
            InitializeComponent();
            SetupDiseno();
        }

        private void SetupDiseno()
        {
            this.Text            = "Editar Insumo";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.Size            = new Size(800, 580);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = Color.White;

            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = colorAzulBtn };
            Label lblTit = new Label { Text = "EDITAR INSUMO", Dock = DockStyle.Fill, ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            pnlHeader.Controls.Add(lblTit);
            this.Controls.Add(pnlHeader);

            Panel pnlCuerpo = new Panel { Size = new Size(740, 380), Location = new Point(25, 70), BackColor = colorGrisFondo };
            this.Controls.Add(pnlCuerpo);
            RedondearControl(pnlCuerpo, 15);

            txtNombre = CrearCampo(pnlCuerpo, "Nombre del insumo:", 20,  20, 450);
            dtpFecha  = CrearCalendario(pnlCuerpo, "Última entrada:", 100, 20, 340);
            txtMinimo = CrearCampo(pnlCuerpo, "Stock Mínimo:",   180,  20, 340);
            txtMinimo.KeyPress += SoloNumeros_KeyPress;
            txtActual = CrearCampo(pnlCuerpo, "Stock Actual:",   180, 380, 340);
            txtActual.KeyPress += SoloNumeros_KeyPress;
            txtCosto  = CrearCampo(pnlCuerpo, "Costo Unitario:", 260,  20, 340);
            txtCosto.KeyPress += SoloNumeros_KeyPress;
            txtUnidad = CrearCampo(pnlCuerpo, "Unidad:",         260, 380, 340);

            Button btnGuardar = new Button { Text = "GUARDAR CAMBIOS", Size = new Size(340, 50), Location = new Point(425, 470), BackColor = colorAzulBtn, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnGuardar.Click += GuardarCambios_Click;
            this.Controls.Add(btnGuardar);
            RedondearControl(btnGuardar, 20);

            Button btnCan = new Button { Text = "CANCELAR", Size = new Size(340, 50), Location = new Point(25, 470), BackColor = Color.FromArgb(180, 180, 180), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCan.Click += (s, e) => this.Close();
            this.Controls.Add(btnCan);
            RedondearControl(btnCan, 20);
        }

        public void CargarDatosParaEdicion(string id, string nombre, string stockActual, string unidad, string stockMinimo, string fechaEntrada, string costo)
        {
            idInsumo = id;

            SetTexto(txtNombre, nombre);
            SetTexto(txtActual, stockActual);
            SetTexto(txtUnidad, unidad);
            SetTexto(txtMinimo, stockMinimo);
            SetTexto(txtCosto, costo.Replace("$", "").Trim());

            if (DateTime.TryParseExact(fechaEntrada, "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime fecha))
                dtpFecha.Value = fecha;
        }

        private static void SetTexto(TextBox tb, string valor)
        {
            tb.Text      = valor;
            tb.ForeColor = Color.Black;
        }

        private void SoloNumeros_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                e.Handled = true;
            if (e.KeyChar == '.' && ((TextBox)sender).Text.Contains('.'))
                e.Handled = true;
        }

        private void GuardarCambios_Click(object sender, EventArgs e)
        {
            string nombre = txtNombre.Text.Trim();
            if (nombre == "" || nombre == "Escribe aquí...")
            {
                MostrarMensaje("El nombre es obligatorio ❌", Color.FromArgb(239, 83, 80));
                return;
            }

            try
            {
                string cadena = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                using (var con = new SQLiteConnection(cadena))
                {
                    con.Open();
                    string q = @"UPDATE Insumos SET Nombre=@n, StockActual=@sa, Unidad=@u,
                                 StockMinimo=@sm, FechaEntrada=@f, Costo=@c
                                 WHERE id_insumo=@id";
                    using (var cmd = new SQLiteCommand(q, con))
                    {
                        cmd.Parameters.AddWithValue("@n",  nombre);
                        cmd.Parameters.AddWithValue("@sa", ObtenerTexto(txtActual, "0"));
                        cmd.Parameters.AddWithValue("@u",  ObtenerTexto(txtUnidad, "N/A"));
                        cmd.Parameters.AddWithValue("@sm", ObtenerTexto(txtMinimo, "0"));
                        cmd.Parameters.AddWithValue("@f",  dtpFecha.Value.ToString("dd/MM/yyyy"));
                        cmd.Parameters.AddWithValue("@c",  ObtenerTexto(txtCosto, "0"));
                        cmd.Parameters.AddWithValue("@id", idInsumo);
                        cmd.ExecuteNonQuery();
                    }
                }
                MostrarMensaje("Insumo actualizado ✅", Color.FromArgb(44, 160, 44));
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error: " + ex.Message, Color.FromArgb(239, 83, 80));
            }
        }

        private string ObtenerTexto(TextBox t, string porDefecto = "")
            => (t.Text == "Escribe aquí..." || string.IsNullOrWhiteSpace(t.Text)) ? porDefecto : t.Text;

        private TextBox CrearCampo(Panel p, string titulo, int y, int x, int ancho)
        {
            Label lbl = new Label { Text = titulo, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(64, 64, 64) };
            p.Controls.Add(lbl);

            Panel pnlTxt = new Panel { Size = new Size(ancho, 35), Location = new Point(x, y + 22), BackColor = Color.White };
            p.Controls.Add(pnlTxt);
            RedondearControl(pnlTxt, 15);

            TextBox t = new TextBox { Size = new Size(ancho - 20, 20), Location = new Point(10, 8), BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, Text = "Escribe aquí..." };
            t.Enter += (s, e) => { if (t.Text == "Escribe aquí...") { t.Text = ""; t.ForeColor = Color.Black; } };
            t.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(t.Text)) { t.Text = "Escribe aquí..."; t.ForeColor = Color.Gray; } };
            pnlTxt.Controls.Add(t);
            return t;
        }

        private DateTimePicker CrearCalendario(Panel p, string titulo, int y, int x, int ancho)
        {
            Label lbl = new Label { Text = titulo, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(64, 64, 64) };
            p.Controls.Add(lbl);

            Panel pnlDtp = new Panel { Size = new Size(ancho, 35), Location = new Point(x, y + 22), BackColor = Color.White };
            p.Controls.Add(pnlDtp);
            RedondearControl(pnlDtp, 15);

            DateTimePicker dtp = new DateTimePicker { Size = new Size(ancho - 5, 25), Location = new Point(2, 6), Format = DateTimePickerFormat.Short };
            pnlDtp.Controls.Add(dtp);
            return dtp;
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
            Form toast = new Form { FormBorderStyle = FormBorderStyle.None, StartPosition = FormStartPosition.Manual, Size = new Size(340, 55), BackColor = color, Opacity = 0.95, TopMost = true, ShowInTaskbar = false };
            toast.Location = new Point(this.Left + (this.Width / 2) - 170, this.Top + this.Height - 80);
            Label lbl = new Label { Text = mensaje, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            toast.Controls.Add(lbl);
            toast.Show(this);
            var timer = new System.Windows.Forms.Timer { Interval = 2500 };
            timer.Tick += (s, ev) => { timer.Stop(); toast.Close(); };
            timer.Start();
        }
    }
}
