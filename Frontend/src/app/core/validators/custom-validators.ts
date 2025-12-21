import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export class CustomValidators {
  /**
   * Validador de DNI peruano: 8 dígitos, no puede empezar con 0
   */
  static dni(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;

      if (!value) {
        return null; // No validar si está vacío (usar Validators.required por separado)
      }

      // Verificar que sea solo dígitos
      if (!/^\d+$/.test(value)) {
        return { dni: { value, message: 'El DNI solo puede contener dígitos' } };
      }

      // Verificar longitud exacta de 8
      if (value.length !== 8) {
        return { dni: { value, message: 'El DNI debe tener exactamente 8 dígitos' } };
      }

      // Verificar que no empiece con 0
      if (value[0] === '0') {
        return { dni: { value, message: 'El DNI no puede empezar con 0' } };
      }

      return null;
    };
  }

  /**
   * Validador de RUC peruano: 11 dígitos, debe empezar con 10, 15, 16, 17 o 20
   */
  static ruc(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;

      if (!value) {
        return null;
      }

      // Verificar que sea solo dígitos
      if (!/^\d+$/.test(value)) {
        return { ruc: { value, message: 'El RUC solo puede contener dígitos' } };
      }

      // Verificar longitud exacta de 11
      if (value.length !== 11) {
        return { ruc: { value, message: 'El RUC debe tener exactamente 11 dígitos' } };
      }

      // Verificar que empiece con prefijo válido
      const prefijo = value.substring(0, 2);
      const prefijosValidos = ['10', '15', '16', '17', '20'];

      if (!prefijosValidos.includes(prefijo)) {
        return {
          ruc: {
            value,
            message: 'El RUC debe empezar con 10, 15, 16, 17 o 20'
          }
        };
      }

      return null;
    };
  }

  /**
   * Validador de teléfono: solo números, +, -, (), espacios
   */
  static telefono(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;

      if (!value) {
        return null;
      }

      // Permitir solo caracteres válidos para teléfono
      if (!/^[0-9+\-\s()]*$/.test(value)) {
        return {
          telefono: {
            value,
            message: 'El teléfono solo puede contener números, +, -, paréntesis y espacios'
          }
        };
      }

      return null;
    };
  }

  /**
   * Validador de decimales: máximo N decimales permitidos
   */
  static decimal(maxDecimales: number): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;

      if (value === null || value === undefined || value === '') {
        return null;
      }

      const valueStr = value.toString();

      // Verificar si tiene decimales
      if (valueStr.includes('.')) {
        const partes = valueStr.split('.');
        const decimales = partes[1];

        if (decimales.length > maxDecimales) {
          return {
            decimal: {
              value,
              maxDecimales,
              message: `Solo se permiten ${maxDecimales} decimales`
            }
          };
        }
      }

      return null;
    };
  }

  /**
   * Validador de rango de fechas: fechaInicio debe ser <= fechaFin
   * Usar en el FormGroup, no en un control individual
   */
  static rangoFechas(fechaInicioKey: string, fechaFinKey: string): ValidatorFn {
    return (formGroup: AbstractControl): ValidationErrors | null => {
      const fechaInicio = formGroup.get(fechaInicioKey)?.value;
      const fechaFin = formGroup.get(fechaFinKey)?.value;

      if (!fechaInicio || !fechaFin) {
        return null;
      }

      const inicio = new Date(fechaInicio);
      const fin = new Date(fechaFin);

      if (inicio > fin) {
        return {
          rangoFechas: {
            message: 'La fecha de inicio debe ser menor o igual a la fecha fin'
          }
        };
      }

      return null;
    };
  }

  /**
   * Validador de monto: debe ser mayor a cero y con máximo 2 decimales
   */
  static monto(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;

      if (value === null || value === undefined || value === '') {
        return null;
      }

      const numero = parseFloat(value);

      // Verificar que sea mayor a 0
      if (numero <= 0) {
        return { monto: { value, message: 'El monto debe ser mayor a 0' } };
      }

      // Verificar máximo 2 decimales
      const valueStr = value.toString();
      if (valueStr.includes('.')) {
        const decimales = valueStr.split('.')[1];
        if (decimales.length > 2) {
          return { monto: { value, message: 'El monto solo puede tener 2 decimales' } };
        }
      }

      return null;
    };
  }

  /**
   * Validador de peso: debe ser mayor a 0.1 kg y con máximo 1 decimal
   */
  static peso(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;

      if (value === null || value === undefined || value === '') {
        return null;
      }

      const numero = parseFloat(value);

      // Verificar que sea mayor a 0.1
      if (numero < 0.1) {
        return { peso: { value, message: 'El peso debe ser mayor a 0.1 kg' } };
      }

      // Verificar máximo 1 decimal
      const valueStr = value.toString();
      if (valueStr.includes('.')) {
        const decimales = valueStr.split('.')[1];
        if (decimales.length > 1) {
          return { peso: { value, message: 'El peso solo puede tener 1 decimal' } };
        }
      }

      return null;
    };
  }

  /**
   * Helper: Obtener mensaje de error de un control
   */
  static getErrorMessage(control: AbstractControl | null): string {
    if (!control || !control.errors) {
      return '';
    }

    const errors = control.errors;

    if (errors['required']) return 'Este campo es requerido';
    if (errors['email']) return 'Email inválido';
    if (errors['min']) return `Valor mínimo: ${errors['min'].min}`;
    if (errors['max']) return `Valor máximo: ${errors['max'].max}`;
    if (errors['minlength']) return `Longitud mínima: ${errors['minlength'].requiredLength}`;
    if (errors['maxlength']) return `Longitud máxima: ${errors['maxlength'].requiredLength}`;
    if (errors['pattern']) return 'Formato inválido';

    // Validadores personalizados
    if (errors['dni']) return errors['dni'].message;
    if (errors['ruc']) return errors['ruc'].message;
    if (errors['telefono']) return errors['telefono'].message;
    if (errors['decimal']) return errors['decimal'].message;
    if (errors['monto']) return errors['monto'].message;
    if (errors['peso']) return errors['peso'].message;
    if (errors['rangoFechas']) return errors['rangoFechas'].message;

    return 'Campo inválido';
  }
}
