import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'formatoMoneda',
  standalone: true
})
export class FormatoMonedaPipe implements PipeTransform {
  transform(value: number | null | undefined): string {
    if (value === null || value === undefined) {
      return 'S/ 0.00';
    }

    return 'S/ ' + value.toLocaleString('es-PE', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    });
  }
}
