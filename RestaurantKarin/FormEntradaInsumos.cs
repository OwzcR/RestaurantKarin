using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Configuration;

namespace RestaurantKarin
{
    public partial class FormEntradaInsumos : Form
    {
       
        private string idInsumo;
        private Label lblNombreInsumo;
        private TextBox txtUnidad;
        private TextBox txtCantidad;
        private TextBox txtCosto;
        private DateTimePicker dtpCaducidad;

        private Color colorAzulTitulo = Color.FromArgb(29, 53, 87);
        private Color colorBtnCargar = Color.FromArgb(26, 90, 122);

        public FormEntradaInsumos()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Size = new Size(450, 400);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = colorAzulTitulo };
            Label lblTitulo = new Label
            {
                Text = "ENTRADA DE INSUMOS",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(15, 12),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitulo);
            this.Controls.Add(pnlHeader);

            // Insumo Seleccionado
            Label lblEstatico = new Label { Text = "Insumo Seleccionado:", Location = new Point(20, 70), AutoSize = true };
            lblNombreInsumo = new Label
            { 
                Text = "...",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                Location = new Point(160, 70),
                ForeColor = Color.Gray,
                AutoSize = true
            };

            
            Label lblUni = new Label { Text = "Unidad", Location = new Point(230, 110), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            txtUnidad = new TextBox
            {
                Location = new Point(230, 135),
                Width = 180,
                ReadOnly = true,
                BackColor = Color.FromArgb(240, 244, 248)
            };

            
            Label lblCant = new Label { Text = "Cantidad Entrante", Location = new Point(20, 110), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            txtCantidad = new TextBox { Location = new Point(20, 135), Width = 180 };

            
            Label lblCos = new Label { Text = "Costo Unitario", Location = new Point(20, 190), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            txtCosto = new TextBox { Location = new Point(20, 215), Width = 180 };

            
            Button btnCargar = new Button
            {
                Text = "CARGAR AL INVENTARIO",
                Size = new Size(180, 40),
                Location = new Point(230, 330),
                BackColor = colorBtnCargar,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCargar.Click += BtnCargar_Click;

            Button btnCancelar = new Button
            {
                Text = "CANCELAR",
                Size = new Size(100, 40),
                Location = new Point(120, 330),
                FlatStyle = FlatStyle.Flat
            };
            btnCancelar.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblEstatico, lblNombreInsumo, lblUni, txtUnidad,
                lblCant, txtCantidad, lblCos, txtCosto, btnCargar, btnCancelar
            });
        }

       
        public void CargarDatos(string id, string nombre, string unidad)
        {
            this.idInsumo = id;
            lblNombreInsumo.Text = nombre;
            txtUnidad.Text = unidad;
        }

        private void BtnCargar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCantidad.Text)) return;

            try
            {
                string cadena = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                using (SQLiteConnection conn = new SQLiteConnection(cadena))
                {
                    conn.Open();
                    
                    string query = "UPDATE Insumos SET StockActual = StockActual + @cant, Costo = @costo, FechaEntrada = @fecha WHERE id_insumo = @id";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@cant", txtCantidad.Text);
                        cmd.Parameters.AddWithValue("@costo", txtCosto.Text.Replace("$", ""));
                        cmd.Parameters.AddWithValue("@fecha", DateTime.Now.ToString("dd/MM/yyyy"));
                        cmd.Parameters.AddWithValue("@id", idInsumo);
                        cmd.ExecuteNonQuery();
                    }
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
    }
}