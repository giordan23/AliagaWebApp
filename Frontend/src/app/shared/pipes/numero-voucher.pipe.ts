import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'numeroVoucher',
  standalone: true
})
export class NumeroVoucherPipe implements PipeTransform {
  transform(value: string | number | null | undefined): string {
    if (!value) {
      return '00000000';
    }

    const numStr = value.toString();
    return numStr.padStart(8, '0');
  }
}
