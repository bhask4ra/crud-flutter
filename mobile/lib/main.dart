import 'package:flutter/material.dart';

import 'services/medico_api_service.dart';
import 'screens/medicos_list_screen.dart';

void main() {
  runApp(const MedicosApp());
}

class MedicosApp extends StatelessWidget {
  const MedicosApp({super.key});

  @override
  Widget build(BuildContext context) {
    final api = MedicoApiService();
    return MaterialApp(
      title: 'CRUD Médicos',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorSchemeSeed: Colors.teal,
        useMaterial3: true,
      ),
      home: MedicosListScreen(api: api),
    );
  }
}
