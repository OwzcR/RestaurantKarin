using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public partial class FormReportes : UserControl
    {
        // ── Paleta de colores ──────────────────────────────────────────────
        private static readonly Color CAzul = Color.FromArgb(30, 80, 120);
        private static readonly Color CAzulMedio = Color.FromArgb(45, 105, 150);
        private static readonly Color CNaranja = Color.FromArgb(240, 140, 55);
        private static readonly Color CTexto = Color.FromArgb(20, 45, 75);
        private static readonly Color CFondo = Color.FromArgb(200, 225, 240);
        private static readonly Color CBlancoCard = Color.White;
        private static readonly Color CHeaderCard = Color.FromArgb(28, 78, 118);

        // Datos del gráfico
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
            // ── BackColor Transparent — hereda del contenedor padre (FormBase) ──
            this.BackColor = Color.Transparent;
            this.Padding = new Padding(16);

            // ── Panel scroll que ocupa todo el UserControl ─────────────────
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent  // transparente para ver el fondo
            };
            this.Controls.Add(scroll);

            // ── Panel envolvente (Fill) — sirve para centrar ───────────────
            var wrapper = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            scroll.Controls.Add(wrapper);

            // ── Panel de contenido con ancho fijo ─────────────────────────
            const int W = 1080;   // ancho total del contenido

            var inner = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                Margin = new Padding(0),
                Width = W
            };

            // Recentrar cada vez que cambie el tamaño de la ventana
            wrapper.Resize += (s, e) =>
            {
                inner.Location = new Point(Math.Max(0, (wrapper.Width - W) / 2), 12);
            };
            wrapper.Controls.Add(inner);

            // Centrado inicial (antes del primer Resize)
            inner.Location = new Point(Math.Max(0, (wrapper.Width - W) / 2), 12);


            // 1. BARRA DE FILTROS
            var pnlFiltros = new Panel
            {
                Size = new Size(W, 52),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 14)
            };
            inner.Controls.Add(pnlFiltros);

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

            pnlFiltros.Controls.Add(MakeButton("VER REPORTE", CAzul, 418, 10, 130, 32));

            // 2. FILA 1 — Ventas | Productos Más Vendidos
            var fila1 = new Panel
            {
                Size = new Size(W, 295),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 14)
            };
            inner.Controls.Add(fila1);

            // Tarjeta Ventas
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

            cardVentas.Controls.Add(new Label
            {
                Text = "$3,250",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = CTexto,
                AutoSize = true,
                Location = new Point(14, 255)
            });
            cardVentas.Controls.Add(new Label
            {
                Text = "Total",
                Font = new Font("Segoe UI", 10),
                ForeColor = CTexto,
                AutoSize = true,
                Location = new Point(100, 262)
            });

            // Tarjeta Productos Más Vendidos
            var cardProd = MakeCard(534, 0, 526, 295, "Productos Más Vendidos");
            fila1.Controls.Add(cardProd);

            string[] platillos = { "Platillo 1", "Platillo 2", "Platillo 3", "Platillo 4" };
            int[] cantidades = { 48, 25, 45, 10 };
            for (int i = 0; i < platillos.Length; i++)
                AddProductRow(cardProd, platillos[i], cantidades[i], 50, i);

            cardProd.Controls.Add(new Label
            {
                Text = "128 Ventas Totales",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = CTexto,
                AutoSize = true,
                Location = new Point(14, 258)
            });

            // ═══════════════════════════════════════════════════════════════
            // 3. FILA 2 — Consumo Inventario | Ingresos por Empleado
            // ═══════════════════════════════════════════════════════════════
            var fila2 = new Panel
            {
                Size = new Size(W, 295),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 14)
            };
            inner.Controls.Add(fila2);

            // Tarjeta Consumo Inventario
            var cardInv = MakeCard(0, 0, 520, 295, "Consumo de Inventario");
            fila2.Controls.Add(cardInv);

            string[] insumos = { "Insumo 1", "Insumo 2", "Insumo 3", "Insumo 4" };
            int[] consumos = { 24, 18, 16, 10 };
            for (int i = 0; i < insumos.Length; i++)
                AddProductRow(cardInv, insumos[i], consumos[i], 28, i);

            cardInv.Controls.Add(new Label
            {
                Text = "67% del Inventario Consumido",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = CTexto,
                AutoSize = true,
                Location = new Point(14, 258)
            });

            // Tarjeta Ingresos por Empleado
            var cardEmp = MakeCard(534, 0, 526, 295, "Ingresos por Empleado");
            fila2.Controls.Add(cardEmp);

            string[] nombres = { "Felipe", "José", "Carla", "Román" };
            string[] ingresos = { "$1,320.00", "$780.00", "$650.00", "$500.00" };
            int[] porcentajes = { 90, 53, 44, 34 };
            for (int i = 0; i < nombres.Length; i++)
                AddEmployeeRow(cardEmp, nombres[i], ingresos[i], porcentajes[i], i);

            var btnPDF = MakeButton("+ EXPORTAR PDF", CAzul, 14, 255, 160, 32);
            var btnXLS = MakeButton("+ EXPORTAR EXCEL", CNaranja, 184, 255, 160, 32);
            cardEmp.Controls.Add(btnPDF);
            cardEmp.Controls.Add(btnXLS);
            btnPDF.BringToFront();
            btnXLS.BringToFront();

            btnPDF.Click += (s, e) => MessageBox.Show("Exportar a PDF", "Reportes");
            btnXLS.Click += (s, e) => MessageBox.Show("Exportar a Excel", "Reportes");
        }

        // ─────────────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────────────

        private Panel MakeCard(int x, int y, int w, int h, string title)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = CBlancoCard
            };
            RoundControl(card, 10);

            var hdr = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(w, 30),
                BackColor = CHeaderCard
            };
            card.Controls.Add(hdr);

            hdr.Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                BackColor = Color.Transparent
            });

            return card;
        }

        private void AddProductRow(Panel card, string name, int value, int max, int index)
        {
            int yBase = 36 + index * 52;
            int barW = 300;

            var ico = new Panel { Location = new Point(12, yBase + 2), Size = new Size(22, 22), BackColor = CAzul };
            RoundControl(ico, 11);
            ico.Controls.Add(new Label { Text = "🍽", Font = new Font("Segoe UI Emoji", 8), ForeColor = Color.White, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });
            card.Controls.Add(ico);

            card.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 9), ForeColor = CTexto, AutoSize = true, Location = new Point(40, yBase + 4) });

            var barBg = new Panel { Location = new Point(40, yBase + 26), Size = new Size(barW, 12), BackColor = CAzulMedio };
            RoundControl(barBg, 6);
            card.Controls.Add(barBg);

            int fill = Math.Max(1, (int)(barW * value / (double)max));
            var barFg = new Panel { Location = new Point(0, 0), Size = new Size(fill, 12), BackColor = CNaranja };
            RoundControl(barFg, 6);
            barBg.Controls.Add(barFg);

            card.Controls.Add(new Label { Text = value.ToString(), Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = CTexto, AutoSize = true, Location = new Point(350, yBase + 24) });
        }

        private void AddEmployeeRow(Panel card, string name, string income, int pct, int index)
        {
            int yBase = 36 + index * 52;
            int barW = 280;

            var avatar = new Panel { Location = new Point(12, yBase + 2), Size = new Size(22, 22), BackColor = CAzulMedio };
            RoundControl(avatar, 11);
            avatar.Controls.Add(new Label { Text = "👤", Font = new Font("Segoe UI Emoji", 8), ForeColor = Color.White, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });
            card.Controls.Add(avatar);

            card.Controls.Add(new Label { Text = name, Font = new Font("Segoe UI", 9), ForeColor = CTexto, AutoSize = true, Location = new Point(40, yBase + 4) });
            card.Controls.Add(new Label { Text = income, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = CNaranja, AutoSize = true, Location = new Point(360, yBase + 4) });

            var barBg = new Panel { Location = new Point(40, yBase + 26), Size = new Size(barW, 12), BackColor = CAzulMedio };
            RoundControl(barBg, 6);
            card.Controls.Add(barBg);

            int fill = Math.Max(1, (int)(barW * pct / 100.0));
            var barFg = new Panel { Location = new Point(0, 0), Size = new Size(fill, 12), BackColor = CNaranja };
            RoundControl(barFg, 6);
            barBg.Controls.Add(barFg);
        }

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

        private void DrawBarChart(Graphics g, Panel panel)
        {
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int padL = 42, padR = 10, padT = 10, padB = 30;
            int gW = panel.Width - padL - padR;
            int gH = panel.Height - padT - padB;
            int ox = padL, oy = panel.Height - padB;

            using var penGrid = new Pen(Color.FromArgb(210, 220, 230), 1f);
            using var fntAx = new Font("Segoe UI", 7);
            using var brTxt = new SolidBrush(CTexto);
            using var brBar = new SolidBrush(CNaranja);

            double maxVal = 700;
            int steps = 7;

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