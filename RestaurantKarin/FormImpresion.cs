using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public class FormImpresion : Form
    {
        public bool EnviarCocina   { get; private set; }
        public bool ImprimirTicket { get; private set; }
        public bool Confirmado     { get; private set; }

        private Button _btnCocina  = null!;
        private Button _btnTicket  = null!;
        private bool   _selCocina;
        private bool   _selTicket;

        private static readonly Color ColConfirm = Color.FromArgb(14, 77, 120);
        private static readonly Color ColBg      = Color.FromArgb(235, 235, 235);
        private static readonly Color ColOptIdle = Color.FromArgb(210, 210, 210);
        private static readonly Color ColOptSel  = Color.FromArgb(14, 77, 120);
        private static readonly Color ColCancel  = Color.FromArgb(200, 200, 200);

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        public FormImpresion()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            FormBorderStyle = FormBorderStyle.None;
            Size            = new Size(800, 290);
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = ColBg;
            DoubleBuffered  = true;

            Load   += (_, _) => ApplyFormRegion();
            Resize += (_, _) => ApplyFormRegion();

            var p = new Panel { Dock = DockStyle.Fill, BackColor = ColBg };

            // ── Title ──────────────────────────────────────────────────────────
            var lblTitle = new Label
            {
                Text      = "Seleccionar destino de impresión",
                Font      = new Font("Sansation", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 20, 20),
                AutoSize  = true,
                Location  = new Point(30, 22),
                BackColor = Color.Transparent
            };
            p.Controls.Add(lblTitle);

            // ── Separator ─────────────────────────────────────────────────────
            p.Controls.Add(new Panel
            {
                Location  = new Point(30, 55),
                Size      = new Size(740, 1),
                BackColor = Color.FromArgb(100, 100, 100)
            });

            // ── Option buttons ────────────────────────────────────────────────
            _btnCocina = MakeOptionBtn("Enviar a cocina",         new Point(30,  70));
            _btnTicket = MakeOptionBtn("Imprimir ticket completo", new Point(410, 70));

            _btnCocina.Click  += (_, _) => Toggle(ref _selCocina,  _btnCocina);
            _btnTicket.Click  += (_, _) => Toggle(ref _selTicket, _btnTicket);

            p.Controls.Add(_btnCocina);
            p.Controls.Add(_btnTicket);

            // ── Confirm button ────────────────────────────────────────────────
            var btnConfirm = new Button
            {
                Text      = "CONFIRMAR Y ENVIAR",
                Font      = new Font("Sansation", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = ColConfirm,
                FlatStyle = FlatStyle.Flat,
                Location  = new Point(30, 184),
                Size      = new Size(555, 54),
                Cursor    = Cursors.Hand
            };
            btnConfirm.FlatAppearance.BorderSize = 0;
            ApplyRounded(btnConfirm, 10);
            btnConfirm.Click += (_, _) =>
            {
                EnviarCocina   = _selCocina;
                ImprimirTicket = _selTicket;
                Confirmado     = true;
                DialogResult   = DialogResult.OK;
                Close();
            };
            p.Controls.Add(btnConfirm);

            // ── Cancel button ─────────────────────────────────────────────────
            var btnCancel = new Button
            {
                Text      = "CANCELAR",
                Font      = new Font("Sansation", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                BackColor = ColCancel,
                FlatStyle = FlatStyle.Flat,
                Location  = new Point(605, 184),
                Size      = new Size(165, 54),
                Cursor    = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize  = 1;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(150, 150, 150);
            ApplyRounded(btnCancel, 10);
            btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
            p.Controls.Add(btnCancel);

            // Draggable from background and title
            EnableDrag(p);
            EnableDrag(lblTitle);

            Controls.Add(p);
        }

        private Button MakeOptionBtn(string text, Point loc)
        {
            var btn = new Button
            {
                Text      = text,
                Font      = new Font("Sansation", 12),
                ForeColor = Color.FromArgb(30, 30, 30),
                BackColor = ColOptIdle,
                FlatStyle = FlatStyle.Flat,
                Location  = loc,
                Size      = new Size(360, 96),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize  = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(170, 170, 170);
            ApplyRounded(btn, 12);
            return btn;
        }

        private static void Toggle(ref bool state, Button btn)
        {
            state = !state;
            btn.BackColor = state ? ColOptSel  : ColOptIdle;
            btn.ForeColor = state ? Color.White : Color.FromArgb(30, 30, 30);
            btn.FlatAppearance.BorderColor = state
                ? Color.FromArgb(14, 77, 120)
                : Color.FromArgb(170, 170, 170);
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

        private void ApplyFormRegion()
        {
            const int r = 14;
            var gp = new GraphicsPath();
            var rc = new Rectangle(0, 0, Width, Height);
            gp.AddArc(rc.Left,         rc.Top,          r * 2, r * 2, 180, 90);
            gp.AddArc(rc.Right - r*2,  rc.Top,          r * 2, r * 2, 270, 90);
            gp.AddArc(rc.Right - r*2,  rc.Bottom - r*2, r * 2, r * 2, 0,   90);
            gp.AddArc(rc.Left,         rc.Bottom - r*2, r * 2, r * 2, 90,  90);
            gp.CloseFigure();
            Region = new Region(gp);
        }

        private static void ApplyRounded(Control ctrl, int r)
        {
            ctrl.Resize += (_, _) =>
            {
                if (ctrl.Width <= 0 || ctrl.Height <= 0) return;
                var gp = new GraphicsPath();
                var rc = new Rectangle(0, 0, ctrl.Width, ctrl.Height);
                gp.AddArc(rc.Left,         rc.Top,          r * 2, r * 2, 180, 90);
                gp.AddArc(rc.Right - r*2,  rc.Top,          r * 2, r * 2, 270, 90);
                gp.AddArc(rc.Right - r*2,  rc.Bottom - r*2, r * 2, r * 2, 0,   90);
                gp.AddArc(rc.Left,         rc.Bottom - r*2, r * 2, r * 2, 90,  90);
                gp.CloseFigure();
                ctrl.Region = new Region(gp);
            };
        }
    }
}
