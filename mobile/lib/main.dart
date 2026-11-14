import 'package:flutter/material.dart';

import 'services/producto_api_service.dart';
import 'screens/productos_list_screen.dart';

void main() {
  runApp(const ProductosApp());
}

class ProductosApp extends StatelessWidget {
  const ProductosApp({super.key});

  @override
  Widget build(BuildContext context) {
    final api = ProductoApiService();
    return MaterialApp(
      title: 'catProductos',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorSchemeSeed: Colors.teal,
        useMaterial3: true,
      ),
      home: ProductosListScreen(api: api),
    );
  }
}
