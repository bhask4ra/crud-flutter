/// Modelo de dominio del médico en el cliente.
/// Mapea el JSON camelCase que devuelve la API ASP.NET Core.
class Medico {
  final int? idMedico;
  final String nombre;
  final String apPaterno;
  final String? apMaterno;
  final String? telefono;
  final String? direccion;
  final String especialidad;
  final bool estatus;

  const Medico({
    this.idMedico,
    required this.nombre,
    required this.apPaterno,
    this.apMaterno,
    this.telefono,
    this.direccion,
    required this.especialidad,
    this.estatus = true,
  });

  factory Medico.fromJson(Map<String, dynamic> json) => Medico(
        idMedico: json['idMedico'] as int?,
        nombre: json['nombre'] as String? ?? '',
        apPaterno: json['apPaterno'] as String? ?? '',
        apMaterno: json['apMaterno'] as String?,
        telefono: json['telefono'] as String?,
        direccion: json['direccion'] as String?,
        especialidad: json['especialidad'] as String? ?? '',
        estatus: json['estatus'] as bool? ?? true,
      );

  /// JSON para crear/actualizar (sin el id, que va en la ruta).
  Map<String, dynamic> toJson() => {
        'nombre': nombre,
        'apPaterno': apPaterno,
        'apMaterno': apMaterno,
        'telefono': telefono,
        'direccion': direccion,
        'especialidad': especialidad,
        'estatus': estatus,
      };

  String get nombreCompleto =>
      [nombre, apPaterno, apMaterno].where((s) => (s ?? '').isNotEmpty).join(' ');

  Medico copyWith({
    int? idMedico,
    String? nombre,
    String? apPaterno,
    String? apMaterno,
    String? telefono,
    String? direccion,
    String? especialidad,
    bool? estatus,
  }) =>
      Medico(
        idMedico: idMedico ?? this.idMedico,
        nombre: nombre ?? this.nombre,
        apPaterno: apPaterno ?? this.apPaterno,
        apMaterno: apMaterno ?? this.apMaterno,
        telefono: telefono ?? this.telefono,
        direccion: direccion ?? this.direccion,
        especialidad: especialidad ?? this.especialidad,
        estatus: estatus ?? this.estatus,
      );
}
