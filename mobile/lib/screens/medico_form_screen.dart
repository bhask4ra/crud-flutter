import 'package:flutter/material.dart';

import '../models/medico.dart';
import '../services/medico_api_service.dart';

/// Formulario de alta y edición. Si [medico] es null, es alta.
class MedicoFormScreen extends StatefulWidget {
  final MedicoApiService api;
  final Medico? medico;
  const MedicoFormScreen({super.key, required this.api, this.medico});

  @override
  State<MedicoFormScreen> createState() => _MedicoFormScreenState();
}

class _MedicoFormScreenState extends State<MedicoFormScreen> {
  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _nombre;
  late final TextEditingController _apPaterno;
  late final TextEditingController _apMaterno;
  late final TextEditingController _telefono;
  late final TextEditingController _direccion;
  late final TextEditingController _especialidad;
  late bool _estatus;
  bool _saving = false;

  bool get _isEdit => widget.medico != null;

  @override
  void initState() {
    super.initState();
    final m = widget.medico;
    _nombre = TextEditingController(text: m?.nombre ?? '');
    _apPaterno = TextEditingController(text: m?.apPaterno ?? '');
    _apMaterno = TextEditingController(text: m?.apMaterno ?? '');
    _telefono = TextEditingController(text: m?.telefono ?? '');
    _direccion = TextEditingController(text: m?.direccion ?? '');
    _especialidad = TextEditingController(text: m?.especialidad ?? '');
    _estatus = m?.estatus ?? true;
  }

  @override
  void dispose() {
    _nombre.dispose();
    _apPaterno.dispose();
    _apMaterno.dispose();
    _telefono.dispose();
    _direccion.dispose();
    _especialidad.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _saving = true);

    final medico = Medico(
      idMedico: widget.medico?.idMedico,
      nombre: _nombre.text.trim(),
      apPaterno: _apPaterno.text.trim(),
      apMaterno: _apMaterno.text.trim().isEmpty ? null : _apMaterno.text.trim(),
      telefono: _telefono.text.trim().isEmpty ? null : _telefono.text.trim(),
      direccion: _direccion.text.trim().isEmpty ? null : _direccion.text.trim(),
      especialidad: _especialidad.text.trim(),
      estatus: _estatus,
    );

    try {
      if (_isEdit) {
        await widget.api.update(widget.medico!.idMedico!, medico);
      } else {
        await widget.api.create(medico);
      }
      if (!mounted) return;
      Navigator.pop(context, true);
    } catch (e) {
      if (!mounted) return;
      setState(() => _saving = false);
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('$e')));
    }
  }

  String? _required(String? v) =>
      (v == null || v.trim().isEmpty) ? 'Campo obligatorio' : null;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(_isEdit ? 'Editar médico' : 'Nuevo médico')),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            TextFormField(
              controller: _nombre,
              decoration: const InputDecoration(labelText: 'Nombre *'),
              validator: _required,
            ),
            TextFormField(
              controller: _apPaterno,
              decoration: const InputDecoration(labelText: 'Apellido paterno *'),
              validator: _required,
            ),
            TextFormField(
              controller: _apMaterno,
              decoration: const InputDecoration(labelText: 'Apellido materno'),
            ),
            TextFormField(
              controller: _telefono,
              decoration: const InputDecoration(labelText: 'Teléfono'),
              keyboardType: TextInputType.phone,
            ),
            TextFormField(
              controller: _direccion,
              decoration: const InputDecoration(labelText: 'Dirección'),
            ),
            TextFormField(
              controller: _especialidad,
              decoration: const InputDecoration(labelText: 'Especialidad *'),
              validator: _required,
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
              label: Text(_isEdit ? 'Guardar cambios' : 'Crear médico'),
            ),
          ],
        ),
      ),
    );
  }
}
