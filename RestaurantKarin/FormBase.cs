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
            this.Text = "Pedidos";
            this.MinimumSize = new Size(1024, 768);
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
                using (SolidBrush overlayBrush = new SolidBrush(Color.FromArgb(180, 13, 41, 78)))
                {
                    e.Graphics.FillRectangle(overlayBrush, PanelContenedor.ClientRectangle);
                }
            };

            PanelMenu = new Panel();
            PanelMenu.Dock = DockStyle.Left;
            PanelMenu.Width = ExpandedWidth;

            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, PanelMenu, new object[] { true });

            PanelMenu.Paint += (s, e) =>
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    PanelMenu.ClientRectangle,
                    Color.FromArgb(64, 196, 204),
                    Color.FromArgb(13, 41, 78),
                    90F))
                {
                    e.Graphics.FillRectangle(brush, PanelMenu.ClientRectangle);
                }
            };

            PanelMenu.Resize += (s, e) =>
            {
                PanelMenu.Invalidate();
                if (btnLogOut != null)
                {
                    btnLogOut.Location = new Point(0, PanelMenu.Height - 60);
                    btnLogOut.Width = PanelMenu.Width;
                }
            };

            BtnToggleMenu = new Button();
            BtnToggleMenu.Text = "◄";
            BtnToggleMenu.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            BtnToggleMenu.ForeColor = Color.White;
            BtnToggleMenu.BackColor = Color.Transparent;
            BtnToggleMenu.FlatStyle = FlatStyle.Flat;
            BtnToggleMenu.FlatAppearance.BorderSize = 0;
            BtnToggleMenu.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 255, 255, 255);
            BtnToggleMenu.Size = new Size(40, 40);
            BtnToggleMenu.Location = new Point(ExpandedWidth - 45, 10);
            BtnToggleMenu.Cursor = Cursors.Hand;
            BtnToggleMenu.Click += (s, e) =>
            {
                if (!isAnimating)
                {
                    isAnimating = true;
                    animacionTimer.Start();
                }
            };
            PanelMenu.Controls.Add(BtnToggleMenu);

            int startY = 80;
            int spacing = 65;

            menuButtons.Add(CrearBotonMenu("Pedidos", "pedidos.png", startY));
            menuButtons.Add(CrearBotonMenu("Cuentas", "cuentas.png", startY + spacing));
            menuButtons.Add(CrearBotonMenu("Inventario", "inventario.png", startY + spacing * 2));
            // Módulo Recetas: pantallas embebidas en el panel principal (no afecta otras rutas del menú).
            Button btnRecetas = CrearBotonMenu("Recetas", "recetas.png", startY + spacing * 3);
            btnRecetas.Click += (_, _) => CargarModuloRecetas();
            menuButtons.Add(btnRecetas);
            menuButtons.Add(CrearBotonMenu("Reportes", "reportes.png", startY + spacing * 4));

            // Solo Admin ve Ajustes
            if (Sesion.EsAdmin)
            {
                Button btnAjustes = CrearBotonMenu("Ajustes", "configuration.png", startY + spacing * 5);
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
                    foreach (var btn in menuButtons)
                    {
                        btn.Text = "";
                        btn.ImageAlign = ContentAlignment.MiddleCenter;
                        btn.Padding = new Padding(0);
                    }
                }

                PanelMenu.Width -= AnimationSpeed;
                BtnToggleMenu.Location = new Point((PanelMenu.Width - 40) / 2, 10);

                if (PanelMenu.Width <= CollapsedWidth)
                {
                    PanelMenu.Width = CollapsedWidth;
                    isMenuExpanded = false;
                    isAnimating = false;
                    animacionTimer.Stop();
                }
            }
            else
            {
                PanelMenu.Width += AnimationSpeed;

                int nuevaPosX = PanelMenu.Width - 45;
                BtnToggleMenu.Location = new Point(nuevaPosX > 0 ? nuevaPosX : 0, 10);

                if (PanelMenu.Width >= ExpandedWidth)
                {
                    PanelMenu.Width = ExpandedWidth;
                    BtnToggleMenu.Text = "◄";
                    BtnToggleMenu.Location = new Point(ExpandedWidth - 45, 10);

                    foreach (var btn in menuButtons)
                    {
                        btn.Text = "   " + btn.Tag.ToString();
                        btn.ImageAlign = ContentAlignment.MiddleLeft;
                        btn.Padding = new Padding(15, 0, 0, 0);
                    }

                    isMenuExpanded = true;
                    isAnimating = false;
                    animacionTimer.Stop();
                }
            }
        }

        /// <summary>
        /// Inserta el módulo de recetas en el panel de contenido principal (área a la derecha del menú lateral).
        /// Los demás botones del menú permanecen como estaban para no interferir con el trabajo del equipo.
        /// </summary>
        public void CargarModuloRecetas()
        {
            PanelContenedor.Controls.Clear();
            var moduloRecetas = new PantallaRecetas
            {
                Dock = DockStyle.Fill
            };
            PanelContenedor.Controls.Add(moduloRecetas);
        }

        private void BtnLogOut_Click(object sender, EventArgs e)
        {
            var confirmacion = MessageBox.Show("¿Estás seguro de que deseas cerrar sesión?", "Cerrar Sesión",
                                               MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmacion == DialogResult.Yes)
            {
                Sesion.Cerrar();
                FormLogin login = new FormLogin();
                login.Show();
                this.Hide();
            }
        }
    }
}