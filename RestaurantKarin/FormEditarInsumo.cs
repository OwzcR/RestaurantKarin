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
        private Color colorAzulBtn = Color.FromArgb(26, 90, 122);
        private Color colorGrisFondo = Color.FromArgb(230, 233, 235);
        private TextBox txtNombre, txtFecha, txtId, txtMinimo, txtActual, txtCosto, txtUnidad;

        public FormEditarInsumo()
        {
            InitializeComponent();
            SetupDiseno();
        }

        private void SetupDiseno()
        {
            this.Text = "Editar Insumo";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Size = new Size(800, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(45, 45, 48);

            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = colorAzulBtn };
            Label lblTit = new Label { Text = "EDITAR INSUMO EXISTENTE", Dock = DockStyle.Fill, ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            pnlHeader.Controls.Add(lblTit);
            this.Controls.Add(pnlHeader);

            Panel pnlCuerpo = new Panel { Size = new Size(740, 380), Location = new Point(25, 70), BackColor = colorGrisFondo };
            this.Controls.Add(pnlCuerpo);
            RedondearControl(pnlCuerpo, 15);

            txtNombre = CrearCampo(pnlCuerpo, "Nombre del insumo:", 20, 20, 450);
            txtFecha = CrearCampo(pnlCuerpo, "Última entrada:", 100, 20, 340);
            txtId = CrearCampo(pnlCuerpo, "ID del insumo (No editable):", 100, 380, 340);

            // BLOQUEO VISUAL DEL ID
            txtId.ReadOnly = true;
            txtId.BackColor = Color.FromArgb(170, 170, 170);
            txtId.Parent.BackColor = Color.FromArgb(170, 170, 170);

            txtMinimo = CrearCampo(pnlCuerpo, "Stock Mínimo:", 180, 20, 340);
            txtActual = CrearCampo(pnlCuerpo, "Stock Actual:", 180, 380, 340);
            txtCosto = CrearCampo(pnlCuerpo, "Costo Unitario:", 260, 20, 340);
            txtUnidad = CrearCampo(pnlCuerpo, "Unidad:", 260, 380, 340);

            Button btnGuardar = new Button { Text = "💾 GUARDAR CAMBIOS", Size = new Size(340, 50), Location = new Point(425, 470), BackColor = colorAzulBtn, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnGuardar.Click += GuardarCambios_Click;
            this.Controls.Add(btnGuardar);
            RedondearControl(btnGuardar, 20);

            Button btnCan = new Button { Text = "CANCELAR", Size = new Size(340, 50), Location = new Point(25, 470), BackColor = Color.FromArgb(180, 180, 180), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
            btnCan.Click += (s, e) => this.Close();
            this.Controls.Add(btnCan);
            RedondearControl(btnCan, 20);
        }

        public void CargarDatosParaEdicion(string id, string nom, string actual, string uni, string min, string fecha, string costo)
        {
            txtId.Text = id;
            txtNombre.Text = nom; txtNombre.ForeColor = Color.Black;
            txtActual.Text = actual; txtActual.ForeColor = Color.Black;
            txtUnidad.Text = uni; txtUnidad.ForeColor = Color.Black;
            txtMinimo.Text = min; txtMinimo.ForeColor = Color.Black;
            txtFecha.Text = fecha; txtFecha.ForeColor = Color.Black;
            txtCosto.Text = costo.Replace("$", "").Trim(); txtCosto.ForeColor = Color.Black;
        }

        private void GuardarCambios_Click(object sender, EventArgs e)
        {
            try
            {
                string cadena = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                using (var con = new SQLiteConnection(cadena))
                {
                    con.Open();
                    string q = "UPDATE Insumos SET Nombre=@n, StockActual=@sa, Unidad=@u, StockMinimo=@sm, FechaEntrada=@f, Costo=@c WHERE id_insumo=@id";
                    using (var cmd = new SQLiteCommand(q, con))
                    {
                        cmd.Parameters.AddWithValue("@n", txtNombre.Text);
                        cmd.Parameters.AddWithValue("@sa", txtActual.Text);
                        cmd.Parameters.AddWithValue("@u", txtUnidad.Text);
                        cmd.Parameters.AddWithValue("@sm", txtMinimo.Text);
                        cmd.Parameters.AddWithValue("@f", txtFecha.Text);
                        cmd.Parameters.AddWithValue("@c", txtCosto.Text);
                        cmd.Parameters.AddWithValue("@id", txtId.Text);
                        cmd.ExecuteNonQuery();
                    }
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
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
            pnlTxt.Controls.Add(t);
            return t;
        }

        private void RedondearControl(Control c, int r)
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddArc(0, 0, r, r, 180, 90); gp.AddArc(c.Width - r, 0, r, r, 270, 90);
            gp.AddArc(c.Width - r, c.Height - r, r, r, 0, 90); gp.AddArc(0, c.Height - r, r, r, 90, 90);
            c.Region = new Region(gp);
        }
    }
}