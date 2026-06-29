import { ApiError } from '../client/ApiError';
import type { ClassificationDto, CreateClassificationRequestDto, UpdateClassificationRequestDto } from '../contracts/classifications';
import type { Classification, ClassificationStatus } from '../../types/classification';

const statuses: ClassificationStatus[] = ['Vigente', 'Pendiente', 'Observada', 'Rechazada'];
function assertString(value: unknown, field: string): string { if (typeof value !== 'string' || !value.trim()) throw invalid(field); return value; }
function assertNumber(value: unknown, field: string): number { if (typeof value !== 'number' || !Number.isFinite(value)) throw invalid(field); return value; }
function invalid(field: string) { return new ApiError({ code: 'INVALID_RESPONSE', message: `Respuesta inválida de API en campo ${field}.` }); }
export function isoDateToDisplay(value: string): string { if (!/^\d{4}-\d{2}-\d{2}$/.test(value)) throw invalid('paymentDate'); const [year, month, day] = value.split('-'); return `${day}-${month}-${year}`; }
export function displayDateToIso(value: string): string { if (!/^\d{2}-\d{2}-\d{4}$/.test(value)) throw invalid('fechaPago'); const [day, month, year] = value.split('-'); return `${year}-${month}-${day}`; }
export function classificationDtoToDomain(dto: ClassificationDto): Classification {
  const status = assertString(dto.status, 'status'); if (!statuses.includes(status as ClassificationStatus)) throw invalid('status');
  return { id: assertString(dto.id, 'id'), mercado: assertString(dto.market, 'market'), origen: assertString(dto.source, 'source'), ejercicio: assertNumber(dto.fiscalYear, 'fiscalYear'), instrumento: assertString(dto.instrument, 'instrument'), fechaPago: isoDateToDisplay(dto.paymentDate), descripcion: assertString(dto.description, 'description'), secuenciaEvento: assertString(dto.eventSequence, 'eventSequence'), factorActualizacion: assertNumber(dto.updateFactor, 'updateFactor'), monto: assertNumber(dto.amount, 'amount'), estado: status as ClassificationStatus };
}
export function classificationDomainToCreateRequest(domain: Classification): CreateClassificationRequestDto { return { market: domain.mercado, source: domain.origen, fiscalYear: domain.ejercicio, instrument: domain.instrumento, paymentDate: displayDateToIso(domain.fechaPago), description: domain.descripcion, eventSequence: domain.secuenciaEvento, updateFactor: domain.factorActualizacion, amount: domain.monto, status: domain.estado }; }
export function classificationDomainToUpdateRequest(domain: Classification): UpdateClassificationRequestDto { return classificationDomainToCreateRequest(domain); }
export function classificationDomainToDto(domain: Classification): ClassificationDto { return { id: domain.id, ...classificationDomainToCreateRequest(domain) }; }
