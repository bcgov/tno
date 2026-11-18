import {
  KeyboardSensor,
  PointerActivationConstraints,
  PointerSensor,
  type Draggable,
} from '@dnd-kit/dom';

/** How far (px) a mouse or pen must move before a drag starts; react-beautiful-dnd used 5. */
export const DRAG_DISTANCE = 5;

/**
 * Elements that keep their own pointer behaviour inside a drag handle, as react-beautiful-dnd
 * did. Links are left out because a click on one never moves far enough to start a drag.
 */
const INTERACTIVE = [
  'input:not([disabled])',
  'select:not([disabled])',
  'textarea:not([disabled])',
  'button:not([disabled])',
  'option',
  'optgroup',
  'video',
  'audio',
  '[contenteditable]:not([contenteditable="false"])',
].join(',');

/**
 * When a pointer press becomes a drag. dnd-kit starts a mouse drag the instant a handle is
 * pressed, which turns every click inside a whole-item handle (a section's chevron, a story's
 * headline) into a drag and swallows it. Here a mouse or pen drag starts only after moving
 * DRAG_DISTANCE, so a click stays a click. Touch keeps dnd-kit's press-and-hold, so a list
 * can still be scrolled with a finger.
 */
export const activationConstraints = (event: PointerEvent) =>
  event.pointerType === 'touch'
    ? [new PointerActivationConstraints.Delay({ value: 250, tolerance: 5 })]
    : [new PointerActivationConstraints.Distance({ value: DRAG_DISTANCE })];

/**
 * True when the press starts on a form control or media inside the draggable, so typing in,
 * selecting text in, or using it never drags the item. dnd-kit's own rule allows a drag from any
 * control inside the handle, and the handle is often the whole item.
 */
export const preventActivation = (event: PointerEvent, source: Draggable) => {
  const target = event.target;
  if (!(target instanceof Element)) return false;
  const control = target.closest(INTERACTIVE);
  if (!control || control === source.handle || control === source.element) return false;
  return !!source.element?.contains(control);
};

/** The sensors every DragDropContext uses. */
export const sensors = [
  PointerSensor.configure({ activationConstraints, preventActivation }),
  KeyboardSensor,
];
