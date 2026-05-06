using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public class FormCerrarPedido : Form
    {
        private readonly decimal _total;
        private readonly int    _numeroMesa;
        private readonly string _folio;
        private TextBox _txtMonto          = null!;
        private TextBox _txtPropina        = null!;
        private TextBox _txtCambio         = null!;
        private Label   _lblDetallePropinaTxt = null!;
        private Label   _lblDetallePropina    = null!;
        private Label   _lblDetalleTotalNeto  = null!;

        public bool    Confirmado        { get; private set; }
        public decimal PropinaConfirmada { get; private set; }

        private static readonly Color ColHeader   = Color.FromArgb(14, 77, 120);
        private static readonly Color ColBg       = Color.FromArgb(217, 217, 217);
        private static readonly Color ColCancel   = Color.FromArgb(128, 128, 128);
        private static readonly Color ColCambioOk = Color.White;
        private static readonly Color ColCambioErr = Color.FromArgb(255, 200, 200);

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        public FormCerrarPedido(int mesaId, decimal total, int numeroMesa = 0, string folio = "")
        {
            _total      = total;
            _numeroMesa = numeroMesa;
            _folio      = folio;
            BuildUI();
        }

        private void BuildUI()
        {
            FormBorderStyle = FormBorderStyle.None;
            Size            = new Size(900, 640);
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = ColBg;
            DoubleBuffered  = true;

            Load += (_, _) =>
            {
                const int r = 16;
                var gp = new GraphicsPath();
                var rc = new Rectangle(0, 0, Width, Height);
                gp.AddArc(rc.Left,        rc.Top,           r * 2, r * 2, 180, 90);
                gp.AddArc(rc.Right - r*2, rc.Top,           r * 2, r * 2, 270, 90);
                gp.AddArc(rc.Right - r*2, rc.Bottom - r*2,  r * 2, r * 2, 0,   90);
                gp.AddArc(rc.Left,        rc.Bottom - r*2,  r * 2, r * 2, 90,  90);
                gp.CloseFigure();
                Region = new Region(gp);
            };

            // ── Title bar ─────────────────────────────────────────────────────
            var titleBar = new Panel { Height = 58, Dock = DockStyle.Top, BackColor = ColHeader };
            var lblTitle = new Label
            {
                Text      = "CERRAR PEDIDO",
                Font      = new Font("Sansation", 16, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock      = DockStyle.Fill
            };
            titleBar.Controls.Add(lblTitle);
            EnableDrag(titleBar);
            EnableDrag(lblTitle);

            // ── Button bar ────────────────────────────────────────────────────
            var btnBar = new Panel { Height = 76, Dock = DockStyle.Bottom, BackColor = ColBg };

            var btnConfirm = MakeBtn("CONFIRMAR Y CERRAR CUENTA", ColHeader,
                                     new Point(40, 15), new Size(490, 48));
            btnConfirm.Click += (_, _) =>
            {
                decimal propina  = ParsePropina(_txtPropina?.Text ?? "");
                string  rawMonto = _txtMonto.Text.Replace("$", "").Trim();

                if (!decimal.TryParse(rawMonto, out decimal monto) || monto - _total - propina < 0)
                {
                    MessageBox.Show(
                        "El monto recibido es insuficiente.\nEl cambio no puede ser negativo.",
                        "Monto insuficiente",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                using var dlg = new FormImpresion();
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Confirmado)
                {
                    PropinaConfirmada = propina;
                    Confirmado        = true;
                    DialogResult      = DialogResult.OK;
                    Close();
                }
            };

            var btnCancel = MakeBtn("CANCELAR", ColCancel,
                                    new Point(672, 15), new Size(188, 48));
            btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

            btnBar.Controls.Add(btnConfirm);
            btnBar.Controls.Add(btnCancel);

            // ── Content ───────────────────────────────────────────────────────
            var content = new Panel { Dock = DockStyle.Fill, BackColor = ColBg };
            PopulateContent(content);

            Controls.Add(content);
            Controls.Add(btnBar);
            Controls.Add(titleBar);
        }

        private void EnableDrag(Control ctrl)
        {
            ctrl.MouseDown += (_, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };
        }

        private void PopulateContent(Panel p)
        {
            const int lx = 44, rx = 856;
            int y = 28;

            // ═══ RESUMEN Y PAGO ═══════════════════════════════════════════════
            int titleY = y;
            SectionHead(p, "RESUMEN Y PAGO", lx, rx, ref y);
            if (_numeroMesa > 0)
                Add(p, Bold($"Mesa : {_numeroMesa}", 530, titleY));
            if (!string.IsNullOrEmpty(_folio))
                Add(p, Bold($"ID : {_folio}", 670, titleY));

            Add(p, Bold("TOTAL A PAGAR:", lx, y));
            Add(p, Plain($"${_total:0}", lx + 242, y));
            Add(p, Bold("CAMBIO:", 528, y));
            y += 42;

            Add(p, Bold("MONTO RECIBIDO:", lx, y));

            _txtMonto = Input(lx + 242, y - 3, 185);
            _txtMonto.Text = "$";
            _txtMonto.TextChanged += RecalcCambio;
            Add(p, _txtMonto);

            _txtCambio = Input(528, y - 3, 185);
            _txtCambio.Text      = "$0.00";
            _txtCambio.ReadOnly  = true;
            _txtCambio.BackColor = ColCambioOk;
            Add(p, _txtCambio);
            y += 64;

            // ═══ DETALLE DE COBRO ═════════════════════════════════════════════
            SectionHead(p, "DETALLE DE COBRO", lx, rx, ref y);

            DetailRow(p, "SUBTOTAL:", $"${_total:0}", lx, rx, ref y);

            // Propina row — dynamic label and value
            _lblDetallePropinaTxt = Bold("PROPINA SUGERIDA (10%):", lx, y);
            Add(p, _lblDetallePropinaTxt);
            _lblDetallePropina = MakeDetailValue($"${_total * 0.10m:0.00}", rx, y);
            Add(p, _lblDetallePropina);
            y += 38;

            // Total neto row — dynamic value
            Add(p, Bold("TOTAL NETO:", lx, y));
            _lblDetalleTotalNeto = MakeDetailValue($"${_total + _total * 0.10m:0.00}", rx, y);
            Add(p, _lblDetalleTotalNeto);
            y += 38;

            y += 28;

            // ═══ PROPINA ══════════════════════════════════════════════════════
            SectionHead(p, "PROPINA", lx, rx, ref y);

            Add(p, Bold("PROPINA RECIBIDA:", lx, y));
            _txtPropina = Input(lx + 242, y - 3, 185);
            _txtPropina.PlaceholderText = "ej: $50 o 10%";
            _txtPropina.TextChanged += RecalcCambio;
            Add(p, _txtPropina);
        }

        // ── Calculation helpers ───────────────────────────────────────────────

        private decimal ParsePropina(string text)
        {
            text = text.Trim();
            if (text.EndsWith("%"))
            {
                if (decimal.TryParse(text[..^1].Trim(), out decimal pct))
                    return _total * pct / 100m;
            }
            else
            {
                if (decimal.TryParse(text.Replace("$", "").Trim(), out decimal amt))
                    return amt;
            }
            return 0m;
        }

        private void RecalcCambio(object? sender, EventArgs e)
        {
            string  rawMonto = _txtMonto.Text.Replace("$", "").Trim();
            decimal propina  = ParsePropina(_txtPropina?.Text ?? "");

            // Update detail section labels
            _lblDetallePropina.Text    = $"${propina:0.00}";
            _lblDetalleTotalNeto.Text  = $"${_total + propina:0.00}";

            string rawPropina = (_txtPropina?.Text ?? "").Trim();
            if (rawPropina.EndsWith("%") &&
                decimal.TryParse(rawPropina[..^1].Trim(), out decimal pct) && pct > 0)
                _lblDetallePropinaTxt.Text = $"PROPINA ({pct:0}%):";
            else if (propina > 0)
                _lblDetallePropinaTxt.Text = "PROPINA RECIBIDA:";
            else
                _lblDetallePropinaTxt.Text = "PROPINA SUGERIDA (10%):";

            // Update cambio
            if (decimal.TryParse(rawMonto, out decimal monto))
            {
                decimal cambio = monto - _total - propina;
                bool    ok     = cambio >= 0;
                _txtCambio.Text      = ok ? $"${cambio:0.00}" : $"-${Math.Abs(cambio):0.00}";
                _txtCambio.BackColor = ok ? ColCambioOk : ColCambioErr;
                _txtCambio.ForeColor = ok ? Color.FromArgb(20, 20, 20) : Color.FromArgb(180, 0, 0);
            }
            else
            {
                _txtCambio.Text      = "$0.00";
                _txtCambio.BackColor = ColCambioOk;
                _txtCambio.ForeColor = Color.FromArgb(20, 20, 20);
            }
        }

        // ── Layout helpers ────────────────────────────────────────────────────

        private static void SectionHead(Panel p, string title, int lx, int rx, ref int y)
        {
            Add(p, Bold(title, lx, y));
            y += 34;
            Add(p, new Panel
            {
                Location  = new Point(lx, y),
                Size      = new Size(rx - lx, 1),
                BackColor = Color.FromArgb(90, 90, 90)
            });
            y += 18;
        }

        private static void DetailRow(Panel p, string label, string value, int lx, int rx, ref int y)
        {
            Add(p, Bold(label, lx, y));
            Add(p, MakeDetailValue(value, rx, y));
            y += 38;
        }

        private static Label MakeDetailValue(string text, int rx, int y)
        {
            var val = Plain(text, 0, y);
            val.AutoSize  = false;
            val.Width     = 95;
            val.Location  = new Point(rx - 95, y);
            val.TextAlign = ContentAlignment.MiddleRight;
            return val;
        }

        private static Label Bold(string text, int x, int y) => new()
        {
            Text      = text,
            Font      = new Font("Sansation", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(20, 20, 20),
            AutoSize  = true,
            Location  = new Point(x, y),
            BackColor = Color.Transparent
        };

        private static Label Plain(string text, int x, int y) => new()
        {
            Text      = text,
            Font      = new Font("Sansation", 11),
            ForeColor = Color.FromArgb(20, 20, 20),
            AutoSize  = true,
            Location  = new Point(x, y),
            BackColor = Color.Transparent
        };

        private static TextBox Input(int x, int y, int w) => new()
        {
            Location    = new Point(x, y),
            Size        = new Size(w, 30),
            Font        = new Font("Sansation", 11),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor   = Color.White
        };

        private static Button MakeBtn(string text, Color bg, Point loc, Size sz)
        {
            var btn = new Button
            {
                Text      = text,
                Font      = new Font("Sansation", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = bg,
                FlatStyle = FlatStyle.Flat,
                Location  = loc,
                Size      = sz,
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            ApplyRounded(btn, 6);
            return btn;
        }

        private static void Add(Panel p, Control c) => p.Controls.Add(c);

        private static void ApplyRounded(Control ctrl, int r)
        {
            ctrl.Resize += (_, _) =>
            {
                if (ctrl.Width <= 0 || ctrl.Height <= 0) return;
                var gp = new GraphicsPath();
                var rc = new Rectangle(0, 0, ctrl.Width, ctrl.Height);
                gp.AddArc(rc.Left,        rc.Top,           r * 2, r * 2, 180, 90);
                gp.AddArc(rc.Right - r*2, rc.Top,           r * 2, r * 2, 270, 90);
                gp.AddArc(rc.Right - r*2, rc.Bottom - r*2,  r * 2, r * 2, 0,   90);
                gp.AddArc(rc.Left,        rc.Bottom - r*2,  r * 2, r * 2, 90,  90);
                gp.CloseFigure();
                ctrl.Region = new Region(gp);
            };
        }
    }
}
