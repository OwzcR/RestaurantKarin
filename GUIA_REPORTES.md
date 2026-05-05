# 📊 Guía de Reportes - KarinRestaurant

## 🎯 Características Implementadas

### 1. **Validación Robusta de Campos**
- ✅ Verifica que las fechas no sean nulas
- ✅ Valida que fecha inicio ≤ fecha fin
- ✅ Muestra errores con ErrorProvider y MessageBox
- ✅ Método: `ValidarCampos()` → retorna `bool`

### 2. **Conexión a Base de Datos SQLite**
- ✅ Usa DatabaseHelper con cadena de conexión centralizada
- ✅ Métodos de consulta con parámetros parametrizados (previene SQL Injection)
- ✅ Validación de conexión antes de ejecutar consultas

### 3. **Consultas de Reportes (4 tipos)**

#### **Reporte de Ventas**
```csharp
DatabaseHelper.ObtenerReporteVentas(DateTime inicio, DateTime fin)
```
- Ventas totales por día
- Cantidad de órdenes
- Desglose completadas/pendientes

#### **Reporte de Productos Más Vendidos**
```csharp
DatabaseHelper.ObtenerReporteProductosMasVendidos(DateTime inicio, DateTime fin)
```
- Top 10 productos
- Cantidad vendida
- Ingresos totales por producto

#### **Reporte de Ingresos por Empleado**
```csharp
DatabaseHelper.ObtenerReporteIngresosPorEmpleado(DateTime inicio, DateTime fin)
```
- Ingresos generados por empleado
- Cantidad de órdenes procesadas
- Ranking de vendedores

#### **Reporte de Consumo de Inventario**
```csharp
DatabaseHelper.ObtenerReporteConsumoInventario(DateTime inicio, DateTime fin)
```
- Stock actual de insumos
- Costo unitario
- Costo total por insumo

### 4. **Actualización Dinámica de UI**

#### **Tarjeta de Ventas**
- `_lblTotalVentas` → Muestra total en $
- `_lblCantidadOrdenes` → Muestra número de órdenes
- `_pnlChart` → Gráfico que se redibuja con datos reales

#### **Tarjeta de Productos**
- Se limpia y repuebla con top 4 productos
- Barra de progreso por cantidad vendida
- Total de ventas actualizado

#### **Tarjeta de Ingresos por Empleado**
- Se limpia y repuebla con top 4 empleados
- Barras de progreso por ingresos
- Botones de exportación se mantienen

#### **Tarjeta de Inventario**
- Se limpia y repuebla con insumos
- Porcentaje de consumo calculado automáticamente
- Stock actual mostrado

---

## 🔄 Flujo de Uso

```
Usuario selecciona fechas
	↓
Hace clic en "VER REPORTE"
	↓
ValidarCampos() verifica fechas
	↓
CargarReportes() ejecuta 4 consultas SQL
	↓
ProcesarReporte*() actualiza UI dinámicamente
	↓
Usuario ve datos en tiempo real
```

---

## 🛡️ Seguridad Implementada

1. **Prevención de SQL Injection**
   - Todas las consultas usan parámetros (@inicio, @fin)
   - Método `SanitizarSQL()` disponible para entrada de usuario

2. **Validación de Entrada**
   - `EsIDValido()` - valida IDs numéricos
   - `EsNumerico()` - valida números decimales
   - `SanitizarNumerico()` - extrae solo dígitos

3. **Manejo de Errores**
   - Try-catch en todas las consultas
   - Mensajes de error informativo

---

## 📝 Métodos de Sanitización

```csharp
// Sanitizar texto (previene inyección SQL)
string textoSeguro = FormReportes.SanitizarSQL(txtBusqueda.Text);

// Sanitizar números
string numeroSeguro = FormReportes.SanitizarNumerico(txtID.Text);

// Validar ID
if (FormReportes.EsIDValido(id))
{
	// Proceder con seguridad
}
```

---

## 🎨 Referencia de Controles

| Control | Variable Global | Uso |
|---------|-----------------|-----|
| DateTimePicker Inicio | `_dtpInicio` | Fecha de inicio del reporte |
| DateTimePicker Fin | `_dtpFin` | Fecha de fin del reporte |
| Label Total Ventas | `_lblTotalVentas` | Muestra $ total |
| Label Órdenes | `_lblCantidadOrdenes` | Muestra cantidad |
| Panel Gráfico | `_pnlChart` | Gráfico de barras |
| Card Productos | `_cardProd` | Tabla productos |
| Card Empleados | `_cardEmp` | Tabla empleados |
| Card Inventario | `_cardInv` | Tabla inventario |

---

## 🚀 Próximas Mejoras (Opcionales)

- [ ] Gráficos más interactivos (Chart control)
- [ ] Filtrado por categoría/empleado
- [ ] Exportación a PDF/Excel reales
- [ ] Caché de datos para mejor rendimiento
- [ ] Reportes programados por email

---

**Fecha de creación:** 2026
**Versión:** 1.0
**Estado:** ✅ Producción

