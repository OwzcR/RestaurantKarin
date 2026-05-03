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
        // ── Paleta reducida ──
        private readonly Color C_BG = Color.FromArgb(248, 249, 251);
        private readonly Color C_SURFACE = Color.White;
        private readonly Color C_BORDER = Color.FromArgb(226, 232, 240);
        private readonly Color C_TEXT1 = Color.FromArgb(15, 23, 42);
        private readonly Color C_TEXT2 = Color.FromArgb(100, 116, 139);
        private readonly Color C_BRAND = Color.FromArgb(29, 78, 137);
        private readonly Color C_ACCENT = Color.FromArgb(64, 196, 204);
        private readonly Color C_PRIMARY = Color.FromArgb(37, 99, 235);
        private readonly Color C_DANGER = Color.FromArgb(220, 38, 38);
        private readonly Color C_SUCCESS = Color.FromArgb(22, 163, 74);

        private readonly string[] MODULOS = { "Pedidos", "Cuentas", "Inventario", "Recetas", "Reportes" };

        private Panel _content;
        private Button _activeNav;

        public FormConfiguracion()
        {
            InitializeComponent();
            BuildShell();
        }

        // ════════════════════════════════════════════════════════
        //  SHELL
        // ════════════════════════════════════════════════════════
        private void BuildShell()
        {
            this.Text = "Configuración";
            this.Size = new Size(1080, 720);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = C_BG;
            this.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, Width, Height, 12, 12));

            // ── Sidebar ──────────────────────────────────────
            Panel sb = new Panel();
            sb.Size = new Size(230, 720); sb.Location = Point.Empty;
            DoubleBufferPanel(sb);
            sb.Paint += (s, e) =>
            {
                using (var br = new LinearGradientBrush(sb.ClientRectangle, C_ACCENT, C_BRAND, 90F))
                    e.Graphics.FillRectangle(br, sb.ClientRectangle);
            };
            this.Controls.Add(sb);

            // Icono
            PictureBox ico = new PictureBox();
            ico.Size = new Size(48, 48); ico.Location = new Point(91, 32);
            ico.SizeMode = PictureBoxSizeMode.Zoom; ico.BackColor = Color.Transparent;
            try { ico.Image = Image.FromFile(Path.Combine(Application.StartupPath, "Imgs", "icono.ico")); } catch { }
            sb.Controls.Add(ico);

            // App name
            sb.Controls.Add(MkLbl("CONFIGURACIÓN", new Font("Segoe UI", 10, FontStyle.Bold),
                Color.White, 0, 88, 230, 22, ContentAlignment.MiddleCenter));

            // User chip
            sb.Controls.Add(MkLbl("● " + Sesion.Nombre, new Font("Segoe UI", 8, FontStyle.Bold),
                Color.FromArgb(200, 235, 245), 0, 116, 230, 22, ContentAlignment.MiddleCenter));

            sb.Controls.Add(MkHRule(20, 152, 190, Color.FromArgb(50, 255, 255, 255)));
            sb.Controls.Add(MkLbl("MENÚ", new Font("Segoe UI", 7, FontStyle.Bold),
                Color.FromArgb(160, 220, 235), 0, 164, 230, 18, ContentAlignment.MiddleCenter));

            Button nU = MkNavBtn("Usuarios", 190, sb);
            Button nP = MkNavBtn("Cambiar PIN", 238, sb);

            sb.Controls.Add(MkHRule(20, 632, 190, Color.FromArgb(50, 255, 255, 255)));

            Button btnX = MkFlatBtn("Cerrar", new Font("Segoe UI", 9),
                Color.FromArgb(180, 210, 220), Color.Transparent, 20, 642, 190, 42, sb);
            btnX.TextAlign = ContentAlignment.MiddleLeft;
            btnX.Padding = new Padding(10, 0, 0, 0);
            btnX.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, C_DANGER);
            btnX.Click += (s, e) => this.Close();

            // ── Topbar ───────────────────────────────────────
            Panel top = new Panel();
            top.Size = new Size(850, 52); top.Location = new Point(230, 0);
            top.BackColor = C_SURFACE;
            this.Controls.Add(top); top.BringToFront();

            top.Controls.Add(MkLbl("Panel de administración",
                new Font("Segoe UI", 12, FontStyle.Bold), C_TEXT1,
                24, 0, 500, 52, ContentAlignment.MiddleLeft));

            top.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(C_BORDER), 0, 51, 850, 51);

            // Arrastre
            bool drag = false; Point dp = Point.Empty;
            top.MouseDown += (s, e) => { drag = true; dp = e.Location; };
            top.MouseMove += (s, e) =>
            {
                if (drag) this.Location = new Point(
                    this.Location.X + e.X - dp.X, this.Location.Y + e.Y - dp.Y);
            };
            top.MouseUp += (s, e) => drag = false;

            // ── Área contenido ───────────────────────────────
            _content = new Panel();
            _content.Size = new Size(850, 668); _content.Location = new Point(230, 52);
            _content.BackColor = C_BG;
            this.Controls.Add(_content);

            nU.Click += (s, e) => { ActivarNav(nU); CargarSeccion(PageUsuarios()); };
            nP.Click += (s, e) => { ActivarNav(nP); CargarSeccion(PagePin()); };

            ActivarNav(nU);
            CargarSeccion(PageUsuarios());
        }

        // ════════════════════════════════════════════════════════
        //  PÁGINA: USUARIOS
        // ════════════════════════════════════════════════════════
        private Panel PageUsuarios()
        {
            Panel page = new Panel(); page.BackColor = C_BG;

            PageHeader(page, "Usuarios", "Administra los accesos al sistema");

            // ── Tarjeta tabla ─────────────────────────────────
            Panel cT = MkCard(24, 86, 800, 210, page);
            SecLabel("Usuarios registrados", cT, 20, 16);

            ListView lv = MkListView(cT, 20, 44, 760, 152);
            lv.Columns.Add("", 28);
            lv.Columns.Add("Nombre", 178);
            lv.Columns.Add("Rol", 90);
            lv.Columns.Add("Estado", 78);
            lv.Columns.Add("PIN", 65);
            lv.Columns.Add("Pantallas permitidas", 279);
            LoadUsers(lv);

            // ── Tarjeta agregar ───────────────────────────────
            Panel cA = MkCard(24, 310, 800, 188, page);
            SecLabel("Agregar usuario", cA, 20, 16);

            FldLabel("Nombre", cA, 20, 50);
            TextBox tNom = MkInput(cA, 20, 68, 224);

            FldLabel("Rol", cA, 260, 50);
            ComboBox cRol = MkCombo(cA, 260, 68, 148);
            cRol.Items.AddRange(new object[] { "Mesero", "Admin" });
            cRol.SelectedIndex = 0;

            FldLabel("PIN", cA, 424, 50);
            TextBox tPin = MkInput(cA, 424, 68, 112, true); tPin.MaxLength = 4;

            FldLabel("Pantallas permitidas", cA, 20, 112);
            Dictionary<string, CheckBox> cks = MkPermChecks(cA, 20, 130);

            cRol.SelectedIndexChanged += (s, e) =>
            {
                bool adm = cRol.SelectedItem.ToString() == "Admin";
                foreach (var cb in cks.Values) { cb.Checked = true; cb.Enabled = !adm; }
            };

            Button btnAdd = MkPrimary("Agregar", cA, 660, 66, 120, 38);
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
                            cmd.Parameters.AddWithValue("@n", nom);
                            cmd.Parameters.AddWithValue("@r", rol);
                            cmd.Parameters.AddWithValue("@p", pin);
                            cmd.Parameters.AddWithValue("@pe", perms);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    Toast("Usuario agregado", true);
                    tNom.Clear(); tPin.Clear(); LoadUsers(lv);
                }
                catch (Exception ex) { Toast("Error: " + ex.Message, false); }
            };

            // ── Tarjeta editar permisos ───────────────────────
            Panel cE = MkCard(24, 512, 800, 142, page);
            SecLabel("Permisos del usuario seleccionado", cE, 20, 16);

            FldLabel("Pantallas habilitadas", cE, 20, 50);
            Dictionary<string, CheckBox> cksE = MkPermChecks(cE, 20, 68);

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

            Button btnPerm = MkSecondary("Guardar permisos", cE, 530, 60, 168, 38);
            Button btnDel = MkDanger("Eliminar", cE, 710, 60, 102, 38);

            btnPerm.Click += (s, e) =>
            {
                if (lv.SelectedItems.Count == 0) { Toast("Selecciona un usuario", false); return; }
                string rol2 = lv.SelectedItems[0].SubItems[2].Text;
                string perms = rol2 == "Admin" ? string.Join(",", MODULOS) : GetPerms(cksE);
                int id = GetId(lv);
                try
                {
                    using (var con = Conn())
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand(
                            "UPDATE usuario SET permisos=@pe WHERE id_usuario=@id", con))
                        {
                            cmd.Parameters.AddWithValue("@pe", perms);
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    Toast("Permisos guardados", true); LoadUsers(lv);
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
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    Toast("Usuario eliminado", true); LoadUsers(lv);
                }
                catch (Exception ex) { Toast("Error: " + ex.Message, false); }
            };

            return page;
        }

        // ════════════════════════════════════════════════════════
        //  PÁGINA: CAMBIAR PIN
        // ════════════════════════════════════════════════════════
        private Panel PagePin()
        {
            Panel page = new Panel(); page.BackColor = C_BG;
            PageHeader(page, "Cambiar PIN", "Reasigna el PIN de acceso de cualquier usuario");

            Panel card = MkCard(24, 86, 800, 450, page);
            SecLabel("Selecciona usuario", card, 20, 16);

            FldLabel("Usuario", card, 20, 52);
            ComboBox cmb = MkCombo(card, 20, 70, 490);
            LoadCmb(cmb);

            Button btnRef = new Button();
            btnRef.Text = "↺"; btnRef.Size = new Size(40, 36); btnRef.Location = new Point(518, 70);
            btnRef.FlatStyle = FlatStyle.Flat; btnRef.FlatAppearance.BorderSize = 1;
            btnRef.FlatAppearance.BorderColor = C_BORDER; btnRef.BackColor = C_BG;
            btnRef.ForeColor = C_TEXT2; btnRef.Cursor = Cursors.Hand;
            btnRef.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            btnRef.Click += (s, e) => LoadCmb(cmb);
            card.Controls.Add(btnRef);

            card.Controls.Add(MkHRule(20, 126, 760, C_BORDER));

            // Nuevo PIN
            FldLabel("Nuevo PIN", card, 20, 146);
            TextBox tN = MkInput(card, 20, 164, 290, true);
            tN.MaxLength = 4; tN.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            Button oj1 = MkEyeBtn(tN); oj1.Location = new Point(318, 167); card.Controls.Add(oj1);

            // Barra progreso
            Panel barBg = new Panel();
            barBg.Size = new Size(290, 4); barBg.Location = new Point(20, 212);
            barBg.BackColor = C_BORDER; card.Controls.Add(barBg);
            Panel barFg = new Panel();
            barFg.Size = new Size(0, 4); barFg.Location = Point.Empty;
            barFg.BackColor = C_PRIMARY; barBg.Controls.Add(barFg);

            Label lSeg = new Label();
            lSeg.Font = new Font("Segoe UI", 8, FontStyle.Italic);
            lSeg.Location = new Point(20, 222); lSeg.Size = new Size(380, 18);
            lSeg.ForeColor = C_TEXT2; card.Controls.Add(lSeg);

            tN.TextChanged += (s, e) =>
            {
                int n = tN.Text.Length;
                barFg.Width = (290 / 4) * n;
                if (n == 0) { lSeg.Text = ""; barFg.Width = 0; return; }
                if (n < 4) { lSeg.Text = $"Faltan {4 - n} dígito(s)"; lSeg.ForeColor = C_DANGER; barFg.BackColor = C_DANGER; }
                else { lSeg.Text = "PIN completo"; lSeg.ForeColor = C_SUCCESS; barFg.BackColor = C_SUCCESS; }
            };

            // Confirmar PIN
            FldLabel("Confirmar PIN", card, 20, 256);
            TextBox tC = MkInput(card, 20, 274, 290, true);
            tC.MaxLength = 4; tC.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            Button oj2 = MkEyeBtn(tC); oj2.Location = new Point(318, 277); card.Controls.Add(oj2);

            Label lMatch = new Label();
            lMatch.Font = new Font("Segoe UI", 8, FontStyle.Italic);
            lMatch.Location = new Point(20, 318); lMatch.Size = new Size(380, 18);
            lMatch.ForeColor = C_TEXT2; card.Controls.Add(lMatch);

            tC.TextChanged += (s, e) =>
            {
                if (tC.Text.Length == 0) { lMatch.Text = ""; return; }
                bool ok = tN.Text == tC.Text;
                lMatch.Text = ok ? "Los PINs coinciden" : "Los PINs no coinciden";
                lMatch.ForeColor = ok ? C_SUCCESS : C_DANGER;
            };

            card.Controls.Add(MkHRule(20, 354, 760, C_BORDER));

            Button btnSave = MkPrimary("Actualizar PIN", card, 20, 372, 180, 46);
            Button btnCnc = MkSecondary("Cancelar", card, 214, 372, 120, 46);

            btnCnc.Click += (s, e) =>
            {
                tN.Clear(); tC.Clear();
                lSeg.Text = ""; lMatch.Text = "";
                barFg.Width = 0; barFg.BackColor = C_PRIMARY;
            };

            btnSave.Click += (s, e) =>
            {
                if (cmb.SelectedItem == null) { Toast("Selecciona un usuario", false); return; }
                string nv = tN.Text.Trim(), cf = tC.Text.Trim();
                if (nv == "" || cf == "") { Toast("Completa los dos campos", false); return; }
                if (nv != cf) { Toast("Los PINs no coinciden", false); return; }
                if (nv.Length != 4) { Toast("El PIN debe tener 4 dígitos", false); return; }
                if (MessageBox.Show("¿Actualizar el PIN de este usuario?", "Confirmar",
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
                            cmd.Parameters.AddWithValue("@p", nv);
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    Toast("PIN actualizado correctamente", true);
                    tN.Clear(); tC.Clear();
                    lSeg.Text = ""; lMatch.Text = "";
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
            page.Controls.Add(MkLbl(titulo, new Font("Segoe UI", 18, FontStyle.Bold),
                C_TEXT1, 24, 18, 790, 34, ContentAlignment.MiddleLeft));
            page.Controls.Add(MkLbl(sub, new Font("Segoe UI", 10),
                C_TEXT2, 24, 54, 790, 22, ContentAlignment.MiddleLeft));
            Panel acc = new Panel();
            acc.Size = new Size(42, 3); acc.Location = new Point(24, 78);
            acc.BackColor = C_ACCENT; page.Controls.Add(acc);
        }

        private void SecLabel(string text, Panel card, int x, int y)
        {
            card.Controls.Add(MkLbl(text, new Font("Segoe UI", 10, FontStyle.Bold),
                C_TEXT1, x, y, 500, 22, ContentAlignment.MiddleLeft));
        }

        private Label FldLabel(string text, Panel p, int x, int y)
        {
            Label l = MkLbl(text, new Font("Segoe UI", 8, FontStyle.Bold),
                C_TEXT2, x, y, 200, 16, ContentAlignment.MiddleLeft);
            p.Controls.Add(l); return l;
        }

        private Panel MkCard(int x, int y, int w, int h, Panel parent)
        {
            Panel c = new Panel();
            c.Size = new Size(w, h); c.Location = new Point(x, y);
            c.BackColor = C_SURFACE;
            c.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(C_BORDER), 0, 0, c.Width - 1, c.Height - 1);
            parent.Controls.Add(c); return c;
        }

        private TextBox MkInput(Panel p, int x, int y, int w, bool pwd = false)
        {
            TextBox t = new TextBox();
            t.Size = new Size(w, 36); t.Location = new Point(x, y);
            t.Font = new Font("Segoe UI", 11); t.BackColor = C_BG;
            t.BorderStyle = BorderStyle.FixedSingle; t.UseSystemPasswordChar = pwd;
            t.Enter += (s, e) => t.BackColor = Color.FromArgb(239, 246, 255);
            t.Leave += (s, e) => t.BackColor = C_BG;
            p.Controls.Add(t); return t;
        }

        private ComboBox MkCombo(Panel p, int x, int y, int w)
        {
            ComboBox c = new ComboBox();
            c.Size = new Size(w, 36); c.Location = new Point(x, y);
            c.Font = new Font("Segoe UI", 11); c.BackColor = C_BG;
            c.DropDownStyle = ComboBoxStyle.DropDownList; c.FlatStyle = FlatStyle.Flat;
            p.Controls.Add(c); return c;
        }

        private ListView MkListView(Panel p, int x, int y, int w, int h)
        {
            ListView lv = new ListView();
            lv.Size = new Size(w, h); lv.Location = new Point(x, y);
            lv.View = View.Details; lv.FullRowSelect = true;
            lv.GridLines = false; lv.BorderStyle = BorderStyle.None;
            lv.Font = new Font("Segoe UI", 10); lv.BackColor = C_SURFACE;
            lv.OwnerDraw = true;

            lv.DrawColumnHeader += (s, e) =>
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(248, 250, 252)), e.Bounds);
                e.Graphics.DrawLine(new Pen(C_BORDER),
                    e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
                if (e.Header.Index == 0) return;
                using (StringFormat sf = new StringFormat { LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString(e.Header.Text,
                        new Font("Segoe UI", 8, FontStyle.Bold), new SolidBrush(C_TEXT2),
                        new RectangleF(e.Bounds.X + 8, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height), sf);
            };

            lv.DrawItem += (s, e) => e.DrawDefault = true;

            lv.DrawSubItem += (s, e) =>
            {
                Color bg = e.Item.Selected
                    ? Color.FromArgb(219, 234, 254)
                    : e.ItemIndex % 2 == 0 ? C_SURFACE : Color.FromArgb(249, 250, 251);
                e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);
                e.Graphics.DrawLine(new Pen(Color.FromArgb(241, 245, 249)),
                    e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);

                if (e.ColumnIndex == 0)
                {
                    // Punto de color: estado Activo=verde, Inactivo=rojo
                    bool activo = e.Item.SubItems.Count > 3 &&
                                  e.Item.SubItems[3].Text == "Activo";
                    Color dot = activo ? C_SUCCESS : C_DANGER;
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillEllipse(new SolidBrush(dot),
                        e.Bounds.X + 8, e.Bounds.Y + 8, 10, 10);
                    return;
                }

                using (StringFormat sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                })
                    e.Graphics.DrawString(e.SubItem.Text,
                        new Font("Segoe UI", 10), new SolidBrush(C_TEXT1),
                        new RectangleF(e.Bounds.X + 8, e.Bounds.Y, e.Bounds.Width - 10, e.Bounds.Height), sf);
            };

            p.Controls.Add(lv); return lv;
        }

        private Dictionary<string, CheckBox> MkPermChecks(Panel p, int x, int y)
        {
            var d = new Dictionary<string, CheckBox>(); int cx = x;
            foreach (string m in MODULOS)
            {
                CheckBox cb = new CheckBox();
                cb.Text = m; cb.Font = new Font("Segoe UI", 9);
                cb.ForeColor = C_TEXT1; cb.Size = new Size(124, 24);
                cb.Location = new Point(cx, y); cb.Checked = true;
                cb.FlatStyle = FlatStyle.Flat;
                cb.FlatAppearance.BorderColor = C_BORDER;
                p.Controls.Add(cb); d[m] = cb; cx += 130;
            }
            return d;
        }

        private Button MkPrimary(string t, Panel p, int x, int y, int w, int h)
        {
            Button b = new Button();
            b.Text = t; b.Size = new Size(w, h); b.Location = new Point(x, y);
            b.BackColor = C_PRIMARY; b.ForeColor = Color.White;
            b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 0;
            b.Font = new Font("Segoe UI", 10, FontStyle.Bold); b.Cursor = Cursors.Hand;
            b.MouseEnter += (s, e) => b.BackColor = Color.FromArgb(29, 78, 216);
            b.MouseLeave += (s, e) => b.BackColor = C_PRIMARY;
            p.Controls.Add(b); return b;
        }

        private Button MkSecondary(string t, Panel p, int x, int y, int w, int h)
        {
            Button b = new Button();
            b.Text = t; b.Size = new Size(w, h); b.Location = new Point(x, y);
            b.BackColor = C_SURFACE; b.ForeColor = C_TEXT1;
            b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = C_BORDER;
            b.Font = new Font("Segoe UI", 10); b.Cursor = Cursors.Hand;
            b.MouseEnter += (s, e) => b.BackColor = C_BG;
            b.MouseLeave += (s, e) => b.BackColor = C_SURFACE;
            p.Controls.Add(b); return b;
        }

        private Button MkDanger(string t, Panel p, int x, int y, int w, int h)
        {
            Button b = new Button();
            b.Text = t; b.Size = new Size(w, h); b.Location = new Point(x, y);
            b.BackColor = Color.FromArgb(254, 242, 242); b.ForeColor = C_DANGER;
            b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = Color.FromArgb(254, 202, 202);
            b.Font = new Font("Segoe UI", 10); b.Cursor = Cursors.Hand;
            b.MouseEnter += (s, e) => b.BackColor = Color.FromArgb(254, 226, 226);
            b.MouseLeave += (s, e) => b.BackColor = Color.FromArgb(254, 242, 242);
            p.Controls.Add(b); return b;
        }

        private Button MkNavBtn(string text, int y, Panel sb)
        {
            Button b = new Button();
            b.Text = text; b.Size = new Size(230, 44); b.Location = new Point(0, y);
            b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 255, 255, 255);
            b.BackColor = Color.Transparent; b.ForeColor = Color.FromArgb(210, 235, 245);
            b.Font = new Font("Segoe UI", 10); b.TextAlign = ContentAlignment.MiddleLeft;
            b.Padding = new Padding(24, 0, 0, 0); b.Cursor = Cursors.Hand;
            sb.Controls.Add(b); return b;
        }

        private Button MkFlatBtn(string t, Font f, Color fg, Color bg,
            int x, int y, int w, int h, Panel p)
        {
            Button b = new Button();
            b.Text = t; b.Font = f; b.Size = new Size(w, h); b.Location = new Point(x, y);
            b.BackColor = bg; b.ForeColor = fg; b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0; b.Cursor = Cursors.Hand;
            p.Controls.Add(b); return b;
        }

        private Button MkEyeBtn(TextBox txt)
        {
            Button b = new Button();
            b.Text = "○"; b.Size = new Size(36, 34);
            b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = C_BORDER; b.BackColor = C_BG;
            b.ForeColor = C_TEXT2; b.Font = new Font("Segoe UI", 10); b.Cursor = Cursors.Hand;
            b.Click += (s, e) =>
            {
                txt.UseSystemPasswordChar = !txt.UseSystemPasswordChar;
                b.Text = txt.UseSystemPasswordChar ? "○" : "●";
            };
            return b;
        }

        private Label MkLbl(string t, Font f, Color fg,
            int x, int y, int w, int h, ContentAlignment a)
        {
            return new Label
            {
                Text = t,
                Font = f,
                ForeColor = fg,
                Location = new Point(x, y),
                Size = new Size(w, h),
                TextAlign = a
            };
        }

        private Panel MkHRule(int x, int y, int w, Color c)
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
            b.BackColor = Color.FromArgb(40, 255, 255, 255);
            b.ForeColor = Color.White;
            b.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _activeNav = b;
        }

        private void CargarSeccion(Panel p)
        {
            _content.Controls.Clear();
            p.Dock = DockStyle.Fill;
            _content.Controls.Add(p);
        }

        // ════════════════════════════════════════════════════════
        //  DB HELPERS
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
                            var it = new ListViewItem(""); // col dot
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
        //  GFXHELPERS
        // ════════════════════════════════════════════════════════
        [System.Runtime.InteropServices.DllImport("Gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int l, int t, int r, int b, int w, int h);

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
            toast.Size = new Size(320, 48); toast.BackColor = ok ? C_SUCCESS : C_DANGER;
            toast.Opacity = 0.97; toast.TopMost = true; toast.ShowInTaskbar = false;
            toast.StartPosition = FormStartPosition.Manual;
            toast.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, 320, 48, 8, 8));
            toast.Location = new Point(this.Left + this.Width - 336, this.Top + this.Height - 64);

            Label l = new Label();
            l.Text = (ok ? "✓  " : "✕  ") + msg;
            l.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            l.ForeColor = Color.White; l.Dock = DockStyle.Fill;
            l.TextAlign = ContentAlignment.MiddleCenter;
            toast.Controls.Add(l); toast.Show(this);

            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Interval = 2600;
            t.Tick += (s, e) => { t.Stop(); toast.Close(); };
            t.Start();
        }
    }
}