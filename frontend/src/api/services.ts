import { isMockMode } from './config';
import { getJson } from './httpClient';
import { mockClassifications } from '../mocks/classifications';
import type { Classification } from '../types';

export async function listClassifications(): Promise<Classification[]> {
  if (isMockMode) return Promise.resolve(mockClassifications);
  return getJson<Classification[]>('/tax-classifications');
}
