-- ========================================
-- Vuelve a crear las claves foráneas que quita QuitarClavesForaneas.sql,
-- con las mismas definiciones que tenía la base al 14/11/2026.
-- Si hay datos huérfanos (por ejemplo, filas que apuntan a una tabla que
-- se borró y no se restauró), la clave correspondiente no se crea y SQL
-- Server informa qué tabla revisar.
-- No cambia datos. El script se puede ejecutar más de una vez.
-- ========================================
IF OBJECT_ID('FK_BackUp_Usuario', 'F') IS NULL
    ALTER TABLE [BackUp] ADD CONSTRAINT [FK_BackUp_Usuario] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id]);
IF OBJECT_ID('FK_Bitacora_EliminadoPor', 'F') IS NULL
    ALTER TABLE [Bitacora] ADD CONSTRAINT [FK_Bitacora_EliminadoPor] FOREIGN KEY ([EliminadoPor]) REFERENCES [Usuario] ([Id]);
IF OBJECT_ID('FK_Dispositivo_Usuario', 'F') IS NULL
    ALTER TABLE [Dispositivo] ADD CONSTRAINT [FK_Dispositivo_Usuario] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id]);
IF OBJECT_ID('FK_Documento_Usuario', 'F') IS NULL
    ALTER TABLE [Documento] ADD CONSTRAINT [FK_Documento_Usuario] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id]);
IF OBJECT_ID('FK_DocumentoCompartido_Documento', 'F') IS NULL
    ALTER TABLE [DocumentoCompartido] ADD CONSTRAINT [FK_DocumentoCompartido_Documento] FOREIGN KEY ([DocumentoId]) REFERENCES [Documento] ([Id]);
IF OBJECT_ID('FK_DocumentoCompartido_Usuario', 'F') IS NULL
    ALTER TABLE [DocumentoCompartido] ADD CONSTRAINT [FK_DocumentoCompartido_Usuario] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id]);
IF OBJECT_ID('FK_FamiliaComponente_Familia', 'F') IS NULL
    ALTER TABLE [FamiliaComponente] ADD CONSTRAINT [FK_FamiliaComponente_Familia] FOREIGN KEY ([FamiliaId]) REFERENCES [Familia] ([Id]);
IF OBJECT_ID('FK_FamiliaComponente_Permiso', 'F') IS NULL
    ALTER TABLE [FamiliaComponente] ADD CONSTRAINT [FK_FamiliaComponente_Permiso] FOREIGN KEY ([PermisoId]) REFERENCES [Permiso] ([Id]);
IF OBJECT_ID('FK_FamiliaComponente_SubFamilia', 'F') IS NULL
    ALTER TABLE [FamiliaComponente] ADD CONSTRAINT [FK_FamiliaComponente_SubFamilia] FOREIGN KEY ([SubFamiliaId]) REFERENCES [Familia] ([Id]);
IF OBJECT_ID('FK_Notificacion_Usuario', 'F') IS NULL
    ALTER TABLE [Notificacion] ADD CONSTRAINT [FK_Notificacion_Usuario] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id]);
IF OBJECT_ID('FK_NotificacionEvento_Notificacion', 'F') IS NULL
    ALTER TABLE [NotificacionEvento] ADD CONSTRAINT [FK_NotificacionEvento_Notificacion] FOREIGN KEY ([NotificacionId]) REFERENCES [Notificacion] ([Id]);
IF OBJECT_ID('FK_NotificacionRecibida_Usuario', 'F') IS NULL
    ALTER TABLE [NotificacionRecibida] ADD CONSTRAINT [FK_NotificacionRecibida_Usuario] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id]);
IF OBJECT_ID('FK_Pago_MedioPago', 'F') IS NULL
    ALTER TABLE [Pago] ADD CONSTRAINT [FK_Pago_MedioPago] FOREIGN KEY ([MedioPagoId]) REFERENCES [MedioPago] ([Id]);
IF OBJECT_ID('FK_Pago_Suscripcion', 'F') IS NULL
    ALTER TABLE [Pago] ADD CONSTRAINT [FK_Pago_Suscripcion] FOREIGN KEY ([SuscripcionId]) REFERENCES [Suscripcion] ([Id]);
IF OBJECT_ID('FK_PerfilFamilia_Familia', 'F') IS NULL
    ALTER TABLE [PerfilFamilia] ADD CONSTRAINT [FK_PerfilFamilia_Familia] FOREIGN KEY ([FamiliaId]) REFERENCES [Familia] ([Id]);
IF OBJECT_ID('FK_PerfilFamilia_Perfil', 'F') IS NULL
    ALTER TABLE [PerfilFamilia] ADD CONSTRAINT [FK_PerfilFamilia_Perfil] FOREIGN KEY ([PerfilId]) REFERENCES [Perfil] ([Id]);
IF OBJECT_ID('FK_Suscripcion_Plan', 'F') IS NULL
    ALTER TABLE [Suscripcion] ADD CONSTRAINT [FK_Suscripcion_Plan] FOREIGN KEY ([PlanId]) REFERENCES [Plan] ([Id]);
IF OBJECT_ID('FK_Suscripcion_Usuario', 'F') IS NULL
    ALTER TABLE [Suscripcion] ADD CONSTRAINT [FK_Suscripcion_Usuario] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id]);
IF OBJECT_ID('FK_Usuario_Empresa', 'F') IS NULL
    ALTER TABLE [Usuario] ADD CONSTRAINT [FK_Usuario_Empresa] FOREIGN KEY ([EmpresaId]) REFERENCES [Usuario] ([Id]);
IF OBJECT_ID('FK_Usuario_Idioma', 'F') IS NULL
    ALTER TABLE [Usuario] ADD CONSTRAINT [FK_Usuario_Idioma] FOREIGN KEY ([IdiomaId]) REFERENCES [Idioma] ([Id]);
IF OBJECT_ID('FK_Usuario_Perfil', 'F') IS NULL
    ALTER TABLE [Usuario] ADD CONSTRAINT [FK_Usuario_Perfil] FOREIGN KEY ([PerfilId]) REFERENCES [Perfil] ([Id]);
IF OBJECT_ID('FK_VerificacionDocumento_Documento', 'F') IS NULL
    ALTER TABLE [VerificacionDocumento] ADD CONSTRAINT [FK_VerificacionDocumento_Documento] FOREIGN KEY ([DocumentoId]) REFERENCES [Documento] ([Id]);
IF OBJECT_ID('FK_VerificacionDocumento_Usuario', 'F') IS NULL
    ALTER TABLE [VerificacionDocumento] ADD CONSTRAINT [FK_VerificacionDocumento_Usuario] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id]);
GO
