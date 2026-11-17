/// Información pública del usuario autenticado.
class Usuario {
  final int idUsuario;
  final String nombre;
  final String email;

  const Usuario({
    required this.idUsuario,
    required this.nombre,
    required this.email,
  });

  factory Usuario.fromJson(Map<String, dynamic> json) => Usuario(
        idUsuario: (json['idUsuario'] as num?)?.toInt() ?? 0,
        nombre: json['nombre'] as String? ?? '',
        email: json['email'] as String? ?? '',
      );
}

/// Sesión autenticada devuelta por /api/auth/login y /api/auth/register.
class AuthSession {
  final String token;
  final DateTime expiresAt;
  final Usuario usuario;

  const AuthSession({
    required this.token,
    required this.expiresAt,
    required this.usuario,
  });

  factory AuthSession.fromJson(Map<String, dynamic> json) => AuthSession(
        token: json['token'] as String? ?? '',
        expiresAt: DateTime.tryParse(json['expiresAt']?.toString() ?? '') ??
            DateTime.now().add(const Duration(hours: 8)),
        usuario: Usuario.fromJson(
            (json['usuario'] as Map<String, dynamic>?) ?? const {}),
      );
}
