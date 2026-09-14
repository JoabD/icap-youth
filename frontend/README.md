# ICAP Juvenil — Frontend (Angular 21+)

Estructura basada 100% en **Standalone Components** (sin NgModules), Signals
para estado local y `inject()` para dependencias, según el Documento de
Diseño del Sistema.

```
frontend/
└── src/
    └── app/
        ├── core/
        │   ├── models/          (interfaces TS espejo de los DTOs del backend)
        │   ├── services/        (AuthService, ReceiptService)
        │   ├── guards/          (authGuard — protege "/admin")
        │   └── interceptors/    (authInterceptor, errorInterceptor)
        ├── features/
        │   ├── landing/         (Landing Page pública en "/")
        │   ├── login/           (LoginComponent — pantalla de acceso)
        │   └── receipt/         (ReceiptViewComponent, ReceiptFormComponent)
        ├── app.component.ts
        ├── app.config.ts
        └── app.routes.ts
```

## Componentes entregados en detalle

### `LoginComponent` (`features/login`)
- Formulario reactivo (`ReactiveFormsModule`) con validación de correo/contraseña.
- Angular Material: `mat-card`, `mat-form-field`, `mat-button`, `mat-icon`.
- Estado local con `signal()`: `isLoading`, `errorMessage`, `hidePassword`.
- Al autenticar, `AuthService.login()` (RxJS únicamente aquí) guarda el JWT y
  redirige a `/admin`.

### `ReceiptViewComponent` (`features/receipt`)
- Carga el recibo por `id` de ruta vía `ReceiptService.getById()`.
- Renderiza **dos tarjetas idénticas** (`Original` y `Copia`) iterando un
  arreglo `copies = ['Original', 'Copia']` con `@for`, para que ambas
  compartan exactamente la misma plantilla y solo cambie la etiqueta (chip).
- Cada tarjeta incluye un QR (`angularx-qrcode`) generado a partir de
  `receipt.qrHash` (el valor que produce el backend), y un bloque de firmas
  (`.signature-slot`) listo para firmarse a mano tras imprimir.
- `receipt-view.component.scss` implementa la regla clave de impresión:
  - `.no-print { display: none !important; }` oculta toolbar/nav al imprimir.
  - `break-inside: avoid` evita que una tarjeta se corte entre hojas.
  - El botón "Imprimir recibo" solo llama a `window.print()`; todo el layout
    de impresión vive en CSS (`@media print`), no en JS.

## Servicios / infraestructura del cliente

- **`AuthService`**: sesión con Signals (`currentUser`, `isAuthenticated`,
  `isAdmin` como `computed()`), persistida en `localStorage` para sobrevivir
  refresh de página.
- **`ReceiptService`**: `getById`, `getAll`, `create`, `cancel` — mapean 1:1
  a los endpoints que expondrá `Icap.WebApi` (`GET/POST /api/v1/receipts`).
- **`authInterceptor`**: adjunta `Authorization: Bearer <token>` a cada
  request saliente.
- **`errorInterceptor`**: ante un 401 (token vencido/inválido) cierra sesión
  y redirige a `/login`.
- **`authGuard`**: función (`CanActivateFn`) que protege todo `/admin/**`.

### `ReceiptListComponent` (`features/receipt`)
- Tabla de registro (`MatTable`) en `/admin/receipts`, ruta por defecto tras
  el login. Muestra folio, delegado, área, cantidad, total, estatus y fecha,
  con acción para ver/imprimir cada recibo.
- El título cambia entre "(todos)" / "(míos)" según `AuthService.isAdmin` —
  el filtrado real ya lo aplica `GET /api/v1/receipts` en el backend según
  el rol del JWT, este componente solo refleja esa respuesta.

## Pendiente para una siguiente entrega

- Tests unitarios (Karma/Jasmine o migración a Vitest) de `LoginComponent`,
  `ReceiptViewComponent` y `ReceiptListComponent`.
- Paginación / filtros en `ReceiptListComponent` si el volumen crece.
- Acción de cancelar recibo desde la UI (el backend ya expone
  `POST /api/v1/receipts/{id}/cancel`, restringido a rol Admin).

## Cómo correr el proyecto

```bash
cd frontend
npm install
npm start        # http://localhost:4200
```

> Ajusta `src/environments/environment.ts` con la URL real de `Icap.WebApi`.
