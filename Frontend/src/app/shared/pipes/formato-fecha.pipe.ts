import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'formatoFecha',
  standalone: true
})
export class FormatoFechaPipe implements PipeTransform {
  transform(value: string | Date | null | undefined, incluirHora: boolean = false): string {
    if (!value) {
      return '-';
    }

    const fecha = new Date(value);

    if (isNaN(fecha.getTime())) {
      return '-';
    }

    const dia = fecha.getDate().toString().padStart(2, '0');
    const mes = (fecha.getMonth() + 1).toString().padStart(2, '0');
    const anio = fecha.getFullYear();

    if (incluirHora) {
      const horas = fecha.getHours().toString().padStart(2, '0');
      const minutos = fecha.getMinutes().toString().padStart(2, '0');
      return `${dia}/${mes}/${anio} ${horas}:${minutos}`;
    }

    return `${dia}/${mes}/${anio}`;
  }
}
