import { Position } from './position.model';

export interface Square {
    position: Position;
    tile: string;
    isAnchor: boolean;
    isBlank: boolean;
    isPlayed: boolean;
    color?: string;
    value: number;
}