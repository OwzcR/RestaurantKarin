using System;
using System.Configuration;
using System.Data.SQLite; 
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
            DatabaseHelper.InicializarBaseDeDatos();
            SetupUI();
        }

        private void SetupUI()
        {
            // ===== 1. CONFIGURACIÓN DEL FORMULARIO BASE =====
            this.Text = "Login - Restaurante Karin";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            
            this.AutoScaleMode = AutoScaleMode.None;

            // RUTAS DINÁMICAS: Encuentra la carpeta Imgs sin importar en qué PC estés
            string rutaFondo = Path.Combine(Application.StartupPath, "Imgs", "fondo.png");
            string rutaLogo = Path.Combine(Application.StartupPath, "Imgs", "icono.ico");
            string rutaIcono = Path.Combine(Application.StartupPath, "Imgs", "icono.ico"); // <-- Tu nuevo icono

            // --- ICONO DE LA VENTANA Y BARRA DE TAREAS ---
            try
            {
                this.Icon = new Icon(rutaIcono);
            }
            catch { /* Si no encuentra el .ico, usa el de por defecto de Windows */ }

            // --- FONDO DE PANTALLA ---
            this.BackColor = Color.FromArgb(29, 53, 87);
            try
            {
                this.BackgroundImage = Image.FromFile(rutaFondo);
                this.BackgroundImageLayout = ImageLayout.Stretch;
            }
            catch { }

            // ===== 2. EL CONTENEDOR MAESTRO =====
            TableLayoutPanel tlpBase = new TableLayoutPanel();
            tlpBase.Dock = DockStyle.Fill;
            tlpBase.BackColor = Color.Transparent;
            tlpBase.ColumnCount = 3;
            tlpBase.RowCount = 3;

            tlpBase.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            // Aumentamos un poquito el ancho y alto absoluto para darle respiro a los controles
            tlpBase.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 340F));
            tlpBase.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            tlpBase.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpBase.RowStyles.Add(new RowStyle(SizeType.Absolute, 540F));
            tlpBase.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            this.Controls.Add(tlpBase);

            // ===== 3. ELEMENTOS DE LAS ESQUINAS =====
            PictureBox picLogo = new PictureBox();
            picLogo.Size = new Size(130, 130);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.Location = new Point(20, 20);
            picLogo.BackColor = Color.Transparent;

            try
            {
                picLogo.Image = Image.FromFile(rutaLogo);
            }
            catch
            {
                picLogo.BorderStyle = BorderStyle.FixedSingle;
                picLogo.BackColor = Color.Red;
            }

            this.Controls.Add(picLogo);
            picLogo.BringToFront();

            Label lblAviso = new Label();
            lblAviso.Text = "¿Olvidaste tu PIN?\nContacta al administrador del sistema.";
            lblAviso.ForeColor = Color.White;
            lblAviso.BackColor = Color.Transparent;
            lblAviso.Font = new Font("Segoe UI", 8, FontStyle.Italic | FontStyle.Bold);
            lblAviso.AutoSize = true;
            lblAviso.Location = new Point(20, this.ClientSize.Height - 60);
            lblAviso.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            this.Controls.Add(lblAviso);
            lblAviso.BringToFront();

            // ===== 4. LA TARJETA BLANCA DE LOGIN =====
            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.BackColor = Color.White;
            card.Margin = new Padding(0);
            tlpBase.Controls.Add(card, 1, 1);

            // ===== 5. CONTROLES DENTRO DE LA TARJETA =====
            Label lblTitulo = new Label();
            lblTitulo.Text = "INICIAR SESIÓN";
            lblTitulo.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(29, 53, 87);
            lblTitulo.Size = new Size(340, 40); // Ajustado al nuevo ancho de la tarjeta
            lblTitulo.Location = new Point(0, 30);
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            card.Controls.Add(lblTitulo);

            TextBox txtPIN = new TextBox();
            txtPIN.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            txtPIN.Size = new Size(240, 45);
            txtPIN.Location = new Point(50, 85); // Centrado ajustado
            txtPIN.TextAlign = HorizontalAlignment.Center;
            txtPIN.UseSystemPasswordChar = true;
            txtPIN.BackColor = Color.FromArgb(245, 245, 245);
            txtPIN.BorderStyle = BorderStyle.FixedSingle;
            card.Controls.Add(txtPIN);

            // Teclado
            int startX = 65; // Ajustado para centrar mejor
            int startY = 135;
            int spacing = 75; // Un poco más de espacio entre botones
            int count = 1;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Button btn = CrearBotonNumero(count.ToString(), txtPIN);
                    btn.Location = new Point(startX + (j * spacing), startY + (i * spacing));
                    card.Controls.Add(btn);
                    count++;
                }
            }

            Button btn0 = CrearBotonNumero("0", txtPIN);
            btn0.Location = new Point(startX + spacing, startY + (3 * spacing));
            card.Controls.Add(btn0);

            Button btnBorrar = new Button();
            btnBorrar.Text = "Borrar";
            btnBorrar.Size = new Size(100, 35);
            btnBorrar.Location = new Point(120, 425);
            btnBorrar.BackColor = Color.FromArgb(239, 83, 80);
            btnBorrar.ForeColor = Color.White;
            btnBorrar.FlatStyle = FlatStyle.Flat;
            btnBorrar.FlatAppearance.BorderSize = 0;
            btnBorrar.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnBorrar.Cursor = Cursors.Hand;
            btnBorrar.Click += (s, e) => { txtPIN.Clear(); };
            card.Controls.Add(btnBorrar);

            Button btnEntrar = new Button();
            btnEntrar.Text = "ENTRAR";
            btnEntrar.Size = new Size(240, 45);
            btnEntrar.Location = new Point(50, 475);
            btnEntrar.BackColor = Color.FromArgb(29, 53, 87);
            btnEntrar.ForeColor = Color.White;
            btnEntrar.FlatStyle = FlatStyle.Flat;
            btnEntrar.FlatAppearance.BorderSize = 0;
            btnEntrar.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnEntrar.Cursor = Cursors.Hand;

            btnEntrar.Click += (s, e) =>
            {
                string pinIngresado = txtPIN.Text;
                if (pinIngresado.Length == 0) return;

                string cadenaConexion = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;

                try
                {
                    using (SQLiteConnection conexion = new SQLiteConnection(cadenaConexion))
                    {
                        conexion.Open();

                        string query = "SELECT nombre, rol FROM usuario WHERE pin_acceso = @pin AND estado = 1";

                        using (SQLiteCommand comando = new SQLiteCommand(query, conexion))
                        {
                            comando.Parameters.AddWithValue("@pin", pinIngresado);

                            using (SQLiteDataReader lector = comando.ExecuteReader())
                            {
                                if (lector.Read())
                                {
                                    string nombreUsuario = lector["nombre"].ToString();
                                    string rolUsuario = lector["rol"].ToString();

                                    MessageBox.Show($"¡Acceso Autorizado!\n\nBienvenido(a): {nombreUsuario}\nRol: {rolUsuario}",
                                                    "Login Exitoso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    txtPIN.Clear();

                                    Base principal = new Base();
                                    principal.FormClosed += (senderArgs, evtArgs) => Application.Exit();
                                    principal.Show();
                                    this.Hide();
                                }
                                else
                                {
                                    MessageBox.Show("El PIN ingresado es incorrecto o el usuario está inactivo.",
                                                    "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    txtPIN.Clear();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo acceder a la base de datos local.\n\nDetalle: " + ex.Message,
                                    "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            card.Controls.Add(btnEntrar);
        }

        private Button CrearBotonNumero(string numero, TextBox txt)
        {
            Button btn = new Button();
            btn.Text = numero;
            btn.Size = new Size(60, 60);
            btn.BackColor = Color.FromArgb(243, 244, 246);
            btn.ForeColor = Color.FromArgb(51, 51, 51);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 220, 220);

            btn.Click += (s, e) =>
            {
                if (txt.Text.Length < 6)
                    txt.Text += numero;
            };

            return btn;
        }
    }
}