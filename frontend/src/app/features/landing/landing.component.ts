import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';

/**
 * Landing Page pública en la ruta raíz "/". Placeholder mínimo: el alcance
 * de esta entrega es Login + Vista de Recibo; este componente solo cumple
 * el requisito de arquitectura ("la Landing Page será un componente en la
 * ruta raíz") y enlaza al acceso del sistema.
 */
@Component({
  selector: 'icap-landing',
  standalone: true,
  imports: [RouterLink, MatButtonModule],
  template: `
    <section class="landing">
      <h1>ICAP Juvenil</h1>
      <p>Plataforma de emisión y control de recibos para el evento.</p>
      <a mat-flat-button color="primary" routerLink="/login">Acceder al sistema</a>
    </section>
  `,
  styles: `
    .landing {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 16px;
      text-align: center;
      padding: 24px;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingComponent {}
