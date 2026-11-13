import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

import 'package:medicos_app/models/medico.dart';
import 'package:medicos_app/services/medico_api_service.dart';

void main() {
  const baseUrl = 'http://localhost:5000';

  group('MedicoApiService', () {
    test('getAll parsea la lista de médicos', () async {
      final mock = MockClient((req) async {
        expect(req.url.path, '/api/medicos');
        expect(req.method, 'GET');
        return http.Response(
          jsonEncode([
            {
              'idMedico': 1,
              'nombre': 'Ana',
              'apPaterno': 'García',
              'apMaterno': 'López',
              'telefono': '555',
              'direccion': 'Calle 1',
              'especialidad': 'Cardiología',
              'estatus': true,
            }
          ]),
          200,
          headers: {'content-type': 'application/json'},
        );
      });

      final api = MedicoApiService(client: mock, baseUrl: baseUrl);
      final medicos = await api.getAll();

      expect(medicos, hasLength(1));
      expect(medicos.first.nombre, 'Ana');
      expect(medicos.first.nombreCompleto, 'Ana García López');
      expect(medicos.first.estatus, isTrue);
    });

    test('create envía POST y devuelve el médico creado', () async {
      final mock = MockClient((req) async {
        expect(req.method, 'POST');
        final body = jsonDecode(req.body) as Map<String, dynamic>;
        expect(body['nombre'], 'Nuevo');
        return http.Response(
          jsonEncode({
            'idMedico': 10,
            'nombre': 'Nuevo',
            'apPaterno': 'Medico',
            'especialidad': 'Pediatría',
            'estatus': true,
          }),
          201,
          headers: {'content-type': 'application/json'},
        );
      });

      final api = MedicoApiService(client: mock, baseUrl: baseUrl);
      final creado = await api.create(const Medico(
        nombre: 'Nuevo',
        apPaterno: 'Medico',
        especialidad: 'Pediatría',
      ));

      expect(creado.idMedico, 10);
    });

    test('update hace PUT y acepta 204', () async {
      final mock = MockClient((req) async {
        expect(req.method, 'PUT');
        expect(req.url.path, '/api/medicos/5');
        return http.Response('', 204);
      });

      final api = MedicoApiService(client: mock, baseUrl: baseUrl);
      await api.update(
        5,
        const Medico(nombre: 'X', apPaterno: 'Y', especialidad: 'E'),
      );
      // Si no lanzó excepción, pasó.
    });

    test('delete hace DELETE y acepta 204', () async {
      final mock = MockClient((req) async {
        expect(req.method, 'DELETE');
        expect(req.url.path, '/api/medicos/3');
        return http.Response('', 204);
      });

      final api = MedicoApiService(client: mock, baseUrl: baseUrl);
      await api.delete(3);
    });

    test('getAll lanza ApiException ante error del servidor', () async {
      final mock = MockClient((req) async => http.Response('boom', 500));
      final api = MedicoApiService(client: mock, baseUrl: baseUrl);
      expect(() => api.getAll(), throwsA(isA<ApiException>()));
    });
  });
}
