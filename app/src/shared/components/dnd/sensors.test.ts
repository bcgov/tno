import { PointerActivationConstraints, PointerSensor, type Draggable } from '@dnd-kit/dom';
import { describe, expect, it } from 'vitest';

import { activationConstraints, DRAG_DISTANCE, preventActivation, sensors } from './sensors';

const press = (pointerType: string, target: EventTarget | null = null) =>
  ({ pointerType, target }) as unknown as PointerEvent;

/** A draggable whose whole item is the drag handle, as most lists in the apps use. */
const item = (html: string) => {
  const element = document.createElement('div');
  element.innerHTML = html;
  return { element, source: { element, handle: element } as unknown as Draggable };
};

describe('drag sensors', () => {
  it('starts a mouse or pen drag only after the pointer moves, so a click stays a click', () => {
    for (const type of ['mouse', 'pen']) {
      const constraints = activationConstraints(press(type));
      expect(constraints).toHaveLength(1);
      expect(constraints[0]).toBeInstanceOf(PointerActivationConstraints.Distance);
      expect((constraints[0] as unknown as { options: { value: number } }).options.value).toBe(
        DRAG_DISTANCE,
      );
    }
  });

  it('keeps press-and-hold for touch so lists can still scroll', () => {
    const [constraint] = activationConstraints(press('touch'));
    expect(constraint).toBeInstanceOf(PointerActivationConstraints.Delay);
  });

  it('never starts a drag from a form control inside the item', () => {
    const { element, source } = item(
      '<input /><textarea></textarea><select><option>a</option></select><button><svg></svg></button>',
    );
    for (const target of [
      element.querySelector('input'),
      element.querySelector('textarea'),
      element.querySelector('select'),
      element.querySelector('option'),
      element.querySelector('svg'),
    ]) {
      expect(preventActivation(press('mouse', target), source)).toBe(true);
    }
  });

  it('starts drags from the rest of the item, including links and disabled controls', () => {
    const { element, source } = item(
      '<span class="headline">Story</span><a href="/view/1">Story</a><button disabled>x</button>',
    );
    expect(preventActivation(press('mouse', element), source)).toBe(false);
    expect(preventActivation(press('mouse', element.querySelector('.headline')), source)).toBe(
      false,
    );
    expect(preventActivation(press('mouse', element.querySelector('a')), source)).toBe(false);
    expect(preventActivation(press('mouse', element.querySelector('button')), source)).toBe(false);
  });

  it('starts drags from a handle that is itself a control', () => {
    const element = document.createElement('div');
    element.innerHTML = '<button class="grip"></button>';
    const handle = element.querySelector('button')!;
    const source = { element, handle } as unknown as Draggable;
    expect(preventActivation(press('mouse', handle), source)).toBe(false);
  });

  it('is what every DragDropContext uses', () => {
    const [pointer] = sensors as unknown as { plugin: unknown; options: unknown }[];
    expect(pointer.plugin).toBe(PointerSensor);
    expect(pointer.options).toEqual({ activationConstraints, preventActivation });
  });
});
