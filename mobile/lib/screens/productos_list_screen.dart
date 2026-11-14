import 'package:flutter/material.dart';

import '../models/producto.dart';
import '../services/producto_api_service.dart';
import 'producto_form_screen.dart';

class ProductosListScreen extends StatefulWidget {
  final ProductoApiService api;
  const ProductosListScreen({super.key, required this.api});

  @override
  State<ProductosListScreen> createState() => _ProductosListScreenState();
}

class _ProductosListScreenState extends State<ProductosListScreen> {
  final _searchCtrl = TextEditingController();
  TipoProducto? _tipoFiltro;
  bool? _estatusFiltro;

  late Future<List<Producto>> _futureProductos;
  late Future<ProductosResumen> _futureResumen;

  @override
  void initState() {
    super.initState();
    _refresh();
  }

  @override
  void dispose() {
    _searchCtrl.dispose();
    super.dispose();
  }

  void _refresh() {
    setState(() {
      _futureProductos = widget.api.getAll(
        search: _searchCtrl.text,
        tipo: _tipoFiltro,
        estatus: _estatusFiltro,
      );
      _futureResumen = widget.api.getResumen();
    });
  }

  Future<void> _openForm([Producto? producto]) async {
    final changed = await Navigator.of(context).push<bool>(
      MaterialPageRoute(
        builder: (_) =>
            ProductoFormScreen(api: widget.api, producto: producto),
      ),
    );
    if (changed == true) _refresh();
  }

