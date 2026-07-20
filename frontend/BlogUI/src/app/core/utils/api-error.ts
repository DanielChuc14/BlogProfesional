import { HttpErrorResponse } from '@angular/common/http';

interface ValidationError {
  field: string;
  message: string;
}

// El backend devuelve los fallos de validacion como
// { title: "Validation failed", errors: [{ field, message }] }.
// Leer solo `title` mostraba siempre el generico "Validation failed",
// ocultando que campo fallo.
export function extractApiError(err: HttpErrorResponse, fallback: string): string {
  const body = err?.error;
  if (!body) return fallback;

  if (typeof body === 'string') return body;

  const errors: ValidationError[] | undefined = body.errors;
  if (Array.isArray(errors) && errors.length > 0) {
    // Los mensajes de FluentValidation ya nombran el campo ("Title is required."),
    // asi que anteponer el nombre otra vez seria redundante.
    return errors.map(e => e.message).filter(Boolean).join('\n');
  }

  return body.error ?? body.title ?? fallback;
}
