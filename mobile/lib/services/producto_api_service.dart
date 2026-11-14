import 'dart:convert';
import 'dart:io' show Platform;

import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:http/http.dart' as http;

import '../models/producto.dart';

/// Capa de acceso a la API REST de catProductos.
class ProductoApiService {
  final http.Client _client;
  final String baseUrl;

  ProductoApiService({http.Client? client, String? baseUrl})
      : _client = client ?? http.Client(),
        baseUrl = baseUrl ?? _defaultBaseUrl();

  static String _defaultBaseUrl() {
    const envUrl = String.fromEnvironment('API_BASE_URL');
    if (envUrl.isNotEmpty) return envUrl;
    if (kIsWeb) return '';
    if (Platform.isAndroid) return 'http://10.0.2.2:5000';
    return 'http://localhost:5000';
  }

  Uri _uri(String path, [Map<String, String>? query]) {
    final uri = Uri.parse('$baseUrl$path');
    if (query == null || query.isEmpty) return uri;
    return uri.replace(queryParameters: query);
  }

  Future<List<Producto>> getAll({
    String? search,
    TipoProducto? tipo,
    bool? estatus,
  }) async {
    final query = <String, String>{};
    if (search != null && search.trim().isNotEmpty) {
      query['search'] = search.trim();
    }
    if (tipo != null) query['tipo'] = tipo.apiValue;
    if (estatus != null) query['estatus'] = estatus.toString();

    final resp = await _client.get(_uri('/api/productos', query));
    if (resp.statusCode != 200) {
      throw ApiException('Error al listar productos (${resp.statusCode}).');
    }
    final data = jsonDecode(resp.body) as List<dynamic>;
    return data
        .map((e) => Producto.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<ProductosResumen> getResumen() async {
    final resp = await _client.get(_uri('/api/productos/resumen'));
    if (resp.statusCode != 200) {
      throw ApiException('Error al obtener resumen (${resp.statusCode}).');
    }
    return ProductosResumen.fromJson(
        jsonDecode(resp.body) as Map<String, dynamic>);
  }

  Future<Producto> getById(int id) async {
    final resp = await _client.get(_uri('/api/productos/$id'));
    if (resp.statusCode != 200) {
      throw ApiException('No se encontró el producto $id (${resp.statusCode}).');
    }
    return Producto.fromJson(jsonDecode(resp.body) as Map<String, dynamic>);
  }

  Future<Producto> create(Producto producto) async {
    final resp = await _client.post(
      _uri('/api/productos'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode(producto.toJson()),
    );
    if (resp.statusCode != 201) {
      throw ApiException('No se pudo crear el producto (${resp.statusCode}).');
    }
    return Producto.fromJson(jsonDecode(resp.body) as Map<String, dynamic>);
  }

  Future<void> update(int id, Producto producto) async {
    final resp = await _client.put(
      _uri('/api/productos/$id'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode(producto.toJson()),
    );
    if (resp.statusCode != 204) {
      throw ApiException(
          'No se pudo actualizar el producto (${resp.statusCode}).');
    }
  }

  Future<Producto> ajustarExistencias(int id, int delta) async {
    final resp = await _client.patch(
      _uri('/api/productos/$id/existencias'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'delta': delta}),
    );
    if (resp.statusCode != 200) {
      String msg = 'No se pudieron ajustar existencias (${resp.statusCode}).';
      try {
        final body = jsonDecode(resp.body) as Map<String, dynamic>;
        if (body['message'] != null) msg = body['message'].toString();
      } catch (_) {}
      throw ApiException(msg);
    }
    return Producto.fromJson(jsonDecode(resp.body) as Map<String, dynamic>);
  }

  Future<void> delete(int id) async {
    final resp = await _client.delete(_uri('/api/productos/$id'));
    if (resp.statusCode != 204) {
      throw ApiException(
          'No se pudo eliminar el producto (${resp.statusCode}).');
    }
  }

  void dispose() => _client.close();
}

class ApiException implements Exception {
  final String message;
  ApiException(this.message);
  @override
  String toString() => message;
}
