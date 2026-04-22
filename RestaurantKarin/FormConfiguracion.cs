using System;
using System.Configuration;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public partial class FormConfiguracion : Form
    {
        // Colores del sistema
        private readonly Color AzulOscuro = Color.FromArgb(13, 41, 78);
        private readonly Color AzulMedio = Color.FromArgb(29, 53, 87);
        private readonly Color AzulClaro = Color.FromArgb(64, 196, 204);
        private readonly Color Blanco = Color.White;
        private readonly Color GrisFondo = Color.FromArgb(245, 247, 250);
        private readonly Color GrisBorde = Color.FromArgb(220, 225, 232);
        private readonly Color Rojo = Color.FromArgb(239, 83, 80);
        private readonly Color Verde = Color.FromArgb(34, 197, 94);

        // Panels de contenido
        private Panel panelContenido;
        private Button btnNavActivo;

        public FormConfiguracion()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Ajustes — Administrador";
            this.Size = new Size(860, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = GrisFondo;

            // ===== SIDEBAR IZQUIERDO =====
            Panel sidebar = new Panel();
            sidebar.Size = new Size(200, 600);
            sidebar.Location = new Point(0, 0);
            sidebar.BackColor = AzulOscuro;

            // Degradado en el sidebar
            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, sidebar, new object[] { true });

            sidebar.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    sidebar.ClientRectangle,
                    Color.FromArgb(64, 196, 204),
                    Color.FromArgb(13, 41, 78),
                    90F))
                {
                    e.Graphics.FillRectangle(brush, sidebar.ClientRectangle);
                }
            };

            this.Controls.Add(sidebar);

            // Logo/ícono arriba del sidebar
            PictureBox pic = new PictureBox();
            pic.Size = new Size(64, 64);
            pic.Location = new Point(68, 28);
            pic.SizeMode = PictureBoxSizeMode.Zoom;
            pic.BackColor = Color.Transparent;
            try { pic.Image = Image.FromFile(Path.Combine(Application.StartupPath, "Imgs", "icono.ico")); } catch { }
            sidebar.Controls.Add(pic);

            Label lblSideTitle = new Label();
            lblSideTitle.Text = "Ajustes";
            lblSideTitle.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            lblSideTitle.ForeColor = Color.White;
            lblSideTitle.Size = new Size(200, 28);
            lblSideTitle.Location = new Point(0, 100);
            lblSideTitle.TextAlign = ContentAlignment.MiddleCenter;
            sidebar.Controls.Add(lblSideTitle);

            Label lblSideAdmin = new Label();
            lblSideAdmin.Text = Sesion.Nombre;
            lblSideAdmin.Font = new Font("Segoe UI", 8, FontStyle.Italic);
            lblSideAdmin.ForeColor = Color.FromArgb(180, 220, 240);
            lblSideAdmin.Size = new Size(200, 20);
            lblSideAdmin.Location = new Point(0, 126);
            lblSideAdmin.TextAlign = ContentAlignment.MiddleCenter;
            sidebar.Controls.Add(lblSideAdmin);

            // Separador
            Panel sepSide = new Panel();
            sepSide.Size = new Size(160, 1);
            sepSide.Location = new Point(20, 155);
            sepSide.BackColor = Color.FromArgb(80, 255, 255, 255);
            sidebar.Controls.Add(sepSide);

            // Botones de navegación
            Button btnNavUsuarios = CrearBotonNav("👥   Usuarios", 165);
            Button btnNavPin = CrearBotonNav("🔑   Cambiar PIN", 220);

            sidebar.Controls.Add(btnNavUsuarios);
            sidebar.Controls.Add(btnNavPin);

            // Botón cerrar abajo
            Button btnCerrar = new Button();
            btnCerrar.Text = "✕   Cerrar";
            btnCerrar.Size = new Size(160, 40);
            btnCerrar.Location = new Point(20, 510);
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, Rojo);
            btnCerrar.BackColor = Color.Transparent;
            btnCerrar.ForeColor = Color.FromArgb(200, 200, 200);
            btnCerrar.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            btnCerrar.TextAlign = ContentAlignment.MiddleLeft;
            btnCerrar.Cursor = Cursors.Hand;
            btnCerrar.Click += (s, e) => this.Close();
            sidebar.Controls.Add(btnCerrar);

            // ===== ÁREA DE CONTENIDO =====
            panelContenido = new Panel();
            panelContenido.Size = new Size(658, 600);
            panelContenido.Location = new Point(200, 0);
            panelContenido.BackColor = GrisFondo;
            this.Controls.Add(panelContenido);

            // Navegación inicial
            btnNavUsuarios.Click += (s, e) => {
                ActivarNav(btnNavUsuarios);
                MostrarSeccion(ConstruirSeccionUsuarios());
            };
            btnNavPin.Click += (s, e) => {
                ActivarNav(btnNavPin);
                MostrarSeccion(ConstruirSeccionPin());
            };

            // Cargar sección inicial
            ActivarNav(btnNavUsuarios);
            MostrarSeccion(ConstruirSeccionUsuarios());
        }

        private Button CrearBotonNav(string texto, int posY)
        {
            Button btn = new Button();
            btn.Text = texto;
            btn.Size = new Size(200, 45);
            btn.Location = new Point(0, posY);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 255, 255, 255);
            btn.BackColor = Color.Transparent;
            btn.ForeColor = Color.FromArgb(200, 230, 245);
            btn.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(18, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private void ActivarNav(Button btn)
        {
            if (btnNavActivo != null)
            {
                btnNavActivo.BackColor = Color.Transparent;
                btnNavActivo.ForeColor = Color.FromArgb(200, 230, 245);
                btnNavActivo.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            }
            btn.BackColor = Color.FromArgb(50, 255, 255, 255);
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnNavActivo = btn;
        }

        private void MostrarSeccion(Panel seccion)
        {
            panelContenido.Controls.Clear();
            seccion.Dock = DockStyle.Fill;
            panelContenido.Controls.Add(seccion);
        }

        // =====================================================
        //  SECCIÓN 1 — GESTIÓN DE USUARIOS
        // =====================================================
        private Panel ConstruirSeccionUsuarios()
        {
            Panel panel = new Panel();
            panel.BackColor = GrisFondo;
            panel.Padding = new Padding(30, 25, 30, 25);

            // Título de sección
            Label lblTitulo = new Label();
            lblTitulo.Text = "Gestión de Usuarios";
            lblTitulo.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitulo.ForeColor = AzulOscuro;
            lblTitulo.Location = new Point(30, 20);
            lblTitulo.AutoSize = true;
            panel.Controls.Add(lblTitulo);

            Label lblSub = new Label();
            lblSub.Text = "Administra los accesos al sistema";
            lblSub.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            lblSub.ForeColor = Color.Gray;
            lblSub.Location = new Point(30, 48);
            lblSub.AutoSize = true;
            panel.Controls.Add(lblSub);

            // ===== TARJETA LISTA =====
            Panel cardLista = CrearTarjeta(30, 78, 595, 185);
            panel.Controls.Add(cardLista);

            ListView lista = new ListView();
            lista.Size = new Size(565, 155);
            lista.Location = new Point(15, 15);
            lista.View = View.Details;
            lista.FullRowSelect = true;
            lista.GridLines = false;
            lista.BorderStyle = BorderStyle.None;
            lista.Font = new Font("Segoe UI", 10);
            lista.BackColor = Blanco;
            lista.ForeColor = AzulOscuro;
            lista.HeaderStyle = ColumnHeaderStyle.Nonclickable;

            lista.Columns.Add("ID", 45);
            lista.Columns.Add("Nombre", 175);
            lista.Columns.Add("Rol", 110);
            lista.Columns.Add("Estado", 90);
            lista.Columns.Add("PIN", 100);

            // Colorear filas alternas
            lista.OwnerDraw = true;
            lista.DrawColumnHeader += (s, e) =>
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(240, 245, 250)), e.Bounds);
                e.Graphics.DrawString(e.Header.Text, new Font("Segoe UI", 9, FontStyle.Bold),
                    new SolidBrush(AzulMedio), e.Bounds.X + 4, e.Bounds.Y + 5);
            };
            lista.DrawItem += (s, e) => e.DrawDefault = true;
            lista.DrawSubItem += (s, e) =>
            {
                Color bg = e.ItemIndex % 2 == 0 ? Blanco : Color.FromArgb(248, 250, 253);
                if (e.Item.Selected) bg = Color.FromArgb(220, 235, 255);
                e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
                e.Graphics.DrawString(e.SubItem.Text, new Font("Segoe UI", 10),
                    new SolidBrush(AzulOscuro), e.Bounds.X + 4, e.Bounds.Y + 4);
            };

            cardLista.Controls.Add(lista);
            CargarUsuarios(lista);

            // ===== TARJETA AGREGAR =====
            Panel cardAgregar = CrearTarjeta(30, 278, 595, 185);
            panel.Controls.Add(cardAgregar);

            Label lblAgrTitle = new Label();
            lblAgrTitle.Text = "Agregar nuevo usuario";
            lblAgrTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblAgrTitle.ForeColor = AzulMedio;
            lblAgrTitle.Location = new Point(15, 12);
            lblAgrTitle.AutoSize = true;
            cardAgregar.Controls.Add(lblAgrTitle);

            // Nombre
            Label lblNombre = MakeLabel("Nombre", 15, 45);
            cardAgregar.Controls.Add(lblNombre);
            TextBox txtNombre = MakeTextBox(15, 65, 210, false);
            cardAgregar.Controls.Add(txtNombre);

            // Rol
            Label lblRol = MakeLabel("Rol", 240, 45);
            cardAgregar.Controls.Add(lblRol);
            ComboBox cmbRol = new ComboBox();
            cmbRol.Size = new Size(130, 34);
            cmbRol.Location = new Point(240, 65);
            cmbRol.Font = new Font("Segoe UI", 11);
            cmbRol.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRol.BackColor = GrisFondo;
            cmbRol.FlatStyle = FlatStyle.Flat;
            cmbRol.Items.AddRange(new string[] { "Mesero", "Admin" });
            cmbRol.SelectedIndex = 0;
            cardAgregar.Controls.Add(cmbRol);

            // PIN
            Label lblPin = MakeLabel("PIN (4 dígitos)", 385, 45);
            cardAgregar.Controls.Add(lblPin);
            TextBox txtPin = MakeTextBox(385, 65, 110, true);
            txtPin.MaxLength = 4;
            cardAgregar.Controls.Add(txtPin);

            // Botones
            Button btnAgregar = CrearBoton("➕  Agregar", AzulMedio, 15, 125, 140, 40);
            cardAgregar.Controls.Add(btnAgregar);

            Button btnEliminar = CrearBoton("🗑  Eliminar", Rojo, 170, 125, 140, 40);
            cardAgregar.Controls.Add(btnEliminar);

            // ===== LÓGICA AGREGAR =====
            btnAgregar.Click += (s, e) =>
            {
                string nombre = txtNombre.Text.Trim();
                string rol = cmbRol.SelectedItem.ToString();
                string pin = txtPin.Text.Trim();

                if (nombre == "" || pin == "")
                { MostrarMensaje("Llena todos los campos ❌", Rojo); return; }
                if (pin.Length != 4)
                { MostrarMensaje("El PIN debe tener exactamente 4 dígitos ❌", Rojo); return; }

                try
                {
                    string cadena = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                    using (var con = new SQLiteConnection(cadena))
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand("INSERT INTO usuario (nombre, rol, pin_acceso) VALUES (@n, @r, @p)", con))
                        {
                            cmd.Parameters.AddWithValue("@n", nombre);
                            cmd.Parameters.AddWithValue("@r", rol);
                            cmd.Parameters.AddWithValue("@p", pin);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    MostrarMensaje("Usuario agregado correctamente ✅", Verde);
                    txtNombre.Clear(); txtPin.Clear();
                    CargarUsuarios(lista);
                }
                catch (Exception ex) { MostrarMensaje("Error: " + ex.Message, Rojo); }
            };

            // ===== LÓGICA ELIMINAR =====
            btnEliminar.Click += (s, e) =>
            {
                if (lista.SelectedItems.Count == 0)
                { MostrarMensaje("Selecciona un usuario de la lista ❌", Rojo); return; }

                string nombreSel = lista.SelectedItems[0].SubItems[1].Text;
                string rolSel = lista.SelectedItems[0].SubItems[2].Text;
                int idSel = int.Parse(lista.SelectedItems[0].SubItems[0].Text);

                if (rolSel == "Admin" && nombreSel == Sesion.Nombre)
                { MostrarMensaje("No puedes eliminar tu propia cuenta ❌", Rojo); return; }

                if (MessageBox.Show($"¿Eliminar al usuario '{nombreSel}'?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

                try
                {
                    string cadena = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                    using (var con = new SQLiteConnection(cadena))
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand("DELETE FROM usuario WHERE id_usuario = @id", con))
                        {
                            cmd.Parameters.AddWithValue("@id", idSel);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    MostrarMensaje("Usuario eliminado ✅", Verde);
                    CargarUsuarios(lista);
                }
                catch (Exception ex) { MostrarMensaje("Error: " + ex.Message, Rojo); }
            };

            return panel;
        }

        // =====================================================
        //  SECCIÓN 2 — CAMBIAR PIN
        // =====================================================
        private Panel ConstruirSeccionPin()
        {
            Panel panel = new Panel();
            panel.BackColor = GrisFondo;

            Label lblTitulo = new Label();
            lblTitulo.Text = "Cambiar PIN";
            lblTitulo.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitulo.ForeColor = AzulOscuro;
            lblTitulo.Location = new Point(30, 20);
            lblTitulo.AutoSize = true;
            panel.Controls.Add(lblTitulo);

            Label lblSub = new Label();
            lblSub.Text = "Modifica el PIN de cualquier usuario";
            lblSub.Font = new Font("Segoe UI", 9);
            lblSub.ForeColor = Color.Gray;
            lblSub.Location = new Point(30, 48);
            lblSub.AutoSize = true;
            panel.Controls.Add(lblSub);

            // Tarjeta
            Panel card = CrearTarjeta(30, 78, 595, 360);
            panel.Controls.Add(card);

            // Usuario
            Label lblSel = MakeLabel("Selecciona el usuario", 20, 20);
            card.Controls.Add(lblSel);

            ComboBox cmbUsuario = new ComboBox();
            cmbUsuario.Size = new Size(440, 34);
            cmbUsuario.Location = new Point(20, 42);
            cmbUsuario.Font = new Font("Segoe UI", 11);
            cmbUsuario.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbUsuario.BackColor = GrisFondo;
            card.Controls.Add(cmbUsuario);

            Button btnRefresh = new Button();
            btnRefresh.Text = "🔄";
            btnRefresh.Size = new Size(40, 34);
            btnRefresh.Location = new Point(468, 42);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 1;
            btnRefresh.FlatAppearance.BorderColor = GrisBorde;
            btnRefresh.BackColor = GrisFondo;
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.Font = new Font("Segoe UI", 13);
            btnRefresh.Click += (s, e) => CargarComboUsuarios(cmbUsuario);
            card.Controls.Add(btnRefresh);

            CargarComboUsuarios(cmbUsuario);

            // Separador interno
            Panel sepCard = new Panel();
            sepCard.Size = new Size(555, 1);
            sepCard.Location = new Point(20, 92);
            sepCard.BackColor = GrisBorde;
            card.Controls.Add(sepCard);

            // Nuevo PIN
            Label lblNuevo = MakeLabel("Nuevo PIN", 20, 108);
            card.Controls.Add(lblNuevo);
            TextBox txtNuevo = MakeTextBox(20, 130, 260, true);
            txtNuevo.MaxLength = 4;
            card.Controls.Add(txtNuevo);
            Button btnOjo1 = CrearBotonOjo(txtNuevo);
            btnOjo1.Location = new Point(285, 130);
            card.Controls.Add(btnOjo1);

            // Indicador
            Label lblSeguridad = new Label();
            lblSeguridad.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lblSeguridad.Location = new Point(20, 172);
            lblSeguridad.Size = new Size(400, 20);
            lblSeguridad.ForeColor = Color.Gray;
            card.Controls.Add(lblSeguridad);

            txtNuevo.TextChanged += (s, e) =>
            {
                string v = txtNuevo.Text;
                if (v.Length == 0) { lblSeguridad.Text = ""; return; }
                if (v.Length < 4) { lblSeguridad.Text = "PIN incompleto ❌"; lblSeguridad.ForeColor = Rojo; }
                else { lblSeguridad.Text = "PIN listo ✅"; lblSeguridad.ForeColor = Verde; }
            };

            // Confirmar PIN
            Label lblConfirmar = MakeLabel("Confirmar nuevo PIN", 20, 200);
            card.Controls.Add(lblConfirmar);
            TextBox txtConfirmar = MakeTextBox(20, 222, 260, true);
            txtConfirmar.MaxLength = 4;
            card.Controls.Add(txtConfirmar);
            Button btnOjo2 = CrearBotonOjo(txtConfirmar);
            btnOjo2.Location = new Point(285, 222);
            card.Controls.Add(btnOjo2);

            // Botones
            Button btnActualizar = CrearBoton("🔑  Actualizar PIN", AzulMedio, 20, 290, 180, 44);
            card.Controls.Add(btnActualizar);

            Button btnLimpiar = CrearBoton("Cancelar", Color.FromArgb(180, 180, 180), 215, 290, 120, 44);
            btnLimpiar.ForeColor = Color.FromArgb(60, 60, 60);
            btnLimpiar.Click += (s, e) => { txtNuevo.Clear(); txtConfirmar.Clear(); lblSeguridad.Text = ""; };
            card.Controls.Add(btnLimpiar);

            // ===== LÓGICA =====
            btnActualizar.Click += (s, e) =>
            {
                if (cmbUsuario.SelectedItem == null)
                { MostrarMensaje("Selecciona un usuario ❌", Rojo); return; }

                string nuevo = txtNuevo.Text.Trim();
                string confirmar = txtConfirmar.Text.Trim();

                if (nuevo == "" || confirmar == "")
                { MostrarMensaje("Llena todos los campos ❌", Rojo); return; }
                if (nuevo != confirmar)
                { MostrarMensaje("Los PINs no coinciden ❌", Rojo); return; }
                if (nuevo.Length != 4)
                { MostrarMensaje("El PIN debe tener exactamente 4 dígitos ❌", Rojo); return; }

                int idUsuario = (int)cmbUsuario.SelectedValue;

                if (MessageBox.Show("¿Cambiar el PIN de este usuario?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

                try
                {
                    string cadena = ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString;
                    using (var con = new SQLiteConnection(cadena))
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand("UPDATE usuario SET pin_acceso = @pin WHERE id_usuario = @id", con))
                        {
                            cmd.Parameters.AddWithValue("@pin", nuevo);
                            cmd.Parameters.AddWithValue("@id", idUsuario);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    MostrarMensaje("PIN actualizado correctamente ✅", Verde);
                    txtNuevo.Clear(); txtConfirmar.Clear(); lblSeguridad.Text = "";
                }
                catch (Exception ex) { MostrarMensaje("Error: " + ex.Message, Rojo); }
            };

            return panel;
        }

        // =====================================================
        //  HELPERS DE UI
        // =====================================================
        private Panel CrearTarjeta(int x, int y, int w, int h)
        {
            Panel card = new Panel();
            card.Size = new Size(w, h);
            card.Location = new Point(x, y);
            card.BackColor = Blanco;

            // Sombra simulada con Paint
            card.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle,
                    GrisBorde, 1, ButtonBorderStyle.Solid,
                    GrisBorde, 1, ButtonBorderStyle.Solid,
                    GrisBorde, 1, ButtonBorderStyle.Solid,
                    GrisBorde, 1, ButtonBorderStyle.Solid);
            };

            return card;
        }

        private Label MakeLabel(string text, int x, int y)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            lbl.ForeColor = Color.Gray;
            lbl.Location = new Point(x, y);
            lbl.AutoSize = true;
            return lbl;
        }

        private TextBox MakeTextBox(int x, int y, int w, bool password)
        {
            TextBox txt = new TextBox();
            txt.Size = new Size(w, 34);
            txt.Location = new Point(x, y);
            txt.Font = new Font("Segoe UI", 11);
            txt.BackColor = GrisFondo;
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.UseSystemPasswordChar = password;
            return txt;
        }

        private Button CrearBoton(string texto, Color fondo, int x, int y, int w, int h)
        {
            Button btn = new Button();
            btn.Text = texto;
            btn.Size = new Size(w, h);
            btn.Location = new Point(x, y);
            btn.BackColor = fondo;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private Button CrearBotonOjo(TextBox txt)
        {
            Button btn = new Button();
            btn.Text = "👁";
            btn.Size = new Size(38, 34);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = GrisBorde;
            btn.BackColor = GrisFondo;
            btn.Cursor = Cursors.Hand;
            btn.Font = new Font("Segoe UI", 12);
            btn.Click += (s, e) =>
            {
                txt.UseSystemPasswordChar = !txt.UseSystemPasswordChar;
                btn.Text = txt.UseSystemPasswordChar ? "👁" : "🙈";
            };
            return btn;
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
                    using (var cmd = new SQLiteCommand("SELECT id_usuario, nombre, rol, estado, pin_acceso FROM usuario ORDER BY id_usuario", con))
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var item = new ListViewItem(r["id_usuario"].ToString());
                            item.SubItems.Add(r["nombre"].ToString());
                            item.SubItems.Add(r["rol"].ToString());
                            item.SubItems.Add(r["estado"].ToString() == "1" ? "✅ Activo" : "❌ Inactivo");
                            item.SubItems.Add(r["pin_acceso"].ToString());
                            lista.Items.Add(item);
                        }
                    }
                }
            }
            catch { }
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
                this.Left + this.Width - 355,
                this.Top + this.Height - 80);

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
            t.Tick += (s, e) => { t.Stop(); toast.Close(); };
            t.Start();
        }
    }
}