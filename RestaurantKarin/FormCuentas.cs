using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public partial class FormCuentas : Form
    {
        private DataGridView dgvCuentas;
        private string cadenaConexion = "Data Source=karin_pos.db;Version=3;";

        public FormCuentas()
        {
            ConfigurarInterfaz();
            CargarHistorial();
        }

        private void ConfigurarInterfaz()
        {
            this.Text = "Historial de Cuentas - Restaurant Karin";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            Label lblTitulo = new Label
            {
                Text = "HISTORIAL DE CUENTAS",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true,
                ForeColor = Color.DarkSlateGray
            };

            dgvCuentas = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(740, 350),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            this.Controls.Add(lblTitulo);
            this.Controls.Add(dgvCuentas);
        }

        private void CargarHistorial()
        {
            try
            {
                using (SQLiteConnection conexion = new SQLiteConnection(cadenaConexion))
                {
                    conexion.Open();
                    // Consulta que une la cuenta con el nombre del usuario que la abrió
                    string query = @"
                        SELECT 
                            c.id_cuenta AS 'Folio', 
                            c.id_mesa AS 'Mesa', 
                            u.nombre AS 'Atendió', 
                            c.fecha_apertura AS 'Fecha', 
                            c.estado_cuenta AS 'Estado', 
                            c.total AS 'Total ($)'
                        FROM cuenta c
                        JOIN usuario u ON c.id_usuario_apertura = u.id_usuario
                        ORDER BY c.fecha_apertura DESC";

                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, conexion);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvCuentas.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar cuentas: " + ex.Message);
            }
        }
    }
}