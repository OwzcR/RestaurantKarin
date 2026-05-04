using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace RestaurantKarin
{
    // ─────────────────────────────────────────────────────────────────────────
    //  Model (renombrado a MesaModel para evitar ambigüedades con otra clase Mesa)
    // ─────────────────────────────────────────────────────────────────────────
    public class MesaModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int CapacidadMax { get; set; }
        public bool Activa { get; set; }
        public int Personas { get; set; }
        public DateTime HoraLlegada { get; set; }
        public decimal Cuenta { get; set; }
        public int PropinaPercent { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Form
    // ─────────────────────────────────────────────────────────────────────────
    public partial class Pedidos : Form
    {
        // ── Controls ─────────────────────────────────────────────────────────
        private Panel PanelMenu = null!;
        private Panel PanelContenedor = null!;
        private Button BtnToggleMenu = null!;
        private Button BtnLogOut = null!;
        private readonly List<Button> _menuButtons = new();
        private FlowLayoutPanel FlowActivas = null!;
        private FlowLayoutPanel FlowDisponibles = null!;
        private Panel ScrollPanel = null!;

        // ── State ─────────────────────────────────────────────────────────────
        private bool _menuExpanded = true;
        private List<MesaModel> _mesas = new();

        // ── Layout constants ──────────────────────────────────────────────────
        private const int MenuExpanded = 200;
        private const int MenuCollapsed = 64;
        private const int CardHeight = 130;
        private const int RowHeight = 46;

        // ── Palette ───────────────────────────────────────────────────────────
        private static readonly Color SidebarTop = Color.FromArgb(64, 188, 216);
        private static readonly Color SidebarBottom = Color.FromArgb(13, 41, 78);
        private static readonly Color MainBg = Color.FromArgb(26, 58, 107);
        private static readonly Color CardWhite = Color.White;
        private static readonly Color AccentTeal = Color.FromArgb(0, 137, 123);
        private static readonly Color AccentSlate = Color.FromArgb(84, 110, 122);
        private static readonly Color AccentDarkNavy = Color.FromArgb(26, 35, 126);
        private static readonly Color AvailableAlpha = Color.FromArgb(38, 255, 255, 255);
        private static readonly Color TextDark = Color.FromArgb(30, 30, 30);

        // ── Shared fonts (created once, reused everywhere) ────────────────────
        private static readonly Font FontNav = new("Segoe UI", 11, FontStyle.Regular);
        private static readonly Font FontBold9 = new("Segoe UI", 9, FontStyle.Bold);
        private static readonly Font FontRegular9 = new("Segoe UI", 9, FontStyle.Regular);
        private static readonly Font FontBold13 = new("Segoe UI", 13, FontStyle.Bold);
        private static readonly Font FontBold10 = new("Segoe UI", 10, FontStyle.Bold);
        private static readonly Font FontBold8 = new("Segoe UI", 8, FontStyle.Bold);

        // ─────────────────────────────────────────────────────────────────────
        public Pedidos()
        {
            this.DoubleBuffered = true;
            InitializeMesaData();
            SetupUI();
        }

        // ── Sample / seed data ────────────────────────────────────────────────
        private void InitializeMesaData()
        {
            _mesas = new List<MesaModel>
            {
                new() { Id=5, Nombre="MESA : 5", CapacidadMax=4, Activa=true,  Personas=2, HoraLlegada=DateTime.Now.AddMinutes(-30), Cuenta=264, PropinaPercent=10 },
                new() { Id=3, Nombre="MESA : 3", CapacidadMax=4, Activa=true,  Personas=2, HoraLlegada=DateTime.Now.AddHours(-1),    Cuenta=264, PropinaPercent=10 },
                new() { Id=1, Nombre="MESA : 1", CapacidadMax=2, Activa=false },
                new() { Id=2, Nombre="MESA : 2", CapacidadMax=4, Activa=false },
                new() { Id=4, Nombre="MESA : 4", CapacidadMax=5, Activa=false },
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Top-level UI assembly
        // ─────────────────────────────────────────────────────────────────────
        private void SetupUI()
        {
            Text = "Pedidos — Restaurante Karin";
            MinimumSize = new Size(1024, 768);
            WindowState = FormWindowState.Maximized;
            AutoScaleMode = AutoScaleMode.None;
            BackColor = MainBg;

            try { Icon = new Icon(Path.Combine(Application.StartupPath, "Imgs", "icono.ico")); } catch { }

            BuildSidebar();
            BuildContentPanel();

            // Sidebar renders on top of content
            Controls.Add(PanelContenedor);
            Controls.Add(PanelMenu);

            BuildMesasUI();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Sidebar
        // ─────────────────────────────────────────────────────────────────────
        private void BuildSidebar()
        {
            PanelMenu = new Panel { Dock = DockStyle.Left, Width = MenuExpanded };
            EnableDoubleBuffer(PanelMenu);

            // Gradient background
            PanelMenu.Paint += (_, e) =>
            {
                using var brush = new LinearGradientBrush(
                    PanelMenu.ClientRectangle, SidebarTop, SidebarBottom, 90f);
                e.Graphics.FillRectangle(brush, PanelMenu.ClientRectangle);
            };

            // Reposition logout button when sidebar resizes
            PanelMenu.Resize += (_, _) =>
            {
                PanelMenu.Invalidate();
                if (BtnLogOut != null)
                {
                    BtnLogOut.Location = new Point(0, PanelMenu.Height - 56);
                    BtnLogOut.Width = PanelMenu.Width;
                }
            };

            // ── Logo badge ────────────────────────────────────────────────────
            var badge = new Label
            {
                Size = new Size(56, 56),
                Location = new Point((MenuExpanded - 56) / 2, 16),
                Text = "KARIN",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(200, 168, 75),
                Cursor = Cursors.Hand
            };
            var circlePath = new GraphicsPath();
            circlePath.AddEllipse(0, 0, 55, 55);
            badge.Region = new Region(circlePath);
            PanelMenu.Controls.Add(badge);

            // ── Toggle button ─────────────────────────────────────────────────
            BtnToggleMenu = new Button
            {
                Text = "◄",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(32, 32),
                // FIX: position relative to collapsed width so it's always visible
                Location = new Point(MenuExpanded - 38, 88),
                Cursor = Cursors.Hand
            };
            BtnToggleMenu.FlatAppearance.BorderSize = 0;
            BtnToggleMenu.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 255, 255, 255);
            BtnToggleMenu.Click += (_, _) => ToggleMenu();
            PanelMenu.Controls.Add(BtnToggleMenu);

            // ── Nav items ─────────────────────────────────────────────────────
            int y = 88, step = 52;
            _menuButtons.Add(CreateNavButton("Pedidos", "pedidos.png", y));
            _menuButtons.Add(CreateNavButton("Cuentas", "cuentas.png", y + step));
            _menuButtons.Add(CreateNavButton("Inventario", "inventario.png", y + step * 2));
            _menuButtons.Add(CreateNavButton("Recetas", "recetas.png", y + step * 3));
            _menuButtons.Add(CreateNavButton("Reportes", "reportes.png", y + step * 4));
            _menuButtons.Add(CreateNavButton("Configuración", "configuration.png", y + step * 5));

            // ── Logout button (bottom-anchored) ───────────────────────────────
            // FIX: use a separate reference; don't mix posY=0 with nav items
            BtnLogOut = CreateNavButton("Salir", "logout.png", PanelMenu.Height - 56);
            BtnLogOut.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            BtnLogOut.ForeColor = Color.FromArgb(200, 255, 255, 255);
            BtnLogOut.Click += BtnLogOut_Click;
            // BtnLogOut is already added to PanelMenu inside CreateNavButton
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Main content panel
        // ─────────────────────────────────────────────────────────────────────
        private void BuildContentPanel()
        {
            PanelContenedor = new Panel { Dock = DockStyle.Fill, BackColor = MainBg };
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Mesas UI
        // ─────────────────────────────────────────────────────────────────────
        private void BuildMesasUI()
        {
            PanelContenedor.Controls.Clear();

            ScrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(24, 20, 24, 20),
                BackColor = Color.Transparent
            };

            // ── Search bar ────────────────────────────────────────────────────
            var searchPanel = BuildSearchBar();

            // ── Section labels + flow panels ──────────────────────────────────
            var lblActivas = MakeSectionLabel("Mesas Activas :");
            var lblDisponibles = MakeSectionLabel("Mesas Disponibles :");

            FlowActivas = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 6, 0, 6)
            };

            FlowDisponibles = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 6, 0, 6)
            };

            // Controls added bottom-to-top when Dock = Top
            ScrollPanel.Controls.Add(FlowDisponibles);
            ScrollPanel.Controls.Add(lblDisponibles);
            ScrollPanel.Controls.Add(FlowActivas);
            ScrollPanel.Controls.Add(lblActivas);
            ScrollPanel.Controls.Add(searchPanel);

            PanelContenedor.Controls.Add(ScrollPanel);

            // ── Populate cards ────────────────────────────────────────────────
            RefreshMesaCards();

            // ── Responsive sizing ─────────────────────────────────────────────
            AdjustCardWidths();
            Resize += (_, _) => AdjustCardWidths();
        }

        private void RefreshMesaCards()
        {
            FlowActivas.Controls.Clear();
            FlowDisponibles.Controls.Clear();

            foreach (var m in _mesas.Where(x => x.Activa))
            {
                var card = BuildActivaCard(m);
                card.Margin = new Padding(0, 0, 0, 10);
                FlowActivas.Controls.Add(card);
            }

            foreach (var m in _mesas.Where(x => !x.Activa))
            {
                var row = BuildDisponibleRow(m);
                row.Margin = new Padding(0, 0, 0, 8);
                FlowDisponibles.Controls.Add(row);
            }

            AdjustCardWidths();
        }

        // ── Search bar ────────────────────────────────────────────────────────
        private Panel BuildSearchBar()
        {
            var searchPanel = new Panel
            {
                Height = 52,
                Dock = DockStyle.Top,
                BackColor = Color.Transparent
            };

            var searchBg = new Panel
            {
                Size = new Size(380, 36),
                Left = 0,
                Top = 8,
                BackColor = Color.White
            };
            ApplyRoundedRegion(searchBg, 18);
            searchBg.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(new Rectangle(0, 0, searchBg.Width - 1, searchBg.Height - 1), 18);
                using var fill = new SolidBrush(Color.White);
                using var pen = new Pen(Color.FromArgb(200, 200, 200));
                e.Graphics.FillPath(fill, path);
                e.Graphics.DrawPath(pen, path);
            };

            var lblIcon = new Label
            {
                Text = "🔍",
                Font = FontRegular9,
                AutoSize = true,
                Location = new Point(10, 8),
                BackColor = Color.Transparent
            };

            var txtSearch = new TextBox
            {
                Width = 300,
                BorderStyle = BorderStyle.None,
                PlaceholderText = "BUSCAR MESA :",
                Location = new Point(34, 9),
                Font = FontRegular9
            };
            // FIX: live filter as user types
            txtSearch.TextChanged += (_, _) => FilterMesas(txtSearch.Text);

            searchBg.Controls.Add(lblIcon);
            searchBg.Controls.Add(txtSearch);

            var btnSelect = new Button
            {
                Text = "SELECCIONAR",
                Left = searchBg.Right + 12,
                Top = 8,
                Height = 34,
                AutoSize = true,
                BackColor = AccentTeal,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = FontBold9,
                Cursor = Cursors.Hand
            };
            btnSelect.FlatAppearance.BorderSize = 0;
            ApplyRoundedButton(btnSelect, 8);
            btnSelect.Click += (_, _) => FilterMesas(txtSearch.Text);

            searchPanel.Controls.Add(searchBg);
            searchPanel.Controls.Add(btnSelect);
            return searchPanel;
        }

        // ── Filter logic ──────────────────────────────────────────────────────
        private void FilterMesas(string query)
        {
            query = query.Trim().ToLower();

            FlowActivas.Controls.Clear();
            FlowDisponibles.Controls.Clear();

            var filtered = string.IsNullOrWhiteSpace(query)
                ? _mesas
                : _mesas.Where(m => m.Id.ToString().Contains(query)
                               || m.Nombre.ToLower().Contains(query)).ToList();

            foreach (var m in filtered.Where(x => x.Activa))
            {
                var card = BuildActivaCard(m);
                card.Margin = new Padding(0, 0, 0, 10);
                FlowActivas.Controls.Add(card);
            }

            foreach (var m in filtered.Where(x => !x.Activa))
            {
                var row = BuildDisponibleRow(m);
                row.Margin = new Padding(0, 0, 0, 8);
                FlowDisponibles.Controls.Add(row);
            }

            AdjustCardWidths();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Active mesa card
        // ─────────────────────────────────────────────────────────────────────
        private Panel BuildActivaCard(MesaModel m)
        {
            var card = new Panel
            {
                Height = CardHeight,
                BackColor = CardWhite,
                Padding = new Padding(16, 12, 16, 0)
            };

            // FIX: draw rounded rect on Paint instead of using Region clipping
            // so that inner controls remain fully visible
            card.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 12);
                using var fill = new SolidBrush(CardWhite);
                e.Graphics.FillPath(fill, path);
            };

            // Table icon
            var iconPanel = new Panel
            {
                Size = new Size(60, 70),
                Location = new Point(16, 14),
                BackColor = Color.Transparent
            };
            iconPanel.Paint += DrawTableIcon;
            card.Controls.Add(iconPanel);

            // Info labels
            int ix = 88;
            card.Controls.Add(MakeInfoLabel($"MESA : {m.Id}", ix, 12));
            card.Controls.Add(MakeInfoLabel($"Personas : {m.Personas}", ix + 130, 12));
            card.Controls.Add(MakeInfoLabel($"Hora Llegada : {m.HoraLlegada:hh:mm tt}", ix + 300, 12));
            card.Controls.Add(MakeInfoLabel($"Cuenta : {m.Cuenta:C}", ix, 42));
            card.Controls.Add(MakeInfoLabel($"Propina ({m.PropinaPercent}%) : {m.Cuenta * m.PropinaPercent / 100:C}", ix + 130, 42));

            // ── Action button bar ─────────────────────────────────────────────
            // FIX: use TableLayoutPanel for reliable equal-width button columns
            var actionBar = new TableLayoutPanel
            {
                Height = 34,
                Dock = DockStyle.Bottom,       // FIX: Dock=Bottom is reliable
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = Padding.Empty,
                Margin = Padding.Empty
            };
            actionBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            actionBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            actionBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));
            actionBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var btnA = MakeActionBtn("Agregar Pedido", AccentTeal, () => OnAgregarPedido(m));
            var btnD = MakeActionBtn("Detalles", AccentSlate, () => OnVerDetalles(m));
            var btnC = MakeActionBtn("Cerrar Cuenta", AccentDarkNavy, () => OnCerrarCuenta(m));

            btnA.Dock = DockStyle.Fill;
            btnD.Dock = DockStyle.Fill;
            btnC.Dock = DockStyle.Fill;

            actionBar.Controls.Add(btnA, 0, 0);
            actionBar.Controls.Add(btnD, 1, 0);
            actionBar.Controls.Add(btnC, 2, 0);

            card.Controls.Add(actionBar);
            return card;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Available mesa row
        // ─────────────────────────────────────────────────────────────────────
        private Panel BuildDisponibleRow(MesaModel m)
        {
            var row = new Panel
            {
                Height = RowHeight,
                BackColor = AvailableAlpha
            };

            // FIX: paint rounded rect; avoid Region clipping that clips child controls
            row.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(new Rectangle(0, 0, row.Width - 1, row.Height - 1), 10);
                using var fill = new SolidBrush(AvailableAlpha);
                e.Graphics.FillPath(fill, path);
            };

            var lblName = new Label
            {
                Text = $"MESA : {m.Id}",
                Font = FontBold10,
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(16, 12),
                BackColor = Color.Transparent
            };

            var lblCap = new Label
            {
                Text = $"Capacidad : {m.CapacidadMax} Personas",
                Font = FontRegular9,
                ForeColor = Color.FromArgb(220, 255, 255, 255),
                AutoSize = true,
                Location = new Point(160, 14),
                BackColor = Color.Transparent
            };

            var btnAbrir = new Button
            {
                Text = "Abrir Cuenta",
                Font = FontBold9,
                ForeColor = Color.White,
                BackColor = AccentTeal,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(110, 28),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            btnAbrir.FlatAppearance.BorderSize = 0;
            btnAbrir.Click += (_, _) => OnAbrirCuenta(m);

            // FIX: use Layout instead of a lambda closure that may capture stale width
            row.Layout += (_, _) =>
            {
                btnAbrir.Location = new Point(row.Width - 126, 9);
            };

            row.Controls.Add(lblName);
            row.Controls.Add(lblCap);
            row.Controls.Add(btnAbrir);
            return row;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Business-logic actions  (replace MessageBox with real logic)
        // ─────────────────────────────────────────────────────────────────────
        private void OnAgregarPedido(MesaModel m)
        {
            // TODO: open AgregarPedido sub-form passing m.Id
            MessageBox.Show($"Agregar pedido — Mesa {m.Id}", "Pedidos",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnVerDetalles(MesaModel m)
        {
            // TODO: open Detalles sub-form
            var info = $"Mesa {m.Id}\nPersonas: {m.Personas}\nLlegada: {m.HoraLlegada:hh:mm tt}\n" +
                       $"Cuenta: {m.Cuenta:C}\nPropina ({m.PropinaPercent}%): {m.Cuenta * m.PropinaPercent / 100:C}";
            MessageBox.Show(info, "Detalles de Mesa", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnCerrarCuenta(MesaModel m)
        {
            var r = MessageBox.Show($"¿Cerrar cuenta de Mesa {m.Id}?\nTotal: {m.Cuenta:C}",
                "Cerrar Cuenta", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.Yes)
            {
                m.Activa = false;
                RefreshMesaCards();
            }
        }

        private void OnAbrirCuenta(MesaModel m)
        {
            var r = MessageBox.Show($"¿Abrir cuenta para Mesa {m.Id} ({m.CapacidadMax} personas)?",
                "Abrir Cuenta", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.Yes)
            {
                m.Activa = true;
                m.HoraLlegada = DateTime.Now;
                m.Personas = 1;
                m.Cuenta = 0;
                RefreshMesaCards();
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Sidebar helpers
        // ─────────────────────────────────────────────────────────────────────
        private Button CreateNavButton(string label, string icon, int posY)
        {
            var btn = new Button
            {
                Text = "   " + label,
                Size = new Size(PanelMenu?.Width ?? MenuExpanded, 48),
                Location = new Point(0, posY),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = FontNav,
                TextAlign = ContentAlignment.MiddleLeft,
                ImageAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Cursor = Cursors.Hand,
                Tag = label,
                Padding = new Padding(14, 0, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 255, 255, 255);

            try
            {
                btn.Image = Image.FromFile(Path.Combine(Application.StartupPath, "Imgs", icon));
            }
            catch { /* icon not found — render text-only */ }

            btn.MouseEnter += (_, _) => btn.BackColor = Color.FromArgb(40, 255, 255, 255);
            btn.MouseLeave += (_, _) => btn.BackColor = Color.Transparent;

            PanelMenu?.Controls.Add(btn);
            return btn;
        }

        private void ToggleMenu()
        {
            if (PanelMenu == null) return;
            _menuExpanded = !_menuExpanded;

            SuspendLayout();

            if (_menuExpanded)
            {
                PanelMenu.Width = MenuExpanded;
                BtnToggleMenu.Text = "◄";
                // FIX: restore correct location after expand
                BtnToggleMenu.Location = new Point(MenuExpanded - 38, 88);

                foreach (var btn in _menuButtons)
                {
                    btn.Text = "   " + (btn.Tag as string ?? "");
                    btn.ImageAlign = ContentAlignment.MiddleLeft;
                    btn.Padding = new Padding(14, 0, 0, 0);
                }
            }
            else
            {
                PanelMenu.Width = MenuCollapsed;
                BtnToggleMenu.Text = "►";
                // FIX: center toggle inside collapsed sidebar
                BtnToggleMenu.Location = new Point((MenuCollapsed - 32) / 2, 88);

                foreach (var btn in _menuButtons)
                {
                    btn.Text = "";
                    btn.ImageAlign = ContentAlignment.MiddleCenter;
                    btn.Padding = Padding.Empty;
                }
            }

            ResumeLayout(true);
        }

        private void BtnLogOut_Click(object? sender, EventArgs e)
        {
            var r = MessageBox.Show("¿Estás seguro de que deseas cerrar sesión?",
                "Cerrar Sesión", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.Yes)
            {
                new FormLogin().Show();
                Hide();
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Layout helpers
        // ─────────────────────────────────────────────────────────────────────
        private void AdjustCardWidths()
        {
            if (FlowActivas == null || FlowDisponibles == null || ScrollPanel == null) return;
            int w = Math.Max(400, ScrollPanel.ClientSize.Width - 48);
            foreach (Control c in FlowActivas.Controls) c.Width = w;
            foreach (Control c in FlowDisponibles.Controls) c.Width = w;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Factory helpers
        // ─────────────────────────────────────────────────────────────────────
        private static Label MakeSectionLabel(string text) => new()
        {
            Text = text,
            Font = FontBold13,
            ForeColor = Color.White,
            Height = 32,
            Dock = DockStyle.Top,
            BackColor = Color.Transparent
        };

        private static Label MakeInfoLabel(string text, int x, int y) => new()
        {
            Text = text,
            Font = FontBold9,
            ForeColor = TextDark,
            AutoSize = true,
            Location = new Point(x, y),
            BackColor = Color.Transparent
        };

        private static Button MakeActionBtn(string text, Color bg, Action onClick)
        {
            var btn = new Button
            {
                Text = text,
                Font = FontBold8,
                ForeColor = Color.White,
                BackColor = bg,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (_, _) => onClick();
            return btn;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Drawing utilities
        // ─────────────────────────────────────────────────────────────────────
        private static void DrawTableIcon(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel p) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using var dark = new SolidBrush(Color.FromArgb(84, 110, 122));
            using var light = new SolidBrush(Color.FromArgb(120, 144, 156));
            using var seat = new SolidBrush(Color.FromArgb(80, 255, 255, 255));

            e.Graphics.FillRectangle(dark, 4, 22, 52, 26);   // table body
            e.Graphics.FillRectangle(light, 8, 18, 44, 8);    // table surface
            e.Graphics.FillRectangle(dark, 6, 46, 8, 14);   // left leg
            e.Graphics.FillRectangle(dark, 46, 46, 8, 14);   // right leg
            e.Graphics.FillRectangle(seat, 8, 28, 18, 12);   // left seat
            e.Graphics.FillRectangle(seat, 34, 28, 18, 12);   // right seat
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(r.Left, r.Top, radius * 2, radius * 2, 180, 90);
            path.AddArc(r.Right - radius * 2, r.Top, radius * 2, radius * 2, 270, 90);
            path.AddArc(r.Right - radius * 2, r.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(r.Left, r.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        // FIX: only apply Region clipping for controls where child controls
        // don't need to bleed to the edges (search box, button).
        // Cards and rows use Paint-based rounded drawing instead.
        private static void ApplyRoundedRegion(Control ctrl, int radius)
        {
            ctrl.Resize += (_, _) =>
            {
                if (ctrl.Width <= 0 || ctrl.Height <= 0) return;
                using var path = RoundedRect(new Rectangle(0, 0, ctrl.Width, ctrl.Height), radius);
                ctrl.Region = new Region(path);
            };
        }

        private static void ApplyRoundedButton(Button btn, int radius)
        {
            btn.Resize += (_, _) =>
            {
                if (btn.Width <= 0 || btn.Height <= 0) return;
                using var path = RoundedRect(new Rectangle(0, 0, btn.Width, btn.Height), radius);
                btn.Region = new Region(path);
            };
        }

        private static void EnableDoubleBuffer(Control ctrl)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, ctrl, new object[] { true });
        }
    }
}