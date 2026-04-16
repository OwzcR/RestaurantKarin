using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data.SQLite;
using System.Drawing;
using System.Runtime.Intrinsics.Arm;
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

            // --- FONDO DE PANTALLA ---
            this.BackColor = Color.FromArgb(29, 53, 87); // Color oscuro de respaldo por si falla la imagen
            try
            {
                this.BackgroundImage = Image.FromFile(@"C:\Users\oscar\source\repos\RestaurantKarin\RestaurantKarin\Imgs\fondo.png");
                this.BackgroundImageLayout = ImageLayout.Stretch;
            }
            catch { }


            // ===== 2. EL CONTENEDOR MAESTRO (La cuadrícula invisible) =====
            TableLayoutPanel tlpBase = new TableLayoutPanel();
            tlpBase.Dock = DockStyle.Fill;
            tlpBase.BackColor = Color.Transparent; 
            tlpBase.ColumnCount = 3;
            tlpBase.RowCount = 3;

            tlpBase.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpBase.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320F));
            tlpBase.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            tlpBase.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlpBase.RowStyles.Add(new RowStyle(SizeType.Absolute, 520F));
            tlpBase.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            this.Controls.Add(tlpBase);


            // ===== 3. ELEMENTOS DE LAS ESQUINAS =====

            // A) LOGO (Arriba a la izquierda -> Columna 0, Fila 0)
            PictureBox picLogo = new PictureBox();
            picLogo.Size = new Size(130, 130);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.Location = new Point(20, 20); // Lo fijamos a 20 pixeles de las orillas
            picLogo.BackColor = Color.Transparent; // Para que deje ver la playa de fondo

            try
            {
                // OJO: Verifica que esta ruta sea 100% correcta
                picLogo.Image = Image.FromFile(@"C:\Users\oscar\source\repos\RestaurantKarin\RestaurantKarin\Imgs\logo.png");
            }
            catch
            {
                // Si la ruta falla, le ponemos un borde rojo temporalmente para que sepas que el recuadro sí está ahí
                picLogo.BorderStyle = BorderStyle.FixedSingle;
                picLogo.BackColor = Color.Red;
            }

            this.Controls.Add(picLogo);
            picLogo.BringToFront();

            // B) AVISO ADMINISTRADOR (Abajo a la izquierda -> Columna 0, Fila 2)
            Label lblAviso = new Label();
            lblAviso.Text = "¿Olvidaste tu PIN?\nContacta al administrador del sistema.";
            lblAviso.ForeColor = Color.White;
            lblAviso.BackColor = Color.Transparent; // Fundamental para que el fondo se vea
            lblAviso.Font = new Font("Segoe UI", 10, FontStyle.Italic | FontStyle.Bold);
            lblAviso.AutoSize = true;

            // Calculamos su posición justo en la esquina inferior
            lblAviso.Location = new Point(20, this.ClientSize.Height - 60);
            // Le decimos a Windows que, si se cambia el tamaño de la ventana, lo deje anclado abajo
            lblAviso.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            this.Controls.Add(lblAviso);
            lblAviso.BringToFront();


            // ===== 4. LA TARJETA BLANCA DE LOGIN =====
            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.BackColor = Color.White;
            card.Margin = new Padding(0);

            // Se añade al centro (Columna 1, Fila 1)
            tlpBase.Controls.Add(card, 1, 1);


            // ===== 5. CONTROLES DENTRO DE LA TARJETA =====
            // (Este código es el mismo que ya habías ajustado)

            Label lblTitulo = new Label();
            lblTitulo.Text = "INICIAR SESIÓN";
            lblTitulo.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(29, 53, 87);
            lblTitulo.Size = new Size(320, 40);
            lblTitulo.Location = new Point(0, 30);
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            card.Controls.Add(lblTitulo);

            TextBox txtPIN = new TextBox();
            txtPIN.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            txtPIN.Size = new Size(240, 45);
            txtPIN.Location = new Point(40, 85);
            txtPIN.TextAlign = HorizontalAlignment.Center;
            txtPIN.UseSystemPasswordChar = true;
            txtPIN.BackColor = Color.FromArgb(245, 245, 245);
            txtPIN.BorderStyle = BorderStyle.FixedSingle;
            card.Controls.Add(txtPIN);

            // Teclado
            int startX = 55;
            int startY = 135;
            int spacing = 70;
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
            btnBorrar.Location = new Point(110, 415);
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
            btnEntrar.Location = new Point(40, 460);
            btnEntrar.BackColor = Color.FromArgb(29, 53, 87);
            btnEntrar.ForeColor = Color.White;
            btnEntrar.FlatStyle = FlatStyle.Flat;
            btnEntrar.FlatAppearance.BorderSize = 0;
            btnEntrar.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnEntrar.Cursor = Cursors.Hand;
            // Botón Entrar (Azul oscuro principal)
            btnEntrar.Click += (s, e) =>
            {
                string pinIngresado = txtPIN.Text;
                if (pinIngresado.Length == 0) return; // Si está vacío, no hace nada

                string cadenaConexion = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;

                try
                {
                    // CAMBIO 1: Usamos SQLiteConnection en lugar de MySqlConnection
                    using (SQLiteConnection conexion = new SQLiteConnection(cadenaConexion))
                    {
                        conexion.Open(); // Abrimos el archivo local

                        // CAMBIO 2: Cambiamos "estado = TRUE" por "estado = 1" (Así lo maneja SQLite)
                        string query = "SELECT nombre, rol FROM usuario WHERE pin_acceso = @pin AND estado = 1";

                        // CAMBIO 3: Usamos SQLiteCommand
                        using (SQLiteCommand comando = new SQLiteCommand(query, conexion))
                        {
                            comando.Parameters.AddWithValue("@pin", pinIngresado);

                            // CAMBIO 4: Usamos SQLiteDataReader
                            using (SQLiteDataReader lector = comando.ExecuteReader())
                            {
                                if (lector.Read()) // Si encontró una fila, el PIN es correcto
                                {
                                  
                                    string nombreUsuario = lector["nombre"].ToString();
                                    string rolUsuario = lector["rol"].ToString();

                                    MessageBox.Show($"¡Acceso Autorizado!\n\nBienvenido(a): {nombreUsuario}\nRol: {rolUsuario}",
                                                    "Login Exitoso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    txtPIN.Clear();

                                    Base principal = new Base();

                                    principal.FormClosed += (s, args) => Application.Exit();

                                    principal.Show();
                                    this.Hide();
                                }
                                else // Si no encontró nada, el PIN no existe
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
                    // Mensaje de error actualizado para entorno local
                    MessageBox.Show("No se pudo acceder a la base de datos local.\n\nDetalle: " + ex.Message,
                                    "Error de Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            card.Controls.Add(btnEntrar);
        }

        // Método auxiliar para no repetir código al crear los botones numéricos
        private Button CrearBotonNumero(string numero, TextBox txt)
        {
            Button btn = new Button();
            btn.Text = numero;
            btn.Size = new Size(60, 60);
            btn.BackColor = Color.FromArgb(243, 244, 246); // Gris súper claro (#F3F4F6)
            btn.ForeColor = Color.FromArgb(51, 51, 51); // Texto oscuro
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 220, 220);

            btn.Click += (s, e) =>
            {
                if (txt.Text.Length < 6) // Límite de 6 dígitos para el PIN
                    txt.Text += numero;
            };

            return btn;
        }
    }
}