using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public partial class FormReportes : UserControl
    {
        // ── Paleta de colores (exacta según imagen) ────────────────────────
        private static readonly Color CAzul = Color.FromArgb(30, 80, 120);   // azul oscuro navbar/headers
        private static readonly Color CAzulMedio = Color.FromArgb(45, 105, 150);   // barras de fondo
        private static readonly Color CNaranja = Color.FromArgb(240, 140, 55);   // barras de progreso / acento
        private static readonly Color CTexto = Color.FromArgb(20, 45, 75);   // texto principal
        private static readonly Color CFondo = Color.FromArgb(200, 225, 240);   // fondo general
        private static readonly Color CBlancoCard = Color.White;
        private static readonly Color CHeaderCard = Color.FromArgb(28, 78, 118);   // cabecera tarjeta

        // Datos del gráfico de barras
        private readonly string[] _dias = { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };
        private readonly double[] _ventas = { 400, 500, 350, 450, 650, 600, 550 };

        public FormReportes()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            BuildUI();
        }

        private void BuildUI()
        {
            this.BackColor = CFondo;
            this.Padding = new Padding(16);

            // ── Scroll container ──────────────────────────────────────────
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = CFondo
            };
            this.Controls.Add(scroll);

            // Inner content panel — grows downward
            var inner = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = CFondo,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            scroll.Controls.Add(inner);

            // ═══════════════════════════════════════════════════════════════
            // 1. BARRA DE FILTROS  ──  PERÍODO DE TIEMPO  07/02/26  07/02/26  VER REPORTE
            // ═══════════════════════════════════════════════════════════════
            var pnlFiltros = new Panel
            {
                Size = new Size(1060, 52),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 14)
            };
            inner.Controls.Add(pnlFiltros);

            // "PERÍODO DE TIEMPO" badge
            var lblPeriodo = new Label
            {
                Text = "PERÍODO DE TIEMPO",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = CAzul,
                AutoSize = false,
                Size = new Size(158, 32),
                Location = new Point(0, 10),
                TextAlign = ContentAlignment.MiddleCenter
            };
            RoundControl(lblPeriodo, 16);
            pnlFiltros.Controls.Add(lblPeriodo);

            // DateTimePicker inicio
            var dtpInicio = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Value = new DateTime(2026, 2, 7),
                Location = new Point(168, 10),
                Size = new Size(115, 32),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                CalendarForeColor = CTexto,
                CalendarMonthBackground = Color.White
            };
            pnlFiltros.Controls.Add(dtpInicio);

            var dtpFin = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Value = new DateTime(2026, 2, 7),
                Location = new Point(293, 10),
                Size = new Size(115, 32),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            pnlFiltros.Controls.Add(dtpFin);

            var btnVer = MakeButton("VER REPORTE", CAzul, 418, 10, 130, 32);
            pnlFiltros.Controls.Add(btnVer);

            // ═══════════════════════════════════════════════════════════════
            // 2. FILA 1:  Ventas (izq)  |  Productos Más Vendidos (der)
            // ═══════════════════════════════════════════════════════════════
            var fila1 = new Panel
            {
                Size = new Size(1060, 295),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 14)
            };
            inner.Controls.Add(fila1);

            // ── Tarjeta Ventas ────────────────────────────────────────────
            var cardVentas = MakeCard(0, 0, 520, 295, "Ventas");
            fila1.Controls.Add(cardVentas);

            var pnlChart = new Panel
            {
                Location = new Point(10, 38),
                Size = new Size(500, 210),
                BackColor = Color.White
            };
            pnlChart.Paint += (s, e) => DrawBarChart(e.Graphics, pnlChart);
            cardVentas.Controls.Add(pnlChart);

            var lblMoney = new Label
            {
                Text = "$3,250",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = CTexto,
                AutoSize = true,
                Location = new Point(14, 255)
            };
            cardVentas.Controls.Add(lblMoney);

            var lblTotal = new Label
            {
                Text = "Total",
                Font = new Font("Segoe UI", 10),
                ForeColor = CTexto,
                AutoSize = true,
                Location = new Point(100, 262)
            };
            cardVentas.Controls.Add(lblTotal);

            // ── Tarjeta Productos Más Vendidos ───────────────────────────
            var cardProd = MakeCard(534, 0, 526, 295, "Productos Más Vendidos");
            fila1.Controls.Add(cardProd);

            string[] platillos = { "Platillo 1", "Platillo 2", "Platillo 3", "Platillo 4" };
            int[] cantidades = { 48, 25, 45, 10 };
            for (int i = 0; i < platillos.Length; i++)
                AddProductRow(cardProd, platillos[i], cantidades[i], 50, i, showIcon: true);

            var lblVentasTot = new Label
            {
                Text = "128 Ventas Totales",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = CTexto,
                AutoSize = true,
                Location = new Point(14, 258)
            };
            cardProd.Controls.Add(lblVentasTot);

            // ═══════════════════════════════════════════════════════════════
            // 3. FILA 2:  Consumo Inventario (izq)  |  Ingresos Empleado (der)
            // ═══════════════════════════════════════════════════════════════
            var fila2 = new Panel
            {
                Size = new Size(1060, 295),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 14)
            };
            inner.Controls.Add(fila2);

            // ── Tarjeta Consumo Inventario ────────────────────────────────
            var cardInv = MakeCard(0, 0, 520, 295, "Consumo de Inventario");
            fila2.Controls.Add(cardInv);

            string[] insumos = { "Insumo 1", "Insumo 2", "Insumo 3", "Insumo 4" };
            int[] consumos = { 24, 18, 16, 10 };
            for (int i = 0; i < insumos.Length; i++)
                AddProductRow(cardInv, insumos[i], consumos[i], 28, i, showIcon: true);

            var lblPct = new Label
            {
                Text = "67% del Inventario Consumido",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = CTexto,
                AutoSize = true,
                Location = new Point(14, 258)
            };
            cardInv.Controls.Add(lblPct);

            // ── Tarjeta Ingresos por Empleado ─────────────────────────────
            var cardEmp = MakeCard(534, 0, 526, 295, "Ingresos por Empleado");
            fila2.Controls.Add(cardEmp);

            string[] nombres = { "Felipe", "José", "Carla", "Román" };
            string[] ingresos = { "$1,320.00", "$780.00", "$650.00", "$500.00" };
            int[] porcentajes = { 90, 53, 44, 34 };
            for (int i = 0; i < nombres.Length; i++)
                AddEmployeeRow(cardEmp, nombres[i], ingresos[i], porcentajes[i], i);

            // Botones exportar — dentro de cardEmp, abajo a la derecha
            var btnPDF = MakeButton("+ EXPORTAR PDF", CAzul, 14, 255, 160, 32);
            cardEmp.Controls.Add(btnPDF);
            btnPDF.BringToFront();

            var btnXLS = MakeButton("+ EXPORTAR EXCEL", CNaranja, 184, 255, 160, 32);
            cardEmp.Controls.Add(btnXLS);
            btnXLS.BringToFront();

            btnPDF.Click += (s, e) => MessageBox.Show("Exportar a PDF", "Reportes");
            btnXLS.Click += (s, e) => MessageBox.Show("Exportar a Excel", "Reportes");
        }

        // ─────────────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Crea una tarjeta blanca con header azul</summary>
        private Panel MakeCard(int x, int y, int w, int h, string title)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = CBlancoCard
            };
            RoundControl(card, 10);

            // Header
            var hdr = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(w, 30),
                BackColor = CHeaderCard
            };
            card.Controls.Add(hdr);

            var lbl = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                BackColor = Color.Transparent
            };
            hdr.Controls.Add(lbl);

            return card;
        }

        /// <summary>Fila icono + nombre + barra progreso + número</summary>
        private void AddProductRow(Panel card, string name, int value, int max, int index, bool showIcon)
        {
            int yBase = 36 + index * 52;
            int barW = 300;

            // Ícono circular (simulado)
            if (showIcon)
            {
                var ico = new Panel
                {
                    Location = new Point(12, yBase + 2),
                    Size = new Size(22, 22),
                    BackColor = CAzul
                };
                RoundControl(ico, 11);
                // fork-knife symbol
                var icoLbl = new Label
                {
                    Text = "🍽",
                    Font = new Font("Segoe UI Emoji", 8),
                    ForeColor = Color.White,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                };
                ico.Controls.Add(icoLbl);
                card.Controls.Add(ico);
            }

            var lblName = new Label
            {
                Text = name,
                Font = new Font("Segoe UI", 9),
                ForeColor = CTexto,
                AutoSize = true,
                Location = new Point(40, yBase + 4)
            };
            card.Controls.Add(lblName);

            // Barra de fondo (azul)
            var barBg = new Panel
            {
                Location = new Point(40, yBase + 26),
                Size = new Size(barW, 12),
                BackColor = CAzulMedio
            };
            RoundControl(barBg, 6);
            card.Controls.Add(barBg);

            // Barra rellena (naranja)
            int fill = Math.Max(1, (int)(barW * value / (double)max));
            var barFg = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(fill, 12),
                BackColor = CNaranja
            };
            RoundControl(barFg, 6);
            barBg.Controls.Add(barFg);

            var lblVal = new Label
            {
                Text = value.ToString(),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = CTexto,
                AutoSize = true,
                Location = new Point(350, yBase + 24)
            };
            card.Controls.Add(lblVal);
        }

        /// <summary>Fila empleado: icono + nombre + barra + ingreso</summary>
        private void AddEmployeeRow(Panel card, string name, string income, int pct, int index)
        {
            int yBase = 36 + index * 52;
            int barW = 280;

            // Avatar circular
            var avatar = new Panel
            {
                Location = new Point(12, yBase + 2),
                Size = new Size(22, 22),
                BackColor = CAzulMedio
            };
            RoundControl(avatar, 11);
            var avLbl = new Label
            {
                Text = "👤",
                Font = new Font("Segoe UI Emoji", 8),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            avatar.Controls.Add(avLbl);
            card.Controls.Add(avatar);

            var lblName = new Label
            {
                Text = name,
                Font = new Font("Segoe UI", 9),
                ForeColor = CTexto,
                AutoSize = true,
                Location = new Point(40, yBase + 4)
            };
            card.Controls.Add(lblName);

            var lblIncome = new Label
            {
                Text = income,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = CNaranja,
                AutoSize = true,
                Location = new Point(360, yBase + 4)
            };
            card.Controls.Add(lblIncome);

            var barBg = new Panel
            {
                Location = new Point(40, yBase + 26),
                Size = new Size(barW, 12),
                BackColor = CAzulMedio
            };
            RoundControl(barBg, 6);
            card.Controls.Add(barBg);

            int fill = Math.Max(1, (int)(barW * pct / 100.0));
            var barFg = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(fill, 12),
                BackColor = CNaranja
            };
            RoundControl(barFg, 6);
            barBg.Controls.Add(barFg);
        }

        /// <summary>Crea un botón estilizado</summary>
        private Button MakeButton(string text, Color bg, int x, int y, int w, int h)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = bg,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        /// <summary>Dibuja el gráfico de barras de ventas</summary>
        private void DrawBarChart(Graphics g, Panel panel)
        {
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int padL = 42, padR = 10, padT = 10, padB = 30;
            int gW = panel.Width - padL - padR;
            int gH = panel.Height - padT - padB;
            int ox = padL;
            int oy = panel.Height - padB;

            using var penGrid = new Pen(Color.FromArgb(210, 220, 230), 1f);
            using var fntAx = new Font("Segoe UI", 7);
            using var brTxt = new SolidBrush(CTexto);
            using var brBar = new SolidBrush(CNaranja);

            double maxVal = 700;
            int steps = 7;   // $100 cada paso hasta $700

            for (int i = 0; i <= steps; i++)
            {
                float y = oy - (float)(gH * i / steps);
                g.DrawLine(penGrid, ox, y, ox + gW, y);
                string lbl = $"${i * 100}";
                var sz = g.MeasureString(lbl, fntAx);
                g.DrawString(lbl, fntAx, brTxt, ox - sz.Width - 4, y - sz.Height / 2);
            }

            int n = _ventas.Length;
            float slot = gW / (float)n;
            float bw = slot * 0.55f;
            float bo = (slot - bw) / 2f;

            for (int i = 0; i < n; i++)
            {
                float barH = (float)(_ventas[i] / maxVal * gH);
                float bx = ox + i * slot + bo;
                float by = oy - barH;

                // Esquinas superiores redondeadas
                var path = new GraphicsPath();
                float r = 4;
                path.AddArc(bx, by, r * 2, r * 2, 180, 90);
                path.AddArc(bx + bw - r * 2, by, r * 2, r * 2, 270, 90);
                path.AddLine(bx + bw, oy, bx, oy);
                path.CloseFigure();
                g.FillPath(brBar, path);
                path.Dispose();

                var szL = g.MeasureString(_dias[i], fntAx);
                g.DrawString(_dias[i], fntAx, brTxt, bx + bw / 2 - szL.Width / 2, oy + 4);
            }
        }

        /// <summary>Aplica Region redondeada a un control</summary>
        private static void RoundControl(Control ctrl, int radius)
        {
            int w = Math.Max(ctrl.Width, 1);
            int h = Math.Max(ctrl.Height, 1);
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(0, 0, d, d, 180, 90);
            path.AddArc(w - d, 0, d, d, 270, 90);
            path.AddArc(w - d, h - d, d, d, 0, 90);
            path.AddArc(0, h - d, d, d, 90, 90);
            path.CloseAllFigures();
            ctrl.Region = new Region(path);
        }
    }
}