import 'package:flutter/material.dart';

import '../services/auth_service.dart';

/// Pantalla de inicio de sesión y registro de usuarios.
class AuthScreen extends StatefulWidget {
  final AuthService auth;
  const AuthScreen({super.key, required this.auth});

  @override
  State<AuthScreen> createState() => _AuthScreenState();
}

class _AuthScreenState extends State<AuthScreen> {
  final _formKey = GlobalKey<FormState>();
  final _nombreCtrl = TextEditingController();
  final _emailCtrl = TextEditingController();
  final _passwordCtrl = TextEditingController();
  final _confirmCtrl = TextEditingController();

  bool _isRegister = false;
  bool _obscurePassword = true;
  bool _loading = false;
  String? _errorMsg;

  @override
  void dispose() {
    _nombreCtrl.dispose();
    _emailCtrl.dispose();
    _passwordCtrl.dispose();
    _confirmCtrl.dispose();
    super.dispose();
  }

  void _toggleMode() {
    setState(() {
      _isRegister = !_isRegister;
      _errorMsg = null;
      _formKey.currentState?.reset();
    });
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() {
      _loading = true;
      _errorMsg = null;
    });

    try {
      if (_isRegister) {
        await widget.auth.register(
          nombre: _nombreCtrl.text,
          email: _emailCtrl.text,
          password: _passwordCtrl.text,
        );
      } else {
        await widget.auth.login(
          email: _emailCtrl.text,
          password: _passwordCtrl.text,
        );
      }
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _errorMsg = '$e';
        _loading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Scaffold(
      body: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 420),
            child: Card(
              elevation: 2,
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Form(
                  key: _formKey,
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      Icon(
                        Icons.inventory_2_rounded,
                        size: 48,
                        color: theme.colorScheme.primary,
                      ),
                      const SizedBox(height: 12),
                      Text(
                        _isRegister ? 'Crear cuenta' : 'Iniciar sesión',
                        textAlign: TextAlign.center,
                        style: theme.textTheme.headlineSmall?.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      const SizedBox(height: 4),
                      Text(
                        'Catálogo de Productos',
                        textAlign: TextAlign.center,
                        style: theme.textTheme.bodyMedium?.copyWith(
                          color: theme.colorScheme.onSurfaceVariant,
                        ),
                      ),
                      const SizedBox(height: 20),
                      if (_errorMsg != null) ...[
                        Container(
                          padding: const EdgeInsets.all(12),
                          decoration: BoxDecoration(
                            color: theme.colorScheme.errorContainer,
                            borderRadius: BorderRadius.circular(8),
                          ),
                          child: Text(
                            _errorMsg!,
                            style: TextStyle(
                              color: theme.colorScheme.onErrorContainer,
                            ),
                          ),
                        ),
                        const SizedBox(height: 16),
                      ],
                      if (_isRegister) ...[
                        TextFormField(
                          controller: _nombreCtrl,
                          textInputAction: TextInputAction.next,
                          decoration: const InputDecoration(
                            labelText: 'Nombre completo',
                            prefixIcon: Icon(Icons.person_outline),
                            border: OutlineInputBorder(),
                          ),
                          validator: (v) {
                            if (v == null || v.trim().length < 2) {
                              return 'Ingresa al menos 2 caracteres';
                            }
                            return null;
                          },
                        ),
                        const SizedBox(height: 12),
                      ],
                      TextFormField(
                        controller: _emailCtrl,
                        keyboardType: TextInputType.emailAddress,
                        textInputAction: TextInputAction.next,
                        decoration: const InputDecoration(
                          labelText: 'Correo electrónico',
                          prefixIcon: Icon(Icons.email_outlined),
                          border: OutlineInputBorder(),
                        ),
                        validator: (v) {
                          final val = (v ?? '').trim();
                          if (val.isEmpty || !val.contains('@')) {
                            return 'Ingresa un correo válido';
                          }
                          return null;
                        },
                      ),
                      const SizedBox(height: 12),
                      TextFormField(
                        controller: _passwordCtrl,
                        obscureText: _obscurePassword,
                        textInputAction: _isRegister
                            ? TextInputAction.next
                            : TextInputAction.done,
                        onFieldSubmitted: (_) {
                          if (!_isRegister && !_loading) _submit();
                        },
                        decoration: InputDecoration(
                          labelText: 'Contraseña',
                          prefixIcon: const Icon(Icons.lock_outline),
                          border: const OutlineInputBorder(),
                          suffixIcon: IconButton(
                            icon: Icon(_obscurePassword
                                ? Icons.visibility_outlined
                                : Icons.visibility_off_outlined),
                            onPressed: () => setState(
                                () => _obscurePassword = !_obscurePassword),
                          ),
                        ),
                        validator: (v) {
                          if (v == null || v.length < 6) {
                            return 'Mínimo 6 caracteres';
                          }
                          return null;
                        },
                      ),
                      if (_isRegister) ...[
                        const SizedBox(height: 12),
                        TextFormField(
                          controller: _confirmCtrl,
                          obscureText: _obscurePassword,
                          textInputAction: TextInputAction.done,
                          onFieldSubmitted: (_) {
                            if (!_loading) _submit();
                          },
                          decoration: const InputDecoration(
                            labelText: 'Confirmar contraseña',
                            prefixIcon: Icon(Icons.lock_reset_outlined),
                            border: OutlineInputBorder(),
                          ),
                          validator: (v) {
                            if (v != _passwordCtrl.text) {
                              return 'Las contraseñas no coinciden';
                            }
                            return null;
                          },
                        ),
                      ],
                      const SizedBox(height: 20),
                      FilledButton(
                        onPressed: _loading ? null : _submit,
                        child: Padding(
                          padding: const EdgeInsets.symmetric(vertical: 12),
                          child: _loading
                              ? const SizedBox(
                                  height: 20,
                                  width: 20,
                                  child: CircularProgressIndicator(
                                    strokeWidth: 2,
                                    color: Colors.white,
                                  ),
                                )
                              : Text(_isRegister
                                  ? 'Registrarme'
                                  : 'Iniciar sesión'),
                        ),
                      ),
                      const SizedBox(height: 12),
                      TextButton(
                        onPressed: _loading ? null : _toggleMode,
                        child: Text(
                          _isRegister
                              ? '¿Ya tienes cuenta? Inicia sesión'
                              : '¿No tienes cuenta? Regístrate',
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
