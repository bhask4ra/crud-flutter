import 'package:flutter/material.dart';

import '../models/medico.dart';
import '../services/medico_api_service.dart';
import 'medico_form_screen.dart';

class MedicosListScreen extends StatefulWidget {
  final MedicoApiService api;
  const MedicosListScreen({super.key, required this.api});

  @override
  State<MedicosListScreen> createState() => _MedicosListScreenState();
}

class _MedicosListScreenState extends State<MedicosListScreen> {
  late Future<List<Medico>> _future;

  @override
  void initState() {
    super.initState();
    _refresh();
  }

  void _refresh() {
    setState(() => _future = widget.api.getAll());
  }

  Future<void> _openForm([Medico? medico]) async {
    final changed = await Navigator.of(context).push<bool>(
      MaterialPageRoute(
        builder: (_) => MedicoFormScreen(api: widget.api, medico: medico),
      ),
    );
    if (changed == true) _refresh();
  }

  Future<void> _delete(Medico medico) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Eliminar médico'),
        content: Text('¿Eliminar a ${medico.nombreCompleto}?'),
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
      await widget.api.delete(medico.idMedico!);
      if (!mounted) return;
      _refresh();
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Médico eliminado.')),
      );
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text('$e')));
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Médicos')),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _openForm(),
        icon: const Icon(Icons.add),
        label: const Text('Nuevo'),
      ),
      body: RefreshIndicator(
        onRefresh: () async => _refresh(),
        child: FutureBuilder<List<Medico>>(
          future: _future,
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Center(child: CircularProgressIndicator());
            }
            if (snapshot.hasError) {
              return _ErrorView(
                  message: '${snapshot.error}', onRetry: _refresh);
            }
            final medicos = snapshot.data ?? [];
            if (medicos.isEmpty) {
              return const Center(child: Text('No hay médicos registrados.'));
            }
            return ListView.separated(
              itemCount: medicos.length,
              separatorBuilder: (_, __) => const Divider(height: 1),
              itemBuilder: (context, i) {
                final m = medicos[i];
                return ListTile(
                  leading: CircleAvatar(
                    backgroundColor:
                        m.estatus ? Colors.teal : Colors.grey.shade400,
                    child: Text(m.nombre.isNotEmpty ? m.nombre[0] : '?'),
                  ),
                  title: Text(m.nombreCompleto),
                  subtitle: Text(
                      '${m.especialidad}  ·  ${m.estatus ? "Activo" : "Inactivo"}'),
                  trailing: IconButton(
                    icon: const Icon(Icons.delete_outline),
                    onPressed: () => _delete(m),
                  ),
                  onTap: () => _openForm(m),
                );
              },
            );
          },
        ),
      ),
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
          child: FilledButton(onPressed: onRetry, child: const Text('Reintentar')),
        ),
      ],
    );
  }
}
