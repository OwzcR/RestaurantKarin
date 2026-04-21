using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RestaurantKarin
{
    /// <summary>
    /// Pantalla para agregar receta nueva o editar una existente (mismo diseño, distinto título).
    /// Colores y estilos: clase <see cref="Diseno"/> al final. Datos: <see cref="RecetasBaseDatos.Guardar"/>.
    /// </summary>
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
        private Button _btnAnadir = null!;
        private Button _btnCancelar = null!;
        private Button _btnGuardar = null!;
        private readonly Panel _pnlGridHost = new Panel();
        private readonly Label _lblGridVacio = new Label();

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
                Height = 52,
                BackColor = Diseno.TealPrimario
            };
            pnlHeader.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Text = _recetaOriginal == null ? "AGREGAR NUEVA RECETA" : "EDITAR RECETA"
            });

            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                Padding = new Padding(20, 10, 20, 10),
                BackColor = Color.White
            };
            _btnCancelar = new Button { Text = "CANCELAR", Anchor = AnchorStyles.Left | AnchorStyles.Bottom };
            Diseno.BotonSecundario(_btnCancelar);
            _btnCancelar.Size = new Size(220, 44);
            _btnCancelar.Location = new Point(20, 10);

            _btnGuardar = new Button
            {
                Text = "+  GUARDAR RECETA",
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                Size = new Size(260, 44)
            };
            Diseno.BotonPrimario(_btnGuardar);
            pnlFooter.Controls.Add(_btnCancelar);
            pnlFooter.Controls.Add(_btnGuardar);
            pnlFooter.Resize += (_, _) =>
            {
                _btnGuardar.Left = pnlFooter.ClientSize.Width - _btnGuardar.Width - 20;
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
            pnlInfo.Height = 168;

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
            _pnlGridHost.Dock = DockStyle.Fill;
            _lblGridVacio.Dock = DockStyle.Fill;
            _lblGridVacio.TextAlign = ContentAlignment.MiddleCenter;
            _lblGridVacio.Font = new Font("Segoe UI", 10f, FontStyle.Italic);
            _lblGridVacio.ForeColor = Diseno.TextoPlaceholder;
            _lblGridVacio.Text = "Añade ingredientes para comenzar...";
            _lblGridVacio.BackColor = Color.White;
            _pnlGridHost.Controls.Add(_dgv);
            _pnlGridHost.Controls.Add(_lblGridVacio);
            _lblGridVacio.BringToFront();

            pnlIzq.Controls.Add(_pnlGridHost);
            pnlIzq.Controls.Add(_lblAyudaEdicion);
            pnlIzq.Controls.Add(pnlInfo);

            var pnlDer = new Panel { Dock = DockStyle.Fill, Margin = new Padding(12, 0, 0, 0) };
            var pnlAdd = CrearBloqueAnadirInsumo();
            pnlAdd.Dock = DockStyle.Top;
            pnlAdd.Height = 260;
            var pnlCostos = CrearBloqueCostos();
            pnlCostos.Dock = DockStyle.Top;
            pnlCostos.Height = 200;
            pnlCostos.Margin = new Padding(0, 14, 0, 0);
            pnlDer.Controls.Add(pnlCostos);
            pnlDer.Controls.Add(pnlAdd);

            tlpCuerpo.Controls.Add(pnlIzq, 0, 0);
            tlpCuerpo.Controls.Add(pnlDer, 1, 0);

            Controls.Add(tlpCuerpo);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
        }

        private Panel CrearBloqueGris()
        {
            var p = new Panel
            {
                BackColor = Diseno.PanelGrisSuave,
                Padding = new Padding(14),
                Margin = new Padding(0, 0, 0, 12)
            };

            p.Controls.Add(new Label
            {
                Text = "Nombre de la Receta",
                AutoSize = true,
                Font = Diseno.FuenteTituloSeccion(),
                ForeColor = Diseno.TextoNormal,
                Location = new Point(14, 12)
            });
            _txtNombre = new TextBox
            {
                Location = new Point(14, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = Diseno.FuenteCuerpo()
            };
            _txtNombre.Width = 400;

            p.Controls.Add(new Label
            {
                Text = "Descripcion",
                AutoSize = true,
                Font = Diseno.FuenteTituloSeccion(),
                ForeColor = Diseno.TextoNormal,
                Location = new Point(14, 72)
            });
            _txtDescripcion = new TextBox
            {
                Location = new Point(14, 96),
                Height = 56,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = Diseno.FuenteCuerpo()
            };

            p.Resize += (_, _) =>
            {
                _txtNombre.Width = p.ClientSize.Width - 28;
                _txtDescripcion.Width = p.ClientSize.Width - 28;
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
            g.AlternatingRowsDefaultCellStyle.BackColor = Diseno.ZebraClaro;
            g.RowsDefaultCellStyle.BackColor = Color.White;

            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colInsumo", HeaderText = "Insumo", FillWeight = 40 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCant", HeaderText = "Cantidad", FillWeight = 15 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colUnid", HeaderText = "Unidad", FillWeight = 20 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCosto", HeaderText = "Costo Total", FillWeight = 15 });
            g.Columns.Add(new DataGridViewButtonColumn { Name = "colEdit", HeaderText = "", Text = "✏", UseColumnTextForButtonValue = true, FillWeight = 5 });
            g.Columns.Add(new DataGridViewButtonColumn { Name = "colDel", HeaderText = "", Text = "🗑", UseColumnTextForButtonValue = true, FillWeight = 5 });
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
                Text = "Seleccionar Insumo",
                AutoSize = true,
                Location = new Point(14, 40),
                Font = Diseno.FuenteCuerpo(9f)
            });
            _txtInsumo = new TextBox
            {
                Location = new Point(14, 60),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = Diseno.FuenteCuerpo()
            };

            p.Controls.Add(new Label { Text = "Cantidad", Location = new Point(14, 96), AutoSize = true });
            _numCantidad = new NumericUpDown
            {
                Location = new Point(14, 116),
                Width = 120,
                DecimalPlaces = 2,
                Maximum = 999999,
                Minimum = 0
            };
            p.Controls.Add(new Label { Text = "Unidad", Location = new Point(150, 96), AutoSize = true });
            _cmbUnidad = new ComboBox
            {
                Location = new Point(150, 116),
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbUnidad.Items.AddRange(new object[] { "gramos", "rebanada", "mililitros", "pieza", "cucharada", "pizca" });
            _cmbUnidad.SelectedIndex = 0;

            _btnAnadir = new Button
            {
                Text = "+  AÑADIR A LA LISTA",
                Location = new Point(14, 154),
                Height = 42
            };
            Diseno.BotonPrimario(_btnAnadir);
            p.Resize += (_, _) =>
            {
                _txtInsumo.Width = p.ClientSize.Width - 28;
                _btnAnadir.Width = p.ClientSize.Width - 28;
            };

            p.Controls.Add(_btnAnadir);
            p.Controls.Add(_cmbUnidad);
            p.Controls.Add(_numCantidad);
            p.Controls.Add(_txtInsumo);
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
            p.Controls.Add(new Label { Text = "Costo Total de Ingredientes", Location = new Point(14, 42), AutoSize = true });
            _lblCostoIngredientes = new Label
            {
                Text = "$ 0.00",
                Location = new Point(14, 62),
                Height = 32,
                Width = 200,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Diseno.TextoNormal,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
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

            p.Controls.Add(new Label { Text = "Porciones (Cantidad)", Location = new Point(14, 104), AutoSize = true });
            _numPorciones = new NumericUpDown
            {
                Location = new Point(14, 124),
                Width = 120,
                Minimum = 1,
                Maximum = 9999,
                Value = 1
            };

            p.Controls.Add(new Label { Text = "Costo por Porcion", Location = new Point(14, 158), AutoSize = true });
            _lblCostoPorcion = new Label
            {
                Text = "$ 0.00",
                Location = new Point(14, 178),
                Height = 32,
                Width = 200,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Diseno.TextoNormal,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
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
            _trabajo.Lineas.Add(new RecetaLinea
            {
                Insumo = ins,
                Cantidad = _numCantidad.Value,
                Unidad = _cmbUnidad.SelectedItem?.ToString() ?? "gramos",
                CostoTotal = 0
            });
            _txtInsumo.Clear();
            _numCantidad.Value = 0;
            _lblAyudaEdicion.Text = "";
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
                _trabajo.Lineas.RemoveAt(e.RowIndex);
                _lblAyudaEdicion.Text = "";
                PoblarGrid();
                RefrescarMontos();
            }
            else
            {
                _txtInsumo.Text = linea.Insumo;
                _numCantidad.Value = linea.Cantidad > _numCantidad.Maximum ? _numCantidad.Maximum : linea.Cantidad;
                var i = _cmbUnidad.Items.IndexOf(linea.Unidad);
                _cmbUnidad.SelectedIndex = i >= 0 ? i : 0;
                _trabajo.Lineas.RemoveAt(e.RowIndex);
                PoblarGrid();
                RefrescarMontos();
                _lblAyudaEdicion.Text = "Línea cargada para edición: ajuste y pulse + AÑADIR A LA LISTA.";
            }
        }

        private void PoblarGrid()
        {
            _dgv.Rows.Clear();
            foreach (var l in _trabajo.Lineas)
                _dgv.Rows.Add(l.Insumo, l.Cantidad.ToString("0.##"), l.Unidad, l.CostoTotal.ToString("0.00"));
            var vacio = _trabajo.Lineas.Count == 0;
            _lblGridVacio.Visible = vacio;
            _dgv.Visible = !vacio;
            if (vacio)
                _lblGridVacio.BringToFront();
        }

        private void RefrescarMontos()
        {
            var total = _trabajo.Lineas.Sum(x => x.CostoTotal);
            _lblCostoIngredientes.Text = "$ " + total.ToString("0.00");
            var cpp = _numPorciones.Value <= 0 ? 0 : Math.Round(total / _numPorciones.Value, 2, MidpointRounding.AwayFromZero);
            _lblCostoPorcion.Text = "$ " + cpp.ToString("0.00");
        }

        private static class Diseno
        {
            public static readonly Color TealPrimario = Color.FromArgb(26, 82, 118);
            public static readonly Color TealOscuro = Color.FromArgb(21, 67, 96);
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
                new Font("Segoe UI", t, FontStyle.Bold, GraphicsUnit.Point);

            public static Font FuenteCuerpo(float t = 10f) =>
                new Font("Segoe UI", t, FontStyle.Regular, GraphicsUnit.Point);

            public static Font FuenteBotonMayus(float t = 9.5f) =>
                new Font("Segoe UI", t, FontStyle.Bold, GraphicsUnit.Point);

            public static void BotonPrimario(Button b)
            {
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 0;
                b.BackColor = TealPrimario;
                b.ForeColor = Color.White;
                b.Font = FuenteBotonMayus();
                b.Cursor = Cursors.Hand;
                b.UseVisualStyleBackColor = false;
                b.MouseEnter += (_, _) => { b.BackColor = TealOscuro; };
                b.MouseLeave += (_, _) => { b.BackColor = TealPrimario; };
            }

            public static void BotonSecundario(Button b)
            {
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 0;
                b.BackColor = BotonCancelarFondo;
                b.ForeColor = BotonCancelarTexto;
                b.Font = FuenteBotonMayus();
                b.Cursor = Cursors.Hand;
                b.UseVisualStyleBackColor = false;
            }
        }
    }
}
