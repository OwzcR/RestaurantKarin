using System;
using System.Configuration;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public partial class FormAgregarInsumo : Form
    {
      
        private Color colorAzulBtn = Color.FromArgb(26, 90, 122);
        private Color colorGrisFondo = Color.FromArgb(230, 233, 235);

        // Variables para capturar los datos
        private TextBox txtNombre, txtFecha, txtId, txtMinimo, txtActual, txtCosto, txtUnidad;

        public FormAgregarInsumo()
        {
            InitializeComponent();
            SetupDiseno();
        }

        private void SetupDiseno()
        {
            this.Text = "Nuevo Insumo";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Size = new Size(800, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = colorAzulBtn };
            Label lblTit = new Label { Text = "AGREGAR NUEVO INSUMO", Dock = DockStyle.Fill, ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            pnlHeader.Controls.Add(lblTit);
            this.Controls.Add(pnlHeader);

            Panel pnlCuerpo = new Panel { Size = new Size(740, 380), Location = new Point(25, 70), BackColor = colorGrisFondo };
            this.Controls.Add(pnlCuerpo);
            RedondearControl(pnlCuerpo, 15);

            // Creación de campos
            txtNombre = CrearCampo(pnlCuerpo, "Nombre del insumo:", 20, 20, 450);
            txtFecha = CrearCampo(pnlCuerpo, "Última entrada:", 100, 20, 340);
            txtId = CrearCampo(pnlCuerpo, "ID del insumo:", 100, 380, 340);
            txtMinimo = CrearCampo(pnlCuerpo, "Stock Mínimo:", 180, 20, 340);
            txtActual = CrearCampo(pnlCuerpo, "Stock Actual:", 180, 380, 340);
            txtCosto = CrearCampo(pnlCuerpo, "Costo Unitario:", 260, 20, 340);
            txtUnidad = CrearCampo(pnlCuerpo, "Unidad:", 260, 380, 340);

            // Botón Guardar
            Button btnGuardar = new Button { Text = "+ GUARDAR INSUMO", Size = new Size(340, 50), Location = new Point(425, 470), BackColor = colorAzulBtn, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnGuardar.Click += GuardarInsumo_Click;
            this.Controls.Add(btnGuardar);
            RedondearControl(btnGuardar, 20);

            // Botón Cancelar
            Button btnCan = new Button { Text = "CANCELAR", Size = new Size(340, 50), Location = new Point(25, 470), BackColor = Color.FromArgb(180, 180, 180), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCan.Click += (s, e) => this.Close();
            this.Controls.Add(btnCan);
            RedondearControl(btnCan, 20);
        }

        private void GuardarInsumo_Click(object sender, EventArgs e)
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
                    string q = @"INSERT INTO Insumos (Nombre, StockActual, Unidad, StockMinimo, FechaEntrada, Costo) 
                                 VALUES (@n, @sa, @u, @sm, @f, @c)";

                    using (var cmd = new SQLiteCommand(q, con))
                    {
                        cmd.Parameters.AddWithValue("@n", nombre);
                        cmd.Parameters.AddWithValue("@sa", Convert.ToDouble(ObtenerTexto(txtActual, "0")));
                        cmd.Parameters.AddWithValue("@u", ObtenerTexto(txtUnidad, "N/A"));
                        cmd.Parameters.AddWithValue("@sm", Convert.ToDouble(ObtenerTexto(txtMinimo, "0")));
                        cmd.Parameters.AddWithValue("@f", ObtenerTexto(txtFecha, DateTime.Now.ToString("dd/MM/yyyy")));
                        cmd.Parameters.AddWithValue("@c", Convert.ToDouble(ObtenerTexto(txtCosto, "0")));

                        cmd.ExecuteNonQuery();
                    }
                }

                MostrarMensaje("Insumo guardado con éxito ✅", Color.FromArgb(44, 160, 44));
                this.DialogResult = DialogResult.OK; 
                this.Close();
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error: " + ex.Message, Color.FromArgb(239, 83, 80));
            }
        }

        private string ObtenerTexto(TextBox t, string porDefecto = "")
        {
            return (t.Text == "Escribe aquí..." || string.IsNullOrWhiteSpace(t.Text)) ? porDefecto : t.Text;
        }

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
            Form toast = new Form();
            toast.FormBorderStyle = FormBorderStyle.None;
            toast.StartPosition = FormStartPosition.Manual;
            toast.Size = new Size(340, 55);
            toast.BackColor = color;
            toast.Opacity = 0.95;
            toast.TopMost = true;
            toast.ShowInTaskbar = false;
            toast.Location = new Point(this.Left + (this.Width / 2) - 170, this.Top + this.Height - 80);

            Label lbl = new Label { Text = mensaje, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            toast.Controls.Add(lbl);
            toast.Show(this);

            var timer = new System.Windows.Forms.Timer { Interval = 2500 };
            timer.Tick += (s, e) => { timer.Stop(); toast.Close(); };
            timer.Start();
        }
    }
}