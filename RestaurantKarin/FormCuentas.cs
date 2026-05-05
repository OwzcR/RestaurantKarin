using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public class FormCuentas : UserControl
    {
        private const string Conn = "Data Source=karin_pos.db;Version=3;";

        // ── Palette ───────────────────────────────────────────────────────────
        private static readonly Color ColPanelBg   = Color.FromArgb(232, 232, 232); // #E8E8E8
        private static readonly Color ColRowBg     = Color.FromArgb(211, 211, 211); // #D3D3D3
        private static readonly Color ColIdText    = Color.FromArgb(23, 91, 122);   // #175B7A
        private static readonly Color ColBtnCerrar = Color.FromArgb(0, 35, 55);     // #002337
        private static readonly Color ColBtnDetall = Color.FromArgb(0, 124, 145);   // #007C91
        private static readonly Color ColBtnSelect = Color.FromArgb(98, 199, 211);  // teal

        // ── Fonts ─────────────────────────────────────────────────────────────
        private static readonly Font FntBtn    = new("Sansation", 9,  FontStyle.Bold);
        private static readonly Font FntBold9  = new("Sansation", 9,  FontStyle.Bold);
        private static readonly Font FntReg9   = new("Sansation", 9,  FontStyle.Regular);
        private static readonly Font FntBold11 = new("Sansation", 11, FontStyle.Bold);

        // ── Controls ──────────────────────────────────────────────────────────
        private Panel           _pnlSearch = null!;
        private Panel           _pnlMain   = null!;
        private FlowLayoutPanel _flow      = null!;
        private TextBox         _txtBuscar = null!;

        // ── Data ──────────────────────────────────────────────────────────────
        private List<CuentaItem> _cuentas = new();

        private sealed class CuentaItem
        {
            public int      Id            { get; init; }
            public string   Folio         { get; init; } = "";
            public string   Estado        { get; init; } = "";
            public decimal  Total         { get; init; }
            public bool     Abierta       { get; init; }
            public int      NumeroMesa    { get; init; }
            public int      IdMesa        { get; init; }
            public DateTime FechaApertura { get; init; }
        }

        // ─────────────────────────────────────────────────────────────────────
        public FormCuentas()
        {
            DoubleBuffered = true;
            BackColor      = Color.Transparent;
            BuildUI();
            CargarCuentas();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  UI construction
        // ─────────────────────────────────────────────────────────────────────
        private void BuildUI()
        {
            // ── Search bar (transparent — sits on the base-form background) ──
            _pnlSearch = new Panel { BackColor = Color.Transparent };

            var searchBg = new Panel { Size = new Size(300, 36), Location = new Point(0, 8), BackColor = Color.White };
            searchBg.Paint += PaintRoundedWhite;
            ApplyRoundedRegion(searchBg, 18);
            searchBg.HandleCreated += (_, _) =>
            {
                using var path = RoundedPath(new Rectangle(0, 0, searchBg.Width, searchBg.Height), 18);
                searchBg.Region = new Region(path);
            };

            var lblIcon = new Label { Text = "🔍", AutoSize = true, Location = new Point(10, 9), BackColor = Color.Transparent, Font = FntReg9 };
            _txtBuscar = new TextBox
            {
                Width           = 220,
                BorderStyle     = BorderStyle.None,
                PlaceholderText = "BUSCAR ID CUENTA :",
                Location        = new Point(32, 10),
                Font            = FntReg9
            };
            _txtBuscar.TextChanged += (_, _) => FiltrarYRenderizar(_txtBuscar.Text);

            searchBg.Controls.Add(lblIcon);
            searchBg.Controls.Add(_txtBuscar);

            var btnSelect = new Button
            {
                Text      = "SELECCIONAR",
                Location  = new Point(searchBg.Right + 12, 8),
                Height    = 34,
                AutoSize  = true,
                BackColor = ColBtnSelect,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = FntBtn,
                Cursor    = Cursors.Hand
            };
            btnSelect.FlatAppearance.BorderSize = 0;
            ApplyRoundedButton(btnSelect, 8);
            btnSelect.Click += (_, _) => FiltrarYRenderizar(_txtBuscar.Text);

            _pnlSearch.Controls.Add(searchBg);
            _pnlSearch.Controls.Add(btnSelect);

            // ── Main rounded panel ────────────────────────────────────────────
            _pnlMain = new Panel { BackColor = ColPanelBg };
            _pnlMain.Resize += (_, _) => ApplyRoundedRegion(_pnlMain, 20);
            EnableDoubleBuffer(_pnlMain);

            // "Cuentas :" tab label
            var tabHost = new Panel { Height = 42, Dock = DockStyle.Top, BackColor = Color.Transparent };
            var lblTab = new Label
            {
                Text      = "Cuentas :",
                Font      = FntBold11,
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize  = true,
                Location  = new Point(16, 12),
                BackColor = Color.Transparent
            };
            tabHost.Controls.Add(lblTab);

            // Flow panel — scrollable account rows
            _flow = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                AutoScroll    = true,
                Padding       = new Padding(14, 6, 14, 14),
                BackColor     = Color.Transparent
            };
            EnableDoubleBuffer(_flow);
            _flow.ClientSizeChanged += (_, _) => AdjustRowWidths();

            _pnlMain.Controls.Add(_flow);
            _pnlMain.Controls.Add(tabHost);

            Controls.Add(_pnlSearch);
            Controls.Add(_pnlMain);

            Resize       += (_, _) => Reposition();
            HandleCreated += (_, _) => Reposition();
        }

        private void Reposition()
        {
            const int mH = 16, mV = 16, searchH = 54, gap = 8;
            int totalW = Math.Max(400, ClientSize.Width  - mH * 2);
            int totalH = Math.Max(200, ClientSize.Height - mV * 2);

            _pnlSearch.Bounds = new Rectangle(mH, mV, totalW, searchH);
            _pnlMain.Bounds   = new Rectangle(mH, mV + searchH + gap,
                                               totalW, totalH - searchH - gap);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Data
        // ─────────────────────────────────────────────────────────────────────
        private void CargarCuentas()
        {
            _cuentas.Clear();
            try
            {
                using var con = new SQLiteConnection(Conn);
                con.Open();
                using var cmd = new SQLiteCommand(@"
                    SELECT c.id_cuenta, c.id_mesa, c.fecha_apertura, c.estado_cuenta,
                           COALESCE(c.subtotal, 0) AS monto,
                           COALESCE(m.numero_mesa, 0) AS numero_mesa
                    FROM   cuenta c
                    LEFT JOIN mesa m ON m.id_mesa = c.id_mesa
                    ORDER  BY c.fecha_apertura DESC;", con);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    var estadoRaw = r["estado_cuenta"].ToString()!;
                    var fecha = r["fecha_apertura"] != DBNull.Value
                        ? DateTime.SpecifyKind(Convert.ToDateTime(r["fecha_apertura"]),
                                               DateTimeKind.Utc).ToLocalTime()
                        : DateTime.Now;
                    int id = Convert.ToInt32(r["id_cuenta"]);
                    _cuentas.Add(new CuentaItem
                    {
                        Id            = id,
                        Folio         = $"{fecha:yyyyMMdd}-{id:D3}",
                        Estado        = estadoRaw == "Abierta" ? "Activa" : "Pagada",
                        Total         = Convert.ToDecimal(r["monto"]),
                        Abierta       = estadoRaw == "Abierta",
                        NumeroMesa    = Convert.ToInt32(r["numero_mesa"]),
                        IdMesa        = r["id_mesa"] != DBNull.Value ? Convert.ToInt32(r["id_mesa"]) : 0,
                        FechaApertura = fecha
                    });
                }
            }
            catch { }
            FiltrarYRenderizar(_txtBuscar?.Text ?? "");
        }

        private void FiltrarYRenderizar(string query)
        {
            query = query.Trim().ToLower();
            var lista = string.IsNullOrWhiteSpace(query)
                ? _cuentas
                : _cuentas.FindAll(c =>
                    c.Folio.ToLower().Contains(query) ||
                    c.Estado.ToLower().Contains(query));

            _flow.SuspendLayout();
            _flow.Controls.Clear();
            foreach (var c in lista)
            {
                var row = BuildRow(c);
                row.Margin = new Padding(0, 0, 0, 8);
                _flow.Controls.Add(row);
            }
            AdjustRowWidths();
            _flow.ResumeLayout(true);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Row builder
        // ─────────────────────────────────────────────────────────────────────
        private Panel BuildRow(CuentaItem c)
        {
            var row = new Panel { Height = 50, BackColor = Color.Transparent };
            row.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RoundedPath(new Rectangle(0, 0, row.Width - 1, row.Height - 1), 10);
                using var fill = new SolidBrush(ColRowBg);
                e.Graphics.FillPath(fill, path);
            };

            var lblId = new Label
            {
                Text      = $"ID : {c.Folio}",
                Font      = FntBold9,
                ForeColor = ColIdText,
                AutoSize  = true,
                Location  = new Point(14, 16),
                BackColor = Color.Transparent
            };

            var lblMesa = new Label
            {
                Text      = c.NumeroMesa > 0 ? $"Mesa : {c.NumeroMesa}" : "Mesa : —",
                Font      = FntBold9,
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize  = true,
                Location  = new Point(230, 16),
                BackColor = Color.Transparent
            };

            var lblEstado = new Label
            {
                Text      = $"Estado : {c.Estado}",
                Font      = FntBold9,
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize  = true,
                Location  = new Point(360, 16),
                BackColor = Color.Transparent
            };

            var lblTotal = new Label
            {
                Text      = $"Cuenta: {c.Total:0}$",
                Font      = FntBold9,
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize  = true,
                Location  = new Point(500, 16),
                BackColor = Color.Transparent
            };

            row.Controls.Add(lblId);
            row.Controls.Add(lblMesa);
            row.Controls.Add(lblEstado);
            row.Controls.Add(lblTotal);

            if (c.Abierta)
            {
                var btnCerrar = MakeRowBtn("Cerrar Cuenta", ColBtnCerrar, 110);
                var btnDet    = MakeRowBtn("Detalles",      ColBtnDetall, 82);

                row.Layout += (_, _) =>
                {
                    btnDet.Location    = new Point(row.Width - btnDet.Width - 10, 10);
                    btnCerrar.Location = new Point(row.Width - btnDet.Width - btnCerrar.Width - 18, 10);
                };

                btnCerrar.Click += (_, _) =>
                {
                    using var dlg = new FormCerrarPedido(0, c.Total, c.NumeroMesa, c.Folio);
                    if (dlg.ShowDialog(FindForm()) == DialogResult.OK && dlg.Confirmado)
                    {
                        CuentaRepository.CerrarCuentaPorId(c.Id);
                        CargarCuentas();
                    }
                };
                btnDet.Click += (_, _) => MostrarDetalles(c);

                row.Controls.Add(btnCerrar);
                row.Controls.Add(btnDet);
            }
            else
            {
                var btnDet = MakeRowBtn("Detalles", ColBtnDetall, 82);
                row.Layout += (_, _) =>
                    btnDet.Location = new Point(row.Width - btnDet.Width - 10, 10);
                btnDet.Click += (_, _) => MostrarDetalles(c);
                row.Controls.Add(btnDet);
            }

            return row;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Actions
        // ─────────────────────────────────────────────────────────────────────
        private void MostrarDetalles(CuentaItem c)
        {
            var mesa = new MesaModel
            {
                Id          = c.IdMesa,
                NumeroMesa  = c.NumeroMesa,
                IdCuenta    = c.Id,
                HoraLlegada = c.FechaApertura,
                Activa      = c.Abierta,
                Nombre      = c.NumeroMesa > 0 ? $"MESA : {c.NumeroMesa}" : "—"
            };
            var mainForm = FindForm();
            if (mainForm == null) return;
            using var overlay = CreateOverlay(mainForm);
            overlay.Show(mainForm);
            using var frm = new FormDetallesMesa(mesa);
            if (frm.ShowDialog(overlay) == DialogResult.OK)
                CargarCuentas();
            overlay.Close();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────────────────────────────
        private void AdjustRowWidths()
        {
            int w = Math.Max(300, _flow.ClientSize.Width - _flow.Padding.Horizontal);
            foreach (Control c in _flow.Controls)
                c.Width = w;
        }

        private static Button MakeRowBtn(string text, Color bg, int width)
        {
            var btn = new Button
            {
                Text      = text,
                Font      = FntBtn,
                ForeColor = Color.White,
                BackColor = bg,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(width, 30),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            ApplyRoundedButton(btn, 6);
            return btn;
        }

        private static void PaintRoundedWhite(object? sender, PaintEventArgs e)
        {
            if (sender is not Control c) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = RoundedPath(new Rectangle(0, 0, c.Width - 1, c.Height - 1), 18);
            using var fill = new SolidBrush(Color.White);
            using var pen  = new Pen(Color.FromArgb(200, 200, 200));
            e.Graphics.FillPath(fill, path);
            e.Graphics.DrawPath(pen, path);
        }

        private static GraphicsPath RoundedPath(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            p.AddArc(r.Left,              r.Top,               radius * 2, radius * 2, 180, 90);
            p.AddArc(r.Right - radius*2,  r.Top,               radius * 2, radius * 2, 270, 90);
            p.AddArc(r.Right - radius*2,  r.Bottom - radius*2, radius * 2, radius * 2, 0,   90);
            p.AddArc(r.Left,              r.Bottom - radius*2, radius * 2, radius * 2, 90,  90);
            p.CloseFigure();
            return p;
        }

        private static void ApplyRoundedRegion(Control ctrl, int radius)
        {
            ctrl.Resize += (_, _) =>
            {
                if (ctrl.Width <= 0 || ctrl.Height <= 0) return;
                using var path = RoundedPath(new Rectangle(0, 0, ctrl.Width, ctrl.Height), radius);
                ctrl.Region = new Region(path);
            };
        }

        private static void ApplyRoundedButton(Button btn, int radius)
        {
            btn.Resize += (_, _) =>
            {
                if (btn.Width <= 0 || btn.Height <= 0) return;
                using var path = RoundedPath(new Rectangle(0, 0, btn.Width, btn.Height), radius);
                btn.Region = new Region(path);
            };
        }

        private static Form CreateOverlay(Form owner)
        {
            var origin = owner.PointToScreen(Point.Empty);
            return new Form
            {
                FormBorderStyle   = FormBorderStyle.None,
                AllowTransparency = true,
                BackColor         = Color.FromArgb(13, 41, 78),
                Opacity           = 0.80,
                StartPosition     = FormStartPosition.Manual,
                Location          = origin,
                Size              = owner.ClientSize,
                ShowInTaskbar     = false
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
