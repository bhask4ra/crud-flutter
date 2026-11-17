import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

import 'package:medicos_app/services/auth_service.dart';
import 'package:medicos_app/services/producto_api_service.dart';

void main() {
  const baseUrl = 'http://localhost:5000';

  group('AuthService', () {
    test('login guarda sesión y token al responder 200', () async {
      final mock = MockClient((req) async {
        expect(req.method, 'POST');
        expect(req.url.path, '/api/auth/login');
        final body = jsonDecode(req.body) as Map<String, dynamic>;
        expect(body['email'], 'ana@test.com');
        expect(body['password'], 'secret123');
        return http.Response(
          jsonEncode({
            'token': 'jwt-token-123',
            'expiresAt': '2026-11-18T00:00:00Z',
            'usuario': {
              'idUsuario': 1,
              'nombre': 'Ana López',
              'email': 'ana@test.com',
            },
          }),
          200,
          headers: {'content-type': 'application/json'},
        );
      });

      final auth = AuthService(client: mock, baseUrl: baseUrl);
      final session =
          await auth.login(email: 'ana@test.com', password: 'secret123');

      expect(auth.isAuthenticated, isTrue);
      expect(auth.token, 'jwt-token-123');
      expect(session.usuario.nombre, 'Ana López');

      auth.logout();
      expect(auth.isAuthenticated, isFalse);
      expect(auth.token, isNull);
    });

    test('register guarda sesión al responder 201', () async {
      final mock = MockClient((req) async {
        expect(req.method, 'POST');
        expect(req.url.path, '/api/auth/register');
        final body = jsonDecode(req.body) as Map<String, dynamic>;
        expect(body['nombre'], 'Luis');
        return http.Response(
          jsonEncode({
            'token': 'jwt-reg-456',
            'expiresAt': '2026-11-18T00:00:00Z',
            'usuario': {
              'idUsuario': 2,
              'nombre': 'Luis',
              'email': 'luis@test.com',
            },
          }),
          201,
          headers: {'content-type': 'application/json'},
        );
      });

      final auth = AuthService(client: mock, baseUrl: baseUrl);
      await auth.register(
        nombre: 'Luis',
        email: 'luis@test.com',
        password: 'password123',
      );

      expect(auth.isAuthenticated, isTrue);
      expect(auth.token, 'jwt-reg-456');
    });

    test('login lanza ApiException con mensaje del servidor ante 401', () async {
      final mock = MockClient((req) async => http.Response(
            jsonEncode({'message': 'Correo o contraseña incorrectos.'}),
            401,
            headers: {'content-type': 'application/json'},
          ));

      final auth = AuthService(client: mock, baseUrl: baseUrl);
      expect(
        () => auth.login(email: 'x@test.com', password: 'bad'),
        throwsA(predicate(
            (e) => e is ApiException && e.message.contains('incorrectos'))),
      );
    });
  });

  group('ProductoApiService con token', () {
    test('envía header Authorization Bearer cuando hay token', () async {
      final mock = MockClient((req) async {
        expect(req.headers['Authorization'], 'Bearer mi-jwt');
        return http.Response('[]', 200);
      });

      final api = ProductoApiService(
        client: mock,
        baseUrl: baseUrl,
        tokenProvider: () => 'mi-jwt',
      );
      final list = await api.getAll();
      expect(list, isEmpty);
    });

    test('lanza UnauthorizedException ante respuesta 401', () async {
      final mock = MockClient((req) async => http.Response('', 401));
      final api = ProductoApiService(client: mock, baseUrl: baseUrl);
      expect(() => api.getAll(), throwsA(isA<UnauthorizedException>()));
    });
  });
}
