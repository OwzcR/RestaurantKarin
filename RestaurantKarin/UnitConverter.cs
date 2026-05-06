using System;
using System.Collections.Generic;

namespace RestaurantKarin
{
    internal static class UnitConverter
    {
        // Factor to convert a unit to its base.
        // Weight base: grams (g)  |  Volume base: mL  |  Quantity base: pz (1)
        private static readonly Dictionary<string, (decimal Factor, string Type)> _map =
            new(StringComparer.OrdinalIgnoreCase)
        {
            // ── Peso ──────────────────────────────────────────────────────────
            { "kg",           (1000m,      "Peso") },
            { "kilogramo",    (1000m,      "Peso") },
            { "kilogramos",   (1000m,      "Peso") },
            { "g",            (1m,         "Peso") },
            { "gramo",        (1m,         "Peso") },
            { "gramos",       (1m,         "Peso") },
            { "mg",           (0.001m,     "Peso") },
            { "miligramo",    (0.001m,     "Peso") },
            { "miligramos",   (0.001m,     "Peso") },
            { "oz",           (28.3495m,   "Peso") },
            { "onza",         (28.3495m,   "Peso") },
            { "onzas",        (28.3495m,   "Peso") },
            { "lb",           (453.592m,   "Peso") },
            { "libra",        (453.592m,   "Peso") },
            { "libras",       (453.592m,   "Peso") },
            // ── Volumen ───────────────────────────────────────────────────────
            { "lts",          (1000m,      "Volumen") },
            { "litro",        (1000m,      "Volumen") },
            { "litros",       (1000m,      "Volumen") },
            { "l",            (1000m,      "Volumen") },
            { "ml",           (1m,         "Volumen") },
            { "mililitro",    (1m,         "Volumen") },
            { "mililitros",   (1m,         "Volumen") },
            { "cup",          (240m,       "Volumen") },
            { "taza",         (240m,       "Volumen") },
            { "tazas",        (240m,       "Volumen") },
            { "tbsp",         (15m,        "Volumen") },
            { "cucharada",    (15m,        "Volumen") },
            { "cucharadas",   (15m,        "Volumen") },
            { "tsp",          (5m,         "Volumen") },
            { "cucharadita",  (5m,         "Volumen") },
            { "cucharaditas", (5m,         "Volumen") },
            { "piz",          (1m,         "Volumen") },
            { "pizca",        (1m,         "Volumen") },
            { "pizcas",       (1m,         "Volumen") },
            // ── Cantidad ──────────────────────────────────────────────────────
            { "pz",           (1m,         "Cantidad") },
            { "pieza",        (1m,         "Cantidad") },
            { "piezas",       (1m,         "Cantidad") },
            { "pcs",          (1m,         "Cantidad") },
            { "doc",          (12m,        "Cantidad") },
            { "docena",       (12m,        "Cantidad") },
            { "docenas",      (12m,        "Cantidad") },
            { "rbd",          (1m,         "Cantidad") },
            { "rebanada",     (1m,         "Cantidad") },
            { "rebanadas",    (1m,         "Cantidad") },
            { "unidad",       (1m,         "Cantidad") },
            { "unidades",     (1m,         "Cantidad") },
        };

        /// <summary>
        /// Converts <paramref name="qty"/> from <paramref name="from"/> to <paramref name="to"/>.
        /// Returns null when the units are incompatible or unknown.
        /// </summary>
        public static decimal? Convert(decimal qty, string from, string to)
        {
            if (!_map.TryGetValue(from.Trim(), out var f)) return null;
            if (!_map.TryGetValue(to.Trim(),   out var t)) return null;
            if (f.Type != t.Type) return null;

            return qty * f.Factor / t.Factor;
        }
    }
}
