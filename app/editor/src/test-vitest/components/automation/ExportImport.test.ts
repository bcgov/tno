import {
  collectFilterIds,
  collectLlmIds,
  parseDefinition,
  remapDefinition,
  serializeDefinition,
} from 'features/admin/automation/designer';

const definition = parseDefinition(
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
        analyses: [
          { name: 'extract', prompt: { ref: 'extract' }, returns: { v: 'bool' }, llmId: 4 },
        ],
        actions: [{ type: 'dedupe', against: '$run.a', llmId: 5, isEnabled: true }],
      },
    ],
  }),
);

describe('automation export/import id handling', () => {
  it('collects every filter and llm id referenced inside the definition', () => {
    expect(collectFilterIds(definition).sort()).toEqual([12, 14, 15, 16, 18]);
    expect(collectLlmIds(definition).sort()).toEqual([3, 4, 5]);
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
    const remapped = parseDefinition(
      serializeDefinition(
        remapDefinition(
          definition,
          (id) => (id ? fMap.get(id) : undefined),
          (id) => (id ? lMap.get(id) : undefined),
        ),
      ),
    );
    expect(collectFilterIds(remapped).sort()).toEqual([112, 114, 115, 116, 118]);
    expect(collectLlmIds(remapped).sort()).toEqual([103, 104, 105]);
    // Unmapped ids survive unchanged rather than dropping.
    const untouched = remapDefinition(
      definition,
      () => undefined,
      () => undefined,
    );
    expect(collectFilterIds(untouched).sort()).toEqual([12, 14, 15, 16, 18]);
  });
});
