using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Configuration;

namespace RestaurantKarin
{
    public partial class FormEliminarInsumo : Form
    {
        private string idInsumo;
        private Label lblNombreInsumo;

        // Colores consistentes con Restaurant Karin
        private Color colorAzulTitulo = Color.FromArgb(26, 90, 122);
        private Color colorTextoGris = Color.FromArgb(80, 80, 80);
        private Color colorBotonConfirmar = Color.FromArgb(14, 77, 110);
        private Color colorBotonCancelar = Color.FromArgb(230, 233, 235);

        public FormEliminarInsumo()
        {
            InitializeComponent();
            SetupDisenoEstandar();
        }

        private void SetupDisenoEstandar()
        {
            // --- CONFIGURACIÓN DE VENTANA ---
            this.Text = "Eliminar Insumo";
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.Size = new Size(580, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.MinimizeBox = true;
            this.MaximizeBox = true;

            // --- CONTENIDO CENTRAL ---
            Label lblMainTitle = new Label
            {
                Text = "¿ELIMINAR INSUMO?",
                Font = new Font("Segoe UI Semibold", 22, FontStyle.Bold),
                ForeColor = colorAzulTitulo,
                TextAlign = ContentAlignment.BottomCenter,
                Dock = DockStyle.Top,
                Height = 90
            };

            lblNombreInsumo = new Label
            {
                Text = "\"CARGANDO...\"",
                Font = new Font("Segoe UI Semilight", 16, FontStyle.Italic),
                ForeColor = Color.Silver,
                TextAlign = ContentAlignment.TopCenter,
                Dock = DockStyle.Top,
                Height = 50
            };

            // Ajuste de altura para que la descripción no se corte
            Label lblAviso = new Label
            {
                Text = "Atención: El insumo seleccionado y toda su información asociada (existencias, costos, etc.) serán eliminados permanentemente.",
                Font = new Font("Segoe UI", 12),
                ForeColor = colorTextoGris,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 100,
                Padding = new Padding(50, 10, 50, 10)
            };

            Label lblContinuar = new Label
            {
                Text = "¿Deseas continuar?",
                Font = new Font("Segoe UI Semilight", 14),
                ForeColor = colorTextoGris,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50
            };

            // --- PANEL DE BOTONES CENTRADOS ---
            FlowLayoutPanel pnlBotonesCentrados = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 100,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            Button btnNo = new Button
            {
                Text = "CANCELAR",
                Size = new Size(170, 50),
                BackColor = colorBotonCancelar,
                ForeColor = Color.FromArgb(70, 70, 70),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(12, 0, 12, 0)
            };
            btnNo.FlatAppearance.BorderSize = 0;
            btnNo.Click += (s, e) => this.Close();

            Button btnSi = new Button
            {
                Text = "SÍ, ELIMINAR",
                Size = new Size(170, 50),
                BackColor = colorBotonConfirmar,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(12, 0, 12, 0)
            };
            btnSi.FlatAppearance.BorderSize = 0;
            btnSi.Click += BtnSi_Click; // Lógica de BD vinculada

            // Lógica para centrar botones dinámicamente
            pnlBotonesCentrados.SizeChanged += (s, e) => {
                int totalWidth = btnNo.Width + btnSi.Width + (btnNo.Margin.Horizontal * 2) + (btnSi.Margin.Horizontal * 2);
                pnlBotonesCentrados.Padding = new Padding((pnlBotonesCentrados.Width - totalWidth) / 2, 20, 0, 0);
            };

            pnlBotonesCentrados.Controls.Add(btnNo);
            pnlBotonesCentrados.Controls.Add(btnSi);

            this.Controls.Add(pnlBotonesCentrados);
            this.Controls.Add(lblContinuar);
            this.Controls.Add(lblAviso);
            this.Controls.Add(lblNombreInsumo);
            this.Controls.Add(lblMainTitle);
        }

        public void CargarDatos(string id, string nombre)
        {
            this.idInsumo = id;
            lblNombreInsumo.Text = $"\"{nombre.ToUpper()}\"";
        }

        // Lógica de eliminación en base de datos
        private void BtnSi_Click(object sender, EventArgs e)
        {
            try
            {
                string cadena = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                using (SQLiteConnection conn = new SQLiteConnection(cadena))
                {
                    conn.Open();
                    string query = "DELETE FROM Insumos WHERE id_insumo = @id";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", idInsumo);
                        cmd.ExecuteNonQuery();
                    }
                }

                MostrarMensaje("Insumo eliminado con éxito ✅", Color.FromArgb(44, 160, 44));
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception)
            {
                MostrarMensaje("Error al eliminar ❌", Color.FromArgb(239, 83, 80));
            }
        }

        private void MostrarMensaje(string mensaje, Color color)
        {
            Form toast = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                Size = new Size(280, 45),
                BackColor = color,
                Opacity = 0.95,
                TopMost = true,
                ShowInTaskbar = false
            };

            toast.Location = new Point(this.Left + (this.Width / 2) - 140, this.Top + this.Height - 80);
            Label lbl = new Label
            {
                Text = mensaje,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            toast.Controls.Add(lbl);
            toast.Show(this);

            // Solución al error de ambigüedad del Timer
            System.Windows.Forms.Timer tCierre = new System.Windows.Forms.Timer { Interval = 2000 };
            tCierre.Tick += (s, e) => {
                tCierre.Stop();
                toast.Close();
                tCierre.Dispose();
            };
            tCierre.Start();
        }
    }
}