<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ConfirmacionAccion.ascx.cs" Inherits="ConfirmacionAccion" %>

<%-- Ventana de confirmación de la página (una sola, la comparten todos sus
     botones). El botón le pasa su mensaje con Para(clave); si se confirma,
     el botón se vuelve a "clickear" y su postback sigue normalmente. --%>
<div class="modal fade" id="modalConfirmacion" tabindex="-1" aria-labelledby="tituloConfirmacion" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="tituloConfirmacion"><span class="text-warning me-2" aria-hidden="true">&#9888;</span><%: Traductor.T("confirmacion.titulo") %></h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="<%: Traductor.T("confirmacion.cancelar") %>"></button>
            </div>
            <div class="modal-body">
                <p id="mensajeConfirmacion" class="mb-0"></p>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal"><%: Traductor.T("confirmacion.cancelar") %></button>
                <button type="button" class="btn btn-danger" id="aceptarConfirmacion"><%: Traductor.T("confirmacion.confirmar") %></button>
            </div>
        </div>
    </div>
</div>
<script>
    var ConfirmacionAccion = (function () {
        var Pendiente = null;
        var Modal = document.getElementById('modalConfirmacion');
        // Se llama desde el OnClientClick del botón. La primera vez muestra la
        // ventana y frena el postback; la segunda (ya confirmado) lo deja seguir.
        function pedir(boton, mensaje) {
            if (boton.getAttribute('data-confirmado') === '1') {
                boton.removeAttribute('data-confirmado');
                return true;
            }
            Pendiente = boton;
            document.getElementById('mensajeConfirmacion').textContent = mensaje;
            bootstrap.Modal.getOrCreateInstance(Modal).show();
            return false;
        }
        document.getElementById('aceptarConfirmacion').addEventListener('click', function () {
            bootstrap.Modal.getOrCreateInstance(Modal).hide();
            if (!Pendiente) return;
            var Boton = Pendiente;
            Pendiente = null;
            Boton.setAttribute('data-confirmado', '1');
            Boton.click();
        });
        // Cancelar o cerrar: no se ejecuta nada.
        Modal.addEventListener('hidden.bs.modal', function () { Pendiente = null; });
        return { pedir: pedir };
    })();
</script>
