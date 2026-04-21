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
            this.Text = "Ajustes — Administrador";
            this.Size = new Size(700, 620);
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

            // ===== TABS =====
            TabControl tabs = new TabControl();
            tabs.Size = new Size(640, 530);
            tabs.Location = new Point(30, 35);
            tabs.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.Controls.Add(tabs);

            TabPage tabUsuarios = new TabPage("👥  Usuarios");
            tabUsuarios.BackColor = Color.White;
            tabs.TabPages.Add(tabUsuarios);

            TabPage tabPin = new TabPage("🔑  Cambiar PIN");
            tabPin.BackColor = Color.White;
            tabs.TabPages.Add(tabPin);

            ConstruirTabUsuarios(tabUsuarios);
            ConstruirTabPin(tabPin);
        }

        // =====================================================
        //  TAB 1 — GESTIÓN DE USUARIOS
        // =====================================================
        private void ConstruirTabUsuarios(TabPage tab)
        {
            // Lista de usuarios
            ListView lista = new ListView();
            lista.Size = new Size(590, 220);
            lista.Location = new Point(20, 15);
            lista.View = View.Details;
            lista.FullRowSelect = true;
            lista.GridLines = true;
            lista.Font = new Font("Segoe UI", 10);
            lista.Columns.Add("ID", 40);
            lista.Columns.Add("Nombre", 180);
            lista.Columns.Add("Rol", 100);
            lista.Columns.Add("Estado", 80);
            lista.Columns.Add("PIN", 100);
            tab.Controls.Add(lista);

            CargarUsuarios(lista);

            // Separador
            Panel sep = new Panel();
            sep.Size = new Size(590, 2);
            sep.Location = new Point(20, 248);
            sep.BackColor = Color.FromArgb(220, 220, 220);
            tab.Controls.Add(sep);

            Label lblNuevo = new Label();
            lblNuevo.Text = "Agregar nuevo usuario";
            lblNuevo.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblNuevo.ForeColor = Color.FromArgb(29, 53, 87);
            lblNuevo.Location = new Point(20, 260);
            lblNuevo.AutoSize = true;
            tab.Controls.Add(lblNuevo);

            // Nombre
            Label lblNombre = new Label();
            lblNombre.Text = "Nombre";
            lblNombre.Font = new Font("Segoe UI", 9);
            lblNombre.ForeColor = Color.Gray;
            lblNombre.Location = new Point(20, 290);
            lblNombre.AutoSize = true;
            tab.Controls.Add(lblNombre);

            TextBox txtNombre = new TextBox();
            txtNombre.Size = new Size(200, 30);
            txtNombre.Location = new Point(20, 308);
            txtNombre.Font = new Font("Segoe UI", 11);
            txtNombre.BackColor = Color.FromArgb(245, 245, 245);
            txtNombre.BorderStyle = BorderStyle.FixedSingle;
            tab.Controls.Add(txtNombre);

            // Rol
            Label lblRol = new Label();
            lblRol.Text = "Rol";
            lblRol.Font = new Font("Segoe UI", 9);
            lblRol.ForeColor = Color.Gray;
            lblRol.Location = new Point(235, 290);
            lblRol.AutoSize = true;
            tab.Controls.Add(lblRol);

            ComboBox cmbRol = new ComboBox();
            cmbRol.Size = new Size(130, 30);
            cmbRol.Location = new Point(235, 308);
            cmbRol.Font = new Font("Segoe UI", 11);
            cmbRol.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRol.Items.AddRange(new string[] { "Mesero", "Admin" });
            cmbRol.SelectedIndex = 0;
            tab.Controls.Add(cmbRol);

            // PIN nuevo usuario
            Label lblPinNew = new Label();
            lblPinNew.Text = "PIN (4 dígitos)";
            lblPinNew.Font = new Font("Segoe UI", 9);
            lblPinNew.ForeColor = Color.Gray;
            lblPinNew.Location = new Point(380, 290);
            lblPinNew.AutoSize = true;
            tab.Controls.Add(lblPinNew);

            TextBox txtPinNew = new TextBox();
            txtPinNew.Size = new Size(120, 30);
            txtPinNew.Location = new Point(380, 308);
            txtPinNew.Font = new Font("Segoe UI", 11);
            txtPinNew.MaxLength = 4;
            txtPinNew.UseSystemPasswordChar = true;
            txtPinNew.BackColor = Color.FromArgb(245, 245, 245);
            txtPinNew.BorderStyle = BorderStyle.FixedSingle;
            tab.Controls.Add(txtPinNew);

            // Botón agregar
            Button btnAgregar = new Button();
            btnAgregar.Text = "➕  Agregar";
            btnAgregar.Size = new Size(130, 38);
            btnAgregar.Location = new Point(20, 360);
            btnAgregar.BackColor = Color.FromArgb(29, 53, 87);
            btnAgregar.ForeColor = Color.White;
            btnAgregar.FlatStyle = FlatStyle.Flat;
            btnAgregar.FlatAppearance.BorderSize = 0;
            btnAgregar.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnAgregar.Cursor = Cursors.Hand;
            tab.Controls.Add(btnAgregar);

            // Botón eliminar
            Button btnEliminar = new Button();
            btnEliminar.Text = "🗑  Eliminar";
            btnEliminar.Size = new Size(130, 38);
            btnEliminar.Location = new Point(165, 360);
            btnEliminar.BackColor = Color.FromArgb(239, 83, 80);
            btnEliminar.ForeColor = Color.White;
            btnEliminar.FlatStyle = FlatStyle.Flat;
            btnEliminar.FlatAppearance.BorderSize = 0;
            btnEliminar.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnEliminar.Cursor = Cursors.Hand;
            tab.Controls.Add(btnEliminar);

            // ===== LÓGICA AGREGAR =====
            btnAgregar.Click += (s, e) =>
            {
                string nombre = txtNombre.Text.Trim();
                string rol = cmbRol.SelectedItem.ToString();
                string pin = txtPinNew.Text.Trim();

                if (nombre == "" || pin == "")
                {
                    MostrarMensaje("Llena todos los campos ❌", Color.FromArgb(239, 83, 80)); return;
                }
                if (pin.Length != 4)
                {
                    MostrarMensaje("El PIN debe tener exactamente 4 dígitos ❌", Color.FromArgb(239, 83, 80)); return;
                }

                try
                {
                    string cadena = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                    using (var con = new SQLiteConnection(cadena))
                    {
                        con.Open();
                        string q = "INSERT INTO usuario (nombre, rol, pin_acceso) VALUES (@n, @r, @p)";
                        using (var cmd = new SQLiteCommand(q, con))
                        {
                            cmd.Parameters.AddWithValue("@n", nombre);
                            cmd.Parameters.AddWithValue("@r", rol);
                            cmd.Parameters.AddWithValue("@p", pin);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    MostrarMensaje("Usuario agregado ✅", Color.FromArgb(44, 160, 44));
                    txtNombre.Clear();
                    txtPinNew.Clear();
                    CargarUsuarios(lista);
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error: " + ex.Message, Color.FromArgb(239, 83, 80));
                }
            };

            // ===== LÓGICA ELIMINAR =====
            btnEliminar.Click += (s, e) =>
            {
                if (lista.SelectedItems.Count == 0)
                {
                    MostrarMensaje("Selecciona un usuario de la lista ❌", Color.FromArgb(239, 83, 80)); return;
                }

                string nombreSel = lista.SelectedItems[0].SubItems[1].Text;
                string rolSel = lista.SelectedItems[0].SubItems[2].Text;

                if (rolSel == "Admin" && nombreSel == Sesion.Nombre)
                {
                    MostrarMensaje("No puedes eliminar tu propia cuenta ❌", Color.FromArgb(239, 83, 80)); return;
                }

                int idSel = int.Parse(lista.SelectedItems[0].SubItems[0].Text);

                var confirm = MessageBox.Show($"¿Eliminar al usuario '{nombreSel}'?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirm != DialogResult.Yes) return;

                try
                {
                    string cadena = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                    using (var con = new SQLiteConnection(cadena))
                    {
                        con.Open();
                        string q = "DELETE FROM usuario WHERE id_usuario = @id";
                        using (var cmd = new SQLiteCommand(q, con))
                        {
                            cmd.Parameters.AddWithValue("@id", idSel);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    MostrarMensaje("Usuario eliminado ✅", Color.FromArgb(44, 160, 44));
                    CargarUsuarios(lista);
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error: " + ex.Message, Color.FromArgb(239, 83, 80));
                }
            };
        }

        private void CargarUsuarios(ListView lista)
        {
            lista.Items.Clear();
            try
            {
                string cadena = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                using (var con = new SQLiteConnection(cadena))
                {
                    con.Open();
                    string q = "SELECT id_usuario, nombre, rol, estado, pin_acceso FROM usuario ORDER BY id_usuario";
                    using (var cmd = new SQLiteCommand(q, con))
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var item = new ListViewItem(r["id_usuario"].ToString());
                            item.SubItems.Add(r["nombre"].ToString());
                            item.SubItems.Add(r["rol"].ToString());
                            item.SubItems.Add(r["estado"].ToString() == "1" ? "Activo" : "Inactivo");
                            item.SubItems.Add(r["pin_acceso"].ToString());
                            lista.Items.Add(item);
                        }
                    }
                }
            }
            catch { }
        }

        // =====================================================
        //  TAB 2 — CAMBIAR PIN (de cualquier usuario)
        // =====================================================
        private void ConstruirTabPin(TabPage tab)
        {
            Label lblSel = new Label();
            lblSel.Text = "Selecciona el usuario";
            lblSel.Font = new Font("Segoe UI", 10);
            lblSel.ForeColor = Color.Gray;
            lblSel.Location = new Point(30, 25);
            lblSel.AutoSize = true;
            tab.Controls.Add(lblSel);

            ComboBox cmbUsuario = new ComboBox();
            cmbUsuario.Size = new Size(300, 32);
            cmbUsuario.Location = new Point(30, 45);
            cmbUsuario.Font = new Font("Segoe UI", 11);
            cmbUsuario.DropDownStyle = ComboBoxStyle.DropDownList;
            tab.Controls.Add(cmbUsuario);

            Button btnRefresh = new Button();
            btnRefresh.Text = "🔄";
            btnRefresh.Size = new Size(38, 32);
            btnRefresh.Location = new Point(340, 45);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.BackColor = Color.FromArgb(230, 230, 230);
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.Font = new Font("Segoe UI", 13);
            tab.Controls.Add(btnRefresh);

            CargarComboUsuarios(cmbUsuario);
            btnRefresh.Click += (s, e) => CargarComboUsuarios(cmbUsuario);

            // PIN nuevo
            Label lblNuevo = new Label();
            lblNuevo.Text = "Nuevo PIN";
            lblNuevo.Font = new Font("Segoe UI", 10);
            lblNuevo.ForeColor = Color.Gray;
            lblNuevo.Location = new Point(30, 100);
            lblNuevo.AutoSize = true;
            tab.Controls.Add(lblNuevo);

            TextBox txtNuevo = new TextBox();
            txtNuevo.Size = new Size(260, 32);
            txtNuevo.Location = new Point(30, 120);
            txtNuevo.Font = new Font("Segoe UI", 13);
            txtNuevo.UseSystemPasswordChar = true;
            txtNuevo.MaxLength = 4;
            txtNuevo.BackColor = Color.FromArgb(245, 245, 245);
            txtNuevo.BorderStyle = BorderStyle.FixedSingle;
            tab.Controls.Add(txtNuevo);

            Button btnVerNuevo = CrearBotonOjo(txtNuevo);
            btnVerNuevo.Location = new Point(295, 120);
            tab.Controls.Add(btnVerNuevo);

            // Confirmar PIN
            Label lblConfirmar = new Label();
            lblConfirmar.Text = "Confirmar nuevo PIN";
            lblConfirmar.Font = new Font("Segoe UI", 10);
            lblConfirmar.ForeColor = Color.Gray;
            lblConfirmar.Location = new Point(30, 170);
            lblConfirmar.AutoSize = true;
            tab.Controls.Add(lblConfirmar);

            TextBox txtConfirmar = new TextBox();
            txtConfirmar.Size = new Size(260, 32);
            txtConfirmar.Location = new Point(30, 190);
            txtConfirmar.Font = new Font("Segoe UI", 13);
            txtConfirmar.UseSystemPasswordChar = true;
            txtConfirmar.MaxLength = 4;
            txtConfirmar.BackColor = Color.FromArgb(245, 245, 245);
            txtConfirmar.BorderStyle = BorderStyle.FixedSingle;
            tab.Controls.Add(txtConfirmar);

            Button btnVerConfirmar = CrearBotonOjo(txtConfirmar);
            btnVerConfirmar.Location = new Point(295, 190);
            tab.Controls.Add(btnVerConfirmar);

            // Indicador
            Label lblSeguridad = new Label();
            lblSeguridad.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lblSeguridad.Location = new Point(30, 228);
            lblSeguridad.Size = new Size(400, 22);
            lblSeguridad.ForeColor = Color.Gray;
            tab.Controls.Add(lblSeguridad);

            txtNuevo.TextChanged += (s, e) =>
            {
                string v = txtNuevo.Text;
                if (v.Length == 0) { lblSeguridad.Text = ""; return; }
                if (v.Length < 4) { lblSeguridad.Text = "PIN incompleto ❌"; lblSeguridad.ForeColor = Color.Red; }
                else { lblSeguridad.Text = "PIN listo ✅"; lblSeguridad.ForeColor = Color.Green; }
            };

            // Botones
            Button btnActualizar = new Button();
            btnActualizar.Text = "Actualizar PIN";
            btnActualizar.Size = new Size(160, 42);
            btnActualizar.Location = new Point(30, 265);
            btnActualizar.BackColor = Color.FromArgb(29, 53, 87);
            btnActualizar.ForeColor = Color.White;
            btnActualizar.FlatStyle = FlatStyle.Flat;
            btnActualizar.FlatAppearance.BorderSize = 0;
            btnActualizar.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnActualizar.Cursor = Cursors.Hand;
            tab.Controls.Add(btnActualizar);

            Button btnLimpiar = new Button();
            btnLimpiar.Text = "Cancelar";
            btnLimpiar.Size = new Size(120, 42);
            btnLimpiar.Location = new Point(205, 265);
            btnLimpiar.BackColor = Color.FromArgb(200, 200, 200);
            btnLimpiar.ForeColor = Color.FromArgb(50, 50, 50);
            btnLimpiar.FlatStyle = FlatStyle.Flat;
            btnLimpiar.FlatAppearance.BorderSize = 0;
            btnLimpiar.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnLimpiar.Cursor = Cursors.Hand;
            btnLimpiar.Click += (s, e) =>
            {
                txtNuevo.Clear();
                txtConfirmar.Clear();
                lblSeguridad.Text = "";
            };
            tab.Controls.Add(btnLimpiar);

            // ===== LÓGICA =====
            btnActualizar.Click += (s, e) =>
            {
                if (cmbUsuario.SelectedItem == null)
                {
                    MostrarMensaje("Selecciona un usuario ❌", Color.FromArgb(239, 83, 80)); return;
                }

                string nuevo = txtNuevo.Text.Trim();
                string confirmar = txtConfirmar.Text.Trim();

                if (nuevo == "" || confirmar == "")
                {
                    MostrarMensaje("Llena todos los campos ❌", Color.FromArgb(239, 83, 80)); return;
                }
                if (nuevo != confirmar)
                {
                    MostrarMensaje("Los PINs no coinciden ❌", Color.FromArgb(239, 83, 80)); return;
                }
                if (nuevo.Length != 4)
                {
                    MostrarMensaje("El PIN debe tener exactamente 4 dígitos ❌", Color.FromArgb(239, 83, 80)); return;
                }

                int idUsuario = (int)((ComboBox)tab.Controls[1]).SelectedValue;

                var confirm = MessageBox.Show("¿Cambiar el PIN de este usuario?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;

                try
                {
                    string cadena = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                    using (var con = new SQLiteConnection(cadena))
                    {
                        con.Open();
                        string q = "UPDATE usuario SET pin_acceso = @pin WHERE id_usuario = @id";
                        using (var cmd = new SQLiteCommand(q, con))
                        {
                            cmd.Parameters.AddWithValue("@pin", nuevo);
                            cmd.Parameters.AddWithValue("@id", idUsuario);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    MostrarMensaje("PIN actualizado correctamente ✅", Color.FromArgb(44, 160, 44));
                    txtNuevo.Clear();
                    txtConfirmar.Clear();
                    lblSeguridad.Text = "";
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error: " + ex.Message, Color.FromArgb(239, 83, 80));
                }
            };
        }

        private void CargarComboUsuarios(ComboBox cmb)
        {
            try
            {
                string cadena = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                using (var con = new SQLiteConnection(cadena))
                {
                    con.Open();
                    string q = "SELECT id_usuario, nombre || ' (' || rol || ')' AS display FROM usuario WHERE estado = 1 ORDER BY nombre";
                    using (var cmd = new SQLiteCommand(q, con))
                    using (var r = cmd.ExecuteReader())
                    {
                        var tabla = new System.Data.DataTable();
                        tabla.Columns.Add("id_usuario", typeof(int));
                        tabla.Columns.Add("display", typeof(string));
                        while (r.Read())
                            tabla.Rows.Add(r["id_usuario"], r["display"]);

                        cmb.DataSource = tabla;
                        cmb.DisplayMember = "display";
                        cmb.ValueMember = "id_usuario";
                    }
                }
            }
            catch { }
        }

        private Button CrearBotonOjo(TextBox txt)
        {
            Button btn = new Button();
            btn.Text = "👁";
            btn.Size = new Size(38, 32);
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
            toast.Location = new Point(
                this.Left + this.Width - 350,
                this.Top + this.Height - 75);

            Label lbl = new Label();
            lbl.Text = mensaje;
            lbl.ForeColor = Color.White;
            lbl.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lbl.Dock = DockStyle.Fill;
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            toast.Controls.Add(lbl);
            toast.Show(this);

            var t = new System.Windows.Forms.Timer();
            t.Interval = 2800;
            t.Tick += (s, e2) => { t.Stop(); toast.Close(); };
            t.Start();
        }
    }
}