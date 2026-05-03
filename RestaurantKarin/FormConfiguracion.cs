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
        // ── Paleta coherente con el proyecto ──────────────────
        private readonly Color C_GRAD_TOP = Color.FromArgb(64, 196, 204); // turquesa sidebar top
        private readonly Color C_GRAD_BOT = Color.FromArgb(13, 41, 78); // azul sidebar bottom
        private readonly Color C_ACCENT = Color.FromArgb(64, 196, 204); // turquesa accent
        private readonly Color C_BG = Color.FromArgb(244, 247, 251); // fondo página
        private readonly Color C_SURFACE = Color.White;
        private readonly Color C_BORDER = Color.FromArgb(220, 228, 240);
        private readonly Color C_TEXT1 = Color.FromArgb(13, 41, 78); // texto principal (azul oscuro del proyecto)
        private readonly Color C_TEXT2 = Color.FromArgb(100, 120, 150); // texto secundario
        private readonly Color C_PRIMARY = Color.FromArgb(29, 78, 137); // botón primario
        private readonly Color C_PRIMARY_H = Color.FromArgb(20, 58, 107); // hover primario
        private readonly Color C_DANGER = Color.FromArgb(210, 45, 45);
        private readonly Color C_DANGER_BG = Color.FromArgb(255, 240, 240);
        private readonly Color C_SUCCESS = Color.FromArgb(22, 155, 70);
        private readonly Color C_NAV_ACT = Color.FromArgb(50, 255, 255, 255);

        private readonly string[] MODULOS = { "Pedidos", "Cuentas", "Inventario", "Recetas", "Reportes" };

        private Panel _content;
        private Button _activeNav;

        public FormConfiguracion()
        {
            InitializeComponent();
            BuildShell();
        }

        // ════════════════════════════════════════════════════════
        //  SHELL PRINCIPAL
        // ════════════════════════════════════════════════════════
        private void BuildShell()
        {
            this.Text = "Ajustes";
            this.Size = new Size(1100, 740);
            this.MinimumSize = new Size(1100, 740);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = C_BG;
            this.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, Width, Height, 14, 14));

            // ── SIDEBAR 240px ─────────────────────────────────
            Panel sb = new Panel();
            sb.Size = new Size(240, 740); sb.Location = Point.Empty;
            DoubleBufferPanel(sb);
            sb.Paint += (s, e) =>
            {
                using (var br = new LinearGradientBrush(
                    sb.ClientRectangle, C_GRAD_TOP, C_GRAD_BOT, 90F))
                    e.Graphics.FillRectangle(br, sb.ClientRectangle);
            };
            this.Controls.Add(sb);

            // Logo — más grande como pediste
            PictureBox logo = new PictureBox();
            logo.Size = new Size(80, 80); logo.Location = new Point(80, 24);
            logo.SizeMode = PictureBoxSizeMode.Zoom; logo.BackColor = Color.Transparent;
            try { logo.Image = Image.FromFile(Path.Combine(Application.StartupPath, "Imgs", "icono.ico")); } catch { }
            sb.Controls.Add(logo);

            // Título app
            Label lApp = new Label();
            lApp.Text = "AJUSTES"; lApp.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lApp.ForeColor = Color.White; lApp.Size = new Size(240, 24);
            lApp.Location = new Point(0, 112); lApp.TextAlign = ContentAlignment.MiddleCenter;
            sb.Controls.Add(lApp);

            // Chip usuario
            Label lUser = new Label();
            lUser.Text = "● " + Sesion.Nombre;
            lUser.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lUser.ForeColor = Color.FromArgb(210, 240, 245);
            lUser.Size = new Size(200, 28); lUser.Location = new Point(20, 142);
            lUser.TextAlign = ContentAlignment.MiddleCenter;
            lUser.BackColor = Color.FromArgb(30, 255, 255, 255);
            sb.Controls.Add(lUser);
            // Redondear chip
            lUser.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var p = RoundedPath(new Rectangle(0, 0, lUser.Width - 1, lUser.Height - 1), 14))
                using (var br = new SolidBrush(Color.FromArgb(30, 255, 255, 255)))
                    g.FillPath(br, p);
            };

            // Separador + label menú
            sb.Controls.Add(SbSep(20, 186, 200));
            Label lMenu = new Label();
            lMenu.Text = "NAVEGACIÓN"; lMenu.Font = new Font("Segoe UI", 7, FontStyle.Bold);
            lMenu.ForeColor = Color.FromArgb(160, 220, 235);
            lMenu.Size = new Size(240, 18); lMenu.Location = new Point(0, 196);
            lMenu.TextAlign = ContentAlignment.MiddleCenter;
            sb.Controls.Add(lMenu);

            Button nU = SbNavBtn("Usuarios", 220, sb);
            Button nP = SbNavBtn("Cambiar PIN", 268, sb);

            // Separador inferior
            sb.Controls.Add(SbSep(20, 668, 200));

            // Botón cerrar — bien ubicado arriba del borde
            Button btnX = new Button();
            btnX.Text = "✕  Cerrar"; btnX.Size = new Size(200, 42);
            btnX.Location = new Point(20, 678);
            btnX.FlatStyle = FlatStyle.Flat; btnX.FlatAppearance.BorderSize = 0;
            btnX.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, C_DANGER);
            btnX.BackColor = Color.Transparent; btnX.ForeColor = Color.FromArgb(200, 215, 225);
            btnX.Font = new Font("Segoe UI", 10); btnX.TextAlign = ContentAlignment.MiddleLeft;
            btnX.Padding = new Padding(12, 0, 0, 0); btnX.Cursor = Cursors.Hand;
            btnX.Click += (s, e) => this.Close();
            sb.Controls.Add(btnX);

            // ── TOPBAR 58px ───────────────────────────────────
            Panel top = new Panel();
            top.Size = new Size(860, 58); top.Location = new Point(240, 0);
            top.BackColor = C_SURFACE; this.Controls.Add(top); top.BringToFront();

            Label lTitle = new Label();
            lTitle.Text = "Panel de Administración";
            lTitle.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            lTitle.ForeColor = C_TEXT1; lTitle.Location = new Point(28, 0);
            lTitle.Size = new Size(600, 58); lTitle.TextAlign = ContentAlignment.MiddleLeft;
            top.Controls.Add(lTitle);

            // Badge admin
            Label lBadge = new Label();
            lBadge.Text = "Admin"; lBadge.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            lBadge.ForeColor = C_PRIMARY; lBadge.BackColor = Color.FromArgb(220, 234, 255);
            lBadge.Size = new Size(56, 24); lBadge.Location = new Point(786, 17);
            lBadge.TextAlign = ContentAlignment.MiddleCenter;
            top.Controls.Add(lBadge);
            lBadge.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var p = RoundedPath(new Rectangle(0, 0, lBadge.Width - 1, lBadge.Height - 1), 12))
                using (var br = new SolidBrush(Color.FromArgb(220, 234, 255)))
                    g.FillPath(br, p);
                using (var p = RoundedPath(new Rectangle(0, 0, lBadge.Width - 1, lBadge.Height - 1), 12))
                using (var pen = new Pen(Color.FromArgb(180, 210, 255)))
                    g.DrawPath(pen, p);
            };

            top.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(C_BORDER), 0, 57, 860, 57);

            // Arrastre
            bool drag = false; Point dp = Point.Empty;
            top.MouseDown += (s, e) => { drag = true; dp = e.Location; };
            top.MouseMove += (s, e) =>
            {
                if (drag) this.Location = new Point(
                    this.Location.X + e.X - dp.X, this.Location.Y + e.Y - dp.Y);
            };
            top.MouseUp += (s, e) => drag = false;

            // ── Área de contenido ─────────────────────────────
            _content = new Panel();
            _content.Size = new Size(860, 682); _content.Location = new Point(240, 58);
            _content.BackColor = C_BG;
            this.Controls.Add(_content);

            nU.Click += (s, e) => { ActivarNav(nU); LoadPage(PageUsuarios()); };
            nP.Click += (s, e) => { ActivarNav(nP); LoadPage(PagePin()); };

            ActivarNav(nU);
            LoadPage(PageUsuarios());
        }

        // ════════════════════════════════════════════════════════
        //  PÁGINA USUARIOS
        // ════════════════════════════════════════════════════════
        private Panel PageUsuarios()
        {
            Panel page = new Panel(); page.BackColor = C_BG;

            // ── Encabezado de página ──
            PageHeader(page, "Gestión de Usuarios",
                "Agrega, edita permisos y elimina usuarios del sistema");

            // ── CARD: Tabla de usuarios ──
            Panel cT = Card(24, 92, 812, 218, page);

            CardTitle(cT, "Usuarios registrados", 20, 16);

            // ListView rediseñada
            ListView lv = BuildLV(cT, 20, 46, 772, 158);
            lv.Columns.Add("", 26);   // dot estado
            lv.Columns.Add("Nombre", 188);
            lv.Columns.Add("Rol", 94);
            lv.Columns.Add("Estado", 80);
            lv.Columns.Add("PIN", 64);
            lv.Columns.Add("Pantallas permitidas", 280);
            LoadUsers(lv);

            // ── CARD: Agregar usuario ──
            Panel cA = Card(24, 324, 812, 188, page);
            CardTitle(cA, "Agregar nuevo usuario", 20, 16);

            // Fila inputs Y=50
            FieldLbl("Nombre", cA, 20, 50);
            TextBox tNom = RoundInput(cA, 20, 68, 228);

            FieldLbl("Rol", cA, 264, 50);
            ComboBox cRol = RoundCombo(cA, 264, 68, 148);
            cRol.Items.AddRange(new object[] { "Mesero", "Admin" });
            cRol.SelectedIndex = 0;

            FieldLbl("PIN (4 dígitos)", cA, 428, 50);
            TextBox tPin = RoundInput(cA, 428, 68, 118, pwd: true); tPin.MaxLength = 4;

            // Checkboxes permisos
            FieldLbl("Pantallas permitidas", cA, 20, 114);
            var cks = PermChecks(cA, 20, 134);

            cRol.SelectedIndexChanged += (s, e) =>
            {
                bool adm = cRol.SelectedItem.ToString() == "Admin";
                foreach (var cb in cks.Values) { cb.Checked = true; cb.Enabled = !adm; }
            };

            Button btnAdd = BtnPrimary("Agregar", cA, 664, 68, 128, 38);
            btnAdd.Click += (s, e) =>
            {
                string nom = tNom.Text.Trim(), rol = cRol.SelectedItem.ToString(), pin = tPin.Text.Trim();
                if (nom == "" || pin == "") { Toast("Completa nombre y PIN", false); return; }
                if (pin.Length != 4) { Toast("El PIN debe tener 4 dígitos", false); return; }
                string perms = rol == "Admin" ? string.Join(",", MODULOS) : GetPerms(cks);
                try
                {
                    using (var con = Conn())
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand(
                            "INSERT INTO usuario (nombre,rol,pin_acceso,permisos) VALUES (@n,@r,@p,@pe)", con))
                        {
                            cmd.Parameters.AddWithValue("@n", nom); cmd.Parameters.AddWithValue("@r", rol);
                            cmd.Parameters.AddWithValue("@p", pin); cmd.Parameters.AddWithValue("@pe", perms);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    Toast("Usuario agregado correctamente", true);
                    tNom.Clear(); tPin.Clear(); LoadUsers(lv);
                }
                catch (Exception ex) { Toast("Error: " + ex.Message, false); }
            };

            // ── CARD: Editar permisos ──
            Panel cE = Card(24, 526, 812, 142, page);
            CardTitle(cE, "Permisos del usuario seleccionado", 20, 16);

            FieldLbl("Pantallas habilitadas", cE, 20, 50);
            var cksE = PermChecks(cE, 20, 68);

            lv.SelectedIndexChanged += (s, e) =>
            {
                if (lv.SelectedItems.Count == 0) return;
                string perm = lv.SelectedItems[0].SubItems[5].Text;
                string rol2 = lv.SelectedItems[0].SubItems[2].Text;
                foreach (string m in MODULOS)
                {
                    cksE[m].Checked = perm.Contains(m);
                    cksE[m].Enabled = rol2 != "Admin";
                }
            };

            Button btnPerm = BtnSecondary("Guardar permisos", cE, 534, 60, 162, 38);
            Button btnDel = BtnDanger("Eliminar", cE, 710, 60, 102, 38);

            btnPerm.Click += (s, e) =>
            {
                if (lv.SelectedItems.Count == 0) { Toast("Selecciona un usuario", false); return; }
                string rol2 = lv.SelectedItems[0].SubItems[2].Text;
                string perms = rol2 == "Admin" ? string.Join(",", MODULOS) : GetPerms(cksE);
                int id = GetId(lv);
                if (id == 0) { Toast("Selecciona un usuario válido", false); return; }
                try
                {
                    using (var con = Conn())
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand(
                            "UPDATE usuario SET permisos=@pe WHERE id_usuario=@id", con))
                        {
                            cmd.Parameters.AddWithValue("@pe", perms); cmd.Parameters.AddWithValue("@id", id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    Toast("Permisos actualizados", true); LoadUsers(lv);
                }
                catch (Exception ex) { Toast("Error: " + ex.Message, false); }
            };

            btnDel.Click += (s, e) =>
            {
                if (lv.SelectedItems.Count == 0) { Toast("Selecciona un usuario", false); return; }
                string nom2 = lv.SelectedItems[0].SubItems[1].Text;
                string rol2 = lv.SelectedItems[0].SubItems[2].Text;
                int id = GetId(lv);
                if (rol2 == "Admin" && nom2 == Sesion.Nombre)
                { Toast("No puedes eliminar tu propia cuenta", false); return; }
                if (MessageBox.Show($"¿Eliminar a '{nom2}'?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                try
                {
                    using (var con = Conn())
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand(
                            "DELETE FROM usuario WHERE id_usuario=@id", con))
                        { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
                    }
                    Toast("Usuario eliminado", true); LoadUsers(lv);
                }
                catch (Exception ex) { Toast("Error: " + ex.Message, false); }
            };

            return page;
        }

        // ════════════════════════════════════════════════════════
        //  PÁGINA CAMBIAR PIN
        // ════════════════════════════════════════════════════════
        private Panel PagePin()
        {
            Panel page = new Panel(); page.BackColor = C_BG;
            PageHeader(page, "Cambiar PIN",
                "Reasigna el PIN de acceso de cualquier usuario del sistema");

            Panel card = Card(24, 92, 812, 456, page);
            CardTitle(card, "Selecciona usuario y nuevo PIN", 20, 16);

            // Combo usuario
            FieldLbl("Usuario", card, 20, 52);
            ComboBox cmb = RoundCombo(card, 20, 70, 510);
            LoadCmb(cmb);

            // Botón refresh
            Button btnR = new Button();
            btnR.Text = "↺"; btnR.Size = new Size(40, 38); btnR.Location = new Point(540, 70);
            btnR.FlatStyle = FlatStyle.Flat; btnR.FlatAppearance.BorderSize = 1;
            btnR.FlatAppearance.BorderColor = C_BORDER; btnR.BackColor = C_BG;
            btnR.ForeColor = C_TEXT2; btnR.Cursor = Cursors.Hand;
            btnR.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            btnR.Click += (s, e) => LoadCmb(cmb);
            card.Controls.Add(btnR);

            // Separador
            card.Controls.Add(HLine(20, 126, 772, C_BORDER));

            // Nuevo PIN
            FieldLbl("Nuevo PIN", card, 20, 146);
            TextBox tN = RoundInput(card, 20, 166, 300, pwd: true);
            tN.MaxLength = 4; tN.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            Button oj1 = EyeBtn(tN); oj1.Location = new Point(328, 169); card.Controls.Add(oj1);

            // Barra de progreso PIN
            Panel barBg = new Panel();
            barBg.Size = new Size(300, 5); barBg.Location = new Point(20, 216);
            barBg.BackColor = C_BORDER; card.Controls.Add(barBg);
            Panel barFg = new Panel();
            barFg.Size = new Size(0, 5); barFg.Location = Point.Empty;
            barFg.BackColor = C_PRIMARY; barBg.Controls.Add(barFg);

            Label lSeg = new Label();
            lSeg.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lSeg.Location = new Point(20, 228); lSeg.Size = new Size(400, 18);
            lSeg.ForeColor = C_TEXT2; card.Controls.Add(lSeg);

            tN.TextChanged += (s, e) =>
            {
                int n = tN.Text.Length;
                if (n == 0) { lSeg.Text = ""; barFg.Width = 0; return; }
                barFg.Width = (300 / 4) * n;
                if (n < 4) { lSeg.Text = $"Faltan {4 - n} dígito(s)"; lSeg.ForeColor = C_DANGER; barFg.BackColor = C_DANGER; }
                else { lSeg.Text = "PIN completo ✓"; lSeg.ForeColor = C_SUCCESS; barFg.BackColor = C_SUCCESS; }
            };

            // Confirmar PIN
            FieldLbl("Confirmar PIN", card, 20, 260);
            TextBox tC = RoundInput(card, 20, 278, 300, pwd: true);
            tC.MaxLength = 4; tC.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            Button oj2 = EyeBtn(tC); oj2.Location = new Point(328, 281); card.Controls.Add(oj2);

            Label lMatch = new Label();
            lMatch.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lMatch.Location = new Point(20, 322); lMatch.Size = new Size(400, 18);
            lMatch.ForeColor = C_TEXT2; card.Controls.Add(lMatch);

            tC.TextChanged += (s, e) =>
            {
                if (tC.Text.Length == 0) { lMatch.Text = ""; return; }
                bool ok = tN.Text == tC.Text;
                lMatch.Text = ok ? "Los PINs coinciden ✓" : "Los PINs no coinciden";
                lMatch.ForeColor = ok ? C_SUCCESS : C_DANGER;
            };

            card.Controls.Add(HLine(20, 358, 772, C_BORDER));

            // Botones
            Button btnSave = BtnPrimary("Actualizar PIN", card, 20, 378, 180, 46);
            Button btnCnc = BtnSecondary("Cancelar", card, 214, 378, 120, 46);

            btnCnc.Click += (s, e) =>
            {
                tN.Clear(); tC.Clear(); lSeg.Text = ""; lMatch.Text = "";
                barFg.Width = 0; barFg.BackColor = C_PRIMARY;
            };

            btnSave.Click += (s, e) =>
            {
                if (cmb.SelectedItem == null) { Toast("Selecciona un usuario", false); return; }
                string nv = tN.Text.Trim(), cf = tC.Text.Trim();
                if (nv == "" || cf == "") { Toast("Completa ambos campos", false); return; }
                if (nv != cf) { Toast("Los PINs no coinciden", false); return; }
                if (nv.Length != 4) { Toast("El PIN debe tener 4 dígitos", false); return; }
                if (MessageBox.Show("¿Actualizar el PIN?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                int id = (int)cmb.SelectedValue;
                try
                {
                    using (var con = Conn())
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand(
                            "UPDATE usuario SET pin_acceso=@p WHERE id_usuario=@id", con))
                        {
                            cmd.Parameters.AddWithValue("@p", nv); cmd.Parameters.AddWithValue("@id", id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    Toast("PIN actualizado correctamente", true);
                    tN.Clear(); tC.Clear(); lSeg.Text = ""; lMatch.Text = "";
                    barFg.Width = 0; barFg.BackColor = C_PRIMARY;
                }
                catch (Exception ex) { Toast("Error: " + ex.Message, false); }
            };

            return page;
        }

        // ════════════════════════════════════════════════════════
        //  FACTORY DE CONTROLES
        // ════════════════════════════════════════════════════════

        private void PageHeader(Panel page, string titulo, string sub)
        {
            Label t = new Label();
            t.Text = titulo; t.Font = new Font("Segoe UI", 17, FontStyle.Bold);
            t.ForeColor = C_TEXT1; t.Location = new Point(24, 18);
            t.Size = new Size(810, 32); page.Controls.Add(t);

            Label s = new Label();
            s.Text = sub; s.Font = new Font("Segoe UI", 10);
            s.ForeColor = C_TEXT2; s.Location = new Point(24, 52);
            s.Size = new Size(810, 20); page.Controls.Add(s);

            Panel acc = new Panel();
            acc.Size = new Size(44, 3); acc.Location = new Point(24, 76);
            acc.BackColor = C_ACCENT; page.Controls.Add(acc);
        }

        private Panel Card(int x, int y, int w, int h, Panel parent)
        {
            Panel c = new Panel();
            c.Size = new Size(w, h); c.Location = new Point(x, y);
            c.BackColor = C_SURFACE;
            c.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var p = RoundedPath(new Rectangle(0, 0, c.Width - 1, c.Height - 1), 10))
                using (var pen = new Pen(C_BORDER))
                { g.FillPath(Brushes.White, p); g.DrawPath(pen, p); }
            };
            parent.Controls.Add(c); return c;
        }

        private void CardTitle(Panel card, string text, int x, int y)
        {
            Label l = new Label();
            l.Text = text; l.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            l.ForeColor = C_TEXT1; l.Location = new Point(x, y);
            l.Size = new Size(600, 22); card.Controls.Add(l);

            Panel acc = new Panel();
            acc.Size = new Size(32, 2); acc.Location = new Point(x, y + 24);
            acc.BackColor = C_ACCENT; card.Controls.Add(acc);
        }

        private Label FieldLbl(string text, Panel p, int x, int y)
        {
            Label l = new Label();
            l.Text = text; l.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            l.ForeColor = C_TEXT2; l.Location = new Point(x, y);
            l.AutoSize = true; p.Controls.Add(l); return l;
        }

        // Input redondeado — dibujado con owner draw
        private TextBox RoundInput(Panel p, int x, int y, int w, bool pwd = false)
        {
            Panel wrap = new Panel();
            wrap.Size = new Size(w, 38); wrap.Location = new Point(x, y);
            wrap.BackColor = C_BG;
            wrap.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                Color bc = wrap.ContainsFocus ? C_PRIMARY : C_BORDER;
                using (var path = RoundedPath(new Rectangle(0, 0, wrap.Width - 1, wrap.Height - 1), 8))
                {
                    g.FillPath(new SolidBrush(C_BG), path);
                    g.DrawPath(new Pen(bc, 1.5f), path);
                }
            };
            p.Controls.Add(wrap);

            TextBox t = new TextBox();
            t.Size = new Size(w - 16, 24); t.Location = new Point(8, 7);
            t.BorderStyle = BorderStyle.None; t.BackColor = C_BG;
            t.Font = new Font("Segoe UI", 11); t.UseSystemPasswordChar = pwd;
            t.Enter += (s, e) => wrap.Invalidate();
            t.Leave += (s, e) => wrap.Invalidate();
            wrap.Controls.Add(t); return t;
        }

        private ComboBox RoundCombo(Panel p, int x, int y, int w)
        {
            ComboBox c = new ComboBox();
            c.Size = new Size(w, 38); c.Location = new Point(x, y);
            c.Font = new Font("Segoe UI", 11); c.BackColor = C_BG;
            c.DropDownStyle = ComboBoxStyle.DropDownList; c.FlatStyle = FlatStyle.Flat;
            p.Controls.Add(c); return c;
        }

        private ListView BuildLV(Panel p, int x, int y, int w, int h)
        {
            ListView lv = new ListView();
            lv.Size = new Size(w, h); lv.Location = new Point(x, y);
            lv.View = View.Details; lv.FullRowSelect = true;
            lv.GridLines = false; lv.BorderStyle = BorderStyle.None;
            lv.Font = new Font("Segoe UI", 10); lv.BackColor = C_SURFACE;
            lv.OwnerDraw = true;

            lv.DrawColumnHeader += (s, e) =>
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(246, 249, 253)), e.Bounds);
                e.Graphics.DrawLine(new Pen(C_BORDER), e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
                if (e.Header.Index == 0) { e.DrawDefault = false; return; }
                using (StringFormat sf = new StringFormat { LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString(e.Header.Text,
                        new Font("Segoe UI", 8, FontStyle.Bold),
                        new SolidBrush(C_TEXT2),
                        new RectangleF(e.Bounds.X + 10, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height), sf);
            };

            lv.DrawItem += (s, e) => e.DrawDefault = true;

            lv.DrawSubItem += (s, e) =>
            {
                bool sel = e.Item.Selected;
                Color bg = sel ? Color.FromArgb(218, 232, 255)
                         : e.ItemIndex % 2 == 0 ? C_SURFACE : Color.FromArgb(249, 251, 254);
                e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
                e.Graphics.DrawLine(new Pen(Color.FromArgb(240, 244, 250)),
                    e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);

                if (e.ColumnIndex == 0)
                {
                    bool activo = e.Item.SubItems.Count > 3 &&
                                  e.Item.SubItems[3].Text == "Activo";
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillEllipse(new SolidBrush(activo ? C_SUCCESS : C_DANGER),
                        e.Bounds.X + 7, e.Bounds.Y + 9, 10, 10);
                    return;
                }

                Color fg = e.ColumnIndex == 2 && e.SubItem.Text == "Admin"
                    ? C_PRIMARY : C_TEXT1;
                using (StringFormat sf = new StringFormat
                { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
                    e.Graphics.DrawString(e.SubItem.Text,
                        new Font("Segoe UI", e.ColumnIndex == 2 ? 9 : 10,
                            e.ColumnIndex == 2 ? FontStyle.Bold : FontStyle.Regular),
                        new SolidBrush(fg),
                        new RectangleF(e.Bounds.X + 10, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height), sf);
            };

            p.Controls.Add(lv); return lv;
        }

        private Dictionary<string, CheckBox> PermChecks(Panel p, int x, int y)
        {
            var d = new Dictionary<string, CheckBox>(); int cx = x;
            foreach (string m in MODULOS)
            {
                CheckBox cb = new CheckBox();
                cb.Text = m; cb.Font = new Font("Segoe UI", 9);
                cb.ForeColor = C_TEXT1; cb.Size = new Size(126, 24);
                cb.Location = new Point(cx, y); cb.Checked = true;
                cb.FlatStyle = FlatStyle.Flat;
                cb.FlatAppearance.BorderColor = C_BORDER;
                cb.FlatAppearance.CheckedBackColor = Color.FromArgb(220, 234, 255);
                p.Controls.Add(cb); d[m] = cb; cx += 132;
            }
            return d;
        }

        // ── Botones con bordes redondeados ────────────────────
        private Button BtnPrimary(string t, Panel p, int x, int y, int w, int h)
        {
            Button b = new Button();
            b.Text = t; b.Size = new Size(w, h); b.Location = new Point(x, y);
            b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 0;
            b.BackColor = C_PRIMARY; b.ForeColor = Color.White;
            b.Font = new Font("Segoe UI", 10, FontStyle.Bold); b.Cursor = Cursors.Hand;
            b.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, w, h, 8, 8));
            b.MouseEnter += (s, e) => b.BackColor = C_PRIMARY_H;
            b.MouseLeave += (s, e) => b.BackColor = C_PRIMARY;
            p.Controls.Add(b); return b;
        }

        private Button BtnSecondary(string t, Panel p, int x, int y, int w, int h)
        {
            Button b = new Button();
            b.Text = t; b.Size = new Size(w, h); b.Location = new Point(x, y);
            b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = C_BORDER;
            b.BackColor = C_SURFACE; b.ForeColor = C_TEXT1;
            b.Font = new Font("Segoe UI", 10); b.Cursor = Cursors.Hand;
            b.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, w, h, 8, 8));
            b.MouseEnter += (s, e) => b.BackColor = C_BG;
            b.MouseLeave += (s, e) => b.BackColor = C_SURFACE;
            p.Controls.Add(b); return b;
        }

        private Button BtnDanger(string t, Panel p, int x, int y, int w, int h)
        {
            Button b = new Button();
            b.Text = t; b.Size = new Size(w, h); b.Location = new Point(x, y);
            b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = Color.FromArgb(250, 200, 200);
            b.BackColor = C_DANGER_BG; b.ForeColor = C_DANGER;
            b.Font = new Font("Segoe UI", 10); b.Cursor = Cursors.Hand;
            b.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, w, h, 8, 8));
            b.MouseEnter += (s, e) => b.BackColor = Color.FromArgb(255, 225, 225);
            b.MouseLeave += (s, e) => b.BackColor = C_DANGER_BG;
            p.Controls.Add(b); return b;
        }

        private Button EyeBtn(TextBox txt)
        {
            Button b = new Button();
            b.Text = "○"; b.Size = new Size(38, 36);
            b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = C_BORDER; b.BackColor = C_BG;
            b.ForeColor = C_TEXT2; b.Font = new Font("Segoe UI", 10); b.Cursor = Cursors.Hand;
            b.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, 38, 36, 6, 6));
            b.Click += (s, e) =>
            {
                txt.UseSystemPasswordChar = !txt.UseSystemPasswordChar;
                b.Text = txt.UseSystemPasswordChar ? "○" : "●";
            };
            return b;
        }

        // ── Helpers sidebar ───────────────────────────────────
        private Button SbNavBtn(string text, int y, Panel sb)
        {
            Button b = new Button();
            b.Text = text; b.Size = new Size(240, 44); b.Location = new Point(0, y);
            b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 255, 255, 255);
            b.BackColor = Color.Transparent; b.ForeColor = Color.FromArgb(210, 235, 245);
            b.Font = new Font("Segoe UI", 10); b.TextAlign = ContentAlignment.MiddleLeft;
            b.Padding = new Padding(26, 0, 0, 0); b.Cursor = Cursors.Hand;
            sb.Controls.Add(b); return b;
        }

        private Panel SbSep(int x, int y, int w)
        {
            Panel s = new Panel();
            s.Size = new Size(w, 1); s.Location = new Point(x, y);
            s.BackColor = Color.FromArgb(55, 255, 255, 255); return s;
        }

        private Panel HLine(int x, int y, int w, Color c)
        {
            return new Panel { Size = new Size(w, 1), Location = new Point(x, y), BackColor = c };
        }

        // ════════════════════════════════════════════════════════
        //  NAVEGACIÓN
        // ════════════════════════════════════════════════════════
        private void ActivarNav(Button b)
        {
            if (_activeNav != null)
            {
                _activeNav.BackColor = Color.Transparent;
                _activeNav.ForeColor = Color.FromArgb(210, 235, 245);
                _activeNav.Font = new Font("Segoe UI", 10);
            }
            b.BackColor = C_NAV_ACT;
            b.ForeColor = Color.White;
            b.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _activeNav = b;
        }

        private void LoadPage(Panel p)
        {
            _content.Controls.Clear();
            p.Dock = DockStyle.Fill;
            _content.Controls.Add(p);
        }

        // ════════════════════════════════════════════════════════
        //  HELPERS DB
        // ════════════════════════════════════════════════════════
        private SQLiteConnection Conn() =>
            new SQLiteConnection(ConfigurationManager.ConnectionStrings["KarinDB"].ConnectionString);

        private void LoadUsers(ListView lv)
        {
            lv.Items.Clear();
            try
            {
                using (var con = Conn())
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(
                        "SELECT id_usuario,nombre,rol,estado,pin_acceso,permisos FROM usuario ORDER BY nombre", con))
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var it = new ListViewItem("");
                            it.Tag = (int)(long)r["id_usuario"];
                            it.SubItems.Add(r["nombre"].ToString());
                            it.SubItems.Add(r["rol"].ToString());
                            it.SubItems.Add(r["estado"].ToString() == "1" ? "Activo" : "Inactivo");
                            it.SubItems.Add(r["pin_acceso"].ToString());
                            it.SubItems.Add(r["permisos"] == DBNull.Value ? "" : r["permisos"].ToString());
                            lv.Items.Add(it);
                        }
                    }
                }
            }
            catch { }
        }

        private void LoadCmb(ComboBox cmb)
        {
            try
            {
                using (var con = Conn())
                {
                    con.Open();
                    using (var cmd = new SQLiteCommand(
                        "SELECT id_usuario, nombre||' — '||rol AS d FROM usuario WHERE estado=1 ORDER BY nombre", con))
                    using (var r = cmd.ExecuteReader())
                    {
                        var t = new System.Data.DataTable();
                        t.Columns.Add("id_usuario", typeof(int));
                        t.Columns.Add("d", typeof(string));
                        while (r.Read()) t.Rows.Add(r["id_usuario"], r["d"]);
                        cmb.DataSource = t; cmb.DisplayMember = "d"; cmb.ValueMember = "id_usuario";
                    }
                }
            }
            catch { }
        }

        private int GetId(ListView lv)
        {
            if (lv.SelectedItems.Count == 0) return 0;
            object tag = lv.SelectedItems[0].Tag;
            return tag != null ? (int)tag : 0;
        }

        private string GetPerms(Dictionary<string, CheckBox> d)
        {
            var l = new List<string>();
            foreach (var kv in d) if (kv.Value.Checked) l.Add(kv.Key);
            return string.Join(",", l);
        }

        // ════════════════════════════════════════════════════════
        //  HELPERS GFX
        // ════════════════════════════════════════════════════════
        [System.Runtime.InteropServices.DllImport("Gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int l, int t, int r, int b, int w, int h);

        private GraphicsPath RoundedPath(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(r.Right - radius * 2, r.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(r.Right - radius * 2, r.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(r.X, r.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure(); return path;
        }

        private void DoubleBufferPanel(Panel p) =>
            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, p, new object[] { true });

        // ════════════════════════════════════════════════════════
        //  TOAST
        // ════════════════════════════════════════════════════════
        private void Toast(string msg, bool ok)
        {
            Form toast = new Form();
            toast.FormBorderStyle = FormBorderStyle.None;
            toast.Size = new Size(330, 52); toast.BackColor = ok ? C_SUCCESS : C_DANGER;
            toast.Opacity = 0.96; toast.TopMost = true; toast.ShowInTaskbar = false;
            toast.StartPosition = FormStartPosition.Manual;
            toast.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, 330, 52, 10, 10));
            toast.Location = new Point(this.Left + this.Width - 348, this.Top + this.Height - 68);

            Label l = new Label();
            l.Text = (ok ? "✓  " : "✕  ") + msg;
            l.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            l.ForeColor = Color.White; l.Dock = DockStyle.Fill;
            l.TextAlign = ContentAlignment.MiddleCenter;
            toast.Controls.Add(l); toast.Show(this);

            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Interval = 2800;
            t.Tick += (s, e) => { t.Stop(); toast.Close(); };
            t.Start();
        }
    }
}