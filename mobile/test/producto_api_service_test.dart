import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

import 'package:medicos_app/models/producto.dart';
import 'package:medicos_app/services/producto_api_service.dart';

void main() {
  const baseUrl = 'http://localhost:5000';

  group('ProductoApiService', () {
    test('getAll parsea la lista de productos y envía filtros', () async {
      final mock = MockClient((req) async {
        expect(req.url.path, '/api/productos');
        expect(req.url.queryParameters['search'], 'lap');
        expect(req.url.queryParameters['tipo'], 'Articulo');
        expect(req.method, 'GET');
        return http.Response(
          jsonEncode([
            {
              'idProducto': 1,
              'nombre': 'Laptop Dell',
              'precioUnitario': 14500.0,
              'estatus': true,
              'existencias': 8,
              'tipo': 'Articulo',
            }
          ]),
          200,
          headers: {'content-type': 'application/json'},
        );
      });

      final api = ProductoApiService(client: mock, baseUrl: baseUrl);
      final productos =
          await api.getAll(search: 'lap', tipo: TipoProducto.articulo);

      expect(productos, hasLength(1));
      expect(productos.first.nombre, 'Laptop Dell');
      expect(productos.first.tipo, TipoProducto.articulo);
      expect(productos.first.existencias, 8);
    });

    test('getResumen parsea el resumen de inventario', () async {
      final mock = MockClient((req) async {
        expect(req.url.path, '/api/productos/resumen');
        return http.Response(
          jsonEncode({
            'totalProductos': 3,
            'activos': 3,
            'inactivos': 0,
            'totalArticulos': 2,
            'totalServicios': 1,
            'stockBajo': 1,
            'valorTotalInventario': 117350.0,
          }),
          200,
          headers: {'content-type': 'application/json'},
        );
      });

      final api = ProductoApiService(client: mock, baseUrl: baseUrl);
      final resumen = await api.getResumen();

      expect(resumen.totalProductos, 3);
      expect(resumen.stockBajo, 1);
      expect(resumen.valorTotalInventario, 117350.0);
    });

    test('create envía POST y devuelve el producto creado', () async {
      final mock = MockClient((req) async {
        expect(req.method, 'POST');
        final body = jsonDecode(req.body) as Map<String, dynamic>;
        expect(body['nombre'], 'Soporte');
        expect(body['tipo'], 'Servicio');
        expect(body['existencias'], 0);
        return http.Response(
          jsonEncode({
            'idProducto': 10,
            'nombre': 'Soporte',
            'precioUnitario': 500.0,
            'estatus': true,
            'existencias': 0,
            'tipo': 'Servicio',
          }),
          201,
          headers: {'content-type': 'application/json'},
        );
      });

      final api = ProductoApiService(client: mock, baseUrl: baseUrl);
      final creado = await api.create(const Producto(
        nombre: 'Soporte',
        precioUnitario: 500.0,
        existencias: 5,
        tipo: TipoProducto.servicio,
      ));

      expect(creado.idProducto, 10);
      expect(creado.tipo, TipoProducto.servicio);
    });

    test('ajustarExistencias envía PATCH y devuelve producto actualizado',
        () async {
      final mock = MockClient((req) async {
        expect(req.method, 'PATCH');
        expect(req.url.path, '/api/productos/2/existencias');
        final body = jsonDecode(req.body) as Map<String, dynamic>;
        expect(body['delta'], 3);
        return http.Response(
          jsonEncode({
            'idProducto': 2,
            'nombre': 'Mouse',
            'precioUnitario': 450.0,
            'estatus': true,
            'existencias': 6,
            'tipo': 'Articulo',
          }),
          200,
          headers: {'content-type': 'application/json'},
        );
      });

      final api = ProductoApiService(client: mock, baseUrl: baseUrl);
      final actualizado = await api.ajustarExistencias(2, 3);
      expect(actualizado.existencias, 6);
    });

    test('update hace PUT y acepta 204', () async {
      final mock = MockClient((req) async {
        expect(req.method, 'PUT');
        expect(req.url.path, '/api/productos/5');
        return http.Response('', 204);
      });

      final api = ProductoApiService(client: mock, baseUrl: baseUrl);
      await api.update(
        5,
        const Producto(
          nombre: 'X',
          precioUnitario: 100.0,
          existencias: 4,
          tipo: TipoProducto.articulo,
        ),
      );
    });

    test('delete hace DELETE y acepta 204', () async {
      final mock = MockClient((req) async {
        expect(req.method, 'DELETE');
        expect(req.url.path, '/api/productos/3');
        return http.Response('', 204);
      });

      final api = ProductoApiService(client: mock, baseUrl: baseUrl);
      await api.delete(3);
    });

    test('getAll lanza ApiException ante error del servidor', () async {
      final mock = MockClient((req) async => http.Response('boom', 500));
      final api = ProductoApiService(client: mock, baseUrl: baseUrl);
      expect(() => api.getAll(), throwsA(isA<ApiException>()));
    });
  });
}
