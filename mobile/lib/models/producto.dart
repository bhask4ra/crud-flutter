/// Clasificación del producto en catProductos.
enum TipoProducto {
  articulo,
  servicio;

  String get apiValue => switch (this) {
        TipoProducto.articulo => 'Articulo',
        TipoProducto.servicio => 'Servicio',
      };

  String get label => switch (this) {
        TipoProducto.articulo => 'Artículo (Inventariable)',
        TipoProducto.servicio => 'Servicio',
      };

  static TipoProducto fromApi(dynamic value) {
    final s = value?.toString().toLowerCase() ?? '';
    if (s == 'servicio' || s == '2') return TipoProducto.servicio;
    return TipoProducto.articulo;
  }
}

/// Modelo de dominio de catProductos en el cliente.
class Producto {
  final int? idProducto;
  final String nombre;
  final double precioUnitario;
  final bool estatus;
  final int existencias;
  final TipoProducto tipo;

  const Producto({
    this.idProducto,
    required this.nombre,
    required this.precioUnitario,
    this.estatus = true,
    this.existencias = 0,
    this.tipo = TipoProducto.articulo,
  });

  factory Producto.fromJson(Map<String, dynamic> json) => Producto(
        idProducto: json['idProducto'] as int?,
        nombre: json['nombre'] as String? ?? '',
        precioUnitario: (json['precioUnitario'] as num?)?.toDouble() ?? 0.0,
        estatus: json['estatus'] as bool? ?? true,
        existencias: (json['existencias'] as num?)?.toInt() ?? 0,
        tipo: TipoProducto.fromApi(json['tipo']),
      );

  Map<String, dynamic> toJson() => {
        'nombre': nombre,
        'precioUnitario': precioUnitario,
        'estatus': estatus,
        'existencias': tipo == TipoProducto.servicio ? 0 : existencias,
        'tipo': tipo.apiValue,
      };

  Producto copyWith({
    int? idProducto,
    String? nombre,
    double? precioUnitario,
    bool? estatus,
    int? existencias,
    TipoProducto? tipo,
  }) =>
      Producto(
        idProducto: idProducto ?? this.idProducto,
        nombre: nombre ?? this.nombre,
        precioUnitario: precioUnitario ?? this.precioUnitario,
        estatus: estatus ?? this.estatus,
        existencias: existencias ?? this.existencias,
        tipo: tipo ?? this.tipo,
      );
}

/// Resumen estadístico del catálogo de productos.
class ProductosResumen {
  final int totalProductos;
  final int activos;
  final int inactivos;
  final int totalArticulos;
  final int totalServicios;
  final int stockBajo;
  final double valorTotalInventario;

  const ProductosResumen({
    required this.totalProductos,
    required this.activos,
    required this.inactivos,
    required this.totalArticulos,
    required this.totalServicios,
    required this.stockBajo,
    required this.valorTotalInventario,
  });

  factory ProductosResumen.fromJson(Map<String, dynamic> json) =>
      ProductosResumen(
        totalProductos: (json['totalProductos'] as num?)?.toInt() ?? 0,
        activos: (json['activos'] as num?)?.toInt() ?? 0,
        inactivos: (json['inactivos'] as num?)?.toInt() ?? 0,
        totalArticulos: (json['totalArticulos'] as num?)?.toInt() ?? 0,
        totalServicios: (json['totalServicios'] as num?)?.toInt() ?? 0,
        stockBajo: (json['stockBajo'] as num?)?.toInt() ?? 0,
        valorTotalInventario:
            (json['valorTotalInventario'] as num?)?.toDouble() ?? 0.0,
      );
}
