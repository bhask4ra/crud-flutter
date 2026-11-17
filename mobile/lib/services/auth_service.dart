import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;

import '../models/usuario.dart';
import 'api_config.dart';
import 'producto_api_service.dart';

/// Servicio de autenticación (login, registro y sesión actual).
class AuthService extends ChangeNotifier {
  final http.Client _client;
  final String baseUrl;

  AuthSession? _session;

  AuthService({http.Client? client, String? baseUrl})
      : _client = client ?? http.Client(),
        baseUrl = baseUrl ?? ApiConfig.defaultBaseUrl();

  AuthSession? get session => _session;
  Usuario? get currentUser => _session?.usuario;
  String? get token => _session?.token;
  bool get isAuthenticated =>
      _session != null && _session!.token.isNotEmpty;

  Uri _uri(String path) => Uri.parse('$baseUrl$path');

  Future<AuthSession> login({
    required String email,
    required String password,
  }) async {
    final resp = await _client.post(
      _uri('/api/auth/login'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'email': email.trim(),
        'password': password,
      }),
    );

    if (resp.statusCode != 200) {
      throw ApiException(_extractError(
          resp, 'No se pudo iniciar sesión (${resp.statusCode}).'));
    }

    _session = AuthSession.fromJson(
        jsonDecode(resp.body) as Map<String, dynamic>);
    notifyListeners();
    return _session!;
  }

  Future<AuthSession> register({
    required String nombre,
    required String email,
    required String password,
  }) async {
    final resp = await _client.post(
      _uri('/api/auth/register'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'nombre': nombre.trim(),
        'email': email.trim(),
        'password': password,
      }),
    );

    if (resp.statusCode != 201) {
      throw ApiException(_extractError(
          resp, 'No se pudo registrar la cuenta (${resp.statusCode}).'));
    }

    _session = AuthSession.fromJson(
        jsonDecode(resp.body) as Map<String, dynamic>);
    notifyListeners();
    return _session!;
  }

  void logout() {
    if (_session == null) return;
    _session = null;
    notifyListeners();
  }

  String _extractError(http.Response resp, String fallback) {
    try {
      final body = jsonDecode(resp.body);
      if (body is Map<String, dynamic>) {
        if (body['message'] != null) return body['message'].toString();
        if (body['title'] != null) return body['title'].toString();
      }
    } catch (_) {}
    return fallback;
  }

  @override
  void dispose() {
    _client.close();
    super.dispose();
  }
}
