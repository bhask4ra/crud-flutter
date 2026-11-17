import 'dart:io' show Platform;

import 'package:flutter/foundation.dart' show kIsWeb;

/// Resuelve la URL base de la API según la plataforma de ejecución.
class ApiConfig {
  static String defaultBaseUrl() {
    const envUrl = String.fromEnvironment('API_BASE_URL');
    if (envUrl.isNotEmpty) return envUrl;
    if (kIsWeb) return 'http://localhost:5000';
    if (Platform.isAndroid) return 'http://10.0.2.2:5000';
    return 'http://localhost:5000';
  }
}
