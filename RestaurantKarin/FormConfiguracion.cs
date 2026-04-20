using System;
using System.Configuration;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public partial class FormConfiguracion : Form
    {
        public FormConfiguracion()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Ajustes";
            this.Size = new Size(560, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(29, 53, 87);

            string rutaFondo = Path.Combine(Application.StartupPath, "Imgs", "fondo.png");
            try
            {
                this.BackgroundImage = Image.FromFile(rutaFondo);
                this.BackgroundImageLayout = ImageLayout.Stretch;
            }
            catch { }

            // ===== TARJETA BLANCA =====
            Panel card = new Panel();
            card.Size = new Size(460, 410);
            card.Location = new Point(50, 50);
            card.BackColor = Color.White;
            card.BorderStyle = BorderStyle.None;
            this.Controls.Add(card);

            // Título
            Label lblTitulo = new Label();
            lblTitulo.Text = "Cambiar PIN";
            lblTitulo.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(29, 53, 87);
            lblTitulo.Size = new Size(460, 45);
            lblTitulo.Location = new Point(0, 20);
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            card.Controls.Add(lblTitulo);

            // Línea separadora
            Panel sep = new Panel();
            sep.Size = new Size(400, 2);
            sep.Location = new Point(30, 68);
            sep.BackColor = Color.FromArgb(220, 220, 220);
            card.Controls.Add(sep);

            // ===== PIN ACTUAL =====
            Label lblActual = new Label();
            lblActual.Text = "PIN actual";
            lblActual.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblActual.ForeColor = Color.FromArgb(80, 80, 80);
            lblActual.Location = new Point(30, 88);
            lblActual.AutoSize = true;
            card.Controls.Add(lblActual);

            TextBox txtActual = new TextBox();
            txtActual.Size = new Size(360, 35);
            txtActual.Location = new Point(30, 108);
            txtActual.Font = new Font("Segoe UI", 13, FontStyle.Regular);
            txtActual.UseSystemPasswordChar = true;
            txtActual.BackColor = Color.FromArgb(245, 245, 245);
            txtActual.BorderStyle = BorderStyle.FixedSingle;
            txtActual.MaxLength = 4;
            card.Controls.Add(txtActual);

            Button btnVerActual = CrearBotonOjo(txtActual);
            btnVerActual.Location = new Point(395, 108);
            card.Controls.Add(btnVerActual);

            // ===== PIN NUEVO =====
            Label lblNuevo = new Label();
            lblNuevo.Text = "Nuevo PIN";
            lblNuevo.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblNuevo.ForeColor = Color.FromArgb(80, 80, 80);
            lblNuevo.Location = new Point(30, 158);
            lblNuevo.AutoSize = true;
            card.Controls.Add(lblNuevo);

            TextBox txtNuevo = new TextBox();
            txtNuevo.Size = new Size(360, 35);
            txtNuevo.Location = new Point(30, 178);
            txtNuevo.Font = new Font("Segoe UI", 13, FontStyle.Regular);
            txtNuevo.UseSystemPasswordChar = true;
            txtNuevo.BackColor = Color.FromArgb(245, 245, 245);
            txtNuevo.BorderStyle = BorderStyle.FixedSingle;
            txtNuevo.MaxLength = 4;
            card.Controls.Add(txtNuevo);

            Button btnVerNuevo = CrearBotonOjo(txtNuevo);
            btnVerNuevo.Location = new Point(395, 178);
            card.Controls.Add(btnVerNuevo);

            // Indicador de seguridad
            Label lblSeguridad = new Label();
            lblSeguridad.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lblSeguridad.Location = new Point(30, 218);
            lblSeguridad.Size = new Size(400, 22);
            lblSeguridad.ForeColor = Color.Gray;
            card.Controls.Add(lblSeguridad);

            txtNuevo.TextChanged += (s, e) =>
            {
                string v = txtNuevo.Text;
                if (v.Length == 0) { lblSeguridad.Text = ""; return; }
                if (v.Length < 4) { lblSeguridad.Text = "PIN incompleto ❌"; lblSeguridad.ForeColor = Color.Red; }
                else { lblSeguridad.Text = "PIN listo ✅"; lblSeguridad.ForeColor = Color.Green; }
            };

            // ===== CONFIRMAR PIN =====
            Label lblConfirmar = new Label();
            lblConfirmar.Text = "Confirmar nuevo PIN";
            lblConfirmar.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblConfirmar.ForeColor = Color.FromArgb(80, 80, 80);
            lblConfirmar.Location = new Point(30, 248);
            lblConfirmar.AutoSize = true;
            card.Controls.Add(lblConfirmar);

            TextBox txtConfirmar = new TextBox();
            txtConfirmar.Size = new Size(360, 35);
            txtConfirmar.Location = new Point(30, 268);
            txtConfirmar.Font = new Font("Segoe UI", 13, FontStyle.Regular);
            txtConfirmar.UseSystemPasswordChar = true;
            txtConfirmar.BackColor = Color.FromArgb(245, 245, 245);
            txtConfirmar.BorderStyle = BorderStyle.FixedSingle;
            txtConfirmar.MaxLength = 4;
            card.Controls.Add(txtConfirmar);

            Button btnVerConfirmar = CrearBotonOjo(txtConfirmar);
            btnVerConfirmar.Location = new Point(395, 268);
            card.Controls.Add(btnVerConfirmar);

            // ===== BOTONES =====
            Button btnActualizar = new Button();
            btnActualizar.Text = "Actualizar";
            btnActualizar.Size = new Size(140, 40);
            btnActualizar.Location = new Point(30, 330);
            btnActualizar.BackColor = Color.FromArgb(29, 53, 87);
            btnActualizar.ForeColor = Color.White;
            btnActualizar.FlatStyle = FlatStyle.Flat;
            btnActualizar.FlatAppearance.BorderSize = 0;
            btnActualizar.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnActualizar.Cursor = Cursors.Hand;
            card.Controls.Add(btnActualizar);

            Button btnCancelar = new Button();
            btnCancelar.Text = "Cancelar";
            btnCancelar.Size = new Size(120, 40);
            btnCancelar.Location = new Point(185, 330);
            btnCancelar.BackColor = Color.FromArgb(200, 200, 200);
            btnCancelar.ForeColor = Color.FromArgb(50, 50, 50);
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnCancelar.Cursor = Cursors.Hand;
            btnCancelar.Click += (s, e) =>
            {
                txtActual.Clear();
                txtNuevo.Clear();
                txtConfirmar.Clear();
                lblSeguridad.Text = "";
            };
            card.Controls.Add(btnCancelar);

            // ===== LÓGICA ACTUALIZAR =====
            btnActualizar.Click += (s, e) =>
            {
                string actual = txtActual.Text.Trim();
                string nuevo = txtNuevo.Text.Trim();
                string confirmar = txtConfirmar.Text.Trim();

                if (actual == "" || nuevo == "" || confirmar == "")
                {
                    MostrarMensaje("Llena todos los campos ❌", Color.FromArgb(239, 83, 80));
                    return;
                }

                if (nuevo != confirmar)
                {
                    MostrarMensaje("Los PINs no coinciden ❌", Color.FromArgb(239, 83, 80));
                    return;
                }

                if (nuevo.Length != 4)
                {
                    MostrarMensaje("El PIN debe tener exactamente 4 dígitos ❌", Color.FromArgb(239, 83, 80));
                    return;
                }

                var confirmResult = MessageBox.Show(
                    "¿Estás seguro de que deseas cambiar el PIN?",
                    "Confirmar cambio",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmResult != DialogResult.Yes) return;

                try
                {
                    string cadena = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                    using (var con = new SQLiteConnection(cadena))
                    {
                        con.Open();

                        // Verificar PIN actual
                        string queryVerify = "SELECT COUNT(*) FROM usuario WHERE pin_acceso = @actual AND estado = 1";
                        using (var cmd = new SQLiteCommand(queryVerify, con))
                        {
                            cmd.Parameters.AddWithValue("@actual", actual);
                            long existe = (long)cmd.ExecuteScalar();
                            if (existe == 0)
                            {
                                MostrarMensaje("El PIN actual es incorrecto ❌", Color.FromArgb(239, 83, 80));
                                return;
                            }
                        }

                        // Actualizar PIN
                        string queryUpdate = "UPDATE usuario SET pin_acceso = @nuevo WHERE pin_acceso = @actual";
                        using (var cmd = new SQLiteCommand(queryUpdate, con))
                        {
                            cmd.Parameters.AddWithValue("@nuevo", nuevo);
                            cmd.Parameters.AddWithValue("@actual", actual);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MostrarMensaje("PIN actualizado correctamente ✅", Color.FromArgb(44, 160, 44));
                    txtActual.Clear();
                    txtNuevo.Clear();
                    txtConfirmar.Clear();
                    lblSeguridad.Text = "";
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al actualizar: " + ex.Message, Color.FromArgb(239, 83, 80));
                }
            };
        }

        // ===== BOTÓN OJO =====
        private Button CrearBotonOjo(TextBox txt)
        {
            Button btn = new Button();
            btn.Text = "👁";
            btn.Size = new Size(38, 35);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.FromArgb(230, 230, 230);
            btn.Cursor = Cursors.Hand;
            btn.Font = new Font("Segoe UI", 12);
            btn.Click += (s, e) =>
            {
                txt.UseSystemPasswordChar = !txt.UseSystemPasswordChar;
                btn.Text = txt.UseSystemPasswordChar ? "👁" : "🙈";
            };
            return btn;
        }

        // ===== TOAST =====
        private void MostrarMensaje(string mensaje, Color color)
        {
            Form toast = new Form();
            toast.FormBorderStyle = FormBorderStyle.None;
            toast.StartPosition = FormStartPosition.Manual;
            toast.Size = new Size(320, 55);
            toast.BackColor = color;
            toast.Opacity = 0.95;
            toast.TopMost = true;
            toast.ShowInTaskbar = false;
            toast.Location = new Point(
                this.Left + this.Width - 330,
                this.Top + this.Height - 75);

            Label lbl = new Label();
            lbl.Text = mensaje;
            lbl.ForeColor = Color.White;
            lbl.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lbl.Dock = DockStyle.Fill;
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            toast.Controls.Add(lbl);

            toast.Show(this);

            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Interval = 2800;
            t.Tick += (s, e) => { t.Stop(); toast.Close(); };
            t.Start();
        }
    }
}