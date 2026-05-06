using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public class FormDetallesMesa : Form
    {
        [DllImport("user32.dll")] private static extern int  SendMessage(IntPtr h, int msg, int w, int l);
        [DllImport("user32.dll")] private static extern bool ReleaseCapture();

        private const string Conn = "Data Source=karin_pos.db;Version=3;";

        // ── Palette ───────────────────────────────────────────────────────────
        private static readonly Color ColBg        = Color.White;
        private static readonly Color ColBorder    = Color.FromArgb(210, 210, 210);
        private static readonly Color ColSeparator = Color.FromArgb(220, 220, 220);
        private static readonly Color ColBtnGray   = Color.FromArgb(195, 195, 195);
        private static readonly Color ColBtnNavy   = Color.FromArgb(13,  41,  78);
        private static readonly Color ColHeader    = Color.FromArgb(14,  77, 120);
        private static readonly Color ColText      = Color.FromArgb(30,  30,  30);
        private static readonly Color ColTextGray  = Color.FromArgb(110, 110, 110);
        private static readonly Color ColRowLine   = Color.FromArgb(228, 228, 228);

        // ── Fonts ─────────────────────────────────────────────────────────────
        private static readonly Font FntTitle   = new("Sansation", 13, FontStyle.Bold);
        private static readonly Font FntLabel   = new("Sansation", 10, FontStyle.Bold);
        private static readonly Font FntValue   = new("Sansation", 10, FontStyle.Regular);
        private static readonly Font FntTHead   = new("Sansation",  9, FontStyle.Bold);
        private static readonly Font FntRow     = new("Sansation",  9, FontStyle.Regular);
        private static readonly Font FntBtn     = new("Sansation", 10, FontStyle.Bold);
        private static readonly Font FntTotals  = new("Sansation", 10, FontStyle.Bold);

        // ── Table column X positions ──────────────────────────────────────────
        private const int CxProd = 8;
        private const int CxCant = 240;
        private const int CxSub  = 330;
        private const int CxObs  = 420;
        private const int CxAct  = 576;

        // ── Data ──────────────────────────────────────────────────────────────
        private readonly MesaModel  _mesa;
        private List<DetalleItem>   _items = new();
        private bool                _isClosed = false;

        // ── Live controls ─────────────────────────────────────────────────────
        private Panel _tableBody   = null!;
        private Label _lblSubtotal = null!;
        private Label _lblPropina  = null!;
        private Label _lblTotal    = null!;

        // ── Saved totals for closed accounts ─────────────────────────────────
        private decimal _savedSubtotal = -1;
        private decimal _savedPropina  =  0;
        private decimal _savedTotal    = -1;

        private sealed class DetalleItem
        {
            public int     Id            { get; init; }
            public string  Producto      { get; init; } = "";
            public int     Cantidad      { get; init; }
            public decimal PrecioUnit    { get; init; }
            public decimal Subtotal      { get; init; }
            public string  Notas         { get; init; } = "";
        }

        // ─────────────────────────────────────────────────────────────────────
        public FormDetallesMesa(MesaModel mesa)
        {
            _mesa = mesa;
            BuildUI();
            CargarDatos();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  UI construction
        // ─────────────────────────────────────────────────────────────────────
        private void BuildUI()
        {
            FormBorderStyle = FormBorderStyle.None;
            Size            = new Size(720, 620);
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = ColBg;
            DoubleBuffered  = true;

            Load      += (_, _) => ApplyRounded(this, 16);
            MouseDown += Drag;

            const int mx = 20, my = 16;
            int innerW = Width - mx * 2;   // 680

            // ── Title tab ─────────────────────────────────────────────────────
            var titleTab = new Panel
            {
                Location  = new Point(mx, my),
                Size      = new Size(130, 34),
                BackColor = ColBg
            };
            titleTab.Paint += PaintBordered;
            ApplyRounded(titleTab, 8);

            var lblTitle = new Label
            {
                Text      = $"Mesa {_mesa.NumeroMesa}:",
                Font      = FntTitle,
                ForeColor = ColText,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            titleTab.Controls.Add(lblTitle);
            titleTab.MouseDown += Drag;
            lblTitle.MouseDown += Drag;
            Controls.Add(titleTab);

            // ── Info card ─────────────────────────────────────────────────────
            int cardY   = my + titleTab.Height + 10;
            var infoCard = new Panel
            {
                Location  = new Point(mx, cardY),
                Size      = new Size(innerW, 88),
                BackColor = ColBg
            };
            infoCard.Paint += PaintBordered;
            ApplyRounded(infoCard, 10);
            infoCard.MouseDown += Drag;

            // Determine if account is closed
            _isClosed = _mesa.FechaCierre.HasValue && !_mesa.Activa;
            
            TimeSpan elapsed;
            if (_isClosed)
            {
                // Calculate time from opening to closing
                elapsed = _mesa.FechaCierre.Value - _mesa.HoraLlegada;
            }
            else
            {
                // Calculate time from opening to now
                elapsed = DateTime.Now - _mesa.HoraLlegada;
            }
            
            string   elapsedS = elapsed.TotalHours >= 1
                                    ? $"{(int)elapsed.TotalHours}h {elapsed.Minutes}m"
                                    : $"{elapsed.Minutes}m";
            string   estado   = _isClosed ? "Pagada" : "Ocupada";
            string   mesero   = string.IsNullOrWhiteSpace(Sesion.Nombre) ? "—" : Sesion.Nombre;

            AddInfoColumn(infoCard, "Estado:",              estado,   14);
            AddInfoColumn(infoCard, "Mesero:",              mesero,    230);
            AddInfoColumn(infoCard, "Tiempo transcurrido:", elapsedS,  450);
            Controls.Add(infoCard);

            // ── Table header ──────────────────────────────────────────────────
            int tHeadY = cardY + infoCard.Height + 12;
            var tHead  = new Panel
            {
                Location  = new Point(mx, tHeadY),
                Size      = new Size(innerW, 32),
                BackColor = ColBg
            };
            tHead.Paint += (_, e) =>
            {
                using var pen = new Pen(ColBorder);
                e.Graphics.DrawLine(pen, 0, 0, tHead.Width, 0);
                e.Graphics.DrawLine(pen, 0, tHead.Height - 1, tHead.Width, tHead.Height - 1);
            };
            AddColHeader(tHead, "PRODUCTO",      CxProd, CxCant - CxProd - 4, ContentAlignment.MiddleLeft);
            AddColHeader(tHead, "CANTIDAD",      CxCant, CxSub  - CxCant - 4, ContentAlignment.MiddleCenter);
            AddColHeader(tHead, "SUBTOTAL",      CxSub,  CxObs  - CxSub  - 4, ContentAlignment.MiddleCenter);
            AddColHeader(tHead, "OBSERVACIONES", CxObs,  CxAct  - CxObs  - 4, ContentAlignment.MiddleLeft);
            Controls.Add(tHead);

            // ── Table body ────────────────────────────────────────────────────
            int tBodyY = tHeadY + tHead.Height;
            _tableBody = new Panel
            {
                Location   = new Point(mx, tBodyY),
                Size       = new Size(innerW, 200),
                AutoScroll = true,
                BackColor  = ColBg
            };
            EnableDoubleBuffer(_tableBody);
            _tableBody.SizeChanged += (_, _) =>
            {
                int w = _tableBody.ClientSize.Width;
                foreach (Control c in _tableBody.Controls) c.Width = w;
            };
            Controls.Add(_tableBody);

            // bottom border
            Controls.Add(new Panel
            {
                Location  = new Point(mx, tBodyY + _tableBody.Height),
                Size      = new Size(innerW, 1),
                BackColor = ColBorder
            });

            // ── Totals ────────────────────────────────────────────────────────
            int totY = tBodyY + _tableBody.Height + 14;
            int lblX = Width - mx - 330;
            int valX = Width - mx - 68;

            string propinaLabel = _isClosed ? "PROPINA:" : "PROPINA SUGERIDA (10%):";
            AddTotalRow("SUBTOTAL:",    lblX, valX, totY,       out _lblSubtotal);
            AddTotalRow(propinaLabel,   lblX, valX, totY + 28,  out _lblPropina);
            AddTotalRow("TOTAL:",       lblX, valX, totY + 56,  out _lblTotal);

            // ── "+ Agregar Pedido" button ─────────────────────────────────────
            var btnAgrPed = BuildAgregarBtn();
            btnAgrPed.Location = new Point(mx, totY + 88);
            
            if (_isClosed)
            {
                btnAgrPed.Enabled = false;
                btnAgrPed.BackColor = Color.FromArgb(200, 200, 200);
                btnAgrPed.Cursor = Cursors.No;
            }
            
            void DoAgregar(object? s, EventArgs e)
            {
                if (_isClosed) return;
                using var frm = new FormAgregarPedido(_mesa.Id, _mesa.IdCuenta);
                frm.ShowDialog(this);
                CargarDatos();
            }
            btnAgrPed.Click += DoAgregar;
            foreach (Control ch in btnAgrPed.Controls) ch.Click += DoAgregar;
            Controls.Add(btnAgrPed);

            // ── Separator above button bar ────────────────────────────────────
            int sepY = Height - 74;
            Controls.Add(new Panel
            {
                Location  = new Point(0, sepY),
                Size      = new Size(Width, 1),
                BackColor = ColSeparator
            });

            // ── Button bar ────────────────────────────────────────────────────
            int btnBarY = sepY + 1;
            var btnBar  = new Panel
            {
                Location  = new Point(0, btnBarY),
                Size      = new Size(Width, Height - btnBarY),
                BackColor = ColBg
            };
            btnBar.MouseDown += Drag;

            // Gray buttons (left side)
            var btnCancelar = MakeGrayBtn("Cancelar",      new Size(108, 44));
            var btnImprimir = MakeGrayBtn("Imprimir",      new Size(108, 44));
            var btnCerrar   = MakeGrayBtn("Cerrar cuenta", new Size(128, 44));

            btnCancelar.Location = new Point(mx, 14);
            btnImprimir.Location = new Point(mx + 108 + 10, 14);
            btnCerrar.Location   = new Point(mx + 108 + 10 + 108 + 10, 14);

            btnCancelar.Click += (_, _) => Close();
            btnImprimir.Click += (_, _) => { using var f = new FormImpresion(); f.ShowDialog(this); };
            btnCerrar.Click   += OnCerrarCuenta;
            
            // Disable close button if already closed
            if (_isClosed)
            {
                btnCerrar.Enabled = false;
                btnCerrar.BackColor = Color.FromArgb(150, 150, 150);
                btnCerrar.Cursor = Cursors.No;
            }

            // "+ Guardar Cambios" button (navy, right side, split-style)
            var btnGuardar = BuildGuardarBtn();
            btnGuardar.Location = new Point(Width - mx - btnGuardar.Width, 14);
            
            // Disable save button if already closed
            if (_isClosed)
            {
                btnGuardar.Enabled = false;
                btnGuardar.BackColor = Color.FromArgb(100, 100, 100);
                btnGuardar.Cursor = Cursors.No;
            }

            btnBar.Controls.AddRange(new Control[] { btnCancelar, btnImprimir, btnCerrar, btnGuardar });
            Controls.Add(btnBar);
        }

        // Builds the "+ Guardar Cambios" panel-button matching the screenshot
        private Panel BuildGuardarBtn()
        {
            var pnl = new Panel
            {
                Size      = new Size(158, 44),
                BackColor = ColBtnNavy,
                Cursor    = Cursors.Hand
            };
            ApplyRounded(pnl, 8);

            // Hover effect
            pnl.MouseEnter += (_, _) => pnl.BackColor = Color.FromArgb(20, 55, 100);
            pnl.MouseLeave += (_, _) => pnl.BackColor = ColBtnNavy;

            var lblPlus = new Label
            {
                Text      = "+",
                Font      = new Font("Sansation", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location  = new Point(10, 0),
                Size      = new Size(28, 44),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand
            };

            var lblText = new Label
            {
                Text      = "Guardar\nCambios",
                Font      = new Font("Sansation", 9, FontStyle.Bold),
                ForeColor = Color.White,
                Location  = new Point(44, 0),
                Size      = new Size(106, 44),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand
            };

            void DoClick(object? s, EventArgs e) { DialogResult = DialogResult.OK; Close(); }
            pnl.Click    += DoClick;
            lblPlus.Click += DoClick;
            lblText.Click += DoClick;

            pnl.Controls.Add(lblPlus);
            pnl.Controls.Add(lblText);
            return pnl;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Data
        // ─────────────────────────────────────────────────────────────────────
        private void CargarDatos()
        {
            _items.Clear();
            try
            {
                using var con = new SQLiteConnection(Conn);
                con.Open();

                using var cmd = new SQLiteCommand(@"
                    SELECT d.id_detalle, p.nombre, d.cantidad,
                           d.precio_unitario, d.subtotal, d.notas
                    FROM   detalle_cuenta d
                    JOIN   producto p ON p.id_producto = d.id_producto
                    WHERE  d.id_cuenta = @id
                    ORDER  BY d.id_detalle;", con);
                cmd.Parameters.AddWithValue("@id", _mesa.IdCuenta);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                    _items.Add(new DetalleItem
                    {
                        Id         = Convert.ToInt32(r["id_detalle"]),
                        Producto   = r["nombre"].ToString()!,
                        Cantidad   = Convert.ToInt32(r["cantidad"]),
                        PrecioUnit = Convert.ToDecimal(r["precio_unitario"]),
                        Subtotal   = Convert.ToDecimal(r["subtotal"]),
                        Notas      = r["notas"]?.ToString() ?? ""
                    });

                if (_isClosed)
                {
                    using var cmd2 = new SQLiteCommand(@"
                        SELECT subtotal, cargo_servicio_extra, total
                        FROM   cuenta WHERE id_cuenta = @id;", con);
                    cmd2.Parameters.AddWithValue("@id", _mesa.IdCuenta);
                    using var r2 = cmd2.ExecuteReader();
                    if (r2.Read())
                    {
                        _savedSubtotal = Convert.ToDecimal(r2["subtotal"]);
                        _savedPropina  = Convert.ToDecimal(r2["cargo_servicio_extra"]);
                        _savedTotal    = Convert.ToDecimal(r2["total"]);
                    }
                }
            }
            catch { }
            RenderTable();
        }

        private void RenderTable()
        {
            _tableBody.SuspendLayout();
            _tableBody.Controls.Clear();

            int w = Math.Max(1, _tableBody.ClientSize.Width);
            int y = 0;
            foreach (var item in _items)
            {
                var row = BuildRow(item, y, w);
                _tableBody.Controls.Add(row);
                y += row.Height;
            }

            _tableBody.ResumeLayout(true);
            UpdateTotals();
        }

        private Panel BuildRow(DetalleItem item, int top, int w)
        {
            const int rowH = 32;
            var row = new Panel
            {
                Location  = new Point(0, top),
                Size      = new Size(w, rowH),
                BackColor = ColBg
            };
            row.Paint += (_, e) =>
            {
                using var pen = new Pen(ColRowLine);
                e.Graphics.DrawLine(pen, 0, row.Height - 1, row.Width, row.Height - 1);
            };

            row.Controls.Add(MakeCell(item.Producto,             CxProd, CxCant - CxProd - 4, rowH, ContentAlignment.MiddleLeft,   ColText));
            row.Controls.Add(MakeCell(item.Cantidad.ToString(),  CxCant, CxSub  - CxCant - 4, rowH, ContentAlignment.MiddleCenter, ColText));
            row.Controls.Add(MakeCell($"${item.Subtotal:0}",     CxSub,  CxObs  - CxSub  - 4, rowH, ContentAlignment.MiddleCenter, ColText));
            row.Controls.Add(MakeCell(item.Notas,                CxObs,  CxAct  - CxObs  - 4, rowH, ContentAlignment.MiddleLeft,   ColTextGray));

            // Edit icon button
            var btnEdit = new Button
            {
                Text      = "✏",
                Font      = new Font("Segoe UI Emoji", 10),
                ForeColor = Color.FromArgb(80, 80, 80),
                BackColor = Color.FromArgb(235, 235, 235),
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(28, 24),
                Location  = new Point(CxAct, (rowH - 24) / 2),
                Cursor    = Cursors.Hand
            };
            btnEdit.FlatAppearance.BorderSize        = 0;
            btnEdit.FlatAppearance.MouseOverBackColor = Color.FromArgb(210, 210, 210);
            ApplyRounded(btnEdit, 4);
            btnEdit.Click += (_, _) => 
            { 
                if (!_isClosed) EditarItem(item);
            };
            
            if (_isClosed)
            {
                btnEdit.Enabled = false;
                btnEdit.BackColor = Color.FromArgb(200, 200, 200);
                btnEdit.Cursor = Cursors.No;
            }

            // Delete icon button
            var btnDel = new Button
            {
                Text      = "🗑",
                Font      = new Font("Segoe UI Emoji", 10),
                ForeColor = Color.FromArgb(180, 50, 50),
                BackColor = Color.FromArgb(235, 235, 235),
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(28, 24),
                Location  = new Point(CxAct + 34, (rowH - 24) / 2),
                Cursor    = Cursors.Hand
            };
            btnDel.FlatAppearance.BorderSize        = 0;
            btnDel.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 210, 210);
            ApplyRounded(btnDel, 4);
            btnDel.Click += (_, _) => 
            { 
                if (!_isClosed) EliminarItem(item);
            };
            
            if (_isClosed)
            {
                btnDel.Enabled = false;
                btnDel.BackColor = Color.FromArgb(200, 200, 200);
                btnDel.Cursor = Cursors.No;
            }

            row.Controls.Add(btnEdit);
            row.Controls.Add(btnDel);
            return row;
        }

        private static Label MakeCell(string text, int x, int w, int h, ContentAlignment align, Color fg) => new()
        {
            Text      = text,
            Font      = FntRow,
            ForeColor = fg,
            Location  = new Point(x, 0),
            Size      = new Size(w, h),
            TextAlign = align,
            BackColor = Color.Transparent
        };

        private void UpdateTotals()
        {
            if (_isClosed && _savedTotal > 0)
            {
                _lblSubtotal.Text = $"${_savedSubtotal:0}";
                _lblPropina.Text  = $"${_savedPropina:0}";
                _lblTotal.Text    = $"${_savedTotal:0}";
            }
            else
            {
                decimal sub   = _items.Sum(i => i.Subtotal);
                decimal prop  = sub * 0.10m;
                decimal total = sub + prop;
                _lblSubtotal.Text = $"${sub:0}";
                _lblPropina.Text  = $"${prop:0}";
                _lblTotal.Text    = $"${total:0}";
            }
        }

        // ── Actions ───────────────────────────────────────────────────────────
        private void EditarItem(DetalleItem item)
        {
            using var dlg = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                Size            = new Size(400, 270),
                StartPosition   = FormStartPosition.CenterParent,
                BackColor       = Color.FromArgb(217, 217, 217)
            };
            EnableDoubleBuffer(dlg);
            dlg.Load += (_, _) => ApplyRounded(dlg, 12);

            // Title bar
            var bar = new Panel { Height = 50, Dock = DockStyle.Top, BackColor = ColHeader };
            var lbl = new Label
            {
                Text      = item.Producto,
                Font      = new Font("Sansation", 11, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock      = DockStyle.Fill
            };
            bar.Controls.Add(lbl);
            void DragDlg(object? s, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(dlg.Handle, 0xA1, 0x2, 0); }
            }
            bar.MouseDown += DragDlg;
            lbl.MouseDown += DragDlg;

            // Content
            var content = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(217, 217, 217) };

            var lblCant = new Label
            {
                Text      = "Cantidad:",
                Font      = new Font("Sansation", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize  = true,
                Location  = new Point(24, 18),
                BackColor = Color.Transparent
            };

            var nudCant = new NumericUpDown
            {
                Location  = new Point(24, 42),
                Size      = new Size(90, 28),
                Minimum   = 1,
                Maximum   = 99,
                Value     = Math.Max(1, item.Cantidad),
                Font      = new Font("Sansation", 11),
                BackColor = Color.White
            };

            var lblObs = new Label
            {
                Text      = "Observaciones:",
                Font      = new Font("Sansation", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30),
                AutoSize  = true,
                Location  = new Point(24, 82),
                BackColor = Color.Transparent
            };

            var txtObs = new TextBox
            {
                Location    = new Point(24, 106),
                Size        = new Size(348, 28),
                Text        = item.Notas,
                Font        = new Font("Sansation", 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor   = Color.White
            };

            // Dialog buttons
            var btnGuardar = MakeDlgBtn("Guardar",  ColBtnNavy,  Color.White,           new Point(220, 152), new Size(76, 34));
            var btnCancel  = MakeDlgBtn("Cancelar", ColBtnGray,  Color.FromArgb(50,50,50), new Point(304, 152), new Size(76, 34));

            btnGuardar.Click += (_, _) =>
            {
                int nuevaCant = (int)nudCant.Value;
                if (nuevaCant < 1)
                {
                    MessageBox.Show("La cantidad debe ser al menos 1.", "Validación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    nudCant.Value = 1;
                    return;
                }
                try
                {
                    decimal nuevaSub = item.PrecioUnit * nuevaCant;
                    using var con = new SQLiteConnection(Conn);
                    con.Open();
                    using var cmd = new SQLiteCommand(@"
                        UPDATE detalle_cuenta
                        SET    cantidad  = @cant,
                               notas     = @notas,
                               subtotal  = @sub
                        WHERE  id_detalle = @id;", con);
                    cmd.Parameters.AddWithValue("@cant",  nuevaCant);
                    cmd.Parameters.AddWithValue("@notas", txtObs.Text.Trim());
                    cmd.Parameters.AddWithValue("@sub",   nuevaSub);
                    cmd.Parameters.AddWithValue("@id",    item.Id);
                    cmd.ExecuteNonQuery();

                    using var upd = new SQLiteCommand(@"
                        UPDATE cuenta SET subtotal =
                            (SELECT COALESCE(SUM(subtotal),0)
                             FROM   detalle_cuenta WHERE id_cuenta = @cid)
                        WHERE id_cuenta = @cid;", con);
                    upd.Parameters.AddWithValue("@cid", _mesa.IdCuenta);
                    upd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al guardar: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                dlg.DialogResult = DialogResult.OK;
                dlg.Close();
            };
            btnCancel.Click += (_, _) => { dlg.DialogResult = DialogResult.Cancel; dlg.Close(); };

            content.Controls.AddRange(new Control[] { lblCant, nudCant, lblObs, txtObs, btnGuardar, btnCancel });
            dlg.Controls.Add(content);
            dlg.Controls.Add(bar);

            if (dlg.ShowDialog(this) == DialogResult.OK)
                CargarDatos();
        }

        private void EliminarItem(DetalleItem item)
        {
            if (MessageBox.Show($"¿Eliminar \"{item.Producto}\" del pedido?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                using var con = new SQLiteConnection(Conn);
                con.Open();
                using var del = new SQLiteCommand(
                    "DELETE FROM detalle_cuenta WHERE id_detalle = @id;", con);
                del.Parameters.AddWithValue("@id", item.Id);
                del.ExecuteNonQuery();

                using var upd = new SQLiteCommand(@"
                    UPDATE cuenta SET subtotal =
                        (SELECT COALESCE(SUM(subtotal),0) FROM detalle_cuenta WHERE id_cuenta = @cid)
                    WHERE id_cuenta = @cid;", con);
                upd.Parameters.AddWithValue("@cid", _mesa.IdCuenta);
                upd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            CargarDatos();
        }

        private void OnCerrarCuenta(object? sender, EventArgs e)
        {
            decimal sub   = _items.Sum(i => i.Subtotal);
            string  folio = _mesa.IdCuenta > 0
                                ? $"{_mesa.HoraLlegada:yyyyMMdd}-{_mesa.IdCuenta:D3}" : "";
            using var overlay = CreateOverlay(this);
            overlay.Show(this);
            using var dlg = new FormCerrarPedido(_mesa.Id, sub, _mesa.NumeroMesa, folio);
            if (dlg.ShowDialog(overlay) == DialogResult.OK && dlg.Confirmado)
            {
                decimal propina = dlg.PropinaConfirmada;
                decimal total   = sub + propina;
                CuentaRepository.CerrarCuentaConTotales(_mesa.IdCuenta, sub, propina, total);
                overlay.Close();
                DialogResult = DialogResult.OK;
                Close();
                return;
            }
            overlay.Close();
        }

        // ── Layout helpers ────────────────────────────────────────────────────
        private static void AddInfoColumn(Panel card, string label, string value, int x)
        {
            card.Controls.Add(new Label { Text = label, Font = FntLabel, ForeColor = ColText, AutoSize = true, Location = new Point(x, 12), BackColor = Color.Transparent });
            card.Controls.Add(new Label { Text = value, Font = FntValue, ForeColor = ColText, AutoSize = true, Location = new Point(x, 40), BackColor = Color.Transparent });
        }

        private static void AddColHeader(Panel p, string text, int x, int w, ContentAlignment align)
        {
            p.Controls.Add(new Label { Text = text, Font = FntTHead, ForeColor = ColText, Location = new Point(x, 0), Size = new Size(w, p.Height), TextAlign = align, BackColor = Color.Transparent });
        }

        private void AddTotalRow(string label, int lblX, int valX, int y, out Label valueLbl)
        {
            Controls.Add(new Label { Text = label, Font = FntTotals, ForeColor = ColText, Location = new Point(lblX, y), Size = new Size(valX - lblX - 6, 22), TextAlign = ContentAlignment.MiddleRight, BackColor = Color.Transparent });
            var val = new Label { Text = "$0", Font = FntTotals, ForeColor = ColText, Location = new Point(valX, y), Size = new Size(Width - 20 - valX, 22), TextAlign = ContentAlignment.MiddleRight, BackColor = Color.Transparent };
            Controls.Add(val);
            valueLbl = val;
        }

        private static Button MakeGrayBtn(string text, Size size)
        {
            var btn = new Button
            {
                Text      = text,
                Font      = FntBtn,
                ForeColor = Color.FromArgb(50, 50, 50),
                BackColor = ColBtnGray,
                FlatStyle = FlatStyle.Flat,
                Size      = size,
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize         = 0;
            btn.FlatAppearance.MouseOverBackColor  = Color.FromArgb(175, 175, 175);
            ApplyRounded(btn, 8);
            return btn;
        }

        private static Button MakeDlgBtn(string text, Color bg, Color fg, Point loc, Size size)
        {
            var btn = new Button
            {
                Text      = text,
                Font      = new Font("Sansation", 10, FontStyle.Bold),
                ForeColor = fg,
                BackColor = bg,
                FlatStyle = FlatStyle.Flat,
                Location  = loc,
                Size      = size,
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            ApplyRounded(btn, 6);
            return btn;
        }

        // ── Drag ──────────────────────────────────────────────────────────────
        private void Drag(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, 0xA1, 0x2, 0); }
        }

        // ── Drawing utilities ─────────────────────────────────────────────────
        private static void PaintBordered(object? sender, PaintEventArgs e)
        {
            if (sender is not Control c) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen  = new Pen(ColBorder);
            using var path = RoundedPath(new Rectangle(0, 0, c.Width - 1, c.Height - 1), 10);
            e.Graphics.DrawPath(pen, path);
        }

        private static GraphicsPath RoundedPath(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            p.AddArc(r.Left,             r.Top,               radius * 2, radius * 2, 180, 90);
            p.AddArc(r.Right - radius*2, r.Top,               radius * 2, radius * 2, 270, 90);
            p.AddArc(r.Right - radius*2, r.Bottom - radius*2, radius * 2, radius * 2,   0, 90);
            p.AddArc(r.Left,             r.Bottom - radius*2, radius * 2, radius * 2,  90, 90);
            p.CloseFigure();
            return p;
        }

        private static void ApplyRounded(Control ctrl, int radius)
        {
            void Apply()
            {
                if (ctrl.Width <= 0 || ctrl.Height <= 0) return;
                using var path = RoundedPath(new Rectangle(0, 0, ctrl.Width, ctrl.Height), radius);
                ctrl.Region = new Region(path);
            }
            ctrl.Resize += (_, _) => Apply();
            Apply();
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

        private Panel BuildAgregarBtn()
        {
            var pnl = new Panel
            {
                Size      = new Size(158, 40),
                BackColor = ColBtnNavy,
                Cursor    = Cursors.Hand
            };
            ApplyRounded(pnl, 8);
            pnl.MouseEnter += (_, _) => pnl.BackColor = Color.FromArgb(20, 55, 100);
            pnl.MouseLeave += (_, _) => pnl.BackColor = ColBtnNavy;

            var lblPlus = new Label
            {
                Text      = "+",
                Font      = new Font("Sansation", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location  = new Point(10, 0),
                Size      = new Size(28, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand
            };
            var lblText = new Label
            {
                Text      = "Agregar\nPedido",
                Font      = new Font("Sansation", 9, FontStyle.Bold),
                ForeColor = Color.White,
                Location  = new Point(44, 0),
                Size      = new Size(106, 40),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand
            };
            pnl.Controls.Add(lblPlus);
            pnl.Controls.Add(lblText);
            return pnl;
        }

        private static void EnableDoubleBuffer(Control ctrl)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, ctrl, new object[] { true });
        }
    }
}
