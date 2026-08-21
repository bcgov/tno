import {
  collectV2FilterIds,
  collectV2LlmIds,
  parseV2Definition,
  remapV2Definition,
  serializeV2Definition,
} from 'features/admin/automation/v2';

const definition = parseV2Definition(
  JSON.stringify({
    prompts: { extract: 'text' },
    steps: [
      {
        name: 'load',
        phase: 'init',
        isEnabled: true,
        analyses: [],
        actions: [
          { type: 'search', filter: 12, into: '$run.a', isEnabled: true },
          { type: 'search', filter: 18, into: '$run.b', isEnabled: true },
        ],
      },
      {
        name: 'review',
        phase: 'process',
        isEnabled: true,
        llmId: 3,
        source: { from: 'filter', filter: 14, include: [15], exclude: [16] },
        analyses: [{ name: 'extract', prompt: { ref: 'extract' }, returns: { v: 'bool' }, llmId: 4 }],
        actions: [{ type: 'dedupe', against: '$run.a', llmId: 5, isEnabled: true }],
      },
    ],
  }),
);

describe('v2 export/import id handling', () => {
  it('collects every filter and llm id referenced inside the definition', () => {
    expect(collectV2FilterIds(definition).sort()).toEqual([12, 14, 15, 16, 18]);
    expect(collectV2LlmIds(definition).sort()).toEqual([3, 4, 5]);
  });

  it('remaps ids through the import maps and round-trips through serialization', () => {
    const fMap = new Map([
      [12, 112],
      [14, 114],
      [15, 115],
      [16, 116],
      [18, 118],
    ]);
    const lMap = new Map([
      [3, 103],
      [4, 104],
      [5, 105],
    ]);
    const remapped = parseV2Definition(
      serializeV2Definition(
        remapV2Definition(
          definition,
          (id) => (id ? fMap.get(id) : undefined),
          (id) => (id ? lMap.get(id) : undefined),
        ),
      ),
    );
    expect(collectV2FilterIds(remapped).sort()).toEqual([112, 114, 115, 116, 118]);
    expect(collectV2LlmIds(remapped).sort()).toEqual([103, 104, 105]);
    // Unmapped ids survive unchanged rather than dropping.
    const untouched = remapV2Definition(
      definition,
      () => undefined,
      () => undefined,
    );
    expect(collectV2FilterIds(untouched).sort()).toEqual([12, 14, 15, 16, 18]);
  });
});
