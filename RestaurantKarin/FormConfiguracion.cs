using System;
using System.Collections.Generic;
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
        private readonly Color Azul1 = Color.FromArgb(13, 41, 78);
        private readonly Color Azul2 = Color.FromArgb(29, 53, 87);
        private readonly Color Aqua = Color.FromArgb(64, 196, 204);
        private readonly Color Fondo = Color.FromArgb(236, 241, 247);
        private readonly Color Blanco = Color.White;
        private readonly Color GrisBorde = Color.FromArgb(210, 218, 230);
        private readonly Color GrisTexto = Color.FromArgb(110, 125, 145);
        private readonly Color Rojo = Color.FromArgb(220, 70, 70);
        private readonly Color Verde = Color.FromArgb(39, 174, 96);
        private readonly Color AzulHov = Color.FromArgb(42, 72, 110);
        private readonly Color Morado = Color.FromArgb(108, 92, 172);
        private readonly Color MoradoHov = Color.FromArgb(80, 68, 140);

        private readonly string[] MODULOS = { "Pedidos", "Cuentas", "Inventario", "Recetas", "Reportes" };

        private Panel panelContenido;
        private Button btnNavActivo;

        public FormConfiguracion()
        {
            InitializeComponent();
            SetupUI();
        }

        // ══════════════════════════════════════════════════════
        //  SETUP PRINCIPAL
        // ══════════════════════════════════════════════════════
        private void SetupUI()
        {
            this.Text = "Ajustes";
            this.Size = new Size(1060, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Fondo;
            this.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, this.Width, this.Height, 14, 14));

            // ── SIDEBAR ──
            Panel sidebar = new Panel();
            sidebar.Size = new Size(220, 700);
            sidebar.Location = new Point(0, 0);
            SetDB(sidebar);
            sidebar.Paint += (s, e) =>
            {
                using (var br = new LinearGradientBrush(sidebar.ClientRectangle, Aqua, Azul1, 90F))
                    e.Graphics.FillRectangle(br, sidebar.ClientRectangle);
            };
            this.Controls.Add(sidebar);

            // Ícono
            PictureBox pic = new PictureBox();
            pic.Size = new Size(64, 64); pic.Location = new Point(78, 26);
            pic.SizeMode = PictureBoxSizeMode.Zoom; pic.BackColor = Color.Transparent;
            try { pic.Image = Image.FromFile(Path.Combine(Application.StartupPath, "Imgs", "icono.ico")); } catch { }
            sidebar.Controls.Add(pic);

            // Título
            Label lST = new Label();
            lST.Text = "AJUSTES"; lST.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            lST.ForeColor = Color.White; lST.Size = new Size(220, 26);
            lST.Location = new Point(0, 98); lST.TextAlign = ContentAlignment.MiddleCenter;
            sidebar.Controls.Add(lST);

            // Admin badge
            Panel badge = new Panel();
            badge.Size = new Size(180, 28); badge.Location = new Point(20, 128);
            badge.BackColor = Color.FromArgb(40, 255, 255, 255);
            badge.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundPath(badge.ClientRectangle, 14))
                using (var br = new SolidBrush(Color.FromArgb(40, 255, 255, 255)))
                    e.Graphics.FillPath(br, path);
            };
            sidebar.Controls.Add(badge);

            Label lAdm = new Label();
            lAdm.Text = "⭐ " + Sesion.Nombre; lAdm.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lAdm.ForeColor = Color.White; lAdm.Dock = DockStyle.Fill;
            lAdm.TextAlign = ContentAlignment.MiddleCenter; badge.Controls.Add(lAdm);

            SSep(sidebar, 170); SLabel(sidebar, "NAVEGACIÓN", 180);

            Button btnU = NavBtn("👥   Usuarios", 206, sidebar);
            Button btnP = NavBtn("🔑   Cambiar PIN", 254, sidebar);
            SSep(sidebar, 590);

            Button btnX = new Button();
            btnX.Text = "✕   Cerrar"; btnX.Size = new Size(180, 44); btnX.Location = new Point(20, 600);
            btnX.FlatStyle = FlatStyle.Flat; btnX.FlatAppearance.BorderSize = 0;
            btnX.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, Rojo);
            btnX.BackColor = Color.Transparent; btnX.ForeColor = Color.FromArgb(200, 210, 220);
            btnX.Font = new Font("Segoe UI", 10); btnX.TextAlign = ContentAlignment.MiddleLeft;
            btnX.Padding = new Padding(10, 0, 0, 0); btnX.Cursor = Cursors.Hand;
            btnX.Click += (s, e) => this.Close();
            sidebar.Controls.Add(btnX);

            // ── BARRA TÍTULO ──
            Panel tb = new Panel();
            tb.Size = new Size(840, 52); tb.Location = new Point(220, 0);
            tb.BackColor = Blanco; this.Controls.Add(tb); tb.BringToFront();

            Label lTitle = new Label();
            lTitle.Text = "Panel de Administración";
            lTitle.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            lTitle.ForeColor = Azul1; lTitle.Location = new Point(24, 0);
            lTitle.Size = new Size(600, 52); lTitle.TextAlign = ContentAlignment.MiddleLeft;
            tb.Controls.Add(lTitle);

            // Rol badge en título
            Label lRol = new Label();
            lRol.Text = "Admin"; lRol.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lRol.ForeColor = Blanco; lRol.BackColor = Azul2;
            lRol.Size = new Size(60, 26); lRol.Location = new Point(740, 13);
            lRol.TextAlign = ContentAlignment.MiddleCenter; tb.Controls.Add(lRol);

            Panel tl = new Panel();
            tl.Size = new Size(840, 2); tl.Location = new Point(220, 52);
            tl.BackColor = GrisBorde; this.Controls.Add(tl); tl.BringToFront();

            // Arrastre
            bool drag = false; Point dp = Point.Empty;
            tb.MouseDown += (s, e) => { drag = true; dp = e.Location; };
            tb.MouseMove += (s, e) => { if (drag) this.Location = new Point(this.Location.X + e.X - dp.X, this.Location.Y + e.Y - dp.Y); };
            tb.MouseUp += (s, e) => drag = false;

            // ── CONTENIDO ──
            panelContenido = new Panel();
            panelContenido.Size = new Size(840, 646); panelContenido.Location = new Point(220, 54);
            panelContenido.BackColor = Fondo;
            this.Controls.Add(panelContenido);

            btnU.Click += (s, e) => { ActivarNav(btnU); MostrarSeccion(SeccionUsuarios()); };
            btnP.Click += (s, e) => { ActivarNav(btnP); MostrarSeccion(SeccionPin()); };

            ActivarNav(btnU);
            MostrarSeccion(SeccionUsuarios());
        }

        // ══════════════════════════════════════════════════════
        //  SECCIÓN: USUARIOS
        // ══════════════════════════════════════════════════════
        private Panel SeccionUsuarios()
        {
            Panel panel = new Panel(); panel.BackColor = Fondo;

            Encabezado(panel, "👥  Gestión de Usuarios",
                "Agrega, consulta, edita permisos y elimina accesos al sistema");

            // ── Tarjeta tabla (Y=82) ──
            Panel ct = Tarjeta(22, 82, 796, 210); panel.Controls.Add(ct);
            CardHeader(ct, "📋  Usuarios registrados");

            ListView lista = new ListView();
            lista.Size = new Size(764, 158); lista.Location = new Point(16, 44);
            lista.View = View.Details; lista.FullRowSelect = true;
            lista.GridLines = false; lista.BorderStyle = BorderStyle.None;
            lista.Font = new Font("Segoe UI", 10); lista.BackColor = Blanco;
            lista.OwnerDraw = true;
            lista.Columns.Add("ID", 40);
            lista.Columns.Add("Nombre", 170);
            lista.Columns.Add("Rol", 100);
            lista.Columns.Add("Estado", 90);
            lista.Columns.Add("PIN", 75);
            lista.Columns.Add("Pantallas permitidas", 255);

            lista.DrawColumnHeader += (s, e) =>
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(238, 244, 251)), e.Bounds);
                e.Graphics.DrawLine(new Pen(GrisBorde), e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
                using (var sf = new StringFormat { LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString(e.Header.Text, new Font("Segoe UI", 9, FontStyle.Bold),
                        new SolidBrush(Azul2), new RectangleF(e.Bounds.X + 6, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height), sf);
            };
            lista.DrawItem += (s, e) => e.DrawDefault = true;
            lista.DrawSubItem += (s, e) =>
            {
                Color bg = e.ItemIndex % 2 == 0 ? Blanco : Color.FromArgb(247, 250, 254);
                if (e.Item.Selected) bg = Color.FromArgb(214, 230, 255);
                e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
                e.Graphics.DrawLine(new Pen(Color.FromArgb(235, 240, 248)),
                    e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
                using (var sf = new StringFormat { LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString(e.SubItem.Text, new Font("Segoe UI", 10), new SolidBrush(Azul1),
                        new RectangleF(e.Bounds.X + 6, e.Bounds.Y, e.Bounds.Width - 6, e.Bounds.Height), sf);
            };
            ct.Controls.Add(lista);
            CargarUsuarios(lista);

            // ── Tarjeta AGREGAR (Y=305) ──
            Panel ca = Tarjeta(22, 305, 796, 170); panel.Controls.Add(ca);
            CardHeader(ca, "➕  Agregar nuevo usuario");

            MkLabel("Nombre completo", ca, 16, 52);
            TextBox txtNombre = MkBox(ca, 16, 70, 230, false);

            MkLabel("Rol", ca, 260, 52);
            ComboBox cmbRol = new ComboBox();
            cmbRol.Size = new Size(140, 34); cmbRol.Location = new Point(260, 70);
            cmbRol.Font = new Font("Segoe UI", 11); cmbRol.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRol.BackColor = Fondo; cmbRol.FlatStyle = FlatStyle.Flat;
            cmbRol.Items.AddRange(new string[] { "Mesero", "Admin" }); cmbRol.SelectedIndex = 0;
            ca.Controls.Add(cmbRol);

            MkLabel("PIN (4 dígitos)", ca, 414, 52);
            TextBox txtPin = MkBox(ca, 414, 70, 120, true); txtPin.MaxLength = 4;

            MkLabel("Pantallas permitidas", ca, 16, 116);

            var checks = new Dictionary<string, CheckBox>();
            int cx = 16;
            foreach (string mod in MODULOS)
            {
                CheckBox cb = new CheckBox();
                cb.Text = mod; cb.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                cb.ForeColor = Azul1; cb.Size = new Size(120, 26);
                cb.Location = new Point(cx, 134); cb.Checked = true;
                cb.FlatStyle = FlatStyle.Flat;
                ca.Controls.Add(cb); checks[mod] = cb; cx += 126;
            }

            cmbRol.SelectedIndexChanged += (s, e) =>
            {
                bool isAdmin = cmbRol.SelectedItem.ToString() == "Admin";
                foreach (var cb in checks.Values) { cb.Checked = true; cb.Enabled = !isAdmin; }
            };

            Button btnAgregar = BtnAcc("➕  Agregar", Azul2, ca, 560, 116, 212, 44);
            HoverBtn(btnAgregar, AzulHov, Azul2);

            btnAgregar.Click += (s, e) =>
            {
                string nom = txtNombre.Text.Trim(), rol = cmbRol.SelectedItem.ToString(), pin = txtPin.Text.Trim();
                if (nom == "" || pin == "") { Toast("Llena nombre y PIN ❌", Rojo); return; }
                if (pin.Length != 4) { Toast("El PIN debe tener exactamente 4 dígitos ❌", Rojo); return; }
                string perms = rol == "Admin" ? string.Join(",", MODULOS) : ObtenerPermisos(checks);
                try
                {
                    using (var con = Conn())
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand("INSERT INTO usuario (nombre,rol,pin_acceso,permisos) VALUES (@n,@r,@p,@pe)", con))
                        {
                            cmd.Parameters.AddWithValue("@n", nom); cmd.Parameters.AddWithValue("@r", rol);
                            cmd.Parameters.AddWithValue("@p", pin); cmd.Parameters.AddWithValue("@pe", perms);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    Toast("Usuario agregado correctamente ✅", Verde);
                    txtNombre.Clear(); txtPin.Clear(); CargarUsuarios(lista);
                }
                catch (Exception ex) { Toast("Error: " + ex.Message, Rojo); }
            };

            // ── Tarjeta EDITAR PERMISOS (Y=488) ──
            Panel ce = Tarjeta(22, 488, 796, 148); panel.Controls.Add(ce);
            CardHeader(ce, "🛡  Editar permisos del usuario seleccionado");

            MkLabel("Pantallas a habilitar", ce, 16, 50);

            var checksEdit = new Dictionary<string, CheckBox>();
            int ex2 = 16;
            foreach (string mod in MODULOS)
            {
                CheckBox cb = new CheckBox();
                cb.Text = mod; cb.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                cb.ForeColor = Azul1; cb.Size = new Size(120, 26);
                cb.Location = new Point(ex2, 70); cb.FlatStyle = FlatStyle.Flat;
                ce.Controls.Add(cb); checksEdit[mod] = cb; ex2 += 126;
            }

            // Al seleccionar usuario de la tabla, cargar sus permisos
            lista.SelectedIndexChanged += (s, e) =>
            {
                if (lista.SelectedItems.Count == 0) return;
                string perms = lista.SelectedItems[0].SubItems[5].Text;
                string rolSel = lista.SelectedItems[0].SubItems[2].Text;
                foreach (var mod in MODULOS)
                {
                    checksEdit[mod].Checked = perms.Contains(mod);
                    checksEdit[mod].Enabled = rolSel != "Admin";
                }
            };

            Button btnGuardarPerm = BtnAcc("🛡  Guardar Permisos", Morado, ce, 560, 52, 212, 44);
            Button btnEliminar = BtnAcc("🗑  Eliminar Usuario", Rojo, ce, 560, 102, 212, 38);
            HoverBtn(btnGuardarPerm, MoradoHov, Morado);
            HoverBtn(btnEliminar, Color.FromArgb(180, 50, 50), Rojo);

            btnGuardarPerm.Click += (s, e) =>
            {
                if (lista.SelectedItems.Count == 0) { Toast("Selecciona un usuario de la lista ❌", Rojo); return; }
                string rolSel = lista.SelectedItems[0].SubItems[2].Text;
                string perms = rolSel == "Admin" ? string.Join(",", MODULOS) : ObtenerPermisos(checksEdit);
                int id = int.Parse(lista.SelectedItems[0].SubItems[0].Text);
                try
                {
                    using (var con = Conn())
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand("UPDATE usuario SET permisos=@pe WHERE id_usuario=@id", con))
                        { cmd.Parameters.AddWithValue("@pe", perms); cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
                    }
                    Toast("Permisos actualizados correctamente ✅", Verde); CargarUsuarios(lista);
                }
                catch (Exception ex) { Toast("Error: " + ex.Message, Rojo); }
            };

            btnEliminar.Click += (s, e) =>
            {
                if (lista.SelectedItems.Count == 0) { Toast("Selecciona un usuario de la lista ❌", Rojo); return; }
                string nomSel = lista.SelectedItems[0].SubItems[1].Text;
                string rolSel = lista.SelectedItems[0].SubItems[2].Text;
                int id = int.Parse(lista.SelectedItems[0].SubItems[0].Text);
                if (rolSel == "Admin" && nomSel == Sesion.Nombre) { Toast("No puedes eliminar tu propia cuenta ❌", Rojo); return; }
                if (MessageBox.Show($"¿Eliminar al usuario '{nomSel}'?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                try
                {
                    using (var con = Conn())
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand("DELETE FROM usuario WHERE id_usuario=@id", con))
                        { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
                    }
                    Toast("Usuario eliminado ✅", Verde); CargarUsuarios(lista);
                }
                catch (Exception ex) { Toast("Error: " + ex.Message, Rojo); }
            };

            return panel;
        }

        // ══════════════════════════════════════════════════════
        //  SECCIÓN: CAMBIAR PIN
        // ══════════════════════════════════════════════════════
        private Panel SeccionPin()
        {
            Panel panel = new Panel(); panel.BackColor = Fondo;
            Encabezado(panel, "🔑  Cambiar PIN de Usuario",
                "Modifica el PIN de acceso de cualquier usuario del sistema");

            Panel card = Tarjeta(22, 82, 796, 420); panel.Controls.Add(card);
            CardHeader(card, "Selecciona el usuario y asigna el nuevo PIN");

            // Usuario
            MkLabel("Usuario", card, 16, 54);
            ComboBox cmb = new ComboBox();
            cmb.Size = new Size(520, 36); cmb.Location = new Point(16, 74);
            cmb.Font = new Font("Segoe UI", 12); cmb.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb.BackColor = Fondo; cmb.FlatStyle = FlatStyle.Flat;
            card.Controls.Add(cmb);

            Button btnRef = new Button(); btnRef.Text = "⟳";
            btnRef.Size = new Size(44, 36); btnRef.Location = new Point(544, 74);
            btnRef.FlatStyle = FlatStyle.Flat; btnRef.FlatAppearance.BorderSize = 1;
            btnRef.FlatAppearance.BorderColor = GrisBorde; btnRef.BackColor = Fondo;
            btnRef.ForeColor = Azul2; btnRef.Cursor = Cursors.Hand;
            btnRef.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            btnRef.Click += (s, e) => CargarCmb(cmb);
            card.Controls.Add(btnRef); CargarCmb(cmb);

            // Separador
            HSep(card, 126, 764);

            // Nuevo PIN
            MkLabel("Nuevo PIN", card, 16, 144);
            TextBox txtN = MkBox(card, 16, 164, 300, true); txtN.MaxLength = 4;
            txtN.Font = new Font("Segoe UI", 14);
            Button ojo1 = OjoBtn(txtN); ojo1.Location = new Point(320, 164); card.Controls.Add(ojo1);

            // Indicador de fuerza
            Panel indBar = new Panel();
            indBar.Size = new Size(300, 6); indBar.Location = new Point(16, 210);
            indBar.BackColor = GrisBorde;
            card.Controls.Add(indBar);

            Panel indFill = new Panel();
            indFill.Size = new Size(0, 6); indFill.Location = new Point(0, 0);
            indFill.BackColor = Verde; indBar.Controls.Add(indFill);

            Label lblSeg = new Label();
            lblSeg.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lblSeg.Location = new Point(16, 220); lblSeg.Size = new Size(400, 20);
            lblSeg.ForeColor = GrisTexto; card.Controls.Add(lblSeg);

            txtN.TextChanged += (s, e) =>
            {
                int len = txtN.Text.Length;
                if (len == 0) { lblSeg.Text = ""; indFill.Width = 0; return; }
                if (len < 4)
                {
                    lblSeg.Text = $"Faltan {4 - len} dígito(s) ❌";
                    lblSeg.ForeColor = Rojo;
                    indFill.BackColor = Rojo;
                    indFill.Width = (300 / 4) * len;
                }
                else
                {
                    lblSeg.Text = "PIN completo ✅";
                    lblSeg.ForeColor = Verde;
                    indFill.BackColor = Verde;
                    indFill.Width = 300;
                }
            };

            // Confirmar PIN
            MkLabel("Confirmar nuevo PIN", card, 16, 248);
            TextBox txtC = MkBox(card, 16, 268, 300, true); txtC.MaxLength = 4;
            txtC.Font = new Font("Segoe UI", 14);
            Button ojo2 = OjoBtn(txtC); ojo2.Location = new Point(320, 268); card.Controls.Add(ojo2);

            Label lblMatch = new Label();
            lblMatch.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lblMatch.Location = new Point(16, 310); lblMatch.Size = new Size(400, 20);
            lblMatch.ForeColor = GrisTexto; card.Controls.Add(lblMatch);

            txtC.TextChanged += (s, e) =>
            {
                if (txtC.Text.Length == 0) { lblMatch.Text = ""; return; }
                if (txtN.Text == txtC.Text) { lblMatch.Text = "Los PINs coinciden ✅"; lblMatch.ForeColor = Verde; }
                else { lblMatch.Text = "Los PINs no coinciden ❌"; lblMatch.ForeColor = Rojo; }
            };

            HSep(card, 340, 764);

            // Botones
            Button btnAct = BtnAcc("🔑  Actualizar PIN", Azul2, card, 16, 358, 220, 48);
            Button btnLmp = BtnAcc("Cancelar", Color.FromArgb(175, 185, 200), card, 250, 358, 140, 48);
            btnLmp.ForeColor = Azul1;
            HoverBtn(btnAct, AzulHov, Azul2);
            btnLmp.Click += (s, e) => { txtN.Clear(); txtC.Clear(); lblSeg.Text = ""; lblMatch.Text = ""; indFill.Width = 0; };

            btnAct.Click += (s, e) =>
            {
                if (cmb.SelectedItem == null) { Toast("Selecciona un usuario ❌", Rojo); return; }
                string nv = txtN.Text.Trim(), cf = txtC.Text.Trim();
                if (nv == "" || cf == "") { Toast("Llena todos los campos ❌", Rojo); return; }
                if (nv != cf) { Toast("Los PINs no coinciden ❌", Rojo); return; }
                if (nv.Length != 4) { Toast("El PIN debe tener exactamente 4 dígitos ❌", Rojo); return; }
                if (MessageBox.Show("¿Cambiar el PIN de este usuario?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                int id = (int)cmb.SelectedValue;
                try
                {
                    using (var con = Conn())
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand("UPDATE usuario SET pin_acceso=@p WHERE id_usuario=@id", con))
                        { cmd.Parameters.AddWithValue("@p", nv); cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
                    }
                    Toast("PIN actualizado correctamente ✅", Verde);
                    txtN.Clear(); txtC.Clear(); lblSeg.Text = ""; lblMatch.Text = ""; indFill.Width = 0;
                }
                catch (Exception ex) { Toast("Error: " + ex.Message, Rojo); }
            };

            return panel;
        }

        // ══════════════════════════════════════════════════════
        //  HELPERS DB
        // ══════════════════════════════════════════════════════
        private SQLiteConnection Conn() =>
            new SQLiteConnection(ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString);

        private void CargarUsuarios(ListView lista)
        {
            lista.Items.Clear();
            try
            {
                using (var con = Conn())
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(
                        "SELECT id_usuario,nombre,rol,estado,pin_acceso,permisos FROM usuario ORDER BY id_usuario", con))
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var it = new ListViewItem(r["id_usuario"].ToString());
                            it.SubItems.Add(r["nombre"].ToString());
                            it.SubItems.Add(r["rol"].ToString());
                            it.SubItems.Add(r["estado"].ToString() == "1" ? "✅ Activo" : "❌ Inactivo");
                            it.SubItems.Add(r["pin_acceso"].ToString());
                            it.SubItems.Add(r["permisos"] == DBNull.Value ? "" : r["permisos"].ToString());
                            lista.Items.Add(it);
                        }
                    }
                }
            }
            catch { }
        }

        private void CargarCmb(ComboBox cmb)
        {
            try
            {
                using (var con = Conn())
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(
                        "SELECT id_usuario, nombre||' ('||rol||')' AS d FROM usuario WHERE estado=1 ORDER BY nombre", con))
                    using (var r = cmd.ExecuteReader())
                    {
                        var t = new System.Data.DataTable();
                        t.Columns.Add("id_usuario", typeof(int)); t.Columns.Add("d", typeof(string));
                        while (r.Read()) t.Rows.Add(r["id_usuario"], r["d"]);
                        cmb.DataSource = t; cmb.DisplayMember = "d"; cmb.ValueMember = "id_usuario";
                    }
                }
            }
            catch { }
        }

        private string ObtenerPermisos(Dictionary<string, CheckBox> checks)
        {
            var l = new List<string>();
            foreach (var kv in checks) if (kv.Value.Checked) l.Add(kv.Key);
            return string.Join(",", l);
        }

        // ══════════════════════════════════════════════════════
        //  HELPERS UI
        // ══════════════════════════════════════════════════════
        [System.Runtime.InteropServices.DllImport("Gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int l, int t, int r, int b, int w, int h);

        private GraphicsPath RoundPath(Rectangle r, int rad)
        {
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, rad * 2, rad * 2, 180, 90);
            p.AddArc(r.Right - rad * 2, r.Y, rad * 2, rad * 2, 270, 90);
            p.AddArc(r.Right - rad * 2, r.Bottom - rad * 2, rad * 2, rad * 2, 0, 90);
            p.AddArc(r.X, r.Bottom - rad * 2, rad * 2, rad * 2, 90, 90);
            p.CloseFigure(); return p;
        }

        private void SetDB(Panel p) =>
            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, p, new object[] { true });

        private Button NavBtn(string text, int y, Panel sidebar)
        {
            Button btn = new Button();
            btn.Text = text; btn.Size = new Size(220, 44); btn.Location = new Point(0, y);
            btn.FlatStyle = FlatStyle.Flat; btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(35, 255, 255, 255);
            btn.BackColor = Color.Transparent; btn.ForeColor = Color.FromArgb(210, 235, 245);
            btn.Font = new Font("Segoe UI", 10); btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(22, 0, 0, 0); btn.Cursor = Cursors.Hand;
            sidebar.Controls.Add(btn); return btn;
        }

        private void SSep(Panel s, int y)
        {
            Panel p = new Panel(); p.Size = new Size(180, 1); p.Location = new Point(20, y);
            p.BackColor = Color.FromArgb(60, 255, 255, 255); s.Controls.Add(p);
        }

        private void SLabel(Panel s, string t, int y)
        {
            Label l = new Label(); l.Text = t; l.Font = new Font("Segoe UI", 7, FontStyle.Bold);
            l.ForeColor = Color.FromArgb(140, 200, 220); l.Size = new Size(220, 20);
            l.Location = new Point(0, y); l.TextAlign = ContentAlignment.MiddleCenter;
            s.Controls.Add(l);
        }

        private void ActivarNav(Button btn)
        {
            if (btnNavActivo != null)
            {
                btnNavActivo.BackColor = Color.Transparent;
                btnNavActivo.ForeColor = Color.FromArgb(210, 235, 245);
                btnNavActivo.Font = new Font("Segoe UI", 10);
            }
            btn.BackColor = Color.FromArgb(45, 255, 255, 255);
            btn.ForeColor = Color.White; btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnNavActivo = btn;
        }

        private void MostrarSeccion(Panel sec)
        {
            panelContenido.Controls.Clear();
            sec.Dock = DockStyle.Fill;
            panelContenido.Controls.Add(sec);
        }

        private void Encabezado(Panel panel, string titulo, string sub)
        {
            Label t = new Label(); t.Text = titulo;
            t.Font = new Font("Segoe UI", 15, FontStyle.Bold); t.ForeColor = Azul1;
            t.Location = new Point(22, 16); t.Size = new Size(790, 30); panel.Controls.Add(t);

            Label s = new Label(); s.Text = sub;
            s.Font = new Font("Segoe UI", 9); s.ForeColor = GrisTexto;
            s.Location = new Point(22, 48); s.Size = new Size(790, 18); panel.Controls.Add(s);

            Panel acc = new Panel(); acc.Size = new Size(46, 3);
            acc.Location = new Point(22, 69); acc.BackColor = Aqua; panel.Controls.Add(acc);
        }

        private Panel Tarjeta(int x, int y, int w, int h)
        {
            Panel c = new Panel(); c.Size = new Size(w, h); c.Location = new Point(x, y);
            c.BackColor = Blanco;
            c.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, c.ClientRectangle,
                GrisBorde, 1, ButtonBorderStyle.Solid, GrisBorde, 1, ButtonBorderStyle.Solid,
                GrisBorde, 1, ButtonBorderStyle.Solid, GrisBorde, 1, ButtonBorderStyle.Solid);
            return c;
        }

        private void CardHeader(Panel card, string texto)
        {
            Panel ch = new Panel(); ch.Size = new Size(card.Width, 40); ch.Location = new Point(0, 0);
            ch.BackColor = Color.FromArgb(243, 247, 252);
            ch.Paint += (s, e) => e.Graphics.DrawLine(new Pen(GrisBorde), 0, 39, card.Width, 39);
            Panel bar = new Panel(); bar.Size = new Size(4, 40); bar.Location = new Point(0, 0);
            bar.BackColor = Aqua; ch.Controls.Add(bar);
            Label lbl = new Label(); lbl.Text = texto;
            lbl.Font = new Font("Segoe UI", 9, FontStyle.Bold); lbl.ForeColor = Azul2;
            lbl.Location = new Point(14, 0); lbl.Size = new Size(card.Width - 16, 40);
            lbl.TextAlign = ContentAlignment.MiddleLeft; ch.Controls.Add(lbl);
            card.Controls.Add(ch);
        }

        private void HSep(Panel p, int y, int w)
        {
            Panel sep = new Panel(); sep.Size = new Size(w, 1);
            sep.Location = new Point(16, y); sep.BackColor = GrisBorde; p.Controls.Add(sep);
        }

        private Label MkLabel(string t, Panel p, int x, int y)
        {
            Label l = new Label(); l.Text = t; l.Font = new Font("Segoe UI", 9);
            l.ForeColor = GrisTexto; l.Location = new Point(x, y); l.AutoSize = true;
            p.Controls.Add(l); return l;
        }

        private TextBox MkBox(Panel p, int x, int y, int w, bool pwd)
        {
            TextBox txt = new TextBox(); txt.Size = new Size(w, 34); txt.Location = new Point(x, y);
            txt.Font = new Font("Segoe UI", 11); txt.BackColor = Fondo;
            txt.BorderStyle = BorderStyle.FixedSingle; txt.UseSystemPasswordChar = pwd;
            p.Controls.Add(txt); return txt;
        }

        private Button BtnAcc(string t, Color bg, Panel p, int x, int y, int w, int h)
        {
            Button btn = new Button(); btn.Text = t; btn.Size = new Size(w, h); btn.Location = new Point(x, y);
            btn.BackColor = bg; btn.ForeColor = Blanco; btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0; btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.Cursor = Cursors.Hand; p.Controls.Add(btn); return btn;
        }

        private void HoverBtn(Button btn, Color hover, Color normal)
        {
            btn.MouseEnter += (s, e) => btn.BackColor = hover;
            btn.MouseLeave += (s, e) => btn.BackColor = normal;
        }

        private Button OjoBtn(TextBox txt)
        {
            Button btn = new Button(); btn.Text = "👁"; btn.Size = new Size(42, 34);
            btn.FlatStyle = FlatStyle.Flat; btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = GrisBorde; btn.BackColor = Fondo;
            btn.Cursor = Cursors.Hand; btn.Font = new Font("Segoe UI", 12);
            btn.Click += (s, e) => { txt.UseSystemPasswordChar = !txt.UseSystemPasswordChar; btn.Text = txt.UseSystemPasswordChar ? "👁" : "🙈"; };
            return btn;
        }

        private void Toast(string msg, Color color)
        {
            Form toast = new Form();
            toast.FormBorderStyle = FormBorderStyle.None; toast.StartPosition = FormStartPosition.Manual;
            toast.Size = new Size(360, 54); toast.BackColor = color; toast.Opacity = 0.96;
            toast.TopMost = true; toast.ShowInTaskbar = false;
            toast.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, 360, 54, 10, 10));
            toast.Location = new Point(this.Left + this.Width - 375, this.Top + this.Height - 74);
            Label lbl = new Label(); lbl.Text = msg; lbl.ForeColor = Blanco;
            lbl.Font = new Font("Segoe UI", 10, FontStyle.Bold); lbl.Dock = DockStyle.Fill;
            lbl.TextAlign = ContentAlignment.MiddleCenter; toast.Controls.Add(lbl);
            toast.Show(this);
            var t = new System.Windows.Forms.Timer(); t.Interval = 2800;
            t.Tick += (s, e) => { t.Stop(); toast.Close(); }; t.Start();
        }
    }
}