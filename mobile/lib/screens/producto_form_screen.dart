import 'package:flutter/material.dart';

import '../models/producto.dart';
import '../services/producto_api_service.dart';

/// Formulario de alta y edición de catProductos.
class ProductoFormScreen extends StatefulWidget {
  final ProductoApiService api;
  final Producto? producto;
  const ProductoFormScreen({super.key, required this.api, this.producto});

  @override
  State<ProductoFormScreen> createState() => _ProductoFormScreenState();
}

class _ProductoFormScreenState extends State<ProductoFormScreen> {
  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _nombre;
  late final TextEditingController _precio;
  late final TextEditingController _existencias;
  late TipoProducto _tipo;
  late bool _estatus;
  bool _saving = false;

  bool get _isEdit => widget.producto != null;

  @override
  void initState() {
    super.initState();
    final p = widget.producto;
    _nombre = TextEditingController(text: p?.nombre ?? '');
    _precio = TextEditingController(
        text: p != null ? p.precioUnitario.toStringAsFixed(2) : '');
    _existencias =
        TextEditingController(text: p != null ? '${p.existencias}' : '0');
    _tipo = p?.tipo ?? TipoProducto.articulo;
    _estatus = p?.estatus ?? true;
  }

  @override
  void dispose() {
    _nombre.dispose();
    _precio.dispose();
    _existencias.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _saving = true);

    final precio = double.parse(_precio.text.trim());
    final existencias = _tipo == TipoProducto.servicio
        ? 0
        : int.parse(_existencias.text.trim());

    final producto = Producto(
      idProducto: widget.producto?.idProducto,
      nombre: _nombre.text.trim(),
      precioUnitario: precio,
      estatus: _estatus,
      existencias: existencias,
      tipo: _tipo,
    );

    try {
      if (_isEdit) {
        await widget.api.update(widget.producto!.idProducto!, producto);
      } else {
        await widget.api.create(producto);
      }
      if (!mounted) return;
      Navigator.pop(context, true);
    } catch (e) {
      if (!mounted) return;
      setState(() => _saving = false);
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('$e')));
    }
  }

  @override
  Widget build(BuildContext context) {
    final esServicio = _tipo == TipoProducto.servicio;
    return Scaffold(
      appBar: AppBar(
        title: Text(_isEdit ? 'Editar producto' : 'Nuevo producto'),
      ),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            TextFormField(
              controller: _nombre,
              decoration: const InputDecoration(labelText: 'Nombre *'),
              validator: (v) =>
                  (v == null || v.trim().isEmpty) ? 'Campo obligatorio' : null,
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _precio,
              decoration: const InputDecoration(
                labelText: 'Precio unitario *',
                prefixText: '\$ ',
              ),
              keyboardType:
                  const TextInputType.numberWithOptions(decimal: true),
              validator: (v) {
                final val = double.tryParse((v ?? '').trim());
                if (val == null || val < 0) {
                  return 'Ingresa un precio válido (>= 0)';
                }
                return null;
              },
            ),
            const SizedBox(height: 12),
            DropdownButtonFormField<TipoProducto>(
              initialValue: _tipo,
              decoration: const InputDecoration(labelText: 'Tipo *'),
              items: TipoProducto.values
                  .map((t) => DropdownMenuItem(value: t, child: Text(t.label)))
                  .toList(),
              onChanged: (val) {
                if (val == null) return;
                setState(() {
                  _tipo = val;
                  if (val == TipoProducto.servicio) {
                    _existencias.text = '0';
                  }
                });
              },
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _existencias,
              enabled: !esServicio,
              decoration: InputDecoration(
                labelText: 'Existencias',
                helperText: esServicio
                    ? 'Los servicios no son inventariables (existencias = 0)'
                    : null,
              ),
              keyboardType: TextInputType.number,
              validator: (v) {
                if (esServicio) return null;
                final val = int.tryParse((v ?? '').trim());
                if (val == null || val < 0) {
                  return 'Ingresa un número entero válido (>= 0)';
                }
                return null;
              },
            ),
            const SizedBox(height: 8),
            SwitchListTile(
              title: const Text('Estatus (activo)'),
              value: _estatus,
              onChanged: (v) => setState(() => _estatus = v),
            ),
            const SizedBox(height: 16),
            FilledButton.icon(
              onPressed: _saving ? null : _save,
              icon: _saving
                  ? const SizedBox(
                      width: 18,
                      height: 18,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Icon(Icons.save),
              label: Text(_isEdit ? 'Guardar cambios' : 'Crear producto'),
            ),
          ],
        ),
      ),
    );
  }
}