  Future<void> _ajustarStock(Producto producto, int delta) async {
    try {
      await widget.api.ajustarExistencias(producto.idProducto!, delta);
      if (!mounted) return;
      _refresh();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('$e')));
    }
  }

  Future<void> _delete(Producto producto) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Eliminar producto'),
        content: Text('¿Eliminar "${producto.nombre}"?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('Eliminar'),
          ),
        ],
      ),
    );
    if (confirm != true) return;
    try {
      await widget.api.delete(producto.idProducto!);
      if (!mounted) return;
      _refresh();
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Producto eliminado.')),
      );
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('$e')));
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Catálogo de Productos')),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _openForm(),
        icon: const Icon(Icons.add),
        label: const Text('Nuevo'),
      ),
      body: Column(
        children: [
          // Funcionalidad 1a: Resumen de inventario (KPIs)
          FutureBuilder<ProductosResumen>(
            future: _futureResumen,
            builder: (context, snap) {
              if (!snap.hasData) return const SizedBox.shrink();
              final r = snap.data!;
              return Padding(
                padding: const EdgeInsets.fromLTRB(12, 8, 12, 4),
                child: Wrap(
                  spacing: 8,
                  runSpacing: 8,
                  children: [
                    _KpiChip(
                      icon: Icons.inventory_2_outlined,
                      label: 'Total: ${r.totalProductos}',
                    ),
                    _KpiChip(
                      icon: Icons.attach_money,
                      label:
                          'Inventario: \$${r.valorTotalInventario.toStringAsFixed(2)}',
                    ),
                    if (r.stockBajo > 0)
                      _KpiChip(
                        icon: Icons.warning_amber_rounded,
                        label: 'Stock bajo: ${r.stockBajo}',
                        color: Colors.orange.shade100,
                      ),
                  ],
                ),
              );
            },
          ),
          // Funcionalidad 1b: Búsqueda y filtros por Tipo/Estatus
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
            child: TextField(
              controller: _searchCtrl,
              decoration: InputDecoration(
                hintText: 'Buscar por nombre...',
                prefixIcon: const Icon(Icons.search),
                suffixIcon: _searchCtrl.text.isNotEmpty
                    ? IconButton(
                        icon: const Icon(Icons.clear),
                        onPressed: () {
                          _searchCtrl.clear();
                          _refresh();
                        },
                      )
                    : null,
                border: const OutlineInputBorder(),
                isDense: true,
              ),
              onSubmitted: (_) => _refresh(),
              onChanged: (_) => _refresh(),
            ),
          ),
          SingleChildScrollView(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.symmetric(horizontal: 12),
            child: Row(
              children: [
                ChoiceChip(
                  label: const Text('Todos'),
                  selected: _tipoFiltro == null,
                  onSelected: (_) {
                    _tipoFiltro = null;
                    _refresh();
                  },
                ),
                const SizedBox(width: 8),
                ChoiceChip(
                  label: const Text('Artículos'),
                  selected: _tipoFiltro == TipoProducto.articulo,
                  onSelected: (_) {
                    _tipoFiltro = TipoProducto.articulo;
                    _refresh();
                  },
                ),
                const SizedBox(width: 8),
                ChoiceChip(
                  label: const Text('Servicios'),
                  selected: _tipoFiltro == TipoProducto.servicio,
                  onSelected: (_) {
                    _tipoFiltro = TipoProducto.servicio;
                    _refresh();
                  },
                ),
                const SizedBox(width: 8),
                FilterChip(
                  label: const Text('Solo activos'),
                  selected: _estatusFiltro == true,
                  onSelected: (selected) {
                    _estatusFiltro = selected ? true : null;
                    _refresh();
                  },
                ),
              ],
            ),
          ),
          const Divider(height: 8),
          Expanded(
            child: RefreshIndicator(
              onRefresh: () async => _refresh(),
              child: FutureBuilder<List<Producto>>(
                future: _futureProductos,
                builder: (context, snapshot) {
                  if (snapshot.connectionState == ConnectionState.waiting) {
                    return const Center(child: CircularProgressIndicator());
                  }
                  if (snapshot.hasError) {
                    return _ErrorView(
                      message: '${snapshot.error}',
                      onRetry: _refresh,
                    );
                  }
                  final productos = snapshot.data ?? [];
                  if (productos.isEmpty) {
                    return const Center(
                        child: Text('No hay productos registrados.'));
                  }
                  return ListView.separated(
                    itemCount: productos.length,
                    separatorBuilder: (_, __) => const Divider(height: 1),
                    itemBuilder: (context, i) {
                      final p = productos[i];
                      final esArticulo = p.tipo == TipoProducto.articulo;
                      final stockBajo =
                          esArticulo && p.estatus && p.existencias < 5;

                      return ListTile(
                        leading: CircleAvatar(
                          backgroundColor:
                              p.estatus ? Colors.teal : Colors.grey.shade400,
                          child: Icon(
                            esArticulo
                                ? Icons.inventory_2
                                : Icons.miscellaneous_services,
                            color: Colors.white,
                          ),
                        ),
                        title: Text(p.nombre),
                        subtitle: Text(
                          '\$${p.precioUnitario.toStringAsFixed(2)}  ·  ${p.tipo.label}'
                          '${esArticulo ? "  ·  Stock: ${p.existencias}${stockBajo ? " (Bajo)" : ""}" : ""}'
                          '  ·  ${p.estatus ? "Activo" : "Inactivo"}',
                        ),
                        // Funcionalidad 2: Ajuste rápido de existencias (+/-) para Artículos
                        trailing: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            if (esArticulo) ...[
                              IconButton(
                                tooltip: 'Salida (-1)',
                                icon: const Icon(Icons.remove_circle_outline),
                                onPressed: p.existencias > 0
                                    ? () => _ajustarStock(p, -1)
                                    : null,
                              ),
                              IconButton(
                                tooltip: 'Entrada (+1)',
                                icon: const Icon(Icons.add_circle_outline),
                                onPressed: () => _ajustarStock(p, 1),
                              ),
                            ],
                            IconButton(
                              tooltip: 'Eliminar',
                              icon: const Icon(Icons.delete_outline),
                              onPressed: () => _delete(p),
                            ),
                          ],
                        ),
                        onTap: () => _openForm(p),
                      );
                    },
                  );
                },
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _KpiChip extends StatelessWidget {
  final IconData icon;
  final String label;
  final Color? color;
  const _KpiChip({required this.icon, required this.label, this.color});

  @override
  Widget build(BuildContext context) {
    return Chip(
      avatar: Icon(icon, size: 16),
      label: Text(label),
      backgroundColor: color,
    );
  }
}

class _ErrorView extends StatelessWidget {
  final String message;
  final VoidCallback onRetry;
  const _ErrorView({required this.message, required this.onRetry});

  @override
  Widget build(BuildContext context) {
    return ListView(
      children: [
        const SizedBox(height: 80),
        const Icon(Icons.cloud_off, size: 48),
        const SizedBox(height: 8),
        Center(
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Text(message, textAlign: TextAlign.center),
          ),
        ),
        Center(
          child:
              FilledButton(onPressed: onRetry, child: const Text('Reintentar')),
        ),
      ],
    );
  }
}
