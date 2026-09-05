// Código desofuscado
// Tipo: IndexedItem<TItem, TIndex> (anteriormente Class6<T0, T1>)
// Assembly: Server.Game, Version=1.1.25163.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

/// <summary>
/// Representa un elemento con un índice o identificador asociado
/// </summary>
/// <typeparam name="TItem">El tipo del elemento</typeparam>
/// <typeparam name="TIndex">El tipo del índice o identificador</typeparam>
[CompilerGenerated]
internal sealed class IndexedItem<TItem, TIndex>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly TItem _item;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly TIndex _index;

    /// <summary>
    /// Obtiene el elemento
    /// </summary>
    public TItem item => this._item;

    /// <summary>
    /// Obtiene el índice o identificador
    /// </summary>
    public TIndex inx => this._index;

    /// <summary>
    /// Inicializa una nueva instancia de IndexedItem
    /// </summary>
    /// <param name="item">El elemento</param>
    /// <param name="index">El índice o identificador</param>
    [DebuggerHidden]
    public IndexedItem(TItem item, TIndex index)
    {
        this._item = item;
        this._index = index;
    }

    /// <summary>
    /// Determina si el objeto especificado es igual al objeto actual
    /// </summary>
    /// <param name="obj">El objeto a comparar</param>
    /// <returns>true si los objetos son iguales; de lo contrario, false</returns>
    [DebuggerHidden]
    public override bool Equals(object obj)
    {
        IndexedItem<TItem, TIndex> other = obj as IndexedItem<TItem, TIndex>;

        if (this == other)
            return true;

        return other != null &&
               EqualityComparer<TItem>.Default.Equals(this._item, other._item) &&
               EqualityComparer<TIndex>.Default.Equals(this._index, other._index);
    }

    /// <summary>
    /// Calcula el código hash para este objeto
    /// </summary>
    /// <returns>Un código hash para el objeto actual</returns>
    [DebuggerHidden]
    public override int GetHashCode()
    {
        return (EqualityComparer<TItem>.Default.GetHashCode(this._item) - 1959725626) * -1521134295 +
               EqualityComparer<TIndex>.Default.GetHashCode(this._index);
    }

    /// <summary>
    /// Devuelve una representación en cadena del objeto
    /// </summary>
    /// <returns>Una cadena que representa el objeto actual</returns>
    [DebuggerHidden]
    
    public override string ToString()
    {
        object[] formatArgs = new object[2];

        TItem itemValue = this._item;
        formatArgs[0] = itemValue?.ToString();

        TIndex indexValue = this._index;
        formatArgs[1] = indexValue?.ToString();

        return FormatString(null, "{{ item = {0}, inx = {1} }}", formatArgs);
    }

    /// <summary>
    /// Método auxiliar para formatear cadenas
    /// </summary>
    /// <param name="provider">Proveedor de formato</param>
    /// <param name="format">Cadena de formato</param>
    /// <param name="args">Argumentos para el formato</param>
    /// <returns>Cadena formateada</returns>
    private static string FormatString(IFormatProvider provider, string format, object[] args)
    {
        return string.Format(provider, format, args);
    }
}