import { describe, expect, it } from 'vitest';

import { normalizeChartData } from '@/subscriber/features/my-reports/edit/settings/template/utils/normalizeChartData';

describe('normalizeChartData', () => {
  it('replaces the sentiment colour function with a colour for each value', () => {
    const result = normalizeChartData(
      {
        labels: ['a', 'b', 'c', 'd'],
        datasets: [
          {
            label: 'Average Sentiment',
            data: [1, 0, -1, null],
            backgroundColor: '(ctx,options) => {{ }}' as any,
            borderColor: '(ctx,options) => {{ }}' as any,
          },
        ],
      },
      { datasetColors: ['#0f0', '#ff0', '#f00'] } as any,
    );
    expect(result.datasets[0].backgroundColor).toEqual(['#0f0', '#ff0', '#f00', '#ff0']);
    expect(result.datasets[0].borderColor).toEqual(['#0f0', '#ff0', '#f00', '#ff0']);
  });

  it('keeps colour arrays, and removes a null so the chart picks its default', () => {
    const result = normalizeChartData({
      labels: ['a'],
      datasets: [{ label: 'x', data: [1], backgroundColor: ['#123'], borderColor: null as any }],
    });
    expect(result.datasets[0].backgroundColor).toEqual(['#123']);
    expect(result.datasets[0].borderColor).toBeUndefined();
  });
});
