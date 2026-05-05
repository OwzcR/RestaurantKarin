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

    
    public partial class Pedidos : UserControl
    {
        // ── Controls ─────────────────────────────────────────────────────────
        private Panel PanelContenedor = null!;
        private FlowLayoutPanel FlowActivas = null!;
        private FlowLayoutPanel FlowDisponibles = null!;
        private Panel ScrollPanel = null!;

        // ── State ─────────────────────────────────────────────────────────────
        private List<MesaModel> _mesas = new();

        // ── Layout constants ──────────────────────────────────────────────────
        private const int CardHeight = 175;
        private const int RowHeight  = 56;

        // ── Palette ───────────────────────────────────────────────────────────
        private static readonly Color MainBg         = Color.FromArgb(26, 58, 107);
        private static readonly Color CardBg         = Color.FromArgb(232, 232, 232);   // #E8E8E8
        private static readonly Color BtnAgregar     = Color.FromArgb(0, 151, 167);     // #0097A7
        private static readonly Color BtnDetalles    = Color.FromArgb(0, 124, 145);     // #007C91
        private static readonly Color BtnCerrar      = Color.FromArgb(0, 35, 55);       // #002337
        private static readonly Color BtnAbrir       = Color.FromArgb(23, 91, 122);     // #175B7A
        private static readonly Color BtnSeleccionar = Color.FromArgb(98, 199, 211);    // #62C7D3
        private static readonly Color AccentDarkNavy = Color.FromArgb(26, 35, 126);
        private static readonly Color TextDark       = Color.FromArgb(30, 30, 30);

        // ── Shared fonts (created once, reused everywhere) ────────────────────
        private static readonly Font FontBtn      = new("Sansation", 10, FontStyle.Bold);
        private static readonly Font FontBold9    = new("Sansation", 11, FontStyle.Bold);
        private static readonly Font FontRegular9 = new("Sansation", 10, FontStyle.Regular);
        private static readonly Font FontBold13   = new("Sansation", 13, FontStyle.Bold);
        private static readonly Font FontBold10   = new("Sansation", 11, FontStyle.Bold);

        public Pedidos()
        {
            DoubleBuffered = true;
            InitializeMesaData();
            SetupUI();
        }

        
        private void InitializeMesaData()
        {
            try
            {
                _mesas = MesaRepository.GetAll() ?? new List<MesaModel>();
                if (_mesas == null || !_mesas.Any())
                {
                    _mesas = GetSampleMesas();
                }
            }
            catch
            {
                _mesas = GetSampleMesas();
            }
        }

        private static List<MesaModel> GetSampleMesas()
        {
            return new List<MesaModel>
            {
                new MesaModel
                {
                    Id = 1,
                    Nombre = "Mesa 1",
                    CapacidadMax = 4,
                    Activa = true,
                    Personas = 2,
                    HoraLlegada = DateTime.Now.AddMinutes(-45),
                    Cuenta = 120.50m,
                    PropinaPercent = 10
                },
                new MesaModel
                {
                    Id = 2,
                    Nombre = "Mesa 2",
                    CapacidadMax = 6,
                    Activa = false,
                    Personas = 0,
                    HoraLlegada = DateTime.MinValue,
                    Cuenta = 0m,
                    PropinaPercent = 10
                },
                new MesaModel
                {
                    Id = 3,
                    Nombre = "Mesa 3",
                    CapacidadMax = 2,
                    Activa = true,
                    Personas = 1,
                    HoraLlegada = DateTime.Now.AddMinutes(-15),
                    Cuenta = 45.00m,
                    PropinaPercent = 10
                }
            };
        }

   
        private void SetupUI()
        {
            SuspendLayout();
            BackColor = Color.Transparent;
            BuildContentPanel();
            Controls.Add(PanelContenedor);
            BuildMesasUI();
            ResumeLayout(true);
        }


        // ─────────────────────────────────────────────────────────────────────
        //  Main content panel
        // ─────────────────────────────────────────────────────────────────────
        private void BuildContentPanel()
        {
            PanelContenedor = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Mesas UI
        // ─────────────────────────────────────────────────────────────────────
        private void BuildMesasUI()
        {
            PanelContenedor.SuspendLayout();
            PanelContenedor.Controls.Clear();

            ScrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(24, 20, 24, 20),
                BackColor = Color.Transparent
            };
            EnableDoubleBuffer(ScrollPanel);
            ScrollPanel.SuspendLayout();

            // ── Search bar ────────────────────────────────────────────────────
            var searchPanel = BuildSearchBar();

            // ── Section labels + flow panels ──────────────────────────────────
            var lblActivas = MakeSectionLabel("Mesas Activas :");
            var lblDisponibles = MakeSectionLabel("Mesas Disponibles :");

            FlowActivas = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 0,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 6, 0, 6),
                BackColor = Color.Transparent
            };
            EnableDoubleBuffer(FlowActivas);

            FlowDisponibles = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 0,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 6, 0, 6),
                BackColor = Color.Transparent
            };
            EnableDoubleBuffer(FlowDisponibles);

            // Controls added bottom-to-top when Dock = Top
            ScrollPanel.Controls.Add(FlowDisponibles);
            ScrollPanel.Controls.Add(lblDisponibles);
            ScrollPanel.Controls.Add(FlowActivas);
            ScrollPanel.Controls.Add(lblActivas);
            ScrollPanel.Controls.Add(searchPanel);

            PanelContenedor.Controls.Add(ScrollPanel);

            // ── Populate cards ────────────────────────────────────────────────
            RefreshMesaCards();

            ScrollPanel.ResumeLayout(false);
            PanelContenedor.ResumeLayout(false);

            // ── Responsive sizing ─────────────────────────────────────────────
            AdjustCardWidths();
            Resize += (_, _) => AdjustCardWidths();
        }

        private void RefreshMesaCards()
        {
            FlowActivas.SuspendLayout();
            FlowDisponibles.SuspendLayout();
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

            FlowActivas.ResumeLayout(false);
            FlowDisponibles.ResumeLayout(false);
            UpdateFlowHeights();
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
                BackColor = BtnSeleccionar,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = FontBtn,
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

            FlowActivas.SuspendLayout();
            FlowDisponibles.SuspendLayout();
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

            FlowActivas.ResumeLayout(false);
            FlowDisponibles.ResumeLayout(false);
            UpdateFlowHeights();
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
                BackColor = CardBg,
                Padding = new Padding(16, 12, 16, 0)
            };

            card.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 12);
                using var fill = new SolidBrush(CardBg);
                e.Graphics.FillPath(fill, path);
            };

            // Table icon
            var iconPanel = new Panel
            {
                Size = new Size(72, 92),
                Location = new Point(14, 18),
                BackColor = Color.Transparent
            };
            iconPanel.Paint += DrawTableIcon;
            card.Controls.Add(iconPanel);

            // Info labels — two-row grid matching the original design
            int ix = 100;
            card.Controls.Add(MakeInfoLabel($"MESA : {m.Id}",           ix,       22));
            card.Controls.Add(MakeInfoLabel($"Personas : {m.Personas}", ix + 175, 22));
            card.Controls.Add(MakeInfoLabel($"Hora Llegada : {m.HoraLlegada.ToString("hh:mm tt").ToLower()}", ix + 360, 22));
            card.Controls.Add(MakeInfoLabel($"Cuenta : {m.Cuenta:0}$",  ix,       64));
            card.Controls.Add(MakeInfoLabel($"Propina : {m.PropinaPercent}%",     ix + 175, 64));

            // ── Action button bar ─────────────────────────────────────────────
            // FIX: use TableLayoutPanel for reliable equal-width button columns
            var actionBar = new TableLayoutPanel
            {
                Height = 40,
                Dock = DockStyle.Bottom,
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

            var btnA = MakeActionBtn("Agregar Pedido", BtnAgregar, () => OnAgregarPedido(m));
            var btnD = MakeActionBtn("Detalles", BtnDetalles, () => OnVerDetalles(m));
            var btnC = MakeActionBtn("Cerrar Cuenta", BtnCerrar, () => OnCerrarCuenta(m));

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
                BackColor = Color.Transparent
            };

            // FIX: paint rounded rect; avoid Region clipping that clips child controls
            row.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedRect(new Rectangle(0, 0, row.Width - 1, row.Height - 1), 10);
                using var fill = new SolidBrush(CardBg);
                e.Graphics.FillPath(fill, path);
            };

            var lblName = new Label
            {
                Text = $"MESA : {m.Id}",
                Font = FontBold10,
                ForeColor = AccentDarkNavy,
                AutoSize = true,
                Location = new Point(16, 18),
                BackColor = Color.Transparent
            };

            var lblCap = new Label
            {
                Text = $"Capacidad : {m.CapacidadMax} Personas",
                Font = FontRegular9,
                ForeColor = Color.FromArgb(60, 80, 120),
                AutoSize = true,
                Location = new Point(200, 18),
                BackColor = Color.Transparent
            };

            var btnAbrir = new Button
            {
                Text = "Abrir Cuenta",
                Font = FontBtn,
                ForeColor = Color.White,
                BackColor = BtnAbrir,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(126, 36),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            btnAbrir.FlatAppearance.BorderSize = 0;
            btnAbrir.Click += (_, _) => OnAbrirCuenta(m);
            ApplyRoundedButton(btnAbrir, 8);

            row.Layout += (_, _) =>
            {
                btnAbrir.Location = new Point(row.Width - 140, 10);
            };

            row.Controls.Add(lblName);
            row.Controls.Add(lblCap);
            row.Controls.Add(btnAbrir);
            return row;
        }

      
        private void OnAgregarPedido(MesaModel m)
        {
            // Obtener el id_cuenta activo de esta mesa
            int idCuenta = CuentaRepository.ObtenerIdCuentaAbierta(m.Id);

            if (idCuenta <= 0)
            {
                MessageBox.Show("Esta mesa no tiene una cuenta abierta.\nAbre la cuenta primero.",
                    "Sin cuenta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var form = new FormAgregarPedido(m.Id, idCuenta);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                // Refresca la pantalla para mostrar el nuevo total
                _mesas = MesaRepository.GetAll();
                RefreshMesaCards();
            }
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
            using var dlg = new FormCerrarPedido(m.Id, m.Cuenta);
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Confirmado)
            {
                CuentaRepository.CerrarCuenta(m.Id);
                _mesas = MesaRepository.GetAll();
                RefreshMesaCards();
            }
        }

        private void OnAbrirCuenta(MesaModel m)
        {
            var r = MessageBox.Show(
                $"¿Abrir cuenta para Mesa {m.Id} ({m.CapacidadMax} personas)?",
                "Abrir Cuenta", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (r == DialogResult.Yes)
            {
                // idUsuario = 1 por ahora; más adelante pasa el usuario de sesión
                CuentaRepository.AbrirCuenta(m.Id, idUsuario: 1);

                // Refresca desde la BD para reflejar el cambio real
                _mesas = MesaRepository.GetAll();
                RefreshMesaCards();
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

        private void UpdateFlowHeights()
        {
            if (FlowActivas == null || FlowDisponibles == null) return;
            FlowActivas.Height = Math.Max(12, FlowActivas.Controls.Count * (CardHeight + 10) + 12);
            FlowDisponibles.Height = Math.Max(12, FlowDisponibles.Controls.Count * (RowHeight + 8) + 12);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Factory helpers
        // ─────────────────────────────────────────────────────────────────────
        private static Label MakeSectionLabel(string text) => new()
        {
            Text = text,
            Font = FontBold13,
            ForeColor = Color.FromArgb(20, 30, 55),
            Height = 36,
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
                Font = FontBtn,
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
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int cx = p.Width / 2, cy = p.Height / 2;

            using var pen = new Pen(Color.FromArgb(84, 110, 122), 1.8f);
            using var tableFill = new SolidBrush(Color.FromArgb(225, 238, 248));
            using var chairFill = new SolidBrush(Color.FromArgb(200, 220, 238));

            // Table surface (top-down view, centered)
            g.FillRectangle(tableFill, cx - 18, cy - 22, 36, 42);
            g.DrawRectangle(pen, cx - 18, cy - 22, 36, 42);

            // Left chairs
            g.FillRectangle(chairFill, cx - 30, cy - 14, 11, 10);
            g.DrawRectangle(pen, cx - 30, cy - 14, 11, 10);
            g.FillRectangle(chairFill, cx - 30, cy + 4, 11, 10);
            g.DrawRectangle(pen, cx - 30, cy + 4, 11, 10);

            // Right chairs
            g.FillRectangle(chairFill, cx + 19, cy - 14, 11, 10);
            g.DrawRectangle(pen, cx + 19, cy - 14, 11, 10);
            g.FillRectangle(chairFill, cx + 19, cy + 4, 11, 10);
            g.DrawRectangle(pen, cx + 19, cy + 4, 11, 10);
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