using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RestaurantKarin
{
    public class PantallaRecetaAgregarEditar : UserControl
    {
        private readonly Receta? _recetaOriginal;
        private readonly Receta _trabajo;

        private TextBox _txtNombre = null!;
        private TextBox _txtDescripcion = null!;
        private DataGridView _dgv = null!;
        private TextBox _txtInsumo = null!;
        private NumericUpDown _numCantidad = null!;
        private ComboBox _cmbUnidad = null!;
        private NumericUpDown _numPorciones = null!;
        private Label _lblCostoIngredientes = null!;
        private Label _lblCostoPorcion = null!;
        private Label _lblAyudaEdicion = null!;
        private Label _lblUnidad = null!;
        private Button _btnAnadir = null!;
        private Button _btnCancelar = null!;
        private Button _btnGuardar = null!;
        private Panel _pnlSearch = null!;
        private int _indiceEditando = -1;
        private decimal _costoUnitarioActual = 0m;
        private bool _insumoSeleccionadoDeBD = false;
        private readonly Dictionary<string, (string Unidad, decimal Costo)> _cachInsumos = new();

        // Popup no-activante — nunca roba el foco del TextBox
        private ListBox _lstDropdown = null!;
        private ListaPopup _dropdownForm = null!;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);
        private static void EstablecerPlaceholder(TextBox tb, string texto) =>
            SendMessage(tb.Handle, 0x1501, (IntPtr)1, texto);

        public event EventHandler? Guardado;
        public event EventHandler? Cancelado;

        public PantallaRecetaAgregarEditar(Receta? existente)
        {
            _recetaOriginal = existente;
            _trabajo = existente?.CopiaParaEdicion() ?? new Receta();
            DoubleBuffered = true;
            BackColor = Color.White;
            ConstruirUi();
            EnlazarEventos();
            RefrescarMontos();
            if (existente != null)
                CargarDatosIniciales();
            else
                PoblarGrid();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dropdownForm?.Hide();
                _dropdownForm?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void CargarDatosIniciales()
        {
            _txtNombre.Text = _trabajo.Nombre;
            _txtDescripcion.Text = _trabajo.Descripcion;
            _numPorciones.Value = _trabajo.Porciones < 1 ? 1 : (decimal)_trabajo.Porciones;
            PoblarGrid();
        }

        private void ConstruirUi()
        {
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Diseno.TealPrimario
            };
            pnlHeader.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Sansation", 14f, FontStyle.Bold),
                Text = _recetaOriginal == null ? "AGREGAR NUEVA RECETA" : "EDITAR RECETA"
            });

            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                Padding = new Padding(20, 10, 20, 10),
                BackColor = Color.White
            };
            _btnCancelar = new Button { Text = "CANCELAR", Height = 44 };
            Diseno.BotonSecundario(_btnCancelar);

            _btnGuardar = new Button { Text = "+ GUARDAR RECETA", Height = 44 };
            Diseno.BotonPrimario(_btnGuardar);
            pnlFooter.Controls.Add(_btnCancelar);
            pnlFooter.Controls.Add(_btnGuardar);
            pnlFooter.Resize += (_, _) =>
            {
                const int marg = 20, gap = 12;
                int top = (pnlFooter.ClientSize.Height - 44) / 2;
                int avail = Math.Max(100, pnlFooter.ClientSize.Width - marg * 2 - gap);
                int cancelW = avail / 2;
                int guardarW = avail - cancelW;
                _btnCancelar.SetBounds(marg, top, cancelW, 44);
                _btnGuardar.SetBounds(marg + cancelW + gap, top, guardarW, 44);
            };

            var tlpCuerpo = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(20, 16, 20, 16),
                BackColor = Color.White
            };
            tlpCuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62f));
            tlpCuerpo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38f));

            var pnlIzq = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0) };
            var pnlInfo = CrearBloqueGris();
            pnlInfo.Dock = DockStyle.Top;
            pnlInfo.Height = 150;

            _lblAyudaEdicion = new Label
            {
                Dock = DockStyle.Top,
                Height = 22,
                ForeColor = Diseno.TextoSecundario,
                Font = Diseno.FuenteCuerpo(8.5f),
                Text = "",
                TextAlign = ContentAlignment.MiddleLeft
            };

            _dgv = CrearGridInsumos();
            _dgv.Dock = DockStyle.Fill;

            pnlIzq.Controls.Add(_dgv);
            pnlIzq.Controls.Add(_lblAyudaEdicion);
            pnlIzq.Controls.Add(pnlInfo);

            var pnlDer = new Panel { Dock = DockStyle.Fill, Margin = new Padding(12, 0, 0, 0) };
            var pnlAdd = CrearBloqueAnadirInsumo();
            pnlAdd.Dock = DockStyle.Top;
            pnlAdd.Height = 230;
            var pnlCostos = CrearBloqueCostos();
            pnlCostos.Dock = DockStyle.Top;
            pnlCostos.Height = 230;
            var spacerDer = new Panel { Dock = DockStyle.Top, Height = 14, BackColor = Color.White };
            pnlDer.Controls.Add(pnlCostos);
            pnlDer.Controls.Add(spacerDer);
            pnlDer.Controls.Add(pnlAdd);

            tlpCuerpo.Controls.Add(pnlIzq, 0, 0);
            tlpCuerpo.Controls.Add(pnlDer, 1, 0);

            Controls.Add(tlpCuerpo);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);

            // Popup con WS_EX_NOACTIVATE: flota sobre todos los controles sin robar el foco
            _lstDropdown = new ListBox
            {
                Font = Diseno.FuenteCuerpo(),
                BorderStyle = BorderStyle.FixedSingle,
                IntegralHeight = false,
                Dock = DockStyle.Fill
            };
            _lstDropdown.Click += ListaDropdown_Click;
            _dropdownForm = new ListaPopup();
            _dropdownForm.Controls.Add(_lstDropdown);
        }

        private Panel CrearBloqueGris()
        {
            const int labelW = 158;
            const int inputX = 14 + labelW;

            var p = new Panel
            {
                BackColor = Diseno.PanelGrisSuave,
                Padding = new Padding(14),
                Margin = new Padding(0, 0, 0, 12)
            };

            p.Controls.Add(new Label
            {
                Text = "Nombre de la Receta:",
                AutoSize = false,
                Width = 155,
                Height = 22,
                Font = Diseno.FuenteTituloSeccion(),
                ForeColor = Diseno.TextoNormal,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(14, 16)
            });
            _txtNombre = new TextBox
            {
                Location = new Point(inputX, 14),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = Diseno.FuenteCuerpo()
            };
            _txtNombre.HandleCreated += (_, _) => EstablecerPlaceholder(_txtNombre, "Escribe aquí...");

            p.Controls.Add(new Label
            {
                Text = "Descripcion:",
                AutoSize = false,
                Width = 155,
                Height = 22,
                Font = Diseno.FuenteTituloSeccion(),
                ForeColor = Diseno.TextoNormal,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(14, 56)
            });
            _txtDescripcion = new TextBox
            {
                Location = new Point(inputX, 54),
                Height = 68,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = Diseno.FuenteCuerpo()
            };

            p.Resize += (_, _) =>
            {
                var w = p.ClientSize.Width - inputX - 14;
                if (w > 10) { _txtNombre.Width = w; _txtDescripcion.Width = w; }
            };

            p.Controls.Add(_txtDescripcion);
            p.Controls.Add(_txtNombre);
            return p;
        }

        private DataGridView CrearGridInsumos()
        {
            var g = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                CellBorderStyle = DataGridViewCellBorderStyle.Single,
                ColumnHeadersHeight = 36,
                EnableHeadersVisualStyles = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = Diseno.FuenteCuerpo()
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = Diseno.PanelGrisMedio;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Diseno.TextoNormal;
            g.ColumnHeadersDefaultCellStyle.Font = Diseno.FuenteTituloSeccion();
            g.DefaultCellStyle.Padding = new Padding(6, 4, 6, 4);
            g.GridColor = Color.FromArgb(210, 210, 210);
            g.AlternatingRowsDefaultCellStyle.BackColor = Diseno.ZebraClaro;
            g.RowsDefaultCellStyle.BackColor = Color.White;
            g.RowTemplate.Height = 32;

            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colInsumo", HeaderText = "Insumo", FillWeight = 40 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCant", HeaderText = "Cantidad", FillWeight = 15 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colUnid", HeaderText = "Unidad", FillWeight = 20 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCosto", HeaderText = "Costo Total", FillWeight = 15 });
            g.Columns.Add(new DataGridViewButtonColumn { Name = "colEdit", HeaderText = "", Text = "✏", UseColumnTextForButtonValue = true, FillWeight = 5 });
            g.Columns.Add(new DataGridViewButtonColumn { Name = "colDel", HeaderText = "", Text = "🗑", UseColumnTextForButtonValue = true, FillWeight = 5 });

            g.Paint += (sender, e) =>
            {
                var grid = (DataGridView)sender!;
                if (grid.Rows.Count > 0) return;
                var bodyRect = new RectangleF(
                    1, grid.ColumnHeadersHeight,
                    grid.Width - 2, grid.Height - grid.ColumnHeadersHeight - 1);
                using var brush = new SolidBrush(Diseno.TextoPlaceholder);
                using var font = new Font("Sansation", 10f, FontStyle.Italic);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString("Añade ingredientes para comenzar...", font, brush, bodyRect, sf);
            };

            return g;
        }

        private Panel CrearBloqueAnadirInsumo()
        {
            var p = new Panel
            {
                BackColor = Diseno.PanelGrisSuave,
                Padding = new Padding(14)
            };
            p.Controls.Add(new Label
            {
                Text = "AÑADIR NUEVO INGREDIENTE",
                Font = Diseno.FuenteTituloSeccion(10f),
                AutoSize = true,
                Location = new Point(14, 10)
            });
            p.Controls.Add(new Label
            {
                Text = "Seleccionar Insumo...",
                AutoSize = true,
                Location = new Point(14, 40),
                Font = Diseno.FuenteCuerpo(9f)
            });

            _pnlSearch = new Panel
            {
                Location = new Point(14, 62),
                Height = 30,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            _txtInsumo = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = Diseno.FuenteCuerpo(),
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            _txtInsumo.HandleCreated += (_, _) => EstablecerPlaceholder(_txtInsumo, "Nombre del Insumo...");
            _txtInsumo.TextChanged += TxtInsumo_TextChanged;
            _txtInsumo.Enter += (_, _) => MostrarDropdown(BuscarInsumosEnDB(""));
            _txtInsumo.Leave += (_, _) => _dropdownForm.Hide();

            var lblIconoBusqueda = new Label
            {
                Text = "🔍",
                Width = 30,
                Dock = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Diseno.TextoPlaceholder,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10f)
            };
            lblIconoBusqueda.Click += (_, _) => { _txtInsumo.Focus(); MostrarDropdown(BuscarInsumosEnDB("")); };
            _pnlSearch.Controls.Add(_txtInsumo);
            _pnlSearch.Controls.Add(lblIconoBusqueda);
            p.Controls.Add(_pnlSearch);

            p.Controls.Add(new Label { Text = "Cantidad:", Location = new Point(14, 100), AutoSize = true, Font = Diseno.FuenteCuerpo(9f) });
            _lblUnidad = new Label { Text = "Unidad:", Location = new Point(138, 100), AutoSize = true, Font = Diseno.FuenteCuerpo(9f) };
            p.Controls.Add(_lblUnidad);

            _numCantidad = new NumericUpDown
            {
                Location = new Point(14, 118),
                Width = 110,
                DecimalPlaces = 2,
                Maximum = 999999,
                Minimum = 0
            };
            _cmbUnidad = new ComboBox
            {
                Location = new Point(138, 118),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbUnidad.Items.AddRange(new object[] { "gramos", "rebanada", "mililitros", "pieza", "cucharada", "pizca" });
            _cmbUnidad.SelectedIndex = 0;

            _btnAnadir = new Button
            {
                Text = "+ AÑADIR A LA LISTA",
                Location = new Point(14, 160),
                Height = 42
            };
            Diseno.BotonPrimario(_btnAnadir);

            p.Resize += (_, _) =>
            {
                _pnlSearch.Width = p.ClientSize.Width - 28;
                _btnAnadir.Width = p.ClientSize.Width - 28;

                int totalW = p.ClientSize.Width - 28;
                int cantW = Math.Max(80, (int)(totalW * 0.43f));
                int gap = 8;
                int unidX = 14 + cantW + gap;
                int unidW = Math.Max(60, totalW - cantW - gap);
                _numCantidad.Width = cantW;
                _cmbUnidad.Location = new Point(unidX, 118);
                _cmbUnidad.Width = unidW;
                _lblUnidad.Location = new Point(unidX, 100);
            };

            p.Controls.Add(_btnAnadir);
            p.Controls.Add(_cmbUnidad);
            p.Controls.Add(_numCantidad);
            return p;
        }

        private Panel CrearBloqueCostos()
        {
            var p = new Panel
            {
                BackColor = Diseno.PanelGrisSuave,
                Padding = new Padding(14)
            };
            p.Controls.Add(new Label
            {
                Text = "DATOS DE COSTO",
                Font = Diseno.FuenteTituloSeccion(10f),
                Location = new Point(14, 10),
                AutoSize = true
            });
            p.Controls.Add(new Label { Text = "Costo Total de Ingredientes:", Location = new Point(14, 42), AutoSize = true });
            _lblCostoIngredientes = new Label
            {
                Text = "$ 0.00",
                Location = new Point(14, 62),
                Height = 32,
                Width = 200,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Diseno.VerdeBordeCosto,
                Font = new Font("Sansation", 11f, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Padding = new Padding(8, 6, 0, 0)
            };
            _lblCostoIngredientes.Paint += (_, e) =>
            {
                var r = _lblCostoIngredientes.ClientRectangle;
                r.Inflate(-1, -1);
                using var pen = new Pen(Diseno.VerdeBordeCosto, 2);
                e.Graphics.DrawRectangle(pen, r);
            };

            p.Controls.Add(new Label { Text = "Porciones (Cantidad):", Location = new Point(14, 104), AutoSize = true });
            _numPorciones = new NumericUpDown
            {
                Location = new Point(14, 124),
                Width = 120,
                Minimum = 1,
                Maximum = 9999,
                Value = 1
            };

            p.Controls.Add(new Label { Text = "Costo por Porcion:", Location = new Point(14, 158), AutoSize = true });
            _lblCostoPorcion = new Label
            {
                Text = "$ 0.00",
                Location = new Point(14, 178),
                Height = 32,
                Width = 200,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Diseno.TextoNormal,
                Font = new Font("Sansation", 11f, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Padding = new Padding(8, 6, 0, 0)
            };

            p.Controls.Add(_lblCostoPorcion);
            p.Controls.Add(_numPorciones);
            p.Controls.Add(_lblCostoIngredientes);
            return p;
        }

        private void EnlazarEventos()
        {
            _btnCancelar.Click += (_, _) => Cancelado?.Invoke(this, EventArgs.Empty);
            _btnGuardar.Click += BtnGuardar_Click;
            _btnAnadir.Click += BtnAnadir_Click;
            _numPorciones.ValueChanged += (_, _) => RefrescarMontos();
            _dgv.CellContentClick += Dgv_CellContentClick;
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            var nombre = _txtNombre.Text.Trim();
            if (string.IsNullOrWhiteSpace(nombre))
            {
                MessageBox.Show("Indique el nombre de la receta.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _trabajo.Nombre = nombre;
            _trabajo.Descripcion = _txtDescripcion.Text.Trim();
            _trabajo.Porciones = _numPorciones.Value;
            decimal totalIns = _trabajo.Lineas.Sum(x => x.CostoTotal);
            if (totalIns > 0 && _numPorciones.Value > 0)
                _trabajo.CostoPorPorcion = Math.Round(totalIns / _numPorciones.Value, 2, MidpointRounding.AwayFromZero);
            else if (_recetaOriginal == null)
                _trabajo.CostoPorPorcion = 0;
            if (_recetaOriginal != null)
                _trabajo.Id = _recetaOriginal.Id;

            try
            {
                RecetasBaseDatos.Guardar(_trabajo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo guardar en la base de datos.\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Guardado?.Invoke(this, EventArgs.Empty);
        }

        private void BtnAnadir_Click(object? sender, EventArgs e)
        {
            var ins = _txtInsumo.Text.Trim();
            if (string.IsNullOrWhiteSpace(ins))
            {
                MessageBox.Show("Escriba el nombre del insumo.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!_insumoSeleccionadoDeBD && !ExisteInsumoEnDB(ins))
            {
                MessageBox.Show(
                    $"El ingrediente \"{ins}\" no está disponible en el inventario.\nSeleccione un ingrediente de la lista desplegable.",
                    "Ingrediente no disponible",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            var nuevaLinea = new RecetaLinea
            {
                Insumo = ins,
                Cantidad = _numCantidad.Value,
                Unidad = _cmbUnidad.SelectedItem?.ToString() ?? "gramos",
                CostoTotal = Math.Round(_costoUnitarioActual * _numCantidad.Value, 2, MidpointRounding.AwayFromZero)
            };
            _costoUnitarioActual = 0m;

            if (_indiceEditando >= 0 && _indiceEditando < _trabajo.Lineas.Count)
                _trabajo.Lineas[_indiceEditando] = nuevaLinea;
            else
                _trabajo.Lineas.Add(nuevaLinea);

            _indiceEditando = -1;
            _txtInsumo.Clear();
            _numCantidad.Value = 0;
            _lblAyudaEdicion.Text = "";
            _insumoSeleccionadoDeBD = false;
            _dropdownForm.Hide();
            PoblarGrid();
            RefrescarMontos();
        }

        private void Dgv_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var col = _dgv.Columns[e.ColumnIndex].Name;
            if (col != "colEdit" && col != "colDel") return;

            var linea = _trabajo.Lineas[e.RowIndex];
            if (col == "colDel")
            {
                var confirm = MessageBox.Show(
                    $"¿Está seguro de eliminar el insumo \"{linea.Insumo}\"?",
                    "Eliminar Insumo",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;

                if (_indiceEditando == e.RowIndex)
                {
                    _indiceEditando = -1;
                    _txtInsumo.Clear();
                    _numCantidad.Value = 0;
                    _lblAyudaEdicion.Text = "";
                }
                else if (_indiceEditando > e.RowIndex)
                {
                    _indiceEditando--;
                }
                _trabajo.Lineas.RemoveAt(e.RowIndex);
                PoblarGrid();
                RefrescarMontos();
            }
            else
            {
                _indiceEditando = e.RowIndex;
                // Suppress TextChanged search while loading edit data
                _txtInsumo.TextChanged -= TxtInsumo_TextChanged;
                _txtInsumo.Text = linea.Insumo;
                _txtInsumo.TextChanged += TxtInsumo_TextChanged;
                _insumoSeleccionadoDeBD = true;
                _numCantidad.Value = linea.Cantidad > _numCantidad.Maximum ? _numCantidad.Maximum : linea.Cantidad;
                var i = _cmbUnidad.Items.IndexOf(linea.Unidad);
                _cmbUnidad.SelectedIndex = i >= 0 ? i : 0;
                _lblAyudaEdicion.Text = "Línea cargada para edición: ajuste y pulse + AÑADIR A LA LISTA.";
            }
        }

        // ─── Autocomplete DB-connected (Fix 3.3) ──────────────────────────────

        private void TxtInsumo_TextChanged(object? sender, EventArgs e)
        {
            _insumoSeleccionadoDeBD = false;
            MostrarDropdown(BuscarInsumosEnDB(_txtInsumo.Text.Trim()));
        }

        private void ListaDropdown_Click(object? sender, EventArgs e)
        {
            if (_lstDropdown.SelectedItem is not string sel) return;
            _dropdownForm.Hide();
            _insumoSeleccionadoDeBD = true;

            _txtInsumo.TextChanged -= TxtInsumo_TextChanged;
            _txtInsumo.Text = sel;
            _txtInsumo.TextChanged += TxtInsumo_TextChanged;

            if (_cachInsumos.TryGetValue(sel, out var info))
            {
                if (!string.IsNullOrWhiteSpace(info.Unidad))
                {
                    int idx = _cmbUnidad.FindStringExact(info.Unidad);
                    if (idx >= 0)
                        _cmbUnidad.SelectedIndex = idx;
                    else
                    {
                        _cmbUnidad.Items.Add(info.Unidad);
                        _cmbUnidad.SelectedIndex = _cmbUnidad.Items.Count - 1;
                    }
                }
                _costoUnitarioActual = info.Costo;
            }
        }

        private void MostrarDropdown(List<string> items)
        {
            if (items.Count == 0) { _dropdownForm.Hide(); return; }
            if (!_pnlSearch.IsHandleCreated) return;

            int rowH = _lstDropdown.ItemHeight > 0 ? _lstDropdown.ItemHeight : 18;
            int h = Math.Min(items.Count, 7) * rowH + 4;
            _lstDropdown.DataSource = null;
            _lstDropdown.DataSource = items;

            var pos = _pnlSearch.PointToScreen(new Point(0, _pnlSearch.Height));
            _dropdownForm.SetBounds(pos.X, pos.Y, _pnlSearch.Width, h);
            if (!_dropdownForm.Visible)
                _dropdownForm.Show(FindForm());
        }

        private List<string> BuscarInsumosEnDB(string filtro)
        {
            _cachInsumos.Clear();
            var lista = new List<string>();
            try
            {
                var cs = ConfigurationManager.ConnectionStrings["KarinDB"]?.ConnectionString;
                if (string.IsNullOrWhiteSpace(cs)) cs = "Data Source=karin_pos.db;Version=3;";
                if (!cs.Contains("Version=", StringComparison.OrdinalIgnoreCase))
                    cs = cs.Trim().TrimEnd(';') + ";Version=3;";
                using var con = new SQLiteConnection(cs);
                con.Open();
                string q = string.IsNullOrWhiteSpace(filtro)
                    ? "SELECT Nombre, Unidad, Costo FROM Insumos ORDER BY Nombre COLLATE NOCASE LIMIT 50;"
                    : "SELECT Nombre, Unidad, Costo FROM Insumos WHERE Nombre LIKE @f COLLATE NOCASE ORDER BY Nombre COLLATE NOCASE LIMIT 20;";
                using var cmd = new SQLiteCommand(q, con);
                if (!string.IsNullOrWhiteSpace(filtro))
                    cmd.Parameters.AddWithValue("@f", "%" + filtro + "%");
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    if (r.IsDBNull(0)) continue;
                    var nombre = r.GetString(0);
                    var unidad = r.IsDBNull(1) ? "" : r.GetString(1);
                    var costo = r.IsDBNull(2) ? 0m : Convert.ToDecimal(r.GetValue(2), CultureInfo.InvariantCulture);
                    lista.Add(nombre);
                    _cachInsumos[nombre] = (unidad, costo);
                }
            }
            catch { }
            return lista;
        }

        private bool ExisteInsumoEnDB(string nombre)
        {
            try
            {
                var cs = ConfigurationManager.ConnectionStrings["KarinDB"]?.ConnectionString;
                if (string.IsNullOrWhiteSpace(cs)) cs = "Data Source=karin_pos.db;Version=3;";
                if (!cs.Contains("Version=", StringComparison.OrdinalIgnoreCase))
                    cs = cs.Trim().TrimEnd(';') + ";Version=3;";
                using var con = new SQLiteConnection(cs);
                con.Open();
                using var cmd = new SQLiteCommand(
                    "SELECT COUNT(*) FROM Insumos WHERE Nombre = @n COLLATE NOCASE;", con);
                cmd.Parameters.AddWithValue("@n", nombre);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
            catch { return false; }
        }

        // ─── Grid / montos ────────────────────────────────────────────────────

        private void PoblarGrid()
        {
            _dgv.Rows.Clear();
            foreach (var l in _trabajo.Lineas)
                _dgv.Rows.Add(l.Insumo, l.Cantidad.ToString("0.##"), l.Unidad, l.CostoTotal.ToString("0.00"));
            _dgv.Invalidate();
        }

        private void RefrescarMontos()
        {
            var total = _trabajo.Lineas.Sum(x => x.CostoTotal);
            _lblCostoIngredientes.Text = "$ " + total.ToString("0.00");
            var cpp = _numPorciones.Value <= 0 ? 0 : Math.Round(total / _numPorciones.Value, 2, MidpointRounding.AwayFromZero);
            _lblCostoPorcion.Text = "$ " + cpp.ToString("0.00");
        }

        // ─── Diseño ───────────────────────────────────────────────────────────

        private static class Diseno
        {
            public static readonly Color TealPrimario = Color.FromArgb(23, 91, 122);
            public static readonly Color TealOscuro = Color.FromArgb(18, 72, 97);
            public static readonly Color PanelGrisMedio = Color.FromArgb(224, 224, 224);
            public static readonly Color PanelGrisSuave = Color.FromArgb(242, 242, 242);
            public static readonly Color ZebraClaro = Color.FromArgb(250, 250, 250);
            public static readonly Color TextoNormal = Color.FromArgb(33, 33, 33);
            public static readonly Color TextoSecundario = Color.FromArgb(97, 97, 97);
            public static readonly Color TextoPlaceholder = Color.FromArgb(158, 158, 158);
            public static readonly Color VerdeBordeCosto = Color.FromArgb(64, 145, 108);
            public static readonly Color BotonCancelarFondo = Color.FromArgb(204, 204, 204);
            public static readonly Color BotonCancelarTexto = Color.FromArgb(66, 66, 66);

            public static Font FuenteTituloSeccion(float t = 11f) =>
                new Font("Sansation", t, FontStyle.Bold, GraphicsUnit.Point);

            public static Font FuenteCuerpo(float t = 10f) =>
                new Font("Sansation", t, FontStyle.Regular, GraphicsUnit.Point);

            public static Font FuenteBotonMayus(float t = 9.5f) =>
                new Font("Sansation", t, FontStyle.Bold, GraphicsUnit.Point);

            public static void BotonPrimario(Button b)
            {
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 0;
                b.BackColor = TealPrimario;
                b.ForeColor = Color.White;
                b.Font = FuenteBotonMayus(10f);
                b.Cursor = Cursors.Hand;
                b.UseVisualStyleBackColor = false;
                b.Resize += (_, _) => AplicarBordeRedondeado(b, 12);
                b.HandleCreated += (_, _) => AplicarBordeRedondeado(b, 12);
                b.MouseEnter += (_, _) => { b.BackColor = TealOscuro; };
                b.MouseLeave += (_, _) => { b.BackColor = TealPrimario; };
                AplicarBordeRedondeado(b, 12);
            }

            public static void BotonSecundario(Button b)
            {
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 1;
                b.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
                b.BackColor = BotonCancelarFondo;
                b.ForeColor = BotonCancelarTexto;
                b.Font = FuenteBotonMayus(10f);
                b.Cursor = Cursors.Hand;
                b.UseVisualStyleBackColor = false;
                b.Resize += (_, _) => AplicarBordeRedondeado(b, 12);
                b.HandleCreated += (_, _) => AplicarBordeRedondeado(b, 12);
                AplicarBordeRedondeado(b, 12);
            }

            private static void AplicarBordeRedondeado(Control c, int radio)
            {
                if (c.Width <= 0 || c.Height <= 0) return;
                var path = new GraphicsPath();
                int d = radio * 2;
                path.AddArc(0, 0, d, d, 180, 90);
                path.AddArc(c.Width - d, 0, d, d, 270, 90);
                path.AddArc(c.Width - d, c.Height - d, d, d, 0, 90);
                path.AddArc(0, c.Height - d, d, d, 90, 90);
                path.CloseFigure();
                c.Region = new Region(path);
            }
        }

        // Ventana flotante que nunca roba el foco (WS_EX_NOACTIVATE)
        private sealed class ListaPopup : Form
        {
            protected override bool ShowWithoutActivation => true;

            protected override CreateParams CreateParams
            {
                get
                {
                    var cp = base.CreateParams;
                    cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                    return cp;
                }
            }

            public ListaPopup()
            {
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.Manual;
            }
        }
    }
}
