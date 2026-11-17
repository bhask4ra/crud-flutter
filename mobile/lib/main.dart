import 'package:flutter/material.dart';

import 'screens/auth_screen.dart';
import 'screens/productos_list_screen.dart';
import 'services/auth_service.dart';
import 'services/producto_api_service.dart';

void main() {
  runApp(const ProductosApp());
}

class ProductosApp extends StatefulWidget {
  const ProductosApp({super.key});

  @override
  State<ProductosApp> createState() => _ProductosAppState();
}

class _ProductosAppState extends State<ProductosApp> {
  late final AuthService _auth;
  late final ProductoApiService _api;

  @override
  void initState() {
    super.initState();
    _auth = AuthService();
    _api = ProductoApiService(tokenProvider: () => _auth.token);
  }

  @override
  void dispose() {
    _api.dispose();
    _auth.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return ListenableBuilder(
      listenable: _auth,
      builder: (context, _) {
        return MaterialApp(
          title: 'catProductos',
          debugShowCheckedModeBanner: false,
          theme: ThemeData(
            colorSchemeSeed: Colors.teal,
            useMaterial3: true,
          ),
          home: _auth.isAuthenticated
              ? ProductosListScreen(
                  api: _api,
                  userName: _auth.currentUser?.nombre,
                  onLogout: _auth.logout,
                )
              : AuthScreen(auth: _auth),
        );
      },
    );
  }
}
