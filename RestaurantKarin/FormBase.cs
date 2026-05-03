using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public class Base : Form
    {
        private Panel PanelMenu;
        private Panel PanelContenedor;
        private Button BtnToggleMenu;
        private Button btnLogOut;
        private List<Button> menuButtons = new List<Button>();

        private System.Windows.Forms.Timer animacionTimer;
        private bool isMenuExpanded = true;
        private bool isAnimating = false;
        private const int ExpandedWidth = 220;
        private const int CollapsedWidth = 70;
        private const int AnimationSpeed = 15;

        public Base()
        {
            this.DoubleBuffered = true;
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Sistema de Gestión — Restaurante";
            this.MinimumSize = new Size(1280, 850);
            this.WindowState = FormWindowState.Maximized;
            this.AutoScaleMode = AutoScaleMode.None;
            try { this.Icon = new Icon(Path.Combine(Application.StartupPath, "Imgs", "icono.ico")); } catch { }

            animacionTimer = new System.Windows.Forms.Timer();
            animacionTimer.Interval = 10;
            animacionTimer.Tick += AnimacionTimer_Tick;

            PanelContenedor = new Panel();
            PanelContenedor.Dock = DockStyle.Fill;
            PanelContenedor.BackColor = Color.White;
            try
            {
                PanelContenedor.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, "Imgs", "fondo.png"));
                PanelContenedor.BackgroundImageLayout = ImageLayout.Stretch;
            }
            catch { }
            PanelContenedor.Paint += (s, e) =>
            {
                using (var br = new SolidBrush(Color.FromArgb(150, 13, 41, 78)))
                    e.Graphics.FillRectangle(br, PanelContenedor.ClientRectangle);
            };

            PanelMenu = new Panel();
            PanelMenu.Dock = DockStyle.Left;
            PanelMenu.Width = ExpandedWidth;
            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, PanelMenu, new object[] { true });
            PanelMenu.Paint += (s, e) =>
            {
                using (var br = new LinearGradientBrush(PanelMenu.ClientRectangle,
                    Color.FromArgb(64, 196, 204), Color.FromArgb(13, 41, 78), 90F))
                    e.Graphics.FillRectangle(br, PanelMenu.ClientRectangle);
            };
            PanelMenu.Resize += (s, e) =>
            {
                PanelMenu.Invalidate();
                if (btnLogOut != null) { btnLogOut.Location = new Point(0, PanelMenu.Height - 60); btnLogOut.Width = PanelMenu.Width; }
            };

            BtnToggleMenu = new Button();
            BtnToggleMenu.Text = "◄";
            BtnToggleMenu.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            BtnToggleMenu.ForeColor = Color.White;
            BtnToggleMenu.BackColor = Color.Transparent;
            BtnToggleMenu.FlatStyle = FlatStyle.Flat;
            BtnToggleMenu.FlatAppearance.BorderSize = 0;
            BtnToggleMenu.Size = new Size(40, 40);
            BtnToggleMenu.Location = new Point(ExpandedWidth - 45, 10);
            BtnToggleMenu.Cursor = Cursors.Hand;
            BtnToggleMenu.Click += (s, e) => { if (!isAnimating) { isAnimating = true; animacionTimer.Start(); } };
            PanelMenu.Controls.Add(BtnToggleMenu);

            int startY = 80, spacing = 65;

            // Pedidos
            if (Sesion.TienePermiso("Pedidos"))
                menuButtons.Add(CrearBotonMenu("Pedidos", "pedidos.png", startY + spacing * 0));

            // Cuentas
            if (Sesion.TienePermiso("Cuentas"))
            {
                Button btnCuentas = CrearBotonMenu("Cuentas", "cuentas.png", startY + spacing * ContarBotones());
                btnCuentas.Click += (s, e) =>
                {
                    PanelContenedor.Controls.Clear();
                    FormCuentas frm = new FormCuentas();
                    frm.TopLevel = false;
                    frm.FormBorderStyle = FormBorderStyle.None;
                    frm.Dock = DockStyle.Fill;
                    PanelContenedor.Controls.Add(frm);
                    frm.Show();
                };
                menuButtons.Add(btnCuentas);
            }

            // Inventario
            if (Sesion.TienePermiso("Inventario"))
            {
                Button btnInv = CrearBotonMenu("Inventario", "inventario.png", startY + spacing * ContarBotones());
                btnInv.Click += (s, e) =>
                {
                    PanelContenedor.Controls.Clear();
                    FormInventario frm = new FormInventario();
                    frm.TopLevel = false;
                    frm.Dock = DockStyle.Fill;
                    PanelContenedor.Controls.Add(frm);
                    frm.Show();
                };
                menuButtons.Add(btnInv);
            }

            // Recetas
            if (Sesion.TienePermiso("Recetas"))
            {
                Button btnRecetas = CrearBotonMenu("Recetas", "recetas.png", startY + spacing * ContarBotones());
                btnRecetas.Click += (_, _) => CargarModuloRecetas();
                menuButtons.Add(btnRecetas);
            }

            // Reportes
            if (Sesion.TienePermiso("Reportes"))
                menuButtons.Add(CrearBotonMenu("Reportes", "reportes.png", startY + spacing * ContarBotones()));

            // Ajustes — solo admin
            if (Sesion.EsAdmin)
            {
                Button btnAjustes = CrearBotonMenu("Ajustes", "configuration.png", startY + spacing * ContarBotones());
                btnAjustes.Click += (s, e) =>
                {
                    FormConfiguracion frmConfig = new FormConfiguracion();
                    frmConfig.ShowDialog();
                };
                menuButtons.Add(btnAjustes);
            }

            btnLogOut = CrearBotonMenu("Salir", "logout.png", 0);
            btnLogOut.Click += BtnLogOut_Click;
            menuButtons.Add(btnLogOut);

            this.Controls.Add(PanelContenedor);
            this.Controls.Add(PanelMenu);
        }

        private int ContarBotones() => menuButtons.Count;

        private Button CrearBotonMenu(string texto, string nombreIcono, int posY)
        {
            Button btn = new Button();
            btn.Text = "   " + texto;
            btn.Size = new Size(PanelMenu.Width, 50);
            btn.Location = new Point(0, posY);
            btn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.ForeColor = Color.White;
            btn.BackColor = Color.Transparent;
            btn.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.ImageAlign = ContentAlignment.MiddleLeft;
            btn.TextImageRelation = TextImageRelation.ImageBeforeText;
            btn.Cursor = Cursors.Hand;
            btn.Tag = texto;
            btn.Padding = new Padding(15, 0, 0, 0);
            try { btn.Image = Image.FromFile(Path.Combine(Application.StartupPath, "Imgs", nombreIcono)); } catch { }
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(50, 255, 255, 255);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.Transparent;
            PanelMenu.Controls.Add(btn);
            return btn;
        }

        private void AnimacionTimer_Tick(object sender, EventArgs e)
        {
            if (isMenuExpanded)
            {
                if (PanelMenu.Width == ExpandedWidth)
                {
                    BtnToggleMenu.Text = "►";
                    foreach (var btn in menuButtons) { btn.Text = ""; btn.ImageAlign = ContentAlignment.MiddleCenter; btn.Padding = new Padding(0); }
                }
                PanelMenu.Width -= AnimationSpeed;
                BtnToggleMenu.Location = new Point((PanelMenu.Width - 40) / 2, 10);
                if (PanelMenu.Width <= CollapsedWidth) { PanelMenu.Width = CollapsedWidth; isMenuExpanded = false; isAnimating = false; animacionTimer.Stop(); }
            }
            else
            {
                PanelMenu.Width += AnimationSpeed;
                int posX = PanelMenu.Width - 45;
                BtnToggleMenu.Location = new Point(posX > 0 ? posX : 0, 10);
                if (PanelMenu.Width >= ExpandedWidth)
                {
                    PanelMenu.Width = ExpandedWidth;
                    BtnToggleMenu.Text = "◄";
                    BtnToggleMenu.Location = new Point(ExpandedWidth - 45, 10);
                    foreach (var btn in menuButtons) { btn.Text = "   " + btn.Tag.ToString(); btn.ImageAlign = ContentAlignment.MiddleLeft; btn.Padding = new Padding(15, 0, 0, 0); }
                    isMenuExpanded = true; isAnimating = false; animacionTimer.Stop();
                }
            }
        }

        public void CargarModuloRecetas()
        {
            PanelContenedor.Controls.Clear();
            var moduloRecetas = new PantallaRecetas { Dock = DockStyle.Fill };
            PanelContenedor.Controls.Add(moduloRecetas);
        }

        private void BtnLogOut_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Estás seguro de que deseas cerrar sesión?", "Cerrar Sesión",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Sesion.Cerrar();
                FormLogin login = new FormLogin();
                login.Show();
                this.Hide();
            }
        }
    }
}