import 'dart:convert';
import 'dart:io' show Platform;

import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:http/http.dart' as http;

import '../models/medico.dart';

/// Capa de acceso a la API REST de médicos. Separa la red de la UI (SRP) y es
/// testeable inyectando un [http.Client] simulado.
class MedicoApiService {
  final http.Client _client;
  final String baseUrl;

  MedicoApiService({http.Client? client, String? baseUrl})
      : _client = client ?? http.Client(),
        baseUrl = baseUrl ?? _defaultBaseUrl();

  /// El emulador de Android expone la máquina host en 10.0.2.2.
  /// En desktop/web se usa localhost.
  static String _defaultBaseUrl() {
    if (kIsWeb) return 'http://localhost:5000';
    if (Platform.isAndroid) return 'http://10.0.2.2:5000';
    return 'http://localhost:5000';
  }

  Uri _uri(String path) => Uri.parse('$baseUrl$path');

  Future<List<Medico>> getAll() async {
    final resp = await _client.get(_uri('/api/medicos'));
    if (resp.statusCode != 200) {
      throw ApiException('Error al listar médicos (${resp.statusCode}).');
    }
    final data = jsonDecode(resp.body) as List<dynamic>;
    return data
        .map((e) => Medico.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<Medico> getById(int id) async {
    final resp = await _client.get(_uri('/api/medicos/$id'));
    if (resp.statusCode != 200) {
      throw ApiException('No se encontró el médico $id (${resp.statusCode}).');
    }
    return Medico.fromJson(jsonDecode(resp.body) as Map<String, dynamic>);
  }

  Future<Medico> create(Medico medico) async {
    final resp = await _client.post(
      _uri('/api/medicos'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode(medico.toJson()),
    );
    if (resp.statusCode != 201) {
      throw ApiException('No se pudo crear el médico (${resp.statusCode}).');
    }
    return Medico.fromJson(jsonDecode(resp.body) as Map<String, dynamic>);
  }

  Future<void> update(int id, Medico medico) async {
    final resp = await _client.put(
      _uri('/api/medicos/$id'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode(medico.toJson()),
    );
    if (resp.statusCode != 204) {
      throw ApiException('No se pudo actualizar el médico (${resp.statusCode}).');
    }
  }

  Future<void> delete(int id) async {
    final resp = await _client.delete(_uri('/api/medicos/$id'));
    if (resp.statusCode != 204) {
      throw ApiException('No se pudo eliminar el médico (${resp.statusCode}).');
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
