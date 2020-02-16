import { Position } from './position.model';

export interface Bonus {
    position: Position;
    value: number;
    type: number;
}
export interface Rules {
    letters: Record<string, number>;
    bonuses: Bonus[]
}