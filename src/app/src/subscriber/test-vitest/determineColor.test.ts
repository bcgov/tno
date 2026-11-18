import { describe, expect, it } from 'vitest';

import { IChartSectionSettingsModel } from '@/shared';
import { determineColor } from '@/subscriber/features/my-reports/edit/settings/template/utils/determineColor';

const settings = (values: Partial<IChartSectionSettingsModel> = {}) =>
  ({ dataset: '', datasetValue: 'count', ...values }) as IChartSectionSettingsModel;

describe('determineColor', () => {
  it('leaves the colour to Chart.js when no colours are set', () => {
    expect(determineColor(undefined, 0, 'Story Count', settings())).toBeUndefined();
  });

  it('treats an empty list as no colours, so the chart is not drawn in black', () => {
    // Choosing a dataset type leaves both colour lists empty.
    expect(determineColor([], 0, 'Story Count', settings())).toBeUndefined();
    expect(determineColor([], 3, 'Story Count', settings())).toBeUndefined();
    expect(
      determineColor([], 0, 'Story Count', settings({ applyColorToValue: true })),
    ).toBeUndefined();
  });

  it('picks the colour of the dataset, or every colour when applied to each value', () => {
    const colors = ['#111', '#222'];
    expect(determineColor(colors, 1, 'B', settings({ dataset: 'source' }))).toBe('#222');
    expect(determineColor(colors, 2, 'C', settings({ dataset: 'source' }))).toEqual(['#111']);
    expect(determineColor(colors, 0, 'A', settings({ applyColorToValue: true }))).toBe(colors);
  });

  it('keeps the sentiment colours when the list is empty', () => {
    expect(determineColor([], 0, 'Positive', settings({ dataset: 'sentimentSimple' }))).toEqual([
      'green',
    ]);
    expect(determineColor([], 0, 'Negative', settings({ dataset: 'sentiment' }))).toEqual(['red']);
  });
});
